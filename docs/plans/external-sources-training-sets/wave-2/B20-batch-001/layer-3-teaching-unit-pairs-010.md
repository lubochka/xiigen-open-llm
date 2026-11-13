# Layer 3 — Teaching Unit Pairs (010)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU010-001 — Teach: "sanity check" and "double-check" are both gap

Misconception: "sanity check" implies testing and should fire TEST; "double-check" implies verification and should fire TEST.
Correction: Both are informal quality verification phrases — neither appears in the TEST predicate lexicon.
  "Do a sanity check." → [] (gap)
  "Double-check the discount logic." → [] (gap)
  "Run a sanity test." → ["TEST"] (TEST fires on "test", not "sanity")
Rule: Informal QA phrases are gap. "sanity check", "double-check", "sense-check", "gut check" → all gap.

---

## TU010-002 — Teach: "code quality" is gap; "review unit tests" fires TEST via object

Misconception: "improve code quality" fires IMPLEMENT because quality improvement requires code changes.
Correction: "improve" is gap. "code quality" is gap. No predicate fires.
  "Improve the code quality of the OrderService." → [] (gap — both action and object are gap)
  "Review the unit tests for code quality." → ["TEST"] (TEST via "unit tests"; "review" is gap; "code quality" is gap)
Rule: Quality vocabulary without predicate triggers → gap. TEST fires when test-lexicon nouns are present as object.

---

## TU010-003 — Teach: "tested" (past participle) fires TEST

Misconception: "ensure it is tested" — "tested" is an adjective here and shouldn't fire TEST.
Correction: "tested" is a past participle form of "test" and fires TEST.
  "Ensure the service is fully tested." → ["TEST"] (TEST via "tested")
  "Make sure all methods are tested." → ["TEST"] (TEST via "tested")
Rule: All morphological forms of "test" fire TEST: test (verb), tests (noun/verb), testing (gerund), tested (past participle), test (adjective-modified noun).

---

## TU010-004 — Teach: four-predicate requests are valid; factory creates four flows

Misconception: A request with four predicates is ambiguous or should be reduced to the most relevant.
Correction: Four-predicate requests are valid and produce four flows in parallel or sequential execution.
  Example: "Write tests, implement the method, document the API, and assert no SQL injection."
  → ["TEST", "IMPLEMENT", "DOCUMENT", "SECURITY"] | SubPhases: [5.1, 4.1, 6.2, 5.2]
  FlowFactory creates all four flows. ExecutionMode is determined by dependency analysis.
Rule: There is no predicate limit per request. All fired predicates generate flows.

---

## TU010-005 — Teach: "AUTHORIZATION" context in unit tests routes to 5.2 not 6.3

Misconception: AUTHORIZATION in a testing context routes to the security audit sub-phase 6.3.
Correction: AUTHORIZATION routes to 5.2 (security testing tier) when the request is about writing/asserting authorization behavior. AUTHORIZATION routes to 6.3 only when paired with an audit/review action in a security audit context.
  "Verify non-admin users are blocked." → AUTHORIZATION → 5.2
  "Audit the authorization controls." → AUTHORIZATION + SECURITY → 6.3
Rule: "verify/assert/write tests for" authorization → 5.2. "audit/review" authorization → 6.3.
