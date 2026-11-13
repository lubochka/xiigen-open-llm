# Layer 0-A — Compiler Pairs (014)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-014-001 — "rough cut" alone: zero predicates

**Request:** "Just a rough cut of the pipeline — rough cut is fine."

**Compiler analysis:**

- "rough cut" → gap-vibe-coding-rough-cut-001 (gap vocabulary)
- "just" → not an action verb
- "pipeline" → domain noun; no predicate

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-rough-cut-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-014-002 — "rough cut" + "build" → IMPLEMENT fires

**Request:** "Build a rough cut of the data ingestion module."

**Compiler analysis:**

- "rough cut" → gap-vibe-coding-rough-cut-001
- "build" → IMPLEMENT fires

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-rough-cut-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-014-003 — "rough cut" + "design" → ARCHITECTURE fires

**Request:** "Rough cut design of the event-driven architecture."

**Compiler analysis:**

- "rough cut" → gap-vibe-coding-rough-cut-001
- "design" → ARCHITECTURE fires
- "architecture" → ARCHITECTURE fires (confirmed by second trigger)

**Result:** `["ARCHITECTURE"]`

**GapWarnings:** gap-vibe-coding-rough-cut-001

**OutputDecision:** route to ARCHITECTURE flow

**DomainId:** "software-development"

---

## B32-0A-014-004 — "rough-cut" (hyphenated form) treated identically to "rough cut"

**Request:** "A rough-cut implementation is acceptable for the MVP."

**Compiler analysis:**

- "rough-cut" → gap-vibe-coding-rough-cut-001 (hyphenated form is same gap as two-word form)
- "implementation" → IMPLEMENT fires via "implement" root

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-rough-cut-001

**Note:** Both "rough cut" (space-separated) and "rough-cut" (hyphenated) map to gap-vibe-coding-rough-cut-001.

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-014-005 — "rough cut" + UI term → UI fires

**Request:** "Rough cut of the checkout screen — doesn't need to be polished."

**Compiler analysis:**

- "rough cut" → gap-vibe-coding-rough-cut-001
- "screen" → UI fires (screen is in UI lexicon)

**Result:** `["UI"]`

**GapWarnings:** gap-vibe-coding-rough-cut-001

**OutputDecision:** route to UI flow

**DomainId:** "software-development"

---
