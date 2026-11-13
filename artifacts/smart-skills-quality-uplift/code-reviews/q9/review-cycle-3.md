# Q9 review cycle 3 - asset lifecycle and hash

Вердикт: PASS

## Что проверено

Проверена готовность asset lifecycle/hash части Q9 для PromptToTask после repair. Главный риск цикла: checkpoint/manifest/index hashes не должны расходиться, committed asset test не должен быть привязан к stale pattern count, и hash repair не должен менять semantic-quality claim.

Проверенные пути:

- `models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json`
- `models/prompt-to-task/prompt-to-task-v1/manifest.json`
- `models/index.json`
- `Tests/PromptToTaskMachineAssetLifecycleTests.cs`
- `43.SmartSkillsDomain/PromptToTaskMachine.cs`

## Findings

Открытых asset lifecycle/hash findings нет.

Предыдущий extended lifecycle gate правильно нашел два actionable gaps: committed PromptToTask checkpoint содержал 12 patterns, а stale test ожидал старое количество; также тест был привязан к старому misleading `learnedState.manifestSha256` expectation. Эти gaps были исправлены.

Текущая ремонтная логика честная: committed asset test теперь берет реальный committed pattern из checkpoint и сверяет `learnedPatterns.Count` с `Manifest.LearnedPatternCount`, а не hardcoded stale count.

Hash repair не поднимает semantic-quality claim. Parent evidence прямо фиксирует, что `learnedState.stateSha256`, manifest `checkpointSha256` и `models/index.json` синхронизированы, при этом `semanticQualityClaimed=false` сохранен.

## Required Fix

Не требуется.

## Evidence

- Известная parent evidence: extended lifecycle/API gate initially failed on PromptToTask asset lifecycle, was repaired, then PASS 59/59.
- Известная parent evidence: PromptToTask asset hash repair synced `learnedState.stateSha256`, manifest `checkpointSha256`, and `models/index.json`.
- Известная parent evidence: `git diff --check` PASS.
- Read context: `PromptToTaskMachineAssetLifecycle_CommittedRepoAssetFreshLoadsAndImports` and `CommittedRepoAssetUsesHonestHashes` assert pattern count agreement, false semantic-quality claim, physical checkpoint hash agreement, and canonical learned-state hash.
- В этом review cycle тесты не запускались.
