# Layer 2 — Capability Gap Extended Pairs (015)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-015-001 — "quick win" alone → zero predicates

A: "We need a quick win here — just a quick win for the team."
B: Predicates: [] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted

---

## B32-gap-015-002 — "quick win + build" → IMPLEMENT fires; gap warning; style noted

A: "Build a quick win feature — something we can ship fast."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---

## B32-gap-015-003 — "quick win + implement" → IMPLEMENT fires; gap warning

A: "Implement a quick win for the reporting module."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---

## B32-gap-015-004 — "quick win + document" → DOCUMENT fires; style is warning only

A: "Document this quick win so the team knows how it works."
B: Predicates: [DOCUMENT] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---

## B32-gap-015-005 — "quick win + test" → TEST fires; style is warning only

A: "Test this quick win before we merge it."
B: Predicates: [TEST] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---
