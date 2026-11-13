# Phase 09.01 - Semantic quality improvement intake/downgrade

CURRENT TASK: Phase 09.01 Semantic quality improvement eval/downgrade. Build a human-readable matrix from existing evidence only for PromptToTask, OrchestrationReasoning, and SkillSelection. Separate narrow tests/baselines from semantic quality improvement proof. Use Russian status.

active_task_sentence_source: Luba current unquoted instruction: "CURRENT TASK: Phase 09.01 Semantic quality improvement eval/downgrade. Build a human-readable matrix from existing evidence only for PromptToTask, OrchestrationReasoning, and SkillSelection."

NOT THE TASK: no new evals/tests/builds/training/probes, no GUI launch, no prompts/messages, no model/training/checkpoint/locator mutation, no code edit, no semantic quality improvement claim, no stage/commit/push/switch.

EVIDENCE-ONLY: Phase 03 automated boundary report and old command outputs, Phase 05/06 readiness matrices, Trainable Architecture Audit, semantic-quality plan/state files, orchestration/skill-selection state files, and the external analysis JSON path as existing analysis evidence only. They can explain status, but they do not authorize new execution.

## Фаза и шаг

Фаза 10 из 17: Semantic quality improvement eval/downgrade. Смысл фазы: проверить, есть ли уже доказательства реального улучшения semantic quality для PromptToTask, OrchestrationReasoning и SkillSelection, и если полного доказательства нет, честно оставить claim закрытым.

Шаг 09.01: intake/downgrade. Смысл шага: собрать matrix rows из существующего evidence, отделить узкие API/tests/baselines от доказательства semantic quality improvement, записать missing evidence, next bounded steps и overclaim-ban.

Статус: DONE_AS_MATRIX_INTAKE_ONLY. Semantic quality improvement claim remains closed.

## Что требовалось бы для semantic quality improvement

- Source-disjoint eval: held-out rows должны быть отделены по источникам от train/dev/candidate material, с явными leakage checks и результатами по каждому слою: PromptToTask, OrchestrationReasoning, SkillSelection.
- Ablations: нужен разбор против скрытых deterministic shortcuts, включая exact/regex/keyword overlap для PromptToTask, BM25/filename/title/static route для OrchestrationReasoning, candidate-order/position/requested-skill shortcut для SkillSelection.
- Checkpoint rewrite: должен существовать переписанный checkpoint или эквивалентный обученный артефакт, который заменяет baseline/nearest-overlap поведение и имеет traceable dataset/source ledger.
- Positive fresh-load/continue-training gates: после reload/import и после continue/no-op continuation должны сохраняться те же quality свойства, а не только формат, импортируемость или hash lifecycle.
- No hidden deterministic shortcut: audit должен показать, что улучшение не объясняется row id, scenario pair hash, order of candidates, filenames, titles, keyword matching, test fixture leakage или hard-coded route.
- All-three-components gate: claim нельзя открыть, если хотя бы один из PromptToTask, OrchestrationReasoning, SkillSelection имеет только guard/baseline/lifecycle evidence или missing proof row.

## Что существующее evidence доказывает узко

- Phase 03 old command outputs доказывают только guard/API/scoring behavior: система умеет держать semantic claim false без positive evidence и не должна пропускать claim при missing layers или forbidden failures. Эти команды не запускались в этом шаге.
- Trainable Architecture Audit и Q9 state показывают, что PromptToTask и OrchestrationReasoning переведены на typed `ILogicalUnit` baseline authority и что API firewall требует positive evidence от всех трех компонентов.
- Orchestration/Prompt/SkillSelection state records показывают hierarchy/adapter parity, asset lifecycle, import/no-op continuation и portability/adaptive coverage как узкие lifecycle/structure доказательства.
- Completion/uplift plans и state records прямо фиксируют: training/eval/ablation/checkpoint rewrite/fresh-load/continue semantic gates не закрыты как full semantic quality proof.

## Почему узкие API/tests/baselines не являются proof

API contracts, unit tests, quality scoring guards, baseline logical units, asset import, no-op continuation и dashboard/report-only evidence могут доказать, что claim заблокирован, что route/adapter работает, или что артефакт загружается. Они не доказывают улучшение semantic quality, потому что не сравнивают обученное поведение с untrained/baseline на source-disjoint held-out rows, не показывают positive ablation deltas, не доказывают checkpoint rewrite, и не исключают deterministic shortcut как причину результата.

## Top counts and conclusion

| Count | Value |
|---|---:|
| total_matrix_rows | 14 |
| narrow_current_evidence_rows | 3 |
| missing_source_disjoint_eval_rows | 3 |
| missing_ablation_rows | 3 |
| missing_fresh_load_continue_quality_rows | 2 |
| narrow_lifecycle_only_rows | 1 |
| partial_hidden_shortcut_guard_rows | 1 |
| conclusion_rows | 1 |
| source_disjoint_eval_proven_rows | 0 |
| ablation_proven_rows | 0 |
| positive_fresh_load_continue_quality_gate_rows | 0 |
| full_hidden_deterministic_shortcut_audit_accepted | 0 |
| semantic_quality_claim_opened | 0 |

