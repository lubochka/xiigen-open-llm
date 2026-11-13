# LEARN-04C source/license/DPO candidate prep

Work order: `LEARN-04C-SOURCE-LICENSE-DPO-CANDIDATE-PREP-01`.

Создан prep-пакет для source/license/DPO candidate route без materialization training rows. Пакет описывает поля будущей candidate строки, source classes, MIT/Apache-2.0/approved permissive requirements, raw external output evidence-only rule, user-local/private route, common route и quarantine route. Никаких source/code/test/plan/governance/skill edits, git add/commit/push, DPO rows as training data, training/continue-training/checkpoint/export/import/restore/fresh-load, runtime/eval/external AI/model calls или learnability/readiness claim не выполнялось.

## Что создано

- `source-license-dpo-candidate-schema.json` - schema/prep contract для будущих candidate refs. Все route decisions остаются `candidate_or_quarantine_decision_only`; `created_as_training_data_now=false`.
- `dpo-candidate-readiness-matrix.json` - 9 строк Claude/Cursor/Codex x PromptToTaskMachine/OrchestrationReasoningMachine/SkillSelectionMachine. Для всех строк есть current prep material из LEARN-01/04B, но отсутствуют concrete chosen/rejected refs, source/license approval refs, DPO acceptance matrix, dataset manifest/hash и operation authority.
- `quarantine-and-route-ledger.json` - user-local/private, common permissive и quarantine route rules. Raw external Claude/Cursor/Codex output remains private diagnostic/evaluation evidence only by default.
- `checks-report.json` - итоговый verification packet после JSON parse, `git diff --check`, scope guard и claim-lock scan.

## Current candidate readiness

Всего строк: 9/9. Для каждой строки current material exists только как static prep evidence: LEARN-01 source route matrix, LEARN-01 trainable sphere lifecycle row, LEARN-04B blocked lifecycle items and dry-run validation result. Это может стать user-local candidate только после transformed DPO authoring approval with provenance/license/privacy/redaction fields. Common candidate route условный и закрыт сейчас: нужен universal source-grounded lesson from MIT, Apache-2.0, or approved equivalent permissive/open source with source/license review, source hash, redaction and material-quality PASS. Raw external output, host mechanics, exact repo paths, package/cache paths, branch/locator recipes, private Luba corrections and project-specific workflow details route to evidence-only, adaptive-only/user-local, or quarantine.

## Missing before any training/lifecycle execution

- concrete `dpo_candidate_id` and row-specific machine target binding;
- `chosen_ref` / `rejected_ref` created from human/XIIGen-authored or licensed source-grounded material, not raw external output;
- `source_ref`, `doc_id`, `source_sha256`, `source_license_ref`, `source_license_hash`;
- source/license/privacy/redaction/provenance review refs;
- provider terms approval if external output is ever proposed beyond private evidence-only;
- DPO acceptance matrix ref and training dataset manifest/hash;
- approved user-local root, distinct import/restore root, target/rollback checkpoint shas, store/locator refs, held-out/ablation refs and operation-specific authority.

## Следующий безопасный work order

`LEARN-05-DPO-CANDIDATE-DRY-RUN-VALIDATION-01`.

Scope: validate this candidate schema and readiness matrix only. It may parse the new JSON, check 9/9 matrix rows, check that raw external output is evidence-only, check route decisions and claim locks, and produce a validation report/state update.

Still forbidden: actual DPO row creation as training data, training/continue-training, checkpoint/export/import/restore/fresh-load, runtime/eval/external AI/model calls, user/model root probe, common training approval, learnability/readiness claim, and git add/commit/push by sub-agent.

## Claim locks

All claim locks remain false: no trainable DPO rows, no training, no checkpoint mutation, no backup/export/import/share/restore/load-without-retrain execution, no fresh-load, no held-out/ablation/rollback, no runtime/eval/model calls, no common readiness, no target promotion, no learnability, no final/product readiness.
