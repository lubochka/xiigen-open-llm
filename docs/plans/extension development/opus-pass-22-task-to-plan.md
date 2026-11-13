# opus-pass-22 — Pipeline: task-to-plan orchestration (реальное состояние)

Проход P22 из 50. Фокус: РЕАЛЬНАЯ вторая стадия конвейера расширений — «task → plan
(orchestration reasoning)». Два слоя, названные заданием: (1) `43.SmartSkillsDomain`
машина `OrchestrationReasoningMachine` + её «обучаемая» единица
`OrchestrationReasoningScenarioUnit` + `OrchestrationReasoningTrainer` (это стадия 2
конвейера трёх расширений); (2) слой `15.OrchestratorModel` — `ILogicalOrchestrator`/
`ILogicalFlow`, `LogicalOrchestratorAssistant`, `ProjectWikiOrchestrationFlow` (TD2).
Плюс runtime scenario gate — как оркестрация реально маршрутизируется в проде.

Метод: bounded-чтения живого кода `C:\Projects\llm_mvp_core` целиком по слайсу
(`OrchestrationReasoningScenarioUnit.cs` 720, `OrchestrationReasoningTrainer.cs` 267,
`OrchestrationReasoningMachine.cs` ключевые методы, `ProjectWikiOrchestrationFlow.cs`
183, `LogicalOrchestratorAssistant.cs` 435, стадия-2 сервиса
`XiigenExtensionApiService.cs:106-160,:522-545`), плюс промоутнутые ассеты
`models/orchestration-reasoning/orchestration-reasoning-v1/*` и
`models/orchestration/orchestration-v1/*`, плюс вычислительная сверка словарей
стадии 1 и стадии 2.

Дедуп: НЕ повторяю P02 gap-3 (task→orchestration inference «always not-ready» как
машинная константа), P02 gap-5 (три машины не собраны в 15), P10 gap-6 (smart-skills
конвейер — плоская цепочка мимо 15), P14 gap-5 (стадия-3 DecideSkills игнорирует
task+plan), P21 (глубокий разбор prompt-to-task, стадия 1). Здесь — стадия 2 и слой 15
с новыми конкретными код-дырами.

---

## Что реально есть (честная картина, чтобы не занижать)

- `OrchestrationReasoningMachine.Plan` РЕАЛЬНО достижима в проде: сервис на стадии 2
  зовёт `activeLoadState.OrchestrationReasoningMachine.Plan(orchestrationRequest)`
  (`XiigenExtensionApiService.cs:131`), а `LoadCommonAssets` (`:1422-1425`) грузит
  промоутнутый чекпойнт `models/orchestration-reasoning/orchestration-reasoning-v1/`
  через `LoadCheckpointAsync`. Значит «always not-ready» (P02 gap-3) — это состояние
  БЕЗ чекпойнта; с чекпойнтом машина реально что-то планирует. Это коррекция P02.
- Есть промоутнутый ассет: manifest 15 learned patterns, 8 DPO-пар, 4 held-out,
  `semanticQualityClaimed:false`, `commonOrAdaptive:"common"`, source-disjoint split
  refs с SHA. Тренер, экспорт/импорт, continue-training, held-out-eval — как код
  существуют.
- `15.OrchestratorModel` реально содержит `AdaptiveLogicalOrchestrator`, реестр flow,
  `ProjectWikiOrchestrationFlow` (обёртка 6 bootstrap-юнитов TD2), зарегистрированный
  в `LogicalOrchestratorAssistant.CreateDefault`.

Но ниже — почему это НЕ «работающая обучаемая task→plan оркестрация», а
цепочка lookup/replay + мёртвый маршрут.

---

## НОВЫЕ дыры P22

### G1 (главная) — Разрыв словарей стадия1→стадия2: «ready/plan» путь оркестрации недостижим end-to-end

`OrchestrationReasoningScenarioUnit.Decide` выдаёт план ТОЛЬКО при `ExactSignalMatchKind`,
т.е. `requiredSatisfied = requiredSignals.All(requestSignals.Contains)` — в запросе
должны присутствовать ВСЕ обязательные сигналы паттерна (`ScenarioUnit.cs:553-554,:569`).
Transferable/структурный overlap Decide переводит в вечный NotReady (`:340-347`).

Запрос оркестрации строится из выхода стадии 1 напрямую:
`BuildOrchestrationRequest(request, promptToTask.StructuredTask)` →
`Task: structuredTask` (`XiigenExtensionApiService.cs:130,:527`). Сигналы берутся из
`Task.TaskType/Constraints/ExplicitUserRequirements/Deliverables/EvidenceNeeds`
(`ScenarioUnit.BuildSignalSet:604-627`).

