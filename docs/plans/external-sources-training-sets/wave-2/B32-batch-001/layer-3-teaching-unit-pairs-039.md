# Layer 3 — Teaching Unit Pairs (039)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-039-001 — Teaching: informal + UI patterns

**Teaching unit:** Hack phrases that fire UI

"Throw together a screen" → UI fires (screen → UI predicate).
"Whip up an interface" → UI fires (interface → UI predicate).
"Hack together a screen" → UI fires.

In each case: hack phrase → gap-vibe-coding-hack-001 + action verb/object context → UI fires.

---

## B32-L3-039-002 — Teaching: "build screen" fires IMPLEMENT + UI

**Teaching unit:** "build [screen/interface]" double-fires

When "build" (or create, implement) is used with "screen" or "interface":
- IMPLEMENT fires from "build"
- UI fires from "screen"/"interface"

Both predicates are in the output. Neither suppresses the other.

---

## B32-L3-039-003 — Teaching: dashboard ≠ UI in any combination

**Teaching unit:** Dashboard invariant (Rule 4)

"Dashboard" never fires UI, regardless of context:
- "build the dashboard" → IMPLEMENT fires; dashboard → gap-vibe-coding-dashboard-001
- "design the dashboard" → ARCHITECTURE fires; dashboard → gap
- "create the dashboard screen" → IMPLEMENT + UI fire; dashboard → gap (screen also present)

The dashboard gap ID always appears in GapWarnings when "dashboard" is in scope.

---

## B32-L3-039-004 — Teaching: UI fires from "UI" noun with action verb

**Teaching unit:** UI predicate from explicit "UI" noun

"Build the UI" → UI fires (IMPLEMENT also fires from "build").
"Create the UI" → UI fires.
"Lean UI" (no verb) → UI does NOT fire.

"UI" as noun requires an action verb to trigger the UI predicate.

---
