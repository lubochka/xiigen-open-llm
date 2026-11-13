# Layer 3 — Teaching Unit Pairs (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU005-001 — Teach: TEST fires in SECURITY domain without adding SECURITY predicate

Misconception: When testing password or encryption logic, SECURITY should always fire alongside TEST.
Correction: TEST fires on the test action verb/noun. SECURITY fires on security-action vocabulary ("XSS", "SQL injection", "token forgery", "encryption", "plaintext", etc.). Both fire independently. Security domain content alone is not sufficient — a security-action word must appear.
  Example A: "Assert the PasswordService hashes correctly." → ["TEST"] only (no security-action word)
  Example B: "Assert the PasswordService prevents plaintext storage." → ["TEST", "SECURITY"] (plaintext → SECURITY)
Rule: Domain proximity to security is not the SECURITY trigger. The security-action word is.

---

## TU005-002 — Teach: "white-box" and "black-box" are testing methodology vocabulary — gap

Misconception: "white-box test" should be equivalent to "unit test" and fire TEST the same way.
Correction: "white-box" and "black-box" are methodology qualifiers — they are gap. TEST fires on the word "test" in "white-box test" as the head noun.
  "Write white-box tests." → ["IMPLEMENT", "TEST"] — TEST from "tests", IMPLEMENT from "write"; "white-box" contributes nothing to predicate.
Rule: Testing methodology terms are domain gap. Predicates fire from action verbs and TEST-lexicon nouns only.

---

## TU005-003 — Teach: "test isolation" fires TEST via "test"

Misconception: "test isolation" is an infrastructure concern, not a test action, so it shouldn't fire TEST.
Correction: "test" in "test isolation" fires TEST. The compound modifier "isolation" is domain vocabulary and doesn't suppress the head word.
  "Improve test isolation." → ["TEST"] (TEST via "test"; "improve" is gap)
Rule: Evaluate each word in a compound independently for predicate membership.

---

## TU005-004 — Teach: multiple TEST triggers in one request = one TEST predicate

Misconception: "verify and assert and validate" in one request should fire TEST three times or increase its weight.
Correction: Multiple triggers for the same predicate produce exactly ONE instance of that predicate in CompiledPredicateIds.
  "Verify the service, assert the output, and validate the schema." → ["TEST"] (one predicate, three triggers)
Rule: CompiledPredicateIds is a set, not a multiset. Duplicate predicate triggers are deduplicated.

---

## TU005-005 — Teach: "testing" (gerund) fires TEST identically to "test" (noun)

Misconception: "testing" as a gerund is weaker than "test" and might not fire TEST in all contexts.
Correction: "testing" fires TEST identically to "test". No strength distinction between noun and gerund forms.
  "Mutation testing is applied." → ["TEST"] (via "testing")
  "Testing the service is blocked." → ["TEST"] (via "testing" gerund subject)
  "The testing strategy is..." → ["TEST"] (via "testing")
Rule: All morphological forms of "test" (test, tests, testing, tested) fire TEST identically.
