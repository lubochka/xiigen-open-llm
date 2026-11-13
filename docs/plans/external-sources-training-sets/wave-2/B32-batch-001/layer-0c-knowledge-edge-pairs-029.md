# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-029-001 — TEST trigger vocabulary: complete list

**Knowledge edge: all vocabulary that fires TEST**

Positive triggers (TEST fires):
- "test", "tests", "testing" as positive action target
- "unit test", "unit tests"
- "write tests", "add tests", "write unit tests"
- "test the X", "test coverage"
- "run tests", "run the tests"

Negative triggers (TEST does NOT fire):
- "no tests needed"
- "don't bother testing"
- "skip the tests"
- "without tests"
- "tests not required"

---

## B32-0C-029-002 — DOCUMENT trigger vocabulary: complete list

**Knowledge edge: all vocabulary that fires DOCUMENT**

Positive triggers (DOCUMENT fires):
- "document", "documentation", "docs"
- "write docs", "add docs", "write documentation"
- "document the X", "document X"
- "spec" as target of write/create

Negative triggers (DOCUMENT does NOT fire):
- "no documentation needed"
- "skip the docs"
- "without docs"
- "documentation not required"

---

## B32-0C-029-003 — QA trigger vocabulary: complete list

**Knowledge edge: all vocabulary that fires QA**

Positive triggers (QA fires):
- "QA", "quality assurance" as directive
- "QA the X", "do QA"
- "qa review"

Negative triggers (QA does NOT fire):
- "no QA needed"
- "skip QA"
- "without QA"
- "QA not required"

---

## B32-0C-029-004 — Combined gap + embedded predicate: knowledge edge summary

**When gap vocabulary is present AND a TEST/DOCUMENT/QA trigger is present:**

| Gap term | Embedded predicate | Result |
|---|---|---|
| good enough | TEST | ["TEST"] + gap warning |
| minimum viable | DOCUMENT | ["DOCUMENT"] + gap warning |
| spike | QA | ["QA"] + gap warning |
| PoC | TEST + QA | ["TEST", "QA"] + gap warning |
| walking skeleton | DOCUMENT + TEST | ["TEST", "DOCUMENT"] + gap warning |

SCHEDULE_CAPABILITY_LEARNING: NOT emitted when predicates non-empty.

---

## B32-0C-029-005 — "walking skeleton" + TEST: detailed knowledge edge

**Walking skeleton with test coverage:**

"Create a walking skeleton with unit tests covering the happy path."

Knowledge edge analysis:
- "walking skeleton" → gap-vibe-coding-spike-001 (investigation framing)
- "unit tests" → TEST fires ✓ (positive with-instrumental)
- "create" → IMPLEMENT fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarning: gap-vibe-coding-spike-001
Route: ImplementationFlow + TestFlow

DomainId: "software-development"

---
