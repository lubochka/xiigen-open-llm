# Layer 1 — Transition Orchestrator Pairs (027)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-027-001 — Orchestration: hyperparameter gap

Gap request: "The hyperparameter search takes too long with grid search."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-training-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-027-002 — Orchestration: build hyperparameter service + integrate

Predicates: ["IMPLEMENT", "API_CONTRACT"]
Request: "Build the hyperparameter configuration service and integrate it with the training orchestrator API."
Flow order: APIContractFlow33 → ImplementationFlow41
TransitionType: feature_addition
Rule applied: API_CONTRACT before IMPLEMENT.

---

## TO-027-003 — Orchestration: secure hyperparameter endpoint

Predicates: ["SECURITY"]
Request: "Secure the hyperparameter configuration endpoint against unauthorized modification."
Flow: SecurityAuditFlow | TransitionType: N/A
Rule: "secure" fires SECURITY. Single-flow orchestration.

---

## TO-027-004 — v6 corruption: HYPERPARAMETER invented predicate

v6 corrupt: "hyperparameter" → "HYPERPARAMETER" predicate
Correct: [] → GapHandler → gap-ai-ml-training-001
Error: v6-01. "hyperparameter" is AI vocabulary. No predicate by that name exists.

---

## TO-027-005 — Orchestration: overfitting gap

Gap request: "Overfitting is detected when validation loss diverges from training loss."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-training-001
Orchestration: No flow. One gap event. DomainId: software-development.