Conclusion: semantic_quality_claim_opened = false. Existing evidence supports narrow guard/baseline/lifecycle status only; it does not prove semantic quality improvement.

## Phase 09.01F untracked-artifact evidence note

Evidence packet: [phase-09-semantic-quality-next-action-fix-evidence.md](phase-09-semantic-quality-next-action-fix-evidence.md).

The Phase 09.01 semantic artifacts are untracked new artifacts in this checkout, so git baseline cannot prove a wording-only change. Rereview acceptance for the next-action repair must use current-state invariants instead: row_count 14, `semantic_quality_claim_opened = false`, top_counts total 14, top_counts claim opened 0, the exact next action `Phase 10 learnability/training and DPO creation intake/downgrade`, and no unsupported semantic quality claim.

## Matrix

| Row label | Status | Evidence links | Missing evidence | Next bounded step | Overclaim ban |
|---|---|---|---|---|---|
| PromptToTask current evidence | PROVEN_NARROW | [phase-03-automated-boundary-report.md](phase-03-automated-boundary-report.md); [04-dotnet-quality-scoring.txt](phase-03-command-output/04-dotnet-quality-scoring.txt); [02-dotnet-api-contracts.txt](phase-03-command-output/02-dotnet-api-contracts.txt); [TRAINABLE-ARCHITECTURE-AUDIT-v4.md](../../TRAINABLE-ARCHITECTURE-AUDIT-v4.md); [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json) | No source-disjoint quality result, no ablation delta, no checkpoint rewrite, no positive fresh-load/continue semantic gate. | Prepare a future bounded eval packet that uses only pre-approved existing datasets/manifests; do not run it in this step. | May claim typed baseline/API guard evidence only; may not claim PromptToTask semantic quality improvement. |
| PromptToTask source-disjoint eval | MISSING_SOURCE_DISJOINT_EVAL | [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md); [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json) | Accepted source-disjoint held-out PromptToTask eval with leakage checks, baseline score, candidate score, failure examples, and row-level source ledger. | After approval, author a bounded source-disjoint eval work order. | A split draft, candidate row, or API guard is not an improvement proof. |
| PromptToTask ablation | MISSING_ABLATION | [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md); [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md) | Exact/regex/keyword/nearest-overlap ablation deltas and negative controls tied to the same held-out eval. | After eval artifacts exist, run a bounded ablation-only packet. | PromptToTask baseline overlap or pattern routing cannot be called semantic improvement. |
| PromptToTask fresh-load/continue-training | MISSING_FRESH_LOAD_CONTINUE | [TRAINABLE-ARCHITECTURE-AUDIT-v4.md](../../TRAINABLE-ARCHITECTURE-AUDIT-v4.md); [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json) | Rewritten PromptToTask checkpoint plus positive fresh-load and continue/no-op continuation gates that preserve quality metrics. | After checkpoint rewrite is authorized and produced, run fresh-load/continue gates as their own bounded step. | Importability, baseline unit execution, or false-claim firewall is not a positive quality lifecycle gate. |
| OrchestrationReasoning current evidence | PROVEN_NARROW | [phase-03-automated-boundary-report.md](phase-03-automated-boundary-report.md); [TRAINABLE-ARCHITECTURE-AUDIT-v4.md](../../TRAINABLE-ARCHITECTURE-AUDIT-v4.md); [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json); [ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json](../../ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json) | No source-disjoint reasoning-quality eval, no ablation deltas, no checkpoint rewrite, no positive fresh-load/continue semantic gate. | Prepare a future bounded OrchestrationReasoning eval packet; keep this step evidence-only. | May claim logical hierarchy/baseline adapter evidence only; may not claim reasoning quality improvement. |
| OrchestrationReasoning source-disjoint eval | MISSING_SOURCE_DISJOINT_EVAL | [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md); [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json) | Accepted source-disjoint held-out orchestration eval with leakage checks, baseline/candidate scores, and failure examples. | After approval, author a bounded source-disjoint OrchestrationReasoning eval work order. | Scenario-unit routing and hierarchy parity are not source-disjoint semantic proof. |
| OrchestrationReasoning ablation | MISSING_ABLATION | [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md); [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md) | BM25/filename/title/static-route ablation deltas and negative controls tied to the same held-out reasoning eval. | After eval artifacts exist, run a bounded ablation-only packet. | Static route, filename/title matching, or BM25-like success cannot be claimed as semantic reasoning improvement. |
| OrchestrationReasoning fresh-load/continue-training | MISSING_FRESH_LOAD_CONTINUE | [TRAINABLE-ARCHITECTURE-AUDIT-v4.md](../../TRAINABLE-ARCHITECTURE-AUDIT-v4.md); [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json) | Rewritten OrchestrationReasoning checkpoint plus positive fresh-load and continue/no-op continuation gates that preserve quality metrics. | After checkpoint rewrite is authorized and produced, run fresh-load/continue gates as their own bounded step. | Host/runtime false markers and baseline unit execution cannot be upgraded into quality lifecycle proof. |
| SkillSelection current evidence | PROVEN_NARROW | [phase-03-automated-boundary-report.md](phase-03-automated-boundary-report.md); [02-dotnet-api-contracts.txt](phase-03-command-output/02-dotnet-api-contracts.txt); [ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json](../../ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json); [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json) | No accepted SkillSelection source-disjoint quality eval and no candidate-order/position ablation tied to positive quality. | Prepare a future bounded SkillSelection eval/ablation work order. | May claim API shortcut guards and lifecycle/adapter records only; may not claim SkillSelection semantic quality improvement. |
| SkillSelection source-disjoint eval | MISSING_SOURCE_DISJOINT_EVAL | [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md); [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md) | Accepted held-out SkillSelection eval with source-disjoint split, baseline/candidate scores, leakage checks, and failure examples. | After approval, author a bounded source-disjoint SkillSelection eval work order. | Candidate list coverage, route presence, or requested-skill handling is not a held-out quality proof. |
| SkillSelection ablation | MISSING_ABLATION | [02-dotnet-api-contracts.txt](phase-03-command-output/02-dotnet-api-contracts.txt); [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-PLAN-v1.md) | Candidate-order permutation report, candidate-position ablation, requested-skill decoy controls, and negative controls tied to the same held-out eval. | After eval artifacts exist, run a bounded SkillSelection ablation-only packet. | API tests that ignore row hash/order are guard evidence, not a positive ablation improvement result. |
| SkillSelection fresh-load/continue-training | PROVEN_NARROW_LIFECYCLE_ONLY | [ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json](../../ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json); [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json) | Quality-linked fresh-load/continue gate after source-disjoint eval and ablation; existing asset lifecycle is not enough. | Tie lifecycle gates to future SkillSelection quality eval artifacts before any claim review. | Import/no-op continuation and registry locator evidence cannot by itself prove semantic selection improvement. |
| Hidden deterministic shortcut audit | PARTIAL_GUARD_ONLY | [02-dotnet-api-contracts.txt](phase-03-command-output/02-dotnet-api-contracts.txt); [SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json); [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1.md) | Full integrated audit after positive eval artifacts, covering exact/regex/keyword, BM25, filename/title, candidate order/position, row ids, hashes, fixture leakage, and static routes. | After all three component eval/ablation packets exist, run a bounded hidden-shortcut audit. | Existing shortcut guards reduce false claims; they do not prove the absence of all hidden deterministic shortcuts for a future uplift. |
| Semantic quality improvement conclusion | CLAIM_CLOSED | [phase-05-product-readiness-matrix.md](phase-05-product-readiness-matrix.md); [phase-06-full-extension-readiness-matrix.md](phase-06-full-extension-readiness-matrix.md); [TRAINABLE-ARCHITECTURE-AUDIT-v4.md](../../TRAINABLE-ARCHITECTURE-AUDIT-v4.md); [SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json](../../SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json) | Source-disjoint eval + ablations + checkpoint rewrite + positive fresh-load/continue gates + integrated hidden shortcut audit across all three components. | Phase 10 learnability/training and DPO creation intake/downgrade; это следующий bounded sub-agent work order, без запуска evals/tests/training/probes в этой навигационной правке. | Do not claim semantic quality improvement, full trained-model readiness, host runtime quality, or all-three-component positive quality. |

