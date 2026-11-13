# Layer 3 — Teaching Unit Pairs (018)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-018-001 — "throwaway" describes intent, not action: complete rule

**Teaching unit:** "Throwaway" communicates that the code is disposable and expected to be discarded. It does not describe what to do with the code right now.

**Core rule:**
```
"throwaway" alone (or with qualifiers but no action verb) → gap-vibe-coding-throwaway-001 → zero predicates
"throwaway" + action verb → predicate fires + gap-throwaway-001 warning
```

---

## B32-L3-018-002 — The future-tense "rewrite" trap

**Request:** "Throwaway code — just throwaway, we'll rewrite later."

- "throwaway" → gap-vibe-coding-throwaway-001
- "rewrite later" → future tense; the rewrite is not being requested now

**Why "rewrite later" does not fire IMPLEMENT:**
The sentence "we'll rewrite later" states the user's future intent, not a current imperative action. The compiler looks for present-tense imperative action verbs describing what to do now.

**Correct output:** predicates = [] → knowledge_edge

**Contrast:** "Write throwaway code for the data migration" → "write" is present imperative → IMPLEMENT fires.

---

## B32-L3-018-003 — "throwaway + UI vocabulary" mix: UI fires independently

**Request:** "Build a throwaway interface for the login flow."

- "throwaway" → gap-vibe-coding-throwaway-001
- "build" → IMPLEMENT fires
- "interface" → UI fires (interface is in UI lexicon)

**Result:** [IMPLEMENT, UI] + gap-throwaway-001

**Teaching:** Throwaway framing does not suppress UI predicate. "Interface" is in the UI lexicon and fires UI regardless of the throwaway/scrappy framing.

---
