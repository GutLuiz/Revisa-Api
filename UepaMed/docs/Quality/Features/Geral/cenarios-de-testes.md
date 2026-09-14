# Visão Geral - Cenários de Testes

- Premissas:
	= Existem os papéis: Proprietário, Revisor, Avaliador e Colaborador.
    = Somente o Proprietário pode excluir uma revisão.
    = Revisor, Avaliador e Colaborador podem sair da revisão a qualquer momento.
    = Título, domínio e tipo de revisão são obrigatórios.
    = Descrição é opcional.
    = todos esses cenários o usuário deve estar autenticado.

- Criar revisão válida:
	= Título, domínio e tipo de revisão válidos
	-> A revisão é criada com sucesso

# Validações da Revisão:

01 - Validar Títutlo Obrigatório
	= Título não informado
	-> A criação é rejeitada com a mensagem "Título não pode ser nulo"

02 - Validar Título somente numérico 
	= Título composto somente por números
	-> A criação é rejeitada com a mensagem "Título não pode ser composto somente por números"

03 - Validar caracteres especiais no título
	= Título contendo caracteres especiais não permitidos
	-> Criação é rejeitada com a mensagem de validação de título.

04 - Validar tamanho de título 
	= Título com menos de 10 caracteres 
	-> A criação é rejeitad com a mensagem de validação para título

05 - Validar limite mínimo aceito do título
	= Título válido com 10 ou mais caracteres 
	-> A revisão é criada com sucesso

06 - Validar domínio obrigátorio
	= Domínio não informado 
	-> A criacção é rejeitada com erro de campo obrigatório

07 - Validar tipo de revisão obrigatório 
	= Tipo de revisão não informado
	-> A criação é rejeitada com erro de campo obrigatório 

08 - Validar descrição opcional
	= Criar revisão sem descrição
	-> A revisão é criada com sucesso

# Validações de edição:

01 - Editar revisão como proprietário
	= Proprietário da revisão altera dados válidos
	-> A revisão é editada com sucesso

02 - Editar revisão como não proprietário
	= Revisor, Avaliador ou colaborador tenta editar a revisão 
	-> A edição é rejeitada

# Validações de Exclusão/saída:

01 - Excluir revisão como proprietário
	= Proprietário solicita a exclusão da revisão 
	-> O usuário deixa de participar da revisão, e a revisão continua existindo para os demais participantes

02 - Sair da revisão como participante
	= Revisor, Avaliador ou colaborador solicita saída da revisão
	-> O usuário deixa de participar da revisão, e a revisão continua existindo para os demais participantes

03 - Impedir exclusão por não proprietário 
	= Revisor, Avaliador ou colaborador tenta excluir a revisão
	-> A exclusão é rejeitada e a revisão continua existindo














	
