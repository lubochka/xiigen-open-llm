# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-034-001 — "rough draft" + DOCUMENT: knowledge edge

**Knowledge edge: rough-draft qualifier + DOCUMENT**

"Rough draft — document the API endpoints."

Analysis:
- "rough draft" → gap-vibe-coding-rough-cut-001
- "document" → DOCUMENT fires ✓ (direct imperative)
- "API endpoints" → scope

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-rough-cut-001]

Note: DOCUMENT fires on the verb "document" itself, not on "draft" or "rough".

---

## B32-0C-034-002 — "thin slice" + DOCUMENT: knowledge edge

**Knowledge edge: thin-slice qualifier + DOCUMENT**

"Thin slice: just document the happy path."

Analysis:
- "thin slice" → gap-vibe-coding-spike-001
- "document" → DOCUMENT fires ✓
- "just" → intensifier; no predicate effect
- "happy path" → scope

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-spike-001]

---

## B32-0C-034-003 — "hack together" + DOCUMENT: knowledge edge

**Knowledge edge: hack-together + DOCUMENT**

"Hack together some docs for the integration."

Analysis:
- "hack together" → gap-vibe-coding-hack-001 (no IMPLEMENT; "together" form)
- "docs" → DOCUMENT fires ✓ (docs as object of implicit "write/add" + "hack together docs" pattern)
- "integration" → scope

Predicates: ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-hack-001]

Note: "hack together" in isolation never fires IMPLEMENT. "hack together docs" fires DOCUMENT via the "docs" trigger noun.

---

## B32-0C-034-004 — DOCUMENT alone with gap: no other predicates

**Knowledge edge: gap + DOCUMENT (no IMPLEMENT)**

| Request | Predicates | Gap ID |
|---|---|---|
| "Rough draft — document endpoints" | ["DOCUMENT"] | gap-vibe-coding-rough-cut-001 |
| "Good enough docs" | ["DOCUMENT"] | gap-vibe-coding-style-001 |
| "Thin slice — document happy path" | ["DOCUMENT"] | gap-vibe-coding-spike-001 |
| "Hack together some docs" | ["DOCUMENT"] | gap-vibe-coding-hack-001 |

DOCUMENT fires independently. IMPLEMENT does NOT fire just because DOCUMENT fires.

---

## B32-0C-034-005 — DOCUMENT + TEST + IMPLEMENT: knowledge edge

**Knowledge edge: all three predicates with gap framing**

"Thin slice: implement the feature and document the API and write tests."

Analysis:
- "thin slice" → gap-vibe-coding-spike-001
- "implement" → IMPLEMENT fires ✓
- "document" → DOCUMENT fires ✓
- "write tests" → TEST fires ✓ (write + tests)

Predicates: ["IMPLEMENT", "DOCUMENT", "TEST"]
GapWarnings: [gap-vibe-coding-spike-001]
DomainId: "software-development"

---
