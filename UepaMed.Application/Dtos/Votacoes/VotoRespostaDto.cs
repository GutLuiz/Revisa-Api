using UepaMed.Domain.Enums.Votacao;

namespace UepaMed.Application.Dtos.Votacoes
{
    public class VotoRespostaDto
    {
        public int Id { get; set; }

        public int VotacaoId { get; set; }

        public int ArtigoId { get; set; }

        public int UsuarioId { get; set; }

        public OpcaoVoto Opcao { get; set; }

        public DateTime DataRegistro { get; set; }
        public int? ResponsavelConflitosUsuarioId { get; set; }
    }
}