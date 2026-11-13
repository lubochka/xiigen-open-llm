# Layer 0-C — Knowledge Edge Pairs (052)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-052-001 — Rule: "circuit breaker" is a domain qualifier (resilience pattern)

```
source_id: xiigen-b12-0c-052-001
pair_kind: knowledge_edge
```

### Circuit breaker domain qualifier

"Circuit breaker" is a resilience pattern (from Hystrix/Polly/resilience4j). It is a domain qualifier.
It does NOT independently fire any predicate.

Examples:
  "Define the circuit breaker API contract" → API_CONTRACT (api + contract) + DOCUMENT (define)
  "Expose the circuit breaker state in the API" → API_CONTRACT (expose + api)
  "Implement the circuit breaker pattern for the REST API endpoint" → IMPLEMENT + API_CONTRACT

"Circuit breaker" describes what kind of API behavior is being contracted, not a predicate-firing term.
The same rule applies to: retry policy, bulkhead pattern, timeout, fallback — all resilience domain qualifiers.

---

## B12-0C-052-002 — Rule: "expose the circuit breaker state in the API response" fires API_CONTRACT only

```
source_id: xiigen-b12-0c-052-002
pair_kind: knowledge_edge
```

### Expose + health check = API_CONTRACT

"Expose the circuit breaker state (open/closed/half-open) in the API health check response" fires:
  API_CONTRACT: "expose" (publication verb) + "api"
  NOT IMPLEMENT: "expose" is a release/publication verb
  NOT DOCUMENT: no documentation verb present

"Health check" is a common API endpoint pattern. "Health check response" provides API context
but does not independently fire any predicate beyond what "api" provides.

---

## B12-0C-052-003 — Rule: "design circuit breaker + define fallback API contract" fires ARCHITECTURE + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0c-052-003
pair_kind: knowledge_edge
```

### Design + define combination in resilience context

"Design the circuit breaker architecture and define the fallback API contract for the order service":
  ARCHITECTURE: "design" (unconditional)
  API_CONTRACT: "api" + "contract"
  DOCUMENT: "define"
  NOT IMPLEMENT: no construction verb
  NOT SECURITY: no authentication keyword

Three predicates: DecisionCount = 3. Routes = [3.1, 3.3, 2.1].
ARCHITECTURE executes first; API_CONTRACT second; DOCUMENT third; QA last.
TransitionType: documentation.
