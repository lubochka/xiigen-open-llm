# LEARN-05 DPO candidate dry-run validation

Work order: `LEARN-05-DPO-CANDIDATE-DRY-RUN-VALIDATION-01`.

## Результат

Dry-run прошел как schema-only / route-only проверка. LEARN-04C candidate schema, readiness matrix и route ledger согласованы с LEARN-01 source route matrix: обязательные группы полей описаны, raw external output остается private diagnostic/evaluation evidence-only, common route закрыт без MIT/Apache-2.0/approved permissive source/license review, user-local route требует private root/privacy/redaction/provenance/DPO authoring approval, quarantine route покрывает missing source/license, private leakage, stack-specific common proposal, reviewer prose as target, raw external output as target и source-disjoint overlap.

Это не создает DPO rows как training data и не доказывает learnability/readiness. Actual trainable DPO rows: `0`. Actual lifecycle-ready rows: `0/9`. Actual common candidates approved now: `0`. Actual user-local candidates approved now: `0`.

## Readiness counts

| Scope | rows | ready_for_user_local_candidate_schema | ready_for_common_candidate_schema | quarantined_if_unrepaired | missing_matrix_rows |
| --- | ---: | ---: | ---: | ---: | ---: |
| Total | 9 | 9 | 9 | 9 | 0 |
| Claude | 3 | 3 | 3 | 3 | 0 |
| Cursor | 3 | 3 | 3 | 3 | 0 |
| Codex | 3 | 3 | 3 | 3 | 0 |
| PromptToTaskMachine | 3 | 3 | 3 | 3 | 0 |
| OrchestrationReasoningMachine | 3 | 3 | 3 | 3 | 0 |
| SkillSelectionMachine | 3 | 3 | 3 | 3 | 0 |

`ready_for_*_candidate_schema` означает только наличие схемы и маршрута для будущего authoring packet. Это не approval, не train-ready status и не common readiness. Все 9 строк имеют quarantine route if unrepaired, потому что concrete chosen/rejected refs, source/license/privacy/redaction/provenance refs, dataset manifest/hash, lifecycle roots, checkpoint shas и operation authority отсутствуют.

## Blocked before actual DPO rows

- Нет concrete `dpo_candidate_id`, `chosen_ref`, `rejected_ref`, `positive_example_ref`, `negative_example_ref`.
- Нет `source_ref`, `doc_id`, `source_sha256`, `source_license_ref`, `source_license_hash`.
- Нет source/license/privacy/redaction/provenance review refs и DPO authoring approval.
- Нет DPO acceptance matrix, training dataset manifest/hash, user-local root, import/restore root, target/rollback checkpoint shas, held-out/ablation refs и operation authority.
- Raw Claude/Cursor/Codex output нельзя переносить в chosen/rejected/source/synthetic/common material.

## Следующий безопасный work order

Так как dry-run passes as schema-only, но actual DPO rows отсутствуют, следующий безопасный work order: docs/QA prep или operation-specific DPO candidate authoring packet prep, без training. Нужен packet, который авторит concrete candidate refs и approvals, но все еще не запускает training/continue-training/checkpoint/export/import/restore/fresh-load/runtime/eval/external AI/model calls.

## Locks

Claim locks остаются false: DPO rows as training data не созданы, training/continue-training/checkpoint mutation/export/import/restore/fresh-load/runtime/eval/external AI/model calls не выполнялись, learnability/readiness/common readiness/final/product readiness не заявлялись. Git add/commit/push не выполнялись; parent commits after review.
