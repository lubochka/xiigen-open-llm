# Layer 0-C — Knowledge Edge Pairs (046)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-046-001 — Rule: "backward compatible" is a qualifier; "upgrade" does NOT fire IMPLEMENT

```
source_id: xiigen-b12-0c-046-001
pair_kind: knowledge_edge
```

### Backward compatibility qualifier and upgrade distinction

"Define the backward compatible API contract for the inventory service REST endpoint upgrade":
  API_CONTRACT: "api" + "contract" + "rest" + "endpoint"
  DOCUMENT: "define"
  NOT IMPLEMENT: "upgrade" is a lifecycle/migration term, not a construction verb
  NOT ARCHITECTURE: "backward compatible" is a qualifier, not a design verb

"Upgrade" vocabulary does NOT fire IMPLEMENT:
  "upgrade from v1 to v2" → lifecycle migration description
  "API upgrade" → version transition label

IMPLEMENT requires explicit construction verbs: implement, build, create, write, develop, scaffold.
"Upgrade" alone is never IMPLEMENT.

---

## B12-0C-046-002 — Rule: "implement the additive-only changes" fires IMPLEMENT (construction verb explicit)

```
source_id: xiigen-b12-0c-046-002
pair_kind: knowledge_edge
```

### Additive-only changes with implement verb

"Implement the additive-only changes to the REST API endpoints to maintain backward compatibility":
  IMPLEMENT: "implement" (explicit construction verb)
  API_CONTRACT: "rest" + "api" + "endpoints"
  NOT DOCUMENT: no documentation verb present
  TransitionType: feature_addition

"Additive-only" and "backward compatibility" are domain qualifiers. The IMPLEMENT predicate
fires from the explicit "implement" verb, not from the compatibility context.

---

## B12-0C-046-003 — Rule: "specify the backward compatibility API policy and document" fires API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0c-046-003
pair_kind: knowledge_edge
```

### Two DOCUMENT triggers in one request

"Specify the backward compatibility API policy and document the allowed breaking change criteria":
  DOCUMENT: "specify" (first DOCUMENT trigger)
  DOCUMENT: "document" (second DOCUMENT trigger)
  API_CONTRACT: "api"
  DOCUMENT multiplicity: two triggers → one DOCUMENT predicate in CompiledPredicateIds
  DecisionCount: 2 (not 3)

Multiple DOCUMENT triggers in one request produce ONE DOCUMENT predicate and ONE routing decision
to 2.1. DOCUMENT multiplicity works the same as SECURITY multiplicity: N triggers → 1 predicate.
