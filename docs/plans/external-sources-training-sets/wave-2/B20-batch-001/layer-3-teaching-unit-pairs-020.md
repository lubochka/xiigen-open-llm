# Layer 3 — Teaching Unit Pairs (020)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-020-001 — "security tests" fires both TEST and SECURITY simultaneously

A: "Write security tests for the PasswordValidator." What fires?
B: "write" → IMPLEMENT. "security tests" → both TEST (via "tests") and SECURITY (via "security").
   Three predicates: TEST + IMPLEMENT + SECURITY. Three independent routing decisions:
   TEST → 5.1, IMPLEMENT → 4.1, SECURITY → 5.2 (testing context routes SECURITY to 5.2).
   Teaching: "security tests" is a two-predicate compound. The compound fires both predicates simultaneously.

---

## Teach-020-002 — SECURITY routes to 5.2 (not 1.x/2.x/3.x) in a testing context

A: "Write security tests for the PasswordValidator to verify timing attack resistance." Where does SECURITY route?
B: SECURITY routes to 5.2 (Security and Compliance Testing), NOT to Phase 1-3 design sub-phases.
   When SECURITY appears alongside TEST in a test-writing request, the SECURITY routing resolves within
   the testing phase. SECURITY → 5.2 in testing context. SECURITY → 3.x or 1.x only in design context.

---

## Teach-020-003 — "testable" (adjective) does NOT fire TEST; "write tests" does

A: "Ensure the PasswordValidator is testable." vs "Write tests for the PasswordValidator." What fires?
B: "Ensure the PasswordValidator is testable." → "testable" is adjective → NOT TEST. "ensure" = gap. Zero predicates.
   "Write tests for the PasswordValidator." → TEST fires on "tests"; IMPLEMENT fires on "write". Two predicates.
   The adjective form "testable" is categorically excluded from the TEST predicate. Only noun/verb/gerund forms fire.

---

## Teach-020-004 — "password history" domain rule: validate fires TEST, "must reject" does not

A: "Validate that the PasswordValidator rejects the last 5 passwords." vs "The PasswordValidator must reject the last 5 passwords." What fires?
B: "Validate that..." → TEST fires on "validate" (universal rule). Result: ["TEST"].
   "The PasswordValidator must reject..." → "must reject" is a requirement modal — no predicate. Zero predicates.
   Teaching: requirement modals ("must", "should", "shall", "needs to") fire no predicate. Only action vocabulary fires.
