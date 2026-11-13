# Layer 0-C — Knowledge-Edge Pairs (033)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-033-001 — "good enough" + TEST: knowledge edge analysis

**Knowledge edge: good enough + TEST co-occurrence**

"Good enough — write unit tests for every branch."

Analysis:
- "good enough" → gap-vibe-coding-style-001 (no predicate)
- "write" → IMPLEMENT fires ✓ (write fires IMPLEMENT as generic action verb)
- "unit tests" → TEST fires ✓ (positive, as object of "write")

Note on "write unit tests": both IMPLEMENT fires (write) AND TEST fires (unit tests).

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]

---

## B32-0C-033-002 — "scrappy" + TEST: knowledge edge analysis

**Knowledge edge: scrappy + TEST co-occurrence**

"Scrappy implementation but still test the critical paths."

Analysis:
- "scrappy" → gap-vibe-coding-style-001
- "implementation" → IMPLEMENT fires ✓ (implementation noun from implement root)
- "test" → TEST fires ✓ (positive imperative verb)

Note: "implementation" as noun can fire IMPLEMENT (root match). "test" as imperative verb fires TEST.

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]

---

## B32-0C-033-003 — "PoC" + TEST: knowledge edge analysis

**Knowledge edge: PoC + TEST co-occurrence**

"PoC only, but write tests for the integration layer."

Analysis:
- "PoC" → gap-vibe-coding-prototype-001
- "write" → IMPLEMENT fires ✓
- "tests" → TEST fires ✓

Note: "but" conjunction does NOT suppress predicates that follow it.

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-prototype-001]

---

## B32-0C-033-004 — "throwaway" + TEST: knowledge edge analysis

**Knowledge edge: throwaway + TEST co-occurrence**

"Throwaway code — but add unit tests so we can verify behaviour."

Analysis:
- "throwaway" → gap-vibe-coding-throwaway-001
- "add" → IMPLEMENT fires ✓ (add fires IMPLEMENT)
- "unit tests" → TEST fires ✓

Note: "so we can verify" = purpose clause; does not affect predicate firing.

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-throwaway-001]

---

## B32-0C-033-005 — "walking skeleton" + TEST + DOCUMENT: knowledge edge

**Knowledge edge: walking skeleton with multiple embedded predicates**

"Walking skeleton with tests and documentation."

Analysis:
- "walking skeleton" → gap-vibe-coding-spike-001
- "tests" → TEST fires ✓ (with-instrumental)
- "documentation" → DOCUMENT fires ✓ (with-instrumental)

Note: "with X and Y" instrumental fires both X and Y if they are predicate triggers.

Predicates: ["TEST", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-spike-001]

DomainId: "software-development"

---
