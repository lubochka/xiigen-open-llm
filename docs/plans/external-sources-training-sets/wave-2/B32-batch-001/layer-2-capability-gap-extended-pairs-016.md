# Layer 2 — Capability Gap Extended Pairs (016)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-016-001 — "hack together" alone → zero predicates → knowledge_edge

A: "Let's hack together a quick solution for the bug."
B: Predicates: [] | GapId: gap-vibe-coding-hack-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted

---

## B32-gap-016-002 — "cobble together" → same gap ID as "hack together"

A: "Cobble together a solution for the cache invalidation problem."
B: Predicates: [] | GapId: gap-vibe-coding-hack-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted | Note: cobble together = hack together = gap-vibe-coding-hack-001

---

## B32-gap-016-003 — "throw together + prototype + throwaway" → triple gap; zero predicates

A: "Throw together a prototype — throwaway code is fine."
B: Predicates: [] | GapId: gap-vibe-coding-hack-001, gap-vibe-coding-prototype-001, gap-vibe-coding-throwaway-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted once (not 3 times)

---

## B32-gap-016-004 — "hack together + build" → IMPLEMENT fires; gap warning

A: "Build and hack together the reporting dashboard — nothing fancy."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---
