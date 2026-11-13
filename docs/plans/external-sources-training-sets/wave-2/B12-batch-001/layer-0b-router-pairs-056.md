# Layer 0-B — Router Pairs (056)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-056-001 — APIAnalytics: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-056-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the API analytics contract for capturing request volume and consumer usage patterns."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "request" (API co-occurrence) → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-056-002 — APIAnalytics: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-056-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the API usage analytics endpoint that reports per-consumer request counts."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "api" + "endpoint" → 3.3 ✓

---

## B12-0B-056-003 — APIAnalytics: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-056-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Connect the API analytics pipeline to the API gateway to capture all inbound request metadata."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "connect" + "api" → 3.3 ✓

---

## B12-0B-056-004 — APIAnalytics: [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-056-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Design the API analytics architecture and document the data collection contract for usage metrics."

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
  v6 check: "design" → 3.1 ✓ | "api" + "contract" → 3.3 ✓ | "document" → 2.1 ✓
