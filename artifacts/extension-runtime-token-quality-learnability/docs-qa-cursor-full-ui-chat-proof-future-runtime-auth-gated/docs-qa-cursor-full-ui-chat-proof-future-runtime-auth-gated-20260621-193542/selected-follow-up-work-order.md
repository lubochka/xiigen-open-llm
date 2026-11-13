# Selected follow-up work order

Selected: `DOCS-QA-CODEX-FULL-UI-CHAT-PROOF-FUTURE-RUNTIME-AUTH-GATED-01`

## Почему это следующий безопасный шаг

Cursor future runtime-auth-gated packet не может и не должен авторизовать runtime. Поэтому выбран следующий безопасный bounded docs-QA/support step из того же full host UI/chat proof family: Codex future runtime-auth-gated packet. Все auth flags остаются false.

## Authority locks

- implementation_authorized_now=false
- runtime_launch_authorized=false
- scoring_authorized=false
- training_authorized=false
- raw_private_reads_authorized=false
- external_ai_or_web_or_model_calls_authorized=false
- proof_eval_scoring_runner_authorized=false

## Evidence locks

Support-only helper/adapter/core evidence нельзя засчитывать как full Codex desktop/app/chat proof. Будущий Codex proof также должен требовать видимый host surface, same-run debug/core refs, redacted input/output refs, RequestedSkills/AvailableSkills binding, negative/error path и rollback/no-side-effect statement.

## Git boundary

Sub-agent git_authority=forbidden, allowed_git_commands=NONE. Parent will perform commit/push after review.