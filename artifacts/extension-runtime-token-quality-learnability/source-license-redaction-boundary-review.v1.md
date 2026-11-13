# Source / License / Redaction Boundary Review v1

Phase: `LEARN-02`.

Verdict: **PASS for boundary review, common training blocked**.

## What This Proves

The 12 local DPO candidates from `local-dpo-candidates-v1.jsonl` are classified for safe storage. Every row remains user-local for now. Common model training is not allowed in this phase.

## What This Does Not Prove

This does not prove DPO quality, model learning, semantic quality, checkpoint readiness, or token savings. It does not run external AI and does not mutate any model.

## Human Decision

Rows from this conversation are useful as local adaptation signals, but they are not automatically common training material. They contain user behavior preferences, project-specific failures, local runtime bugs, and private correction patterns. Before any common use, each row needs source/license/redaction review and a universal-rule extraction pass.

## Row Classification

| Candidate | Machine | Decision | Reason |
|---|---|---|---|
| `LDPO-001` | `SkillSelectionMachine` | User-local only | Prevents deterministic skill-selection shortcuts; redaction required before sharing. |
| `LDPO-002` | `OrchestrationReasoningMachine` | User-local only | Runtime overclaim boundary from this project session. |
| `LDPO-003` | `PromptToTaskMachine` | User-local only | Luba-specific readable-report format preference. |
| `LDPO-004` | `ArchitectBehavior` | User-local private behavior | Authority/role boundary preference. |
| `LDPO-005` | `OrchestrationReasoningMachine` | User-local only | Token-vs-quality boundary from current plan execution. |
| `LDPO-006` | `OrchestrationReasoningMachine` | Quarantine for common | Needs universal-vs-project-specific separation and license review. |
| `LDPO-007` | `SkillSelectionMachine` | User-local only | Local Codex adapter input-boundary bug; common use needs sanitized reproduction. |
| `LDPO-008` | `ArchitectBehavior` | User-local private behavior | User correction about pseudo-solutions and concrete trainable-unit design. |
| `LDPO-009` | `SkillSelectionMachine` | User-local private candidate | Local correction should create DPO candidate, not common mutation. |
| `LDPO-010` | `OrchestrationReasoningMachine` | Quarantine for common | Needs source/license plus target/rollback binding before common training. |
| `LDPO-011` | `PromptToTaskMachine` | User-local only | Debug visibility and privacy boundary. |
| `LDPO-012` | `PromptToTaskMachine` | User-local only | Controlled runtime error wording. |

## Rule For Future Common Training

Common training requires one of:

- verified MIT/Apache-2.0 source material with license evidence;
- explicit Luba approval for the exact source and scope.

It also requires redaction, universal-rule extraction, source-disjoint held-out split, target checkpoint, rollback checkpoint, and a fresh approval packet. Until then: `common_training_allowed=false`.
