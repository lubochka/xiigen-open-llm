# Layer 0-B — Router Pairs (050)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-050-001 — APIGatewayContract: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-050-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the API gateway contract including routing rules, rate limits, and upstream service contracts."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "routing" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-050-002 — APIGatewayContract: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-050-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the API gateway routing layer with path-based routing to backend microservices."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "api" + "routing" → 3.3 ✓

---

## B12-0B-050-003 — APIGatewayContract: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-050-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Wire up the API gateway to the service mesh to propagate distributed tracing headers."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "wire up" + "api" → 3.3 ✓

---

## B12-0B-050-004 — APIGatewayContract: [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY] → 3.1 + 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-050-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Design the API gateway architecture and define the authentication contract for all inbound requests."

B:
  Routes: ["3.1", "3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.1"
  DecisionCount: 4
  TransitionType: "documentation"
  FlowOrder:
    1. ArchitectureFlow31 (must execute first)
    2. APIContractFlow33
    3. parallel(DocumentationFlow21, SecurityAuditFlow34)
    4. QAFlow
  v6 check: "design" → 3.1 ✓ | "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓ | "authentication" → 3.4 ✓
