# Cursor future runtime-auth-gated packet

Дата: 2026-06-21T19:35:42+03:00
Work order: DOCS-QA-CURSOR-FULL-UI-CHAT-PROOF-FUTURE-RUNTIME-AUTH-GATED-01
Sub-agent: bounded docs-QA/support writer
Branch по входным артефактам: codex/chat-gpt-5.5

## Короткий статус

Clean full Cursor chat participant/panel UI proof сейчас не выполнялся и этим packet не доказан. Cursor runtime не запускался, scoring/eval/proof runners не запускались, training/DPO/checkpoint/model/locator/target mutation не выполнялась, raw-private reads не выполнялись, внешние AI/web/model calls не выполнялись.

Этот packet является support-only будущим proof contract. Он фиксирует, какие доказательства понадобятся позже, и закрывает возможность засчитать Extension Host/helper/handler/support evidence как clean full Cursor chat participant/panel UI proof.

## Runtime gate

Запуск Cursor runtime может быть разрешён только свежей текущей явной командой Luba именно на Cursor full UI/chat proof. Старые approve/continue/state next_action, bundle review, support/defer packet или этот packet не дают права запускать runtime.

- runtime_launch_authorized=false
- scoring_authorized=false
- training_authorized=false
- raw_private_reads_authorized=false
- external_ai_or_web_or_model_calls_authorized=false
- proof_eval_scoring_runner_authorized=false

## Что понадобится для будущего clean proof

- Видимый Cursor Extension Host плюс видимый Cursor chat participant или panel UI evidence из одного запуска.
- Same-run debug/hook/helper/core refs с единым correlation id и ссылкой на llm_mvp_core или XiigenExtensionApiService boundary.
- Redacted input/output refs для трёх сценариев: prompt-to-task, skills binding, negative missing target.
- RequestedSkills/AvailableSkills binding: зафиксировать present/absent и evidence refs; отсутствие допустимо только с объяснением.
- Negative/error path в том же Cursor host surface.
- Rollback/no-side-effect statement: нет training, DPO, checkpoint, model, locator, target или code mutation.
- Process/network isolation evidence, если будущая текущая авторизация разрешит runtime launch.

## Граница evidence

No manual substitution: helper stdout, dashboard, direct-handler evidence, Extension Host support evidence, handler-level chat adapter evidence и safe-launch prep остаются support-only. Они могут помогать объяснить boundary, но не заменяют видимый Cursor chat participant/panel UI proof.

## Claim locks

Этот packet не заявляет token savings, quality improvement, semantic quality, learnability, product readiness или final readiness. Эти темы остаются закрытыми до отдельных разрешённых measured/scoring/training/readiness phases.

## Human board

- total=63
- pass=9
- missing=45
- deferred=9
- approvals=0/9
- runner_ready=0/9
- quality_pass=0/9
- done=false
- host_readiness_claimed=false
- operator_input_required=false

## Следующий безопасный шаг

Selected follow-up: DOCS-QA-CODEX-FULL-UI-CHAT-PROOF-FUTURE-RUNTIME-AUTH-GATED-01. Это тоже safe docs-QA/support step с auth flags=false; оно не разрешает runtime, scoring, training, raw-private reads, external calls или proof/eval/scoring runners.

## Git boundary

Sub-agent git_authority=forbidden, allowed_git_commands=NONE. Parent will perform commit/push after review.