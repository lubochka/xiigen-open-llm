# Layer 2 — Capability Gap Extended Pairs (004)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-004-001 — "dashboard" vs "screen": full boundary trace

**Case A: "dashboard" request (zero predicates)**

**Request A:** "Throw together a quick dashboard for the admin panel — nothing fancy."

**Layer 0-A trace:**
```
Token scan:
- "throw together" → gap-vibe-coding-hack-001 (NOT an action predicate)
- "dashboard" → NOT in UI lexicon → gap-vibe-coding-dashboard-001
- "admin panel" → "panel" NOT in UI lexicon → no UI predicate
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001
- No action verbs from IMPLEMENT lexicon
Predicates: []
GapWarnings: [hack-001, dashboard-001, nothing-fancy-001]
```

**Layer 0-B trace:**
```
RouteTarget: knowledge_edge
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: emit
```

**Result A:** knowledge_edge

---

**Case B: "screen" request (UI fires)**

**Request B:** "Throw together a quick screen for the user list — nothing fancy."

**Layer 0-A trace:**
```
Token scan:
- "throw together" → gap-vibe-coding-hack-001
- "screen" → IN UI lexicon → UI fires ✓
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001
- No action verbs → IMPLEMENT does not fire
Predicates: ["UI"]
GapWarnings: [hack-001, nothing-fancy-001]
```

**Layer 0-B trace:**
```
RouteTarget: UIFlow
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates non-empty)
```

**Result B:** [UI] → UIFlow

---

## B32-L2-004-002 — Boundary analysis: dashboard vs screen

| Property | Dashboard (Case A) | Screen (Case B) |
|---|---|---|
| UI lexicon match | No (dashboard not in lexicon) | Yes (screen is in lexicon) |
| Predicates | [] | ["UI"] |
| Route | knowledge_edge | UIFlow |
| Gap warning | gap-vibe-coding-dashboard-001 | none for "screen" |
| SCHEDULE_CAPABILITY_LEARNING | Yes | No |
| DomainId | software-development | software-development |

**The boundary is the UI lexicon.** "screen" is registered; "dashboard" is not. The user intent is similar (both want a visual interface), but only "screen" triggers the UI predicate.

---

## B32-L2-004-003 — Extended UI lexicon analysis in vibe context

**All 10 registered UI tokens fire UI predicate:**

When any of these appears in a vibe-framed request, UI fires regardless of the vibe framing:

screen, page, interface, component, form, layout, button, input, widget, modal

**UI lexicon gap candidates (commonly requested but not registered):**
- dashboard → gap-vibe-coding-dashboard-001
- panel → not registered (candidate for ui-panel-001)
- view → not registered
- sidebar → not registered
- card → not registered

These should be added to the learning pipeline as UI vocabulary extension candidates.

---
