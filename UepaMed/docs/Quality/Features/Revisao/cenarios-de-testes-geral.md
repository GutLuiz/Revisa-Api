# Revisão - Visão Geral - Cenários de Testes

- Premissas:
	= O usuário está autenticado, exceto quando o caso indicar o contrário.
	= Existem os papéis Proprietário, Revisor, Avaliador e Colaborador.
	= Uma votação pode estar nos estados: não iniciada, ativa ou encerrada.
	= Membros que saíram devem permanecer no histórico da revisão.

- Visualizar informações da revisão
	= Participante ativo abre uma revisão da qual faz parte
	-> A visão geral apresenta título, domínio, tipo, descrição, data de criação e status da revisão.

- Visualizar revisão sem descrição
	= Participante abre uma revisão criada sem descrição
	-> A visão geral é apresentada normalmente; a descrição pode estar ausente ou nula, conforme o contrato da API.

- Validar campos obrigatórios da revisão na visão geral
	= Abrir uma revisão criada com dados válidos
	-> Título, domínio e tipo de revisão são apresentados com valores preenchidos.

- Visualizar resumo geral durante votação cega
	= Participante abre uma revisão com votação cega ativa
	-> São exibidos apenas indicadores gerais permitidos, como total de artigos, arquivos importados e itens pendentes.

- Proteger resultados durante votação cega
	= Participante abre a visão geral durante votação cega ativa
	-> Decisões individuais, resultados finais e artigos em conflito não são exibidos

- Visualizar lista de membros
	= Participante ativo abre a lista de membros
	-> A lista apresenta somente membros ativos e membros que saíram da revisão; usuários sem vínculo não são exibidos.

- Validar dados exibidos de cada membro
	= Abrir a lista de membros de uma revisão
	-> Cada membro apresenta nome, e-mail, papel e status de votação (se for necessário).

- Validar sigilo na lista de membros durante votação cega
	= Abrir a lista de membros com votação cega ativa
	-> A lista pode apresentar progresso percentual, mas não revela decisões individuais de voto.

- Validar saída de participante
	-> Revisor, Avaliador ou Colaborador sai da revisão
	= O usuário deixa de constar entre membros ativos e permanece no histórico com status de saída.

- Convidar participante como proprietário
	-> Proprietário convida usuário já cadastrado, que ainda não participa da revisão, fora de votação ativa
	= O convite é criado com sucesso.

- Impedir convite por não proprietário
	-> Revisor, Avaliador ou Colaborador tenta convidar outro usuário
	= A criação do convite é rejeitada.

- impedir convite ao próprio proprietário
	-> Proprietário tenta convidar a si mesmo
	= A criação do convite é rejeitada.

- Impedir convite a membro existente
	= Proprietário tenta convidar usuário que já participa da revisão
	-> A criação do convite é rejeitada.

- impedir convite a usuário não cadastrado
	= Proprietário informa um usuário sem conta no sistema
	-> A criação do convite é rejeitada.

- Aceitar convite antes da votação
	= Usuário convidado aceita convite antes do início da votação
	-> O usuário passa a integrar a revisão como participante ativo.

- Impedir aceite durante votação ativa
	= Usuário convidado tenta aceitar convite com votação ativa
	-> O aceite do convite é rejeitado.

- Aceitar convite após votação encerrada
	= Usuário convidado aceita convite após o término da votação
	-> O usuário passa a integrar a revisão como participante ativo.


