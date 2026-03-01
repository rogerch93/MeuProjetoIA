namespace BoloGestaoAPI.Models
{
    public class MaterialUsado
    {
        public int Id { get; set; }
        public int VendaId { get; set; }
        public string Nome { get; set; } = "";
        public decimal Quantidade { get; set; }
        public decimal CustoUnitario { get; set; }
        public decimal CustoTotal => Quantidade * CustoUnitario;
    }
}
