# E53 Round D Validation Results

Date: 2026-05-04

Round: 16

Artifact under review: Round D trained node-00 compiler replacement.

## Verdict

Round D passes.

The E53 orchestrator now runs node-00 through a trained Flow-01 logical compiler adapter and keeps Round C's trained node-01 splitter active. The compiler preserved all signals required by node-01, and the full E53 trace remains successful.

## Structural Verification

Files claimed by the manifest are present:

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\Experiments\E53HighLevelOrchestrationProof.cs`
- `C:\Projects\llm_mvp_core\experiments-v2\E53-HIGH-LEVEL-ORCHESTRATION-PROOF\results\run-2026-05-04-144327.json`
- `C:\Projects\llm_mvp_core\experiments-v2\E53-HIGH-LEVEL-ORCHESTRATION-PROOF\results\run-2026-05-04-144327.md`
- `C:\Projects\llm_mvp_core\experiments-v2\E53-HIGH-LEVEL-ORCHESTRATION-PROOF\results\run-2026-05-04-144327.mmd`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\E53-ROUND-D\BATCH-ROUND-SUMMARY.md`

Counts are backed by the latest evidence:

- domain packets: 11.
- node-02 input count: 11.
- node-02 output count: 11.
- learning gap packets: 4.
- live external calls: 0.
- `compiled_logical_structure.local_subject`: present.

Test counts came from live runs:

- focused E53 after replacement: 12 passed, 0 failed.
- full suite after replacement: 997 passed, 0 failed.

## Contract Verification

Round D contract: replace only node-00.

Result: passed.

Node-01 remains trained from Round C. Nodes 02 through 09 remain deterministic. The expected trace oracle and test assertions were not changed.

Round D contract: trained node-00 must preserve all node-01 splitter signals.

Result: passed.

Signals present in latest evidence:

- units: `provider_sign_in`, `email_started_users`, `questionnaire_resume`, `duplicate_prevention`, `policy_acceptance`, `token_session_safety`, `biometric_passport_knowledge_edge`, `audit_clarity_request`, `human_decision_output`.
- dependencies: `provider_sign_in -> duplicate_prevention`, `provider_sign_in -> account_resolution`, `account_resolution -> questionnaire_resume`, `policy_acceptance -> full_activation`, `token_session_safety -> continuation`, `biometric_passport_knowledge_edge -> knowledge_edge_context_pack`.
- conflicts: `audit_clarity_request`, `duplicate_account_creation_requested`.
- boundaries: `resume_state_rules_missing`, `policy_acceptance_rules_need_source_evidence`.
- knowledge edges: `missing_biometric_passport_verification`.
- required outputs: `human_readable_decision`.
- no-stack constraint: true.

Round D contract: the trained node-01 splitter must still emit exactly 11 packets.

Result: passed.

The latest evidence contains all expected packet ids from `d01_provider_identity_entry` through `d11_human_readable_decision`.

## Judgment Verification

The important judgment question is whether the trained compiler preserved relationship-bearing signals, not just surface labels.

It did.

The duplicate conflict is still two signals: `audit_clarity_request` and `duplicate_account_creation_requested`. This keeps audit clarity separate from duplicate rejection.

The missing coverage is still separated into two boundaries and one knowledge edge: resume-state rules, policy-acceptance evidence, and biometric passport verification.

The no-stack constraint is preserved as a node-00 signal before formatting, so stack independence is not only a formatter behavior.

The trained node-00 compiler introduced no metric variance relative to Round C.

## Observation

The evidence schema now includes `compiled_logical_structure`. This is an intentional Round D addition because the Round D protocol requires `local_subject` to be visible in the JSON grep gate and in the batch signal comparison.

## Next Action Item

Continue Batch 1 with Round E: replace node-03 scheme planner with a trained planner while leaving node-02 deterministic. The Round E gate should focus on dependency graph preservation, especially provider trust before account resolution and b08 early-start versus b09 resolved-state dependency.
