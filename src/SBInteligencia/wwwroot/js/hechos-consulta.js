// ===============================
// ESTADO GEO GLOBAL
// ===============================
let geoState = {
    lat: null,
    lng: null,
    radio: null
};

$(document).ready(function () {
    autoToggleCheckbox("#calificaciones", "#enableCalificaciones");
    autoToggleCheckbox("#ipp", "#enableIPP");
    autoToggleCheckbox("#rangoFechas", "#enableFechas");
    autoToggleCheckbox("#partido", "#enablePartido");
    autoToggleCheckbox("#relato", "#enableRelato");
    initCalificacionesTomSelect();
    initPartidosTomSelect(); // 🔥 AGREGAR
    
    autoToggleCheckbox("#radio", "#enableGeo");
    // daterangepicker
    $('#rangoFechas').daterangepicker({
        autoUpdateInput: false,
        locale: { format: 'DD/MM/YYYY' }
    });

    $('#rangoFechas').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(
            picker.startDate.format('DD/MM/YYYY') + ' - ' +
            picker.endDate.format('DD/MM/YYYY')
        );
        $("#enableFechas").prop("checked", true);
    });

    //initMapaFiltro();  // 🔵 mapa filtro
    initMapa();        // 🟢 mapa resultados
    map.setMaxBounds([
        [-40.5, -63.5],
        [-33.5, -55.0]
    ]);
    $("#radio").on("input", function () {
        geoState.radio = parseFloat($(this).val());
        drawCircle();
    });

    $("#latitud, #longitud").on("change", function () {

        const lat = parseFloat($("#latitud").val());
        const lng = parseFloat($("#longitud").val());

        if (!lat || !lng) return;

        drawMarker(lat, lng);
        drawCircle();
    });
    $("#btnBuscar").on("click", function () {
        buscar();
    });
    $("#filtroPartido").on("change", function () {
        aplicarFiltrosGrilla();
        actualizarCombos();
    });

    $("#filtroLocalidad").on("change", function () {
        aplicarFiltrosGrilla();
        actualizarCombos();
    });

    $("#filtroDependencia").on("change", function () {
        aplicarFiltrosGrilla();
        actualizarCombos();
    });

    $("#filtroCalificacion").on("change", function () {
        aplicarFiltrosGrilla();
        actualizarCombos();
    });
});

let mapaInicializado = false;

