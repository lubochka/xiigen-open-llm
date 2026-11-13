# Held-Out and Corruption Pairs (027)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H027-001 — Held-out: UserRegistrationTest — duplicate email error

A: "Write E2E test scenarios for the registration form — assert the duplicate email error modal appears."
  Expected: ["TEST", "UI"]
  Held-out: TEST ("write E2E test scenarios", "assert"). UI ("registration form" → form, "error modal" → modal — two triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## H027-002 — Held-out: UserRegistrationTest — "check registration" gap

A: "Check that user registration works correctly."
  Expected: []
  Held-out: "check" is gap. "user registration" is domain vocabulary. "works" is not a trigger. Zero predicates → knowledge_edge_synthetic. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## H027-003 — Held-out: UserRegistrationTest — "build form" + tests

A: "Build the registration form component and write E2E tests for required field validation."
  Expected: ["IMPLEMENT", "TEST", "UI"]
  Held-out: IMPLEMENT ("build the registration form component" — "component" is a feature noun). TEST ("write E2E tests"). UI ("registration form" → form, "component" → component — two UI triggers, one UI). TransitionType: feature_addition. AllSubPhaseIds: [4.1, 5.3, 1.5].

---

## C027-001 — Corruption v6-01: REGISTRATION predicate invented

Pattern: v6-01. Corrupt: predicates = ["TEST", "UI", "REGISTRATION"].
Correct: predicates = ["TEST", "UI"]. Domain names do not become predicates.

---

## C027-002 — Corruption v6-02: "check" → QA

Pattern: v6-02. Corrupt: "Check that registration works" → predicates = ["QA"].
Correct: predicates = []. "check" is gap. QA fires only on the literal abbreviation "QA". "check" is not a QA trigger.
