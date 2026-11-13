# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B38-L1-004-001 — QA + IMPLEMENT: feature_addition TransitionType; QA last

Input: "Audit the build artifacts and approve the release for the order service component."
PredicateSet: ["QA", "IMPLEMENT"]

Orchestrator decision:
- Two flows: BackendServiceFlow41 (4.1) then ReleaseGateFlow71 (7.1)
- IMPLEMENT executes first; QA last (absolute rule)
- TransitionType: feature_addition

Nine-node traversal:
N01: input received; predicates = ["QA", "IMPLEMENT"]
N02: routing decisions: 4.1 + 7.1
N03: BackendServiceFlow41 and ReleaseGateFlow71 selected
N04: no gap warnings
N05: BackendServiceFlow41 executes (build artifact review; order service review)
N06: build artifacts verified (provenance confirmed)
N07: BackendServiceFlow41 output: build_provenance.md, order_service_review.md
N08: ReleaseGateFlow71 executes (using build_provenance.md as evidence)
N09: result = feature_addition; 3 files; release_approved

---

## B38-L1-004-002 — feature_addition: what changes

TransitionType = feature_addition means:
- Code artifact (build, service, pipeline) was part of this transition
- Phase 4.1 ran and produced code-level output
- The codebase has a new or reviewed code contribution
- N09 records: contribution_type = "code_review" (review of IMPLEMENT artifact)

For build artifacts scenario specifically:
- The build itself represents code that was authored
- Reviewing build artifacts is reviewing code
- feature_addition is correct even though no NEW code is written in this step — the IMPLEMENT artifact exists

---

## B38-L1-004-003 — QA last rule in IMPLEMENT+QA sequencing

Execution order:
1. BackendServiceFlow41 (Phase 4.1, IMPLEMENT): order 1
2. ReleaseGateFlow71 (Phase 7.1, QA): order 2 (last)

This is required by Rule 12 (QA ordering rule absolute).

Violation (would be an error):
1. ReleaseGateFlow71: order 1 (QA first — WRONG)
2. BackendServiceFlow41: order 2 (IMPLEMENT after QA — WRONG)

The gate must receive the implementation review output before making a decision.

---

## B38-L1-004-004 — N09 result for QA+IMPLEMENT

{
  "predicate_set": ["QA", "IMPLEMENT"],
  "transition_type": "feature_addition",
  "phase_routing": ["4.1", "7.1"],
  "decision_count": 2,
  "output_files": ["build_provenance.md", "order_service_review.md", "release_gate_decision.md"],
  "file_count": 3,
  "capability_gap_warnings": [],
  "result_state": "release_gate_approved",
  "domain_id": "software-development"
}
