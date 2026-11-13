# Layer 0-A — Compiler Pairs (013)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-013-001 — "scrappy" alone: style qualifier, no predicate

**Request:** "Let's do something scrappy here — just a scrappy solution for now."

**Compiler analysis:**

Step 1 — scan for action predicates:
- "scrappy" → not in IMPLEMENT lexicon; style qualifier → GAP vocabulary
- "solution" → domain noun; no action predicate

Step 2 — scan for UI vocabulary: none present → UI does not fire

Step 3 — scan for ARCHITECTURE vocabulary: none present

Step 4 — gap vocabulary detected:
- "scrappy" → gap-vibe-coding-style-001

**Result:** `[]` — zero predicates fire

**GapWarnings:** gap-vibe-coding-style-001 ("scrappy")

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-013-002 — "scrappy build" → IMPLEMENT fires on "build"; "scrappy" is gap warning

**Request:** "Do a scrappy build of the notification service."

**Compiler analysis:**

- "scrappy" → style qualifier → gap-vibe-coding-style-001 (gap warning, does NOT suppress predicates)
- "build" → IMPLEMENT fires on "build"
- "notification service" → domain noun; no additional predicate

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001 ("scrappy")

**Rule confirmed:** Style qualifier "scrappy" is logged as a gap warning. IMPLEMENT fires on "build" unconditionally.

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-013-003 — "scrappy prototype" alone: both terms produce gap; no predicate

**Request:** "A scrappy prototype is fine — nothing polished."

**Compiler analysis:**

- "scrappy" → gap-vibe-coding-style-001
- "prototype" → no action verb present → gap-vibe-coding-prototype-001
- "nothing polished" → style qualifier; not a predicate trigger

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-style-001, gap-vibe-coding-prototype-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-013-004 — "scrappy implementation" → IMPLEMENT fires on "implementation" root

**Request:** "We need a scrappy implementation of the auth flow — nothing fancy."

**Compiler analysis:**

- "scrappy" → gap-vibe-coding-style-001
- "implementation" → root matches "implement" → IMPLEMENT fires
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-013-005 — "scrappy design" → ARCHITECTURE fires; "scrappy" is warning only

**Request:** "Quick scrappy design for the system architecture — rough sketch."

**Compiler analysis:**

- "scrappy" → gap-vibe-coding-style-001
- "design" → ARCHITECTURE fires on "design"
- "architecture" → ARCHITECTURE fires on "architecture" (both terms confirm)
- "rough" → style qualifier

**Result:** `["ARCHITECTURE"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to ARCHITECTURE flow

**DomainId:** "software-development"

---
