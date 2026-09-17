using UepaMed.Domain.Enums;
using UepaMed.Domain.Enums.Votacao;
using UepaMed.Domain.Enums.Revisoes;


namespace UepaMed.Domain.Entities.Votacoes
{
    public class Votacao
    {
        public int Id { get; private set; }

        public int RevisaoId { get; private set; }

        public StatusVotacao Status { get; private set; }
            = StatusVotacao.NaoIniciada;

        public DateTime? DataInicio { get; private set; }

        public DateTime? DataFinalizacao { get; private set; }

        public ICollection<Voto> Votos { get; private set; }
            = new List<Voto>();
        public ICollection<VotacaoArtigo> Artigos { get; private set; }
            = new List<VotacaoArtigo>();

        public ICollection<VotacaoParticipante> Participantes { get; private set; }
            = new List<VotacaoParticipante>();

        public ICollection<ConflitoVotacao> Conflitos
        {
            get;
            private set;
        } = new List<ConflitoVotacao>();

        private Votacao()
        {
        }

        public Votacao(int revisaoId)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            RevisaoId = revisaoId;
            Status = StatusVotacao.NaoIniciada;
        }

        public void Iniciar()
        {
            var possuiProprietario = Participantes.Any(participante =>
                 participante.Papel == PapelMembroRevisao.Proprietario);

            if (!possuiProprietario)
            {
                throw new InvalidOperationException(
                    "Não é possível iniciar a votação sem um proprietário.");
            }
            if (Status != StatusVotacao.NaoIniciada)
            {
                throw new InvalidOperationException(
                    "Esta votação já foi iniciada.");
            }

            Status = StatusVotacao.EmAndamento;
            DataInicio = DateTime.UtcNow;
        }

        public Voto RegistrarVoto(
            int artigoId,
            int usuarioId,
            OpcaoVoto opcao)
        {
            if (Status != StatusVotacao.EmAndamento)
            {
                throw new InvalidOperationException(
                    "A votação não está em andamento.");
            }

            var votoExistente = Votos.FirstOrDefault(v =>
                v.ArtigoId == artigoId &&
                v.UsuarioId == usuarioId);

            if (votoExistente != null)
            {
                votoExistente.AlterarOpcao(opcao);

                return votoExistente;
            }

            var voto = new Voto(
                Id,
                artigoId,
                usuarioId,
                opcao);

            Votos.Add(voto);

            return voto;
        }

        public StatusArtigo? ApurarArtigo(
        int artigoId)
        {
            if (Status != StatusVotacao.EmAndamento)
            {
                throw new InvalidOperationException(
                    "A votação não está em andamento.");
            }

            var votosDoArtigo = Votos
                .Where(v => v.ArtigoId == artigoId)
                .ToList();

            if (!votosDoArtigo.Any())
            {
                throw new InvalidOperationException(
                    "O artigo ainda não possui votos.");
            }

            var quantidadeIncluir = votosDoArtigo.Count(v =>
                v.Opcao == OpcaoVoto.Incluir);

            var quantidadeExcluir = votosDoArtigo.Count(v =>
                v.Opcao == OpcaoVoto.Excluir);

            if (quantidadeIncluir > quantidadeExcluir)
            {
                return StatusArtigo.Incluido;
            }

            if (quantidadeExcluir > quantidadeIncluir)
            {
                return StatusArtigo.Excluido;
            }

            var todosSeAbstiveram =
                quantidadeIncluir == 0 &&
                quantidadeExcluir == 0;

            var motivoConflito = todosSeAbstiveram
                ? MotivoConflito.TodosSeAbstiveram
                : MotivoConflito.Empate;

            AdicionarConflito(
                artigoId,
                motivoConflito);

            return null;
        }

        public ConflitoVotacao AdicionarConflito(
            int artigoId,
            MotivoConflito motivo)
        {
            if (Status != StatusVotacao.EmAndamento)
            {
                throw new InvalidOperationException(
                    "Não é possível criar conflitos nesta etapa.");
            }

            var conflitoExistente = Conflitos.FirstOrDefault(c =>
                c.ArtigoId == artigoId);

            if (conflitoExistente != null)
            {
                return conflitoExistente;
            }

            var conflito = new ConflitoVotacao(
                Id,
                artigoId,
                motivo);

            Conflitos.Add(conflito);

            return conflito;
        }

