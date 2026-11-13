# Layer 0-B — Router Pairs (051)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-051-001 — ServiceMeshAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-051-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the service mesh API contract for inter-service communication routing and retries."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "routing" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-051-002 — ServiceMeshAPI: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-051-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the service mesh sidecar proxy contract and build the inter-service API routing rules."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" + "build" → 4.3 ✓ | "contract" + "api" + "routing" → 3.3 ✓

---

## B12-0B-051-003 — ServiceMeshAPI: [API_CONTRACT] → 3.3 only (TLS does not fire SECURITY)

```
source_id: xiigen-b12-0b-051-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Connect the service mesh to the API gateway to enforce mutual TLS for all service-to-service calls."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "connect" + "api" → 3.3 ✓
  Note: "TLS" alone does NOT fire SECURITY. "authentication" keyword required for SECURITY.

---

## B12-0B-051-004 — ServiceMeshAPI: [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY] → 3.1 + 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-051-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Design the service mesh API topology and specify the mutual authentication contract between services."

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
  v6 check: "design" → 3.1 ✓ | "api" + "contract" → 3.3 ✓ | "specify" → 2.1 ✓ | "authentication" → 3.4 ✓
