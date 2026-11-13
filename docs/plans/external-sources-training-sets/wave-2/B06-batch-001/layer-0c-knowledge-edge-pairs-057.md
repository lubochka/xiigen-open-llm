# Layer 0-C — Knowledge-Edge Pairs (057)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-057-001 — StockAlertSystem: gap verbs

```
source_id: xiigen-b06-0c-057-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-057-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for StockAlertSystem:
    "Notify subscribed customers when out-of-stock items are replenished." → KNOWLEDGE_EDGE (notify = operational)
    "Send a low-stock alert to the warehouse manager." → KNOWLEDGE_EDGE (send = operational)
    "Subscribe the customer to back-in-stock alerts." → KNOWLEDGE_EDGE (subscribe = operational)
    "Monitor inventory levels and trigger alerts at threshold." → KNOWLEDGE_EDGE (monitor, trigger = operational)
  Teaching: "subscribed customers", "out-of-stock items", "low-stock alert", "inventory levels" are domain nouns.

---

## B06-0C-057-002 — StockAlertSystem: boundary cases

```
source_id: xiigen-b06-0c-057-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-057-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for StockAlertSystem:
    "Define the stock alert requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Notify customers per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Develop the notification service." → NOT knowledge-edge (develop fires IMPLEMENT)
    "Notify customers when stock returns." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-057-003 — StockAlertSystem: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-057-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-057-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for StockAlertSystem:
    gap: "notify/send/subscribe/monitor/trigger stock alerts"
    spec: "define/describe/specify/document stock alert system requirements"
  Teaching: "stock alert", "back-in-stock", "low-stock threshold" are domain nouns — never fire predicates alone.
