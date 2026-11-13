# Layer 3 — Teaching Unit Pairs (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU004-001 — Teach: BDD syntax fires zero predicates without "test"

Misconception: Given/When/Then syntax describes test scenarios and should fire TEST.
Correction: Pure BDD (Behaviour-Driven Development) syntax fires zero predicates unless the word "test" appears in the request.
  "Given X, When Y, Then Z" → [] (gap)
  "Write BDD tests: Given X, When Y, Then Z" → ["IMPLEMENT", "TEST"] (via "write"+"tests")
Rule: The BDD scenario structure itself is not a predicate trigger. "test" must appear explicitly.

---

## TU004-002 — Teach: "assert" fires TEST even when it appears in code context

Misconception: "Assert.Equal(expected, actual)" is code, not a natural-language command, so it shouldn't fire TEST.
Correction: When a user writes "Assert.Equal(expected, actual)" as a task description, "assert" appears and fires TEST.
  Training intent: natural language + code tokens are treated uniformly at the lexicon level.
  CompiledPredicateIds: ["TEST"]
Rule: The predicate lexicon applies to all words in the user request regardless of whether they appear in code snippets.

---

## TU004-003 — Teach: "QA" abbreviation fires QA; "quality assurance" does NOT fire QA

Misconception: "quality assurance" spelled out should fire QA predicate — it's the same concept.
Correction: QA fires ONLY on the literal abbreviation "QA". "quality assurance" spelled out is gap.
  "Log QA results." → QA fires.
  "Perform quality assurance." → [] (gap).
Rule: QA is abbreviation-gated. This is a strict lexicon decision, not a semantic decision.

---

## TU004-004 — Teach: "negative test case" fires TEST; the adjective doesn't suppress

Misconception: "negative test case" might not fire TEST because "negative" could indicate the test is about failures, not testing itself.
Correction: The head noun "test case" fires TEST. Adjective modifiers ("negative", "positive", "boundary") never suppress the head noun's predicate.
  "negative test case" → TEST (via "test case")
  "boundary test case" → TEST (via "test case")
  "positive test case" → TEST (via "test case")
Rule: Evaluate head nouns for predicates; modifiers are domain vocabulary, not predicate gates.

---

## TU004-005 — Teach: TransitionType determination for TEST+IMPLEMENT

Misconception: When both TEST and IMPLEMENT fire, TransitionType should be "testing".
Correction: TransitionType is determined by the IMPLEMENT verb, not the TEST predicate.
  "write" → feature_addition; "refactor" → refactoring; "design"/"document"/"diagram" → documentation.
  TransitionType belongs to the IMPLEMENT flow, not the TEST flow. TEST flow has no TransitionType — it routes directly to sub-phase 5.1/5.2/5.3.
Rule: TransitionType applies to IMPLEMENT predicate only. Never assign TransitionType to TEST routing.
