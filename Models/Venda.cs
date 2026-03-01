namespace BoloGestaoAPI.Models
{
    public class Venda
    {
        public int Id { get; set; }
        public DateTime Data { get; set; } = DateTime.UtcNow;
        public decimal ValorTotal { get; set; }
        public int QuantidadeBolos { get; set; }
        public string FormaPagamento { get; set; } = "";
        public string Usuario { get; set; } = "root";
    }
}
