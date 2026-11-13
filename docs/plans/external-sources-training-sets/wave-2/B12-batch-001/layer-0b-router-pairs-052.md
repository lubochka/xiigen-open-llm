# Layer 0-B — Router Pairs (052)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-052-001 — CircuitBreakerAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-052-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the circuit breaker API contract for the payment gateway downstream service calls."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-052-002 — CircuitBreakerAPI: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-052-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the circuit breaker pattern for the inventory REST API endpoint calls."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "rest" + "api" + "endpoint" → 3.3 ✓

---

## B12-0B-052-003 — CircuitBreakerAPI: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-052-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Expose the circuit breaker state in the API health check response."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "expose" + "api" → 3.3 ✓

---

## B12-0B-052-004 — CircuitBreakerAPI: [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-052-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Design the circuit breaker architecture and define the fallback API contract for the order service."

B:
  Routes: ["3.1", "3.3", "2.1"]
  PrimarySubPhase: "3.1"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. ArchitectureFlow31 (first)
    2. APIContractFlow33
    3. DocumentationFlow21
    4. QAFlow
  v6 check: "design" → 3.1 ✓ | "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓
