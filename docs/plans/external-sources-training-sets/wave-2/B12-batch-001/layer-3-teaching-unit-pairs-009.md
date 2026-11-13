# Layer 3 — Teaching Unit Pairs (009)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-009 — Teaching: "design" → ARCHITECTURE; "design the API" → ARCHITECTURE + API_CONTRACT

```
source_id: xiigen-b12-tu-009
rule_id: B12-RULE-DESIGN-API-COMBINATION
pair_kind: teaching_unit
```

### Concept (Core Rule 5)

Two clauses of the same rule:
  Clause 1: "design" (alone, no API vocabulary) → ARCHITECTURE fires
  Clause 2: "design the API" (with API vocabulary) → ARCHITECTURE + API_CONTRACT both fire

The rule is additive: adding API vocabulary to a "design" request adds API_CONTRACT without
removing ARCHITECTURE. Both predicates fire together.

### Why This Rule Exists

System design and API contract design are related but distinct concerns:
  - System design (3.1): "what are the components and how do they relate?"
  - API contract design (3.3): "what is the exact shape of the interfaces between components?"

When a request involves BOTH designing the system AND specifying the API (e.g., "design the REST
API architecture"), both concerns are active simultaneously → both predicates fire.

### Training Examples — Clause 1 (design alone → ARCHITECTURE only)

A: "Design the microservice architecture for the e-commerce platform."
B: CompiledPredicateIds: ["ARCHITECTURE"]
   Reason: "design" + "architecture" → ARCHITECTURE; no API vocabulary → API_CONTRACT does NOT fire
   Routing: 3.1 only

A: "Design the service topology for the notification system."
B: CompiledPredicateIds: ["ARCHITECTURE"]
   Reason: "design" → ARCHITECTURE; no API vocabulary present

### Training Examples — Clause 2 ("design the API" → ARCHITECTURE + API_CONTRACT)

A: "Design the REST API for the product catalog service."
B: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]
   Reason: "design" → ARCHITECTURE; "REST API" → API_CONTRACT; both fire
   Routing: 3.1 + 3.3

A: "Design the GraphQL schema for the search API."
B: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]
   Reason: "design" → ARCHITECTURE; "GraphQL", "schema" (with graphql co-occurrence) → API_CONTRACT
   Routing: 3.1 + 3.3

A: "Design the REST API architecture for the admin panel and document the route structure."
B: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
   Reason: "design", "architecture" → ARCHITECTURE; "REST API", "route" → API_CONTRACT; "document" → DOCUMENT
   Routing: 3.1 + 3.3 + 2.1

### The "design" does NOT fire IMPLEMENT rule

"design" is a planning/specification verb, not a construction verb.
"design" → ARCHITECTURE (not IMPLEMENT)
"implement" → IMPLEMENT

A: "Design and implement the cart API."
B: CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
   Reason: "design" → ARCHITECTURE; "implement" → IMPLEMENT; "API" → API_CONTRACT
   Note: 3.2 routing (ARCHITECTURE + IMPLEMENT together = 3.2 sub-phase)

### Memory Aid

"design" alone = planning = ARCHITECTURE
"design" + API vocabulary = planning + API specifics = ARCHITECTURE + API_CONTRACT
"design" never = IMPLEMENT (design is specification, not construction)
