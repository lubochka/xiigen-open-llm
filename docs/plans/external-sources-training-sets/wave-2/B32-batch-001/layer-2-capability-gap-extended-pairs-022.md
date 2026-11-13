# Layer 2 — Capability Gap Extended Pairs (022)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-022-001 — "dashboard alone" → zero predicates; NOT UI

A: "Dashboard for the admin users."
B: Predicates: [] | GapId: gap-vibe-coding-dashboard-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted | Note: "dashboard" alone has no action verb and does NOT fire UI

---

## B32-gap-022-002 — "dashboard + build" → IMPLEMENT fires; dashboard NOT UI

A: "Build a dashboard for the sales metrics."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-dashboard-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: UI does NOT fire; dashboard is gap only

---

## B32-gap-022-003 — "screen + build" → IMPLEMENT + UI; NO gap warning

A: "Build a screen for the sales metrics."
B: Predicates: [IMPLEMENT, UI] | GapId: none | DomainId: software-development
   GapWarning: none | Note: "screen" fires UI; no gap vocabulary present; no warning

---

## B32-gap-022-004 — "dashboard screen + build" → IMPLEMENT + UI + gap-dashboard-001

A: "Build a dashboard screen for the admin users."
B: Predicates: [IMPLEMENT, UI] | GapId: gap-vibe-coding-dashboard-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: "screen" fires UI; "dashboard" separately produces gap warning; both effects apply

---
