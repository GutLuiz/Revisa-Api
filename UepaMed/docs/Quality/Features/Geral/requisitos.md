## Visão geral - requisitos:
A API deve permitir a criação e o gerenciamento de revisões por usuários.
As permissões de visualização, edição, exclusão e saída devem respeitar o papel do usuário na revisão.


# Requisitos Funcionais 

01 - Criar Revisão
	= O sistema deve permitir que usuários autenticados criem uma nova revisão.

02 - Listar revisões do proprietário
	= O usuário com papel de proprietário deve visualizar somente as revisões que criou 
	ou das quais é proprietário.

03 - Listar revisões de participantes
	= Usuários com papel diferente de proprietário devem visualizar somente as revisões das quais participam.

04 - Editar revisão
	= O sistema deve permitir a edição de uma revisão somente ao usuário proprietário dela.

05 - Excluir revisão
	= O sistema deve permitir que o proprietário exclua uma revisão.

06 - Sair de uma revisão
	= O sistema deve permitir que participantes que não sejam proprietários saiam de uma revisão da qual 
	participam.

# Requisitos de Válidações

01 - Autenticação obrigatória
	= A criação de uma revisão deve ser permitida somente para usuários autenticados.

02 - Título obrigatório
	= Não deve ser possível criar uma revisão sem informar título.

03 - Domínio obrigatório
	= Não deve ser possível criar uma revisão sem informar domínio.

04 - Tipo de revisão obrigatório
	= Não deve ser possível criar uma revisão sem informar o tipo de revisão.

# Requisitos de permissão

01 - Restrição de edição
	= Usuários que não sejam proprietários não devem conseguir editar uma revisão.

02 - Restrição de exclusão
	= Usuários que não sejam proprietários não devem conseguir excluir uma revisão.

03 - Restrição de saída do proprietário
	= O proprietário não deve sair da revisão utilizando a ação de saída destinada aos participantes. 
	Para encerrar sua relação com a revisão, deve utilizar a exclusão da revisão.






















	- Criar Revisão: O usuário deve conseguir criar uma revisão se estiver logado
	- Título obrigatório: Não deve ser possível criar uma revisão sem Título. 
	- Domínio obrigatório: Não deve ser possível criar uma revisão sem domínio.
	- Tipo Obrigatório: Não deve ser possível criar uma revisão sem tipo de revisão.
	- Listar revisões proprietário: O proprietário deve visualizar somente as revisões
	que criou/possui
	- Listar revisões revisor ou outros papéis: o papel diferente de propietário deve visualizar
	as revisões que participa
	- Editar revisão: Deve ser possível editar uma revisão somente se for proprietário
	- Excluir/sair: Deve ser possível sair (outros papéis) ou excluir (propietário) das
	revisões.
	
	