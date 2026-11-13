# Layer 0-A — Compiler Pairs (015)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-015-001 — "quick win" alone: zero predicates

**Request:** "We need a quick win here — just a quick win for the team."

**Compiler analysis:**

- "quick win" → gap-vibe-coding-style-001 (style framing with no action verb)
- "win" → not in any predicate lexicon

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-style-001 ("quick win")

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-015-002 — "quick win" + "build" → IMPLEMENT fires

**Request:** "Build a quick win feature — something we can ship fast."

**Compiler analysis:**

- "quick win" → gap-vibe-coding-style-001
- "build" → IMPLEMENT fires on "build"

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001

**Rule confirmed:** "quick win" is style framing. IMPLEMENT fires on the action verb "build" regardless.

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-015-003 — "quick win" + "implement" → IMPLEMENT fires

**Request:** "Implement a quick win for the reporting module."

**Compiler analysis:**

- "quick win" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-015-004 — "quick win" + "document" → DOCUMENT fires

**Request:** "Document this quick win so the team knows how it works."

**Compiler analysis:**

- "quick win" → gap-vibe-coding-style-001
- "document" → DOCUMENT fires

**Result:** `["DOCUMENT"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to DOCUMENT flow

**DomainId:** "software-development"

---

## B32-0A-015-005 — "quick win" + "test" → TEST fires alongside style gap

**Request:** "Test this quick win before we merge it."

**Compiler analysis:**

- "quick win" → gap-vibe-coding-style-001
- "test" → TEST fires

**Result:** `["TEST"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to TEST flow

**DomainId:** "software-development"

---
