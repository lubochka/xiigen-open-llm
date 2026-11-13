# Round 06 Manifest - Autonomous Learning Rollback Gate

Date: 2026-05-04

Code repository: `C:\Projects\llm_mvp_core`

Artifact zip:
`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\round-zips\round-06-rollback-gate-2026-05-04.zip`

## Purpose

Close the remaining projects 21-23 transfer gap: prove that an autonomous learning candidate adapter is not promoted when its measured after-score does not improve over the baseline score.

## Changed Files

1. `C:\Projects\llm_mvp_core\16.AutonomousLearning\Pipeline\AutonomousLearningPipeline.cs`
   - Changed the promotion order.
   - The pipeline now trains a candidate, measures enhanced accuracy, and registers the adapter only when `enhanced > baseline`.
   - If the candidate does not improve, the cycle fails with `ACCURACY_NOT_IMPROVED` and the registry remains unchanged.

2. `C:\Projects\llm_mvp_core\Tests\AutonomousLearningTests.cs`
   - Added `AutonomousLearning_Pipeline_AccuracyRegression_DoesNotRegisterAdapter`.
   - The test runs a full idle learning cycle with baseline `0.80` and enhanced `0.70`.
   - It asserts the cycle fails with `ACCURACY_NOT_IMPROVED`.
   - It asserts `DomainModelRegistry.GetAdapter("CODE_Q")` remains `null`.

## Included Reference File

`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md`

This is included so a future session can repeat the same round packaging pattern.

## Verification

Focused pipeline tests:

```powershell
dotnet test --filter "AutonomousLearning_Pipeline"
```

Result: `4/4 passed`

All autonomous learning tests:

```powershell
dotnet test --filter "AutonomousLearning_"
```

Result: `23/23 passed`

Full suite:

```powershell
dotnet test
```

Result: `982/982 passed`

## Design Meaning

The idle learning loop now has a hard before/after promotion gate. Training may produce a candidate adapter, but the adapter does not become active unless evidence says it improves the domain score. This protects the closed-ecosystem assistant from installing weaker local learning during idle-time updates.

## Notes

The file `AutonomousLearningPipeline.cs` also contains the earlier namespace qualification for `ContinuousLearning.IDomainModelRegistry`; that was already present in the working tree from the prior gap-closure round and remains necessary for the project 16 compile ambiguity fix.
