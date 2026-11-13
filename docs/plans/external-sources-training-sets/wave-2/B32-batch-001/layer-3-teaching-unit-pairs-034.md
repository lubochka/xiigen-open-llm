# Layer 3 — Teaching Unit Pairs (034)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-034-001 — Teaching: "just get it working" is a gap qualifier

**Teaching unit:** "just get it working" → gap-vibe-coding-style-001

"Just get it working" is a vibe-code phrasing. "Get it working" is not equivalent to "implement" for predicate firing.

**Rule:** "just get it working" alone → predicates = [] → knowledge_edge → SCL.

**With explicit verb:** "just get it working — implement X" → IMPLEMENT fires + gap warning.

---

## B32-L3-034-002 — Teaching: "clean up later" is a gap qualifier

**Teaching unit:** "clean up later" → gap-vibe-coding-style-001

"Clean up later" signals deferred quality. It is not an action verb. No predicate fires from it.

**Rule:** "clean up later" alone → predicates = [] → SCL. "Clean up later — implement X" → IMPLEMENT fires + gap warning.

---

## B32-L3-034-003 — Teaching: "don't bother with" and "skip the" are negation patterns

**Teaching unit:** Negation pattern recognition

"Don't bother with X" and "skip the X" are negation patterns that suppress the predicate associated with X.

They are not gap qualifiers. They do not add a gap ID to GapWarnings. They only prevent the negated predicate from firing.

---

## B32-L3-034-004 — Teaching: negation leaves GapWarnings unchanged

**Teaching unit:** Negation does not affect gap ID assignment

When "lean sprint — implement feature, skip the QA" is parsed:
- GapWarnings = [gap-vibe-coding-style-001] (from "lean" only)
- "skip the QA" adds no gap ID to GapWarnings
- "skip the QA" only prevents QA from firing

Negation patterns are silent in the gap vocabulary scan.

---
