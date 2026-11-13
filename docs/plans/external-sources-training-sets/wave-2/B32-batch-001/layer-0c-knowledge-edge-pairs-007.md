# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-007-001 — "vibe" / "vibe code" / "vibe coding": all gap forms

**Knowledge edge:** "vibe", "vibe code", and "vibe coding" are all gap vocabulary. No form of "vibe" fires a registered predicate.

### All gap forms of "vibe" vocabulary

| Term | Gap ID | Example usage |
|---|---|---|
| vibe | gap-vibe-coding-style-001 | "vibe it out" |
| vibe code | gap-vibe-coding-style-001 | "vibe code a service" |
| vibe coding | gap-vibe-coding-style-001 | "vibe coding session" |
| vibe out | gap-vibe-coding-style-001 | "vibe out a screen" |
| vibing | gap-vibe-coding-style-001 | "I'm vibing on this" |
| "in vibe mode" | gap-vibe-coding-style-001 | "in vibe mode today" |

All resolve to the same gap ID: gap-vibe-coding-style-001.

---

## B32-0C-007-002 — "vibe code" does NOT suppress "code" root match

**Critical rule:** In the phrase "vibe code":
- "vibe" → gap-vibe-coding-style-001 (gap warning)
- "code" → IMPLEMENT fires via "code" root match

Both effects occur simultaneously. The gap warning is emitted AND IMPLEMENT fires.

**Why this matters:**

"Vibe code a service" produces [IMPLEMENT] not []. A developer asking to "vibe code" something is still asking to CODE something. The "vibe" is the style; "code" is the action.

This is different from "vibe out a service":
- "vibe" → gap
- "out" → not an action predicate
- "service" → domain noun
- Result: [] (zero predicates; knowledge_edge)

---

## B32-0C-007-003 — Grammar variants of "vibe coding"

**As a noun phrase:** "A vibe coding session" → gap-vibe-coding-style-001; "session" not a predicate
**As a gerund:** "Vibe coding the service" → gap + IMPLEMENT (via "coding" root match)
**As an adjective:** "Vibe coding approach" → gap; "approach" not a predicate
**As a compound:** "vibe-code the module" → gap + IMPLEMENT

All variants emit gap-vibe-coding-style-001. When "code" or "coding" appears, IMPLEMENT also fires.

---

## B32-0C-007-004 — "vibe" is not a registered predicate in any scenario

**Across all XIIGen scenarios (B32, B36, B37, and others):**
- "vibe" is never in any predicate lexicon
- "vibe" never routes to any flow
- "vibe" always produces a gap warning
- "VIBE_CODING" is not a registered predicate ID anywhere

This is an invariant of the entire XIIGen system, not just B32.

---
