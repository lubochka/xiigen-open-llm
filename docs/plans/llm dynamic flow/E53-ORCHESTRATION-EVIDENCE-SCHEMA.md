# E53 Orchestration Evidence Schema

Scenario id: `E53-SCENARIO-001-COMPLEX-ONBOARDING-MIGRATION`

Experiment: `E53-HIGH-LEVEL-ORCHESTRATION-PROOF`

Date: 2026-05-04

## Purpose

This file defines the evidence that the E53 deterministic experiment must write.

The evidence must prove orchestration, not only test success.

The evidence must be readable without rerunning the experiment.

## Required Evidence Files

The experiment must write three files for each run:

```text
experiments-v2/E53-HIGH-LEVEL-ORCHESTRATION-PROOF/results/run-YYYY-MM-DD-###.json
experiments-v2/E53-HIGH-LEVEL-ORCHESTRATION-PROOF/results/run-YYYY-MM-DD-###.md
experiments-v2/E53-HIGH-LEVEL-ORCHESTRATION-PROOF/results/run-YYYY-MM-DD-###.mmd
```

JSON proves the machine trace.

Markdown proves the human-readable result and review summary.

Mermaid proves the topology that actually ran.

## Top-Level JSON Shape

```json
{
  "experiment_id": "E53-HIGH-LEVEL-ORCHESTRATION-PROOF",
  "scenario_id": "E53-SCENARIO-001-COMPLEX-ONBOARDING-MIGRATION",
  "run_id": "run-YYYY-MM-DD-###",
  "run_timestamp_utc": "YYYY-MM-DDTHH:mm:ssZ",
  "source_packet_path": "C:/Users/lubao/Documents/xiigen/llm mvp/llm dynamic flow/E53-SCENARIO-001-SOURCE-PACKET.md",
  "expected_trace_path": "C:/Users/lubao/Documents/xiigen/llm mvp/llm dynamic flow/E53-SCENARIO-001-EXPECTED-TRACE.md",
  "stack_independent": true,
  "live_external_calls": 0,
  "node_trace": [],
  "edge_trace": [],
  "array_counts": [],
  "dependency_trace": [],
  "parallel_trace": [],
  "convergence_report": {},
  "knowledge_edge_context_pack": {},
  "arbiter_selections": [],
  "arbiter_verdicts": [],
  "human_fragments": [],
  "final_human_result": "",
  "learning_gap_packets": [],
  "metrics": {},
  "pass_gates": [],
  "diagnostic_classification": {}
}
```

## Node Trace Item

Each node trace item must include:

```json
{
  "node_id": "node-03",
  "node_name": "scheme_planner",
  "node_kind": "single",
  "input_ids": ["t01_provider_identity_entry"],
  "output_ids": ["b01_validate_provider_identity"],
  "input_count": 11,
  "output_count": 11,
  "status": "completed",
  "lost_edges": [],
  "blocked_edges": [],
  "rejected_edges": [],
  "knowledge_edges": [],
  "boundary_edges": [],
  "notes": "Human-readable explanation of this station's decision."
}
```

Rules:

- `node_kind` is `single` or `array`.
- Array nodes must record one output per input unless an item is intentionally blocked.
- Every blocked item must name why it was blocked.
- Every rejected item must name the rejecting rule.
- Every knowledge edge must name the missing competence.

## Edge Trace Item

Each edge trace item must include:

```json
{
  "edge_id": "edge-03",
  "from_node": "node-02",
  "to_node": "node-03",
  "items_moved": ["t01_provider_identity_entry", "t02_email_registration_state"],
  "items_expected": 11,
  "items_actual": 11,
  "dropped_items": [],
  "merged_items": [],
  "blocked_items": [],
  "status": "preserved"
}
```

Pass rule:

`dropped_items` must be empty unless the item is explicitly blocked, rejected, or converted into a knowledge gap with a recorded reason.

## Array Count Item

