# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B38-L1-010-001 — Canonical B38 orchestration patterns

Pattern A: QA-only (no contribution)
Predicates: ["QA"]
Flows: [ReleaseGateFlow71]
Order: QA (only)
TransitionType: no_contribution
File count: 2
Use case: Sign-off collection, approval-only, post-release validation

Pattern B: QA + IMPLEMENT (feature_addition)
Predicates: ["QA", "IMPLEMENT"]
Flows: [BackendServiceFlow41, ReleaseGateFlow71]
Order: IMPLEMENT → QA
TransitionType: feature_addition
File count: 3
Use case: Service release gate, build artifact audit, CI/CD pipeline review

Pattern C: QA + DOCUMENT (no contribution)
Predicates: ["QA", "DOCUMENT"]
Flows: [DocumentationFlow, ReleaseGateFlow71]
Order: DOCUMENT → QA
TransitionType: no_contribution
File count: 2
Use case: Release criteria validation, acceptance criteria sign-off

Pattern D: QA + SECURITY (no contribution)
Predicates: ["QA", "SECURITY"]
Flows: [SecurityAuditFlow, ReleaseGateFlow71]
Order: SECURITY → QA
TransitionType: no_contribution
File count: 3
Use case: Pre-release security audit, compliance check before go-live

Pattern E: Full four-predicate (feature_addition)
Predicates: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
Flows: [BackendServiceFlow41, DocumentationFlow, SecurityAuditFlow, ReleaseGateFlow71]
Order: IMPLEMENT → DOCUMENT → SECURITY → QA
TransitionType: feature_addition
File count: 6
Use case: Full release gate pipeline

Pattern F: knowledge_edge
Predicates: []
Flows: []
Order: N/A
TransitionType: N/A
File count: 0
Use case: Pure operational rollout/cutover/runbook vocabulary

---

## B38-L1-010-002 — Pattern selection by input vocabulary

Input contains "service" or "build" or "pipeline" → Pattern B or E
Input contains "criteria" but no code terms → Pattern C
Input contains "security" + "audit" → Pattern D or E
Input contains all four → Pattern E
Input contains only sign-off/approve/verify → Pattern A
Input is pure gap vocabulary → Pattern F
