# Layer 3 — Teaching Unit Pairs (011)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU011-001 — Teach: "extend" fires IMPLEMENT; "extend tests" fires IMPLEMENT+TEST

Misconception: "extend the existing tests" fires only TEST because it's about tests.
Correction: "extend" fires IMPLEMENT (extending = building on existing). "tests" fires TEST. Both fire.
  "Extend the existing tests for the CartService." → ["IMPLEMENT", "TEST"]
  "Extend the CartService with a new method." → ["IMPLEMENT"]
Rule: "extend" is always IMPLEMENT. If the object contains a TEST trigger word, TEST also fires.

---

## TU011-002 — Teach: "assertion helper" fires TEST via "assertion"; "build" fires IMPLEMENT

Misconception: "build assertion helpers" only fires IMPLEMENT because "helpers" is infrastructure.
Correction: "build" fires IMPLEMENT. "assertion" contains the "assert" root and fires TEST.
  "Build assertion helpers for the OrderService tests." → ["IMPLEMENT", "TEST"]
Rule: "assertion" fires TEST via its root "assert". Helper/utility framing doesn't suppress the predicate.

---

## TU011-003 — Teach: "contract test" fires TEST and API_CONTRACT

Misconception: "contract test" is only about API contracts and fires API_CONTRACT but not TEST.
Correction: "contract test" fires both: "test" → TEST; "contract" → API_CONTRACT.
  "Write contract tests for the ShippingProvider." → ["IMPLEMENT", "TEST", "API_CONTRACT"]
  "Verify the contract tests pass." → ["TEST", "API_CONTRACT"]
Rule: "contract test" is a dual trigger. Evaluate "test" and "contract" independently.

---

## TU011-004 — Teach: "mock contract" fires API_CONTRACT via "contract"; TEST fires from test action verb

Misconception: "mock contract" fires only TEST because mocks are test infrastructure.
Correction: "mock" alone is gap for TEST. "contract" fires API_CONTRACT.
  "Build the mock contract for the PaymentGateway." → ["IMPLEMENT", "API_CONTRACT"]
  "Write tests using the mock contract." → ["IMPLEMENT", "TEST", "API_CONTRACT"]
Rule: "mock" alone → gap. "contract" → API_CONTRACT. "test" must be explicit for TEST to fire.

---

## TU011-005 — Teach: TEST vs DOCUMENT when "spec" is used

Misconception: "spec" always fires both TEST and IMPLEMENT and never DOCUMENT.
Correction: "spec" can participate in DOCUMENT context.
  "Write a spec." → ["IMPLEMENT", "TEST"] (write = IMPLEMENT; spec = TEST)
  "Document the spec." → ["DOCUMENT", "TEST"] (document = DOCUMENT; spec = TEST)
  "Spec and document the testing strategy." → ["IMPLEMENT", "TEST", "DOCUMENT"] (spec = IMPLEMENT+TEST; document = DOCUMENT)
Rule: "spec" fires TEST in all cases. The accompanying verb determines whether IMPLEMENT or DOCUMENT also fires.
