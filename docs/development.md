# Desenvolvimento local

## Inicialização

1. Instale SDK .NET 10.0.300 ou posterior da linha 10.0 e Docker/Compose.
2. Copie `.env.example` para `.env` e altere as senhas. Para estes scripts locais,
   use senhas alfanuméricas, sem espaços, aspas ou ponto e vírgula.
3. Execute `./scripts/dev-up.sh`.

PostgreSQL cria schemas e usuário da API na primeira inicialização do volume. Alterar
senhas no `.env` após essa etapa não altera senhas no banco: faça a rotação via SQL
e atualize a configuração. Não apague o volume para resolver isso se contiver dados necessários.

O serviço `migrate` usa o owner, aplica migrações versionadas e termina. A API só inicia
após sucesso. Sua inicialização normal nunca altera schema. O serviço
`tigerbeetle-format` formata apenas quando o arquivo não existe; não apaga dados ao reiniciar.
Uma atualização de versão do TigerBeetle exige seguir o procedimento de upgrade do fornecedor.

O Compose desativa a tentativa de GSS/Kerberos na conexão local, pois a imagem de runtime
não contém essa biblioteca. Veja a [documentação Npgsql](https://www.npgsql.org/doc/security.html).

```sh
docker compose ps -a
docker compose logs --tail=100 api migrate tigerbeetle
docker compose down
```

## Executar API no host

```sh
dotnet tool restore
dotnet restore
docker compose up -d postgres
set -a
. ./.env
set +a
export ConnectionStrings__Olgp="Host=localhost;Port=${POSTGRES_PORT:-5432};Database=openledg;Username=openledg_owner;Password=$POSTGRES_PASSWORD"
dotnet run --project src/OpenLedg.Api -- --migrate
export ConnectionStrings__Olgp="Host=localhost;Port=${POSTGRES_PORT:-5432};Database=openledg;Username=openledg_app;Password=$POSTGRES_APP_PASSWORD"
dotnet run --project src/OpenLedg.Api -- --urls http://localhost:8080
```

## Testes

```sh
dotnet build
dotnet test tests/OpenLedg.Tests --no-build
./scripts/test-integration.sh
```

Integração exige o banco inicializado e migrado. O script carrega `.env` e define
`OPENLEDG_TEST_OWNER` e `OPENLEDG_TEST_APP`. Os testes fazem rollback e utilizam
dados sintéticos. `dotnet test` na solução inteira exige essas variáveis; sem elas
os testes de integração falham explicitamente, sem serem silenciosamente ignorados.

## Migrações

```sh
dotnet ef migrations add NomeDaMudanca --project src/OpenLedg.Infrastructure --output-dir Persistence/Migrations
dotnet ef migrations has-pending-model-changes --project src/OpenLedg.Infrastructure
dotnet ef migrations script --idempotent --project src/OpenLedg.Infrastructure --output artifacts/migrations.sql
```

Crie `artifacts/` antes de exportar SQL. Revise o SQL e o snapshot. O comando `--migrate`
da API usa a mesma configuração de migrações do factory de design time. A string
de conexão vem de `ConnectionStrings__Olgp`; não há senha embutida no código.

## Git

Use commits pequenos e mensagens `tipo(escopo): descrição`, por exemplo:

```text
chore(repo): estrutura solução
feat(domain): adiciona participantes
feat(infra): prepara persistência e contêineres
test(db): valida restrições e isolamento
doc(project): documenta arquitetura e licenças
```

Stash é apenas uma pausa temporária, nomeada pelo mesmo padrão. A entrega deve ficar
visível no working tree ou em commits, nunca somente em stashes.
