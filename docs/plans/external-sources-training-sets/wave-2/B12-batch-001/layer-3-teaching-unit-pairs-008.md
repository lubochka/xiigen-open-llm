# Layer 3 — Teaching Unit Pairs (008)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-008 — Teaching: DecisionCount counts predicates fired, not unique sub-phases

```
source_id: xiigen-b12-tu-008
rule_id: B12-RULE-DECISIONCOUNT
pair_kind: teaching_unit
```

### Concept (Core Rule 23)

DecisionCount = COUNT(predicates in CompiledPredicateIds)

NOT: count of unique sub-phases
NOT: count of unique flows
NOT: count of unique routing destinations

### Why This Matters

When SECURITY and AUTHORIZATION both route to 3.4, they produce TWO routing decisions
to the same destination. This is intentional — each predicate represents a distinct concern
that must be addressed at 3.4:
  SECURITY: authentication and token handling
  AUTHORIZATION: access control and permission scopes

Even though both go to 3.4, they are separate concerns → separate decisions → separate flows.

### The Formula

Given CompiledPredicateIds: [P1, P2, P3, ..., Pn]
  DecisionCount = n
  (regardless of where each Pi routes)

### Training Examples

Example 1: Two predicates, two sub-phases → DecisionCount = 2
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: 3.3 + 2.1 (two unique sub-phases)
  DecisionCount: 2 ✓

Example 2: Three predicates, three sub-phases → DecisionCount = 3
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Routing: 3.3 + 2.1 + 3.4 (three unique sub-phases)
  DecisionCount: 3 ✓

Example 3: Five predicates, four unique sub-phases → DecisionCount = 5 (NOT 4)
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  Routing: 4.3 + 3.3 + 2.1 + 3.4 + 3.4 (four unique sub-phases)
  DecisionCount: 5 ✓  (five predicates = five decisions)
  NOT DecisionCount: 4 ✗ (counting unique sub-phases — WRONG)

### The SECURITY + AUTHORIZATION to 3.4 Special Case

When BOTH SECURITY and AUTHORIZATION fire:
  SECURITY → 3.4 (decision #1 to 3.4)
  AUTHORIZATION → 3.4 (decision #2 to 3.4)
  Total decisions to 3.4 = 2
  Each uses a different flow: SecurityAuditFlow34 vs AuthorizationFlow34

Even though the destination is the same sub-phase, they are different concerns → different flows
→ two separate DecisionCount entries.

### Training Pairs

A: "Document the REST API contract with OAuth access control."
   (oauth fires both SECURITY and AUTHORIZATION)
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]

B: DecisionCount: 4
   Not 3 (even though SECURITY+AUTHORIZATION both go to 3.4)

---

A: "Write the OpenAPI spec for the auth service, implement OAuth token endpoint, document security."
   CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]

B: DecisionCount: 5
   UniqueSubPhaseCount: 4 (informational only, not DecisionCount)
   4.3, 3.3, 2.1, 3.4, 3.4 → five decisions to four unique sub-phases

### Memory Aid

DecisionCount question: "How many predicates are in CompiledPredicateIds?"
Count the list items. That number = DecisionCount.
Do not count unique destinations. Do not deduplicate by sub-phase.
