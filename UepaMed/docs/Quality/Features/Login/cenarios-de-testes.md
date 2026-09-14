# Login de usuário - Cenários de teste 

- Realizar login válido:
	= email cadastrado e senha correta 
	-> O login é concluído com sucesso e a resposta contém apenas informações não sensíveis do usuário

# Validações do Email:

01 - Válidar ausência do email
	= Email não informado 
	-> O login é rejeitado conforme o contrato de erro da API

02 - Válidar formato do email 
	= Email em formato inválido 
	-> O login é rejeitado conforme o contrato de erro da API

03 - Válidar email não cadastrado 
	= Email válido, mas não cadastrado, e qualquer senha
	-> O login é rejeitado com a mensagem Email ou senha inválidos.

04 - Válidar login sem diferenciar maiúsculas e minúsculas
	= Email cadastrado enviado com letras maiúsculas ou minúsculas diferentes e senha correta
	-> O login é concluído com sucesso

# Validações do Nome:

01 - Válidar ausência de senha
	= email cadastrado e senha não informada
	-> O login é rejeitado conforme o contrato de erro da API

02 - Válidar senha incorreta 
	= email cadastrado e senha incorreta
	-> O login é rejeitado com a mensagem email ou senha inválidos

# Validações de Segurança:

01 - Validar ausência de sessão após falha
	= Tentativa de login malsucedida
	-> Nenhum cookie ou token de autenticação é criado

02 - Validar envio de cookies no login
	= Realizar login com credenciais válidas
	-> A resposta contém os cookies de autenticação no cabeçalho Set-Cookie.

03 - Validar acesso protegido com sessão válida
	= Utilizar cookie de autenticação válido em endpoint protegido
	-> O acesso ao endpoint é autorizado.

04 - Validar sessão expirada
	= Utilizar cookie de autenticação após sua expiração
	-> O acesso ao endpoint protegido é rejeitado.






















- Login realizado com sucesso:
	-> Informar e-mail e senha de um usuário cadastrado → Login concluído, mensagem de sucesso e 
	informações não sensíveis retornadas.
- Validações do e-mail:
	-> Informar e-mail nulo → Login rejeitado.
	-> Informar e-mail em formato inválido → Login rejeitado.
	-> Informar e-mail não cadastrado → Login rejeitado com a mensagem “E-mail ou senha inválidos”.
	-> Informar e-mail cadastrado utilizando letras maiúsculas → Login realizado normalmente.
- Validações da senha:
	-> Informar senha nula → Login rejeitado.
	-> Informar senha incorreta → Login rejeitado com a mensagem “E-mail ou senha inválidos”.
- Validações das credenciais:
	-> Informar e-mail não cadastrado e uma senha qualquer → Login rejeitado com a 
	mensagem genérica “E-mail ou senha inválidos”.
	-> Informar e-mail cadastrado e senha incorreta → Login rejeitado com a mesma 
	mensagem genérica “E-mail ou senha inválidos”.
	-> Examinar uma tentativa malsucedida → Nenhum token ou cookie de autenticação é criado.
	- Validações do token e cookie:
	-> Realizar login com credenciais válidas → Token válido enviado no 
	cookie de autenticação pelo cabeçalho Set-Cookie.
	-> Examinar o cookie de autenticação → Cookie contém os atributos HttpOnly, 
	Secure, SameSite e tempo de expiração.
	-> Utilizar o cookie em um endpoint protegido → Acesso autorizado.
	-> Acessar um endpoint protegido sem o cookie → Acesso rejeitado.
    -> Utilizar um token inválido ou alterado → Acesso rejeitado.
    -> Utilizar o cookie após sua expiração → Acesso rejeitado.
- Validações de segurança:
	-> Examinar o corpo da resposta do login → Resposta não contém token, senha nem hash.
	-> Examinar as informações do usuário retornadas → Resposta contém somente informações não sensíveis.
	-> Comparar as respostas para e-mail inexistente e senha incorreta → Ambas retornam a mesma mensagem genérica.
	-> Examinar os logs durante o login → Logs não contêm senha, hash nem token.