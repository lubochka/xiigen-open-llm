# Layer 0-C — Knowledge-Edge Pairs (048)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-048-001 — "hack together" alone: knowledge edge

**Knowledge edge: hack phrase with vague object**

"Let's hack something together."

Analysis:
- "hack together" → gap-vibe-coding-hack-001
- "something" → vague; not in predicate lexicon

Predicates: []
GapWarnings: [gap-vibe-coding-hack-001]
Route: knowledge_edge + SCL

---

## B32-0C-048-002 — "cobble together" alone: knowledge edge

**Knowledge edge: cobble together with no action object**

"We'll cobble it together."

Analysis:
- "cobble together" → gap-vibe-coding-hack-001
- "it" → pronoun object; no predicate

Predicates: []
GapWarnings: [gap-vibe-coding-hack-001]
Route: knowledge_edge + SCL

---

## B32-0C-048-003 — "hack together" + IMPLEMENT: fires, not knowledge edge

**Knowledge edge boundary:**

"Hack it together — implement the report exporter."

Analysis:
- "hack it together" → gap-vibe-coding-hack-001
- "implement" → IMPLEMENT fires ✓

Predicates: ["IMPLEMENT"]
Not knowledge_edge.

---

## B32-0C-048-004 — "hack together X" with module scope fires IMPLEMENT

**Knowledge edge: "hack together" + module noun fires IMPLEMENT**

"Hack together the auth module."

Analysis:
- "hack together" → gap-vibe-coding-hack-001
- "auth module" → IMPLEMENT fires ✓ (module as implementation scope; "hack together [a module/service/handler]" fires IMPLEMENT)

Predicates: ["IMPLEMENT"]
Not knowledge_edge.

Note: "hack together [a vague thing]" does not fire IMPLEMENT. "Hack together [a specific software artifact]" fires IMPLEMENT.

---

## B32-0C-048-005 — informal IMPLEMENT phrase context-sensitivity summary

| Expression | IMPLEMENT fires? | Reason |
|---|---|---|
| "hack together a module" | YES | Specific software artifact |
| "hack together something" | NO | Vague object |
| "cobble together the service" | YES | Specific software artifact |
| "throw together a screen" | NO (UI fires) | Screen → UI predicate |
| "bang out an implementation" | YES | "Implementation" noun |
| "whip up something" | NO | Vague object |
| "wing it" | NO | No object |

---
