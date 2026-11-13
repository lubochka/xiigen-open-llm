# QUALITY-PRODUCT-ORCHESTRATION-EXPANDED-V1-SCENARIO-GENERALIZATION-REPAIR-03

Решение: FIXED_COMMIT_READY

## Счетчики
- Всего задач фазы: 12
- Готово: 12
- В работе: 0
- Осталось в этой фазе: 0
- Провалено: 0
- Следующее: Parent review/acceptance boundary for repair 03; phase 04 was not started.

## Что исправлено
- Добавлен прямой guard-test: один transferable `decision-token:` может считаться eval evidence, но `Decide` возвращает `IsReady=false`.
- Для выхода в ready требуется минимум два `decision-token:` anchor; один anchor остается только диагностическим evidence.
- Из transferable baseline убраны label-like токены `decision-token:chosen` и `decision-token:plan`.
- Quality-reason строки очищаются от префиксов `Chosen plan` / `Rejected plan` перед token baseline.
- Добавлена boilerplate-only leakage ablation row: source labels, chosen/rejected labels и chosen/plan boilerplate дают 0 passing controls.

## Результат eval
- Held-out строк: 4
- Структурно пройдено: 4
- Structural accuracy: 1
- Точных replay-сценариев: 0
- Source-only controls passed: 0
- Boilerplate-only controls passed: 0

## Ограничения
- SemanticQualityClaimed=false.
- HostRuntimeClaimed=false.
- Это не доказательство semantic quality, token savings или full extension readiness.
- Target promotion не заявлялся.
- Phase 04 не стартовала.

## Тесты
- dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~OrchestrationReasoningExpandedV1ScenarioGeneralizationTests --logger console;verbosity=minimal: PASS (exit 0)
- dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~OrchestrationReasoningContractsTests --logger console;verbosity=minimal: PASS (exit 0)
- dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~OrchestrationReasoningP9BEvalAblationRerunTests --logger console;verbosity=minimal: PASS (exit 0)

## Следующая граница
- Parent review/acceptance and optional parent-owned commit after acceptance only.
