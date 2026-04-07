using System;
using System.Collections.Generic;

namespace SBInteligencia.DTO
{
    public class FiltrosInvolucrados
    {
        public int Anio { get; set; }

        // 🔹 PAGINACIÓN
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 100;

        // 🔹 FILTROS
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? NroDni { get; set; }

        public List<string>? Partido { get; set; }

        // 🔹 FLAGS (igual que hechos)
        public bool UsarNombre { get; set; }
        public bool UsarApellido { get; set; }
        public bool UsarDni { get; set; }
        public bool UsarPartido { get; set; }
    }
}