# T34 Transition Executor Contract
# Round B deterministic transition gate proof
# Date: 2026-05-04

## Scope

Round B implements a deterministic experiment for the T34 Phase 3 -> Phase 4
transition gate.

It must use the Round A oracle documents exactly as written:

- `T34-SOURCE-BINDING-v1.md`
- `T34-TRANSITION-DOMAIN-MAP-v1.md`
- `T34-TRANSITION-FAILURE-TAXONOMY-v1.md`
- `T34-TRANSITION-EXPECTED-TRACE-v1.md`
- `T34-TRANSITION-HUMAN-RESULT-STANDARDS-v1.md`

## What Round B Must Prove

The transition gate must produce:

- T34-1: CONCERN for BaseService stated-but-unenforced constraint.
- T34-2: CONCERN for cross-phase context packet absent out-of-scope marker.
- T34-3: PASS for build-time sequence versus runtime topology distinction.
- Overall verdict: PASS WITH GAPS.
- Block count: 0.

## Files To Read First

Read in this order:

1. `T34-SOURCE-BINDING-v1.md`
2. `T34-TRANSITION-DOMAIN-MAP-v1.md`
3. `T34-TRANSITION-FAILURE-TAXONOMY-v1.md`
4. `T34-TRANSITION-EXPECTED-TRACE-v1.md`
5. `T34-TRANSITION-HUMAN-RESULT-STANDARDS-v1.md`
6. `C:\Projects\llm_mvp_core\docs\plans\AUTONOMOUS-LEARNING-SESSION-PLAN-v1.0.md`
7. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`
8. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md`
9. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DECISIONS-LOCKED.md`

## Required Experiment Output

Round B must write:

- JSON evidence file.
- Markdown evidence file.
- Mermaid transition graph.
- Round manifest.
- Round zip.

Evidence JSON must include:

```
experiment_id
source_binding
node_00_compiled_structure
node_01_domain_packets
node_02_formal_translations
node_03_scheme_plan
node_04_05_06_processed_results
node_07_selected_arbiters
node_08_arbiter_verdicts
node_09_human_result
metrics
pass_gates
overall_verdict
```

## Required Metrics

| Metric | Expected |
|---|---|
| upstream_context_categories_visible | 5 |
| node_01_packet_count | 10 |
| node_02_translation_count | 10 |
| scenario_count | 3 |
| concern_count | 2 |
| pass_count | 1 |
| block_count | 0 |
| false_runtime_topology_rejection | false |
| base_service_gap_named | true |
| cross_phase_context_gap_named | true |
| build_time_sequence_passed | true |
| human_result_sections_present | 5 |
| transition_success | true |

## Required Tests

Use names that make the behavior visible:

```
T34_Run_Passes_Phase3ToPhase4Transition_Gates
T34_Node01_Emits_ExpectedTenDomainPackets
T34_BaseServiceConstraint_IsConcern_NotBlock
T34_CrossPhaseContextEdge_IsConcern_NotDropped
T34_BuildTimeSequence_DoesNotReinstateStaticRuntimePipeline
T34_HumanResult_IsPassWithGaps_AndActionable
T34_EvidenceJson_UsesRequiredMetricKeys
```

## What Not To Do

- Do not write learning-set pairs in Round B.
- Do not replace deterministic fixtures with trained units.
- Do not change the Round A oracle to match implementation output.
- Do not block the phase because of the cross-phase context packet edge.
- Do not accept the phase as a clean full pass.
- Do not classify the build-time sequence as a static runtime pipeline.
- Do not use live external AI.

## Round B Pass Gates

Round B passes only if:

- All required tests pass.
- Full suite remains green.
- The evidence JSON contains all required metrics.
- The human result matches the Round A human result standard.
- The round zip contains source, tests, evidence, manifest, and all five Round
  A oracle documents.

## Learning-Set Work After Round B

Only after Round B passes:

1. Apply corrections if the deterministic proof reveals any oracle issue.
2. Write gap-closure pairs for node-02, node-03, node-07, and node-08.
3. Write coverage-extension pairs only for proxied nodes whose outputs were
   validated by Round B evidence.

The B output of every pair must be the actual node output, not a description
of the output.

