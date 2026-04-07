using System;

namespace SBInteligencia.DTO
{
    public class FiltrosHechos
    {
        public int Anio { get; set; }

        public DateOnly FechaDesde { get; set; }
        public DateOnly FechaHasta { get; set; }

        public bool UsarIPP { get; set; }
        public string? IPP { get; set; }

        // 🔥 MULTI
        public List<string>? Partido { get; set; }

        public bool UsarLocalidad { get; set; }
        public string? Localidad { get; set; }

        // 🔥 MULTI
        public List<string>? Calificaciones { get; set; }

        public bool UsarRelato { get; set; }
        public string? Relato { get; set; }

        public bool UsarDomicilio { get; set; }
        public string? Calle { get; set; }
        public int? AlturaDesde { get; set; }
        public int? AlturaHasta { get; set; }

        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 100;

        public bool UsarGeo { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public int RadioMetros { get; set; } = 1000;
    }
}