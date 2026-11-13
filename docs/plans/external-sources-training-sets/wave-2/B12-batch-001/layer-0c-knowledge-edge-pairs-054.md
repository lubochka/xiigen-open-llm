# Layer 0-C — Knowledge Edge Pairs (054)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-054-001 — Rule: "timeout" is a domain qualifier; SLA does NOT fire DOCUMENT independently

```
source_id: xiigen-b12-0c-054-001
pair_kind: knowledge_edge
```

### Timeout domain qualifier and SLA boundary

"Timeout" is a resilience pattern term. It is a domain qualifier (B12-R25 extended).
"SLA" (Service Level Agreement) is a business term. Neither "timeout" nor "SLA" independently fires any predicate.

Examples:
  "Define the timeout API contract" → API_CONTRACT (api + contract) + DOCUMENT (define)
  "Specify the timeout policy and document the SLA" → DOCUMENT (specify + document) + API_CONTRACT (rest + api + endpoints)
  "Expose timeout configuration in the API contract" → API_CONTRACT (expose + api + contract)

"SLA" alone → no predicate fire. A documentation verb (specify, document, define) must be present for DOCUMENT.

---

## B12-0C-054-002 — Rule: "expose timeout configuration in the API contract" fires API_CONTRACT only

```
source_id: xiigen-b12-0c-054-002
pair_kind: knowledge_edge
```

### Timeout exposure = API_CONTRACT only

"Expose the timeout configuration values in the API contract for callers to understand SLA expectations":
  API_CONTRACT: "expose" + "api" + "contract"
  NOT DOCUMENT: no documentation verb present
  NOT SECURITY: no authentication keyword

"Callers to understand SLA expectations" is context/purpose language. It does not fire DOCUMENT.
DOCUMENT requires an explicit documentation verb. Purpose clauses ("for callers to understand X")
are not documentation verbs.

---

## B12-0C-054-003 — Boundary: "specify timeout policy AND document SLA" fires DOCUMENT twice = ONE DOCUMENT predicate

```
source_id: xiigen-b12-0c-054-003
pair_kind: knowledge_edge
```

### DOCUMENT multiplicity in timeout context

"Specify the timeout policy and document the SLA for the product search REST API endpoints":
  DOCUMENT: "specify" (first DOCUMENT trigger)
  DOCUMENT: "document" (second DOCUMENT trigger)
  API_CONTRACT: "rest" + "api" + "endpoints"
  DOCUMENT multiplicity → ONE DOCUMENT predicate

Two DOCUMENT triggers ("specify" + "document") in the same request produce ONE DOCUMENT predicate
and ONE routing decision to 2.1. DecisionCount = 2 (not 3).

This reinforces the multiplicity rule: N triggers for predicate P → 1 instance of P in CompiledPredicateIds.
