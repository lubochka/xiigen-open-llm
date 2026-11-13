# Layer 0-A — Compiler Pairs (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-007-001 — "coding" fires IMPLEMENT via "code" root match

**Request:** "Exploratory coding session — hack together the notification service module."

**Compiler analysis:**

Step 1 — action predicate scan:
- "coding" → root match on "code" → IMPLEMENT fires ✓
  Rule: "coding" as gerund matches "code" root; IMPLEMENT fires
- "hack together" → GAP vocabulary; gap-vibe-coding-hack-001
- "module" → domain noun; may contribute to IMPLEMENT context? No — "module" alone does not fire a predicate; predicates fire on action verbs
- "service" → domain noun; IMPLEMENT context noun (not a trigger alone)

Step 2 — gap vocabulary:
- "exploratory" → gap-vibe-coding-exploratory-001
- "hack together" → gap-vibe-coding-hack-001

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-exploratory-001 ("exploratory")
- gap-vibe-coding-hack-001 ("hack together")

**OutputDecision:** route to IMPLEMENT flow

**TransitionType:** feature_addition

**DomainId:** "software-development"

---

## B32-0A-007-002 — "coding" root rule: consistent application

**Request:** "Let's do some exploratory coding on the parser."

**Compiler analysis:**

- "coding" → "code" root → IMPLEMENT fires ✓
- "exploratory" → GAP; gap-vibe-coding-exploratory-001
- "parser" → domain noun

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-exploratory-001

**OutputDecision:** route to IMPLEMENT flow

---

## B32-0A-007-003 — "exploratory" without "coding": gap only, no IMPLEMENT

**Request:** "Exploratory session on the auth module design."

**Compiler analysis:**

- "exploratory" → GAP; gap-vibe-coding-exploratory-001
- "session" → not an action predicate
- "design" → ARCHITECTURE fires on "design" ✓

**Result:** `["ARCHITECTURE"]`

**CapabilityGapWarnings:** gap-vibe-coding-exploratory-001

**OutputDecision:** route to ARCHITECTURE flow

**Note:** "exploratory" is gap, but "design" fires ARCHITECTURE. Style qualifier does not suppress the registered predicate.

---

## B32-0A-007-004 — "hack" alone vs "hack together"

**Request:** "Hack the notification service together quickly."

**Compiler analysis:**

- "hack together" (rearranged: "hack ... together") → GAP; gap-vibe-coding-hack-001
- "hack" alone (if "together" not present) → still gap vocabulary; gap-vibe-coding-hack-001
- No action verbs from IMPLEMENT lexicon present
- "service" → domain noun; does not fire predicate alone

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-hack-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-007-005 — "exploratory coding" + "design": both IMPLEMENT and ARCHITECTURE

**Request:** "Exploratory coding and design spike for the search service."

**Compiler analysis:**

- "coding" → IMPLEMENT fires ✓ (via "code" root)
- "design" → ARCHITECTURE fires ✓
- "exploratory" → GAP; gap-vibe-coding-exploratory-001
- "spike" → GAP; gap-vibe-coding-spike-001

**Result:** `["IMPLEMENT", "ARCHITECTURE"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-exploratory-001
- gap-vibe-coding-spike-001

**OutputDecision:** route to IMPLEMENT + ARCHITECTURE flows

---
