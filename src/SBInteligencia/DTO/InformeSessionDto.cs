namespace SBInteligencia.DTO
{
    public class InformeSessionDto
    {
        public int? IdInforme { get; set; }
        public string Nombre { get; set; }

        public List<HechoRefDto> Hechos { get; set; } = new();
    }

    public class HechoRefDto
    {
        public int IdHecho { get; set; }
        public int Anio { get; set; }
    }
}
