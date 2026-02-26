# Kedu Payments API
API REST + GraphQL com testes de integração e execução via Docker.
Aplicação desenvolvida em **.NET 8** para gerenciamento de planos de pagamento, cobranças e pagamentos.

Projeto construído como parte de um desafio técnico, contemplando modelagem de domínio, regras de negócio, persistência relacional e endpoints REST.

---

## 🚀 Tecnologias Utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker
- xUnit (Testes de Integração)
- GraphQL (HotChocolate)
---

## 📌 Conceitos de Domínio

A aplicação trabalha com os seguintes conceitos:

- Responsável Financeiro
- Centro de Custo
- Plano de Pagamento
- Cobrança
- Pagamento

---

## ✅ Regras de Negócio Implementadas

✔ Um plano pertence a um único responsável  
✔ Um plano possui um único centro de custo  
✔ Centros de custo são cadastráveis via API  
✔ Total do plano é calculado automaticamente  
✔ Plano possui múltiplas cobranças  

✔ Status persistidos da cobrança:

- EMITIDA
- PAGA
- CANCELADA

✔ Status derivado:

- **VENCIDA** (calculado em consulta)

✔ Pagamentos:

- Alteram status para **PAGA**
- Bloqueados para cobranças **CANCELADAS**

✔ Código de pagamento gerado automaticamente:

- BOLETO → Código simulado
- PIX → Código simulado

---

## 🐘 Banco de Dados (PostgreSQL)

Executado via Docker.

### Subir banco:

```bash
docker compose up -d
```

### Parar Banco:
```bash
docker compose down
```

## ⚙️ Configuração
A string de conexão está em:    
`appsettings.json`

## 🧱 Migrations  
```bash
dotnet ef database update
```

## ▶️ Executar a API
```bash
dotnet run
```
Exemplo:  
`http://localhost:5047`

---
## 🧪 Testes via REST Client (.http)
Exemplo:  
`Kedu.Payments.Api.http`  
Permitindo testar facilmente os endpoints dentro do VS Code.

---
## 🧪 Testes
Executar testes de Integração:  
```bash
dotnet test
```
Windows: se ocorrer erro de arquivo bloqueado ao build (MSB3027/MSB3021), finalize a execução da API (Ctrl+C) e tente novamente.

---
## 📡 Endpoints Principais
### Centro de Custo
#### Criar centro de custo:  
Exemplo:  
`POST /centros-de-custo`
```json
{ "nome": "MENSALIDADE" }
```
#### Listar centros de custo
Exemplo:  
`GET /centros-de-custo`

---
### Responsáveis
#### Criar Responsável:
Exemplo:  
`POST /responsaveis`
```json
{ "nome": "Maria da Silva" }
```
#### Obter por Id:
Exemplo:  
`GET /responsaveis/{id}`

---
### Planos de Pagamento
#### Criar Plano:
Exemplo:  
`POST /planos-de-pagamento`
```json
{
  "responsavelId": 1,
  "centroDeCustoId": 1,
  "cobrancas": [
    {
      "valor": 350.00,
      "dataVencimento": "2026-03-10",
      "metodoPagamento": "BOLETO"
    }
  ]
}
```
#### Obter Plano por Id:  
Exemplo:  
`GET /planos-de-pagamento/{id}`

#### Obter Total:  
Exemplo:  
`GET /planos-de-pagamento/{id}/total`

---
### Cobranças
#### Listar cobranças do Responsável:
Exemplo:  
`GET /responsaveis/{id}/cobrancas`  

Inclui:   
✔ Valor  
✔ Vencimento  
✔ Método  
✔ Código de pagamento  
✔ Status  
✔ Indicador de vencida  

#### Listar planos de pagamento por Responsável:
Exemplo:  
`GET /responsaveis/{id}/planos-de-pagamento`

#### Quantidade de cobranças:
Exemplo:  
`GET /responsaveis/{id}/cobrancas/quantidade`

---
### Pagamentos
#### Registrar pagamento:
Exemplo:  
`POST /cobrancas/{id}/pagamentos`  
```json
{
  "valor": 350.00,
  "dataPagamento": "2026-02-25T20:00:00"
}
```

## 🧩 GraphQL

Além dos endpoints REST, a API também expõe operações via **GraphQL (HotChocolate)**.

### URL
`http://localhost:5047/graphql`

Ao rodar em ambiente de desenvolvimento, o HotChocolate disponibiliza uma interface para executar consultas/mutações (GraphQL Playground).

### ✅ Queries

**Listar Centros de Custo**
```graphql
query {
  centrosDeCusto {
    id
    nome
  }
}
```

**Consultar Responsável (com planos)**
```graphql
query {
  responsavel(id: 3) {
    id
    nome
    planosDePagamento {
      id
      total
    }
  }
}
```

**Consultar Plano de Pagamento (com centro de custo e cobranças)**
```graphql
query {
  planoPagamento(id: 1) {
    id
    total
    centroDeCusto { id nome }
    cobrancas {
      id
      valor
      status
      metodoPagamento
      dataVencimento
      codigoPagamento
    }
  }
}
```

**Listar Cobranças**
```graphql
query {
  cobrancas {
    id
    valor
    status
    metodoPagamento
    dataVencimento
  }
}
```

**Listar Cobranças Vencidas (regra derivada)**
```graphql
query {
  cobrancasVencidas {
    id
    valor
    status
    metodoPagamento
    dataVencimento
  }
}
```
### ✅ Mutation
**Registrar Pagamento**
```graphql
mutation {
  registrarPagamento(cobrancaId: 3, valor: 350.00) {
    cobrancaId
    status
  }
}
```
Observação: a data do pagamento é registrada usando horário local (compatível com timestamp without time zone no PostgreSQL).

---

### 🎯 Objetivos

✔ Modelagem de domínio  
✔ Uso de EF Core com PostgreSQL  
✔ Implementação de regras de negócio  
✔ API REST estruturada  
✔ Separação entre entidades e contratos  
✔ Status derivado calculado  
✔ Simulação de integrações financeiras  

---
### 📌 Observações
- Códigos de pagamento são simulados
- Pagamentos parciais não são permitidos (é exigido pagamento integral da cobrança).
- Status VENCIDA é calculado dinamicamente

--- 
### 👩‍💻 Autora

Renata Borges  
Backend Developer (.NET / C#)  