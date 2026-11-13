# Layer 0-C — Knowledge-Edge Pairs
# Batch: B18-batch-001
# Scenario: B18 — Development — refactoring and data migration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B18B1-WDA-kedge-001 — normal IMPLEMENT/migration path: FLOW_EXECUTING

```
source_id: xiigen-b18-kedge-001
pair_kind: knowledge_edge
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  SourceStatus: "ready"

B:
  FlowId: "universal-nine-node-development-flow-4.4"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "2"
    next_flow_id: "universal-nine-node-development-flow-4.4"

---

## Pair B18B1-WDA-kedge-002 — "rename" fires IMPLEMENT; "reorganize" fires IMPLEMENT

```
source_id: xiigen-b18-kedge-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Rename the UserService to CustomerService and reorganize the project namespace structure."

B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  SourceStatus: "ready"
  Predicate analysis:
    "rename" → IMPLEMENT fires on "rename" (rename is a code action)
    "reorganize" → IMPLEMENT fires on "reorganize" (≈ refactor/restructure)
    "structure" → ARCHITECTURE fires on "structure"
    "namespace" — not in any predicate directly
    "project" — not in any predicate
  Result: ["IMPLEMENT", "ARCHITECTURE"]
  Teaching: "rename" and "reorganize" are in the IMPLEMENT lexicon. Code renaming
  is treated as an implementation action (code modification), not a separate
  category. ARCHITECTURE fires on "structure" in "namespace structure."
