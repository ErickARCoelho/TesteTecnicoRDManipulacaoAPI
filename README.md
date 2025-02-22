# TesteTecnicoRDManipulacaoAPI
Avaliação Técnica

Uma API desenvolvida em .NET Core para manipulação de vídeos obtidos a partir da API do YouTube, com operações de consulta, inserção, atualização e exclusão (soft delete). A aplicação utiliza SQLite para persistência e possui autenticação JWT para proteger endpoints de escrita.

## Funcionalidades

- **Consulta e Filtragem:**  
  Filtra vídeos por título, duração, autor, data de publicação e busca geral (em título, descrição e nome do canal).

- **CRUD de Vídeos:**  
  Endpoints para inserir, atualizar e excluir (soft delete) registros de vídeos.

- **Consumo da API do YouTube:**  
  Integração com a YouTube Data API para buscar vídeos relacionados à manipulação de medicamentos (vídeos brasileiros do ano de 2025).

- **Autenticação JWT:**  
  Implementação de login para emissão de token JWT que protege endpoints de escrita (inserção, atualização e exclusão).

- **Middleware Global de Exceção:**  
  Tratamento padronizado de erros através de um middleware que intercepta exceções não tratadas e retorna uma resposta JSON.

- **Testes Automatizados:**  
  Suíte de testes utilizando NUnit, Moq e InMemory DbContext para garantir 100% de cobertura das controllers.

## Tecnologias Utilizadas

- [.NET Core](https://dotnet.microsoft.com/)
- [Entity Framework Core](https://docs.microsoft.com/ef/)
- [SQLite](https://www.sqlite.org/index.html)
- [JWT Authentication](https://jwt.io/)
- [Swagger/OpenAPI](https://swagger.io/)
- [NUnit](https://nunit.org/)
- [Moq](https://github.com/moq/moq)
- [Microsoft.AspNetCore.Mvc.Testing](https://docs.microsoft.com/aspnet/core/test/integration-tests)

## Configuração do Ambiente

### Variáveis de Ambiente

A aplicação utiliza a variável de ambiente `YOUTUBE_API_KEY` para acessar a API do YouTube.

### Como Executar
1. Usando bash
    - Clone o Repositório: https://github.com/ErickARCoelho/TesteTecnicoRDManipulacaoAPI.git
    - cd RDManipulacaoAPI
2. Compilar e Executar a Aplicação:
    -Abra o projeto no Visual Studio 2022 e execute a aplicação (F5 ou Ctrl+F5).

 ### Executando os Testes
- Abra o Test Explorer: No Visual Studio, acesse Test > Test Explorer.

### Estrutura do Projeto
Controllers:
VideosController.cs: Gerencia os endpoints para CRUD de vídeos e filtragem.
AuthController.cs: Gerencia o login e emissão de token JWT.

Models:
Video.cs: Modelo que representa os dados dos vídeos.
LoginModel.cs: Modelo para dados de autenticação.

Data:
AppDbContext.cs: Contexto do Entity Framework Core para o banco SQLite.

Services:
IYouTubeService.cs e YouTubeService.cs: Serviço para consumo da API do YouTube.

Middlewares:
ExceptionMiddleware.cs: Middleware global para tratamento de exceções.
ExceptionMiddlewareExtensions.cs: Método de extensão para registro do middleware.

Testes:
Projeto de testes (NUnit) com cobertura de 100% para as controllers.

### Usuário e senha padrão para teste na autenticação - (para teste em cenário real utilizariamo credenciais de um serviço escolhido)
- Usuário: admin 
- Senha: password