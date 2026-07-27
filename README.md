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
- CORS habilitado
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
```

Swagger:

```
https://localhost:{porta}/swagger
```

### Rodar os testes

```bash
dotnet test
```

---

## Autenticação (JWT)

### Usuários de demonstração

| Usuário  | Senha      |
|----------|------------|
| admin    | admin123   |
| garcom   | garcom123  |
| cozinha  | cozinha123 |

### Como autenticar no Swagger

1. `POST /api/v1/auth/login` com usuário e senha
2. Copie o `token` da resposta
3. Clique em **Authorize**
4. Cole: `Bearer {seu_token}`
5. Agora os endpoints protegidos funcionam

### Login (exemplo)

```json
{
  "username": "admin",
  "password": "admin123"
}
```

> Em produção, a chave JWT deve vir de **User Secrets** ou variável de ambiente, nunca commitada em texto puro.

---

## Endpoints

### Auth

| Método | Rota                    | Auth | Descrição        |
|--------|-------------------------|------|------------------|
| POST   | `/api/v1/auth/login`    | Não  | Gera token JWT   |

### Produtos

| Método | Rota                              | Auth | Descrição                   |
|--------|-----------------------------------|------|-----------------------------|
| GET    | `/api/v1/products`                | *    | Lista produtos (paginado)   |
| GET    | `/api/v1/products/{id}`           | *    | Busca produto por ID        |
| POST   | `/api/v1/products/import`         | Sim  | Importa pratos do TheMealDB |

### Pedidos

| Método | Rota                              | Auth | Descrição                                 |
|--------|-----------------------------------|------|-------------------------------------------|
| GET    | `/api/v1/orders`                  | Sim  | Lista pedidos (paginado)                  |
| GET    | `/api/v1/orders?status=Preparing` | Sim  | Filtra por status                         |
| GET    | `/api/v1/orders?type=1`           | Sim  | Filtra por tipo (1=Salão, 2=Delivery)     |
| GET    | `/api/v1/orders/summary`          | Sim  | Resumo por status                         |
| GET    | `/api/v1/orders/{id}`             | Sim  | Busca pedido por ID                       |
| POST   | `/api/v1/orders`                  | Sim  | Cria pedido                               |
| PATCH  | `/api/v1/orders/{id}/status`      | Sim  | Atualiza status                           |

---

## Exemplos

### Criar pedido (salão)

```json
{
  "customerName": "João Silva",
  "tableNumber": "15",
  "type": 1,
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 3, "quantity": 1 }
  ]
}
```

### Atualizar status

```json
{
  "status": 2,
  "notes": "Pedido em preparo na cozinha"
}
```

### Filtrar pedidos da cozinha

```
GET /api/v1/orders?status=Preparing
```

---

## Estrutura do Projeto

```
FoodOrderAPI/
├── Controllers/           # Endpoints da API
├── DTOs/                  # Objetos de transferência
├── Models/                # Entidades de domínio
├── Services/              # Regras de negócio + Auth
├── Middleware/            # Tratamento global de erros
├── Data/                  # DbContext
├── Migrations/            # Migrations do banco
├── FoodOrderAPI.Tests/    # Testes unitários
├── docs/
│   └── postman/           # Collection do Postman
└── Program.cs
```

---

## Collection do Postman

```
docs/postman/FoodOrderAPI.postman_collection.json
```

Fluxo sugerido:
1. Login
2. Importar pratos
3. Criar pedido
4. Atualizar status
5. Ver resumo

---

## Autor

**José Clenildon Ferreira do Nascimento**  
[LinkedIn](https://www.linkedin.com/in/clenildonferreira)  
[GitHub](https://github.com/ClenildonFerreira)
