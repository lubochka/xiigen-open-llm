# Layer 0-B — Router Pairs (023)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-023-001 — Dashboard ≠ UI: [IMPLEMENT] + gap warning, NOT [IMPLEMENT, UI]

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-dashboard-001

**Request that produced this:** "Build a dashboard for the sales metrics."

**Router decision:**

- IMPLEMENT → ImplementationFlow
- UI NOT present (dashboard did not fire UI)
- gap-vibe-coding-dashboard-001 logged

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-dashboard-001]
```

**Corruption check:** Any output routing to UIFlow on "dashboard" alone is v6-02 corrupt.

---

## B32-0B-023-002 — Screen = UI: [IMPLEMENT, UI] — screen fires UI

**Predicates received:** `["IMPLEMENT", "UI"]`

**Gap context:** none

**Request that produced this:** "Build a screen for the sales metrics."

**Router decision:**

- IMPLEMENT → ImplementationFlow
- UI → UIFlow (screen fired UI correctly)

**Output:**
```
RouteTarget: [ImplementationFlow, UIFlow]
FlowSelected: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: []
```

---

## B32-0B-023-003 — "dashboard screen" co-occurrence: [IMPLEMENT, UI] + gap-dashboard-001

**Predicates received:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-dashboard-001

**Router decision:**

- IMPLEMENT → ImplementationFlow; UI → UIFlow (from "screen")
- gap-vibe-coding-dashboard-001 logged separately

**Output:**
```
RouteTarget: [ImplementationFlow, UIFlow]
FlowSelected: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-dashboard-001]
```

**Teaching:** "screen" fires UI; "dashboard" produces a gap warning. They are independent. Co-occurrence: both effects apply.

---

## B32-0B-023-004 — "panel" alone: NOT UI; no UI route selected

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** none (panel produces no gap ID)

**Request that produced this:** "Build a control panel for the system settings."

**Router decision:**

- IMPLEMENT → ImplementationFlow
- UI NOT present (panel is not in UI lexicon)

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: []
```

---
