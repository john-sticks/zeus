$(document).ready(function () {
    buscar();
});
let coberturaData = [];
function buscar() {

    $.get(`/api/cobertura`, function (data) {

        coberturaData = data;

        render(data);
    });
}
function verDetalle(partidoId, partidoNombre) {

    const item = coberturaData.find(x => x.partidoId === partidoId);
    if (!item) return;

    $("#tituloDetalle").text("Detalle - " + partidoNombre);

    $.get(`/api/cobertura/calendario?partidoId=${partidoId}`, function (calendario) {

        const htmlCalendario = renderCalendarioReal(calendario);

        let totalHechos = 0;
        let totalImputados = 0;

        calendario.forEach(x => {
            totalHechos += x.totalHechos || 0;
            totalImputados += x.totalImputados || 0;
        });

        $("#contenidoDetalle").html(`
    <div class="row">

        <div class="col-md-6">
            <div class="small-box bg-success">
                <div class="inner">
                    <h3>${totalHechos}</h3>
                    <p>Hechos</p>
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <div class="small-box bg-primary">
                <div class="inner">
                    <h3>${totalImputados}</h3>
                    <p>Involucrados</p>
                </div>
            </div>
        </div>

    </div>

    <hr/>

    <div><b>Calendario:</b><br/>${htmlCalendario}</div>
`);

        $("#modalDetalle").modal("show");
    });
}
function obtenerNombrePartido(props) {

    let raw =
        props.nam ||
        props.fna ||
        props.nombre ||
        props.NOMBRE ||
        props.name ||
        "";

    if (!raw) return "";

    raw = normalizar(raw);

    raw = raw
        .replace(/^PARTIDO DE /, "")
        .replace(/^MUNICIPIO DE /, "")
        .replace(/^DE /, "")
        .replace(/^DEL /, "")
        .replace(/^LA /, "")
        .replace(/^EL /, "")
        .trim();

    return raw;
}
function formatFecha(fecha) {
    if (!fecha) return "";

    const d = new Date(fecha);
    return `${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
}
function render(rows) {

    const tbody = $("#tabla tbody");
    tbody.empty();

    rows.forEach(x => {

        tbody.append(`
            <tr>
                <td>${x.partido}</td>

                <td class="bg-success text-white">
                    ${formatFecha(x.hechosDesde)}
                </td>

                <td class="bg-success text-white">
                    ${formatFecha(x.hechosHasta)}
                </td>

                <td class="bg-primary text-white">
                    ${formatFecha(x.imputadosDesde)}
                </td>

                <td class="bg-primary text-white">
                    ${formatFecha(x.imputadosHasta)}
                </td>

                <td>
                    <button class="btn btn-info btn-sm"
                        onclick="verDetalle(${x.partidoId}, '${x.partido}')">
                        Ver
                    </button>
                </td>
            </tr>
        `);
    });
} function calcularResumen(rows) {

        let totalPartidos = rows.length;
        let totalHechos = 0;
        let totalImputados = 0;

        rows.forEach(x => {
            totalHechos += x.totalHechos;
            totalImputados += x.totalImputados;
        });

        $("#totalPartidos").text(totalPartidos);
        $("#totalHechos").text(totalHechos);
        $("#totalImputados").text(totalImputados);

        $("#resumen").show();
}
function renderCalendarioReal(data) {

    let html = "";
    const grouped = {};

    data.forEach(x => {
        if (!grouped[x.anio]) grouped[x.anio] = [];
        grouped[x.anio].push(x);
    });

    Object.keys(grouped).sort().forEach(anio => {

        html += `<div style="margin-bottom:15px;">`;
        html += `<h4>${anio}</h4>`;

        for (let mes = 1; mes <= 12; mes++) {

            const item = grouped[anio].find(x => x.mes === mes);

            let clase = "cal-missing";
            let tooltip = `Mes ${mes}\nSin datos`;

            if (item) {

                if (item.totalHechos > 0 && item.totalImputados > 0)
                    clase = "cal-ambos";
                else if (item.totalHechos > 0)
                    clase = "cal-hecho";
                else if (item.totalImputados > 0)
                    clase = "cal-imputado";
                else
                    clase = "cal-empty";

                tooltip = `Mes ${mes}
Hechos: ${item.hechosDesde ? "SI" : "NO"}
Involucrados: ${item.imputadosDesde ? "SI" : "NO"}`;
            }

            html += `<div class="cal-box ${clase}" title="${tooltip}">${mes}</div>`;
        }

        html += `</div><hr/>`;
    });

    return html;
}
function cargarGeoJSON() {

    fetch('/partidos.geojson') // ajustá ruta
        .then(r => r.json())
        .then(geo => {

            L.geoJSON(geo, {
                style: feature => estiloFeature(feature),
                onEachFeature: (feature, layer) => onEachFeature(feature, layer)
            }).addTo(map);

        });
}
function initMapa() {

    if (map) {
        map.remove();
    }

    map = L.map('mapCobertura').setView([-37.5, -59], 6);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap'
    }).addTo(map);

    cargarGeoJSON();
}

let map = null;

function abrirMapa() {

    $("#modalMapa").modal("show");

    setTimeout(() => {
        initMapa();
    }, 300);
}
function normalizar(txt) {
    if (!txt) return "";
    return txt
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .toUpperCase()
        .trim();
}
function estiloFeature(feature) {

    const nombre = obtenerNombrePartido(feature.properties);

    const item = coberturaData.find(x =>
        matchPartido(nombre, normalizar(x.partido))
    );

    let color = "#e0e0e0";

    if (item) {
        if (item.hechosDesde && item.imputadosDesde)
            color = "#9C27B0";
        else if (item.hechosDesde)
            color = "#4CAF50";
        else if (item.imputadosDesde)
            color = "#2196F3";
    }

    return {
        color: "#333",
        weight: 1,
        fillOpacity: 0.7,
        fillColor: color
    };
} function onEachFeature(feature, layer) {

    const nombre = obtenerNombrePartido(feature.properties);

    const item = coberturaData.find(x =>
        matchPartido(nombre, normalizar(x.partido))
    );

    if (!item) return;

    const html = `
    <b>${item.partido}</b><br/>
    Hechos: ${item.totalHechos}<br/>
    Involucrados: ${item.totalImputados}<br/>
`;

    // 🔥 HOVER (mostrar info)
    layer.on("mouseover", function (e) {
        layer.bindPopup(html).openPopup();
    });

    layer.on("mouseout", function () {
        layer.closePopup();
    });

    // 🔥 CLICK (abrir modal)
    layer.on("click", function () {
        console.log("CLICK:", item); // debug
        verDetalle(item.partidoId || item.idPartido, item.partido);
    });
}
function matchPartido(nombreMapa, nombreData) {

    if (!nombreMapa || !nombreData) return false;

    return nombreMapa.includes(nombreData) || nombreData.includes(nombreMapa);
}