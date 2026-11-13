# Layer 2 — Capability Gap Extended Pairs (014)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-014-001 — "rough cut" alone → zero predicates → knowledge_edge

A: "Just a rough cut of the pipeline — rough cut is fine."
B: Predicates: [] | GapId: gap-vibe-coding-rough-cut-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted

---

## B32-gap-014-002 — "rough cut build" → IMPLEMENT fires; gap warning

A: "Build a rough cut of the data ingestion module."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-rough-cut-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---

## B32-gap-014-003 — "rough cut design" → ARCHITECTURE fires; gap warning

A: "Rough cut design of the event-driven architecture."
B: Predicates: [ARCHITECTURE] | GapId: gap-vibe-coding-rough-cut-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---

## B32-gap-014-004 — "rough-cut implementation" → IMPLEMENT fires; hyphenated form same gap

A: "A rough-cut implementation is acceptable for the MVP."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-rough-cut-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: rough-cut (hyphenated) = same gap as rough cut (space)

---

## B32-gap-014-005 — "rough cut screen build" → IMPLEMENT + UI; gap warning

A: "Rough cut of the checkout screen — doesn't need to be polished."
B: Predicates: [UI] | GapId: gap-vibe-coding-rough-cut-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: no action verb → only "screen" fires UI; no IMPLEMENT

---
