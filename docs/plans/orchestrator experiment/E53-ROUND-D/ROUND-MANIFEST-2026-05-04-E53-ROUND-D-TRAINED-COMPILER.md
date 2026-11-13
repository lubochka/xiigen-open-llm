# Round Manifest - E53 Round D Trained Compiler

Date: 2026-05-04

Round: 16

Experiment: `E53-HIGH-LEVEL-ORCHESTRATION-PROOF`

## Goal

Replace one fixture only: node-00 logical structure compiler.

Round C's trained node-01 splitter stays active. Nodes 02 through 09 stay deterministic. The purpose of this round is to test whether a trained Flow-01 node-00 compiler can preserve the exact signals required by the trained node-01 splitter.

## Source Authority

- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\MASTER-EXPERIMENT-PLAN-v1.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\E53-ROUND-D-PLAN-AND-BATCH-PROTOCOL-v1.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\flow-01-training-sets-v1\node-00-logical-structure-compiler-set.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\flow-01-training-sets-v1\node-01-domain-splitter-set.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\E53-SCENARIO-001-SOURCE-PACKET.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\artifact_reviewing.md`

## Files Changed

- `C:\Projects\llm_mvp_core\35.ExperimentsV2\Experiments\E53HighLevelOrchestrationProof.cs`

## Files Produced

- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\E53-ROUND-D\ROUND-MANIFEST-2026-05-04-E53-ROUND-D-TRAINED-COMPILER.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\E53-ROUND-D\E53-ROUND-D-VALIDATION-RESULTS.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\E53-ROUND-D\BATCH-ROUND-SUMMARY.md`
- `C:\Projects\llm_mvp_core\experiments-v2\E53-HIGH-LEVEL-ORCHESTRATION-PROOF\results\run-2026-05-04-144327.json`
- `C:\Projects\llm_mvp_core\experiments-v2\E53-HIGH-LEVEL-ORCHESTRATION-PROOF\results\run-2026-05-04-144327.md`
- `C:\Projects\llm_mvp_core\experiments-v2\E53-HIGH-LEVEL-ORCHESTRATION-PROOF\results\run-2026-05-04-144327.mmd`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\round-zips\round-16-e53-round-d-trained-compiler-2026-05-04.zip`

## What Changed

`BuildCompiledLogicalStructure()` no longer returns a direct deterministic structure.

The E53 proof now builds the raw E53 human problem and passes it into `Flow01Node00TrainedLogicalCompiler.Compile(...)`.

The trained compiler applies learned signal rules derived from the accepted Flow-01 node-00 pairs. It detects local subject, units, dependencies, conflicts, boundaries, knowledge edges, required outputs, and the no-stack constraint from the raw text.

The evidence JSON now includes `compiled_logical_structure`, so the Round D grep gate can verify `local_subject` and the signal table can compare node-00 output across rounds.

## What Did Not Change

- Node-01 remains the trained splitter from Round C.
- Node-02 remains deterministic.
- Node-03 remains deterministic.
- Nodes 04, 05, 06, 07, 08, and 09 remain deterministic.
- The expected trace oracle did not change.
- The E53 test signatures and assertions did not change.

## Verification

Pre-condition before edits:

- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "E53" --no-build`: 12 passed, 0 failed.
- `dotnet test Tests/LlmFromScratch.Tests.csproj --no-build`: 997 passed, 0 failed.

After replacement:

- `dotnet build LlmFromScratch.sln`: 0 errors.
- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "E53" --no-build`: 12 passed, 0 failed.
- `dotnet test Tests/LlmFromScratch.Tests.csproj --no-build`: 997 passed, 0 failed.

Latest evidence:

- `compiled_logical_structure.local_subject`: `complex_registration_and_onboarding_orchestration`.
- domain packet count: 11.
- `node02_input_count`: 11.
- `node02_output_count`: 11.
- `e2e_orchestration_success`: true.
- `lost_edge_count`: 0.
- `dependency_violation_count`: 0.
- `live_external_calls`: 0.
- `learning_gap_packet_count`: 4.

## Round Contract

Baseline: 997/997 before Round D.

Current: 997/997 after Round D.

Produced: trained node-00 compiler adapter, compiled logical structure evidence, one new E53 evidence triplet, manifest, validation results, batch round summary, round zip.

Phase gate: all existing 12 E53 tests pass unchanged; full suite remains green; latest evidence preserves 11 domain packets and exposes `local_subject`.

Gap: no blocking gap for this round.

## Next Action Item

Continue Batch 1 with Round E: replace node-03 scheme planner with a trained planner while keeping nodes 00 and 01 trained, node-02 deterministic, and all downstream stations deterministic.