$('#boxGeo').on('expanded.boxwidget', function () {

    if (!mapaInicializado) {
        initMapaFiltro();
        mapaInicializado = true;
    }

    setTimeout(function () {
        if (mapFiltro) {
            mapFiltro.invalidateSize();
        }
    }, 300);
});
function getColorByCalificacion(calif) {

    if (!calif) return "gray";

    calif = calif.toLowerCase();

    if (calif.includes("robo")) return "red";
    if (calif.includes("hurto")) return "orange";
    if (calif.includes("homicidio")) return "black";
    if (calif.includes("lesiones")) return "purple";
    if (calif.includes("amenazas")) return "yellow";

    return "blue";
}
function createCustomIcon(color) {

    return L.divIcon({
        className: '',
        html: `
            <div style="
                background:${color};
                width:16px;
                height:16px;
                border-radius:50%;
                border:2px solid white;
                box-shadow:0 0 5px rgba(0,0,0,0.5);
            "></div>
        `,
        iconSize: [20, 20],
        iconAnchor: [10, 10]
    });
}
function isInBuenosAires(lat, lng) {
    return lat >= -40.5 && lat <= -33.5 &&
        lng >= -63.5 && lng <= -55.0;
}
function actualizarMapa(rows) {

    if (!map) return;

   

    // limpiar todo
    if (clusterGroup) map.removeLayer(clusterGroup);
    if (heatLayer) map.removeLayer(heatLayer);
    markers.forEach(m => map.removeLayer(m));
    markers = [];

    const bounds = [];

    // =========================
    // MODO CLUSTER
    // =========================
    if (modoMapa === "cluster") {

        clusterGroup = L.markerClusterGroup();

        rows.forEach(x => {

            const lat = parseFloat(x.latitud);
            const lng = parseFloat(x.longitud);

            if (isNaN(lat) || isNaN(lng)) return;
            if (!isInBuenosAires(lat, lng)) return;
            const color = getColorByCalificacion(x.calificaciones);

            const marker = L.marker(
                [lat, lng],
                { icon: createCustomIcon(color) }
            ).bindPopup(`
                <b>${x.calificaciones}</b><br>
                ${x.partido} - ${x.localidad}
            `);

            clusterGroup.addLayer(marker);
            bounds.push([lat, lng]);
        });

        map.addLayer(clusterGroup);
    }

    // =========================
    // MODO PUNTOS
    // =========================
    if (modoMapa === "puntos") {

        rows.forEach(x => {

            const lat = parseFloat(x.latitud);
            const lng = parseFloat(x.longitud);

            if (isNaN(lat) || isNaN(lng)) return;
            if (!isInBuenosAires(lat, lng)) return;
            const color = getColorByCalificacion(x.calificaciones);

            const marker = L.marker(
                [lat, lng],
                { icon: createCustomIcon(color) }
            ).addTo(map);

            markers.push(marker);
            bounds.push([lat, lng]);
        });
    }

    // =========================
    // MODO HEATMAP (PAGINADO)
    // =========================
    if (modoMapa === "calor") {
        const heatData = [];

        rows.forEach(x => {
            const lat = parseFloat(x.latitud);
            const lng = parseFloat(x.longitud);

            if (isNaN(lat) || isNaN(lng)) return;
            if (!isInBuenosAires(lat, lng)) return;
            // 1. Usa intensidad 1. El "calor" real vendrá de la acumulación de puntos.
            heatData.push([lat, lng, 1]);
            bounds.push([lat, lng]);
        });

        heatLayer = L.heatLayer(heatData, {
            radius: 18,          // Reducimos el radio para que no tape ciudades enteras
            blur: 25,            // Aumentamos el desenfoque para que los bordes sean suaves
            maxZoom: 14,
            max: 1.0,
            minOpacity: 0.3,     // 👈 CLAVE: Hace que las zonas más frías sean casi transparentes
            gradient: {
                0.2: "blue",   // Empezar con azul/verde para zonas frías
                0.4: "red",
                0.6: "yellow",
                0.8: "orange",
                1.0: "red"
            }
        });

        heatLayer.addTo(map);
    }

    // =========================
    // AJUSTAR VISTA
    // =========================
    if (bounds.length > 0) {
        map.fitBounds(bounds, { padding: [30, 30] });
    } else {
        map.setView([-38, -59], 6); // fallback
    }
}
function setYear(year) {

    let desde, hasta;
    const hoy = moment();

    if (year === 2026) {
        desde = moment("01-01-2026", "DD-MM-YYYY");
        hasta = hoy;
    } else {
        desde = moment(`01-01-${year}`, "DD-MM-YYYY");
        hasta = moment(`31-12-${year}`, "DD-MM-YYYY");
    }

    $("#rangoFechas").val(
        desde.format("DD/MM/YYYY") + " - " +
        hasta.format("DD/MM/YYYY")
    );

    $("#enableFechas").prop("checked", true);
}

function buildFiltro() {

    const filtro = {
        pagina: 1,
        tamaño: 50
    };

    if ($("#enableIPP").is(":checked")) {
        filtro.ipp = $("#ipp").val();
        filtro.enableIPP = true;
    }

    if ($("#enablePartido").is(":checked")) {
        filtro.partido = $("#partido").val();
        filtro.enablePartido = true;
    }

    if ($("#enableRelato").is(":checked")) {
        filtro.relato = $("#relato").val();
        filtro.enableRelato = true;
    }

    if ($("#enableGeo").is(":checked")) {
        filtro.latitud = $("#latitud").val();
        filtro.longitud = $("#longitud").val();
        filtro.radioMetros = $("#radio").val();
        filtro.usarGeo = true;
    }

    if ($("#enableFechas").is(":checked")) {

        const rango = $("#rangoFechas").val();

        if (rango) {
            const partes = rango.split(" - ");

            filtro.fechaDesde = moment(partes[0], "DD/MM/YYYY").format("YYYY-MM-DD");
            filtro.fechaHasta = moment(partes[1], "DD/MM/YYYY").format("YYYY-MM-DD");

            filtro.enableFechas = true;
        }
    }

    return filtro;
}

