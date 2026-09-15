using UepaMed.Domain.Enums.Planilhas;

namespace UepaMed.Application.Dtos.Planilhas
{
    public class AdicionarColunaPlanilhaDto
    {
        public string Nome { get; set; } = string.Empty;

        public TipoColunaPlanilha Tipo { get; set; }
    }
}