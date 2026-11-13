# Train/held-out partition report

Partition assigned: **no**. The phase produced a blocker/proposal only; it did not invent train/held-out status.

## Proposed policy after missing refs are filled

- Do not place any scenario in train_candidate until source_ref, doc_id and source_license_ref are verified.
- Do not place scenarios from the same doc_id/source_disjoint_group on both sides of train and held_out_eval.
- Current 12 scenarios may be eval-only candidates only after license/privacy review, but eval-only does not provide a train-vs-held-out split by itself.
- Provide a separate source partition ledger before strict QUALITY-03 rerun.

## Scenario Partition Status

| Scenario | Train/held-out | Partition assigned | Status | Blocker |
|---|---|---:|---|---|
| TQ-001-prompt-task-documentation | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-002-prompt-task-interrupt | quarantine_missing_metadata | false | QUARANTINE_MISSING_METADATA | source_ref/doc_id/license/partition missing |
| TQ-003-prompt-task-redaction | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-004-orchestration-runtime-proof | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-005-orchestration-token-pairing | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-006-orchestration-learning-lifecycle | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-007-skill-architecture-audit | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-008-skill-debug-runtime | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-009-skill-learning-safety | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-010-mixed-no-overclaim | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-011-mixed-shortcut-audit | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
| TQ-012-mixed-final-readiness | quarantine_license_unclear | false | QUARANTINE_LICENSE_UNCLEAR | license and train-vs-held-out partition proof missing |
