# Layer 0-B — Router Pairs (051)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-051-001 — "consult on" security topic = GapHandler (not SecurityAuditFlow)

A: "Consult with the security team on the authorization strategy."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Key distinction: consulting ABOUT security ≠ performing security action.

---

## B34-router-051-002 — "seek advice on" documentation topic = GapHandler (not DocumentationFlow)

A: "Seek advice on the best way to document the service contracts."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Key distinction: seeking advice ABOUT documentation ≠ writing documentation.

---

## B34-router-051-003 — "get feedback on" test results = GapHandler (not UnitTestingFlow51)

A: "Get feedback on the test coverage results from the QA (Quality Assurance) lead."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Key distinction: getting feedback ON test results ≠ writing or running tests.

---

## B34-router-051-004 — "sanity check" API contract = GapHandler (not APIContractFlow33)

A: "Sanity check the API (Application Programming Interface) contract design with the platform architect."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Key distinction: checking/reviewing API design ≠ integrating/publishing API contract.

---

## B34-router-051-005 — "sounding board for architecture" = GapHandler (not ArchitectureFlow31)

A: "Use the principal engineers as a sounding board for the cloud-native architecture approach."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Key distinction: advisory review of architecture ≠ designing/architecting the system.
