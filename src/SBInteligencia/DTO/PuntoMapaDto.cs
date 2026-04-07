namespace SBInteligencia.DTO
{
    public class PuntoMapaDto
    {
        public int Id { get; set; }

        public double? Latitud { get; set; }
        public double? Longitud { get; set; }

        public string Titulo { get; set; }
        public string Descripcion { get; set; }

        public string Calificacion { get; set; }

        // 🔹 opcional (te sirve para reutilizar el mismo JS de búsqueda)
        public string Partido { get; set; }
        public string Localidad { get; set; }
    }
}