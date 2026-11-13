# Layer 0-C — Knowledge-Edge Pairs (049)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-049-001 — "lean UI" alone: knowledge edge

**Knowledge edge: gap qualifier + UI noun, no action verb**

"Lean UI."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "UI" → noun; no action verb

Predicates: []
Route: knowledge_edge + SCL

---

## B32-0C-049-002 — "throw together a screen": UI fires

**Knowledge edge boundary: "screen" fires UI**

"Throw together a screen for the user profile."

Analysis:
- "throw together" → gap-vibe-coding-hack-001
- "screen" → UI fires ✓

Predicates: ["UI"]
Not knowledge_edge.

---

## B32-0C-049-003 — UI trigger summary

**Knowledge edge: what triggers UI predicate**

| Expression | UI fires? |
|---|---|
| "build a screen" | YES |
| "create a screen" | YES |
| "whip up an interface" | YES |
| "throw together a UI screen" | YES |
| "screen" alone (no verb) | NO |
| "UI" alone (no verb) | NO |
| "build the UI" (explicit "build" + "UI") | YES |
| "design the dashboard" | NO (dashboard → gap only) |

---

## B32-0C-049-004 — "build screen" fires both IMPLEMENT and UI

**Knowledge edge: "build screen" double-fires**

"Lean UI — build the settings screen."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "build" → IMPLEMENT fires ✓
- "settings screen" → UI fires ✓

Predicates: ["IMPLEMENT", "UI"]
Both predicates fire independently from the same verb-object construction.

---

## B32-0C-049-005 — "interface" fires UI like "screen"

**Knowledge edge: "interface" as UI trigger**

"Whip up a quick interface for settings."

Analysis:
- "whip up" → gap-vibe-coding-hack-001
- "interface" → UI fires ✓

Predicates: ["UI"]
Not knowledge_edge.

---
