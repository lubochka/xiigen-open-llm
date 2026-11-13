# Pass P10 — Claude: реальный путь prompt-to-task → orchestration → skills

Дата: 2026-07-08. Ассессор: Opus 4.8. Слайс: РЕАЛЬНЫЙ конвейер обработки промпта
Claude — от точки входа Claude через core до трёх стадий (prompt→task,
task→plan/orchestration, task/plan→skills). Оценка ведётся от живого кода
`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\*` и живого host-репо
`C:\Projects\claude-xiigen-smart-skills\*`, а не от копий пакета.

## Что реально прочитано (bounded)

- `43.SmartSkillsDomain\XiigenExtensionApiService.cs` — `ProcessHostExtensionRequest`
  (`:88-201`), `DecideSkills` (`:356-399`), `BuildPromptToTaskRequest` (`:505-520`),
  `BuildOrchestrationRequest` (`:522-545`), `BuildWarnings` (`:877-918`),
  `BuildHostChatDisplayText`/`BuildMethodSpecificDisplayText` (`:927-975`),
  `ShouldClaimSemanticQuality` (`:1364-1387`), константы (`:16-46`).
- `43.SmartSkillsDomain\PromptToTaskMachine.cs` — `Translate` (`:112-215`).
- `43.SmartSkillsDomain\OrchestrationReasoningMachine.cs` — `BuildContractEvidence`
  (`:569-608`), статусные константы (`:105-107`).
- `claude-xiigen-smart-skills\src\claude-entrypoint.js` — дефолты запроса
  (`:8-18`), `buildRequest` (`:313-357`).
- `claude-xiigen-smart-skills\.claude-plugin\plugin.json` — команда SessionStart-hook.
- `claude-xiigen-smart-skills\prompts\claude-smoke-prompts.jsonl` — 2 smoke-промпта.
- Grep-проверки: `SEMANTIC_QUALITY_CLAIMED_TRUE` по всему репо; наличие 3 машин в
  `15.OrchestratorModel` (→ 0).

## Ключевой факт: конвейер СУЩЕСТВУЕТ и СВЯЗАН, но плоско в 43 (не через 15)

Вопреки узкой формулировке P02 («pipeline не собран в оркестраторе», «end-to-end
недостижим»), три стадии РЕАЛЬНО сцеплены и достижимы — но не через
иерархию `ILogicalOrchestrator/ILogicalFlow` в `15.OrchestratorModel`, а
руками, внутри плоского метода `XiigenExtensionApiService.ProcessHostExtensionRequest`:

```
:117  promptToTaskRequest  = BuildPromptToTaskRequest(request, sha)
:118  promptToTaskResult   = PromptToTaskMachine.Translate(...)
:130  orchestrationRequest = BuildOrchestrationRequest(request, structuredTask)
:131  orchestrationResult  = OrchestrationReasoningMachine.Plan(...)
:143  skillSelection       = DecideSkills(request, sha, loadState)   // SkillSelectionMachine
:155  semanticQualityClaimed = ShouldClaimSemanticQuality(ptt, orch, skill)
```

