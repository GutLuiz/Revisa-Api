using UepaMed.Domain.Enums.Planilhas;

namespace UepaMed.Application.Dtos.Planilhas
{
    public class AdicionarColunaPlanilhaRespostaDto
    {
        public int ColunaId { get; set; }

        public string Nome { get; set; } = string.Empty;

        public TipoColunaPlanilha Tipo { get; set; }

        public int Ordem { get; set; }
    }
}