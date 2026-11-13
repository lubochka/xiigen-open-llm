# Phase 10.01 - Learnability/training and DPO creation intake/downgrade

CURRENT TASK: Phase 10.01 Learnability/training and DPO creation intake/downgrade. Build a human-readable matrix from existing evidence only. Separate DPO schema/guards from actual local model learnability/training proof. Use Russian status.

active_task_sentence_source: Luba current unquoted instruction: "CURRENT TASK: Phase 10.01 Learnability/training and DPO creation intake/downgrade. Build a human-readable matrix from existing evidence only."

NOT THE TASK: no training/evals/tests/builds/probes; no model/checkpoint/locator mutation; no training data creation beyond this matrix artifact; no prompts/messages; no learnability/training readiness claim; no stage/commit/push/switch.

EVIDENCE-ONLY: Phase 03 automated boundary report and packets, Phase 05/06/09 matrices, DPO/training/learning skills and guides, Trainable Architecture Audit, execution state/log. These inputs explain downgrade status only and do not open learnability, training, DPO quality, lifecycle, or host-learning claims.

## Фаза и шаг

Фаза 11 из 17: Learnability/training and DPO creation. Смысл фазы: отделить наличие DPO-схем, writer/runbook и quarantine rules от реального доказательства, что локальная модель обучилась, улучшилась, загрузилась свежей копией и может продолжать обучение.

Шаг 10.01: intake/downgrade. Смысл шага: собрать matrix rows из существующего evidence only, записать status, evidence links, missing evidence, next bounded step и overclaim-ban для каждой строки, не запуская training/eval/probe.

Статус: DONE_AS_MATRIX_INTAKE_ONLY. Learnability/training claim remains closed.

## Что именно разделено

- DPO row/schema existence vs actual DPO quality: существующие skills/guides задают поля DPO rows/triples, context package, chosen/rejected, teaching point, source/license, quarantine and held-out split. Это доказывает только наличие контракта. Actual DPO quality требует concrete accepted rows with positive examples, high-quality near-miss negative examples, source/license proof, leakage checks, train/eval split, and row-level provenance.
- DPO quality vs training run: даже accepted DPO rows не доказывают training. Training run должен иметь training_run_id, input corpus refs/hashes, before/after metrics, held-out evaluation, ablation/no-hidden-determinism report, checkpoint/manifest refs, and non-regression evidence.
- Training run vs fresh-load/continue improvement: even a training step is not enough. Fresh-load must reload the saved checkpoint into a fresh machine and reproduce behavior/metrics; continue-training must load an existing checkpoint, apply an approved batch, preserve/report metric continuity, and show improvement or honest no-improvement.
- Local user model two-leg lifecycle: common repo/global leg and user/adaptive personal leg are separate. Private/user data can stay in the adaptive side; global/common rows need compatible source/license or explicit consent. Backup/export/import/share/continue-training require real checkpoint files, manifest/readback, backup/rollback, fresh-load, and share/import boundaries. Existing docs describe the lifecycle, but accepted Phase 10 evidence has not executed it.

## Top counts and conclusion

| Count | Value |
|---|---:|
| total_matrix_rows | 11 |
| schema_or_guard_only_rows | 4 |
| missing_dpo_quality_rows | 2 |
| not_run_training_or_lifecycle_rows | 3 |
| runbook_only_lifecycle_rows | 1 |
| conclusion_rows | 1 |
| accepted_dpo_positive_example_rows | 0 |
| accepted_dpo_negative_example_rows | 0 |
| training_runs_executed | 0 |
| fresh_load_after_training_proven | 0 |
| continue_training_from_checkpoint_proven | 0 |
| host_triggered_local_learning_paths_proven | 0 |
| learnability_training_claim_opened | 0 |

Conclusion: `learnability_training_claim_opened = false`. Existing evidence supports schema/guard/runbook readiness only. It does not prove DPO quality, a training run, fresh-load improvement, continue-training improvement, user adaptive lifecycle, or host-triggered local learning from Claude/Cursor/Codex.

## Evidence base

