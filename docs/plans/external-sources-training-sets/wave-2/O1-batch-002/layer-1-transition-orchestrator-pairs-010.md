# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: orchestration

---

## O1b-L1-010-001 — QA always last: SECURITY+QA → SecurityVerification34Flow → QAGatingFlow73

A: SelectedLogicalTransition: { predicates: ["SECURITY", "QA"], transition_type: "feature_addition", routing: [{ sub_phase: "3.4", state: "trusted_learned" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", sub_phase: "3.4", execution: "first" }, { flow: "QAGatingFlow73", sub_phase: "QA", execution: "last", note: "QA gates the result after all domain flows complete" }], execution: "sequential" }

---

## O1b-L1-010-002 — QA execution is always last

A: Why does QA always run last in orchestration?

B: QA gating requires all domain outputs to be available before it can validate. QA cannot run in parallel with domain flows because it reviews the final result, not the intermediate outputs. By canonical ordering, QA is absolute last. QAGatingFlow73 runs after all other flows complete and result fragments are assembled.

---

## O1b-L1-010-003 — QA with full predicate set

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "IMPLEMENT", "QA"] }

B: OrchestrationPlan: { flows: [{ sub_phase: "3.4", execution: "first" }, { sub_phase: "4.1", execution: "parallel_or_after_3.4" }, { sub_phase: "QA", execution: "absolute_last" }] }

---

## O1b-L1-010-004 — QA does not add a routing decision but does spawn a flow

A: Predicate QA: routing decision vs. orchestration flow

B: ROUTING: QA does not add a routing decision (no sub-phase mapping). ORCHESTRATION: QA does spawn QAGatingFlow73 as the final step. This is the only case where a predicate (QA) maps to an orchestration step but not to a routing decision. The QAGatingFlow is implicit — it is always appended when QA predicate is present.

---

## O1b-L1-010-005 — QA + knowledge_edge: QA cannot gate an empty result

A: Predicates: ["QA"] → orchestration?

B: OrchestrationPlan: { flows: [], execution: "none", reason: "QA alone produces no domain flow; no result to gate" }
   Note: QA alone is no_contribution at TransitionType level, and produces no flows (no domain flows to gate). QA only gates when at least one other predicate produces a domain flow result.
