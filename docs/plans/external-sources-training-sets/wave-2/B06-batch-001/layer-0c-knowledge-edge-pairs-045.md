# Layer 0-C — Knowledge-Edge Pairs (045)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-045-001 — BackorderManagement: gap verbs

```
source_id: xiigen-b06-0c-045-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-045-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for BackorderManagement:
    "Queue the customer order when inventory is exhausted." → KNOWLEDGE_EDGE (queue = operational)
    "Notify customers when their backordered item restocks." → KNOWLEDGE_EDGE (notify = operational)
    "Fulfill backordered items in FIFO order after restock." → KNOWLEDGE_EDGE (fulfill = operational)
    "Monitor inventory levels for backorder triggers." → KNOWLEDGE_EDGE (monitor = operational)
  Teaching: "customer order", "inventory", "backordered item", "restock" are domain nouns.

---

## B06-0C-045-002 — BackorderManagement: boundary cases

```
source_id: xiigen-b06-0c-045-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-045-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for BackorderManagement:
    "Define the backorder management requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Queue orders per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Create the backorder queue manager." → NOT knowledge-edge (create fires IMPLEMENT)
    "Queue customer orders when stock runs out." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-045-003 — BackorderManagement: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-045-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-045-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for BackorderManagement:
    gap: "queue/notify/fulfill/monitor/process backorder"
    spec: "define/describe/specify/document backorder management requirements"
  Teaching: "backorder", "FIFO (First In First Out)", "restock trigger" are domain nouns — never fire predicates alone.
