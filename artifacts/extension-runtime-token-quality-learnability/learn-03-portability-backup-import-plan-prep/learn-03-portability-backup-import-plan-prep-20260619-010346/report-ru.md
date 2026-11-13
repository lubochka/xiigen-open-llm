# LEARN-03 portability/backup/import plan prep

## Результат

Ветка: `codex/chat-gpt-5.5`.

Создан prep-пакет для `LEARN-03-PORTABILITY-BACKUP-IMPORT-PLAN-PREP-01`. Пакет описывает, какие файлы, checksums, manifests, rollback refs, privacy/redaction refs и host/machine rows нужны для будущего user-local adaptive lifecycle: backup, export, import/share, restore, load-without-retrain и continue-training. Никаких actual backup/export/import/share/restore/load-without-retrain/checkpoint/training/DPO/runtime/eval действий не выполнялось.

## Что подготовлено

- `portability-backup-import-plan.json` - статический план lifecycle-пакета: обязательные manifest files, checksum fields, bundle refs, redacted share refs, same-machine restore и another-root restore.
- `lifecycle-execution-prerequisites.json` - точный список missing prerequisites, которые должны быть закрыты до actual lifecycle execution.
- `rollback-target-binding-needed.json` - почему следующим безопасным шагом должен быть target/rollback binding prep, и какие refs нужны до любой мутации.
- `checks-report.json` - ledger разрешённых проверок и запретов для этого prep-пакета.

## Важно

Этот пакет доказывает только готовность контракта для будущего lifecycle execution. Он не доказывает, что user-local model был backed up, exported, imported, restored, loaded, continued-trained или rolled back. Он не доказывает learnability, readiness, token saving, quality improvement, DPO trainability, target promotion или locator update.

## Чего не хватает перед actual lifecycle execution

- Нет concrete approved user-local root и отдельного target import/restore root.
- Нет operation-specific work order, который разрешает actual backup/export/import/share/restore/load-without-retrain/continue-training.
- Нет row-specific trainable DPO refs для Claude/Cursor/Codex x PromptToTaskMachine/OrchestrationReasoningMachine/SkillSelectionMachine.
- Нет source/license/redaction review refs для share/common route.
- Нет base checkpoint sha, imported checkpoint sha, target checkpoint sha и rollback checkpoint sha, связанных в одном target packet.
- Нет dataset shas для training и held-out/source-disjoint checks.
- Нет rollback proof, no-memorization/ablation proof и fresh-load proof.

## Следующий безопасный work order

`LEARN-04-USER-LOCAL-ROOT-AND-TARGET-ROLLBACK-BINDING-PREP-01`.

Он должен оставаться prep-only: выбрать/описать approved user-local root, another-root restore target, base checkpoint refs, target candidate refs, rollback refs, locator/store pointer refs и deferred decision queue без mutation, training, runtime/eval, external AI/model calls или readiness claim.

## Всего / Сделано / Сейчас / Осталось

Всего: нужен переносимый user-local adaptive lifecycle contract для Claude/Cursor/Codex x PromptToTaskMachine/OrchestrationReasoningMachine/SkillSelectionMachine, который заранее связывает manifest files, checksums, privacy/redaction, backup/export/import/share/restore/load-without-retrain/continue-training и rollback.

Сделано: подготовлен LEARN-03 prep-пакет: `report-ru.md`, `portability-backup-import-plan.json`, `lifecycle-execution-prerequisites.json`, `rollback-target-binding-needed.json`, `checks-report.json`.

Сейчас: пакет остаётся plan/prep-only. Actual lifecycle execution rows: 0/9. DPO rows as training data: 0. Training/continue-training/checkpoint/export/import/share/restore/load-without-retrain/fresh-load/held-out/ablation/rollback не выполнялись. Claim locks false.

Осталось: следующий безопасный шаг - `LEARN-04-USER-LOCAL-ROOT-AND-TARGET-ROLLBACK-BINDING-PREP-01`; любая actual lifecycle mutation/execution требует отдельный bounded work order с exact target/rollback/source packet.
