# FoodOrderAPI

API de gestão de pedidos para restaurantes, desenvolvida em **.NET 10** com foco nas regras de negócio de operação de salão, cozinha e delivery.

O projeto simula o fluxo comercial de um ERP gastronômico, incluindo cardápio, criação de pedidos, controle de status, histórico de alterações, autenticação JWT e importação de produtos.

A ideia da aplicação surgiu após ver uma vaga em .NET para sistema de alimentação, exigindo boas práticas de desenvolvimento e regras de negócio reais.
---

## Tecnologias

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core + PostgreSQL (via Docker)
- MediatR (CQS)
- FluentValidation
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
- Criação de pedidos para salão e delivery
- Listagem com paginação e filtros comerciais
- Consulta e acompanhamento por ID
- Atualização controlada de status
- Histórico de mudanças de status
- Resumo por status para gestão de cozinha
- Cálculo automático do total do pedido

### Qualidade
- Validações de regras de negócio
- Middleware global de tratamento de erros
- CORS habilitado
- Testes unitários
- Cobertura de regras de negócio ~90%

---

## Regras de Negócio

### Modelagem de Domínio
O domínio principal é composto pelas seguintes entidades e enumerações:
- **User**: Representa os usuários do sistema, diferenciados pelo perfil de acesso.
- **Product**: Pratos ou bebidas disponíveis no cardápio.
- **Order**: Pedidos realizados (salão ou delivery).
- **OrderItem**: Itens que compõem um pedido.
- **OrderStatusHistory**: Registro histórico das mudanças de status de um pedido.
- **UserRole (Enum)**: Perfis de acesso do sistema, sendo 1=Admin, 2=Garçom e 3=Cozinha.

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
- Docker
- Docker Compose

### Passos

```bash
git clone https://github.com/ClenildonFerreira/FoodOrderAPI.git
cd FoodOrderAPI
dotnet restore
docker-compose up -d
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


### Como autenticar no Swagger

1. `POST /api/v1/auth/login` com usuário e senha
2. Copie o `token` da resposta
3. Clique em **Authorize**
4. Cole: `Bearer {seu_token}`
5. Agora os endpoints protegidos funcionam

### Login (exemplo)

```json
{
  "email": "admin@restaurante.com",
  "password": "admin123"
}
```

### Register (exemplo)

Para criar um novo usuário, envie os dados para `POST /api/v1/auth/register`:

```json
{
  "name": "João Garçom",
  "email": "joao.garcom@restaurante.com",
  "password": "senhaSegura123",
  "role": 2
}
```

> **Aviso de Segurança**: A chave criptográfica JWT não é versionada no `appsettings.json`. 

Para rodar o projeto localmente, configure a chave via **User Secrets** rodando os seguintes comandos no terminal:

```bash
dotnet user-secrets init

dotnet user-secrets set "Jwt:Key" "coloque-aqui-sua-chave-super-secreta-com-mais-de-32-caracteres"
```

Em ambiente de produção (Docker/Linux), utilize **Variáveis de Ambiente**:
`export Jwt__Key="sua-chave-aqui"`

---

## Endpoints

### Auth

| Método | Rota                       | Auth | Descrição                 |
|--------|----------------------------|------|---------------------------|
| POST   | `/api/v1/auth/login`       | Não  | Gera token JWT            |
| POST   | `/api/v1/auth/register`    | Não  | Registra um novo usuário  |

### Produtos

| Método | Rota                                      | Auth | Descrição                   |
|--------|-------------------------------------------|------|-----------------------------|
| GET    | `/api/v1/products`                        | *    | Lista produtos (paginado)   |
| GET    | `/api/v1/products/{id}`                   | *    | Busca produto por ID        |
| POST   | `/api/v1/products/import?quantity={qtd}`  | Sim  | Importa pratos do TheMealDB |

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
    { "productId": "d290f1ee-6c54-4b01-90e6-d701748f0851", "quantity": 2 },
    { "productId": "e14b2d30-8a1a-4f51-b01a-8c5e6f3d1f42", "quantity": 1 }
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

O projeto segue os princípios de **Clean Architecture**, com separação clara de responsabilidades:

```
FoodOrderAPI/
├── API/                              # Camada de Apresentação
│   ├── Controllers/                  # Endpoints da API
│   ├── Middleware/                   # Tratamento global de erros
│   └── Program.cs
├── Application/                      # Camada de Aplicação (Casos de Uso e CQS)
│   ├── Auth/                         # Features de Autenticação
│   ├── Common/                       # Funcionalidades Comuns e Pipelines
│   ├── DTOs/                         # Objetos de transferência de dados
│   ├── Interfaces/                   # Contratos (Services e Repositories)
│   ├── Orders/                       # Features de Pedidos
│   └── Products/                     # Features de Produtos
├── Domain/                           # Camada de Domínio
│   ├── Entities/                     # Entidades de negócio
│   └── Services/                     # Regras de domínio (ex: transição de status)
├── Infrastructure/                   # Camada de Infraestrutura
│   ├── Data/
│   │   ├── Repositories/             # Implementação dos repositórios
│   │   └── AppDbContext.cs           # Contexto do Entity Framework
│   └── Migrations/                   # Migrations do banco
├── FoodOrderAPI.Tests/               # Testes unitários e de integração
└── docs/
    └── postman/                      # Collection do Postman
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

**Clenildon Ferreira**  
[LinkedIn](https://www.linkedin.com/in/clenildonferreira)