function validar(filtro) {

    if (!filtro.enableIPP &&
        !filtro.enablePartido &&
        !filtro.enableRelato &&
        !filtro.enableGeo &&
        !filtro.enableFechas) {

        alert("Debe activar al menos un filtro");
        return false;
    }

    return true;
}


function renderTabla(rows) {

    const tbody = document.querySelector("#tabla tbody");
    tbody.innerHTML = "";

    if (!rows || rows.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8">Sin resultados</td></tr>`;
        return;
    }

    rows.forEach(x => {

        const tr = document.createElement("tr");

        const anio = new Date(x.fechaCarga).getFullYear();

        tr.innerHTML = `
            

            <td><input type="checkbox"
       class="chk-hecho"
       data-id="${x.idHecho}" 
       data-anio="${anio}"></td>
            <td>${x.idHecho}</td>
            <td>${formatearFecha(x.fechaCarga)}</td>
            <td>${x.partido || ""}</td>
            <td>${x.localidad || ""}</td>
            <td>${x.calificaciones || ""}</td>
            <td>${x.dependencia || ""}</td>
            <!-- 🔹 BOTON DETALLE -->
            <td>
                <a href="#" onclick="verDetalle(${x.idHecho}, ${anio}); return false;" title="Ver Info">
                    <img src="../img/grid_info.png" style="cursor:pointer;" />
                </a>
            </td>
        `;

        tbody.appendChild(tr);
    });

    bindSeleccion();
}
function verDetalle(id, anio) {
    const url = `/Hechos/Detalle?id=${id}&anio=${anio}`;
    window.open(url, "_blank");
}
function formatearFecha(fecha) {

    if (!fecha) return "";

    return moment(fecha).format("DD/MM/YYYY");
}
let hechosSeleccionados = [];

function bindSeleccion() {

    document.querySelectorAll(".chk-hecho").forEach(chk => {

        chk.addEventListener("change", function () {

            const id = parseInt(this.dataset.id);
            const anio = parseInt(this.dataset.anio);

            if (this.checked) {

                if (!hechosSeleccionados.some(x => x.id === id)) {
                    hechosSeleccionados.push({
                        id: id,
                        anio: anio
                    });
                }

            } else {

                hechosSeleccionados = hechosSeleccionados.filter(x => x.id !== id);
            }

            console.log("Seleccionados:", hechosSeleccionados);
        });

    });
}
document.addEventListener("change", function (e) {

    if (e.target.id === "chkAll") {

        const checked = e.target.checked;

        document.querySelectorAll(".chk-hecho").forEach(chk => {
            chk.checked = checked;
            chk.dispatchEvent(new Event("change"));
        });
    }

});

let map;
let markers = [];

function initMapa() {

    if (typeof L === "undefined") {
        console.error("Leaflet no cargado");
        return;
    }

    map = L.map('mapa').setView([-38, -59], 6);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);
}

function setGeoInputs(lat, lng) {

    $("#latitud").val(lat.toFixed(6));
    $("#longitud").val(lng.toFixed(6));
    $("#enableGeo").prop("checked", true);
}

