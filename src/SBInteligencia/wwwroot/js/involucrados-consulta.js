
let hechosSeleccionados = [];
$(document).ready(function () {
    $("#btnBuscar").on("click", function () {
        buscarInvolucrados();
    });
});


function renderTabla(rows) {

    const tbody = document.querySelector("#tabla tbody");
    tbody.innerHTML = "";

    if (!rows || rows.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5">Sin resultados</td></tr>`;
        return;
    }

    rows.forEach(x => {

        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${x.apellido || ""}</td>
            <td>${x.nombre || ""}</td>
            <td>${x.nroDni || ""}</td>
            <td>${x.tipoDni || ""}</td>
            <td>
                <button class="btn btn-xs btn-info"
                    onclick="verDetalleInvolucrado(${x.idHecho}, ${$('#anio').val()})">
                    Ver Info
                </button>
            </td>
            <td>
                <button class="btn btn-xs btn-info"
                onclick="verDetalle(${x.idHecho}, ${$('#anio').val()})">
                    Ver Hecho
                </button>
            </td>
        `;

        tbody.appendChild(tr);
    });
} function verDetalle(id, anio) {
    const url = `/Hechos/Detalle?id=${id}&anio=${anio}`;
    window.open(url, "_blank");
}
function verDetalleInvolucrado(id, anio) {

    $.ajax({
        url: `/api/involucrados/${id}/${anio}`,
        method: "GET",
        success: function (data) {

            mostrarModalDetalle(data);

        },
        error: function () {
            toastr.error("Error al obtener detalle");
        }
    });
}

let paginaActual = 1;

function buscarInvolucrados(pagina = 1) {

    console.log("BUSCAR INVOLUCRADOS");

    paginaActual = pagina;

    const filtros = {
        anio: parseInt($("#anio").val()),
        nombre: $("#nombre").val(),
        apellido: $("#apellido").val(),
        nroDni: $("#dni").val(),
        pagina: paginaActual,
        tamañoPagina: parseInt($("#cantidad").val()) || 100
    };

    if (!filtros.nombre && !filtros.apellido && !filtros.nroDni) {
        toastr.warning("Debe ingresar al menos un filtro", "Validación");
        return;
    }

    showLoader("Buscando involucrados...");

    $.ajax({
        url: "/api/involucrados/buscar",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(filtros),

        success: function (res) {

            window.lastRows = res.data;
            window.filteredRows = res.data;

            renderTabla(res.data);

            renderPaginacionInvolucrados(res.total, res.pagina, res.tamañoPagina);

            $("#boxResultados").show();

            if (res.data && res.data.length > 0) {
                toastr.success("Resultados obtenidos");
            } else {
                toastr.info("Sin resultados");
            }
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
function renderPaginacionInvolucrados(total, pagina, tamaño) {

    const totalPaginas = Math.ceil(total / tamaño);
    const cont = $("#paginador");

    cont.empty();

    if (totalPaginas <= 1) return;

    cont.append(`<button onclick="buscarInvolucrados(${pagina - 1})" ${pagina === 1 ? "disabled" : ""}>Anterior</button>`);

    for (let i = Math.max(1, pagina - 2); i <= Math.min(totalPaginas, pagina + 2); i++) {

        if (i === pagina)
            cont.append(`<span style="margin:5px;font-weight:bold;">${i}</span>`);
        else
            cont.append(`<button onclick="buscarInvolucrados(${i})">${i}</button>`);
    }

    cont.append(`<button onclick="buscarInvolucrados(${pagina + 1})" ${pagina === totalPaginas ? "disabled" : ""}>Siguiente</button>`);

    cont.append(`<span style="margin-left:10px;">Total: ${total}</span>`);
}
function limpiar() {

    if (!confirm("¿Desea limpiar los filtros?")) return;

    toastr.info("Filtros limpiados");
    location.reload();
}
let involucradoActual = null;
function mostrarModalDetalle(x) {
    involucradoActual = x;

    let html = `
    <div class="box-tools pull-right">
        <button class="btn btn-primary btn-sm" onclick="compartirInvolucrado()">
            <i class="fa fa-share"></i> Compartir
        </button>
    </div>
        <div class="row">

            <div class="col-md-6">
                <b>Apellido:</b> ${x.apellido || ""}<br/>
                <b>Nombre:</b> ${x.nombre || ""}<br/>
                <b>DNI:</b> ${x.nroDni || ""}<br/>
                <b>Tipo DNI:</b> ${x.tipoDni || ""}<br/>
                <b>Género:</b> ${x.genero || ""}<br/>
                <b>Fecha Nac:</b> ${x.fechaNacimiento || ""}<br/>
            </div>

            <div class="col-md-6">
                <b>País:</b> ${x.paisOrigen || ""}<br/>
                <b>Provincia Nac:</b> ${x.provinciaNacimiento || ""}<br/>
                <b>Ciudad Nac:</b> ${x.ciudadNacimiento || ""}<br/>
                <b>Profesión:</b> ${x.profesion || ""}<br/>
            </div>

            <div class="col-md-12 mt-3">
                <b>Domicilio:</b><br/>
                ${x.calleDomicilio || ""} ${x.nroDomicilio || ""}<br/>
                ${x.localidadDomicilio || ""} - ${x.partidoDomicilio || ""}<br/>
                ${x.provinciaDomicilio || ""}
            </div>

            <div class="col-md-12 mt-3">
                <b>Observaciones:</b><br/>
                ${x.observaciones || ""}
            </div>

        </div>
    `;

    $("#detalleBody").html(html);
    $("#modalDetalle").modal("show");
}
function obtenerTextoInvolucrado() {

    if (!involucradoActual) return "";

    const x = involucradoActual;

    function safe(v) {
        return v || "-";
    }

    return `DATOS DEL INVOLUCRADO

Apellido: ${safe(x.apellido)}
Nombre: ${safe(x.nombre)}
DNI: ${safe(x.nroDni)} (${safe(x.tipoDni)})
Género: ${safe(x.genero)}
Fecha Nacimiento: ${safe(x.fechaNacimiento)}
País: ${safe(x.paisOrigen)}
Provincia Nac: ${safe(x.provinciaNacimiento)}
Ciudad Nac: ${safe(x.ciudadNacimiento)}
Ubicación:
${safe(x.calleDomicilio)} ${safe(x.nroDomicilio)}
${safe(x.localidadDomicilio)} - ${safe(x.partidoDomicilio)}
${safe(x.provinciaDomicilio)}

Profesión: ${safe(x.profesion)}

Observaciones:
${safe(x.observaciones)}
`;
}
function compartirInvolucrado() {

    const texto = obtenerTextoInvolucrado();

    if (!texto) {
        toastr.warning("No hay datos para compartir");
        return;
    }

    if (navigator.share) {

        navigator.share({
            title: 'Datos del Involucrado',
            text: texto
        })
            .then(() => console.log('Compartido'))
            .catch((error) => console.log('Error', error));

    } else {

        navigator.clipboard.writeText(texto);
        toastr.success("Copiado al portapapeles");
    }
}


