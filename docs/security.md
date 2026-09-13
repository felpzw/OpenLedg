# Segurança da fundação

A API ainda não oferece autenticação/autorização. Por isso só publica identificação
do serviço e endpoints de saúde, sem leitura ou escrita de dados de participantes.
Não habilitar operações de negócio sem implementar controle de acesso.

Credenciais demográficas e de autenticação ficam em schemas com privilégios distintos.
O usuário de runtime não acessa `security` nem cria tabelas. Uma futura identidade
de serviço deve receber permissões específicas, sem reutilizar o owner.

Senhas de utilizadores são responsabilidade do futuro IdP. Seeds TOTP e chaves
privadas são referências a um cofre, ainda não conectado. O campo WebAuthn identifica
uma credencial; não armazena biometria nem verifica uma asserção. Certificados mTLS
são analisados como certificados públicos, mas cadeia de confiança, revogação,
rotação e autenticação mTLS ainda não estão implementadas. DPoP contém configuração,
sem verificação de provas, nonce ou replay.

Webhooks exigem HTTPS e chave pública ECDSA válida. Antes de implementar envio, incluir
proteção SSRF com política de destinos e validação após resolução DNS, além de timeout,
limites de tamanho, assinatura e reenvio idempotente. A chave cadastrada verifica retornos
da empresa; a assinatura de eventos de saída precisará de chave própria em cofre.

Não registrar documentos, chaves PIX ou segredos em logs. A aplicação não habilita
`EnableSensitiveDataLogging`. Toda conexão/credencial local está em `.env`, ignorado
pelo Git. Os volumes contêm dados persistentes; usar somente dados sintéticos no desenvolvimento.

O Compose é local: portas em loopback, HTTP na API, TLS do PostgreSQL não configurado,
TigerBeetle com uma réplica development e `seccomp=unconfined`/`IPC_LOCK` para io_uring.
Produção requer provisionamento próprio, TLS, secret manager, backup/restore testados,
auditoria e revisão dos acessos. Estes mecanismos não estão implementados nesta sprint.