function drawMarker(lat, lng) {

    if (markerFiltro) {
        mapFiltro.removeLayer(markerFiltro);
    }

    markerFiltro = L.marker([lat, lng]).addTo(mapFiltro);

    mapFiltro.setView([lat, lng], 14);
}
function drawCircle() {

    if (!mapFiltro) return;

    const lat = geoState.lat;
    const lng = geoState.lng;
    const radio = geoState.radio;

    if (circleFiltro) {
        mapFiltro.removeLayer(circleFiltro);
    }

    if (lat == null || lng == null || radio == null) return;

    circleFiltro = L.circle([lat, lng], {
        radius: radio,
        color: 'blue',
        fillColor: '#3c8dbc',
        fillOpacity: 0.2
    }).addTo(mapFiltro);
}
// ===============================
// MAPA FILTRO (mapa superior)
// ===============================

let mapFiltro;
let markerFiltro;
let circleFiltro;

// INIT
function initMapaFiltro() {

    if (typeof L === "undefined") {
        console.error("Leaflet no cargado");
        return;
    }

    mapFiltro = L.map('mapaFiltro').setView([-38, -59], 6);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(mapFiltro);

    // CLICK EN MAPA
    mapFiltro.on('click', function (e) {

        const lat = e.latlng.lat;
        const lng = e.latlng.lng;

        geoState.lat = lat;
        geoState.lng = lng;

        // 🔹 DEFAULT RADIO si no hay
        if (!geoState.radio) {
            geoState.radio = 500;
            $("#radio").val(500);
        }

        setGeoInputs(lat, lng);
        drawMarker(lat, lng);
        drawCircle();
    });
}
function usarUbicacionActual() {

    if (!navigator.geolocation) {
        alert("Geolocalización no soportada");
        return;
    }

    navigator.geolocation.getCurrentPosition(function (pos) {

        const lat = pos.coords.latitude;
        const lng = pos.coords.longitude;

        // 🔹 SETEAR ESTADO
        geoState.lat = lat;
        geoState.lng = lng;

        // 🔹 UI
        setGeoInputs(lat, lng);
        drawMarker(lat, lng);
        drawCircle();

    }, function () {
        alert("No se pudo obtener la ubicación");
    });
}

let paginaActual = 1;

function buscar(pagina = 1) {

    console.log("CLICK BUSCAR");

    paginaActual = pagina;

    let rango = $("#rangoFechas").val();

    if (!rango) {
        alert("Debe seleccionar un rango de fechas");
        return;
    }
    if ($("#enableGeo").is(":checked")) {

        if (!geoState.lat || !geoState.lng) {
            alert("Debe seleccionar un punto en el mapa");
            return;
        }
    }
    let fechas = rango.split(" - ");

    const filtros = {
        anio: moment(fechas[0], "DD/MM/YYYY").year(),

        fechaDesde: moment(fechas[0], "DD/MM/YYYY").format("YYYY-MM-DD"),
        fechaHasta: moment(fechas[1], "DD/MM/YYYY").format("YYYY-MM-DD"),

        usarIPP: $("#enableIPP").is(":checked"),
        ipp: $("#ipp").val(),

        // 🔥 FIX REAL
        partido: $("#partido")[0].tomselect.getValue(),

        usarRelato: $("#enableRelato").is(":checked"),
        relato: $("#relato").val(),

        usarDomicilio: $("#enableDomicilio").is(":checked"),
        calle: $("#calle").val(),

        usarGeo: $("#enableGeo").is(":checked"),
        latitud: geoState.lat,
        longitud: geoState.lng,
        radioMetros: parseInt($("#radio").val()) || 1000,

        pagina: paginaActual,
        tamañoPagina: parseInt($("#cantidad").val()),

        // 🔥 FIX TAMBIÉN
        calificaciones: $("#calificaciones")[0].tomselect.getValue(),
    };

    console.log("FILTROS:", filtros);
    expandirGeoSiActivo();
    console.log({
        usarGeo: $("#enableGeo").is(":checked"),
        lat: geoState.lat,
        lng: geoState.lng,
        radio: $("#radio").val()
    });
    $.ajax({
        url: "/api/hechos/buscar",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(filtros),
        success: function (res) {

            window.lastRows = res.data;

            renderPaginacion(res.total, res.pagina, res.tamañoPagina);

            window.lastRows = res.data;      // 🔥 ORIGINAL
            window.filteredRows = res.data;  // 🔥 estado actual

            renderTabla(res.data);
            actualizarMapa(res.data);

            resetFiltrosGrilla();          // 🔥 limpia selects
            actualizarCombos();            // 🔥 reconstruye combos
            renderLeyenda(window.lastRows); // 🔥 sincroniza leyenda

            $("#boxResultados").show();
            $("#boxMapaResultados").show();

            // 🔥 IMPORTANTE: esperar a que el DOM se pinte
            setTimeout(function () {

                if (!map) {
                    initMapa(); // por si no estaba
                }

                map.invalidateSize(); // 🔥 CLAVE

                actualizarMapa(res.data);

            }, 200);
        },
        error: function (err) {

            console.error("ERROR AJAX:", err);

            let mensaje = "Error inesperado";

            try {
                const res = JSON.parse(err.responseText);

                mensaje = res.detail || res.message || mensaje;

            } catch (e) {
                mensaje = err.statusText || mensaje;
            }

            toastr.error(mensaje, "Error");
        }
    });
}

