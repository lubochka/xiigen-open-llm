# Layer 0-C — Knowledge Edge Pairs (052) — CapacityVerification
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E052-001 — "capacity verification" fires TEST; "capacity plan" alone is gap

"capacity verification" → TEST | "run capacity verification" → TEST | "capacity plan" → gap
  Teaching: "verification" in gate context fires TEST. "capacity plan" review is gap.

---

## E052-002 — "verify scaling behavior" fires TEST; "configure auto-scaling" is gap

"configure auto-scaling policies" → gap | "verify scaling behavior" → TEST ("verify" fires TEST)
  Teaching: "verify" fires TEST. "configure" is gap even when in the same sentence.

---

## E052-003 — "capacity checker service" fires IMPLEMENT via "service" rule

"capacity verification service" → IMPLEMENT | "capacity checker service" → IMPLEMENT
  Teaching: "service" fires IMPLEMENT in capacity verification compound nouns.

---

## E052-004 — "load tests for capacity" fires TEST via test-writing verb

"write load tests for peak traffic" → TEST | "add capacity tests for scaling" → TEST
  Teaching: Test-writing verbs fire TEST in capacity verification context.

---

## E052-005 — "throughput criteria" fires DOCUMENT via "criteria" rule

"throughput criteria" → DOCUMENT | "capacity gate criteria" → DOCUMENT
  Teaching: "criteria" fires DOCUMENT in capacity verification compound nouns.
