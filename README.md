# OpenLedg

Software em .NET 10, ASP.NET Core/Kestrel e PostgreSQL (OLGP),
com TigerBeetle como destino exclusivo do estado financeiro.

O PostgreSQL guarda perfis demográficos, conformidade, permissões, metadados de
autenticação e roteamento. **Saldos e lançamentos pertencem ao TigerBeetle.**

## Estrutura

```text
src/
  OpenLedg.Domain/           Participantes e invariantes, sem dependências externas
  OpenLedg.Application/      Contratos de persistência e integração com o ledger
  OpenLedg.Infrastructure/   EF Core, PostgreSQL, repositório e migrações
  OpenLedg.Api/              Kestrel, composição, saúde e comando de migração
tests/
  OpenLedg.Tests/            Domínio, limites arquiteturais e modelo EF
  OpenLedg.IntegrationTests/ Restrições e privilégios no PostgreSQL real
infra/postgres/             Inicialização de schemas e papel da aplicação
scripts/                    Execução e testes locais
docs/                       Arquitetura, operação, segurança e ADRs
```

## Executar

Pré-requisitos: Docker com Compose v2+ e .NET SDK 10.0.300 ou posterior da linha 10.0 para desenvolvimento no host.
Reserve memória suficiente na VM Docker para PostgreSQL, build .NET e TigerBeetle.

```sh
cp .env.example .env
# Edite as senhas locais em .env antes de iniciar.
./scripts/dev-up.sh
curl --fail http://localhost:8080/health/live
curl --fail http://localhost:8080/health/ready
```

O script levanta PostgreSQL, aplica a migração em processo separado e inicia API,
TigerBeetle e Redis. O Redis fica no perfil opcional `auxiliary`; para omiti-lo,
execute `docker compose up -d --build --wait` diretamente.

A API usa um usuário sem DDL e sem acesso ao schema de credenciais. O owner é
exclusivo de bootstrap/migração. Os endpoints atuais são `GET /`, `/health/live`
e `/health/ready`. Readiness verifica PostgreSQL, migrações e leitura de perfis;
não representa disponibilidade de pagamentos ou do ledger.

```sh
dotnet tool restore
dotnet restore
dotnet build --no-restore
dotnet test tests/OpenLedg.Tests --no-build
./scripts/test-integration.sh
docker compose down
```

Os volumes persistem após `down`. Não use `down -v` salvo se quiser apagar os dados locais.
Portas no host: API 8080, PostgreSQL 5432 e TigerBeetle 3000, vinculadas a loopback.

## Escopo entregue

- TPH: `BaseParticipant`, `IndividualClient`, `CorporateClient` e `PaymentInitiator`.
- UUIDv4, criação em UTC, KYC/AML pendentes e idempotência única no cadastro.
- CPF/CNPJ, referências DICT, representantes legais, MFA/WebAuthn, webhooks e mTLS/DPoP.
- Schemas `olgp`, `security` e `routing`, migração inicial e índices únicos.
- FK composta e CHECK impedem que ITP receba conta de liquidação, mesmo por SQL direto.
- Contrato para consultar contas no ledger e roteamento com IDs UInt128 sem perda de precisão.
- Dockerfile, Compose, testes, CI e licenciamento duplo.

Esta sprint prepara a integração. Ainda não há cliente TigerBeetle conectado à aplicação,
transferências, integração DICT, validação real KYC/AML, autenticação, execução de webhooks
ou protocolo Open Finance. CPF/CNPJ recebem normalização e validação de formato;
dígitos verificadores e comprovação cadastral ainda não são validados.

## Documentação e contribuição

Comece por [docs/README.md](docs/README.md). As regras de trabalho e a convenção de
commits `feat(escopo)`, `fix(escopo)`, `doc(escopo)` estão em [AGENTS.md](AGENTS.md).

## Licença

Disponível sob **MIT OU Apache-2.0**, à escolha do utilizador.
Consulte [LICENSE](LICENSE), [LICENSE-MIT](LICENSE-MIT) e [LICENSE-APACHE](LICENSE-APACHE).
