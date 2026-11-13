# Layer 0-A — Compiler Pairs (010)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-010-001 — All seven predicates in vibe framing

**Request:** "Quick vibe session: architect the auth layer, integrate the OAuth provider, build the login service, secure the tokens, write unit tests, document the flow, and QA the implementation."

**Compiler analysis:**

Step 1 — predicate-by-predicate scan:

| Token | Predicate Fired |
|---|---|
| "architect" | ARCHITECTURE ✓ |
| "integrate" | API_CONTRACT ✓ |
| "build" | IMPLEMENT ✓ |
| "secure" | SECURITY ✓ |
| "write unit tests" | TEST ✓ |
| "document" | DOCUMENT ✓ |
| "QA" | QA ✓ |

Step 2 — gap vocabulary:
- "quick vibe session" → gap-vibe-coding-style-001 ("vibe"), general vibe qualifier
- "quick" → general vibe qualifier (not a registered gap ID alone)

Step 3 — deduplication: 7 distinct predicates; no duplicates

**Result:** `["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001 ("vibe session")

**OutputDecision:** route to all 7 flows

**TransitionType:** feature_addition (IMPLEMENT present)

**DomainId:** "software-development"

**Key rule confirmed:** All 7 predicates fire independently. The "vibe session" framing emits a gap warning but does not suppress any predicate. This is the maximal B32 case.

---

## B32-0A-010-002 — Subset of predicates: five with vibe framing

**Request:** "Vibe coding mode: design the architecture, implement the core service, integrate the payment gateway, test the flows, and document the system."

**Compiler analysis:**

| Token | Predicate |
|---|---|
| "design" + "architecture" | ARCHITECTURE ✓ |
| "implement" | IMPLEMENT ✓ |
| "integrate" | API_CONTRACT ✓ |
| "test" | TEST ✓ |
| "document" | DOCUMENT ✓ |

Gap: "vibe coding mode" → gap-vibe-coding-style-001

**Result:** `["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to 5 flows

---

## B32-0A-010-003 — "QA" alone with vibe framing

**Request:** "Quick vibe — QA the login flow."

**Compiler analysis:**

- "QA" → QA fires ✓
- "vibe" → GAP; gap-vibe-coding-style-001
- "quick" → general qualifier

**Result:** `["QA"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001

**TransitionType:** no contribution (QA alone without IMPLEMENT)

**OutputDecision:** route to QA flow

---
