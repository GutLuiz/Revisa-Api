using UepaMed.Domain.Entities.Usuarios;
using UepaMed.Domain.Enums;

namespace UepaMed.Domain.Entities.Revisoes
{
    public class Revisao
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public string Titulo { get; set; } = string.Empty;

        public TipoRevisao Tipo { get; set; }

        public DominioRevisao Dominio { get; set; }

        public string? Descricao { get; set; }

        public DateTime DataCriacao { get; set; }

        public string? CriteriosVotacao { get; set; }

        public DateTime? DataAtualizacao { get; set; }
    }
}
