# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-023-001 — "dashboard" emits gap ID even without vibe framing

**Knowledge edge:** gap-vibe-coding-dashboard-001 fires on "dashboard" regardless of whether any other gap vocabulary is present. It is not conditional on vibe framing.

**Examples:**

| Request | Vibe framing? | Dashboard gap? | Other predicates? |
|---|---|---|---|
| "Build a dashboard" | No | Yes (gap-dashboard-001) | IMPLEMENT |
| "Vibe code a dashboard" | Yes | Yes (gap-dashboard-001 + style-001) | IMPLEMENT |
| "Create an analytics dashboard" | No | Yes | IMPLEMENT |
| "Dashboard for the admin" | No | Yes | none (no action verb) |

**Teaching:** "dashboard" always emits gap-vibe-coding-dashboard-001. This is unconditional.

---

## B32-0C-023-002 — "view" and "tab" are not UI lexicon triggers

**Edge case table:**

| Term | In UI lexicon? | Notes |
|---|---|---|
| screen | Yes | Fires UI |
| page | Yes | Fires UI |
| view | No | Not a UI trigger |
| tab | No | Not a UI trigger |
| panel | No | Not a UI trigger |
| dashboard | No | Produces gap-dashboard-001 |
| layout | Yes | Fires UI |
| component | Yes | Fires UI |
| form | Yes | Fires UI |

"View" and "tab" are frequently used as UI synonyms but are NOT in the registered lexicon. Requests with only "view" or "tab" do not fire UI.

---

## B32-0C-023-003 — "widget" and "modal" ARE in UI lexicon: confirm both

**Confirming the two least-common UI lexicon members:**

**widget:**
- "Build a notification widget" → IMPLEMENT + UI (widget fires UI)
- "Widget for the sidebar" (no action verb) → UI fires from "widget"

**modal:**
- "Create a confirmation modal" → IMPLEMENT + UI
- "Build the modal with the form" → IMPLEMENT + UI (both modal and form fire UI; deduplicates to single UI predicate)

Both "widget" and "modal" are confirmed UI lexicon members. Any B32 training output that omits them is incomplete.

---
