# Layer 0-C — Knowledge Edge Pairs (059)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-059-001 — Rule: "contract test" — "contract" fires API_CONTRACT unconditionally (Core Rule 2)

```
source_id: xiigen-b12-0c-059-001
pair_kind: knowledge_edge
```

### Core Rule 2 in contract testing context

"Define the contract test suite specification for the payment service REST API endpoints":
  API_CONTRACT: "contract" (Core Rule 2: unconditional) + "rest" + "api" + "endpoints"
  DOCUMENT: "define" + "specification"

Core Rule 2: "contract" fires API_CONTRACT regardless of the modifier that precedes it.
"Contract test" = a testing pattern. The word "contract" still fires API_CONTRACT.
Other patterns: "contract testing", "consumer-driven contract", "provider contract" — all fire API_CONTRACT.

Corrupt (v6-01): treating "contract test" as a testing domain term that suppresses API_CONTRACT.
The "test" modifier does NOT suppress the API_CONTRACT fire from "contract".

---

## B12-0C-059-002 — Rule: "integrate the API contract tests into CI" fires API_CONTRACT (integrate ≠ IMPLEMENT)

```
source_id: xiigen-b12-0c-059-002
pair_kind: knowledge_edge
```

### Integrate verb in contract testing context

"Integrate the API contract tests into the CI pipeline to verify compatibility on every deployment":
  API_CONTRACT: "integrate" (with API co-occurrence) + "api" + "contract"
  NOT IMPLEMENT: "integrate" is not a construction verb (B12-R17)
  NOT DOCUMENT: no documentation verb

"Verify compatibility" is a testing purpose clause. It does not fire any predicate.
"Deployment" is an infrastructure term. Does not fire any predicate.

Result: [API_CONTRACT]. DecisionCount: 1. Routes: [3.3].

---

## B12-0C-059-003 — Rule: "implement the API contract tests" fires IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b12-0c-059-003
pair_kind: knowledge_edge
```

### Implement as construction verb in contract testing

"Implement the API contract tests for the order service using consumer-driven contract testing":
  IMPLEMENT: "implement" (explicit construction verb)
  API_CONTRACT: "api" + "contract"
  NOT DOCUMENT: no documentation verb

Contrast:
  "Integrate the API contract tests into CI" → API_CONTRACT only (integrate ≠ IMPLEMENT)
  "Implement the API contract tests" → IMPLEMENT + API_CONTRACT (implement = construction verb)
  "Define the contract test spec" → API_CONTRACT + DOCUMENT (define = documentation verb)

Three distinct verbs → three distinct predicate patterns for contract testing context.
This reinforces B12-R17: only explicit construction verbs fire IMPLEMENT.
