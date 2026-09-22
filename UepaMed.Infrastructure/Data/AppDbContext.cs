using Microsoft.EntityFrameworkCore;
using UepaMed.Domain.Entities.Arquivos;
using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Entities.Bibliotecas;
using UepaMed.Domain.Entities.Planilhas;
using UepaMed.Domain.Entities.Revisoes;
using UepaMed.Domain.Entities.Usuarios;
using UepaMed.Domain.Entities.Votacoes;

namespace UepaMed.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Revisao> Revisoes => Set<Revisao>();
        public DbSet<RevisaoMembro> RevisoesMembro => Set<RevisaoMembro>();
        public DbSet<Artigo> Artigos => Set<Artigo>();
        public DbSet<ArquivoImportacao> ArquivosImportacao { get; set; }
        public DbSet<ConviteRevisao> ConvitesRevisao { get; set; }
        public DbSet<DuplicidadeIgnorada> DuplicidadesIgnoradas { get; set; }
        public DbSet<Votacao> Votacoes { get; set; }

        public DbSet<Voto> Votos { get; set; }

        public DbSet<ConflitoVotacao> ConflitosVotacao { get; set; }
        public DbSet<VotacaoParticipante> VotacaoParticipantes { get; set; }
        public DbSet<VotacaoArtigo> VotacaoArtigos { get; set; }

        public DbSet<PlanilhaRevisao> PlanilhasRevisao { get; set; }
        public DbSet<PlanilhaColuna> PlanilhasColuna { get; set; }
        public DbSet<PlanilhaLinha> PlanilhasLinha { get; set; }
        public DbSet<PlanilhaCelula> PlanilhasCelula { get; set; }

        public DbSet<Biblioteca> Bibliotecas => Set<Biblioteca>();
        public DbSet<ArquivoImportacaoBiblioteca> ArquivosImportacaoBiblioteca
            => Set<ArquivoImportacaoBiblioteca>();
        public DbSet<ArtigoBiblioteca> ArtigosBiblioteca
            => Set<ArtigoBiblioteca>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<ArquivoImportacao>()
             .HasOne(a => a.Revisao)
             .WithMany()
             .HasForeignKey(a => a.RevisaoId)
             .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Artigo>()
            .HasOne(a => a.ArquivoImportacao)
            .WithMany(ai => ai.Artigos)
            .HasForeignKey(a => a.ArquivoImportacaoId)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RevisaoMembro>()
            .HasOne(rm => rm.Revisao)
            .WithMany()
            .HasForeignKey(rm => rm.RevisaoId)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ArquivoImportacao>()
            .Property(arquivo => arquivo.Origem)
            .HasConversion<int>()
            .IsRequired();
            modelBuilder.Entity<ArquivoImportacao>()
            .Property(arquivo => arquivo.BasePesquisa)
            .HasConversion<int?>();
            modelBuilder.Entity<ConviteRevisao>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Revisao)
                    .WithMany()
                    .HasForeignKey(c => c.RevisaoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.UsuarioConvidado)
                    .WithMany()
                    .HasForeignKey(c => c.UsuarioConvidadoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.ConvidadoPorUsuario)
                    .WithMany()
                    .HasForeignKey(c => c.ConvidadoPorUsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new
                {
                    c.RevisaoId,
                    c.UsuarioConvidadoId,
                    c.Status
                });
            });
            modelBuilder.Entity<Votacao>(entity =>
            {
                entity.HasKey(v => v.Id);

                entity.Property(v => v.Status)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(v => v.DataInicio);

                entity.Property(v => v.DataFinalizacao);

                entity.HasIndex(v => v.RevisaoId);

                entity.HasOne<Revisao>()
                    .WithMany()
                    .HasForeignKey(v => v.RevisaoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.Votos)
                    .WithOne(v => v.Votacao)
                    .HasForeignKey(v => v.VotacaoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.Conflitos)
                    .WithOne(c => c.Votacao)
                    .HasForeignKey(c => c.VotacaoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(v => v.Participantes)
                    .WithOne(p => p.Votacao)
                    .HasForeignKey(p => p.VotacaoId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(v => v.Artigos)
                    .WithOne(artigo => artigo.Votacao)
                    .HasForeignKey(artigo => artigo.VotacaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Voto>(entity =>
            {
                entity.HasKey(v => v.Id);

                entity.Property(v => v.Opcao)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(v => v.DataRegistro)
                    .IsRequired();

                entity.HasIndex(v => new
                {
                    v.VotacaoId,
                    v.ArtigoId,
                    v.UsuarioId
                })
                .IsUnique();

                entity.HasOne(v => v.Artigo)
                    .WithMany()
                    .HasForeignKey(v => v.ArtigoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VotacaoParticipante>(entity =>
            {
                entity.HasKey(participante => participante.Id);

                entity.Property(participante => participante.Papel)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(participante => participante.EhVotanteObrigatorio)
                    .IsRequired();

                entity.HasIndex(participante => new
                {
                    participante.VotacaoId,
                    participante.UsuarioId
                })
                .IsUnique();

                entity.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(participante => participante.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<VotacaoArtigo>(entity =>
            {
                entity.HasKey(artigo => artigo.Id);

                entity.HasIndex(artigo => new
                {
                    artigo.VotacaoId,
                    artigo.ArtigoId
                })
                .IsUnique();

                entity.HasOne(artigo => artigo.Artigo)
                    .WithMany()
                    .HasForeignKey(artigo => artigo.ArtigoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ConflitoVotacao>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Motivo)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(c => c.DecisaoFinal)
                    .HasConversion<int>();

                entity.Property(c => c.Resolvido)
                    .IsRequired();

                entity.Property(c => c.DataCriacao)
                    .IsRequired();

                entity.Property(c => c.DataResolucao);

                entity.HasIndex(c => new
                {
                    c.VotacaoId,
                    c.ArtigoId
                })
                .IsUnique();

                entity.HasOne(c => c.Artigo)
                    .WithMany()
                    .HasForeignKey(c => c.ArtigoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<PlanilhaRevisao>(entity =>
            {
                entity.HasKey(planilha => planilha.Id);

                entity.HasIndex(planilha => planilha.RevisaoId)
                    .IsUnique();

                entity.HasOne(planilha => planilha.Revisao)
                    .WithMany()
                    .HasForeignKey(planilha => planilha.RevisaoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(planilha => planilha.Colunas)
                    .WithOne(coluna => coluna.Planilha)
                    .HasForeignKey(coluna => coluna.PlanilhaRevisaoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(planilha => planilha.Linhas)
                    .WithOne(linha => linha.Planilha)
                    .HasForeignKey(linha => linha.PlanilhaRevisaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlanilhaColuna>(entity =>
            {
                entity.HasKey(coluna => coluna.Id);

                entity.Property(coluna => coluna.Nome)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(coluna => coluna.Tipo)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(coluna => coluna.CampoArtigoOrigem)
                    .HasConversion<int?>();

                entity.HasIndex(coluna => new
                {
                    coluna.PlanilhaRevisaoId,
                    coluna.Ordem
                })
                .IsUnique();
            });

            modelBuilder.Entity<PlanilhaLinha>(entity =>
            {
                entity.HasKey(linha => linha.Id);

                entity.HasOne(linha => linha.Artigo)
                    .WithMany()
                    .HasForeignKey(linha => linha.ArtigoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<PlanilhaCelula>(entity =>
            {
                entity.HasKey(celula => celula.Id);

                entity.Property(celula => celula.Valor)
                    .HasMaxLength(8000);

                entity.HasIndex(celula => new
                {
                    celula.PlanilhaLinhaId,
                    celula.PlanilhaColunaId
                })
                .IsUnique();

                entity.HasOne(celula => celula.Linha)
                    .WithMany(linha => linha.Celulas)
                    .HasForeignKey(celula => celula.PlanilhaLinhaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(celula => celula.Coluna)
                    .WithMany(coluna => coluna.Celulas)
                    .HasForeignKey(celula => celula.PlanilhaColunaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Biblioteca>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Titulo).IsRequired();

                entity.HasIndex(b => b.UsuarioId);

                entity.HasOne(b => b.Usuario)
                    .WithMany()
                    .HasForeignKey(b => b.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(b => b.Importacoes)
                    .WithOne(i => i.Biblioteca)
                    .HasForeignKey(i => i.BibliotecaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ArquivoImportacaoBiblioteca>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.NomeArquivo).IsRequired();

                entity.Property(i => i.TipoArquivo)
                    .HasConversion<int>()
                    .IsRequired();

                entity.HasMany(i => i.Artigos)
                    .WithOne(a => a.ArquivoImportacao)
                    .HasForeignKey(a => a.ArquivoImportacaoBibliotecaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(importacao => importacao.BasePesquisa)
                    .HasConversion<int?>();
            });

            modelBuilder.Entity<ArtigoBiblioteca>()
                .HasKey(a => a.Id);
        }
    }
}
