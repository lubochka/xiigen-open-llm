# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-018-001 — "throwaway" with UI vocabulary: the mix case

**Knowledge edge:** "throwaway" is gap vocabulary. If a UI lexicon term co-occurs, UI fires normally.

**Request:** "Build a throwaway interface for the login flow."

- "throwaway" → gap-vibe-coding-throwaway-001
- "build" → IMPLEMENT fires
- "interface" → UI fires (interface is in UI lexicon)

**Result:** `["IMPLEMENT", "UI"]` + GapWarnings: [gap-vibe-coding-throwaway-001]

**Teaching:** Throwaway framing does not suppress UI. If a UI lexicon term is present, UI fires regardless of throwaway/scrappy/vibe framing.

---

## B32-0C-018-002 — "throwaway" alone vs "throwaway write": the gap vs predicate split

**Gap case (no action verb):**

"Throwaway code — just throwaway, we'll rewrite later."

- "throwaway" → gap-vibe-coding-throwaway-001
- Future "rewrite" does not fire IMPLEMENT (future-frame intent, not present imperative)
- Result: [] + gap-vibe-coding-throwaway-001 → knowledge_edge

**Predicate case (action verb present):**

"Write throwaway code for the data migration."

- "throwaway" → gap-vibe-coding-throwaway-001
- "write" → IMPLEMENT fires (present imperative)
- Result: [IMPLEMENT] + gap-vibe-coding-throwaway-001

---

## B32-0C-018-003 — "throwaway prototype demo": triple gap, zero predicates

**Request:** "Just a throwaway prototype — delete it after the demo."

- "throwaway" → gap-vibe-coding-throwaway-001
- "prototype" alone → gap-vibe-coding-prototype-001
- "demo" standalone → gap-vibe-coding-demo-001

**Result:** `[]` + three gap IDs

All three are gap vocabulary with no action verb. SCHEDULE_CAPABILITY_LEARNING emitted once.

**Teaching:** Stacking gap vocabulary multiplies gap IDs but does not create predicates. The output remains knowledge_edge.

---
