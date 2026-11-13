# Held-Out and Corruption Pairs (019)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H019-001 — Held-out: QA+IMPLEMENT+DOCUMENT — multi-service coordinator

A: "Build the release coordinator service, document the inter-service dependency map, and QA sign-off."
  Expected: ["QA", "IMPLEMENT", "DOCUMENT"]
  Held-out: "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "document" → DOCUMENT; "QA sign-off" → QA. feature_addition.

---

## H019-002 — Held-out: gap — deployment sequencing

A: "Deploy UserService, then CartService, then OrderService in sequence."
  Expected: []
  Held-out: Multi-service deployment sequencing → pure gap. knowledge_edge.

---

## H019-003 — Held-out: QA+DOCUMENT — multi-service criteria

A: "Document the multi-service release criteria and QA checklist before the coordinated rollout."
  Expected: ["QA", "DOCUMENT"]
  Held-out: "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA checklist" → QA; "rollout" → gap. documentation.

---

## C019-001 — Corruption: deployment sequencing fires ARCHITECTURE

A: "Deploy services UserService → CartService → OrderService."
  Correct: [] — gap
  Corrupt: ["ARCHITECTURE"] ← wrong (sequencing treated as architectural design)
  Teaching: Deployment execution sequencing is gap. ARCHITECTURE fires on design/structure/layer verbs, not on deployment order lists.

---

## C019-002 — Corruption: "dependency map" fires a new predicate

A: "Document the inter-service dependency map."
  Correct: ["DOCUMENT"] — "document" verb fires DOCUMENT; "dependency map" is document content
  Corrupt: ["DOCUMENT", "DEPENDENCY"] ← invented DEPENDENCY predicate (wrong)
  Teaching: "dependency map" is the content of a document. It does not fire a new predicate.
