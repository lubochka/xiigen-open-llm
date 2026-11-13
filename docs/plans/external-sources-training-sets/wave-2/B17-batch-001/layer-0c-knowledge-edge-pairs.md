# Layer 0-C — Knowledge-Edge Pairs
# Batch: B17-batch-001
# Scenario: B17 — Development — integration — third-party payment connector
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B17B1-WDA-kedge-001 — normal integration path: FLOW_EXECUTING

```
source_id: xiigen-b17-kedge-001
pair_kind: knowledge_edge
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"

B:
  FlowId: "universal-nine-node-development-flow-4.3"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "2"
    next_flow_id: "universal-nine-node-development-flow-4.3"

---

## Pair B17B1-WDA-kedge-002 — "integrate" alone: API_CONTRACT not IMPLEMENT

```
source_id: xiigen-b17-kedge-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Integrate the payment provider into the checkout flow."

B:
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  Predicate analysis:
    "integrate" → API_CONTRACT (NOT IMPLEMENT — "integrate" is an API_CONTRACT term)
    "payment provider" — no predicate
    "checkout flow" — no predicate
  Result: ["API_CONTRACT"] only.
  Teaching: The model must route to 3.3 (API_CONTRACT), not 4.3, when only "integrate"
  fires. 4.3 (Integration sub-phase) is reached via IMPLEMENT, not API_CONTRACT alone.
  Router routes: API_CONTRACT → 3.3, not 4.3.
