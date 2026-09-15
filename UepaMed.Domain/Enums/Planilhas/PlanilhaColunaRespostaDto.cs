using UepaMed.Domain.Enums.Planilhas;

public class PlanilhaColunaRespostaDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public TipoColunaPlanilha Tipo { get; set; }

    public int Ordem { get; set; }
}