- [Phase 03 Automated boundary report](phase-03-automated-boundary-report.md): DPO dataset loader/shuffle tests passed, but DPO train/eval quality was unrun; learnability-training rows have `NO_BOUNDARY_COMMAND_DISCOVERED`.
- [Phase 03 Automated boundary packets JSON](phase-03-automated-boundary-packets.json): machine-readable downgrade details for learnability-training, dpo-quality, asset-lifecycle, checkpoint, and locator rows.
- [DPO dataset command output](phase-03-command-output/05-dotnet-dpo-dataset.txt): 7 DPO dataset loader/shuffle tests passed in prior Phase 03 evidence; this step did not rerun them.
- [Blocker audit for excluded mutating probes](phase-03-command-output/07-blocker-audit-excluded-mutating-probes.txt): prior audit found candidate probes that train, save checkpoints, or update artifacts/state; this step did not run them.
- [Phase 05 product readiness matrix](phase-05-product-readiness-matrix.md): learnability/training and DPO creation stayed `NOT_READY`.
- [Phase 06 full extension readiness matrix](phase-06-full-extension-readiness-matrix.md): host readiness remains boundary-only/marker-only/blocker, not local adaptive learning proof.
- [Phase 09 semantic quality intake/downgrade](phase-09-semantic-quality-intake-downgrade.md): source-disjoint eval, ablation, checkpoint rewrite, and positive fresh-load/continue gates remain missing.
- [DPO triple format skill](../../../skills/dpo-triple-format-SKILL.md): complete context package, chosen/rejected, teaching point, curriculum tier, source/license, unit-learning and quarantine expectations.
- [Planning DPO authoring skill](../../../skills/planning-dpo-authoring-SKILL.md): planning signal types, GENERATED vs REINFORCEMENT, source curation, near-miss quality, and real-reference seed row requirement.
- [Design conversion DPO authoring skill](../../../skills/design-conversion-dpo-authoring-SKILL.md): context-vector completeness, visual verdict, correction, provenance/hash, quarantine, held-out split, and no-shortcut gates.
- [Client adaptive learning loop skill](../../../skills/client-adaptive-learning-loop-SKILL.md): capture, gap diagnosis, DPO candidate drafting, adaptive checkpoint update, before/after, fresh-load, backup/share/import requirements.
- [Learning signal capture skill](../../../skills/learning-signal-capture-SKILL.md), [learning path audit skill](../../../skills/learning-path-audit-SKILL.md), and [learning loop closure skill](../../../skills/learning-loop-closure-SKILL.md): signal validity and loop closure are required before claiming learning.
- [DCM DPO construction guide](../../../skills/dcm-dpo-construction-GUIDE.md) and [DCM training guide](../../../skills/dcm-training-GUIDE.md): two-leg Personal/Global architecture, source/license gates, 50 train + 50 held-out, backup, save/load, export/import, fresh-load, continue-training, and promotion boundaries.
- [Trainable Architecture Audit v4](../../TRAINABLE-ARCHITECTURE-AUDIT-v4.md): typed baseline authority improved, but semantic quality, checkpoint rewrite, and positive fresh-load/continue gates remain unclaimed.

External analysis note: `C:/Users/lubao/Documents/xiigen/sonnet local - analyzes/xiigen-file-analysis-state.json` was checked for existence but not read; the repo evidence above was sufficient for this intake/downgrade matrix, and the file is very large. This artifact does not rely on that external JSON.

## Matrix

