# Opus P02 — Определение «полной реализации» (definition of done) из планов + сверка реальным кодом

Пасс 02 из 50. Оценщик: Opus 4.8 (REAL-STATE ASSESSOR).
Дата: 2026-07-08. Папка: `C:\Users\lubao\Documents\xiigen\llm mvp\extension development`.

## 0. Задача этого прохода

Извлечь из скопированных планов КОНКРЕТНОЕ определение «полной реализации»
(required capabilities/gates) — это фиксирует «done» для всего проекта расширений.
Затем — там где определение done ссылается на конкретный код — сверить с ЖИВЫМ
кодом `C:\Projects\llm_mvp_core` (bounded греп/чтение), чтобы новые дыры имели
реальные file:line, а не только формулировку плана. Глубокий per-file аудит —
задача P03+; здесь сверка точечная, только для якорения гейтов done.

Рамка честности (из P01):
- **COPIED_DOC_ONLY** — план/док скопирован; кода нет.
- **BUILT_NOT_PROVEN** — реальный код есть, но не доказан рантаймом/тестами.
- **STUB** — код-скелет, всегда возвращает not-ready/константу, inference не реализован.
- **PROVEN** — есть рантайм/тест/dual-path доказательство.

## 1. Что прочитано в этом проходе

Планы (bounded, шапки/gate/ledger-секции):
- `copied-plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` (Target Outcome Matrix + dual-path gate)
- `copied-plans/ORCHESTRATION-AND-PROMPT-TO-TASK-FULL-EXTENSIONS-PLAN-v3.md` (§0 11 возможностей + 10 gates; §0/§A0 частично mojibake)
- `copied-plans/API-DASHBOARD-PLAN-v1.md` (Authority Requirements Ledger AR-01..AR-22)
- `copied-plans/DPO-KNOWLEDGE-MAP-v1.md` (полностью)
- `copied-plans/TRAINABLE-ARCHITECTURE-AUDIT-v1.md` (разрешённые уровни + подозрительные счётчики)
- `copied-plans/EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md` (канон DONE/NOT_DONE)
- `copied-plans/EXTENSION-REALITY-MAP-v1.md` (копи-рекон 2026-06-04 — используется как ГИПОТЕЗА, сверяется с живым кодом)

Живой код (bounded греп/чтение, `C:\Projects\llm_mvp_core`):
- `43.SmartSkillsDomain\PromptToTaskMachine.cs`, `OrchestrationReasoningMachine.cs`,
  `SkillSelectionMachine.cs`, `README.md`, `Phase08CommonUserTwoLegLifecycle.cs`
- `15.OrchestratorModel` (греп на регистрацию 3 машин)
- греп на `CandidateOrderLeakageGuard` по всему репо

## 2. Сводное определение «полной реализации» (мастер-чек-лист done)

Пересечение всех планов. Для КАЖДОГО из 3 хостов (Claude, Cursor, Codex)
полная реализация = все пункты доказаны dual-path (Path A boundary + Path B local desktop):

