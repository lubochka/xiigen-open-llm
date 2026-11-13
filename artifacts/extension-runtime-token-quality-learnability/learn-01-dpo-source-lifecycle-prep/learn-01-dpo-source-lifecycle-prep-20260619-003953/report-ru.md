# LEARN-01 DPO/source/lifecycle prep

## Результат

Ветка: `codex/chat-gpt-5.5`.

Создан prep-пакет для `LEARN-01-DPO-SOURCE-LIFECYCLE-PREP-01` без DPO training rows, без training/continue-training, без checkpoint mutation, без runtime host execution, без external AI/model calls, без eval/scoring и без learnability/readiness claim.

QUALITY-03 gap repair учтен как текущий quality input state: 0/9 complete pairs, 0 rows ready for actual scoring, actual eval/scoring closed. Поэтому этот пакет только готовит source/lifecycle маршруты для будущих bounded work orders.

## Артефакты

- `dpo-source-route-matrix.json` - схема будущей DPO row, allowed source classes, raw external AI evidence-only boundary, Luba/user correction route, universal-vs-stack-specific route, common/user-local split и quarantine rules.
- `trainable-sphere-lifecycle-matrix.json` - 9 строк Claude/Cursor/Codex x PromptToTaskMachine/OrchestrationReasoningMachine/SkillSelectionMachine с common leg, user-local leg и будущими требованиями backup/export/import/restore/load-without-retrain/continue-training/fresh-load/held-out/ablation/rollback.
- `lifecycle-prep-checklist.json` - что подготовлено сейчас, что остается закрытым, и какой следующий work order безопасен без training.
- `checks-report.json` - verification ledger.

## Всего / Сделано / Сейчас / Осталось

Всего: нужно подготовить безопасный DPO/source/lifecycle prep-контракт для user-local learning: будущая row schema, source routes, common/user split, two-leg lifecycle для Claude/Cursor/Codex x PromptToTask/OrchestrationReasoning/SkillSelection, и next work order без training/eval/checkpoint mutation.

Сделано: подготовлены DPO/source route matrix, trainable sphere lifecycle matrix, lifecycle prep checklist и checks report. В матрицах зафиксировано, что raw Claude/Cursor/Codex/other external AI output является private diagnostic/evaluation evidence only по умолчанию; reviewer/evaluator/adjudication prose тоже evidence-only до DPO-authoring gate; user/Luba corrections идут user-local first; common training допускает только universal permissive/open-source material после source/license/material review.

Сейчас: LEARN-01 остается prep-only. DPO rows as training data не созданы. Backup/export/import/restore/load-without-retrain/continue-training/fresh-load/held-out/ablation/rollback не выполнялись. Common readiness, training readiness, learnability, target promotion, locator update и final/product readiness не заявлены.

Осталось: следующий безопасный work order - `LEARN-02-USER-LOCAL-ADAPTIVE-STORE-MANIFEST-PREP-01` с source/license/redaction review prep. Любая trainable DPO row creation, training/continue-training, fresh-load, held-out/source-disjoint eval, ablation/no-memorization, checkpoint mutation, rollback, promotion или locator update требует отдельный operation-specific target/rollback/source packet.

## Что можно делать дальше без training

- Подготовить user-local adaptive store manifest schema.
- Подготовить source/license/redaction review packet.
- Подготовить common-training source classification packet.
- Подготовить redacted share packet schema.
- Подготовить operation-specific approval packet template для будущего `LEARN-03`.
- Подготовить host/machine coverage ledger.

## Что требует отдельный operation-specific packet

- Создание trainable DPO rows.
- Training или continue-training.
- Checkpoint/locator mutation.
- Fresh-load, held-out/source-disjoint eval, ablation/no-memorization.
- Rollback, promotion, final readiness.
