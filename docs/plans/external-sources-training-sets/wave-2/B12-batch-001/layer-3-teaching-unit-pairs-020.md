# Layer 3 — Teaching Unit Pairs (020)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-020 — Teaching: "route" fires API_CONTRACT for file upload and all API domains

```
source_id: xiigen-b12-tu-020
rule_id: B12-RULE-ROUTE
pair_kind: teaching_unit
```

### Concept

"Route" (and "routes") is a first-class API_CONTRACT trigger. It fires in any domain context
where an API route/path/URL pattern is being specified. This is domain-independent.

### Training Examples

A: "Document the API routes for the financial reporting service and their request parameters."
B: API_CONTRACT fires on "api" + "routes"; DOCUMENT fires on "document"
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

---

A: "Define the API versioning strategy and route contract for endpoint deprecation."
B: API_CONTRACT fires on "api" + "route" + "contract"; DOCUMENT fires on "define"
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

---

A: "Specify the API routes and request payload format for inventory reservation operations."
B: API_CONTRACT fires on "api" + "routes" + "request"; DOCUMENT fires on "specify"
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

### Route is domain-independent

"route" fires API_CONTRACT regardless of the domain:
  "inventory routes" → API_CONTRACT (with "api" co-occurrence)
  "reporting routes" → API_CONTRACT (with "api" co-occurrence)
  "versioning routes" → API_CONTRACT (with "route" alone — "route" is first-class)
  "admin routes" → API_CONTRACT (with "api"/"route")

### Memory aid

Route = URL path pattern in an API = API_CONTRACT
"route" in any API context → API_CONTRACT fires unconditionally
