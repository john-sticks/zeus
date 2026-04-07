namespace SBInteligencia.DTO
{
    public class CoberturaResumenDto
    {
        public int PartidoId { get; set; }
        public string Partido { get; set; }

        public DateOnly? HechosDesde { get; set; }
        public DateOnly? HechosHasta { get; set; }
        public int TotalHechos { get; set; }

        public DateOnly? ImputadosDesde { get; set; }
        public DateOnly? ImputadosHasta { get; set; }
        public int TotalImputados { get; set; }

        public string Estado { get; set; }
        public string Detalle { get; set; }
    }
}
