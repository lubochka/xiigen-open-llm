# Layer 0-B — Router Pairs
# Batch: B18-batch-001
# Scenario: B18 — Development — refactoring and data migration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B18B1-WDA-router-001 — [IMPLEMENT, ARCHITECTURE] → 4.4 + 3.1

```
source_id: xiigen-b18-router-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  CompiledText: "Write the database migration to move customer data from the legacy schema to the new normalized structure."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.4"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale: IMPLEMENT with migration/data context → 4.4. ARCHITECTURE ("structure") → 3.1.

---

## Pair B18B1-WDA-router-002 — [IMPLEMENT] refactor → 4.1

```
source_id: xiigen-b18-router-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT"]
  CompiledText: "Refactor the OrderService class to extract the payment logic into a separate PaymentProcessor module."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.95
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale: IMPLEMENT with class/logic/module vocabulary → 4.1 (Backend Services).
The refactoring context (extract, refactor) does not change the sub-phase — 4.1 still owns
backend class-level work.

---

## Pair B18B1-WDA-router-003 — [IMPLEMENT] seed → 4.4

```
source_id: xiigen-b18-router-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT"]
  CompiledText: "Create database seed data for the product catalog with 500 sample products and category fixtures."

B:
  RoutingDecisions:
    - PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.4"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale: IMPLEMENT with seed/fixture/database context → 4.4 (Data Migration and
Seeding). 4.1 is nearest wrong (NearestWrongSubPhaseScore 0.40).
