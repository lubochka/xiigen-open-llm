# Layer 0-C — Knowledge Edge Pairs (056)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-056-001 — Rule: "analytics" is a domain qualifier; "request" with API co-occurrence fires API_CONTRACT (B12-R26)

```
source_id: xiigen-b12-0c-056-001
pair_kind: knowledge_edge
```

### Analytics qualifier and request co-occurrence rule

"Define the API analytics contract for capturing request volume and consumer usage patterns":
  API_CONTRACT: "api" + "contract" + "request" (B12-R26: request with API co-occurrence)
  DOCUMENT: "define"

"Analytics" is a domain qualifier. It does not independently fire any predicate.
"Request" fires API_CONTRACT only when API co-occurrence is present (B12-R26).
In this case "api" provides the necessary co-occurrence.

"Consumer usage patterns" is a domain description. Does not fire any predicate.

---

## B12-0C-056-002 — Rule: "connect the API analytics pipeline to the API gateway" fires API_CONTRACT (double "api")

```
source_id: xiigen-b12-0c-056-002
pair_kind: knowledge_edge
```

### Connect + double API context

"Connect the API analytics pipeline to the API gateway to capture all inbound request metadata":
  API_CONTRACT: "connect" (integration verb) + "api" (from "API analytics") + "api" (from "API gateway")
  Multiple API triggers → ONE API_CONTRACT predicate
  NOT IMPLEMENT: "connect" is an integration verb

"Inbound request metadata" — "request" appears here. With "api" co-occurrence present, B12-R26
means "request" reinforces API_CONTRACT, but the predicate is already fired by "api" + "connect".
DecisionCount: 1.

---

## B12-0C-056-003 — Rule: "design the API analytics architecture and document the data collection contract" fires three predicates

```
source_id: xiigen-b12-0c-056-003
pair_kind: knowledge_edge
```

### Architecture + analytics + contract

"Design the API analytics architecture and document the data collection contract for usage metrics":
  ARCHITECTURE: "design" (unconditional)
  API_CONTRACT: "api" + "contract"
  DOCUMENT: "document"
  NOT SECURITY: no authentication keyword

"Data collection contract" — "contract" fires API_CONTRACT via Core Rule 2 (unconditional).
"Data collection" is a domain qualifier. The predicate fires from "contract", not from "data collection".

Three predicates: [ARCHITECTURE, API_CONTRACT, DOCUMENT]. DecisionCount = 3.
