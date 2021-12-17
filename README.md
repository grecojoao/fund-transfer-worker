# Worker - Fund Transfer
[![NPM](https://img.shields.io/npm/l/react)](https://github.com/grecojoao/fund-transfer/blob/master/LICENSE) 

## ⚡ Sobre o projeto

Worker Fund Transfer é uma aplicação back-end desenvolvida para processar transações de valores entre contas.
A aplicação recebe por intermédio de filas no RabbitMq as mensagens a serem processadas(transações), as lê e processa na API AcessoAccount.
Durante o processamento o Status da transação é alterado no banco de dados, sobrescrevendo assim o status anterior informado pela [API Fund Transfer](https://github.com/grecojoao/fund-transfer).
Os dados da transação são tratados pela API antes de serem colocados na fila.

Possíveis status da transação durante todo o processamento:
- In Queue
- Processing
- Confirmed
- Error: "Invalid account number", "Insufficient funds", "Invalid balance".


Qualquer outro tipo de erro/falha que possa acontecer durante o processamento é tratado internamente, afim de manter a resiliência das operações.


## :rocket: Tecnologias
- C#, NET (6.0)
- Docker
- RabbitMq
- RavenDb

## 📝 Como executar o projeto
Pré-requisitos: 
- .NET Desktop Runtime 6.0.1 ou 
- SDK 6.0.1(desenvolvimento)
- RabbitMq rodando em localhost na porta padrão ou
- RabbitMq rodando e configuração da aplicação no AppSettings apontando para o RabbitMq
- RavenDb rodando em localhost na porta padrão ou
- RavenDb rodando e configuração da aplicação no AppSettings apontando para o RavenDb

````bash
# clonar o repositório
git clone https://github.com/grecojoao/fund-transfer-worker.git

# entrar na pasta do worker
cd fund-transfer-worker\src\Application\FundTransfer.Worker

# restaurar as dependências
dotnet restore

# executar o projeto
dotnet watch run
````
