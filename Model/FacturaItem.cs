namespace Gestion.Model
{
    public class FacturaItem
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => Precio * Cantidad;
}
}