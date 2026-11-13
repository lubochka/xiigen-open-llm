# Pass P21 — Pipeline: prompt-to-task (реальное состояние + новые дыры)

Дата: 2026-07-08. Оценщик: Opus 4.8 (REAL-STATE ASSESSOR). Срез: стадия 1 конвейера
расширений — **prompt → structured task** (`43.SmartSkillsDomain/PromptToTaskMachine.cs`,
`PromptToTaskTaskPatternUnit.cs`, `PromptToTaskAblationHarness.cs`,
`tools/PromptToTaskPhase04Runner`, промоутнутый чекпойнт
`models/prompt-to-task/prompt-to-task-v1/*`, тесты `Tests/PromptToTask*`,
проводка в `XiigenExtensionApiService.cs`). Фокус задачи: held-out / fresh-load /
semantic gates; Trainable vs Bootstrap.

## Что реально прочитано (bounded)

- `43.SmartSkillsDomain/PromptToTaskMachine.cs` целиком (787 строк).
- `43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs` целиком (665 строк) — это и есть
  «обучаемая» единица решения (`ILogicalUnit<PromptToTaskRequest,…Decision>`).
- `43.SmartSkillsDomain/PromptToTaskAblationHarness.cs` целиком (551 строка).
- `XiigenExtensionApiService.cs` стадия 1 (`:106-129`) + `LoadCommonAssets` (`:1389-1471`).
- Промоутнутый ассет `models/prompt-to-task/prompt-to-task-v1/manifest.json` (целиком) и
  `prompt-to-task-v1-checkpoint.json` (голова: 12 паттернов).
- `tools/PromptToTaskPhase04Runner/Program.cs` (грепы held-out/accuracy/review-cycle).
- Грепы всех callers (`EvaluateHeldOutRows`, `FindNearestPromptToTaskPatterns`,
  `ContinueWithAcceptedPattern`, `PromptToTaskAblationHarness`, `new PromptToTaskMachine`).

## Ключевой факт: механизм = подстрочный словарь ключевых слов, не обучение

`PromptToTaskMachine.Translate` (`:112`) грузит состояние → строит
`PromptToTaskTaskPatternUnit` → `Decide`. Внутри `Decide` (`PromptToTaskTaskPatternUnit.cs:154`)
запрос превращается в набор «сигналов» и матчится к паттернам по overlap:

- Сигналы промпта строятся `ExtractPromptSignals` (`:339`) = токены 4–40 символов **плюс**
  `BuildUniversalSignalAliases` (`:443-490`) — это **жёстко зашитый словарь ~40 правил вида**
  `prompt.Contains("plan") → "plan"`, `"approval"/"approve" → "approval"`,
  `"do not implement" → "implementation_authorized_false"`, `"gate" → "missing_gates"`,
  `"audit" → "audit"`, `"deliverable"/"artifact" → "deliverable"`, `"source"/"reference" → "source"`,
  `"context" → "context"` и т.д.
- `BuildNearestPattern` (`:502-529`): `coverage = matched/patternSignals`,
  `confidence = pattern.Confidence * coverage`. `pattern.Confidence` — **сохранённая константа**
  из чекпойнта (0.95, 0.94, …), не обновляется ничем.
- Промоутнутый чекпойнт подтверждает круг: `learnedPatterns[*].learnedSignals` **буквально равны**
  тем же alias-токенам из кода. Паттерн `q3-01-prototype-planning-approval-boundary`
  (`checkpoint:18-56`) → `learnedSignals=[approval, deterministic_shortcut,
  implementation_authorized_false, missing_gates, phase, plan, planning, scope_creep]`,
  `taskType=plan_only_or_approval_gated_task`, `confidence=0.95`. То есть «обучение» не выучило
  словарь сигналов — словарь захардкожен в коде, а «паттерны» лишь группируют его в 12 taskType.

Собственный `PromptToTaskPhase04Runner` это признаёт (`Program.cs:683`): «Использован прозрачный
nearest-signal baseline… помечен как baseline/internal и не является semantic quality claim».
`semanticQualityClaimed=false` зашит по всему пути (Machine, Unit, чекпойнт, манифест).

## Проводка в проде

`XiigenExtensionApiService` реально грузит общий чекпойнт: `LoadCommonAssets:1403-1414`
(`models/prompt-to-task/prompt-to-task-v1/…`), при успехе `promptToTask = pttLoad.Value!.Machine`.
Стадия 1 рантайма — `activeLoadState.PromptToTaskMachine.Translate(...)` (`:118`). Значит на
проде с загруженным ассетом машина отдаёт задачу через keyword-overlap (а не всегда not_ready,
как утверждал P02 для незагруженного состояния). **Коррекция/уточнение к P02:** not-ready —
это состояние БЕЗ загруженного чекпойнта; с промоутнутым ассетом путь проходит и выдаёт
keyword-lookup задачу. Fresh-load — единственный gate с реальным покрытием (сервис грузит
чекпойнт свежим `LoadCheckpointAsync`, есть unit-тесты `PromptToTaskMachineAssetLifecycleTests`,
`SmartSkillsSemanticQualityP11FreshLoadTests`), но fresh-load доказывает лишь байтовую
переустановку состояния, НЕ generalization.

