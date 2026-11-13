# Layer 0-A — Compiler Pairs (011)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-011-v6-01 — v6-01 corruption: API_CONTRACT suppressed on "contract" term (wrong)

```
source_id: xiigen-b12-0a-011-v601
corruption_type: v6-01
pair_kind: compiler_corruption
```

Correct behavior:
  Request: "Define the data contracts between the order service and the fulfillment service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "contract" → API_CONTRACT fires REGARDLESS of modifier (Core Rule 2)

v6-01 corrupt behavior:
  CompiledPredicateIds: ["DOCUMENT"]
  Corruption: API_CONTRACT suppressed because "data" modifier made compiler classify as
  non-API usage. The corrupted model treated "data contracts" as a data-modeling term,
  not an API contract term.

Why v6-01 is wrong:
  Core Rule 2 is unconditional: "contract" → API_CONTRACT regardless of modifier.
  The B12 lexicon does not have a "data" suppressor for "contract".
  Five contract modifiers that ALL fire API_CONTRACT:
    - "data contracts"    → API_CONTRACT (corrupted v6-01 suppresses this — WRONG)
    - "service contracts" → API_CONTRACT
    - "message contracts" → API_CONTRACT
    - "event contracts"   → API_CONTRACT
    - "webhook contract"  → API_CONTRACT

Detection signal: If a request contains "contract" and CompiledPredicateIds does NOT include
"API_CONTRACT", this is a v6-01 corruption.

---

## B12-0A-011-v6-02 — v6-02 corruption: SECURITY suppressed for "authentication" in API docs (wrong)

```
source_id: xiigen-b12-0a-011-v602
corruption_type: v6-02
pair_kind: compiler_corruption
```

Correct behavior:
  Request: "Write the OpenAPI spec for the cart service and include authentication requirements."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  Reason: "authentication" → SECURITY fires ALWAYS regardless of context (Core Rule 6)

v6-02 corrupt behavior:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Corruption: SECURITY suppressed because "authentication" appeared in an "API documentation"
  context and the corrupted model classified it as API design vocabulary, not security vocabulary.
  The corrupted model reasoned: "authentication requirements in API docs = API_CONTRACT topic."

Why v6-02 is wrong:
  Core Rule 6 is unconditional: "authentication" → SECURITY ALWAYS regardless of context.
  SECURITY fires even when:
    - "authentication" appears in API documentation requests
    - "authentication" appears in endpoint definition requests
    - "authentication" appears in specification writing requests
  The context does NOT suppress SECURITY. Both API_CONTRACT and SECURITY fire simultaneously.

Detection signal: If a request contains "authentication" and CompiledPredicateIds does NOT include
"SECURITY", this is a v6-02 corruption.

---

## B12-0A-011-v6-03 — v6-03 corruption: three predicates collapsed to single 3.3 routing decision (wrong)

```
source_id: xiigen-b12-0a-011-v603
corruption_type: v6-03
pair_kind: compiler_corruption
```

Correct behavior:
  Request: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0 token endpoint, and document the security requirements."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  RoutingDecisions:
    - IMPLEMENT → Phase 4.3
    - API_CONTRACT → Phase 3.3
    - DOCUMENT → Phase 2.1
    - SECURITY → Phase 3.4
    - AUTHORIZATION → Phase 3.4
  DecisionCount: 5

v6-03 corrupt behavior:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  (predicates correctly detected)
  RoutingDecisions:
    - API_CONTRACT → Phase 3.3 ONLY (all other predicates collapsed into single 3.3 decision)
  DecisionCount: 1

  Corruption: The router collapsed multiple routing decisions into a single 3.3 decision,
  treating API_CONTRACT as the dominant predicate and ignoring all others.

Why v6-03 is wrong:
  Core Rule 23: DecisionCount = number of predicates fired, not unique sub-phases.
  Five predicates → five routing decisions to five entries (even if some route to same sub-phase).
  Each predicate independently routes to its destination:
    IMPLEMENT → 4.3 (not suppressed by API_CONTRACT)
    DOCUMENT  → 2.1 (not suppressed by API_CONTRACT)
    SECURITY  → 3.4 (not suppressed by API_CONTRACT)
    AUTHORIZATION → 3.4 (not suppressed by API_CONTRACT)

Detection signal: If CompiledPredicateIds has N > 1 predicates but RoutingDecisions shows only one
entry (or only the 3.3 entry), this is a v6-03 routing-collapse corruption.

---

## B12-0A-011-summary — v6 corruption summary for B12

| Version | Corruption | Wrong behavior | Correct rule |
|---------|------------|----------------|--------------|
| v6-01 | API_CONTRACT suppressed on "contract" | CompiledPredicateIds missing API_CONTRACT when modifier changes "contract" | "contract" → API_CONTRACT regardless of modifier (Rule 2) |
| v6-02 | SECURITY suppressed in API doc context | CompiledPredicateIds missing SECURITY when "authentication" is in API docs | "authentication" → SECURITY ALWAYS (Rule 6) |
| v6-03 | Multiple routing decisions collapsed to 3.3 | DecisionCount=1 despite multiple predicates | DecisionCount = predicate count, each routes independently (Rule 23) |
