# Layer 0-C — Knowledge-Edge Pairs (054)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-054-001 — "quick" alone: knowledge edge

**Knowledge edge: "quick" without action verb**

"Quick solution."

Analysis:
- "quick" → gap-vibe-coding-style-001 (quick-win framing)
- "solution" → noun; no action verb

Predicates: []
Route: knowledge_edge + SCL

---

## B32-0C-054-002 — "just enough" alone: knowledge edge

**Knowledge edge: sufficiency framing without action verb**

"Just enough."

Analysis:
- "just enough" → gap-vibe-coding-style-001
- No action verb

Predicates: []
Route: knowledge_edge + SCL

---

## B32-0C-054-003 — all six gap IDs in one request: knowledge edge

**Knowledge edge: six gap families, no action verb**

"Lean spike prototype hack-together rough-cut dashboard."

Analysis:
- "lean" → style-001
- "spike" → spike-001
- "prototype" → prototype-001
- "hack-together" → hack-001
- "rough-cut" → rough-cut-001
- "dashboard" → dashboard-001

Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-prototype-001, gap-vibe-coding-hack-001, gap-vibe-coding-rough-cut-001, gap-vibe-coding-dashboard-001]
Route: knowledge_edge + SCL (once)

---

## B32-0C-054-004 — complete B32 knowledge-edge trigger reference

**Any of the following alone (no action verb) → knowledge_edge + SCL:**

Style adjectives, vibe phrases, hack phrases without specific artifact, prototype/PoC vocabulary, spike vocabulary, rough-cut/rough-draft, dashboard alone.

All → predicates = [] → knowledge_edge → SCL (once).

---

## B32-0C-054-005 — knowledge-edge exceptions (things that look like qualifiers but fire predicates)

**Not knowledge_edge despite informal framing:**

| Expression | Fires | Reason |
|---|---|---|
| "vibe code the feature" | IMPLEMENT | "code" root |
| "hack together the module" | IMPLEMENT | Specific software artifact |
| "bang out tests" | TEST | "tests" as object |
| "knock out the implementation" | IMPLEMENT | "implementation" noun |
| "whip up an interface" | UI | "interface" → UI |

These fire predicates despite informal vocabulary because of specific object or action words.

---