## Held-out / ablation / continue-training — что реально есть и почему это дыры

- **Held-out harness ЕСТЬ в коде** (`PromptToTaskTaskPatternUnit.EvaluateHeldOutRows:247-287`,
  порог `MinimumEvaluationAccuracy=0.8`), но callers: только `Tests/PromptToTaskContractsTests.cs:411`
  (синтетика, ассерт «below threshold», `:433`) и `PromptToTaskPhase04Runner/Program.cs:243-283`
  (5 сценариев held-out, **авторски написанных в самом раннере** под те же 5 границ taskType, что
  кодируют 12 паттернов — plan/audit/source/review/deliverable) + `:136` accuracy=passed/total.
  Ни один раннер не подаёт `EvaluateHeldOutRows` **реальный source-disjoint** набор и не гейтит
  промоушен на ≥0.8. Манифест: `dpoPairCount:8`, `heldOutRowCount:4` — крошечный корпус.
- **Ablation harness — структурный no-op** (`PromptToTaskAblationHarness.cs:79`):
  `_translate ?? (request => new PromptToTaskMachine().Translate(request))` — **машина без loader
  → всегда not-trained → not_ready**. Отчёт хардкодит `TrainingRun/EvalRun/CheckpointOrModelChanged/
  SemanticQualityClaimed/TrainingReady=false` (`:132-136`); self-check ждёт `NotReadyCount ==
  PlannedProbeCount` (`:447`). Мандатный ablation-gate (CLAUDE.md) никогда не гоняет загруженное
  решение → не может показать инвариантность к input-transform (в этом весь смысл ablation).
- **Continue-training = append, не обучение** (`PromptToTaskMachine.ContinueWithAcceptedPattern:254-290`):
  берёт **готовый** `PromptToTaskLearnedPattern` (с уже проставленными Confidence/LearnedSignals) и
  конкатенирует в состояние. Нет ни DPO/градиента, ни counter-update; `pattern.Confidence` — константа
  (`BuildNearestPattern:518`). То есть требуемое «continue-training from checkpoint» — метаданный
  append, а не learning update от строк данных к мутации типизированного состояния.
- **Пороги тривиальны:** `FromLearnedState` (`:99-100`) `minimumConfidence=0.01`,
  `minimumTopTwoMargin=0.01`; `Decide:187/195` гейтит по ним. При `confidence=0.94*coverage`
  один совпавший ключ из 8-сигнального паттерна даёт ≈0.118 ≫ 0.01 → «ready». Гейт по
  уверенности/марже фактически всегда открыт; один keyword → уверенная задача.
- **Personal leg не подключён на Translate-пути:** Unit поддерживает two-leg
  (`LoadState = loadUserState() ?? loadCommonState()`, `:289-290`), но Machine строит его
  single-state конструктором (`Machine.cs:124-125` → `Unit.cs:90-93` `this(() => state, () => null)`),
  так что `loadCommonState` всегда null и ветка user→common fallback мертва; персональная/общая
  адаптация в prompt-to-task делается не в юните, а подменой целой машины на уровне сервиса
  (`LoadCommonThenUserAdaptiveAssets`).

## Trainable vs Bootstrap — вердикт среза

Prompt-to-task на проде — не Bootstrap-обёртка статической логики (как 42.ProjectWiki TD1) и не
настоящий trainable юнит: это **детерминированный keyword→taskType lookup**, где «обученное
состояние» — авторски составленные 12 паттернов поверх захардкоженного словаря сигналов, с
константными confidence, без training-update, с held-out на самосогласованной синтетике, с
ablation-гейтом, который никогда не грузит модель, и continue-training = append. `semantic_quality`
честно не заявляется — но и trainable-жизненный цикл (learning signal → state mutation → held-out
uplift → ablation-инвариантность) не доказан ни одним прогоном.

## Новые дыры (детально) — см. таблицу в мастере (P21)

Дедуп: НЕ повторяю P01 phase-8 (фазовый held-out/fresh-load/semantic), P02 (inference «always
not-ready» без состояния; pipeline не собран в 15; SkillSelection position-counter), P05
(трансформерный DPO-движок отсоединён; «обучение расширений = dictionary/counter» — общий тезис),
P10/P14/P18 (покрытие методов/пустой каталог/flat-chain). Здесь — конкретный код prompt-to-task
с file:line: захардкоженный alias-словарь как authority, ablation-no-op, круговой held-out,
continue-training-append, тривиальные пороги, single-state collapse two-leg.
