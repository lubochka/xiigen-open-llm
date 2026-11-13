# Layer 0-A — Compiler Pairs (017)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-017-001 — "low-fi" alone: zero predicates

**Request:** "Low-fi version of the onboarding flow — keep it low-fi."

**Compiler analysis:**

- "low-fi" → style qualifier → gap-vibe-coding-style-001
- "version" → domain noun; not an action predicate
- "onboarding flow" → domain noun

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-style-001 ("low-fi")

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-017-002 — "low-fi" + "build" → IMPLEMENT fires

**Request:** "Build a low-fi version of the settings page."

**Compiler analysis:**

- "low-fi" → gap-vibe-coding-style-001
- "build" → IMPLEMENT fires
- "page" → UI fires (page is in UI lexicon)

**Result:** `["IMPLEMENT", "UI"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to IMPLEMENT + UI flows

**DomainId:** "software-development"

---

## B32-0A-017-003 — "low-fi prototype" alone: prototype + low-fi both gap; no predicate

**Request:** "Just a low-fi prototype for now."

**Compiler analysis:**

- "low-fi" → gap-vibe-coding-style-001
- "prototype" → no action verb → gap-vibe-coding-prototype-001
- "just" → not an action verb

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-style-001, gap-vibe-coding-prototype-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-017-004 — "low-fi wireframe" + "design" → ARCHITECTURE fires

**Request:** "Design a low-fi wireframe for the mobile interface."

**Compiler analysis:**

- "low-fi" → gap-vibe-coding-style-001
- "design" → ARCHITECTURE fires
- "interface" → UI fires (interface is in UI lexicon)

**Result:** `["ARCHITECTURE", "UI"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to ARCHITECTURE + UI flows

**DomainId:** "software-development"

---

## B32-0A-017-005 — "lo-fi" variant: same classification as "low-fi"

**Request:** "Lo-fi implementation — we can polish it later."

**Compiler analysis:**

- "lo-fi" → same classification as "low-fi" → gap-vibe-coding-style-001
- "implementation" → IMPLEMENT fires via "implement" root

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---
