# Registro de usuário - Requisitos:
A API deve permitir o registro de novos usuários para que possam possuir uma conta e, 
posteriormente(login), acessar funcionalidades protegidas do sistema.

# Requisitos Funcionais:

01 - Criar Conta:
	= O sistema deve permitir o cadastro de uma nova conta mediante o envio de nome, e-mail e senha.

02 - Confirmar registro concluído:
	= Ao concluir um registro válido, o sistema deve informar que a conta foi criada com sucesso.

03 - Informar falha no registro:
	= Quando o registro não puder ser concluído, o sistema deve retornar uma mensagem de erro 
	adequada, sem expor informações sensíveis.

# Regras de Validação:

01 - Campo Obrigatório:
	= Nome, e-mail e senha são campos obrigatórios para o registro de uma conta.

02 - Formato de email:
	= O sistema deve aceitar apenas emails em formato válido.

03 - Duplicidade de email:
	= O email informado não pode estar vinculado a outra conta.
	- O email deve ser único independentemente de letras maiúsculas e minúsculas.

04 - Validação do nome:
	= O nome deve possuir entre 2 e 50 caracteres.
	= O nome não deve conter números nem caracteres especiais.

05 - Validação da senha:
	= A senha deve possuir, no mínimo, 6 caracteres.

# Requsitos de segurança:

01 - Proteção da ssenha:
	= A senha nunca deve ser armazenada em texto puro.

02 - Armazenamento segura da senha:
	= A senha deve ser armazenada no banco de dados utlizando algoritimo de hash seguro.

03 - Não exposição da senha:
	= A senha não deve ser retornada nas respostas da API, em mensagens de erro ou em logs.













	