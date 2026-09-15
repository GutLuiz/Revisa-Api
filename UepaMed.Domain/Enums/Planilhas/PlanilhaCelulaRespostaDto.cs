public class PlanilhaCelulaRespostaDto
{
    public int Id { get; set; }

    public int ColunaId { get; set; }

    public string? Valor { get; set; }

    // Útil para exibir o nome no dropdown de responsável.
    public string? ValorExibicao { get; set; }
}