Вычислительная сверка словарей промоутнутых чекпойнтов:
- Гистограмма `requiredTaskTypes` по 15 orchestration-паттернам:
  **`universal_reasoning`×12**, `plan_only_or_approval_gated_task`×1,
  `structured_audit_task`×1, `deliverable_task`×1.
- Тренер жёстко зашивает `RequiredTaskTypes: ["universal_reasoning"]` каждому
  свежему паттерну (`OrchestrationReasoningTrainer.cs:171`).
- Prompt-to-task НИКОГДА не эмитит taskType `universal_reasoning`: его 8 типов =
  `{audit_task, bounded_review_or_consultation_task, deliverable_task,
  plan_only_or_approval_gated_task, planning_task, scope_task,
  source_resolution_required_task, structured_audit_task}`.
- Обязательные content-сигналы 12 «настоящих» reasoning-паттернов
  (`evidence_linkage_reasoning`, `link_source_quality_risk_and_consequence`,
  `multi_axis_decomposition_reasoning`, `authority_boundary_reasoning`,
  `deterministic_shortcut_firewall` и т.д.) ОТСУТСТВУЮТ во всём словаре, который
  умеет эмитить prompt-to-task.

Тест на достижимость (любой одиночный prompt-to-task паттерн + фикс. RuntimePolicyRefs
⊇ полный required-set оркестрации): **12/15 паттернов недостижимы**; достижимы лишь
3/15, построенные на общих framing-токенах. Значит семантическое ядро модели
task→plan (12 reasoning-паттернов) — dead-on-arrival на реальном промпте: prompt-to-task
эмитит task-framing вокабуляр, оркестрация требует reasoning-axis вокабуляр; их
пересечение — только generic-токены, никогда не полный required-set reasoning-паттерна.

Это ОТДЕЛЬНО от P14 gap-5 (стадия-3 DecideSkills игнорирует task+plan) и P02 gap-3
(машинная not-ready-константа): здесь конвейер собран и достижим, но две стадии
обучены на разных датасетах с непересекающимися сигнальными словарями, поэтому ready-путь
второй стадии структурно не срабатывает.

### G2 — `OrchestrationReasoningTrainer` выбрасывает rejected/near-miss сторону DPO; «тренер» = решейп авторского JSON, confidence 0.8 константа

`OrchestrationReasoningTrainer.MaterializeAsync`→`BuildPattern` копирует авторские поля
payload прямо в паттерн: `RequiredConstraintSignals: intentSignals`,
`RequiredRequirementSignals: reasoningSteps`,
`RequiredDeliverableSignals: [universalPattern, ..decisionGuards]`,
`RequiredEvidenceSignals: evidenceExpectations` (`Trainer.cs:159-175`). Rejected-сторона
`near_miss_signals` (`:164 rejectedSignals`) валидируется на непустоту и затем НИКОГДА
не используется в паттерне — DPO chosen-vs-rejected margin отсутствует. `Confidence: 0.8`
зашита константой каждому паттерну (`:182`), `RequiredTaskTypes:["universal_reasoning"]`
зашит (`:171`). Отчёт сам ставит `TrainingStarted:false, ModelAssetCreated:false,
CheckpointCreated:false` (`:92-94`). Т.е. нет градиента, нет reward-margin, нет
counter-update — это авторская JSONL→pattern-материализация, где rejected-сигнал
отброшен. Нарушение CLAUDE.md «reward-scalarization rule» и «acceptance-signal
independence» именно для стадии task→plan. Отдельно от P05 (трансформерный DPO
отсоединён) и P21 gap-4 (prompt-to-task continue-append): здесь ОСНОВНОЙ тренер
оркестрации отбрасывает rejected-сторону и не считает маржу.

### G3 — Runtime scenario gate: `ProjectWikiOrchestrationFlow` зарегистрирован в слое 15, но никогда не маршрутизируется адаптивно; достижим только из тестов

`LogicalOrchestratorAssistant.CreateDefault` регистрирует `ProjectWikiOrchestrationFlow`
(`:132-138`). Но `RunAsync` при неявном сценарии стартует с
`AdaptiveLogicalOrchestrator.ScenarioId`, а pre-route
(`ProjectWikiRequestRouterUnit().Route`) может переключить сценарий ТОЛЬКО на
`ProjectWikiTrainingFlowAdapter.ScenarioId` или
`ProjectDocumentationPipelineFlowAdapter.ScenarioId` (`:233-243`) —
`ProjectWikiOrchestrationFlow.ScenarioId` НЕ выбирается адаптивным роутером НИКОГДА.
Достижим лишь при явной передаче точного scenarioId; единственные вызыватели
`LogicalOrchestratorAssistant` = он сам, `TeachingSessionBuilder.cs` и `Tests/*`
(тесты `ProjectWikiOrchestrationFlowTests` гоняют флоу напрямую, не через роутинг).
Плюс `XiigenExtensionApiService` (граница трёх расширений) имеет **0 ссылок** на
`OrchestratorModel`/`LogicalOrchestratorAssistant`/`AdaptiveLogicalOrchestrator` — то
есть ни одно из трёх расширений не доходит до слоя 15 вообще. Отдельно от P02 gap-5
(три smart-skills машины не собраны в 15) и P10 gap-6 (smart-skills — плоская цепочка):
здесь именно 15-слойный wiki-orchestration flow зарегистрирован, но мёртв на маршруте.

