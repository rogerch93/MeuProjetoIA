namespace MeuProjetoIA.Models;

public class MensagemIA
{
    public int Id {get;set;}
    public string Prompt {get;set;} = string.Empty;
    public string Resposta {get;set;} = string.Empty;
    public DateTime CriadoEm {get;set;} = DateTime.UtcNow;
    public string Usuario {get;set;} = Environment.UserName;
}
