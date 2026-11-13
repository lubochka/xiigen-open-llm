# Local DPO Candidates v1

Phase: `LEARN-01`.

This artifact prepares candidate rows for the user-side adaptive model only. It does not train a model, does not mutate checkpoints, does not update the common model, and does not claim learnability.

## What This Set Is For

The candidates capture repeated failures from the extension/runtime/learning work so the local model can later learn Luba's preferred behavior:

- do not hide deterministic shortcuts behind "semantic" wording;
- do not claim full runtime or learnability from handler/API evidence;
- keep architect and executor boundaries clear;
- write plans and audits in a human-readable form with concrete code-grounded fixes;
- keep universal orchestration learning separate from project-stack-specific details;
- build local DPO signals after corrections without mutating the common model;
- preserve privacy, source/license review, backup, and rollback gates before training.

## Candidate Rows

| Candidate | Target machine | Positive example | Negative example | Why it matters |
|---|---|---|---|---|
| LDPO-001 | SkillSelectionMachine | Choose skills from task meaning and explain evidence. | Choose by candidate position and call it semantic. | Prevents deterministic shortcut masking. |
| LDPO-002 | OrchestrationReasoningMachine | State exactly what runtime/API evidence proves. | Claim full extension readiness from dashboard or handler tests. | Prevents scope inflation. |
| LDPO-003 | PromptToTaskMachine | Extract UML, no-code algorithms, code quotes, and concrete before-to-after fixes. | Produce a generic technical plan with no real solution detail. | Preserves Luba's readable plan format. |
| LDPO-004 | ArchitectBehavior | Stay in architect/sub-agent governance until implementation is explicitly authorized. | Parent session starts execution while a plan/review answer is still required. | Prevents role drift. |
| LDPO-005 | OrchestrationReasoningMachine | Accept token savings only when quality is controlled and non-regressing. | Treat token drop alone as success. | Stops token-only success claims. |
| LDPO-006 | OrchestrationReasoningMachine | Keep only universal orchestration rules for common learning. | Train common model on project-stack-specific implementation details. | Protects the common model from local-stack leakage. |
| LDPO-007 | SkillSelectionMachine | Prove `AvailableSkills` reaches core before claiming runtime skill selection. | Treat runtime probe as skill-selection proof while `AvailableSkills` is empty. | Catches input-boundary bugs. |
| LDPO-008 | ArchitectBehavior | Require exact ILogicalUnit replacement details for dictionary/counter scorers. | Say "make it a unit" without files, methods, training flow, sets, or tests. | Converts pseudo-solutions into implementable design. |
| LDPO-009 | SkillSelectionMachine | Convert user correction into quarantined local DPO candidate. | Patch deterministic/common behavior silently. | Supports local learning without unsafe common mutation. |
| LDPO-010 | OrchestrationReasoningMachine | Require target/rollback/source/held-out/mutation binding before training. | Start training from vague approval. | Protects checkpoints and reproducibility. |
| LDPO-011 | PromptToTaskMachine | Show debug input/output locally and redact shared artifacts. | Omit debug or leak raw private content. | Keeps programmer observability without privacy leakage. |
| LDPO-012 | PromptToTaskMachine | Return controlled human-readable infrastructure error. | Pretend the model answered when core path is missing. | Prevents false success on runtime errors. |

## Positive And Negative Example Shape

Positive local candidate:

```text
User correction: "This is not a semantic skill selector; it is a counter."
Chosen behavior: classify the mechanism honestly, keep it as baseline/internal only, and require held-out/source-disjoint proof before semantic claims.
Rejected behavior: report "SkillSelection trained successfully" because the counter produced a selected skill.
```

Negative local candidate:

```text
User correction: "The plan needs exact replacement code/classes/tests, not a prompt."
Rejected behavior: answer with "create PromptContextSkillSelectionUnit" but no exact files, fields, training algorithm, K-NN role, DPO source, tests, or lifecycle.
Reason: this repeats the pseudo-solution failure and cannot be executed safely by a sub-agent.
```

## Gates Preserved

- `common_training_allowed=false` for every row.
- `user_local_store_required=true` for every row.
- `no_training_executed=true` for every row.
- Rows that may touch source material beyond the current repo require license review.
- Rows with user conversation behavior require redaction review before export.

## Limitations

This is a candidate pack only. It does not prove the model learned the behavior. Later phases still need local storage binding, backup/export/import, continue-training, fresh-load, held-out eval, ablation, and rollback evidence.