| Row label | Status | Evidence links | Missing evidence | Next bounded step | Overclaim ban |
|---|---|---|---|---|---|
| DPO schema/writer availability | PROVEN_NARROW_SCHEMA_ONLY | [dpo-triple-format-SKILL.md](../../../skills/dpo-triple-format-SKILL.md); [planning-dpo-authoring-SKILL.md](../../../skills/planning-dpo-authoring-SKILL.md); [design-conversion-dpo-authoring-SKILL.md](../../../skills/design-conversion-dpo-authoring-SKILL.md); [phase-03 report](phase-03-automated-boundary-report.md) | Concrete accepted train-ready rows, source/license proofs, quality metrics, and model training evidence. | Future DPO corpus-intake packet may enumerate concrete accepted rows without training. | May claim DPO schema/guard availability only; may not claim actual DPO quality, DPO creation success, or training readiness. |
| DPO positive examples quality | MISSING_DPO_QUALITY_EVIDENCE | [dpo-triple-format-SKILL.md](../../../skills/dpo-triple-format-SKILL.md); [planning-dpo-authoring-SKILL.md](../../../skills/planning-dpo-authoring-SKILL.md); [phase-09 matrix](phase-09-semantic-quality-intake-downgrade.md) | No concrete accepted positive examples with source ref, license/private-leg route, chosen_ref, exact state fields updated, checkpoint written, and row-level provenance. | Build a future evidence-only inventory of existing candidate rows if they exist; do not synthesize training data in this step. | A schema example or family description is not a positive training example. |
| DPO negative examples quality | MISSING_DPO_QUALITY_EVIDENCE | [dcm-dpo-construction-GUIDE.md](../../../skills/dcm-dpo-construction-GUIDE.md); [design-conversion-dpo-authoring-SKILL.md](../../../skills/design-conversion-dpo-authoring-SKILL.md); [planning-dpo-authoring-SKILL.md](../../../skills/planning-dpo-authoring-SKILL.md) | No accepted high-quality near-miss rejected examples with taxonomy code, plausible wrong signal, one main decision boundary, permutation/leakage checks, and reviewer explanation. | Future DPO-quality audit of existing rows, if any, with quarantine/rewrite decisions. | Random garbage, generic refusal, vendor/private leakage, or checklist-only rejected examples cannot count as useful negatives. |
| Quarantine/rejection of bad/generic/vendor/private examples | SCHEMA_OR_GUARD_ONLY | [dpo-triple-format-SKILL.md](../../../skills/dpo-triple-format-SKILL.md); [planning-dpo-authoring-SKILL.md](../../../skills/planning-dpo-authoring-SKILL.md); [dcm-dpo-construction-GUIDE.md](../../../skills/dcm-dpo-construction-GUIDE.md) | No executed corpus audit showing row counts for quarantine, rewrite, keep-for-training, keep-for-eval, or reject. | Run only a future read-only corpus classification packet after an approved corpus path exists. | Quarantine policy is not the same as quarantine evidence over an actual dataset. |
| Source/license/universalization gate | SCHEMA_OR_GUARD_ONLY | [planning-dpo-authoring-SKILL.md](../../../skills/planning-dpo-authoring-SKILL.md); [dcm-dpo-construction-GUIDE.md](../../../skills/dcm-dpo-construction-GUIDE.md); [client-adaptive-learning-loop-SKILL.md](../../../skills/client-adaptive-learning-loop-SKILL.md) | No accepted source ledger proving common-vs-adaptive routing, source SHA/version, license/consent, raw_private_text_copied=false, and technical execution refs excluded for concrete rows. | Future source-ledger review before any common model training path. | Private user material, unknown-license source, GPL-family source, or vendor/private examples cannot enter common training by default. |
| Training run executed | NOT_RUN | [phase-03 report](phase-03-automated-boundary-report.md); [05-dotnet-dpo-dataset.txt](phase-03-command-output/05-dotnet-dpo-dataset.txt); [07-blocker audit](phase-03-command-output/07-blocker-audit-excluded-mutating-probes.txt); [dcm-training-GUIDE.md](../../../skills/dcm-training-GUIDE.md) | No training_run_id, no train/eval metrics, no held-out score, no ablation, no checkpoint produced by an authorized Phase 10 training run. | Separate future training work order only after DPO quality and authority gates pass. | DPO loader/shuffle tests and DpoTrainer unit tests are not an executed learnability/training proof for this phase. |
| Fresh-load after training | NOT_RUN | [phase-03 packets](phase-03-automated-boundary-packets.json); [phase-09 matrix](phase-09-semantic-quality-intake-downgrade.md); [dcm-training-GUIDE.md](../../../skills/dcm-training-GUIDE.md) | No trained checkpoint from Phase 10, no fresh machine reload, no reproduced before/after behavior or quality metrics after reload. | Future fresh-load verification only after a real checkpoint-producing training run exists. | Checkpoint readback/fresh-load guard from Phase 03 is not fresh-load improvement after Phase 10 training. |
| Continue-training from checkpoint | NOT_RUN | [phase-03 packets](phase-03-automated-boundary-packets.json); [phase-09 matrix](phase-09-semantic-quality-intake-downgrade.md); [client-adaptive-learning-loop-SKILL.md](../../../skills/client-adaptive-learning-loop-SKILL.md); [dcm-training-GUIDE.md](../../../skills/dcm-training-GUIDE.md) | No existing trained checkpoint loaded for continuation, no approved continuation batch, no continuity metrics, no post-continue behavior evidence. | Future continue-training probe after fresh-load and checkpoint ownership are proven. | Import/no-op continuation or save/load shape is not evidence of continue-training improvement. |
| User local adaptive model backup/export/import/share | RUNBOOK_ONLY_NOT_EXECUTED | [client-adaptive-learning-loop-SKILL.md](../../../skills/client-adaptive-learning-loop-SKILL.md); [dcm-training-GUIDE.md](../../../skills/dcm-training-GUIDE.md); [phase-03 report](phase-03-automated-boundary-report.md); [phase-05 matrix](phase-05-product-readiness-matrix.md) | No accepted backup, export, import, share, rollback, manifest/readback, user/adaptive checkpoint, or host-visible restored behavior evidence. | Phase 11 backup/export/import/share/continue-training/rollback intake/downgrade. | A runbook for Personal/Global legs is not local user model lifecycle proof. |
| Host-triggered local learning path from Claude/Cursor/Codex | NOT_RUN | [phase-06 matrix](phase-06-full-extension-readiness-matrix.md); [phase-03 report](phase-03-automated-boundary-report.md); [client-adaptive-learning-loop-SKILL.md](../../../skills/client-adaptive-learning-loop-SKILL.md) | No host capture, gap diagnosis, DPO candidate review, adaptive checkpoint update, fresh-load, export/import/backup/share, continue-training, or changed behavior shown through Claude/Cursor/Codex. | Future host-specific learning path proof only after safe host route and no-send/prompt constraints are authorized. | Do not claim host-triggered learning from boundary APIs, marker-only Cursor evidence, or blocked Claude/Codex routes. |
| Learnability conclusion | CLAIM_CLOSED | [phase-03 report](phase-03-automated-boundary-report.md); [phase-05 matrix](phase-05-product-readiness-matrix.md); [phase-09 matrix](phase-09-semantic-quality-intake-downgrade.md); [TRAINABLE-ARCHITECTURE-AUDIT-v4.md](../../TRAINABLE-ARCHITECTURE-AUDIT-v4.md) | Full chain is missing: DPO creation with accepted quality, training run, fresh-load improvement, continue-training improvement, hidden-shortcut audit, lifecycle backup/export/import/share, and host-visible changed behavior. | Next bounded step: Phase 11 backup/export/import/share/continue-training/rollback intake/downgrade, unless Luba asks for a Phase 10 repair. | Do not claim learnability/training readiness; evidence supports only schema/guard/runbook status and missing-proof downgrade. |

