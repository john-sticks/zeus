namespace SBInteligencia.DTO
{
    public class HechoDetalleDto
    {
        public int IdHecho { get; set; }
        public string NroRegistro { get; set; }
        public string Ipp { get; set; }
        public DateOnly FechaCarga { get; set; }
        public string Partido { get; set; }
        public string Localidad { get; set; }
        public string Calle { get; set; }
        public string Altura { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public string Calificaciones { get; set; }
        public string Dependencia { get; set; }
        public string Relato { get; set; }
        public string LatitudRaw { get; set; }
        public string LongitudRaw { get; set; }

        public bool TieneCoordenadas => Latitud.HasValue && Longitud.HasValue;
    }
}
