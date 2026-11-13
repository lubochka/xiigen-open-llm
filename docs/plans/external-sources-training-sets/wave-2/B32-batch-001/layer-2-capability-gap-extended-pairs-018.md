# Layer 2 — Capability Gap Extended Pairs (018)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-018-001 — "throwaway" alone → zero predicates → knowledge_edge

A: "Throwaway code — just throwaway, we'll rewrite later."
B: Predicates: [] | GapId: gap-vibe-coding-throwaway-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted | Note: "rewrite later" is future frame; does not fire IMPLEMENT

---

## B32-gap-018-002 — "throwaway prototype demo" → triple gap; zero predicates

A: "Just a throwaway prototype — delete it after the demo."
B: Predicates: [] | GapId: gap-vibe-coding-throwaway-001, gap-vibe-coding-prototype-001, gap-vibe-coding-demo-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted once

---

## B32-gap-018-003 — "throwaway + build interface" → IMPLEMENT + UI; gap warning

A: "Build a throwaway interface for the login flow."
B: Predicates: [IMPLEMENT, UI] | GapId: gap-vibe-coding-throwaway-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: "interface" is in UI lexicon → UI fires

---

## B32-gap-018-004 — "throwaway UI abbreviation alone" → zero predicates; UI abbrev not a trigger

A: "Throwaway UI for the admin panel — rough throwaway."
B: Predicates: [] | GapId: gap-vibe-coding-throwaway-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted | Note: "UI" abbreviation and "panel" are NOT UI lexicon triggers

---
