# Layer 3 — Teaching Unit Pairs (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU003-001 — Teach: "spec" fires both TEST and IMPLEMENT

Misconception: "spec" only belongs to testing vocabulary.
Correction: "spec" is a dual trigger.
  As a noun ("write a spec") → TEST fires on "spec"; IMPLEMENT fires on "write".
  As a verb ("spec the API") → TEST fires (spec = specification of expected behaviour); IMPLEMENT fires on "spec" as a creation verb.
  Exception: "spec" in "specification document" alone without action verb → borderline; treat as DOCUMENT if "document" also present.
Rule: "spec" + any action verb → ["TEST", "IMPLEMENT"]; "spec" as standalone object noun → ["TEST"].

---

## TU003-002 — Teach: "testing strategy" fires TEST + ARCHITECTURE via "define"

Misconception: "define the testing strategy" is about planning and fires only ARCHITECTURE.
Correction: "testing" in "testing strategy" fires TEST. "define" fires ARCHITECTURE. Both predicates fire.
  CompiledPredicateIds: ["ARCHITECTURE", "TEST"]
  Sub-phases: ARCHITECTURE → 3.1; TEST → 5.1
Rule: "testing strategy" is a compound where "testing" is the TEST trigger and "strategy" is the noun. The compound doesn't suppress either predicate.

---

## TU003-003 — Teach: gap accumulation — multiple gap verbs still fire on object nouns

Misconception: If the action verb is gap, no predicate fires regardless of object.
Correction: Gap verbs suppress their own predicate potential but do not suppress predicates fired by object nouns.
  Example: "Review the unit tests" → "review" is gap; "unit tests" → TEST fires.
  Example: "Check the test suite" → "check" is gap; "test suite" → TEST fires.
Rule: Always evaluate subject, verb, and object separately for predicate triggers.

---

## TU003-004 — Teach: "code coverage" does NOT fire TEST

Misconception: "code coverage" is a testing metric and should fire TEST.
Correction: "coverage" alone is gap. "code coverage" contains neither "test" nor a TEST predicate trigger.
  "test coverage" → TEST fires on "test".
  "code coverage" → gap (zero predicates from this phrase alone).
Rule: The word "test" must be present. "coverage" alone is insufficient.

---

## TU003-005 — Teach: "reproduce" is gap; TEST fires only on "failing test" if added

Misconception: "reproduce the bug" should fire TEST because it's a QA (Quality Assurance) activity.
Correction: "reproduce" is gap. It fires no predicate.
  "Reproduce the bug and write a failing test" → TEST (via "failing test") + IMPLEMENT (via "write").
  "Reproduce the bug" alone → [] (gap).
Rule: Activity-description verbs without predicate lexicon membership are gap regardless of QA intent.
