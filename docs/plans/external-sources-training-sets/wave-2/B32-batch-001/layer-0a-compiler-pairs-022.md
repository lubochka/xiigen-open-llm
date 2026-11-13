# Layer 0-A — Compiler Pairs (022)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-022-001 — "rough architecture sketch" → ARCHITECTURE fires; "rough" is warning

**Request:** "Draw up a rough architecture sketch for the service mesh."

**Compiler analysis:**

- "rough" → style qualifier → no gap ID (rough alone without "cut" does not map to rough-cut-001; it is a general style qualifier)
- "architecture" → ARCHITECTURE fires
- "sketch" → not in any predicate lexicon; it is a style noun
- "service mesh" → domain noun

**Result:** `["ARCHITECTURE"]`

**GapWarnings:** none (rough alone without "cut" does not produce a registered gap ID in this batch)

**Note:** "rough cut" → gap-vibe-coding-rough-cut-001. "rough" alone is a general style qualifier with no registered gap ID.

**OutputDecision:** route to ARCHITECTURE flow

**DomainId:** "software-development"

---

## B32-0A-022-002 — "scrappy architecture" + "design" → ARCHITECTURE fires; "scrappy" is gap warning

**Request:** "Scrappy architecture design for the batch processing system."

**Compiler analysis:**

- "scrappy" → gap-vibe-coding-style-001
- "architecture" → ARCHITECTURE fires
- "design" → ARCHITECTURE fires (confirmed by second trigger)

**Result:** `["ARCHITECTURE"]`

**GapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to ARCHITECTURE flow

**DomainId:** "software-development"

---

## B32-0A-022-003 — "quick and dirty architecture" → ARCHITECTURE fires; "quick and dirty" is gap

**Request:** "Quick and dirty architecture for the new API gateway."

**Compiler analysis:**

- "quick and dirty" → gap-vibe-coding-quick-dirty-001
- "architecture" → ARCHITECTURE fires
- "API" → API_CONTRACT fires

**Result:** `["ARCHITECTURE", "API_CONTRACT"]`

**GapWarnings:** gap-vibe-coding-quick-dirty-001

**OutputDecision:** route to ARCHITECTURE + API_CONTRACT flows

**DomainId:** "software-development"

---

## B32-0A-022-004 — "rough-cut architecture" → ARCHITECTURE fires; "rough-cut" is gap

**Request:** "Rough-cut architecture for the recommendation engine — just enough to get started."

**Compiler analysis:**

- "rough-cut" → gap-vibe-coding-rough-cut-001
- "architecture" → ARCHITECTURE fires
- "just enough to get started" → style framing; no registered gap ID

**Result:** `["ARCHITECTURE"]`

**GapWarnings:** gap-vibe-coding-rough-cut-001

**OutputDecision:** route to ARCHITECTURE flow

**DomainId:** "software-development"

---

## B32-0A-022-005 — Gap + ARCHITECTURE + IMPLEMENT: "hack together" + "design" + "build"

**Request:** "Hack together a design and build the core module — scrappy is fine."

**Compiler analysis:**

- "hack together" → gap-vibe-coding-hack-001
- "design" → ARCHITECTURE fires
- "build" → IMPLEMENT fires
- "scrappy" → gap-vibe-coding-style-001

**Result:** `["ARCHITECTURE", "IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-hack-001, gap-vibe-coding-style-001

**OutputDecision:** route to ARCHITECTURE + IMPLEMENT flows

**DomainId:** "software-development"

---
