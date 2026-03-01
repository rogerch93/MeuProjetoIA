namespace BoloGestaoAPI.Models
{
    public class Gasto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; } = DateTime.UtcNow;
        public string Descricao { get; set; } = "";
        public decimal Valor { get; set; }
        public string Usuario { get; set; } = "root";
    }
}
