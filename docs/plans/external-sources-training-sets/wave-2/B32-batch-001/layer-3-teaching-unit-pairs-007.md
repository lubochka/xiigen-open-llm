# Layer 3 — Teaching Unit Pairs (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-007-001 — "vibe", "vibe code", "vibe coding": all gap forms with grammar variations

**Teaching unit:** All forms of the "vibe" vocabulary are gap. There is no form of "vibe" that fires a registered predicate. However, when "code" appears as part of "vibe code" or "vibe coding", the "code" root still fires IMPLEMENT independently.

**Core rule:**
```
"vibe" (any form) → gap-vibe-coding-style-001
"code" (root, even in compound "vibe code") → IMPLEMENT fires
These are independent: vibe always gaps; code always predicate-fires
```

---

## B32-L3-007-002 — Grammar variations of "vibe" and their effects

| Form | Grammatical type | Gap ID | IMPLEMENT? |
|---|---|---|---|
| "vibe" alone | verb/noun | style-001 | No |
| "vibe code" | compound verb | style-001 | Yes (code root) |
| "vibe coding" | gerund phrase | style-001 | Yes (coding→code root) |
| "vibe out" | phrasal verb | style-001 | No ("out" not in lexicon) |
| "vibing" | gerund | style-001 | No ("vibing"→"vibe" root, not in lexicon) |
| "in vibe mode" | prepositional phrase | style-001 | No |
| "creative mode" | noun phrase | style-001 | No |
| "vibe session" | noun phrase | style-001 | No |
| "vibe design" | compound phrase | style-001 | No ("design" → ARCHITECTURE fires) |

---

## B32-L3-007-003 — Key distinction: "vibe code" vs "vibe out"

**"vibe code a screen":**
- "vibe" → gap
- "code" → IMPLEMENT fires (root match)
- "screen" → UI fires
- Result: [UI, IMPLEMENT] + gap warning

**"vibe out a screen":**
- "vibe" → gap
- "out" → NOT in any lexicon; no predicate
- "screen" → UI fires
- Result: [UI] + gap warning

The difference is "code" vs "out". "Code" is in the IMPLEMENT lexicon; "out" is not.

---

## B32-L3-007-004 — Teaching point: "vibe" is always gap; "code" root is always predicate

**The invariant:**
```
"vibe" token → gap-vibe-coding-style-001 (always, in all contexts)
"code" token → IMPLEMENT (always, via root match, in any context)
"vibe code" compound → BOTH effects simultaneously
```

This is not a contradiction. "Vibe" and "code" are parsed independently. "Vibe" feeds the gap channel. "Code" feeds the predicate channel. The compound activates both channels.

---
