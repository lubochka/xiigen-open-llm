# Layer 2 — Capability Gap Extended Pairs (017)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-017-001 — "low-fi" alone → zero predicates → knowledge_edge

A: "Low-fi version of the onboarding flow — keep it low-fi."
B: Predicates: [] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted

---

## B32-gap-017-002 — "low-fi prototype" → double gap; zero predicates

A: "Just a low-fi prototype for now."
B: Predicates: [] | GapId: gap-vibe-coding-style-001, gap-vibe-coding-prototype-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted once

---

## B32-gap-017-003 — "low-fi build + page" → IMPLEMENT + UI; gap warning

A: "Build a low-fi version of the settings page."
B: Predicates: [IMPLEMENT, UI] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---

## B32-gap-017-004 — "lo-fi implementation" → IMPLEMENT fires; lo-fi = low-fi same gap

A: "Lo-fi implementation — we can polish it later."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: lo-fi same gap ID as low-fi

---
