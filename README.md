# FoodOrderAPI

API de gestão de pedidos para restaurantes, desenvolvida em **.NET 10** com foco em regras de negócio reais de salão, cozinha e delivery.

O projeto simula o fluxo operacional de um ERP gastronômico: cardápio, criação de pedidos, controle de status, histórico de alterações, autenticação JWT e importação de produtos.

---

## Tecnologias

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core + SQLite
- JWT Authentication
- xUnit + FluentAssertions + Moq
- Swagger / OpenAPI
- TheMealDB (API externa de cardápio)

---

## Funcionalidades

### Autenticação
- Login com JWT
- Endpoints protegidos com `[Authorize]`
- Swagger configurado com Bearer token

### Produtos
- Listagem de produtos ativos (com paginação)
- Busca por ID
- Importação de pratos via **TheMealDB**

### Pedidos
- Criação de pedidos (Salão ou Delivery)
- Listagem com paginação
- Filtro por status e tipo
- Consulta por ID
- Atualização controlada de status
- Histórico de mudanças de status
- Resumo por status (dashboard)
- Cálculo automático do total

### Qualidade
- Validações de regras de negócio
- Middleware global de tratamento de erros
- Testes unitários
- Cobertura de regras de negócio ~90%

---

## Regras de Negócio

### Transições de Status permitidas

| Status Atual | Pode ir para              |
|--------------|---------------------------|
| Received     | Preparing, Cancelled      |
| Preparing    | Ready, Cancelled          |
| Ready        | Delivered, Cancelled      |
| Delivered    | (final)                   |
| Cancelled    | (final)                   |

### Validações na criação do pedido
- Nome do cliente é obrigatório
- Pedido deve conter pelo menos 1 item
- Quantidade deve ser maior que zero
- Pedidos de salão exigem número da mesa
- Produtos inativos não podem ser pedidos

---

## Como executar

### Pré-requisitos
- .NET 10 SDK

### Passos

```bash
git clone https://github.com/ClenildonFerreira/FoodOrderAPI.git
cd FoodOrderAPI
dotnet restore
dotnet ef database update
dotnet run