# LEARN-02 user-local adaptive store manifest prep

## Результат

Ветка: `codex/chat-gpt-5.5`.

Создан prep-пакет для `LEARN-02-USER-LOCAL-ADAPTIVE-STORE-MANIFEST-PREP-01`: схема/требования manifest для user-local adaptive store, host×machine coverage matrix, manifest gap ledger и checks report. Работа осталась строго schema/prep-only: DPO rows как training data не создавались, training/continue-training/checkpoint/export/import/share/restore/fresh-load/rollback не выполнялись, runtime/eval/external AI/model calls не запускались, readiness/learnability claims не делались.

## Что подготовлено

- `user-local-adaptive-store-manifest-schema.json` фиксирует обязательные roots, privacy classes, host coverage, machines, common-vs-user legs, checkpoint refs, DPO refs и lifecycle refs: backup/export/import/share/restore/load-without-retrain/continue-training/fresh-load/held-out/ablation/rollback.
- `host-machine-coverage-matrix.json` содержит 9 строк Claude/Cursor/Codex × PromptToTaskMachine/OrchestrationReasoningMachine/SkillSelectionMachine. Для каждой строки есть repo/common context refs и честный статус: actual user-local execution missing, no claim.
- `manifest-gap-ledger.json` разделяет, что можно заполнить из существующих LEARN-01/repo-common/scoped lifecycle artifacts, и что остается missing для actual user-local execution.
- `checks-report.json` содержит verification ledger и false claim locks.

## Что можно заполнить из существующего

Из LEARN-01 можно взять source route matrix, raw external AI evidence-only boundary, common/user split и 9 lifecycle requirement rows. Из `adaptive-learnability-01` можно взять machine ids, domain tags, checkpoint ref names и repo-common/user-cache/user-cache-continued/backup/share-import/rollback manifest path candidates. Из старых `backup-export-import-share`, `continue-training`, `fresh-load`, `rollback` reports можно взять форму ref/sha/path fields и limitation labels. Все это context или schema prefill, не actual execution proof.

## Что остается отсутствующим

Нет concrete approved user-local root для нового actual packet, нет row-specific trainable DPO refs, нет full per-host×machine backup/export/import/share/restore/load-without-retrain/continue-training/fresh-load/held-out/ablation/rollback chain, нет operation-specific target/rollback binding, нет source/license/redaction review for share/common route, и PromptToTask/SkillSelection остаются исторически overlay-only/partial в старых lifecycle proofs.

## Следующий безопасный work order

Следующий safe step без training: `LEARN-03-PORTABILITY-BACKUP-IMPORT-PLAN-PREP-01` — подготовить portability backup/import/share/restore/load-without-retrain plan и approval/target packet refs. Он не должен создавать DPO rows, не должен тренировать, не должен mutate checkpoint/export/import/share/restore и не должен claim learnability/readiness.

## Всего / Сделано / Сейчас / Осталось

Всего: нужен manifest contract для user-local adaptive store, который связывает repo/common leg и private user-local leg без смешивания private/project/vendor material с common assets.

Сделано: подготовлены schema, coverage matrix, gap ledger и checks report для LEARN-02. Host×machine coverage заполнен на уровне требований и refs-кандидатов: 9/9 rows represented.

Сейчас: это prep-only пакет. Actual user-local lifecycle execution rows: 0/9. Trainable DPO rows: 0. Training/continue-training/fresh-load/held-out/ablation/rollback: не выполнялись. Claim locks false.

Осталось: parent review, затем `LEARN-03-PORTABILITY-BACKUP-IMPORT-PLAN-PREP-01` или отдельный approval/target packet prep. Любая actual mutation/execution требует отдельного bounded work order.
