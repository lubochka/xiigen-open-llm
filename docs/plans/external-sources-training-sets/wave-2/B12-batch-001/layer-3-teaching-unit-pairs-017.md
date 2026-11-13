# Layer 3 — Teaching Unit Pairs (017)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-017 — Teaching: "design" + API term fires both ARCHITECTURE and API_CONTRACT

```
source_id: xiigen-b12-tu-017
rule_id: B12-RULE-DESIGN-DUAL-FIRE
pair_kind: teaching_unit
```

### Concept

"Design" fires ARCHITECTURE. When "design" appears together with API terms (api/graphql/rest/etc.),
BOTH ARCHITECTURE and API_CONTRACT fire. Neither suppresses the other.

### Training Examples

A: "Design the GraphQL API for the product search service."
B: ARCHITECTURE fires on "design"; API_CONTRACT fires on "GraphQL" + "api"
   CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
   Routes: 3.1 + 3.3 + 2.1; DecisionCount: 3

---

A: "Specify the GraphQL query contract for the product search API."
B: ARCHITECTURE does NOT fire ("specify" ≠ "design"); API_CONTRACT fires
   CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   Routes: 3.3 + 2.1; DecisionCount: 2

---

A: "Design the shipping rates service interface and define the API contract."
B: ARCHITECTURE fires on "design"; API_CONTRACT fires on "interface" (API context) + "api" + "contract"
   CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
   Routes: 3.1 + 3.3 + 2.1; DecisionCount: 3

### FlowOrder impact

When ARCHITECTURE fires with API_CONTRACT:
  FlowOrder: [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]
  (ARCHITECTURE executes FIRST — it informs the API contract design)

When ARCHITECTURE does NOT fire:
  FlowOrder: [APIContractFlow33, DocumentationFlow21]

### Common error

CORRUPT reasoning: "design" + "api" → only API_CONTRACT fires (ARCHITECTURE suppressed)
CORRECT: both fire; DecisionCount increases by 1; FlowOrder gains ArchitectureFlow31 at front
