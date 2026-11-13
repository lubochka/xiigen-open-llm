# Layer 1 — Transition Orchestrator Pairs (028)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-028-001 — Orchestration: regularization + cross-validation gap

Gap request: "Regularization prevents overfitting; cross-validation measures generalization."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-training-001
Orchestration: No flow. One gap event (both terms → same gap ID). DomainId: software-development.

---

## TO-028-002 — Orchestration: implement regularization + create cross-validation

Predicates: ["IMPLEMENT"]
Request: "Implement the regularization wrapper and create the cross-validation runner."
Flow: ImplementationFlow41 (single IMPLEMENT fires; both code verbs point to same predicate)
TransitionType: feature_addition
Teaching: Two code verbs ("implement", "create") both fire IMPLEMENT. One IMPLEMENT predicate, one flow.

---

## TO-028-003 — Orchestration: write tests for regularization

Predicates: ["TEST"]
Request: "Write tests for the regularization service to verify weight decay is applied correctly."
Flow: TestFlow51 | TransitionType: N/A
Rule: "write tests" fires TEST. Single-flow orchestration.

---

## TO-028-004 — v6 corruption: cross-validation → IMPLEMENT (v6-02)

v6 corrupt: "cross-validation" → IMPLEMENT (treating AI evaluation method as code verb)
Correct: [] → GapHandler → gap-ai-ml-training-001
Error: v6-02. "cross-validation" is AI vocabulary. Does not fire IMPLEMENT without explicit code verb.

---

## TO-028-005 — Orchestration: QA the cross-validation pipeline

Predicates: ["QA"]
Request: "QA the cross-validation pipeline before promoting to production."
Flow: QAGatingFlow73 | TransitionType: N/A (QA sub-phase)
Rule: "QA" abbreviation fires QAGatingFlow73. QA always last even as a single flow.
