using UepaMed.Domain.Enums.Planilhas;

namespace UepaMed.Application.Dtos.Prisma
{
    
    public class PrismaResumoDto
    {
        public PrismaIdentificacaoDto Identificacao { get; set; } = new();
        public PrismaDuplicatasDto Duplicatas { get; set; } = new();
        public PrismaTriagemDto Triagem { get; set; } = new();
        public PrismaElegibilidadeDto Elegibilidade { get; set; } = new();
        public PrismaAmostraFinalDto AmostraFinal { get; set; } = new();


    }

    public class PrismaIdentificacaoDto
    {
        public int PubMed { get; set; }
        public int SciELO { get; set; }
        public int Scopus { get; set; }
        public int BVS { get; set; }

        public int SemBasePesquisa { get; set; }

        public int TotalRegistros =>
            PubMed + SciELO + Scopus + BVS + SemBasePesquisa;
    }

    public class PrismaFonteDto
    {
        public int QuantidadeArquivos { get; set; }

        public int QuantidadeRegistros { get; set; }
    }
        public class PrismaDuplicatasDto
    {
        public int RegistrosRemovidos { get; set; }

        public int RegistrosAposDeduplicacao { get; set; }
    }
    public class PrismaTriagemDto
    {
        public int PubMed { get; set; }
        public int SciELO { get; set; }
        public int Scopus { get; set; }
        public int BVS { get; set; }
        public int SemBasePesquisa { get; set; }

        public int ArtigosParaVotacao =>
            PubMed + SciELO + Scopus + BVS + SemBasePesquisa;

        public int ArtigosPendentes { get; set; }
        public int ArtigosIncluidos { get; set; }
        public int ArtigosExcluidos { get; set; }
    }
    public class PrismaElegibilidadeDto
    {
        public int PubMed { get; set; }
        public int SciELO { get; set; }
        public int Scopus { get; set; }
        public int BVS { get; set; }
        public int SemBasePesquisa { get; set; }

        public int TotalEstudosSelecionadosLeituraIntegra =>
            PubMed + SciELO + Scopus + BVS + SemBasePesquisa;

        public List<PrismaMotivoExclusaoDto> MotivosExclusao { get; set; }
            = new();
    }
    public class PrismaMotivoExclusaoDto
    {
        public MotivoExclusaoElegibilidade Motivo { get; set; }

        public int QuantidadeArtigos { get; set; }
    }
    public class PrismaAmostraFinalDto
    {
        public int PubMed { get; set; }
        public int SciELO { get; set; }
        public int Scopus { get; set; }
        public int BVS { get; set; }
        public int SemBasePesquisa { get; set; }

        public int TotalArtigos =>
            PubMed + SciELO + Scopus + BVS + SemBasePesquisa;
    }
}