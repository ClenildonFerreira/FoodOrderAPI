# FoodOrderAPI

API de gestão de pedidos para restaurantes, desenvolvida em **.NET 10** com foco em regras de negócio reais de salão, cozinha e delivery.

O projeto simula o fluxo operacional de um ERP gastronômico: cardápio, criação de pedidos, controle de status e histórico de alterações.

---

## Tecnologias

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger / OpenAPI
- TheMealDB (API externa de cardápio)

---

## Funcionalidades

### Produtos
- Listagem de produtos
- Busca por ID
- Importação automática de pratos via **TheMealDB**

### Pedidos
- Criação de pedidos (Salão ou Delivery)
- Listagem e consulta por ID
- Atualização controlada de status
- Histórico de mudanças de status
- Cálculo automático do total

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