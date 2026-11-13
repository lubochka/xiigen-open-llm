# Layer 0-C — Knowledge-Edge Pairs (042)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-042-001 — "think about security": knowledge edge

**Knowledge edge: cognitive phrase does not fire SECURITY**

"Lean approach — think about security."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "think about" → cognitive phrase; not in SECURITY lexicon
- "security" → noun object

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-042-002 — gap + IMPLEMENT + SECURITY: fires, not knowledge edge

**Knowledge edge boundary: two predicates fire**

"Lean sprint — implement auth flow and secure tokens."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "secure" → SECURITY fires ✓

Predicates: ["IMPLEMENT", "SECURITY"]
Not knowledge_edge.

---

## B32-0C-042-003 — SECURITY trigger verb review

**Knowledge edge: SECURITY fires from "secure" imperative**

| Expression | SECURITY fires? |
|---|---|
| "secure the endpoint" | YES |
| "secure the tokens" | YES |
| "secure the login form" | YES |
| "think about security" | NO |
| "add some security" | NO (ambiguous; "add" doesn't map to SECURITY) |
| "we need security" | NO |
| "audit the security" | YES ("audit" as security verb) |

---

## B32-0C-042-004 — "hack together" gap: different gap ID from "lean/pragmatic"

**Knowledge edge: gap ID differences matter**

"Hack together a security fix — secure the login form."

Analysis:
- "hack together" → gap-vibe-coding-hack-001 (NOT gap-vibe-coding-style-001)
- "secure" → SECURITY fires ✓

GapWarnings: [gap-vibe-coding-hack-001] (not style-001)

This distinction matters: the gap ID in the warning reflects the actual term used.

---

## B32-0C-042-005 — ARCHITECTURE + SECURITY: TransitionType determination

**Knowledge edge: ARCHITECTURE + SECURITY predicate combination**

Predicates: ["ARCHITECTURE", "SECURITY"]
- ARCHITECTURE → documentation
- SECURITY → no_contribution
- No IMPLEMENT or UI present

TransitionType: documentation (ARCHITECTURE is the primary content-generating predicate when IMPLEMENT/UI absent)

---
