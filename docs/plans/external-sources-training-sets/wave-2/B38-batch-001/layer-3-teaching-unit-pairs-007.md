# Layer 3 — Teaching Unit Pairs (007)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-007-001 — Teaching: Phase 7 sub-phase map

LESSON: Phase 7 has four sub-phases with different predicate ownership. B38 uses sub-phase 7.1.

PHASE 7 SUB-PHASES:

Sub-phase 7.1 — Release and Deployment Automation
Primary predicate: QA
Flow: ReleaseGateFlow71
Use case: Release gates, go-live approvals, sign-off collection, deployment authorization
B38 batch focus: YES — this is the B38 sub-phase

Sub-phase 7.2 — CI/CD Automation
Primary predicates: QA + IMPLEMENT (both required)
Flow: CiCdFlow72
Use case: Building and automating CI/CD pipelines
B38 batch focus: Tangential (CI/CD pipeline review appears in B38 but routes to 7.1+4.1, not 7.2)

Sub-phase 7.3 — QA Test Automation
Primary predicates: TEST + QA
Flow: TestAutomationFlow73
Use case: Automating quality assurance tests
B38 batch focus: No (B38 is release gates, not test automation)

Sub-phase 7.4 — Infrastructure Automation
Primary predicates: ARCHITECTURE + IMPLEMENT
Flow: InfrastructureFlow74
Use case: Infrastructure-as-code, cloud provisioning, environment automation
B38 batch focus: No (B38 is release gates, not infrastructure)

---

## B38-TU-007-002 — Teaching: 7.1 is the anchor phase for B38

All B38 scenarios that produce QA predicate route to Phase 7.1.
The B38 sub-phase label "7.1" identifies the primary phase.
Additional predicates (IMPLEMENT, DOCUMENT, SECURITY) route to their own phases (4.1, 2.1, 3.4).

The scenario label "B38 — sub-phase 7.1" means:
"This is a release gate scenario. QA is the primary predicate. ReleaseGateFlow71 is the anchor flow."

It does NOT mean:
"All routing goes to 7.1 only."
Other predicates still route to 4.1, 2.1, 3.4 as appropriate.

---

## B38-TU-007-003 — Teaching: 7.3 boundary (TEST vs QA in B38)

B38 does NOT involve Test Automation (sub-phase 7.3).

TEST predicate fires on: writing tests, authoring test suites, creating test cases.
In B38, "tests have passed" describes a past execution result — this is not TEST predicate.
In B38, "validate acceptance tests" fires QA (validate) not TEST.

If a sentence in B38 context says "write new acceptance tests for the release" → TEST would fire.
But B38 canonical sentences are about gates and approvals, not test authoring.

Remember: the sub-phase classification is part of the scenario (B38 = release gates), not just the predicate.
