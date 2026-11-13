# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-007-001 — Phase 7 sub-phase map

Phase 7 covers Automation sub-phases. In B38, the active sub-phase is 7.1.

| Sub-phase | Name | Primary Predicate(s) | Flow |
|---|---|---|---|
| 7.1 | Release and Deployment Automation | QA | ReleaseGateFlow71 |
| 7.2 | CI/CD Automation | QA + IMPLEMENT | CiCdFlow72 |
| 7.3 | QA Test Automation | TEST + QA | TestAutomationFlow73 |
| 7.4 | Infrastructure Automation | ARCHITECTURE + IMPLEMENT | InfrastructureFlow74 |

B38 is Sub-phase 7.1 because the scenario is release gates and go-live checklists. This is pure release governance — QA is the dominant predicate.

---

## B38-0C-007-002 — Why B38 is 7.1, not 7.2

7.2 (CI/CD Automation) would apply if the primary focus were building and automating the CI/CD pipeline itself — the implementation of the pipeline.

7.1 (Release and Deployment Automation) applies when the primary focus is the release gate process — approvals, sign-offs, quality gates, go/no-go decisions.

B38 Scenario: Project delivery — release gates and go-live checklists → 7.1

Even when "CI/CD pipeline" appears in B38 input (and IMPLEMENT fires from "pipeline"), the routing still goes to 7.1 for QA and 4.1 for IMPLEMENT. The sub-phase label "7.1" is the scenario label, not a restriction on which phases can be routed in the nine-node flow.

---

## B38-0C-007-003 — Predicate ownership by sub-phase

Sub-phase 7.1 owns: QA (exclusive for release gate context)
Sub-phase 7.2 owns: QA + IMPLEMENT (both required for CI/CD)
Sub-phase 7.3 owns: TEST + QA (both required for test automation)
Sub-phase 7.4 owns: ARCHITECTURE + IMPLEMENT (both required for infrastructure)

When a sentence fires only QA, it belongs to 7.1 (not 7.2, 7.3, or 7.4).
When a sentence fires QA + IMPLEMENT in a CI/CD context specifically, it may belong to 7.2.
B38 batch uses 7.1 as the canonical sub-phase across all scenarios in this batch.

---

## B38-0C-007-004 — Other phases referenced in B38 routing

Phase 2 (Documentation):
- 2.1: Technical documentation (criteria documents)
- 2.4: User documentation (release notes, user-facing docs)

Phase 3 (Security):
- 3.4: Security audit and compliance (SECURITY predicate)

Phase 4 (Implementation):
- 4.1: Backend service implementation (IMPLEMENT predicate)

Phase 7.1 is the anchor phase for B38. Predicates beyond QA route to their own phase. The nine-node flow in B38 is labeled ReleaseGateFlow71 and lives in Phase 7.1.
