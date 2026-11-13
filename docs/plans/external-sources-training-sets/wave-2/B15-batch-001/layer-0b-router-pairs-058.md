# Layer 0-B — Router Pairs (058)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-058-001 — DeadLetterService: IMPLEMENT routing

A: "Create a DeadLetterService to capture and replay failed event messages."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-058-002 — DeadLetterService: IMPLEMENT+API_CONTRACT routing

A: "Create DeadLetterService and integrate with the DLQ API."
B: Flow: APIContractFlow33 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-058-003 — DeadLetterService: IMPLEMENT+TEST routing

A: "Create DeadLetterService and write integration tests for message capture and replay."
B: Flow: ImplementationFlow41 → TestingFlow51 | DecisionCount: 2

---

## B15-router-058-004 — DeadLetterService: IMPLEMENT+DOCUMENT routing

A: "Create DeadLetterService and document the replay strategy and failure classification schema."
B: Flow: ImplementationFlow41 ‖ DocumentationFlow21 | DecisionCount: 2 | Parallel-eligible

---

## B15-router-058-005 — DeadLetterService: IMPLEMENT+API_CONTRACT+TEST+DOCUMENT routing

A: "Create DeadLetterService, integrate with the DLQ API, write capture/replay tests, and document the replay strategy."
B: Flow: APIContractFlow33 → ImplementationFlow41 → TestingFlow51 ‖ DocumentationFlow21 | DecisionCount: 4
Ordering: 3.3 → 4.1 → 5.1 ‖ 2.1
