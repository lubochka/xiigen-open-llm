# Layer 3 — Teaching Unit Pairs (002)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU002-001 — Teach: "validate" fires TEST in ANY context

Misconception: "validate" should only fire TEST when the task is explicitly about writing tests.
Correction: "validate" fires TEST universally — user input validation, data validation, business rule validation, all contexts.
  Example A: "Validate user input before saving." → TEST (data validation context)
  Example B: "Validate the EU VAT calculation." → TEST (business rule context)
  Example C: "Validate the test assertion output." → TEST (test context)
Rule: There is no reading of "validate" that does not fire TEST.

---

## TU002-002 — Teach: "fix" does NOT fire IMPLEMENT

Misconception: "fix the bug" should fire IMPLEMENT because fixing a bug involves code changes.
Correction: "fix" is gap in the IMPLEMENT lexicon. The IMPLEMENT predicate fires on: implement, build, create, write, add, generate, develop, extend, increase, apply, scaffold, spec (as verb).
  "fix" is NOT in that list.
  Example A: "Fix the CartService tax calculation." → [] (gap)
  Example B: "Implement the fix for the CartService tax calculation." → ["IMPLEMENT"]
Rule: "fix" alone → gap. Wrap it in an IMPLEMENT verb to fire IMPLEMENT.

---

## TU002-003 — Teach: "run" alone is gap; TEST fires on the object noun

Misconception: "run the tests" should fire TEST because "run" initiates test execution.
Correction: "run" is gap. TEST fires on "tests" as the object noun, not on "run".
  "run" does not fire IMPLEMENT either.
  Example A: "Run the tests." → ["TEST"] (via "tests")
  Example B: "Run the deployment." → [] (gap — "deployment" is not in any predicate lexicon as a standalone)
Rule: Test whether the object noun fires the predicate, not the verb "run".

---

## TU002-004 — Teach: "mutation testing" fires TEST; "apply" fires IMPLEMENT

Misconception: "apply mutation testing" should only fire IMPLEMENT because "apply" is the action verb.
Correction: Both fire. "apply" → IMPLEMENT. "mutation testing" → TEST via "testing" (gerund).
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
Rule: Each trigger word fires its predicate independently. Compound sentences accumulate predicates.

---

## TU002-005 — Teach: sub-phase routing — unit test vs E2E (End-to-End)

Misconception: All TEST predicates route to the same sub-phase.
Correction:
  "unit tests" → 5.1 (unit testing sub-phase)
  "integration tests" (service-level, mocked) → 5.1
  "E2E tests" / "end-to-end tests" / "browser tests" / "smoke tests" → 5.3
  "security tests" → 5.2
Rule: The compound modifier before "tests" determines sub-phase. No modifier → 5.1.