### G4 — Выход «плана» = replay семейств сохранённого паттерна ЛИБО константный task-независимый baseline-план; обобщения нет

При ready `Machine.Plan` строит `OrchestrationScenarioPlan` буквально из полей
выбранного паттерна: `PhaseFamilies/GateFamilies/AssetFamilies/HostCheckSets/RiskFlags/
NextGranularizationActions = chosenPattern.*` (`Machine.cs:176-185`) — план ЕСТЬ
извлечённый авторский паттерн (exact-replay). Не-exact overlap вечно NotReady
(`ScenarioUnit.cs:340-347`) → обобщения нет. Когда prompt-to-task ушёл в baseline-путь
(`Task.SourceProvenance["semantic_translation"]=="baseline_internal_runtime_fallback"`,
`Machine.cs:560-563`), оркестрация возвращает ПОЛНОСТЬЮ ЗАШИТЫЙ константный план:
`PhaseFamilies:["runtime-quality-gates"]`,
`GateFamilies:[source-disjoint-eval, ablation-control, leakage-control, fresh-load,
continue-training]` и т.д. (`Machine.cs:489-515`) — одинаковый для ЛЮБОЙ задачи,
независимый от содержания. Итог реального прода: либо exact-replay сохранённого паттерна
(на настоящем промпте ≈ никогда, см. G1), либо константный baseline-план, либо
NoMatchingPattern. Ни один из вариантов — не обучаемый семантический task→plan
(R4 hidden determinism для стадии 2).

### G5 — Нет персистентного runner-report/held-out-артефакта для orchestration-reasoning-v1

`find models/orchestration-reasoning -name runner-report.json` → пусто, тогда как
`models/prompt-to-task/...` и `models/skill-selection/...` имеют `runner-report.json`.
Manifest заявляет `heldOutRowCount:4`, `dpoPairCount:8`, но НИ ОДНОГО отчёта о
прогоне `EvaluateHeldOutRows` (порог `MinimumEvaluationAccuracy=0.8`,
`ScenarioUnit.cs:108,:451`) для orchestration-reasoning не персистировано. Промоушен
`orchestration-reasoning-v1` не подкреплён сохранённым held-out-числом (в отличие от
prompt-to-task, у которого хоть циркулярный, но исполненный раннер — P21 gap-3). Held-out
gate стадии task→plan не доказан артефактом.

### G6 — Continue-training = self-конкатенация списков паттернов; промоутнутый чекпойнт с удвоенным stateId

`ContinueTrainingFromLearnedState` = `existingPatterns.Concat(deltaState.LearnedPatterns)`
+ новый SHA + `StateId = "{existing}:continued:{delta}"` (`Machine.cs:287-297`), без
градиента/маржи/обновления confidence; ставит `ContinueTrainingRunClaimed:false,
SemanticQualityClaimed:false`. Промоутнутый `checkpoint.json` показывает результат:
`stateId="orchestration-reasoning-state:common:or-04b-trainer-implementation-01:continued:
orchestration-reasoning-state:common:or-04b-trainer-implementation-01"` —
self-continued state (дельта с тем же базовым stateId). Это тот же класс, что P21 gap-4
(prompt-to-task continue-append), но для другой машины (OrchestrationReasoning) и с
конкретным shipped-артефактом-доказательством: «continue-training» — метаданный append
паттернов, не learning-update.

---

## Таблица NEW gaps P22

