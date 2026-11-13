# Phase 06 SkillSelection test commands

- Focused regression: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SkillSelectionLogicalHierarchyTests|FullyQualifiedName~SkillSelectionMachineTests" --logger "console;verbosity=minimal"`
- Phase evidence: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SkillSelectionPhase06TrainedSelectorEvidenceTests" --logger "console;verbosity=minimal"`
- Whitespace gate: `git diff --check`

Expected result: 0 failed.
