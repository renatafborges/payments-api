# Kedu Payments API

API REST desenvolvida em **.NET 8** para gerenciamento de planos de pagamento, cobranças e pagamentos.

Projeto construído como parte de um desafio técnico, contemplando modelagem de domínio, regras de negócio, persistência relacional e endpoints REST.

---

## 🚀 Tecnologias Utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker
- xUnit (Testes de Integração)

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
---
## 📡 Endpoints Principais
### Centro de Custo
#### Criar centro de custo:  
Exemplo:  
`POST/centros-de-custo`

```json
{ "nome": "MENSALIDADE" }
```
#### Listar centro de custo
Exemplo:  
`GET/centros-de-custo`

---
### Responsáveis
#### Criar Responsável:
Exemplo:  
`POST/responsaveis`
```json
{ "nome": "Maria da Silva" }
```
---
### Planos de Pagamento
#### Criar Plano:
Exemplo:  
`POST/planos-de-pagamento`
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
#### Obter Plano:  
Exemplo:  
`GET/planos-de-pagamento/{id}`

#### Obter Total:  
Exemplo:  
`GET /planos-de-pagamento/{id}/total`

---
### Cobranças
#### Listar cobranças do Responsável:
Exemplo:  
`GET/responsaveis/{id}/cobrancas`  

Inclui:   
✔ Valor  
✔ Vencimento  
✔ Método  
✔ Código de pagamento  
✔ Status  
✔ Indicador de vencida  

#### Quantidade de cobranças:
Exemplo:  
`GET/responsaveis/{id}/cobrancas/quantidade`

---
### Pagamentos
#### Registrar pagamento:
Exemplo:  
`POST/cobrancas/{id}/pagamentos`  
```json
{
  "valor": 350.00,
  "dataPagamento": "2026-02-25T20:00:00"
}
```
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
- Pagamentos parciais foram simplificados (pagamento total)
- Status VENCIDA é calculado dinamicamente

--- 
### 👩‍💻 Autora

Renata Borges  
Backend Developer (.NET / C#)  