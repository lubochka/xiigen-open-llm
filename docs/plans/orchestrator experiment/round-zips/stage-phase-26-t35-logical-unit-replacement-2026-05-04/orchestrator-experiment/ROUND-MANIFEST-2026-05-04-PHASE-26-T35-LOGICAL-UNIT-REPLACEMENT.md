# Round Manifest - Phase 26 T35 Logical Unit Replacement

Date: 2026-05-04

Round purpose: replace one logical representation with one actual `ILogicalUnit` by swapping T34 node-01 into `T34TransitionDomainSplitterUnit` inside a new T35 proof.

## Baseline

- Code branch before edits: `main`
- Baseline before Phase 26 edits: 1020/1020 passing
- Focused T34/T35 tests after edits: 16/16 passing
- Full suite after edits: 1028/1028 passing

## Produced Code Artifacts

- `35.ExperimentsV2/LogicalFlow/T34TransitionDomainSplitterUnit.cs`
  - Implements `ILogicalUnit<PhaseContextPacket, DomainPacket[]>`.
  - Consumes the T34 node-00 phase context packet.
  - Emits the exact ten T34 node-01 transition packets.
  - Reports confidence `0.92`, above the `0.70` T35 floor.

- `35.ExperimentsV2/Experiments/T35LogicalUnitReplacementProof.cs`
  - Keeps T34 as the oracle.
  - Runs the actual node-01 logical unit.
  - Compares packet ids, domain labels, coverage statuses, source ids, and confidence.
  - Writes JSON, Markdown, and Mermaid evidence.

- `Tests/DynamicFlowT35Tests.cs`
  - Adds eight tests covering evidence writing, metric keys, replacement gate success, exact packet parity, field preservation, confidence floor, default flags, and human finding quality.

## Produced Evidence

- `experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-05-04-172450.json`
- `experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-05-04-172450.md`
- `experiments-v2/T35-LOGICAL-UNIT-REPLACEMENT-PROOF/results/run-2026-05-04-172450.mmd`

## T35 Findings

- Logical unit replacement success: `true`
- Oracle packet count: 10
- Unit packet count: 10
- Packet id mismatch count: 0
- Domain label mismatch count: 0
- Coverage status mismatch count: 0
- Source id mismatch count: 0
- Unit confidence: 0.92
- T34 transition success: `true`
- T35 transition success: `true`

The node-01 logical unit matched the T34 oracle exactly. No downstream transition metric drifted because the node-01 output parity is exact. The first safe unit replacement path is now proven.

## Scope Check

- T34 proof was not modified.
- E53 proof was not modified.
- Projects 21, 22, and 23 were not modified.
- No logical flow or orchestrator was introduced in this phase.
- Only one unit presence was added: the node-01 transition domain splitter.

## Verification

- Focused command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "T34|T35"` -> 16/16 passing.
- Full command: `dotnet test Tests\LlmFromScratch.Tests.csproj` -> 1028/1028 passing.

## Round Zip

- `round-zips/round-phase-26-t35-logical-unit-replacement-2026-05-04.zip`

## Next Action

Run Phase 27: introduce `ILogicalFlow` and a `T34LogicalFlowAdapter` that wraps the unchanged T34 proof. Do not add an orchestrator or registry in Phase 27.
