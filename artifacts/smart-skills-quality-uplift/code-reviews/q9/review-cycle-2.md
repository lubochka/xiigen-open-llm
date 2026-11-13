# Q9 review cycle 2 - semantic quality firewall

Вердикт: PASS

## Что проверено

Проверен firewall semantic-quality claim на уровне API/checkset и новых baseline units. Главный риск цикла: API или checkset не должны заявлять semantic quality из baseline/no-claim evidence или только из status strings.

Проверенные пути:

- `43.SmartSkillsDomain/XiigenExtensionApiService.cs`
- `43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs`
- `43.SmartSkillsDomain/OrchestrationReasoningScenarioUnit.cs`
- `Tests/XiigenExtensionApiContractsTests.cs`
- state/docs Q9 claim-scope записи

## Findings

Открытых semantic-quality firewall findings нет.

API-level claim gate больше не считает `PromptToTaskMachine.LearnedPatternStatus` и `OrchestrationReasoningMachine.LearnedPatternStatus` достаточными. Для будущего claim требуется явная положительная evidence `SEMANTIC_QUALITY_CLAIMED_TRUE` от всех трех компонентов: PromptToTask, OrchestrationReasoning и SkillSelection.

Baseline/no-claim evidence блокирует claim. В no-claim список входят, среди прочего, `BASELINE_NOT_SEMANTIC_QUALITY`, `SEMANTIC_QUALITY_CLAIMED_FALSE`, not-trained/not-ready evidence и no-matching evidence.

Новые PromptToTask/OrchestrationReasoning units возвращают `SemanticQualityClaimed=false`; OrchestrationReasoning дополнительно держит `HostRuntimeClaimed=false`. Их held-out evaluation reports и nearest-overlap surfaces не превращаются в semantic-quality proof.

Warnings/chat/checkset wording остается честным: core boundary может быть executed, но semantic quality не claimed.

## Required Fix

Не требуется.

## Evidence

- Известная parent evidence: API semantic quality gate requires future positive `SEMANTIC_QUALITY_CLAIMED_TRUE` from PTT/OR/SkillSelection and blocks baseline/no-claim evidence.
- Известная parent evidence: focused test gate PASS 62/62, включая `XiigenExtensionApi`.
- Известная parent evidence: extended lifecycle/API gate PASS 59/59 after repair.
- Read context: `XiigenExtensionApiService.ShouldClaimSemanticQuality` проверяет positive evidence и отсутствие no-claim evidence; tests покрывают baseline PromptToTask/Orchestration units with semantic claim false.
- В этом review cycle тесты не запускались.
