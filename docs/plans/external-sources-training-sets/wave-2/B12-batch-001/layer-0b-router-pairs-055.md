# Layer 0-B — Router Pairs (055)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-055-001 — APIMonitoring: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-055-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the API monitoring contract including response time SLAs and error rate thresholds."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-055-002 — APIMonitoring: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-055-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the API health check endpoint that reports service status and dependency health."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "api" + "endpoint" → 3.3 ✓

---

## B12-0B-055-003 — APIMonitoring: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-055-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Integrate the API monitoring tool with the gateway to track latency per endpoint."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "integrate" + "api" + "endpoint" → 3.3 ✓

---

## B12-0B-055-004 — APIMonitoring: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-055-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Specify the API monitoring contract with alert thresholds for the payment service endpoints."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "endpoints" → 3.3 ✓ | "specify" → 2.1 ✓