function renderLeyenda(rows) {

    const contenedor = $("#leyendaMapa");
    contenedor.empty();

    const agrupados = {};

    rows.forEach(x => {

        const key = x.calificaciones || "Sin dato";

        if (!agrupados[key]) {
            agrupados[key] = 0;
        }

        agrupados[key]++;
    });

    Object.keys(agrupados).forEach(k => {

        const color = getColorByCalificacion(k);

        contenedor.append(`
            <span style="margin-right:10px;">
                <span style="
                    display:inline-block;
                    width:10px;
                    height:10px;
                    background:${color};
                    border-radius:50%;
                    margin-right:5px;">
                </span>
                ${k} (${agrupados[k]})
            </span>
        `);
    });
}

let clusterGroup;
let heatLayer;

let modoMapa = "cluster"; // cluster | puntos | calor

function autoToggleCheckbox(inputSelector, checkboxSelector) {

    const $input = $(inputSelector);
    const $chk = $(checkboxSelector);

    if ($input.length === 0 || $chk.length === 0) return;

    function evaluar() {

        let tieneValor = false;

        if ($input.is("select")) {
            const val = $input.val();
            tieneValor = val && val.length > 0;
        } else {
            tieneValor = $input.val() && $input.val().toString().trim() !== "";
        }

        $chk.prop("checked", tieneValor);
    }

    // input change
    $input.on("change keyup", evaluar);

    // checkbox OFF → limpiar
    $chk.on("change", function () {
        if (!this.checked) {
            if ($input.is("select")) {
                if ($input[0].tomselect) {
                    $input[0].tomselect.clear();
                } else {
                    $input.val(null).trigger("change");
                }
            } else {
                $input.val("");
            }
        }
    });

    // inicial
    evaluar();
}
function expandirGeoSiActivo() {

    const chk = $("#enableGeo");

    if (chk.is(":checked")) {

        const box = $("#boxGeo");

        if (box.hasClass("collapsed-box")) {

            box.removeClass("collapsed-box");
            box.find(".fa-plus")
                .removeClass("fa-plus")
                .addClass("fa-minus");

            setTimeout(() => {
                if (mapFiltro) mapFiltro.invalidateSize();
            }, 300);
        }
    }
}
function setModoMapa(modo) {

    modoMapa = modo;

    // UI
    $(".btn-modo-mapa").removeClass("btn-primary").addClass("btn-default");

    if (modo === "cluster") $("#btnCluster").addClass("btn-primary");
    if (modo === "puntos") $("#btnPuntos").addClass("btn-primary");
    if (modo === "calor") $("#btnCalor").addClass("btn-primary");

    // refresh
    if (window.filteredRows) {
        actualizarMapa(window.filteredRows);
        renderLeyenda(window.filteredRows);
    } else {
        actualizarMapa(window.lastRows);
        renderLeyenda(window.lastRows);
    }
}
function limpiar() {

    if (!confirm("¿Desea limpiar los filtros?")) return;

    location.reload();
}
function renderPaginacion(total, pagina, tamaño) {

    const totalPaginas = Math.ceil(total / tamaño);
    const cont = $("#paginador");

    cont.empty();

    if (totalPaginas <= 1) return;

    cont.append(`<button onclick="buscar(${pagina - 1})" ${pagina === 1 ? "disabled" : ""}>Anterior</button>`);

    for (let i = Math.max(1, pagina - 2); i <= Math.min(totalPaginas, pagina + 2); i++) {

        if (i === pagina)
            cont.append(`<span style="margin:5px;font-weight:bold;">${i}</span>`);
        else
            cont.append(`<button onclick="buscar(${i})">${i}</button>`);
    }

    cont.append(`<button onclick="buscar(${pagina + 1})" ${pagina === totalPaginas ? "disabled" : ""}>Siguiente</button>`);

    cont.append(`<span style="margin-left:10px;">Total: ${total}</span>`);
}

