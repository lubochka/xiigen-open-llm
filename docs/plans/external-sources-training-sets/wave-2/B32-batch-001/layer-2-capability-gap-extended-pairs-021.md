# Layer 2 — Capability Gap Extended Pairs (021)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-021-001 — "vibe code" contains code root → IMPLEMENT fires (not pure gap)

A: "Vibe code the user profile service."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: "code" root fires IMPLEMENT; "vibe" produces gap warning

---

## B32-gap-021-002 — "vibe out" has NO code root → zero predicates without other action verb

A: "Vibe out a dashboard prototype."
B: Predicates: [] | GapId: gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-prototype-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted | Note: "vibe out" ≠ "vibe code"; no IMPLEMENT without code root

---

## B32-gap-021-003 — "vibe coding + screen" → IMPLEMENT + UI; gap warning

A: "Vibe coding a screen for the user profile."
B: Predicates: [IMPLEMENT, UI] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted

---

## B32-gap-021-004 — "vibe coding + dashboard" → IMPLEMENT only; dashboard NOT UI

A: "Vibe coding a dashboard for the analytics."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: dashboard does NOT fire UI; gap-dashboard-001 is separate

---

## B32-gap-021-005 — "vibe" alone + screen: UI fires; IMPLEMENT absent (no code root)

A: "Just vibe the user registration screen."
B: Predicates: [UI] | GapId: gap-vibe-coding-style-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: "vibe" alone ≠ "vibe code"; screen fires UI independently

---
