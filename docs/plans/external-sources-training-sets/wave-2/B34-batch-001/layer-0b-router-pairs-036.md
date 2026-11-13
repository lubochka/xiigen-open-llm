# Layer 0-B — Router Pairs (036)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-036-001 — Round 2 complete routing reference

New vocabulary in Round 2 (025–036):
- show how to → gap-skills-guidance-train-001
- demonstrate → gap-skills-guidance-train-001
- introduce [person] → gap-skills-guidance-train-001 or onboard-001
- brief [team] on → gap-skills-guidance-mentor-001
- familiarize with → gap-skills-guidance-onboard-001
- run through → gap-skills-guidance-mentor-001
- review with → gap-skills-guidance-mentor-001
- present to → gap-skills-guidance-train-001
- discuss → gap-skills-guidance-mentor-001

---

## B34-router-036-002 — "discuss" + full gap (no code actions)

A: "Discuss, brief, and review the new platform strategy with the engineering all-hands."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: discuss + brief + review with = all guidance vocabulary. Predicate set = []. Single GapHandler invocation.

---

## B34-router-036-003 — "run through" + security vulnerability patch

A: "Run through the vulnerability report with the security team and patch all critical issues."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "run through report with team" = gap warning; "patch all critical issues" = security action → SECURITY fires.

---

## B34-router-036-004 — "review with" + TEST routing

A: "Review the test strategy with the QA (Quality Assurance) lead and write the E2E (End-to-End) test suite."
Router decision: Predicates = ["TEST"] → E2ETestingFlow53 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "review with QA lead" = gap warning; "write the E2E test suite" = test-writing action → TEST fires.

---

## B34-router-036-005 — GapHandler suppression rule restatement

GapHandler suppressed whenever any of these verbs appear as requester's direct actions:
implement, build, deploy, configure, integrate, write (code/tests/docs), design, architect, secure, harden, audit, document.
GapHandler invoked ONLY when predicate set = [] after full scan.
Guidance vocabulary never suppresses GapHandler; only code predicates do.
