# Revisão — Requisitos Funcionais

# Requisitos funcionais 

# Visão geral e participantes

01 - Visualizar dados da revisão
	= Todos os participantes ativos da revisão devem visualizar suas informações de cadastro: título,
	domínio, tipo, descrição, data de criação e status

02 - Visualizar indicadores após votação cega
	= Os indicadores e resultados finais da revisão devem permanecer indisponíveis até o encerramento da votação cega.

03 - Visualizar membros da revisão
	= Todos os participantes ativos devem visualizar a lista de membros da revisão, contendo nome, 
	papel e status de participação.

04 - Convidar membros
	= Somente o Proprietário deve poder convidar novos membros para a revisão.

05 - Status de participação
	= A lista de membros deve identificar participantes ativos 

06 - Aceite de convite
	= Convites para participar de uma revisão podem ser aceitos antes do início ou após o término da votação.
	= Não deve ser permitido aceitar novos membros durante uma votação ativa.

07 - Saída de participante
	= Revisor, Avaliador e Colaborador podem sair da revisão a qualquer momento.
	Sem apagar atividades que já tenham sido registradas por ele.

# Análise de artigos e votação cega

01 - Importar artigos
	= Usuários com papel Proprietário ou Revisor devem poder importar arquivos de artigos para a revisão.

02 - Resolver artigos duplicados
	= Somente o Proprietário deve poder resolver artigos duplicados identificados pelo sistema.
	= Artigos duplicados devem voltar para a lista de artigos como excluído

03 - Excluir artigos importados
	= Usuários com papel Proprietário ou Revisor devem poder excluir arquivos de artigos incluídos na revisão

04 - Visualizar lista de artigos
	= Todos os participantes ativos devem poder visualizar os artigos da revisão na seção Lista de artigos.

05 - Visualizar dados de análise
	= Proprietário, Revisor e Avaliador devem ter acesso às seções Visão geral do progresso, Votação 
	individual e Artigos em conflito.

06 - Restringir dados de análise ao Colaborador
	= Usuários com papel Colaborador não devem visualizar as seções Visão geral do progresso, 
	Votação individual e Artigos em conflito.

07 - Exibir progresso individual
	= A visão geral do progresso deve apresentar estatísticas individuais de participação de cada membro

08 - Preservar sigilo durante votação cega
	= Durante a votação cega, a porcentagem individual de participação de cada membro pode ser 
	exibida, desde que não revele suas decisões de voto.
	= As decisões individuais de cada participante devem permanecer ocultas para os demais
	membros durante a votação cega.

09 -  Encerrar votação automaticamente
	= Quando todos os participantes ativos e elegíveis concluírem seus votos, a votação deve ser 
	encerrada automaticamente.

10 - Disponibilizar resultado da votação
	= Após o encerramento da votação cega, o sistema deve disponibilizar os resultados finais e 
	identificar os artigos em conflito.

# Planilha de complementação e decisão final

01 - Adicionar informações complementares
	= Os participantes ativos devem poder adicionar informações complementares aos artigos incluídos na revisão

02 - Realizar votação final
	= Os participantes ativos devem realizar uma nova etapa de votação para definir quais artigos 
	farão parte da revisão final.

03 - Classificar artigos visualmente+
	= Os artigos devem ser classificados visualmente por cor após a análise complementar:
	Verde: artigo incluído na revisão final.
    Vermelho: artigo excluído após análise complementar.

04 - Preservar votação anterior
	= A classificação por cores e a etapa de votação final não devem alterar os resultados da votação anterior
