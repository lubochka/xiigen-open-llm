# Layer 0-B — Router Pairs (021)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-021-001 — [IMPLEMENT, UI] → dual flow: "vibe code a screen"

**Predicates received:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-style-001 ("vibe")

**Router decision:**

- IMPLEMENT → ImplementationFlow
- UI → UIFlow
- Both selected in parallel
- Gap warning logged; does not suppress either flow

**Output:**
```
RouteTarget: [ImplementationFlow, UIFlow]
FlowSelected: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-021-002 — [UI] → UI flow: "vibe out a screen" (no IMPLEMENT since no "code" root)

**Predicates received:** `["UI"]`

**Gap context:** gap-vibe-coding-style-001 ("vibe")

**Router decision:**

- UI → UIFlow
- IMPLEMENT absent (no "code" root; "vibe out" does not trigger IMPLEMENT)
- Gap warning logged

**Output:**
```
RouteTarget: UIFlow
FlowSelected: UIFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-021-003 — [IMPLEMENT, UI] → dual flow: "vibe coding a modal"

**Predicates received:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-style-001

**Router decision:**

- IMPLEMENT → ImplementationFlow; UI → UIFlow
- "modal" fires UI; "coding" fires IMPLEMENT

**Output:**
```
RouteTarget: [ImplementationFlow, UIFlow]
FlowSelected: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0B-021-004 — [IMPLEMENT] only: "vibe code a dashboard" — dashboard NOT UI

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001

**Router decision:**

- IMPLEMENT → ImplementationFlow
- UI NOT present (dashboard not in UI lexicon; UI predicate did not fire)
- Two gap warnings noted

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
CapabilityGapWarning: style_qualifier_noted
```

---
