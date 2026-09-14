# Login do usuário - Requisitos:
A API deve permitir que usuários já cadastrados se autentiquem utilizando e-mail e senha. 
Quando a autenticação for válida, o sistema deve iniciar uma sessão segura por meio de cookies de 
autenticação.

# Requisitos Funcionais:

01 -  Realizar Login:
    = O sistema deve permitir que o um usuário realize login informando email e senha válidos

02 - Confirmar login concluído 
    = Ao concluir um login válido, o sistema deve informar que a autenticação foi realizada com sucesso

03 - Retornar dados não sensíveis:
    = Após a autenticação bem-sucedida, o corpo da resposta deve conter somente informações não 
    sensíveis do usuário.

04 - Enviar cookies de autenticação 
    = Após a autenticação bem-sucedida, o backend deve enviar os cookies de autenticação por meio 
    do cabeçalho Set-Cookie.

# Requisitos de Validação:

01 - Campos Obrigátorios
    = E-mail e senha são campos obrigatórios para a realização do login.

02 - Credenciais válidas
    = O login deve ser permitido somente quando o e-mail estiver cadastrado e a senha informada for 
    correspondente à conta.

03 - Mensagem para credenciais inválidas
    = Quando o login não puder ser concluído por e-mail inexistente ou senha incorreta, o sistema 
    deve retornar a mensagem genérica: E-mail ou senha inválidos.

# Requisitos de Segurança:

01 - Não exposição de dados sensíveis
    = O corpo da resposta não deve conter token, senha ou hash da senha.

02 - Cookies protegidos
    = Os cookies access_token e refresh_token devem possuir os atributos HttpOnly, Secure, 
    SameSite e tempo de expiração configurado.

03 - Proteção contra enumeração de usuários
    = O sistema deve utilizar a mesma mensagem de erro para e-mail inexistente e senha incorreta, 
    evitando informar se determinado e-mail possui uma conta cadastrada.

# Logout do usuário — Requisitos
A API deve permitir que o usuário encerre sua sessão de forma segura, removendo os cookies
de autenticação e invalidando o token de atualização associado à sessão.

# Requisitos Funcionais 

01 - Realizar logout
    = O sistema deve permitir que o usuário autenticado encerre sua sessão.

02 - Remover cookies de autenticação
    = Ao realizar logout, o sistema deve remover os cookies access_token e refresh_token do navegador.

03 - Invalidar refresh token
    = Ao realizar logout, o sistema deve invalidar no banco de dados o refresh token associado ao usuário.

# Requisitos de segurança

01 - Bloquear acesso após logout
    = Após o logout, o usuário não deve conseguir acessar endpoints protegidos utilizando a sessão encerrada.

02 - Impedir renovação de sessão após logout
    = Após o logout, não deve ser possível gerar novos tokens utilizando o refresh token que foi invalidado.




