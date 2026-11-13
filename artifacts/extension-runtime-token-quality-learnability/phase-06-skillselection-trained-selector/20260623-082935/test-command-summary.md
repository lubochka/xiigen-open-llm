# Phase 06 SkillSelection test commands

- Focused regression: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SkillSelectionLogicalHierarchyTests|FullyQualifiedName~SkillSelectionMachineTests" --logger "console;verbosity=minimal"`
- Phase evidence: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SkillSelectionPhase06TrainedSelectorEvidenceTests" --logger "console;verbosity=minimal"`
- Combined selector regression: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SkillSelectionLogicalHierarchyTests|FullyQualifiedName~SkillSelectionMachineTests|FullyQualifiedName~SkillSelectionPhase06TrainedSelectorEvidenceTests" --logger "console;verbosity=minimal"` -> 55 passed, 0 failed.
- Evidence hygiene: JSON/JSONL parse, UTF-8 no BOM, no CR, no trailing whitespace -> 22 files passed.
- Whitespace gate: `git diff --check`

Expected result: 0 failed.
