# ADR 0001 — TPH e fronteira financeira

Status: aceito nesta fundação.

## Contexto

Os três tipos de participantes compartilham identidade, criação e conformidade.
A consulta polimórfica deve ser simples e o ITP não pode receber conta de liquidação.

## Decisão

Usar TPH no EF Core com discriminador explícito e CHECKs de formato por subtipo.
Separar coleções e credenciais em tabelas próprias, sem usar TPT para a herança.
Restringir vínculos por FK composta e CHECK, além das invariantes de domínio.
Guardar somente referências de contas TigerBeetle no PostgreSQL.

## Consequências

Consultas de participantes dispensam joins entre tabelas de herança. A tabela base
tem colunas nulas específicas de subtipo. A escolha não pressupõe que TPH será a
mais rápida para toda carga; medir consultas e índices antes de rever a estratégia.
Novos subtipos exigirão migração explícita dos CHECKs. Nenhum saldo relacional é permitido.
