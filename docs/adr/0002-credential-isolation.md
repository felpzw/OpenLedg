# ADR 0002 — Isolamento de credenciais

Status: aceito nesta fundação.

## Contexto

Perfis demográficos e material de autenticação têm necessidades de acesso diferentes.
Separar apenas classes ou tabelas não estabelece uma barreira de autorização.

## Decisão

Criar schemas `olgp`, `security` e `routing`. `openledg_owner` aplica migrações;
`openledg_app` recebe DML em perfis/roteamento e nenhum acesso a `security`.
Delegar senhas ao futuro IdP e armazenar referências de cofre para segredos MFA e
chaves privadas. Certificados e chaves ECDSA armazenados são públicos.

## Consequências

A API não pode consultar configurações de autenticação com sua credencial atual.
Um futuro serviço de identidade precisará de papel próprio com privilégios mínimos.
O owner local é a conta administrativa criada pela imagem PostgreSQL; não deve ser
usado por processos de negócio nem reproduzido como superusuário em produção.
Schemas não fornecem criptografia nem isolamento do administrador do banco.
