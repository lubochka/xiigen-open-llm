# Layer 0-A — Compiler Pairs (020)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-020-001 — Gap + IMPLEMENT: style qualifier present alongside action verb

**Request:** "Just make it work — build the checkout flow, nothing fancy."

**Compiler analysis:**

- "just make it work" → gap-vibe-coding-just-works-001
- "build" → IMPLEMENT fires (action verb present → predicate fires unconditionally)
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-just-works-001, gap-vibe-coding-nothing-fancy-001

**Rule confirmed:** "just make it work" is gap vocabulary. It does not suppress IMPLEMENT which fires on "build".

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-020-002 — Gap + IMPLEMENT: "quick and dirty" alongside "implement"

**Request:** "Quick and dirty — implement the subscription billing service."

**Compiler analysis:**

- "quick and dirty" → gap-vibe-coding-quick-dirty-001
- "implement" → IMPLEMENT fires

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-quick-dirty-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-020-003 — Gap + IMPLEMENT: "vibe" style + "create" verb

**Request:** "Vibe code it — create a data export feature, rough is fine."

**Compiler analysis:**

- "vibe" → gap-vibe-coding-style-001
- "code" → IMPLEMENT fires via root match
- "create" → IMPLEMENT fires (second match)
- "rough" → style qualifier; no additional gap ID

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-style-001

**Note:** Both "code" and "create" trigger IMPLEMENT. Predicate list deduplicates — [IMPLEMENT] is emitted once.

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-020-004 — Gap + IMPLEMENT: "hack together" + "write"

**Request:** "Hack together a solution — write the storage adapter."

**Compiler analysis:**

- "hack together" → gap-vibe-coding-hack-001
- "write" → IMPLEMENT fires

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-hack-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-020-005 — Gap + IMPLEMENT + TEST: style qualifier with two action verbs

**Request:** "Scrappy build — implement the feature and test it quick."

**Compiler analysis:**

- "scrappy" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires
- "test" → TEST fires
- "quick" → style qualifier (not a separate gap ID)

**Result:** `["IMPLEMENT", "TEST"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to IMPLEMENT + TEST flows

**DomainId:** "software-development"

---
