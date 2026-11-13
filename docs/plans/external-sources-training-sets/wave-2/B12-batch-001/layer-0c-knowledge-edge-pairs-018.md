# Layer 0-C — Knowledge Edge Pairs (018)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-018-001 — "spec" in API context fires DOCUMENT (not API_CONTRACT alone)

```
source_id: xiigen-b12-0c-018-001
rule_id: B12-RULE-SPEC-CONTEXT
pair_kind: knowledge_edge
```

A:
  Case 1: "Define the OpenAPI spec for the reporting API."
  Case 2: "Specify the functional spec for the reporting dashboard."

B:
  Case 1: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    "define" → DOCUMENT fires
    "OpenAPI" → API_CONTRACT fires
    "spec" → DOCUMENT fires (spec = specification, documentation verb)
    "API" → API_CONTRACT fires
    API_CONTRACT fires via "openapi" and "api", NOT via "spec"
    "spec" contributes DOCUMENT, not API_CONTRACT

  Case 2: CompiledPredicateIds: ["DOCUMENT"]
    "specify" → DOCUMENT fires
    "spec" → DOCUMENT fires
    "functional" — no predicate
    "reporting dashboard" — no API term
    No API_CONTRACT: no api/rest/graphql/openapi/swagger/endpoint/contract present

  Teaching: "spec" fires DOCUMENT. "spec" alone does NOT fire API_CONTRACT.
  "OpenAPI spec" fires API_CONTRACT via "openapi", not via "spec".

---

## B12-0C-018-002 — "scaffold" fires IMPLEMENT for ReportingAPI

```
source_id: xiigen-b12-0c-018-002
rule_id: B12-RULE-SCAFFOLD-IMPLEMENT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Scaffold the reporting API controller and define the contract for date-range query endpoints."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "scaffold" → IMPLEMENT fires on "scaffold"
    "API" → API_CONTRACT fires
    "define" → DOCUMENT fires
    "contract" → API_CONTRACT fires
    "endpoints" → API_CONTRACT fires
  TransitionType: feature_addition
  Teaching: "scaffold" is an IMPLEMENT verb. It creates new code structure.
  "scaffold the controller" = creating new code = IMPLEMENT = feature_addition.

---

## B12-0C-018-003 — "develop" fires IMPLEMENT for ReportingAPI

```
source_id: xiigen-b12-0c-018-003
rule_id: B12-RULE-DEVELOP-IMPLEMENT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Develop the reporting API and specify the OpenAPI contract for the dashboard data endpoint."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "develop" → IMPLEMENT fires on "develop"
    "API" → API_CONTRACT fires
    "specify" → DOCUMENT fires
    "OpenAPI" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
    "endpoint" → API_CONTRACT fires
  TransitionType: feature_addition
  Teaching: "develop" is an IMPLEMENT verb alongside build/implement/create/write/scaffold.
  Complete IMPLEMENT verb list for B12: implement, build, create, write, develop, scaffold.
