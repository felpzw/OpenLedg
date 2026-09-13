# Instruções para agentes e colaboradores

## Escopo e arquitetura

- Leia `README.md`, `docs/architecture.md` e os ADRs antes de alterar limites do domínio.
- Respeite as dependências: Domain não depende de frameworks; Application depende de Domain;
  Infrastructure implementa contratos de Application; Api é a raiz de composição e usa Kestrel.
- PostgreSQL (OLGP) armazena perfis, conformidade, permissões, credenciais e roteamento.
  Nunca adicione saldos ou lançamentos financeiros ao banco relacional. TigerBeetle é o ledger.
- PaymentInitiator não pode possuir conta de liquidação direta. Preserve as restrições
  de domínio, CHECK e FK composta que impõem essa regra.
- Preserve UUIDv4, UTC e unicidade das chaves de idempotência de cadastro.
- Não confunda restrição única com protocolo completo de idempotência de pagamentos.

## Segurança e persistência

- Não registre CPF, CNPJ, chaves PIX, certificados completos ou credenciais em logs.
- Nunca versione `.env`, chaves privadas, senhas ou dados biométricos. Use referências
  a um cofre para segredos e metadados de WebAuthn para autenticação biométrica.
- A API usa `openledg_app`; migrações usam `openledg_owner`. Não conceda acesso ao
  schema `security` à API para contornar a falta de um serviço de identidade.
- Toda mudança de modelo EF Core deve incluir migração e snapshot. Não use EnsureCreated.
- Não exponha endpoints de participantes antes de implementar autenticação e autorização.

## Validação

- Execute `dotnet build` e `dotnet test tests/OpenLedg.Tests`.
- Para mudanças relacionais ou de infraestrutura, execute `./scripts/dev-up.sh` e
  `./scripts/test-integration.sh`, com Docker ativo. Não substitua PostgreSQL por EF InMemory.
- Registre limitações reais de validação; não declare testes executados se foram bloqueados.
- Documentação e comentários explicativos: português. Identificadores de código: inglês.

## Git

- Organize alterações em unidades pequenas por responsabilidade e revise `git diff --cached`.
- Mensagens curtas: `feat(domain): adiciona participantes`, `fix(db): corrige restrição`,
  `doc(readme): explica execução`, `chore(repo): configura solução`, `test(db): valida isolamento`.
- Use `doc()` no singular, conforme a convenção do projeto.
- Staging e commits devem ser organizados por escopo. Se precisar interromper trabalho,
  use stash com mensagem, por exemplo `git stash push -m 'feat(domain): participantes em andamento'`.
- Não crie stashes como arquivo permanente e não deixe a entrega escondida em stash.
- Não faça push nem remova trabalho de terceiros sem solicitação.
