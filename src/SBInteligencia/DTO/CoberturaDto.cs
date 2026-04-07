namespace SBInteligencia.DTO
{
    public class CoberturaDTO
    {
        public int PartidoId { get; set; }
        public string Partido { get; set; }

        public DateOnly? HechosDesde { get; set; }
        public DateOnly? HechosHasta { get; set; }

        public DateOnly? ImputadosDesde { get; set; }
        public DateOnly? ImputadosHasta { get; set; }
        public int TotalHechos { get; set; }
        public int TotalImputados { get; set; }
    }
}
