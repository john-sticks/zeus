namespace SBInteligencia.DTO
{
    public class CrearInformeDto
    {
        public int? IdInforme { get; set; }

        public List<HechoRefDto> Hechos { get; set; } = new();

        public string? Nombre { get; set; }

        public string? Usuario { get; set; }
        public string? Area { get; set; }

        public bool Reemplazar { get; set; }
    }
}