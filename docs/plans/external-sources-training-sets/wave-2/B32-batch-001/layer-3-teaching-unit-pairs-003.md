# Layer 3 — Teaching Unit Pairs (003)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-003-001 — "dashboard" not in UI lexicon: complete rule

**Teaching unit:** The word "dashboard" does not appear in the UI predicate lexicon. Requests using "dashboard" as the only UI-like term will produce zero predicates for UI. The word is recognized as a gap term and emits gap-vibe-coding-dashboard-001.

**Core rule:**
```
UI fires on: screen, page, interface, component, form, layout, button, input, widget, modal
UI does NOT fire on: dashboard, panel, view, sidebar, navbar, header, footer, card
```

---

## B32-L3-003-002 — Complete UI lexicon list for reference

**Registered UI predicate triggers (10 tokens):**

1. screen — a display area or page of a digital interface
2. page — a web page or application screen
3. interface — a user interface or API interface (context-dependent)
4. component — a reusable UI component (React component, Vue component, etc.)
5. form — an input form for data collection
6. layout — the spatial arrangement of UI elements
7. button — an interactive control element
8. input — an input field or control
9. widget — a self-contained UI element
10. modal — a dialog box or modal overlay

**Non-UI trigger terms (gap or unregistered):**

| Term | Status |
|---|---|
| dashboard | gap-vibe-coding-dashboard-001 |
| panel | not registered (UI candidate) |
| view | not registered |
| sidebar | not registered |
| card | not registered |
| tooltip | not registered |
| dropdown | not registered |
| navbar | not registered |
| header / footer | not registered |

---

## B32-L3-003-003 — Teaching point: why "dashboard" is not in the UI lexicon

**Reason 1:** "Dashboard" is a composite concept. A dashboard is built from screens, components, charts, and layouts — all of which may be in the UI lexicon. A dashboard itself is not a primitive UI element.

**Reason 2:** "Dashboard" is ambiguous in some contexts (e.g., a car dashboard, a project management dashboard, a monitoring dashboard). Keeping it out of the UI lexicon avoids false positives in non-UI contexts.

**Implication:** A user asking to "build a dashboard" should include at least one registered UI term (screen, component, form, etc.) to trigger the UI predicate. Otherwise, the request routes on IMPLEMENT alone (if "build" is present) or routes to knowledge_edge.

---

## B32-L3-003-004 — Practice examples: dashboard vs registered UI terms

| Request | UI term | UI fires? |
|---|---|---|
| "Build a dashboard" | dashboard | No → gap-vibe-coding-dashboard-001 |
| "Build a main screen for the dashboard" | screen | Yes → UI fires |
| "Create a dashboard with charts and components" | component | Yes → UI fires |
| "Build a dashboard interface" | interface | Yes → UI fires |
| "Build a dashboard panel" | panel | No → not registered |
| "Build a dashboard page" | page | Yes → UI fires |

---