function llenarSelect(id, valores) {
    const select = $(id);
    select.empty();
    select.append(`<option value="">Todos</option>`);

    [...new Set(valores.filter(x => x))]
        .sort()
        .forEach(v => {
            select.append(`<option value="${v}">${v}</option>`);
        });
}
function aplicarFiltrosGrilla() {

    const partido = $("#filtroPartido").val();
    const localidad = $("#filtroLocalidad").val();
    const dependencia = $("#filtroDependencia").val();
    const calificacion = $("#filtroCalificacion").val()?.trim().toLowerCase();

    // RESET automático
    if (!partido && !localidad && !dependencia && !calificacion) {
        renderTabla(window.lastRows);
        actualizarMapa(window.lastRows);
        renderLeyenda(window.lastRows); // 🔥 AGREGAR
        return;
    }

    const filtrados = window.lastRows.filter(x => {

        if (partido && x.partido !== partido) return false;
        if (localidad && x.localidad !== localidad) return false;
        if (dependencia && x.dependencia !== dependencia) return false;

        if (calificacion && !(x.calificaciones || "").toLowerCase().includes(calificacion))
            return false;

        return true;
    });
    window.filteredRows = filtrados;
    renderTabla(filtrados);
    actualizarMapa(filtrados);
    renderLeyenda(filtrados);

}
function actualizarCombos() {

    const partidoSel = $("#filtroPartido").val();
    const localidadSel = $("#filtroLocalidad").val();
    const dependenciaSel = $("#filtroDependencia").val();
    const calificacionSel = $("#filtroCalificacion").val();

    let base = window.lastRows;

    // PARTIDO
    llenarSelect("#filtroPartido", base.map(x => x.partido));
    $("#filtroPartido").val(partidoSel);

    // LOCALIDAD
    let localidades = base;

    if (partidoSel) {
        localidades = base.filter(x => x.partido === partidoSel);
    }

    llenarSelect("#filtroLocalidad", localidades.map(x => x.localidad));
    $("#filtroLocalidad").val(localidadSel);

    // DEPENDENCIA
    let dependencias = base;

    if (partidoSel) {
        dependencias = dependencias.filter(x => x.partido === partidoSel);
    }

    if (localidadSel) {
        dependencias = dependencias.filter(x => x.localidad === localidadSel);
    }

    llenarSelect("#filtroDependencia", dependencias.map(x => x.dependencia));
    $("#filtroDependencia").val(dependenciaSel);

    // 🔥 CALIFICACION (AHORA CASCADA)
    let calificaciones = base;

    if (partidoSel) {
        calificaciones = calificaciones.filter(x => x.partido === partidoSel);
    }

    if (localidadSel) {
        calificaciones = calificaciones.filter(x => x.localidad === localidadSel);
    }

    if (dependenciaSel) {
        calificaciones = calificaciones.filter(x => x.dependencia === dependenciaSel);
    }

    llenarSelect("#filtroCalificacion", calificaciones.map(x => x.calificaciones));
    $("#filtroCalificacion").val(calificacionSel);
} function limpiarFiltrosGrilla() {

    $("#filtroPartido").val("");
    $("#filtroLocalidad").val("");
    $("#filtroDependencia").val("");
    $("#filtroCalificacion").val("");

    renderTabla(window.lastRows);
    actualizarMapa(window.lastRows);

    actualizarCombos(); // 🔥 sin parámetro
}
function resetFiltrosGrilla() {

    $("#filtroPartido").val("");
    $("#filtroLocalidad").val("");
    $("#filtroDependencia").val("");
    $("#filtroCalificacion").val("");

    window.filteredRows = window.lastRows;
}
function initCalificacionesTomSelect() {

    const data = [
        { value: 'abigeato', text: 'Abigeato' },
        { value: 'abuso de arma', text: 'Abuso de Arma' },
        { value: 'abuso sexual', text: 'Abuso sexual' },
        { value: 'amenazas', text: 'Amenazas' },
        { value: 'causales de muerte', text: 'Averiguación Causales de Muerte' },
        { value: 'paradero', text: 'Averiguación de paradero' },
        { value: 'ilícito', text: 'Averiguación de Ilícito' },
        { value: 'daño', text: 'Daño' },
        { value: 'encubrimiento', text: 'Encubrimiento' },
        { value: 'estafa', text: 'Estafa' },
        { value: 'estupefacientes', text: 'Estupefacientes' },
        { value: 'hallazgo', text: 'Hallazgo' },
        { value: 'homicidio', text: 'Homicidio' },
        { value: 'hurto', text: 'Hurto' },
        { value: 'lesiones', text: 'Lesiones' },
        { value: 'resistencia', text: 'Resistencia a la autoridad' },
        { value: 'robo', text: 'Robo' },
        { value: 'usurpación', text: 'Usurpación' }
    ];

    new TomSelect("#calificaciones", {
        options: data,
        valueField: "value",
        labelField: "text",
        searchField: "text",
        plugins: ['remove_button'],
        maxItems: null,
        create: false
    });
}
function initPartidosTomSelect() {

    const data = [
        { value: 'AYACUCHO', text: 'AYACUCHO' },
        { value: 'AZUL', text: 'AZUL' },
        { value: 'BENITO JUAREZ', text: 'BENITO JUAREZ' },
        { value: 'BOLIVAR', text: 'BOLIVAR' },
        { value: 'GENERAL ALVEAR', text: 'GENERAL ALVEAR' },
        { value: 'GENERAL LA MADRID', text: 'GENERAL LA MADRID' },
        { value: 'LAPRIDA', text: 'LAPRIDA' },
        { value: 'LAS FLORES', text: 'LAS FLORES' },
        { value: 'OLAVARRIA', text: 'OLAVARRIA' },
        { value: 'RAUCH', text: 'RAUCH' },
        { value: 'TANDIL', text: 'TANDIL' },
        { value: 'TAPALQUE', text: 'TAPALQUE' },
        { value: 'GONZALES CHAVES', text: 'GONZALES CHAVES' },
        { value: 'BAHIA BLANCA', text: 'BAHIA BLANCA' },
        { value: 'CORONEL DORREGO', text: 'CORONEL DORREGO' },
        { value: 'CORONEL PRINGLES', text: 'CORONEL PRINGLES' },
        { value: 'CORONEL ROSALES', text: 'CORONEL ROSALES' },
        { value: 'CORONEL SUAREZ', text: 'CORONEL SUAREZ' },
        { value: 'MONTE HERMOSO', text: 'MONTE HERMOSO' },
        { value: 'PATAGONES', text: 'PATAGONES' },
        { value: 'PUAN', text: 'PUAN' },
        { value: 'SAAVEDRA', text: 'SAAVEDRA' },
        { value: 'TORNQUIST', text: 'TORNQUIST' },
        { value: 'TRES ARROYOS', text: 'TRES ARROYOS' },
        { value: 'VILLARINO', text: 'VILLARINO' },
        { value: 'CASTELLI', text: 'CASTELLI' },
        { value: 'CHASCOMUS', text: 'CHASCOMUS' },
        { value: 'DOLORES', text: 'DOLORES' },
        { value: 'GENERAL BELGRANO', text: 'GENERAL BELGRANO' },
        { value: 'GENERAL GUIDO', text: 'GENERAL GUIDO' },
        { value: 'LEZAMA', text: 'LEZAMA' },
        { value: 'MAIPU', text: 'MAIPU' },
        { value: 'PILA', text: 'PILA' },
        { value: 'TORDILLO', text: 'TORDILLO' },
        { value: 'EXALTACION DE LA CRUZ', text: 'EXALTACION DE LA CRUZ' },
        { value: 'GENERAL LAS HERAS', text: 'GENERAL LAS HERAS' },
        { value: 'GENERAL RODRIGUEZ', text: 'GENERAL RODRIGUEZ' },
        { value: 'LUJAN', text: 'LUJAN' },
        { value: 'MARCOS PAZ', text: 'MARCOS PAZ' },
        { value: 'MORENO', text: 'MORENO' },
        { value: 'CHACABUCO', text: 'CHACABUCO' },
        { value: 'FLORENTINO AMEGHINO', text: 'FLORENTINO AMEGHINO' },
        { value: 'GENERAL ARENALES', text: 'GENERAL ARENALES' },
        { value: 'GENERAL VIAMONTE', text: 'GENERAL VIAMONTE' },
        { value: 'JUNIN', text: 'JUNIN' },
        { value: 'LINCOLN', text: 'LINCOLN' },
        { value: 'LA MATANZA', text: 'LA MATANZA' },
        { value: 'LA PLATA', text: 'LA PLATA' },
        { value: 'ALMIRANTE BROWN', text: 'ALMIRANTE BROWN' },
        { value: 'AVELLANEDA', text: 'AVELLANEDA' },
        { value: 'LANUS', text: 'LANUS' },
        { value: 'LOMAS DE ZAMORA', text: 'LOMAS DE ZAMORA' },
        { value: 'GENERAL PUEYRREDON', text: 'GENERAL PUEYRREDON' },
        { value: 'MERLO', text: 'MERLO' },
        { value: 'MORON', text: 'MORON' },
        { value: 'NECOCHEA', text: 'NECOCHEA' },
        { value: 'PERGAMINO', text: 'PERGAMINO' },
        { value: 'PILAR', text: 'PILAR' },
        { value: 'QUILMES', text: 'QUILMES' },
        { value: 'SAN ISIDRO', text: 'SAN ISIDRO' },
        { value: 'TIGRE', text: 'TIGRE' },
        { value: 'SAN MARTIN', text: 'SAN MARTIN' },
        { value: 'ZARATE', text: 'ZARATE' }
    ];

    new TomSelect("#partido", {
        options: data,
        valueField: "value",
        labelField: "text",
        searchField: "text",
        plugins: ['remove_button'],
        maxItems: null,
        create: false
    });
}
async function irAInforme() {

    if (hechosSeleccionados.length === 0) {
        alert("Seleccioná al menos uno");
        return;
    }

    const rango = $("#rangoFechas").val();

    if (!rango) {
        alert("Debe seleccionar un rango de fechas");
        return;
    }

    const fechas = rango.split(" - ");
    const anio = moment(fechas[0], "DD/MM/YYYY").year();

    const hechos = hechosSeleccionados.map(x => ({
        idHecho: x.idHecho || x.id,
        anio: anio
    }));

    const res = await fetch("/api/hechos/guardar-session", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            Hechos: hechos,
            Reemplazar: false // 🔥 SIEMPRE
        })
    });

    const result = await res.json();

    if (result.nuevos > 0) {
        toastr.success(`Se agregaron ${result.nuevos} hechos`);
    } else {
        toastr.info("Los hechos ya estaban en el informe");
    }

    window.open("/Informes/Detalle", "_blank").focus();
}