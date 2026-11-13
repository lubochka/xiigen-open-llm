# Layer 0-A — Compiler Pairs (019)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-019-001 — "fast and loose" alone: zero predicates

**Request:** "Play it fast and loose with the implementation details for now."

**Compiler analysis:**

- "fast and loose" → gap-vibe-coding-style-001 (style framing)
- "implementation details" → "implementation" roots to "implement" → IMPLEMENT fires

**Wait — recheck: "implementation details" contains the root "implement":**
- "implementation" → root "implement" → IMPLEMENT fires

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001 ("fast and loose")

**Note:** "fast and loose" is style framing and produces a gap warning, but "implementation" in the same sentence fires IMPLEMENT via root match. The style qualifier does not suppress the predicate.

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-019-002 — "fast and loose" with no action verb → zero predicates

**Request:** "Fast and loose approach — just a rough sketch."

**Compiler analysis:**

- "fast and loose" → gap-vibe-coding-style-001
- "approach" → domain noun; not an action verb
- "rough" → style qualifier
- "sketch" → not in any predicate lexicon

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-019-003 — "fast and loose" + "build" → IMPLEMENT fires

**Request:** "Build it fast and loose — we can tighten it up later."

**Compiler analysis:**

- "fast and loose" → gap-vibe-coding-style-001
- "build" → IMPLEMENT fires

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-019-004 — "fast and loose" + "design" → ARCHITECTURE fires

**Request:** "Fast and loose design of the microservices architecture."

**Compiler analysis:**

- "fast and loose" → gap-vibe-coding-style-001
- "design" → ARCHITECTURE fires
- "architecture" → ARCHITECTURE fires (confirmed by second trigger)

**Result:** `["ARCHITECTURE"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to ARCHITECTURE flow

**DomainId:** "software-development"

---

## B32-0A-019-005 — "fast and loose" + "code" → IMPLEMENT fires via "code" root

**Request:** "Fast and loose coding session — code the user service."

**Compiler analysis:**

- "fast and loose" → gap-vibe-coding-style-001
- "coding" → root matches "code" → IMPLEMENT fires
- "code" → IMPLEMENT fires (second match, same predicate)

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---
