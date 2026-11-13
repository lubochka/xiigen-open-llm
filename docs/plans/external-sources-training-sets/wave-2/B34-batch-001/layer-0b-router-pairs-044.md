# Layer 0-B — Router Pairs (044)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-044-001 — Round 3 advisory vocabulary routing summary

All advisory vocabulary → GapHandler when predicate set = []:
- consult on/with → GapHandler → gap-skills-guidance-mentor-001
- seek advice on → GapHandler → gap-skills-guidance-mentor-001
- get feedback on → GapHandler → gap-skills-guidance-mentor-001
- sanity check [X] with → GapHandler → gap-skills-guidance-mentor-001
- second opinion → GapHandler → gap-skills-guidance-mentor-001
- rubber duck → GapHandler → gap-skills-guidance-mentor-001
- sounding board → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-044-002 — "explain and build" routing = ImplementationFlow41

A: "Explain the auth flow to the team and build the OAuth2 (OAuth 2.0) authorization module."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "explain to team" = gap warning (explain always gap); "build OAuth2 module" = IMPLEMENT → ImplementationFlow41.

---

## B34-router-044-003 — "explain and document" routing = DocumentationFlow

A: "Explain the data model to the new analysts and document the entity relationship diagram."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "explain to analysts" = gap warning; "document entity relationship diagram" = DOCUMENT → DocumentationFlow.

---

## B34-router-044-004 — "explain and test" routing = UnitTestingFlow51

A: "Explain the mocking strategy to the junior devs and write unit tests for the invoice generator."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "explain to junior devs" = gap warning; "write unit tests" = TEST → UnitTestingFlow51.

---

## B34-router-044-005 — "explain and [SECURITY]" routing = SecurityAuditFlow

A: "Explain the secure coding principles to the team and harden the input validation layer."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "explain to team" = gap warning; "harden input validation" = SECURITY → SecurityAuditFlow.
