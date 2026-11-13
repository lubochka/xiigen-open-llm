# Layer 0-A — Compiler Pairs (006)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-006-001 — "prototype" + "build" = IMPLEMENT fires

**Request:** "Build a rough prototype of the checkout flow — throwaway code is fine."

**Compiler analysis:**

Step 1 — scan for action predicates:
- "build" → IMPLEMENT fires ✓
- "prototype" → GAP vocabulary when alone; but here it accompanies "build"

Step 2 — key rule: "prototype" + action verb:
- Rule: when "prototype" appears with an action verb (build/create/write/implement), IMPLEMENT fires on the action verb
- "prototype" itself remains gap vocabulary
- The action verb is the predicate trigger; "prototype" is a style qualifier

Step 3 — gap vocabulary:
- "prototype" → gap-vibe-coding-prototype-001
- "throwaway code" → gap-vibe-coding-throwaway-001
- "rough" → general vibe qualifier (covered under gap-vibe-coding-rough-cut-001)

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-prototype-001 ("prototype")
- gap-vibe-coding-throwaway-001 ("throwaway code")

**OutputDecision:** route to IMPLEMENT flow (BackendServiceFlow41)

**TransitionType:** feature_addition

**DomainId:** "software-development"

---

## B32-0A-006-002 — "prototype" alone: zero predicates → knowledge_edge

**Request:** "A rough prototype for the checkout flow."

**Compiler analysis:**

- "prototype" → GAP; no action verb present
- "rough" → gap qualifier
- No action verb from IMPLEMENT lexicon
- No UI vocabulary present

**Result:** `[]`

**GapWarnings:**
- gap-vibe-coding-prototype-001 ("prototype")

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-006-003 — "create a prototype": IMPLEMENT fires (create = action verb)

**Request:** "Create a quick prototype of the payment flow."

**Compiler analysis:**

- "create" → IMPLEMENT fires ✓
- "prototype" → GAP; gap-vibe-coding-prototype-001
- "quick" → general vibe qualifier

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-prototype-001

**OutputDecision:** route to IMPLEMENT flow

---

## B32-0A-006-004 — "implement a prototype": action verb dominates

**Request:** "Implement a rough prototype of the cart service — throwaway is fine."

**Compiler analysis:**

- "implement" → IMPLEMENT fires ✓
- "prototype" → GAP; gap-vibe-coding-prototype-001
- "rough" → gap qualifier
- "throwaway" → gap-vibe-coding-throwaway-001

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-prototype-001 ("prototype")
- gap-vibe-coding-throwaway-001 ("throwaway")

**OutputDecision:** route to IMPLEMENT flow

---

## B32-0A-006-005 — "write a prototype": IMPLEMENT fires

**Request:** "Write a prototype for the recommendation service — just throwaway code."

**Compiler analysis:**

- "write" → IMPLEMENT fires ✓
- "prototype" → GAP; gap-vibe-coding-prototype-001
- "throwaway code" → gap-vibe-coding-throwaway-001
- "just" → general vibe qualifier

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-prototype-001
- gap-vibe-coding-throwaway-001

**OutputDecision:** route to IMPLEMENT flow

---
