# WDA-00 Executor Contract
# Lifecycle Topology Bootstrap
# Round A - Implementation instructions for Round B
# Date: 2026-05-04

---

## Round B Scope

Implement the deterministic WDA-00 topology bootstrap proof.

One thing is under test: the typed topology mutation surface can represent the
whole development lifecycle as mutable logical topology data.

Do not add document classification, natural-language topology mutation, phase
orchestration, transition-gate behavior, or learning behavior in this round.

---

## Pre-Condition

The codebase must already have:

- `ILogicalTopologyRegistry`
- typed topology command records for phases, sub-phases, and transitions
- topology mutation units for phases, sub-phases, and transitions
- `TopologyMutationFlow`
- `LogicalOrchestratorAssistant` typed command route

If any of these are missing, stop and report the missing runtime surface. Do
not fake the topology bootstrap outside the existing logical surface.

---

## Required Test

Add one deterministic test for WDA-00.

Recommended name:

```text
WDA00_TopologyBootstrap_RegistersWholeDevelopmentLifecycle
```

The test should:

1. Create an in-memory topology registry.
2. Register the topology mutation flow and required mutation units.
3. Send typed commands through `LogicalOrchestratorAssistant`.
4. Add or update seven phases.
5. Add or update thirty-one sub-phases.
6. Add or update six transitions.
7. Run one edit probe that updates an existing item without changing count.
8. Assert all metrics from `WDA-00-EXPECTED-TRACE.md`.
9. Write evidence artifacts if the experiment harness supports evidence
   writing.

---

## Required Evidence

Round B must produce:

- JSON evidence
- Markdown human result
- Mermaid lifecycle topology
- Round manifest
- Round zip

The JSON evidence must contain every metric from `WDA-00-EXPECTED-TRACE.md`.

The markdown human result must pass `WDA-00-HUMAN-RESULT-CHECKLIST.md`.

The Mermaid topology must show seven phase nodes and six transition edges.

---

## Required Assertions

The deterministic test must assert:

```text
selected_flow_id == topology-mutation-flow
intent_source == typed-command
natural_language_intent_classifier_used == false
phase_count == 7
sub_phase_count == 31
transition_count == 6
duplicate_identifier_count == 0
transition_with_missing_source_count == 0
transition_with_missing_target_count == 0
edit_operation_preserves_identifier == true
canonical_sub_phase_count_corrected == true
human_result_phase_sections_present == 7
e2e_topology_bootstrap_success == true
```

---

## Forbidden Implementation Moves

- Do not hardcode the final counts without exercising the topology mutation
  flow.
- Do not write directly to the registry while bypassing the assistant route.
- Do not use natural-language intent classification in this round.
- Do not change the WDA-00 oracle to match a failing implementation.
- Do not reduce sub-phase coverage from 31 to 27.
- Do not proceed to WDA-01 until WDA-00 passes.

---

## Expected Outcome

WDA-00 Round B passes when the orchestrator can represent the lifecycle
topology through typed logical mutation commands and produce a human-readable
lifecycle map.

This proves the topology surface. It does not prove document classification,
phase execution, transition continuity, or learning.

---

## Next Action After Passing

Create WDA-01 Round A artifacts for document-to-sub-phase classification:

1. source document selection ledger
2. expected classification trace
3. confidence and wrong-domain guard checklist
4. executor contract for deterministic classification probes
