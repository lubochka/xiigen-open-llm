# 05 — ILearningUnit / Flow / Orchestrator / DynamicGraph и скрытые подмены
**Не считать наличие интерфейса доказательством обучения.**

## DIF-501 — Необходим many→many эффект, а не единственный nearest output (CONTRACT_DIFF)
[Original task](../../original%20task.md), [Recovered design A02](../RECOVERED-DESIGN-CONTRACT.md): ILearningUnit получает домен и описание входных/выходных векторов, возвращает IQueryable/типизированное множество many→many результатов. KnnLogicalUnit фактически возвращает один `LogicalUnitResult<TOutput>` из best hit. Это расхождение для конкретного bridge; необходимо понять, какие другие units реализуют настоящий many→many, не объявлять все Units broken.

## DIF-502 — Learned Flow correction не заменяется проводкой зависимостей (CONTRACT + OPEN)
ILearningFlow управляет отношением юнитов и обучаемой коррекцией вход→выход. Трассировать существующие `ILogicalFlow`/learning interfaces, owner параметров ребра, update signal, correction feedback, saved state, actual inference fan-out/fan-in. Один вызов static pipeline или статическое исправление не доказывают flow learning. Сравнить fixed units + old vs learned edge under equal budget.

## DIF-503 — Async orchestrator and higher-order graph may be architecture-only (CONTRACT + OPEN)
ILearningOrchestrator должен запускать альтернативные пути асинхронно, управлять quota/cancellation, возвращать typed result; ILearningDynamicGraph — обучаемые отношения pattern orders. Проверить существующие `00.Core/DynamicGraphContracts.cs`, `00.Core/LearningMachineContracts.cs`, actual implementations and consumer route, L2/L5 ownership, save/load. Graph edge count не равен learned composition; нужно доказать changed learned state→better unseen patterns, not simply more rules.

## DIF-504 — Обманчивые суррогаты улучшения (CODE / route OPEN)
[Experiment reconciliation](01-EXPERIMENT-BASELINE-RECONCILIATION.md): исторический P3 creates deterministic discriminator, T35 synthetic latency 0.1 ms; не neural learning. [Existing possibleDifs README](README.md) отмечает legacy TranslationMachine `TrainBatchAsync` прибавляет accuracy=0.01 без unit update. Найти действительный callsite и оговорить научное подтверждение; даже если этот path не вызывается в RP01, это риск ложного product telemetry. Детерминистический закон нужен; дефект — подмена им обученного механизма, искажение метрик.

## DIF-505 — DPO triple → typed Unit/Flow не реализован простым текстовым carrier (CODE + CONTRACT_DIFF)
`SimulationDpoInputRow(Id,Prompt,Chosen,Rejected)` — только текст. Простой текстовый DPO можно считать корректным отдельным экспериментом, но он не обучает автоматически mapping one DPO→law tokens→semantic vector→unit transitions→graph path. Требуется явный compiler/map layer: source truth, domain, structured chosen/rejected differences, typed input/output refs, ownership, learned target, confidence, hierarchical feedback, version. Не смешивать одни и те же 500 строк со всеми компонентами без typed mapping.

## Следы
Для каждого Unit/Flow/Orchestrator/Graph: actual constructor, train API, stored state key, changed value, checkpoint SHA, downstream consumer, paired before/after path, cost and critic. Good: был исправлен конкретный закон на новых inputs; Bad: повысился счётчик/сохранилось имя объекта без нового решения. Работы вне RP01 принадлежат RP03/RP04; их независимая трассировка обязана продолжаться параллельно.