Each array node must write a count item:

```json
{
  "node_id": "node-02",
  "node_name": "domain_translator_array",
  "input_count": 11,
  "output_count": 11,
  "expected_output_count": 11,
  "contract_held": true,
  "collapsed_array": false
}
```

Required array checks:

- node-02 domain translator array: 11 in, 11 out.
- node-04 processor array: 11 in, 11 out.
- node-05 applier array: 11 in, 11 out.
- node-06 result translator array: one fragment per accepted, blocked, rejected, or knowledge-edge formal result that must be shown to the user.

## Dependency Trace Item

Each dependency must be checked:

```json
{
  "dependency_id": "dep-provider-before-account",
  "source": "b01_validate_provider_identity",
  "target": "b03_resolve_existing_or_pending_account",
  "required_order": "source_before_target",
  "actual_order_valid": true,
  "violation_reason": ""
}
```

Required dependencies:

- provider trust before account resolution;
- account resolution before questionnaire continuation;
- policy acceptance before full activation;
- valid trust proof before continuation;
- resume-state rules before exact resume promise;
- knowledge-edge source authority before biometric proofing becomes an activation rule.

## Parallel Trace Item

Each parallel group must be checked:

```json
{
  "parallel_group_id": "pg-initial-evidence",
  "items": [
    "b01_validate_provider_identity",
    "b02_identify_email_started_users",
    "b08_prepare_policy_acceptance_gap",
    "b10_prepare_biometric_context_pack"
  ],
  "allowed": true,
  "reason": "These items inspect independent evidence and do not execute dependent activation work.",
  "invalid_parallel_dependencies": []
}
```

Pass rule:

No item may run in parallel with an item it depends on.

## Convergence Report Shape

```json
{
  "convergence_status": "converged_with_rejections_boundaries_and_knowledge_edge",
  "accepted_fragments": [],
  "blocked_fragments": [],
  "rejected_fragments": [],
  "knowledge_edge_fragments": [],
  "pending_fragments": [],
  "conflicts_detected": [],
  "dependencies_preserved": [],
  "missing_fragments": [],
  "duplicate_fragments": []
}
```

Required contents:

- duplicate-account creation appears in rejected fragments.
- biometric passport verification appears in knowledge-edge fragments.
- saved-state resume appears in blocked or boundary fragments.
- policy acceptance appears in blocked or boundary fragments.
- expired proof appears as a blocked continuation condition.

## Knowledge Edge Context Pack Shape

```json
{
  "context_pack_id": "cp01_biometric_passport_verification",
  "edge_type": "hard_knowledge_edge",
  "station_that_reached_edge": "scheme_planner",
  "missing_competence": "biometric passport verification for onboarding activation",
  "known_local_coverage": [],
  "missing_rules": [],
  "question_to_outside_help": [],
  "required_answer_shape": [],
  "live_external_call": false,
  "approval_required_before_live_call": true
}
```

Pass rule:

The context pack must exist. `live_external_call` must be false in the first E53 proof.

## Arbiter Selection Shape

```json
{
  "arbiter": "identity_consistency",
  "mandatory": true,
  "reason": "Duplicate account creation is explicitly requested and must be rejected.",
  "selected_from": ["r04_duplicate_rejection", "meaning_graph.conflicts"]
}
```

Required arbiters:

- identity_consistency;
- token_safety;
- policy_evidence;
- resume_state_coverage;
- knowledge_edge_non_invention;
- audit_traceability;
- stack_independence_result_check.

## Arbiter Verdict Shape

```json
{
  "arbiter": "identity_consistency",
  "target_fragment": "r04_duplicate_rejection",
  "verdict": "accepted_rejection",
  "allowed_to_continue": true,
  "blocked_if": "Any second account branch survives.",
  "reason": "The accepted work is rejection of duplicate account creation."
}
```

Pass rule:

No arbiter verdict may turn rejected, blocked, or knowledge-edge material into ordinary accepted work.

