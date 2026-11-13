# Selected follow-up work order

Selected: `DOCS-QA-CLAUDE-FULL-UI-CHAT-PROOF-FUTURE-RUNTIME-AUTH-GATED-01`

## Почему это следующий безопасный шаг

Review bundle прошел без repair findings. Подготовленный bundle уже содержит future work order для Claude, Cursor и Codex. Следующим безопасным bounded step выбран первый host-specific future proof packet для Claude.

Этот selection не разрешает runtime. Claude runtime launch, scoring, training, raw-private reads, external AI/web/model calls и proof/eval/scoring runners остаются forbidden, пока Luba не даст свежую явную текущую host-specific авторизацию именно на Claude full UI/chat proof.

## Authority locks

- implementation_authorized_now=false
- runtime_launch_authorized=false
- scoring_authorized=false
- training_authorized=false
- raw_private_reads_authorized=false
- external_ai_or_web_or_model_calls_authorized=false
- proof_eval_scoring_runner_authorized=false

## Evidence locks

Support-only helper/hook/adapter/core evidence нельзя засчитывать как full Claude UI/chat proof. Будущий proof должен содержать visible Claude host surface evidence, same-run debug/core refs, redacted input/output refs, RequestedSkills/AvailableSkills binding и negative/error path без mutation.

## Git boundary

Sub-agent git_authority=forbidden, allowed_git_commands=NONE. Parent will perform commit/push after review.
