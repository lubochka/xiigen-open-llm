# Layer 0-D — Flow Factory Pairs (026)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B38-0D-026-001 — PreReleaseTag flow factory: QA alone

Scenario: PreReleaseTagApproval
Input: "Approve the pre-release tag for the RC (Release Candidate) build."
PredicateSet: ["QA"]

Flows created:
1. ReleaseGateFlow71 (Phase 7.1, QA, order 1)

TransitionType: no_contribution
FlowCount: 1

---

## B38-0D-026-002 — RCCandidate flow factory: QA+DOCUMENT

Scenario: RCCandidateDocApproval
Input: "Document the RC acceptance criteria and sign off the release."
PredicateSet: ["QA","DOCUMENT"]

Flows created:
1. DocumentationFlow (Phase 2.1, DOCUMENT, order 1)
2. ReleaseGateFlow71 (Phase 7.1, QA, order 2 — last)

TransitionType: no_contribution
FlowCount: 2

---

## B38-0D-026-003 — NightlyBuild flow factory: QA+IMPLEMENT

Scenario: NightlyBuildValidation
Input: "Build the nightly release service and validate the build output."
PredicateSet: ["QA","IMPLEMENT"]

Flows created:
1. BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
2. ReleaseGateFlow71 (Phase 7.1, QA, order 2 — last)

TransitionType: feature_addition
FlowCount: 2

---

## B38-0D-026-004 — WeeklyRelease flow factory: DOCUMENT alone

Scenario: WeeklyReleaseProcessDoc
Input: "Document the weekly release process criteria."
PredicateSet: ["DOCUMENT"]

Flows created:
1. DocumentationFlow (Phase 2.1, DOCUMENT, order 1)

TransitionType: no_contribution
FlowCount: 1
