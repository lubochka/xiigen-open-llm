# LEARN-06 operation-specific DPO candidate authoring packet prep

Work order: `LEARN-06-OPERATION-SPECIFIC-DPO-CANDIDATE-AUTHORING-PACKET-PREP-01`.

## Итог

Создан prep-only пакет для 9 строк Claude/Cursor/Codex x PromptToTaskMachine/OrchestrationReasoningMachine/SkillSelectionMachine. Это не DPO dataset, не trainable rows и не approval на обучение. `chosen_ref` и `rejected_ref` намеренно оставлены незаполненными: existing evidence даёт только route/schema refs, blocked-row refs и QUALITY-03 handler-level evidence-only refs.

## Что безопасно предзаполнено

- LEARN-04C schema, readiness matrix и route/quarantine ledger refs.
- LEARN-05 dry-run validation refs и per-row blocked refs.
- QUALITY-03 row-level refs для текущих handler-level baseline/adaptive пар: всего 54 pair rows, 108 redacted trace refs и 108 raw-private trace refs как evidence-only. Raw/private refs нельзя копировать в chosen/rejected/source/common material.
- QUALITY-03 gaps перенесены как блокеры: `QUALITY_RUBRIC_MISSING_OR_STALE`, `QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE`, `SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE`.

## Что не заполнено

Для всех 9 строк отсутствуют concrete `chosen_ref`, `rejected_ref`, source/license/privacy/redaction/provenance refs, authoring authority, reviewer notes, DPO acceptance matrix, dataset manifest/hash, held-out/source-disjoint refs, ablation/no-memorization refs и operation-specific training authority. Поэтому trainable rows сейчас: 0/9.

## Маршруты

- `user-local`: возможен только после DPO authoring approval, privacy/source/license/redaction/provenance review и user-owned storage boundary.
- `common`: условный и закрыт сейчас; нужен universal source-grounded lesson из MIT/Apache-2.0 или approved equivalent permissive/open source плюс material-quality review.
- `quarantine`: обязателен, если missing refs не восстановлены, raw external output попадает в trainable fields, private/project/host-specific материал предлагается как common или source-disjoint split пересекается с training material.

## Следующий безопасный work order

Рекомендация: `DOCS-QA-STATUS-AND-EVIDENCE-SCOPE-PREP-01` перед actual authoring, потому что он может подготовить human-readable QA/status/evidence scope without training. Альтернативно можно идти в `LEARN-07-DPO-AUTHORING-DRY-RUN-VALIDATION-01`, если цель следующего шага - проверить этот packet как dry-run, всё ещё без training и без создания actual DPO rows.

## Locks

No source/code/test/plan/governance/skill edits. No git add/commit/push. No actual DPO row creation as training data. No training/continue-training/checkpoint/export/import/restore/fresh-load. No runtime/eval/external AI/model calls. No learnability/readiness claim.
