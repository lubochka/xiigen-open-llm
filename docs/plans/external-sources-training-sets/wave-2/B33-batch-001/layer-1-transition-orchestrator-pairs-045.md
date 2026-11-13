# Layer 1 — Transition Orchestrator Pairs (045)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-045-001 — Orchestration: LLM + training vocabulary mixed gap (round-2 + round-3)

Gap request: "The LLM uses backpropagation and gradient descent during pretraining."
Predicates: [] | GapHandler → gap-ai-ml-model-001 (model dominant) | One event.

---

## TO-045-002 — Orchestration: implement LLM training manager

Predicates: ["IMPLEMENT"]
Request: "Implement a training manager that wraps LLM pretraining with gradient tracking."
Flow: ImplementationFlow41 | TransitionType: feature_addition

---

## TO-045-003 — Orchestration: API_CONTRACT fires: "orchestrate via API" pattern

Predicates: ["API_CONTRACT"]
Request: "Orchestrate the LLM calls via the model serving API."
Teaching: "via API" + "orchestrate" → API_CONTRACT fires (from earlier B33 rules: "orchestrate" + "via API" → API_CONTRACT).
Flow: APIContractFlow33 | TransitionType: feature_addition

---

## TO-045-004 — Orchestration: ARCHITECTURE fires on "architect" + structural noun

Predicates: ["ARCHITECTURE"]
Request: "Architect the LLM serving layer with caching and load balancing."
Flow: ArchitectureFlow31 | TransitionType: feature_addition
Teaching: "architect" is a variant of "design"; fires ARCHITECTURE when structural noun implied.

---

## TO-045-005 — v6 corruption catalogue for training vocabulary

| v6 corrupt input | Error type | Correct output |
|-----------------|------------|---------------|
| neural-network → NEURAL_NET | v6-01 | [] → gap-ai-ml-training-001 |
| training-loop → IMPLEMENT | v6-02 | [] → gap-ai-ml-training-001 |
| gradient-descent → GRADIENT_DESCENT | v6-01 | [] → gap-ai-ml-training-001 |
| backpropagation → BACKPROP | v6-01 | [] → gap-ai-ml-training-001 |
| dataset-curation → IMPLEMENT | v6-02 | [] → gap-ai-ml-training-001 |
| implement optimizer + integrate API → [IMPLEMENT] | v6-03 | ["IMPLEMENT", "API_CONTRACT"] |
