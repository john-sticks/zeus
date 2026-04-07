namespace SBInteligencia.DTO
{
    public class InformeDetalleDto
    {
        public int IdInforme { get; set; }
        public string Nombre { get; set; }
        public List<PuntoMapaDto> Hechos { get; set; } = new();
    }
}