| Component | Real-status | Evidence file:line | Gap | Closure |
|---|---|---|---|---|
| Task→plan pipeline stage1→stage2 vocabulary | PIPELINE INTEGRATION BREAK / ready-path unreachable | `OrchestrationReasoningTrainer.cs:171`; checkpoint histogram universal_reasoning×12/15; PromptToTask 8 taskTypes (нет universal_reasoning); `XiigenExtensionApiService.cs:130,:527`; `ScenarioUnit.cs:340-347,:553-569` | 12/15 orchestration-паттернов требуют taskType `universal_reasoning` + reasoning-axis сигналы, которые prompt-to-task никогда не эмитит → exact-signal ready-путь стадии 2 структурно недостижим end-to-end; семантическое ядро модели task→plan мертво на проде | Согласовать сигнальный контракт стадий 1↔2 (общий словарь/эмиссия reasoning-сигналов из стадии 1) ИЛИ обучаемый semantic-mapping вместо exact-set membership; e2e-прогон настоящего промпта до ready-плана с числовой достижимостью |
| OrchestrationReasoningTrainer | STUB TRAINER / no reward scalarization | `OrchestrationReasoningTrainer.cs:159-175` (копия авторских полей), `:164` rejectedSignals прочитан-не-использован, `:182` Confidence 0.8 const, `:171` taskType зашит, `:92-94` nothing created | «Тренер» = решейп авторского JSONL в паттерн; rejected/near-miss DPO-сторона валидируется и отбрасывается (нет margin), confidence — константа; нарушение reward-scalarization/acceptance-independence для task→plan | Реализовать реальный DPO-update: reward(chosen)−reward(rejected)→мутация типизированного состояния (сигналы/уверенность паттернов); использовать rejected-сторону; убрать confidence-константу; held-out числовой прирост |
| 15.OrchestratorModel `ProjectWikiOrchestrationFlow` runtime gate | DEAD ROUTE / registered-not-routed | `LogicalOrchestratorAssistant.cs:132-138` register, `:233-243` adaptive router только Training/DocumentationBuild; callers = self+`TeachingSessionBuilder`+Tests; `XiigenExtensionApiService` 0 ссылок на OrchestratorModel | 15-слойный wiki-orchestration flow зарегистрирован, но адаптивным роутером не выбирается никогда; достижим лишь явным scenarioId из тестов; три расширения слой 15 не достигают | Либо подключить flow к реальному роутингу (правило выбора его сценария) + host-путь, либо честно пометить 15-слой как non-shipped/эксперимент; доказать e2e-маршрут одного промпта в этот flow из реального вызывателя |
| OrchestrationReasoningMachine plan output | REPLAY/CONSTANT / no generalization (R4) | `Machine.cs:176-185` план=поля chosenPattern; `ScenarioUnit.cs:340-347` вечный NotReady для non-exact; `Machine.cs:489-515` зашитый baseline-план; `:560-563` триггер | Выход «плана» = либо exact-replay сохранённого авторского паттерна (на проде ≈ никогда, G1), либо константный task-независимый baseline-план; обобщения нет; hidden determinism стадии task→plan | Обучаемый semantic-planner (генерализующий task→plan), а не exact-lookup; убрать константный fallback из success-класса или пометить eval-only; ablation, что план зависит от содержания задачи |
| orchestration-reasoning-v1 promotion evidence | MISSING EVIDENCE / no runner-report | `find models/orchestration-reasoning -name runner-report.json`→0 (у prompt-to-task/skill-selection есть); manifest `heldOutRowCount:4` без отчёта; `ScenarioUnit.cs:108,:451` gate 0.8 не персистирован | Промоушен orchestration-reasoning-v1 не подкреплён сохранённым held-out-числом; held-out gate стадии task→plan не доказан артефактом | Прогнать `EvaluateHeldOutRows` на source-disjoint наборе, персистить runner-report с per-row accuracy; гейтить промоушен на ≥0.8 |
| OrchestrationReasoning continue-training | STUB LIFECYCLE / append not learning | `Machine.cs:287-297` Concat+`StateId="{existing}:continued:{delta}"`, `ContinueTrainingRunClaimed:false`; shipped `checkpoint.json` stateId удвоен `...:continued:...same...` | «Continue-training» = метаданный append списков паттернов без learning-update; shipped-чекпойнт — self-continued state | Реальный continue-training-update от чекпойнта (accepted/rejected→мутация состояния) с числовым before/after на held-out; отделить cold-start authoring от continue |

---

## Вывод P22

Стадия task→plan существует как достижимый код (коррекция P02 gap-3), но это
lookup/replay-механизм, а не обучаемая семантическая оркестрация: (G1) семантическое
ядро из 12 reasoning-паттернов недостижимо из-за разрыва сигнальных словарей стадий 1↔2;
(G2) «тренер» отбрасывает rejected-сторону DPO и зашивает confidence/taskType; (G4)
реальный выход — replay сохранённого паттерна либо константный baseline-план; (G3)
параллельный 15-слой (`ProjectWikiOrchestrationFlow`) зарегистрирован, но мёртв на
маршруте и не достигается расширениями; (G5) промоушен без held-out-артефакта; (G6)
continue-training — append. Итог: A-to-B пары / fan-out / arbiter из def-of-done v3
#4 на реальном runtime не моделируются; «task→plan» = детерминированный exact-set replay
с константным fallback.