1. **Host entrypoint** — реальный код точки входа + manifest/package wiring (v3 #11).
2. **Settings/debug surface** — debug setting, видимый raw/redacted I/O, error path, свежий correlation id (runtime-plan `:119`).
3. **Target-locator / core-API path** — тонкий adapter строит `XiigenExtensionRequest`, вызывает `XiigenExtensionApiService`, показывает `XiigenExtensionResponse`; НИКАКОЙ логики в адаптере (runtime-plan `:120`).
4. **Prompt → task** — `PromptToTaskMachine` (v3 #3).
5. **Task → orchestration/plan** — `OrchestrationReasoningMachine`/`OrchestrationPlanMachine` (v3 #4).
6. **Task/plan → skills** — `SkillSelectionMachine` (v3 #5).
7. **Local adaptive learning** — DPO из user-approved/source-grounded данных, две ноги common/adaptive (v3 #6-10, runtime-plan `:123`).
8. **Export/import/backup/continue-training + rollback** — lifecycle доказан (runtime-plan `:123`, DPO-map held-out/fresh-load/continue-training).
9. **Token before/after** — доказано на парных промптах (одинаковые hash/host/tokenizer), без регрессии качества (runtime-plan `:121`).
10. **Semantic quality** — послойные скоры (PromptToTask/OrchestrationReasoning/SkillSelection) + held-out + ablation + no-memorization (runtime-plan `:122`, audit-v1).
11. **Checkpoint/locator** — promotion через `IModelRepository`/`IRemoteUnitLocator` когда применимо (runtime-plan `:123`).
12. **QA** — через живой host ИЛИ scoped handler; для dashboard дополнительно full WPF UI automation + UX automation (API-dashboard AR-08/18/22).
13. **Dual-path** — Path A (boundary) И Path B (local desktop) для одного и того же сценария/prompt.
14. **Архитектурная легитимность** — все обучаемые единицы в разрешённых уровнях, без скрытого детерминизма (audit-v1 `:37-56`).
15. **Метод** — исполнение только через bounded sub-agents; ≥10 plan reviews ДО и ≥10 dual-path проверок ПОСЛЕ (runtime-plan `:124-126`).

## 3. Сверка определения done с живым кодом (новые якоря)

### 3.1 Prompt→task и Task→orchestration — это СТАБЫ, не «BUILT_NOT_PROVEN»
- `PromptToTaskMachine.cs:90` → `NotReadyTaskType = "not_ready_no_semantic_task"`.
- `43.SmartSkillsDomain\README.md:52` → «Skeleton implementation. No semantic classification or inference is implemented.»
- `OrchestrationReasoningMachine.cs:109` → `NoInferenceScenarioId = "not-ready-no-orchestration-inference"`.
Вывод: пункты done #4 и #5 в реальном коде НЕ реализованы даже на уровне inference —
это скелеты, всегда возвращающие not-ready. Реестр P01 назвал phase-8/phase-9
«BUILT_NOT_PROVEN» — это оверклейм: строить/проверять нечего, кроме контракта.

### 3.2 SkillSelection — обучаема, но решает по ПОЗИЦИИ кандидата (order leakage)
- `SkillSelectionMachine.cs:65` → `private readonly Dictionary<int,int> _candidatePositionWeights = new();`
- `SkillSelectionMachine.cs:767` → `_candidatePositionWeights[index.Value] = ... + 1;` (обучение по ИНДЕКСУ кандидата).
- `SkillSelectionMachine.cs:113` → решение стартует от `_candidatePositionWeights.Count` (позиционные веса = authority).
- Греп `CandidateOrderLeakageGuard` по всему репо → присутствует ТОЛЬКО в `docs/skills/*`, `CLAUDE.md`, `AGENTS.md` и `44.DesignConversionDomain\*` — в `43.SmartSkillsDomain` guard ОТСУТСТВУЕТ.
Вывод: единственная обучаемая машина конвейера учится на позиции кандидата в списке —
именно тот order-leakage, который CLAUDE.md/audit-v1 запрещают как финальную authority;
мандатный `CandidateOrderLeakageGuardUnit` к ней НЕ применён (реализован в другом домене).

### 3.3 Конвейер не собран в оркестраторе
- Греп `15.OrchestratorModel` на `PromptToTask|OrchestrationReasoning|SkillSelection` → **No files found**.
Вывод: три стадии не зарегистрированы/не связаны в `15.OrchestratorModel`; сквозной
prompt→task→orchestration→skills как единый runtime-flow недостижим (Trainable Unit
pattern требует регистрации flow-адаптера в `15.OrchestratorModel`).

### 3.4 Meta: копи-рекон пакета УСТАРЕЛ
- `EXTENSION-REALITY-MAP-v1.md` (2026-06-04) GAP-3/GAP-6: «SkillSelection ExportAsync/ImportAsync return NOT_READY, no persistence».
- Реальный код `SkillSelectionMachine.cs:397` (`ExportAsync`) и `:412` (`ImportAsync`) — полноценные реализации: Export пишет checkpoint+manifest в persistence-директорию; Import валидирует MachineId/DomainTag/CheckpointRef/ManifestRef, читает манифест/checkpoint и `LoadCheckpoint`.
Вывод: рекон пакета — устаревший снимок; принимать package-артефакты как current-state НЕЛЬЗЯ. Планировать по каноничным версиям из `llm_mvp_core\docs\plans` + свежий bounded-рекон.

### 3.5 Two-leg lifecycle частично есть в коде
- Греп в `43.SmartSkillsDomain` → `Phase08CommonUserTwoLegLifecycle.cs`, `SmartSkillsSemanticUnitCatalog.cs`, contracts, machine, README упоминают `ILearningMachinePair`/`anonymised`.
Вывод: two-leg scaffold существует (BUILT_NOT_PROVEN), но anonymised backup/export/import/share round-trip не доказан прогоном; для 2 стаб-машин personal leg вообще невозможен без inference.

## 4. Новые дыры P02 (только те, которых нет в реестре P01)

11 новых дыр записаны в `opus-PROJECT-CLOSURE-PLAN.md` (Pass P02). Кратко:
1. Thin-adapter no-hidden-determinism review gate (MISSING GATE).
2. Prompt→task inference — STUB (real code), корректирует оверклейм P01 phase-8.
3. Task→orchestration inference — STUB (real code), корректирует P01 phase-9.
4. SkillSelection candidate-INDEX order-leakage + отсутствие `CandidateOrderLeakageGuardUnit` в 43-домене.
5. Конвейер не зарегистрирован в `15.OrchestratorModel` (MISSING WIRING).
6. Two-leg pair + anonymised adaptive sync (BUILT_NOT_PROVEN / частично).
7. DPO-KNOWLEDGE-MAP admission gate не выполнен + DPO signal builders contract-only.
8. v3 десять сквозных work-order gates не доказаны исполнением.
9. Метод-гейт: ≥10 plan-reviews ДО + ≥10 dual-path ПОСЛЕ (0/0; отличается от package 50-pass).
10. Dashboard blocking gates AR-10/18/20/22 (WPF UI automation + UX automation + ≥2 review).
11. Package def-of-done source integrity: v3 mojibake + устаревший копи-рекон.

## 5. Разрыв до full implementation и приоритеты закрытия

- Определение done извлечено и сведено в 15-пунктовый чек-лист (§2). По самим планам это COPIED_DOC_ONLY.
- Реальная сверка (§3) показывает: 2 из 3 стадий конвейера (prompt→task, task→orchestration) — СТАБЫ без inference; 3-я (skills) обучаема, но с order-leakage и без guard; конвейер не собран в оркестраторе; персистентность SkillSelection реально есть (delta к устаревшему рекону).
- Приоритет P03+: (1) реализация inference для prompt→task и task→orchestration (без них весь конвейер и Path B недостижимы); (2) устранение order-leakage в SkillSelection + guard; (3) сборка/регистрация конвейера в `15.OrchestratorModel`; (4) реальные host-адаптеры (P01) под dual-path.
- Планирование вести по каноничным `llm_mvp_core\docs\plans`, а не по повреждённой/устаревшей копии пакета.

## 6. Вывод P02

Формальное «done» полное и извлечено (15 пунктов, §2), источники честные и взаимно
согласованные. Новизна P02 против P01: определение done переведено из фазовых
формулировок в конкретные gates/capabilities и заякорено реальным кодом — что
вскрыло два оверклейма P01 (prompt→task и task→orchestration — стабы, не
BUILT_NOT_PROVEN), один живой архитектурный дефект (order-leakage без guard), одну
отсутствующую сборку (нет регистрации в оркестраторе) и один delta к устаревшему
рекону (Export/Import реализованы). Проект НЕ завершён; выполнение критериев done в
живом коде продолжают пассы P03+.
