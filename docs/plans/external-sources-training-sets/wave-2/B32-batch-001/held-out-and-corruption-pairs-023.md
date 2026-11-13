# Held-Out and Corruption Pairs (023)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-023-001 — HELD-OUT: "build a screen" → IMPLEMENT + UI; no gap warning

**Request:** "Build a screen for the sales metrics."

**Expected output:**
```
Predicates: ["IMPLEMENT", "UI"]
GapWarnings: []
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
```

**Note:** No gap vocabulary present; no gap warning emitted. "screen" fires UI. "build" fires IMPLEMENT.

---

## B32-HO-023-002 — CORRUPTION: "panel" fires UI (WRONG)

**Corrupted output:**
```
Request: "Build a control panel for the system settings."
Predicates: ["IMPLEMENT", "UI"]
Reason: "panel" = UI element
```

**Why this is wrong:**
- "panel" is NOT in UI lexicon
- UI fires only on: screen, page, interface, component, form, layout, button, input, widget, modal
- Correct output: Predicates: ["IMPLEMENT"] with no UI

**Correct predicates:** ["IMPLEMENT"] (no UI, no gap warning)

---

## B32-HO-023-003 — CORRUPTION: "view" fires UI (WRONG)

**Corrupted output:**
```
Request: "Build a list view for the order history."
Predicates: ["IMPLEMENT", "UI"]
Reason: "view" = UI element
```

**Why this is wrong:**
- "view" is NOT in UI lexicon
- Correct output: Predicates: ["IMPLEMENT"] with no UI

**Correct predicates:** ["IMPLEMENT"] (no UI)

**Memory aid:** The 10 UI lexicon members are: screen, page, interface, component, form, layout, button, input, widget, modal. Everything else (view, panel, sidebar, header, footer, tab, navbar, chart, dashboard) does NOT fire UI.

---