## Human-readable status

Сделано: создана 14-row matrix, где узкая evidence отделена от proof. PromptToTask, OrchestrationReasoning и SkillSelection имеют narrow evidence, но source-disjoint eval rows = 0, ablation proof rows = 0, positive fresh-load/continue quality gate rows = 0, full hidden deterministic shortcut audit accepted = 0.

В процессе: нет запущенных evals/tests/builds/training/probes/GUI/prompt/model/checkpoint/locator/git actions.

Осталось: следующий bounded sub-agent work order - Phase 10 learnability/training and DPO creation intake/downgrade (human-facing Phase 11 of 17 при one-based report numbering). Эта правка не запускает evals/tests/training/probes.

Review нашёл: Phase 03/05/06/Q9 evidence хорошо закрывает false-claim firewall и baseline/lifecycle status, но не доказывает semantic quality improvement. Дополнительно review нашёл Phase 09.01F evidence gap: все четыре целевых файла untracked, поэтому git baseline не может доказать wording-only change.

Что исправлено: navigation is now correct while semantic invariants remain current-state verified: row_count 14, claim false, top_counts total 14, top_counts claim opened 0, exact next action present, unsupported semantic quality claim not opened.

Следующее действие: Phase 10 learnability/training and DPO creation intake/downgrade; это следующий bounded sub-agent work order.
