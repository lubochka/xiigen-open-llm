# S28 — path hard-negative admission / Student DPO separation

## Versioned diagnostic/evidence record (proposed, NOT asserted existing implementation)
```json
{
  "schema": "xiigen.s28.path-preference.v1",
  "task_id": "actual-id", "project_scope": "authorized-id",
  "subject_law_id": "versioned-law-passport", "task_intent": "CODE_Q",
  "source_family_id": "TRAIN-only-id", "source_rights_ref": "approved-ref",
  "input_digest": "sha256", "condition_digest": "sha256",
  "budget_id": "frozen-pc-policy",
  "path_a": {"path_digest":"sha256","executed":true,"eligible":true,"type_associations":[],"unit_ids":[],"flow_corrections":[],"orchestrator_branches":[],"graph_patterns":[],"retrieval_evidence_ids":[],"state_refs":[],"result_digest":"sha256","measured_cost_ref":"receipt"},
  "path_b": {"path_digest":"sha256","executed":true,"eligible":true,"type_associations":[],"unit_ids":[],"flow_corrections":[],"orchestrator_branches":[],"graph_patterns":[],"retrieval_evidence_ids":[],"state_refs":[],"result_digest":"sha256","measured_cost_ref":"receipt"},
  "adjudication": {"status":"LAWFUL_STRICT_PREFERENCE","preferred_path":"a","judge_receipt":"independent-real","law_vetoes":[],"rubric_version":"frozen"},
  "routing": {"existing_owner":"RP11/RP13 verified method","mutable_state_id":"real-or-none","student_dpo_eligible":false}
}
```
Fields are illustrative requirements; actual schema must reuse existing owner record if equivalent. Never place private corpus/holdout answers in public plan/trace. SHA digest is identity evidence, not mathematical equivalence or successful training.

## Admission
1. Real same `x`, domain, law/edition, rights, frozen parent and PC budget; actually executed candidate alternatives. If not, NO_PAIR.
2. D-15 arbiter/type/rights constraints pass for BOTH paths. Law veto is a protected deterministic example, not a reward preference.
3. Independent judge compares OUTPUT plus all-branch cost under frozen user utility. TIE/BOTH_BAD/UNJUDGEABLE not admitted.
4. No leakage from SEALED_CERT, no duplicated path ID, no cross-project/edition vector-state collision.
5. Route admitted diagnostic to the EXISTING trained recipient (if traced). For policy-level rank learning, named existing owner must justify objective, reference, state and actual normal consumer. No new S28 optimizer.
6. Student DPO is an entirely different admission: CODEX Astra actually chooses lesson; APPROVED external open Chinese model authors BOTH final chosen+rejected for same exact x; CODEX separately assesses unmodified exact pair; RP01 uses correct response-only DPO objective and actual Student/LoRA owner. The role's "path loser" does not become final rejected side.
7. One pair_id/step_id applied at most once under current active WorkLeft conformance. Historical D-06 replay=70/30 remains a named owner conflict, NOT secretly applied in this one-pair pilot. Exact material branch vs legacy Compact(480) must be traced by real current version.
8. Trial never automatically promotes: frozen independent quality, old-domain retention, full-PC cost, cold ordinary adoption and rollback require their own receipts.

## Canonical review outcomes
`GUARDRAIL_VETO`; `NOT_COMPARABLE`; `TIE`; `BOTH_BAD`; `UNJUDGEABLE`; `LAWFUL_STRICT_PREFERENCE`; `LEAKAGE`; `NO_REAL_CONSUMER`; `RECIPIENT_OPEN`; `NOT_AUTHORIZED`; `NOT_RUN`. All are legitimate falsifiable outcomes; only actual LAWFUL_STRICT_PREFERENCE plus independently established recipient gives a policy-learning candidate.
