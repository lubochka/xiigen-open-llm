# Layer 1 — Transition Orchestrator Pairs (031)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-031-001 — Orchestration: BERT gap

Gap request: "BERT is used for sentence classification tasks."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-031-002 — Orchestration: build BERT client + architecture

Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Request: "Design the BERT serving architecture and implement the inference client."
Flow order: ArchitectureFlow31 → ImplementationFlow41
TransitionType: feature_addition
Rule: ARCHITECTURE before IMPLEMENT (canonical rule 1).

---

## TO-031-003 — Orchestration: Llama gap

Gap request: "Llama 3 requires significant compute for inference at full precision."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-031-004 — Orchestration: implement Llama client + write tests

Predicates: ["IMPLEMENT", "TEST"]
Request: "Implement a Llama API client and write integration tests for streaming responses."
Flow order: ImplementationFlow41 → TestFlow51
TransitionType: feature_addition
Rule: IMPLEMENT before TEST (canonical rule 3).

---

## TO-031-005 — v6 corruption: BERT → BERT_ENCODE invented predicate (v6-01)

v6 corrupt: "BERT" → "BERT_ENCODE" predicate
Correct: [] → GapHandler → gap-ai-ml-model-001
Error: v6-01. "BERT" is an AI model name. No BERT_ENCODE predicate exists.
