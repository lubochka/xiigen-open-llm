# Layer 0-A — Compiler Pairs (009)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-009-001 — ARCHITECTURE fires even in vibe context

**Request:** "Design a rough-cut architecture for the feature flag system — nothing fancy."

**Compiler analysis:**

Step 1 — action predicate scan:
- "design" → ARCHITECTURE fires ✓ (ARCHITECTURE lexicon: design, architect, architecture, structure, pattern)
- "architecture" → ARCHITECTURE fires ✓ (also in ARCHITECTURE lexicon; deduplicated)

Step 2 — UI scan: no UI vocabulary present

Step 3 — gap vocabulary:
- "rough-cut" → gap-vibe-coding-rough-cut-001
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001

**Result:** `["ARCHITECTURE"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-rough-cut-001 ("rough-cut")
- gap-vibe-coding-nothing-fancy-001 ("nothing fancy")

**OutputDecision:** route to ARCHITECTURE flow

**TransitionType:** documentation (ARCHITECTURE without IMPLEMENT)

**DomainId:** "software-development"

**Key rule confirmed:** Style qualifiers ("rough-cut", "nothing fancy") NEVER suppress ARCHITECTURE. The predicate fires regardless of vibe framing.

---

## B32-0A-009-002 — "architect" + "rough cut": ARCHITECTURE fires

**Request:** "Rough cut — architect the microservices layout for the billing system."

**Compiler analysis:**

- "rough cut" → GAP; gap-vibe-coding-rough-cut-001
- "architect" → ARCHITECTURE fires ✓
- "layout" → UI fires ✓ ("layout" is in UI lexicon)
- "microservices" → domain noun

**Result:** `["ARCHITECTURE", "UI"]`

**CapabilityGapWarnings:** gap-vibe-coding-rough-cut-001

**OutputDecision:** route to ARCHITECTURE + UI flows

---

## B32-0A-009-003 — "nothing fancy" alone with "build": IMPLEMENT fires; nothing-fancy is gap

**Request:** "Build a logging middleware — nothing fancy, just get it working."

**Compiler analysis:**

- "build" → IMPLEMENT fires ✓
- "nothing fancy" → GAP; gap-vibe-coding-nothing-fancy-001
- "just get it working" → covers gap-vibe-coding-just-works-001

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-nothing-fancy-001
- gap-vibe-coding-just-works-001 ("just get it working")

**OutputDecision:** route to IMPLEMENT flow

---

## B32-0A-009-004 — "rough-cut" alone: gap vocabulary; zero predicates if no action verb

**Request:** "Rough-cut plan for the API integration."

**Compiler analysis:**

- "rough-cut" → GAP; gap-vibe-coding-rough-cut-001
- "plan" → not in any predicate lexicon
- "integration" → may suggest API_CONTRACT? API_CONTRACT fires on: integrate, API, endpoint, contract, interface (interface also fires UI) — "integration" matches "integrate" root → API_CONTRACT fires ✓

**Result:** `["API_CONTRACT"]`

**GapWarnings:** gap-vibe-coding-rough-cut-001

**OutputDecision:** route to API_CONTRACT flow

---
