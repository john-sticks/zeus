namespace SBInteligencia.DTO
{
    public class DashboardDTO
    {
        public int TotalInformes { get; set; }

        public int TotalHechos { get; set; }
        public DateTime? HechosDesde { get; set; }
        public DateTime? HechosHasta { get; set; }

        public int TotalInvolucrados { get; set; }
        public DateTime? InvolucradosDesde { get; set; }
        public DateTime? InvolucradosHasta { get; set; }
    }
}