Grep `15.OrchestratorModel` на все три машины → **0 файлов**. То есть коррекция к
P02: путь не «отсутствует/недостижим», он достижим, но архитектурно ПЛОСКИЙ —
последовательные `.Translate/.Plan/.DecideSkills`, минуя разрешённые уровни
Level-4/Level-3 и `15.OrchestratorModel`. Ни A-to-B пары юнитов, ни fan-out,
ни arbiter-пути (требования def-of-done v3 #3-5, P02) в этой сцепке не смоделированы —
это жёсткий трёхшаговый вызов, не композируемый flow, не обнаруживаемый локатором.

## Ключевой факт: гейт «semantic_quality_claimed» НЕДОСТИЖИМ по построению

`ShouldClaimSemanticQuality` (`:1364-1379`) возвращает true только если ВСЕ три
компонента дают позитивный токен `PositiveSemanticQualityEvidence =
"SEMANTIC_QUALITY_CLAIMED_TRUE"` (`:17`) И ни один не даёт no-claim токен
(`SemanticNoClaimEvidence` = `BASELINE_NOT_SEMANTIC_QUALITY` /
`SEMANTIC_QUALITY_CLAIMED_FALSE`, `:29-33`).

Проверка эмиссии `SEMANTIC_QUALITY_CLAIMED_TRUE` по всему репо: встречается ТОЛЬКО
в `Tests\*`, `tools\*Runner\Program.cs`, `docs\plans\*` — **ни в одной из трёх
машин**. То есть продуктовый путь НИКОГДА не выставляет позитивный токен.

Хуже: каждая машина сама вшивает no-claim токен даже на УСПЕШНОМ пути:
- `PromptToTaskMachine.cs:206-207` — при `LEARNED_PATTERN_MATCHED` в
  `ContractEvidence` всё равно `"BASELINE_NOT_SEMANTIC_QUALITY"` +
  `"SEMANTIC_QUALITY_CLAIMED_FALSE"`.
- `OrchestrationReasoningMachine.cs:599-601` — при загруженном guide-state всё
  равно `"BASELINE_NOT_SEMANTIC_QUALITY"`, `"HOST_RUNTIME_CLAIMED_FALSE"`,
  `"SEMANTIC_QUALITY_CLAIMED_FALSE"`.
- `SkillSelectionMachine` — scaffold-токен `CANDIDATE_POSITION_WEIGHT_SCAFFOLD_ONLY`
  тоже блокирует гейт (`:1373`).

Вывод: `semantic_quality_claimed` и (жёстко зашитый) `runtime_extension_claimed`
для Claude-пути **false-by-construction**. Лучший достижимый ответ Claude —
keyword-pattern baseline, который сам себя объявляет несемантическим. Это сильнее,
чем P02 «машины-стабы возвращают not-ready» и чем P05 «checkpoint = словарь»:
даже когда стадия «успешна», агрегирующий гейт математически не может стать true.

## Ключевой факт: Claude-хост кормит конвейер ПУСТЫМ каталогом скиллов

Дефолт запроса Claude-entrypoint (`claude-entrypoint.js:8-18`):
`Method:"prompt-to-task"`, `RequestedSkills:[]`, `AvailableSkills:[]`.
Поставляемый SessionStart-hook (`plugin.json`) передаёт только `-ConcreteTask`,
`-Prompt` (зашитый smoke), `-CheckSetJson "[]"`, `-CoreRoot`, `-TargetLocator*`,
`-Output/TracePath` — **нет `-Method`, нет `-AvailableSkills`, нет `-RequestedSkills`**.

Следствие в ядре: `DecideSkills` (`:361-371`) при `AvailableSkills.Count==0`
немедленно возвращает `NO_AVAILABLE_SKILLS_PROVIDED`, `SelectedSkillIds:[]`,
`Confidence:0.0`, `RequiresSecondPass:true`. Значит третья стадия конвейера
(«task/plan → skills») на Claude-пути — **постоянный no-op**: ни один скилл
никогда не может быть выбран, потому что Claude-хост не отдаёт каталог доступных
скиллов. Даже smoke-промпт `claude-smoke-001` текстом описывает «prepare a
skill-selection request», но фактически каталог не передаётся.

Дополнительно `RequiresSecondPass:true` из пустого каталога само по себе снова
роняет `ShouldClaimSemanticQuality` (`:1371`).

## Ключевой факт: из Claude драйвится только метод prompt-to-task

Поставляемый hook не задаёт `-Method` → всегда `prompt-to-task`. Значит хостовые
методы `orchestration-plan` и `skill-selection` (ветки
`BuildMethodSpecificDisplayText:966-971`) никогда не запускаются end-to-end из
Claude. Внутри `ProcessHostExtensionRequest` все три стадии прогоняются всегда, но
Claude-поверхность выбирает и показывает только prompt-to-task-ветку. 2 из 3
хостовых методов имеют нулевое Claude-покрытие.

## НОВЫЕ дыры P10 (не дублируют P01–P09)

| Component | Real-status | Evidence file:line | Gap | Closure steps |
|---|---|---|---|---|
| Гейт `semantic_quality_claimed` Claude-конвейера | DEAD-GATE / false-by-construction | `XiigenExtensionApiService.cs:17,:1364-1379`; `PromptToTaskMachine.cs:206-207`; `OrchestrationReasoningMachine.cs:599-601`; grep `SEMANTIC_QUALITY_CLAIMED_TRUE` → только Tests/runners/docs | Агрегирующий гейт требует токен `SEMANTIC_QUALITY_CLAIMED_TRUE` от всех трёх стадий, но НИ ОДНА машина его не эмитит, а каждая жёстко вшивает `SEMANTIC_QUALITY_CLAIMED_FALSE`/`BASELINE_NOT_SEMANTIC_QUALITY` даже на успешном пути. Значит semantic-quality Claude-ответа недостижим математически, а не «пока не доказан». Сильнее P02 (stub not-ready) и P05 (checkpoint=словарь). | Реализовать настоящий обучаемый inference, эмитящий позитивную evidence на разрешённом уровне; убрать безусловные no-claim токены из success-пути; доказать held-out порогом, что гейт может стать true. |
| Стадия 3 (task/plan→skills) на Claude-пути | NO-OP / empty-catalogue | `claude-entrypoint.js:8-18` (`AvailableSkills:[]`,`RequestedSkills:[]`); `plugin.json` hook (нет `-AvailableSkills/-RequestedSkills`); `XiigenExtensionApiService.cs:361-371` `NO_AVAILABLE_SKILLS_PROVIDED`,conf 0 | Claude-хост никогда не передаёт каталог доступных скиллов, поэтому выбор скиллов всегда пустой — третья стадия конвейера физически не работает на Claude. Отдельно от P02 (order-leakage внутри машины) и P09 (manifest surface). | Дать Claude-хосту реальный каталог скиллов (из репо skills index) в `AvailableSkills`; прогнать skill-selection с непустым входом; доказать выбор + ablation, что это не lookup по позиции. |
| Хостовое покрытие методов конвейера из Claude | PARTIAL (1/3 methods) | `plugin.json` hook (нет `-Method`) → `claude-entrypoint.js:10` default `prompt-to-task`; `XiigenExtensionApiService.cs:966-971` ветки orchestration-plan/skill-selection; `claude-smoke-prompts.jsonl` (метод/скиллы не заданы) | Поставляемая Claude-поверхность драйвит только метод `prompt-to-task`; хостовые методы `orchestration-plan` и `skill-selection` из Claude никогда не исполняются end-to-end. 2 из 3 методов конвейера имеют 0 Claude-покрытия. | Добавить в Claude-хост команды/пути для методов `orchestration-plan` и `skill-selection` с реальными входами; прогнать каждый метод end-to-end через Claude; собрать per-method evidence. |
| Сборка конвейера prompt→task→orch→skills = плоская цепочка в 43, не через `15.OrchestratorModel` (коррекция P02) | ARCH-FLAT / not-orchestrated | `XiigenExtensionApiService.cs:117-155` (ручная цепочка `.Translate/.Plan/.DecideSkills`); grep 3 машин в `15.OrchestratorModel` → 0 | Коррекция P02 «pipeline не собран/недостижим»: он ДОСТИЖИМ, но собран руками внутри плоского сервиса, минуя `ILogicalOrchestrator/ILogicalFlow` и `15.OrchestratorModel`. Не композируется как flow, не обнаруживается локатором; A-to-B пары/fan-out/arbiter (def-of-done v3 #3-5) не смоделированы. | Обернуть три стадии в разрешённый `ILogicalOrchestrator` + `ILogicalFlow` в `15.OrchestratorModel`, зарегистрировать flow-адаптеры; доказать сквозной прогон одного промпта через оркестратор с A-to-B/fan-out/arbiter-структурой, а не через плоский вызов. |

## Границы этого прохода

- НЕ прогонял build/test/runtime (assessor-роль, bounded reads).
- НЕ инспектировал `SkillSelectionMachine.DecideFromTrainedState` внутренности
  (order-leakage уже покрыт P02); проверял только вход/выход стадии на Claude-пути.
- Cursor/Codex конвейеры — вне слайса P10 (Claude only).
- «Semantic quality недостижим» — доказано анализом кода эмиссии токенов, не
  прогоном; но факт «ни одна машина не эмитит SEMANTIC_QUALITY_CLAIMED_TRUE»
  подтверждён grep по всему репо (0 совпадений в машинах).
