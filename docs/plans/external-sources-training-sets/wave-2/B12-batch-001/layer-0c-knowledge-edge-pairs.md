# Layer 0-C — Knowledge-Edge Pairs
# Batch: B12-batch-001
# Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B12B1-WDA-kedge-001 — normal API_CONTRACT path: FLOW_EXECUTING

```
source_id: xiigen-b12-kedge-001
pair_kind: knowledge_edge
```

A:
  DocumentId: "B12-compiler-001-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"

B:
  FlowId: "universal-nine-node-development-flow-3.3"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "3"
    next_flow_id: "universal-nine-node-development-flow-3.3"

---

## Pair B12B1-WDA-kedge-002 — "interface" context boundary

```
source_id: xiigen-b12-kedge-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Design the service interface architecture and define the API contracts."

B:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  Predicate analysis:
    "design" → ARCHITECTURE fires
    "interface" → fires ARCHITECTURE (system design context) AND API_CONTRACT (api context)
      — BOTH fire when "interface" appears with both "design/architecture" and "api/contract" co-occurrences
    "architecture" → ARCHITECTURE fires
    "define" → DOCUMENT fires
    "API" → API_CONTRACT fires
    "contracts" → API_CONTRACT fires
  Result: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  Teaching point: "interface" is a dual-predicate term — it fires both ARCHITECTURE and
  API_CONTRACT depending on context. When both contexts are present, both fire.
