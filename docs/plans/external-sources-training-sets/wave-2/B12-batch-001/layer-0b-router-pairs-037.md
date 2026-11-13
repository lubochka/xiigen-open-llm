# Layer 0-B — Router Pairs (037)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-037-001 — RateLimitingAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-037-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the rate limiting API contract for the payments service public endpoints."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "endpoint" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-037-002 — RateLimitingAPI: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-037-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the rate limiting middleware for the REST API gateway and write the throttle contract."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" + "write" → 4.3 ✓ | "rest" + "api" + "contract" → 3.3 ✓

---

## B12-0B-037-003 — RateLimitingAPI: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-037-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Expose the rate limit response headers in the API contract."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "expose" + "api" + "contract" → 3.3 ✓ | no other predicates

---

## B12-0B-037-004 — RateLimitingAPI: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-037-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Define the rate limiting API contract with authentication bypass rules for trusted service accounts."

B:
  Routes: ["3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. parallel(DocumentationFlow21, SecurityAuditFlow34)
    3. QAFlow (always last)
  v6 check:
    "api" + "contract" → API_CONTRACT → 3.3 ✓
    "define" → DOCUMENT → 2.1 ✓
    "authentication" → SECURITY (Core Rule 6: unconditional) → 3.4 ✓
    TransitionType = documentation (no IMPLEMENT) ✓
    DecisionCount = 3 ✓
