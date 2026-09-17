using UepaMed.Domain.Enums.Votacao;

namespace UepaMed.Application.Dtos.Votacoes
{
    public class VotacaoRespostaDto
    {
        public int Id { get; set; }

        public int RevisaoId { get; set; }

        public StatusVotacao Status { get; set; }

        public DateTime? DataInicio { get; set; }

        public DateTime? DataFinalizacao { get; set; }

        public int? ResponsavelConflitosUsuarioId { get; set; }

    }
}