## Human-readable status

Сделано: создана 11-row matrix. Counts: total rows 11; schema/guard-only rows 4; missing DPO quality rows 2; not-run training/lifecycle rows 3; runbook-only lifecycle rows 1; conclusion rows 1; accepted DPO positive examples 0; accepted DPO negative examples 0; training runs executed 0; fresh-load after training proven 0; continue-training from checkpoint proven 0; host-triggered local learning paths proven 0; learnability/training claim opened 0.

В процессе: нет запущенных training/eval/test/build/probe/GUI/prompt/model/checkpoint/locator/git actions.

Осталось: phases done before this artifact: 10/17 through Phase 09. Current phase: 11/17 Phase 10. Remaining future plan phases after this matrix artifact: 6 phases, Phase 11 through Phase 16.

Review нашёл: existing evidence proves DPO schema/writer/quarantine/source-license contracts and prior DPO dataset loader/shuffle behavior only. It does not prove accepted positive/negative DPO quality, actual training, fresh-load improvement, continue-training improvement, user adaptive lifecycle, or host-triggered learning.

Что исправлено: Phase 10 now has a separate human-readable intake/downgrade artifact that prevents DPO-schema evidence from being upgraded into learnability/training readiness.

Следующее действие: Phase 11 backup/export/import/share/continue-training/rollback intake/downgrade as a future bounded work order, unless Luba requests a Phase 10 repair.

## Verification plan for this artifact

- JSON parse `phase-10-learnability-dpo-intake-downgrade.json`.
- Exact search Markdown/JSON for every required row label.
- Search unsupported learnability/training claims; allowed hits must be false/closed/missing/overclaim/non-claim context only.
- `git diff --check` for changed tracked files and no-index whitespace checks for untracked changed files if needed.

## Changed files intended by this step

- `docs/plans/evidence/extension-full-readiness-dual-path/phase-10-learnability-dpo-intake-downgrade.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-10-learnability-dpo-intake-downgrade.json`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md`
