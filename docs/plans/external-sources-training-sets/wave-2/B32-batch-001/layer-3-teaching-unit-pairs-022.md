# Layer 3 — Teaching Unit Pairs (022)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-022-001 — The dashboard ≠ UI rule: full teaching unit

**Teaching unit:** "Dashboard" is NOT in the UI lexicon. This is a deliberate design decision. The UI predicate fires on structural UI primitives. "Dashboard" refers to a business-level display concept that often requires IMPLEMENT and ARCHITECTURE, not just UI.

**UI lexicon (complete):** screen, page, interface, component, form, layout, button, input, widget, modal

**"Dashboard" is NOT in this list.** It produces gap-vibe-coding-dashboard-001 and never fires UI.

---

## B32-L3-022-002 — Examples: dashboard vs screen

| Request | Dashboard present? | Screen present? | Predicates | Notes |
|---|---|---|---|---|
| "Build a dashboard" | Yes | No | [IMPLEMENT] + gap-dashboard-001 | No UI |
| "Build a screen" | No | Yes | [IMPLEMENT, UI] | No gap |
| "Build a dashboard screen" | Yes | Yes | [IMPLEMENT, UI] + gap-dashboard-001 | Both effects |
| "Dashboard prototype" | Yes | No | [] + gap-dashboard-001 + gap-prototype-001 | No action verb |

---

## B32-L3-022-003 — What "dashboard" being gap means for the learning pipeline

**When gap-vibe-coding-dashboard-001 fires:**

The learning pipeline receives:
```
{
  gapId: "gap-vibe-coding-dashboard-001",
  unrecognizedTerm: "dashboard",
  domainId: "software-development",
  action: "SCHEDULE_CAPABILITY_LEARNING"
}
```

This signals that users frequently request dashboards but the system lacks a recognized routing path for pure dashboard requests. The learning pipeline may consider whether to:
1. Add "dashboard" to the UI lexicon
2. Create a new DASHBOARD predicate
3. Keep it as gap vocabulary indefinitely

Until a resolution is made, "dashboard" remains gap vocabulary producing gap-vibe-coding-dashboard-001.

---
