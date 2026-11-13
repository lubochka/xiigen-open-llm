# Layer 0-B — Router Pairs (017)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-017-001 — [QA, IMPLEMENT]: version tagging build verification

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: VersionTaggingRelease
Input: "Verify the build version tag matches the release candidate and approve the final tag."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41) — from "build"
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition
OutputFiles: [build_provenance_record.md, version_tag_approval.md, release_gate_report.md]

---

## B38-0B-017-002 — [QA, DOCUMENT]: version tagging criteria

PredicateSet: ["QA", "DOCUMENT"]
Scenario: VersionTaggingCriteriaValidation
Input: "Validate the version tagging criteria and confirm all release criteria are correctly applied."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution

---

## B38-0B-017-003 — [QA, IMPLEMENT]: version tag with service and build (consolidation)

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: PaymentServiceVersionTag
Input: "Sign off on the version tag for the payment service build before production promotion."

Routing decisions:
1. IMPLEMENT → Phase 4.1 — "service" + "build" both fire IMPLEMENT; they consolidate to ONE predicate
2. QA → Phase 7.1 — QA last

DecisionCount: 2
TransitionType: feature_addition

Note: Multiple IMPLEMENT triggers (service + build) consolidate into a single IMPLEMENT predicate. DecisionCount is still 2 (not 3). This is IMPLEMENT deduplication.

---

## B38-0B-017-004 — Knowledge_edge: version tag rollout gap

PredicateSet: []
Scenario: VersionTagRollout
Input: "Rollout the version tag to all environments on go-live day."

Routing: knowledge_edge
CapabilityGapWarnings: ["rollout", "go-live"]
DecisionCount: 1 (knowledge_edge sentinel)
TransitionType: N/A
