# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B28-transition-008-001 — QA+IMPLEMENT+TEST+SECURITY+DOCUMENT+ARCHITECTURE → full lifecycle

A: ["QA","IMPLEMENT","TEST","SECURITY","DOCUMENT","ARCHITECTURE"] | feature_addition
  Sequence: ARCHITECTURE → SECURITY → IMPLEMENT → TEST → DOCUMENT → QA gate

## B28-transition-008-002 — QA: "QA gate first" ordering error is incorrect

A: ["QA","IMPLEMENT","TEST"] | INCORRECT: QA → IMPLEMENT → TEST (QA first)
  CORRECT: IMPLEMENT → TEST → QA gate (QA last)
  Teaching: QA gate must always be last. Any ordering with QA before other flows is incorrect.

## B28-transition-008-003 — QA+ARCHITECTURE: design → QA

A: ["QA","ARCHITECTURE"] | ARCHITECTURE design → QA sign-off | documentation

## B28-transition-008-004 — QA+TEST+SECURITY: penetration testing QA

A: ["QA","TEST","SECURITY"] | Sequence: SECURITY (audit/pentest) → TEST (write pentest scripts) → QA gate

## B28-transition-008-005 — QA+IMPLEMENT: harness implementation

A: ["QA","IMPLEMENT"] | IMPLEMENT (build harness) → QA gate | feature_addition

