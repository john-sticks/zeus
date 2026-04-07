namespace SBInteligencia.DTO
{
    public class InvolucradoListadoDto
    {
        public int Id { get; set; }
        public int IdHecho { get; set; }

        public string Involucrado { get; set; }
        public string TipoDni { get; set; }
        public string NroDni { get; set; }

        public string Apellido { get; set; }
        public string Nombre { get; set; }

        public string Genero { get; set; }

        public string Partido { get; set; }
        public string Localidad { get; set; }

        public DateOnly? FechaNacimiento { get; set; }

        public string Profesion { get; set; }
    }
}
