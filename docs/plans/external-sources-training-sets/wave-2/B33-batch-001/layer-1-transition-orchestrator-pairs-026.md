# Layer 1 — Transition Orchestrator Pairs (026)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-026-001 — Orchestration: gradient descent + backpropagation gap

Gap request: "Backpropagation uses gradient descent to update weights."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-training-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-026-002 — Orchestration: implement optimizer + integrate API

Predicates: ["IMPLEMENT", "API_CONTRACT"]
Request: "Implement the gradient descent optimizer and integrate it with the training monitoring API."
Flow order: APIContractFlow33 → ImplementationFlow41
TransitionType: feature_addition
Rule applied: API_CONTRACT before IMPLEMENT (canonical rule 2).

---

## TO-026-003 — v6 corruption: GRADIENT_DESCENT invented predicate

v6 corrupt input: "gradient descent" → "GRADIENT_DESCENT" predicate
Correct output: [] → GapHandler → gap-ai-ml-training-001
Error type: v6-01. "gradient descent" is AI vocabulary. No GRADIENT_DESCENT predicate exists.

---

## TO-026-004 — Orchestration: all seven flows with training vocabulary

Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Request: "Architect the gradient descent platform, integrate backpropagation API, implement optimizer, secure training endpoint, test gradient computation, document algorithm, QA."
Flow order: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestFlow51 → DocumentationFlow62 → QAGatingFlow73
TransitionType: feature_addition

---

## TO-026-005 — TransitionType: refactoring for training vocabulary refactor request

Predicates: ["IMPLEMENT"]
Request: "Refactor the gradient descent optimizer to use adaptive learning rates."
Flow: ImplementationFlow41 | TransitionType: refactoring
Rule: "refactor" fires IMPLEMENT with TransitionType = refactoring. "gradient descent optimizer" = object.
