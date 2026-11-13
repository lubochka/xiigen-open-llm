# Layer 0-C — Knowledge Edge Pairs (051)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-051-001 — Rule: "service mesh" is a domain qualifier; contract/route/api fire API_CONTRACT

```
source_id: xiigen-b12-0c-051-001
pair_kind: knowledge_edge
```

### Service mesh domain qualifier

"Service mesh" describes an infrastructure pattern (e.g., Istio, Linkerd). It is a domain qualifier.
API_CONTRACT fires from API vocabulary terms co-occurring with service mesh context.

Examples:
  "Define the service mesh API contract" → API_CONTRACT (api + contract) + DOCUMENT (define)
  "Connect the service mesh to the API gateway" → API_CONTRACT (connect + api)
  "Service mesh routing rules" → "routing" provides API_CONTRACT trigger

"Sidecar proxy" is also a service mesh domain term. Does not independently trigger predicates.
"Sidecar proxy contract" → API_CONTRACT via "contract" (Core Rule 2: unconditional).

---

## B12-0C-051-002 — Rule: "implement service mesh sidecar proxy contract and build inter-service API routing rules"

```
source_id: xiigen-b12-0c-051-002
pair_kind: knowledge_edge
```

### Dual construction verb with sidecar/routing context

"Implement the service mesh sidecar proxy contract and build the inter-service API routing rules":
  IMPLEMENT: "implement" + "build" (two construction verbs → one IMPLEMENT predicate)
  API_CONTRACT: "contract" (Core Rule 2) + "api" + "routing"

IMPLEMENT multiplicity: "implement" and "build" both fire IMPLEMENT, but produce ONE IMPLEMENT
predicate in CompiledPredicateIds. IMPLEMENT multiplicity follows the same rule as SECURITY and DOCUMENT.

---

## B12-0C-051-003 — Rule: "mutual authentication" fires SECURITY (authentication keyword present)

```
source_id: xiigen-b12-0c-051-003
pair_kind: knowledge_edge
```

### Mutual authentication and SECURITY

"Design the service mesh API topology and specify the mutual authentication contract between services":
  SECURITY: "authentication" (Core Rule 6: unconditional — "mutual authentication" contains "authentication")
  ARCHITECTURE: "design"
  API_CONTRACT: "api" + "contract"
  DOCUMENT: "specify"
  Result: [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY], DecisionCount = 4

"Mutual authentication" contains the word "authentication". Core Rule 6 fires on the presence
of the word "authentication" regardless of the modifier ("mutual", "token", "certificate", etc.).
