# Layer 0-A — Compiler Pairs (023)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-023-001 — "dashboard" does NOT fire UI; "screen" DOES fire UI

**Request A:** "Build a dashboard for the sales metrics."
**Request B:** "Build a screen for the sales metrics."

**Compiler analysis — Request A:**

- "build" → IMPLEMENT fires
- "dashboard" → NOT in UI lexicon → gap-vibe-coding-dashboard-001

**Result A:** `["IMPLEMENT"]` + GapWarnings: [gap-vibe-coding-dashboard-001]

**Compiler analysis — Request B:**

- "build" → IMPLEMENT fires
- "screen" → UI fires (screen is in UI lexicon)

**Result B:** `["IMPLEMENT", "UI"]` — no gap warnings

**Boundary rule confirmed:** dashboard → gap (not UI). screen → UI (not gap).

**DomainId:** "software-development"

---

## B32-0A-023-002 — "dashboard screen" — "screen" fires UI; "dashboard" remains gap

**Request:** "Build a dashboard screen for the admin users."

**Compiler analysis:**

- "build" → IMPLEMENT fires
- "dashboard" → NOT in UI lexicon → gap-vibe-coding-dashboard-001
- "screen" → UI fires

**Result:** `["IMPLEMENT", "UI"]`

**GapWarnings:** gap-vibe-coding-dashboard-001

**Key teaching:** When "dashboard" and "screen" co-occur, "screen" fires UI and "dashboard" produces a gap warning. The presence of "screen" does not absorb "dashboard" into the UI predicate.

**OutputDecision:** route to IMPLEMENT + UI flows

**DomainId:** "software-development"

---

## B32-0A-023-003 — "dashboard page" — "page" fires UI; "dashboard" remains gap

**Request:** "Create the analytics dashboard page."

**Compiler analysis:**

- "create" → IMPLEMENT fires
- "dashboard" → gap-vibe-coding-dashboard-001 (not UI)
- "page" → UI fires

**Result:** `["IMPLEMENT", "UI"]`

**GapWarnings:** gap-vibe-coding-dashboard-001

**OutputDecision:** route to IMPLEMENT + UI flows

**DomainId:** "software-development"

---

## B32-0A-023-004 — "panel" alone: NOT in UI lexicon; no UI predicate

**Request:** "Build a control panel for the system settings."

**Compiler analysis:**

- "build" → IMPLEMENT fires
- "panel" → NOT in UI lexicon (confirmed non-member)
- "control panel" → not in lexicon as a phrase

**Result:** `["IMPLEMENT"]`

**GapWarnings:** none

**Note:** "panel" is not a UI trigger. Neither is "control panel". Only: screen, page, interface, component, form, layout, button, input, widget, modal.

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-023-005 — "widget" and "modal" are UI lexicon; "sidebar" is not

**Request:** "Build the notification modal with a sidebar widget."

**Compiler analysis:**

- "build" → IMPLEMENT fires
- "modal" → UI fires (modal is in UI lexicon)
- "widget" → UI fires (widget is in UI lexicon)
- "sidebar" → NOT in UI lexicon

**Result:** `["IMPLEMENT", "UI"]`

**GapWarnings:** none

**Note:** "sidebar" is not a UI trigger even though it is a UI element. Only registered UI lexicon terms fire the predicate.

**OutputDecision:** route to IMPLEMENT + UI flows

**DomainId:** "software-development"

---