## Human Fragment Shape

```json
{
  "fragment_id": "h02_duplicate_rejected",
  "source_results": ["r04_duplicate_rejection"],
  "status": "rejected",
  "text": "The request to create a second account from a repeated provider callback is rejected."
}
```

Pass rule:

The fragment status must match the formal result status.

## Learning Gap Packet Shape

```json
{
  "packet_id": "lg01_biometric_passport_verification",
  "target_station": "knowledge_edge_detector",
  "target_unit_role": "identity_proofing_knowledge_edge",
  "status": "knowledge_edge",
  "source_facts": ["F08", "C02", "U02", "U03"],
  "arbiter_status": "knowledge_edge_non_invention_required",
  "learning_value": "Teach how biometric passport verification should be routed, context-packed, externally reviewed, adapted, and later turned into local training after source authority exists."
}
```

Pass rule:

Every learning gap packet must target a specific station and unit role.

## Metrics Shape

```json
{
  "lost_edge_count": 0,
  "node02_input_count": 11,
  "node02_output_count": 11,
  "node04_input_count": 11,
  "node04_output_count": 11,
  "node05_input_count": 11,
  "node05_output_count": 11,
  "dependency_violation_count": 0,
  "invalid_parallel_run_count": 0,
  "invented_biometric_plan_items": 0,
  "duplicate_creation_accepted": false,
  "forbidden_stack_term_count": 0,
  "human_result_sections_present": "5/5",
  "live_external_calls": 0,
  "learning_gap_packet_count": 4,
  "e2e_orchestration_success": true
}
```

## Pass Gate Shape

Each pass gate must be written explicitly:

```json
{
  "gate": "Dependency management",
  "required": "0 dependency violations",
  "actual": "0 dependency violations",
  "passed": true,
  "failure_diagnostic_if_failed": "node-03 planner or orchestrator dependency runner"
}
```

Required pass gates:

- stack independence;
- edge preservation;
- domain coverage;
- array contract;
- dependency management;
- parallel management;
- knowledge edge;
- external call policy;
- arbiter authority;
- result quality;
- evidence completeness;
- existing test safety.

## Markdown Evidence Shape

The markdown evidence must include:

1. Scenario id.
2. Short human summary.
3. What can proceed.
4. What is blocked.
5. What is rejected.
6. What is unknown.
7. Next actions.
8. Metrics table.
9. Arbiter verdict table.
10. Learning gap packet table.
11. Diagnostic section explaining what would be trained next if the proof failed.

## Mermaid Evidence Shape

The Mermaid file must show:

- source input;
- node-00;
- node-01;
- node-02 array;
- node-03;
- node-04 array;
- node-05 array;
- convergence;
- knowledge-edge context pack;
- arbiter selector;
- arbiter array;
- verdict boundary;
- result translator array;
- formatter;
- final human decision;
- learning gap packets.

The Mermaid graph must show the knowledge-edge branch and the arbiter branch separately.

## Failure Classification

If a gate fails, the evidence must classify it:

| Failure | Diagnostic classification |
|---|---|
| Edge lost | edge preservation or trace schema failure |
| Array collapsed | array-node contract failure |
| Dependency wrong | scheme planner or orchestration runner failure |
| Invalid parallel work | parallel management failure |
| Duplicate creation accepted | arbiter authority failure |
| Biometric proof invented | knowledge-edge failure |
| Activation allowed without policy evidence | policy evidence arbiter failure |
| Expired proof accepted | token safety arbiter failure |
| Final answer unreadable | result translator or formatter failure |
| Stack term appears in final result | stack-independence formatter failure |

## Evidence Review Rule

A green test count is not enough.

The reviewer must be able to open the markdown evidence and understand the orchestration decision without reading the code.

The reviewer must be able to open the JSON evidence and verify every metric without interpreting prose.

The reviewer must be able to open the Mermaid evidence and see the actual route that ran.
