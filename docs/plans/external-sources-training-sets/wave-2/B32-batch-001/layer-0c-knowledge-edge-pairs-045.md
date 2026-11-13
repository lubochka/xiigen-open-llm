# Layer 0-C — Knowledge-Edge Pairs (045)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-045-001 — "whip up" fires IMPLEMENT via "implementation" noun

**Knowledge edge boundary: noun form triggers predicate**

"Whip up a quick implementation of the search."

Analysis:
- "whip up" → gap-vibe-coding-hack-001
- "implementation" → IMPLEMENT fires ✓ (noun form of implement; "implementation" is in IMPLEMENT lexicon)

Predicates: ["IMPLEMENT"]
Not knowledge_edge.

---

## B32-0C-045-002 — "wing it" alone: knowledge edge

**Knowledge edge: informal improvisation phrase**

"Just wing it."

Analysis:
- "wing it" → gap-vibe-coding-style-001
- No action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-045-003 — "throw together" + screen = UI fires: knowledge edge boundary

**Knowledge edge: "screen" fires UI**

"Throw together a UI for the settings screen."

Analysis:
- "throw together" → gap-vibe-coding-hack-001
- "settings screen" → UI fires ✓ ("screen" → UI predicate)

Predicates: ["UI"]
Not knowledge_edge.

---

## B32-0C-045-004 — "dashboard" + "screen" co-occurrence: both effects apply

**Knowledge edge: dashboard gap + screen UI**

"Throw together a UI for the dashboard settings screen."

Analysis:
- "throw together" → gap-vibe-coding-hack-001
- "dashboard" → gap-vibe-coding-dashboard-001 (adds gap ID)
- "settings screen" → UI fires ✓

Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001]

"Dashboard" adds its gap ID; "screen" fires UI. Both effects are independent.

---

## B32-0C-045-005 — informal IMPLEMENT verb forms summary

| Expression | IMPLEMENT fires? |
|---|---|
| "implement X" | YES |
| "build X" | YES |
| "create X" | YES |
| "write X" (code/feature) | YES |
| "code X" | YES |
| "whip up X" | depends on object: "implementation" YES, "idea" NO |
| "implementation of X" (with "whip up") | YES |
| "knock out the implementation" | YES |
| "get it working" | NO (vibe phrase) |
| "wing it" | NO |

---
