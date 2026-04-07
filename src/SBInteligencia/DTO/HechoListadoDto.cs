namespace SBInteligencia.DTO
{
    public class HechoListadoDto
    {
        public int IdHecho { get; set; }
        public string NroRegistro { get; set; }
        public string? Ipp { get; set; }
        public DateOnly FechaCarga { get; set; }
        public string Partido { get; set; }
        public string? Localidad { get; set; }
        public string? Calle { get; set; }
        public string? Altura { get; set; }
        public string Calificaciones { get; set; }
        public string? Relato { get; set; }
        public string? Latitud { get; set; }
        public string? Longitud { get; set; }
        public string? Dependencia { get; set; }
    }
}