# Round Manifest - Projects 21-23 and E52 Comparison

## Round Name

Projects 21-23 internal-first transfer and E52 before/after comparison.

## Source Request

Use the E51A/E51B findings in projects 21-23, keep the assistant internal-first,
require explicit approval before external AI, examine returned results internally,
store learning data, schedule learning while idle, and produce a before/after
comparison that can be examined.

## Files Included

- `21.AgentInfrastructure/Loop/InternalLlmGateway.cs`
- `21.AgentInfrastructure/Loop/LearningEventScheduler.cs`
- `21.AgentInfrastructure/Tools/PermissionGate.cs`
- `22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs`
- `23.OssEquivalents/StateMachine/ExternalAiStateGuards.cs`
- `35.ExperimentsV2/ExperimentsV2.csproj`
- `35.ExperimentsV2/ExperimentRunner.cs`
- `35.ExperimentsV2/ExperimentsV2Dispatcher.cs`
- `35.ExperimentsV2/Infrastructure/ResultWriter.cs`
- `35.ExperimentsV2/Experiments/E52Projects2123CycleComparison.cs`
- `Tests/AgentInfrastructureTests.cs`
- `Tests/AgentAlgorithmsTests.cs`
- `Tests/OssEquivalentsTests.cs`
- `Tests/DynamicFlowE51Tests.cs`
- `docs/DECISIONS-LOCKED.md`
- `experiments-v2/E52-PROJECTS-21-23-CYCLE-COMPARISON/results/run-2026-05-04-085736.json`
- `experiments-v2/E52-PROJECTS-21-23-CYCLE-COMPARISON/results/run-2026-05-04-085736.md`
- `SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md`

## What Changed

- Added internal-first gateway routing with adapter selection and route inspection.
- Added external-AI permission requirement for `call_external_ai`.
- Added internal result examination, external request packaging, external result
  adaptation, and duplicate-account arbiter rejection.
- Added state-machine guard for single-use external AI approval.
- Added idle learning scheduler proof for queued learning events.
- Added E52 before/after cycle comparison evidence.
- Locked D-14 and D-15 in the code repository decisions file.

## Verification

- Focused E51/E52 and projects 21-23 tests: `144/144`
- Full test suite after hardening: `916/916`
- Latest E52 evidence shows baseline cycle success `0%` and enhanced cycle
  success `100%` across V06, CH-01, CH-02, and IDLE-01.

## Remaining Gap

The external result adapter currently contains a CH-02-specific duplicate-account
arbiter check. A later round should generalize this into a reusable arbiter
constraint lookup.