        public void IniciarResolucaoConflitos()
        {
            if (Status != StatusVotacao.EmAndamento)
            {
                throw new InvalidOperationException(
                    "A votação inicial não está em andamento.");
            }

            if (!Conflitos.Any())
            {
                throw new InvalidOperationException(
                    "A votação não possui conflitos.");
            }

            Status = StatusVotacao.ResolucaoConflitos;
        }

        public void ResolverConflito(
            int conflitoId,
            int avaliadorId,
            OpcaoVoto decisaoFinal)
        {
            if (Status != StatusVotacao.ResolucaoConflitos)
            {
                throw new InvalidOperationException(
                    "A votação não está na etapa de conflitos.");
            }

            var conflito = Conflitos.FirstOrDefault(c =>
                c.Id == conflitoId);

            if (conflito == null)
            {
                throw new KeyNotFoundException(
                    "Conflito não encontrado.");
            }

            conflito.Resolver(
                avaliadorId,
                decisaoFinal);
        }

        public bool TodosConflitosForamResolvidos()
        {
            return Conflitos.All(c => c.Resolvido);
        }

        public void Finalizar()
        {
            if (Status == StatusVotacao.NaoIniciada)
            {
                throw new InvalidOperationException(
                    "Não é possível finalizar uma votação que não foi iniciada.");
            }

            if (Status == StatusVotacao.Finalizada)
            {
                throw new InvalidOperationException(
                    "Esta votação já foi finalizada.");
            }

            if (Status == StatusVotacao.ResolucaoConflitos &&
                !TodosConflitosForamResolvidos())
            {
                throw new InvalidOperationException(
                    "Ainda existem conflitos não resolvidos.");
            }

            Status = StatusVotacao.Finalizada;
            DataFinalizacao = DateTime.UtcNow;
        }
        public void AdicionarParticipante(
        int usuarioId,
        PapelMembroRevisao papel)
        {
            if (Status != StatusVotacao.NaoIniciada)
            {
                throw new InvalidOperationException(
                    "Não é possível adicionar participantes após iniciar a votação.");
            }

            var participanteJaExiste = Participantes.Any(participante =>
                participante.UsuarioId == usuarioId);

            if (participanteJaExiste)
            {
                throw new InvalidOperationException(
                    "O usuário já participa desta votação.");
            }

            Participantes.Add(
                new VotacaoParticipante(usuarioId, papel));
        }
        public void AdicionarArtigo(int artigoId)
        {
            if (Status != StatusVotacao.NaoIniciada)
            {
                throw new InvalidOperationException(
                    "Não é possível adicionar artigos após iniciar a votação.");
            }

            var artigoJaExiste = Artigos.Any(artigo =>
                artigo.ArtigoId == artigoId);

            if (artigoJaExiste)
            {
                throw new InvalidOperationException(
                    "O artigo já participa desta votação.");
            }

            Artigos.Add(new VotacaoArtigo(artigoId));
        }

        public int? ResponsavelConflitosUsuarioId { get; private set; }

        public void DefinirResponsavelConflitos(int usuarioId)
        {
            if (Status != StatusVotacao.NaoIniciada)
            {
                throw new InvalidOperationException(
                    "Não é possível definir o responsável após iniciar a votação.");
            }

            var participante = Participantes.FirstOrDefault(participante =>
                participante.UsuarioId == usuarioId);

            var podeSerResponsavel = participante is not null &&
                (participante.Papel == PapelMembroRevisao.Proprietario ||
                 participante.Papel == PapelMembroRevisao.Revisor);

            if (!podeSerResponsavel)
            {
                throw new InvalidOperationException(
                    "O responsável pelos conflitos deve ser proprietário ou revisor da votação.");
            }

            ResponsavelConflitosUsuarioId = usuarioId;
        }
    }
}