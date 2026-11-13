# Layer 2 — Capability Gap Extended Pairs (013)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-013-001 — "scrappy" alone → zero predicates → knowledge_edge

A: "Just something scrappy — we need a scrappy solution here."
B: Predicates: [] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted; no CapabilityGapWarning (predicates empty)

---

## B32-gap-013-002 — "scrappy build" → IMPLEMENT fires; gap warning added

A: "Do a scrappy build of the notification service."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | SCHEDULE_CAPABILITY_LEARNING NOT emitted

---

## B32-gap-013-003 — "scrappy prototype" alone → double gap; zero predicates

A: "A scrappy prototype is fine — nothing polished."
B: Predicates: [] | GapId: gap-vibe-coding-style-001, gap-vibe-coding-prototype-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted once (not twice)

---

## B32-gap-013-004 — "scrappy implementation" → IMPLEMENT fires via root match

A: "We need a scrappy implementation of the auth flow — nothing fancy."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---

## B32-gap-013-005 — "scrappy design" → ARCHITECTURE fires; style qualifier noted

A: "Quick scrappy design for the system architecture."
B: Predicates: [ARCHITECTURE] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---
