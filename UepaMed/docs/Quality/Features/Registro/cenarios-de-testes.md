# Registro de usuário - Cenários de teste 

- Realizar cadastro válido:
	= Nome válido, email válido e ainda não cadastrado, senha com 6 ou mais caracteres
	-> A conta é criada com sucesso
	-> Status code 201 (created)

# Válidações do Nome:

01 - Válidar ausência do nome:
	= Nome não informado
	-> O cadastro é rejeitado e a API informa o erro de campo obrigátorio.

02 - Válidar tamanho mínimo do nome:
	= Nome com 1 caractere 
	-> O cadastro é rejeitado e a API informa erro de validação do campo.

03 - Válidar limite mínimo do nome:
	= Nome com 2 caracteres válidos
	-> O cadastro é realizado com sucesso

04 - Válidar o tamnho máximo do nome:
	= Nome com 51 caracteres
	-> O cadastro é rejeitado e a API informa erro de validação do campo

05 - Válidar números no nome:
	= Nome contendo número 
	-> O cadastro é rejeitado e a API informa erro de validação do campo

06 - Válidar caracteres especiais no nome:
	= Nome contendo carctere especial
	-> O cadastro é rejeitado e a API informa erro de validação do campo

# Validações do email

01 - Válidar ausência do email:
	= Email não informado
	-> O cadastro é rejeitado e a API informa o erro de validação do campo

02 - Válidar email duplicado:
	= O email já vinculado a uma conta
	-> O cadastro é rejeitado  e a API informa que o email já está em uso

03 - Válidar unicidade sem diferenciar maiúscula e minúscula:
	= Email já cadastrado, enviado com letras maiúsculas ou minúsculas diferentes
	-> O cadastro é rejeitado como email duplicado.

# Validações da senha

01 - Válidar ausência de senha:
	= Senha não informada 
	-> O cadastro é rejeitado e a API informa erro de campo obrigátorio para campo

02 - Válidar tamanho mínimo da senha:
	= Senha com 6 caracteres 
	-> O cadastro é rejeitado e a API informa erro de validação do campo

# Validações de segurança

03 - Válidar não exposição de dados sensíveis na resposta 
	= Realiar cadastro válido
	-> A resposta da API não contem senha nem hash da senha

04 - Válidar não exposição de dados sensíveis em logs
	= Realizar cadastro no ambiente de testes com acesso aos logs
	-> Os logs não contém senha nem hash da senha



	









