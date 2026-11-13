# RAG-11 — «extensions reasearch» (папка #11/12)

Источник: `C:/Users/lubao/Documents/xiigen/extensions reasearch` (2200 файлов, sic «reasearch»).
Назначение RAG: питать план ПРОДОЛЖЕНИЯ расширений Visual Studio и VS Code.
Роль папки: здесь лежат ОДНОВРЕМЕННО (а) первые планы расширений (marketplace, token-optimizer, skill-selection-machine, local-testing), (б) три вендоренных host-репозитория с реальным thin-adapter кодом расширений, (в) движок FigmaToCode (design→code). ОЧЕНЬ высокая релевантность — тут и планы, и реальный код расширений.

Проходов всего: 10 (кумулятивно; каждый следующий дописывает этот файл).

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C3-R11, Цикл 3 документного обновления; добавка — ничего ниже не удалено и не переписано). Папка-источник (включая три вендоренных host-репозитория с кодом расширений) статична и на месте. Изменилось окружение; авторитетная таблица — `BRANCH-STATUS-CURRENT-2026-07-28.md` в этой же папке.
> 1. **Рабочие хост-репозитории `claude-/cursor-/codex-xiigen-smart-skills` отсутствуют в `C:\Projects`** (проверено 2026-07-28). Эталонный код жив здесь, в вендоренных копиях папки-источника; но любые шаги «открой рабочий репозиторий хоста на диске» сегодня невыполнимы без повторного клонирования с сервера. Настройка по умолчанию `coreRoot` → `C:\Projects\llm_mvp_core` указывает на копию, стоящую теперь на ветке `codex/dpo-implementation` (`0f221ac89`) с 486 несохранёнными записями.
> 2. **Лицензионно-монетизационный анализ (§AE freemium, ключ `XIIGEN_LICENSE_KEY`, планы `*-LICENSING`) написан до волны GPL/AGPL 23–26 июля** (ветка `PrepareToGPL` в ядре = main + 19 коммитов лицензирования; «Prepare to AGPL» в `xiigen-open-llm`; «prepare to gpl» в `xiigen-mvp`). Совместимость freemium-модели с копилефт-лицензированием — открытый содержательный вопрос, здесь только фиксируется.
> 3. Ветки офлайн-документации кода `local_wiki_planning` (`219d7ba89`) и `claude/sonnet-local-wiki` (`93a463817`) статичны с мая — переподтверждено `git ls-remote` 2026-07-28; 24 июля `main` впитал линии design-to-code и DPO (PR #38/#39).

---

## Pass 1/10 — инвентарь 2200 файлов + PLAN-SKILL-SELECTION-MACHINE-EXTENSION ч.1

### A. Полная опись верхнего уровня (top-level файлы)

Планировочные `.md` (главный приоритет для continue-плана):

| Файл | Размер | Что это | Релевантность |
|---|---|---|---|
| `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` | 132 KB / 3252 строк | ГЛАВНЫЙ план: расширение выбирает нужные скиллы обучаемым слоем (`SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>`). Статус-баннер сверху: «SOURCE INPUT — IMPLEMENTED (2026-06), реализовано в Phase 39 плана ORCHESTRATION-MACHINE-TRAINING-PLAN-v20; текущие контракты в `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\`». | КРИТИЧЕСКАЯ |
| `XIIGEN-MARKETPLACE-MASTER.md` | 69 KB | Мастер-план маркетплейса расширений (20+ платформ). | Высокая |
| `XIIGEN-MARKETPLACE-MASTER-PLAN.md` | 62 KB | План/дорожная карта маркетплейса. | Высокая |
| `XIIGen-Desktop-Plugin-Adaptation-Report-v2.md` (+ `.docx`, `.md.pdf`) | 53 KB | Отчёт об адаптации XIIGen как desktop-плагина; как ядро llm_mvp_core становится плагином хостов. | Высокая |
| `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v1-CLAUDE.md` | 21 KB | Первый план плагина «token optimizer» для Claude. Его Hook 2 УСТАРЕЛ (заменён обучаемым skill-selection; см. §0 главного плана). | Средняя (историческая база хук-упаковки) |
| `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CODEX-PORT-v1.md` | 14 KB | Порт token-optimizer для Codex. | Средняя |
| `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CURSOR-PORT-v1.md` | 13 KB | Порт token-optimizer для Cursor. | Средняя |
| `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-LICENSING-v1.md` | 12 KB | Лицензирование/монетизация плагина. | Средняя |
| `PLAN-LOCAL-TESTING-CLAUDE-CODE-v1.md` | 15 KB | План локального тестирования host-сценариев (Claude Code). | Высокая (Path B / dual-path) |
| `PLAN-LOCAL-TESTING-CODEX-v1.md` | 17 KB | То же для Codex. | Высокая |
| `PLAN-LOCAL-TESTING-CURSOR-v1.md` | 17 KB | То же для Cursor. | Высокая |
| `CONFLICT-MAP.md` | 4 KB | Карта конфликтов между планами/версиями. | Служебная |
| `CONSOLIDATION-INDEX.json` | 20 KB | Индекс консолидации marketplace-планов. | Служебная |
| `STATE-consolidation.json` | 1 KB | Состояние консолидации. | Служебная |
| `STATE-marketplace-final.json` | 1.5 KB | Финальное состояние marketplace-плана. | Служебная |

Не-планировочный «шум» (низкая релевантность к continue-плану расширений):

| Файл | Что это |
|---|---|
| `PLAN--3D-Lesson-Generation-Pipeline-v1.md.pdf` (810 KB), `XIIGEN - 3D Hero Interaction Pipeline.pdf` (1.3 MB) | 3D-контент-пайплайны — вне темы расширений IDE. |
| `FigmaToCode-...zip` (3.8 MB), `V2 files.zip` (41 KB) | Архивы (распакованные версии рядом). |

### B. Опись директорий (структурно, не поштучно)

| Директория | Файлов | Тип | Ключевые входные файлы | Релевантность |
|---|---|---|---|---|
| `cursor-xiigen-smart-skills/` | 1617 | РЕАЛЬНЫЙ host-репозиторий расширения Cursor/VSCode (TypeScript) | `src/extension.ts` (1628 строк — единственный src-файл), `dist/extension.js`, `package.json` (5 KB), `xiigen-extension-contract.json`, `docs/PROJECT-DOC.md` | КРИТИЧЕСКАЯ (реальный код + контракт) |
| `claude-xiigen-smart-skills/` | 102 | host-репозиторий Claude Code (plugin) | `.claude-plugin/`, `hooks/`, `src/claude-entrypoint.ps1` (246 строк PowerShell), `scripts/{install-smoke,smoke}.ps1`, `prompts/claude-smoke-prompts.jsonl`, phase40-*.json трейсы | КРИТИЧЕСКАЯ |
| `codex-xiigen-smart-skills/` | 93 | host-репозиторий Codex | `.codex/`, `src/codex-entrypoint.ps1` (246 строк), `scripts/install-smoke.ps1`, `prompts/codex-smoke-prompts.jsonl`, phase40-codex-host-session-run* трейсы | КРИТИЧЕСКАЯ |
| `FigmaToCode-design-gen-improve_group_css_user/` | 346 | Движок FigmaToCode (design→code) — Figma-плагин v2.0.0, React 17 + TS 4.1 + webpack 4 | вложенная одноимённая папка → `src/plugin/core/{controller,op-executor}.ts`, `src/plugin/models/{Element,Style,Setting,IStorage,...}.ts`, `src/ui/components/design-gen/` (провайдеры Claude/OpenAI, skill-advisor: SkillAdvisor/SkillJudge/skill-pipeline/skill-registry/MemoryStore), `src/ui/components/ai-enhance/{AIService,SkillInjector,PromptTemplates}.ts`, `manifest.json`, `package.json`, `CLAUDE.md` (29 KB), `AGENTS.md` | Высокая (отдельный design→code продукт) |
| `Plans/` | 15 | Планы адаптации FigmaToCode под другие платформы | архивы `figma to code adapt to canva*.zip`, `C5-PIPELINE-ADAPTATION.zip`, папки `framer/`, `m1-miro-plan/`, `w1-webflow-plan.zip` | Средняя |
| `other/` | 6 | Маркетинг/исследование маркетплейса (не код) | `MARKETPLACE-ORGANIC-TRAFFIC-RESEARCH.md` (40 KB), `XIIGEN-PER-PLATFORM-PLUGIN-PLANS-v2.md` (48 KB), `XIIGEN-20-PLUGIN-TRAFFIC-STRATEGY.md`, `MARKETPLACE-NEW-PLATFORM-DISCOVERIES.md`, `MARKETPLACE-RESEARCH-ADDENDUM-v2.md` | Средняя (стратегия распространения) |
| `skill-selection-machine-runs/` | 0 | Пустая (зарезервирована под прогоны рантайма). | Нулевая сейчас |

Где именно лежат 1617 файлов cursor-репо (важно — «настоящего кода» там мало): `reports/` = 628 файлов (JSON-отчёты CU-*/HOST-08*), `phase40-cursor-approved-host-runtime-run*`/`rerun*` папки ≈ 4×125+3×26 = evidence-прогоны рантайма, `test/` (host-qa, phase10, smoke) = 13, `scripts/` = 3 (`debug-trace-helper.js`, `local-runtime-helper.js`, `install-smoke.ps1`), а собственно расширение = `src/extension.ts` (1) + `dist/extension.js` (1). Вывод: cursor-репо на 99% состоит из evidence/отчётов прошлых прогонов, а не из кода.

### C. Контракт thin-adapter границы (из `cursor-xiigen-smart-skills/xiigen-extension-contract.json`) — ЗАГРУЖАЕМАЯ ОПОРА для continue-плана

Схема `xiigen.extension_host_contract.v1`. Точно фиксирует границу «тонкий хост ↔ ядро llm_mvp_core», совпадает с правилами R5/R6 из CLAUDE.md:

- `thin_client: true`, `uses_xiigen_exe_api: true`, `core_logic_in_llm_mvp_core: true`, `host_training_logic: false`, `external_ai_calls: false`, `deterministic_shortcut: false`.
- Интерфейс к ядру: `xiigen exe` → скрипт `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`, команда `powershell -NoProfile -ExecutionPolicy Bypass -File tools/xiigen-extension-core-api.ps1 -RequestPath <request.json> -ResponsePath <response.json> -TracePath <trace.json>`.
- Контракты запроса/ответа: `XiigenExtensionRequest` (поля `Method`, `UserPromptOrTaskText`, `DebugEnabled`, `CorrelationId`, `Provenance`) / `XiigenExtensionResponse`.
- Debug-трейс: настройка `xiigenSmartSkills.debug` (default OFF), при ON показывает в чате `xiigen command:\ninput:<redacted>\noutput:<redacted>`; `raw_private_content_exposed:false`, `secrets_exposed:false`.
- ЗАПРЕЩЕНО хосту: реализация `PromptToTaskMachine`, `OrchestrationReasoningMachine`, детерминированный роутинг, внешние AI-вызовы, обучение, владение чекпоинтом/моделью, копирование сырого приватного корпуса, скрытая бизнес-логика.
- РАЗРЕШЕНО хосту: собрать факты хоста, послать redacted-запрос в `xiigen exe`, получить ответ, показать текст, нести флаг debugEnabled.
- Самопризнание контракта (важно для honesty-гейта): «static package/boundary evidence only… does NOT prove interactive Cursor runtime behavior… does NOT implement DebugTrace/PromptToTaskMachine/OrchestrationReasoningMachine/training/model asset lifecycle». То есть на момент фиксации `runtime_host_proof_performed:false`, `host_prompt_to_task_implementation:false`, `host_orchestration_reasoning_implementation:false`.

### D. Что проектирует план SkillSelectionMachine Extension (ч.1: §0–§6C, строки 1–1727)

Общая идея (§0 Purpose). Цель расширения — НЕ только экономия токенов. Расширение должно грузить ПРАВИЛЬНЫЕ скиллы для AI-задачи через ОБУЧАЕМЫЙ слой принятия решения. Старый token-optimizer оставлен как база хук-упаковки, но его детерминированный Hook 2 отменён. В КАЖДОМ семантическом пути (роутинг, понимание промпта, тип задачи, генерация кандидатов-скиллов, выбор скиллов, построение контекста, детект пробела результата, валидация вывода, промежуточные проверки) ЗАПРЕЩЕНЫ BM25, AstRelevanceRanker, Jaccard, regex, фиксированные таблицы задач, `if`-роутинг, статические роутеры промптов, детерминированные валидаторы. Детерминизм допустим ТОЛЬКО для механики: JSON parse/serialize, наличие файла, разбор пути, сравнение hash/версии, коды выхода процесса, проверка формы схемы, точное I/O. Авторитетное решение: `SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>`; продуктовая тренировочная поверхность `IMachineTraining<TDomain> : ILearningMachine<TDomain>`.

Источники плана (§1). Консолидирует 4 группы: (1) `skills loading as an ai extension` (таксономия скиллов, разделение универсальное/проектное), (2) `extensions reasearch` (эта папка — хост-хуки, упаковка, порты Claude/Codex/Cursor), (3) `orchestrator extension` (обучение оркестрации, «проблема статического мозга», форма потока роутинга), (4) `documentation learning` (дисциплина 50-парных батчей, DPO-гейты, held-out метрики) + текущий тред разговора.

Источники-промпты пользователя UP-001..UP-009 (§1A) — доказательства в самом плане (не в отдельном JSON). Ключевые требования Любы:
- UP-001: сделать опись скиллов, разделить универсальные (архитектор, planning, plan review, product decisions, арбитры) и уникальные проекту (код, деление на проекты).
- UP-002: разделить то, что грузится С СЕРВЕРА (обучаемая библиотека скиллов) и то, что сохраняет ПОЛЬЗОВАТЕЛЬ (вики/предпочтения на уровне проекта); серверная библиотека может апгрейдиться.
- UP-003: для решения «какие скиллы поднимать» ОБЯЗАТЕЛЬНО использовать `IMachineTraining`; начинаем со слоя правильных скиллов при постановке задачи.
- UP-004: первый продукт — AI-host расширение, грузит скиллы XIIGen дополнительно к файлам/скиллам пользователя, зависимо от задачи; если после первого промпта скиллы не вычленяются — восстановить после результата AI (второй проход).
- UP-006: все единицы обучаемы, никакого детерминизма ни для роутинга, ни промптов, ни видов задач, ни выбора скиллов, ни междуступенчатых проверок; сделать мысленный эксперимент.
- UP-008: цикл Plan→Review→fix→Review до нуля даже мелких находок; задачи исполнителя ≤15 минут; интегрировать с ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.
- UP-009: хост-пакеты в ОТДЕЛЬНЫХ репо `[cursor/claude/codex]-xiigen-smart-skills`.
- AAR-001..003 — резолюции ассистента: детерминированный `SkillSelector` отвергнут; введён трёхуровневый бандл-раздел (core project / host repos / user state); три conditional-pass находки закрыты (`IMachineTraining` как конкретный артефакт, per-domain training-схемы, in-plan source evidence).

Ledger требований R-001..R-020 (§2). Каждое R привязано к секции + Gate B + артефакту. Центр тяжести: R-004 (решение через machine training, не BM25/regex/if), R-006 (детерминированные семантические юниты запрещены даже в fallback/candidate-путях), R-015 (ВСЕ семантические юниты обучаемы), R-016 (`IMachineTraining<TDomain>` — конкретный адаптер над `ILearningMachine`), R-010/R-014 (серверная библиотека vs проектное состояние, независимый апгрейд), R-020 (три отдельных host-репо).

Связанные контракты в llm_mvp_core (§1A таблица): `ILearningMachine<TDomain>` (`00.Core/LearningMachineContracts.cs`), новый `IMachineTraining<TDomain>` (`00.Core/MachineTrainingContracts.cs`), `ILearningMachinePair<TPersonal,TGlobal>`, `OrchestrationPlanMachine` (`42.ProjectWikiDomain/OrchestrationPlanMachine.cs` — v20 держит `SkillLoadSessionInit` как passthrough), `ProjectEvidenceIndex` (`42.ProjectWikiDomain/ProjectEvidenceIndex.cs`), `IFeedbackRouter/FeedbackRouter` (`14.ContinuousLearning`), `SkillRetriever` (`22.AgentAlgorithms/Skills` — ТОЛЬКО legacy/offline-лейблинг, в рантайме запрещён).

Архитектурные решения A-001..A-011 (§3):
- A-001/A-001A: финальный слой — `SkillSelectionMachine`; обязателен адаптер `IMachineTraining<TDomain>` + `MachineTrainingAdapter<TDomain>` (делегирует внутренней машине, без семантической логики). 11 семантических адаптеров (PromptUnderstanding/TaskUnderstanding/TaskType/Routing/SkillCandidate/SkillSelection/PromptContext/ResultGap/OutputValidation/TaskCompletion/IntermediateCheck).
- A-002: `ILogicalUnit`/`ILogicalFlow`/hook-runner-exe НЕ имеют train-методов (только исполняют обученную машину или пишут learning-сигналы).
- A-003: нет детерминированных семантических юнитов (полный список запретов + разрешённая механика).
- A-004: обязателен ВТОРОЙ ПРОХОД — после ответа AI `ResultGapMachine` даёт `result-gap-report.json`; по умолчанию патч, полный redo если пропущенный скилл меняет архитектурную роль/гейт планирования/безопасность/границу реализации/стратегию верификации/порядок workflow.
- A-005: первый продуктовый хост — Claude Code hooks; первый ИЗМЕРИТЕЛЬНЫЙ хост — Codex (проще инспектировать JSON/session-метрики).
- A-006: граница с v20 — не менять Phase 37/38; `SkillSelectionMachine` — отдельная доменная машина, не ветка в `OrchestrationPlanMachine.GeneratePlanAsync`; запрет реинтродукции `PromptFlowIndex/_flowIndex/FlowIndex/KnownFlowRetrieval/RetrievalSource/privateCorpusDir`.
- A-007/A-007A: серверная библиотека скиллов (`server://xiigen/skill-library/{version}`, `C:\Projects\llm_mvp_core\skills\server-library`) vs проектное состояние `{user_project_root}\.xiigen\{wiki-domain,skill-preferences,skill-feedback,skill-adaptation-profile.json,skill-library-lock.json}`; апгрейд сервера НИКОГДА не перезаписывает проектное состояние; при неоднозначной миграции — предложить patch, не переписывать молча. `SkillSelectionMachinePair : ILearningMachinePair<Personal,Global>`.
- A-009: 11 отдельных обучаемых машин (перечислены), каждая через свой `IMachineTraining<TDomain>` адаптер; нельзя схлопнуть в lookup/regex.
- A-010: цикл ревью план-суб-агентами (REQUIREMENTS/V20_INTEGRATION/TRAINABLE_SEMANTIC_UNITS/WORK_ORDER_SIZE) до blocking=0 и minor=0; work order ≤15 мин, один узкий артефакт, без целой фазы/нескольких гейтов/commit+push/совмещения Phase37+38; фиксированный формат evidence-пакета.
- A-011: три host-репо (claude/codex/cursor), каждый владеет ТОЛЬКО упаковкой/адаптером/копиями exe/локатором; все семантические решения — в llm_mvp_core.

Рантайм-поток (§4). SessionStart → грузит серверную библиотеку по версии/локатору, инвентарь скиллов пользователя, проектный wiki-индекс, проектные предпочтения, персональный adaptation-профиль. UserPromptSubmit → цепочка машин PromptUnderstanding→TaskUnderstanding→TaskType→Routing→SkillCandidate→SkillSelection→PromptContext→IntermediateCheck. После ответа AI (Stop) → ResultGap→OutputValidation→TaskCompletion→IntermediateCheck→`DpoSignalWriter` пишет learning-строку→feedback-анализатор→решение retry/patch/accept. Hook-trace ДОЛЖЕН доказывать, что ни один семантический шаг не был детерминированным fallback/retrieval/regex/фиксированным роутингом.

Граница ядро/хост/пользователь (§4A). Ядро `llm_mvp_core` владеет: `SkillSelectionDomain`, `SkillSelectionMachine`, `IMachineTraining` + адаптеры, `SkillSelectionMachinePair`, `SkillSelectionOrchestrator`, DTO, серверная библиотека, реестр/conflict-policy/манифесты, контракты загрузки проектного состояния, DPO-валидация/тренировка/held-out/export/import/locator/fresh-load/continue-training, hook-runner-exe. Core-exe: 11 `*-machine-runner.exe`, общаются ТОЛЬКО через JSON stdin/stdout + коды выхода, БЕЗ зависимостей от Claude/Codex/Cursor/VSCode API. Хост-репо владеют только упаковкой (`.claude-plugin/plugin.json`+`hooks/hooks.json` / `.codex` manifest / Cursor extension manifest, `bin\*.exe`, smoke-промпты, README). §4A.3 — 18 JSON-boundary-контрактов (кто производит/потребляет каждый *.json). §4A.4 — последовательность build/package.

Компоненты (§5). 5.1 `SkillSelectionDomain` (marker). 5.2 `SkillSelectionMachine` (train/eval/export/import). 5.3 `SkillSelectionOrchestrator` — принимает `SkillSelectionRequest` (полная JSON-схема: prompt_id, prompt, host, решения всех предшествующих машин, user/xiigen/platform skills, wiki-evidence, feedback-profile, previous_ai_output, selection_mode=first_prompt|after_output_gap|user_correction, context_budget, authority_stack) → `SkillSelectionDecision` (selected/rejected/missing/wrong skills, preserved user skills, conflicts_resolved, wiki_evidence_used, personal_machine_used, skill_patch_proposals, confidence, requires_patch/redo, `machine_decision:true`, `deterministic_semantic_source:false`) → сериализуется в `skill-selection-decision.json` (авторитетный артефакт R-002). 5.4 обучаемые pre-selection машины. 5.5 `PromptContextMachine` держит порядок авторитета: Luba direct → project instructions → user/project skills → XIIGen project skills → XIIGen universal skills → platform adapter skills. 5.6 обучаемые post-output машины (детект пропущенных planning/architecture/plan-review/product-decision/frontend-verification/test-integrity/CI/doc-скиллов, wrong host adapter, overload). 5.7 `DpoSignalWriter`. 5.8 `SkillSelectionTrainingRunner` (`42.ProjectWikiDomain/SkillSelectionTrainingRunner.csproj` — покрывает все 11 доменов). 5.9 Contradiction Sweep Tool (`tools/skill-selection-contradiction-sweep.ps1`). 5.10 Codex Trace Gate (`tools/skill-selection-codex-trace-gate.ps1`; отмечено: `codex.exe --help` дал `Access is denied`, поэтому гейт валидирует JSON-трейсы, а не вызывает CLI). 5.11 полный список обучаемых интерфейсов + `IMachineTraining<*>`-training-интерфейсы + `IDpoSignalWriter`/`ISkillAdaptationWikiIndexer`/`ISkillFeedbackRecorder`/`ISkillPatchProposalWriter`, правило OperationResult<T>. 5.12 `ServerSkillLibraryRegistry`, `SkillAdaptationWikiIndexer` (обёртка над `ProjectEvidenceIndex`), `SkillSelectionMachinePair`, `SkillFeedbackRecorder`, `SkillFeedbackAnalyzer`, `SkillPatchProposalWriter`.

Реестр скиллов (§6) + серверная библиотека vs проектное (§6A/6C). Поля реестра: skill_id, source_kind, source_of_truth, server_library_version, origin_server_library_path, origin_sha256, authority_level, load_mode, skill_patch_policy, host_support. Conflict-policy: server library = source of truth, extension copy read-only, higher_authority_wins, user_project_skill > server_base, direct_instruction > all skills, server_base может suggest не override. §6C.1 универсальные классы скиллов (Architect, Planning, Plan review, Product decisions, Authority/source preservation, Learning/DPO authoring, Execution governance, Feedback loop closure) — с типовыми арбитрами. §6C.2 проектно-специфичное (контракты/пути llm_mvp_core, host-упаковка, two-project boundary, Gate B пороги, v20-ограничения).

### E. Связь с ядром: план УЖЕ реализован (проверено чтением, read-only)

Проверка `C:\Projects\llm_mvp_core\43.SmartSkillsDomain`: директория СУЩЕСТВУЕТ, 36 `.cs`-файлов. Ключевые реализованные машины: `PromptToTaskMachine.cs` (+ Contracts/LogicalHierarchy/TaskPatternUnit/AblationHarness), `OrchestrationReasoningMachine.cs` (+ Contracts/LogicalHierarchy/ScenarioUnit/Trainer), `Phase40ExtensionContracts.cs`, `Phase08CommonUserTwoLegLifecycle.cs`, `ExtensionEvalOnlyQualityRunner*.cs`, `ExtensionQualityScoring.cs`, `Quality03*ProofProducer/ProofContractRunner*.cs`.

ДЕЛЬТА план↔реальность (важно для continue-плана): план называл финальную машину `SkillSelectionMachine`/`SkillSelectionDomain`, а фактически реализовано как `PromptToTaskMachine` (какая задача/скиллы из промпта) + `OrchestrationReasoningMachine` (рассуждение оркестрации). Т.е. 11-машинная схема плана сконденсирована в 2 машины домена 43. Это стыкуется с находкой RAG-10 (43.SmartSkillsDomain как PARTIAL) и с host-контрактом (§C), где запрещённые хосту responsibilities — именно `PromptToTaskMachine` и `OrchestrationReasoningMachine`. Три host-репо (§B) содержат `phase40-*` трейсы, ссылающиеся на `Phase40ExtensionContracts.cs`. Полная сверка «11 машин плана → 2 машины реализации» — задача для последующих проходов (нужно прочитать вторую половину плана: §7–§18, гейты B-1..B-15, фазы 0–12, mental experiment).

---

## Предлагаемое распределение проходов 2–10 (для родителя-верификатора)

- Pass 2/10: `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` ч.2 (строки 1729–3252): §7 Skill Context Packet, §8 DPO Training (batch/pair shape/pair types), §9 ResultGapMachine, §10 Contradiction Sweep, §11 Host Plan (Claude/Codex/Cursor), §12 Gate B-1..B-15, §13 Phases 0–12, §13A v20-интеграция/bounded work orders, §14 FC Matrix, §15 Completion, §16 Verdict, §17 v20 Conflict Check, §18 Mental Experiment (9 сценариев).
- Pass 3/10: `XIIGEN-MARKETPLACE-MASTER.md` + `XIIGEN-MARKETPLACE-MASTER-PLAN.md` (стратегия/структура маркетплейса 20+ платформ) + `CONSOLIDATION-INDEX.json`/`CONFLICT-MAP.md`.
- Pass 4/10: token-optimizer семейство — `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v1-CLAUDE.md` + CODEX-PORT + CURSOR-PORT + LICENSING (историческая база хук-упаковки, что устарело, что переиспользуемо).
- Pass 5/10: local-testing — `PLAN-LOCAL-TESTING-{CLAUDE-CODE,CODEX,CURSOR}-v1.md` (dual-path Path B, что уже проверялось локально) + `XIIGen-Desktop-Plugin-Adaptation-Report-v2.md`.
- Pass 6/10: `cursor-xiigen-smart-skills` — глубокое чтение `src/extension.ts` (1628 строк) + `docs/PROJECT-DOC.md` + `package.json` + evidence в `reports/` (CU-*/HOST-08*) как реальная thin-adapter реализация VSCode/Cursor.
- Pass 7/10: `claude-xiigen-smart-skills` + `codex-xiigen-smart-skills` — `src/*-entrypoint.ps1` (по 246 строк), `.claude-plugin/`+`hooks/`, `.codex/`, phase40-*-core-request/response/trace JSON, smoke-скрипты.
- Pass 8/10: `FigmaToCode-...` ч.1 — `src/plugin/core/{controller,op-executor}.ts`, `src/plugin/models/*`, `manifest.json`, `CLAUDE.md`/`AGENTS.md`, design→code движок.
- Pass 9/10: `FigmaToCode-...` ч.2 — `src/ui/components/design-gen/` (провайдеры Claude/OpenAI, skill-advisor SkillAdvisor/SkillJudge/skill-pipeline/registry/MemoryStore), `ai-enhance/{AIService,SkillInjector,PromptTemplates}` + `Plans/` (framer/miro/webflow/canva адаптации).
- Pass 10/10: `other/` marketplace-research + СИНТЕЗ: как «extensions reasearch» питает continue-план VS/VSCode; сверка плана↔`43.SmartSkillsDomain` реализации; открытые пробелы и рекомендации.

---

## Метаданные прохода

- RAG-файл: `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code continue/RAG-11-extensions-research.md` (СОЗДАН этим проходом).
- Прочитано в Pass 1: `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` строки 1–1727 (полная первая половина, deep); структурная опись всех директорий; `xiigen-extension-contract.json` полностью; read-only сверка `43.SmartSkillsDomain` (36 .cs).
- НЕ прочитано (для следующих проходов): вторая половина главного плана (1728–3252), все прочие top-level планы, тела трёх host-репо, весь FigmaToCode.

---

## Pass 2/10 — PLAN-SKILL-SELECTION-MACHINE ч.2: DPO-training + Gate B-1..15 + Phases 0-12 + v20-интеграция

Прочитано deep этим проходом: `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` строки 1729–3252 (полная вторая половина: §7 Skill Context Packet, §8 DPO Training, §9 ResultGapMachine, §10 Contradiction Sweep, §11 Host Plan, §12 Gate B-1..B-15, §13 Phases 0–12, §13A v20-интеграция, §14 FC Matrix, §15 Completion, §16 Verdict, §17 v20 Conflict, §18 Mental Experiment). Это ПОЛНЫЙ обучаемый build-план (амбиция) — сверить с тем, что реально построено (RAG-10 + §E Pass 1: 11 машин плана сконденсированы в `PromptToTaskMachine` + `OrchestrationReasoningMachine` домена 43).

### F. §7 Skill Context Packet — авторитетный артефакт инъекции

Три файла: `skill-selection-decision.json` (сырое решение машины), `SkillContextPacket.json` + `SkillContextPacket.md` (сжатый материал, инъецируемый в AI-хост). Обязательные поля пакета: `prompt_id`, `host` (claude-code|codex|cursor), ссылки на решения всех пяти pre-машин (`prompt_understanding_decision_ref`, `task_understanding_decision_ref`, `task_type_decision_ref`, `routing_decision_ref`, `skill_candidate_decision_ref`), `selected_skills`, `rejected_skills`, `user_project_skills_preserved`, `xiigen_skills_loaded`, `conflicts_resolved`, `wiki_evidence_used`, `feedback_adaptation_applied`, `personal_profile_id`, `skill_patch_proposals`, `authority_notes`, `context_budget_tokens`, `machine_decision:true`, `deterministic_semantic_source:false`. **Пакет НЕВАЛИДЕН, если `machine_decision=false` ИЛИ `deterministic_semantic_source=true`** — жёсткий honesty-инвариант против детерминированного шортката.

### G. §8 DPO-тренировка SkillSelection — центр обучаемости

**§8.1 Дисциплина батча.** Каждый принятый батч семантического юнита = РОВНО 50 тренировочных пар + 50 held-out строк. Батч-001 обязан покрыть ВСЕ 11 обучаемых доменов (PromptUnderstanding/TaskUnderstanding/TaskType/Routing/SkillCandidate/SkillSelection/PromptContext/ResultGap/OutputValidation/TaskCompletion/IntermediateCheck). «Подготовленные pairset ≠ принятые батчи». Состояния: `PAIRSET_READY_TRAINING_PENDING` → `TRAINED_EVALUATION_PENDING` → `ACCEPT_BATCH` | `REPAIR_SAME_BATCH`.

**§8.2 Правило рантайм-приёмки.** Батч нельзя принять, пока не существует и не гоняет held-out поверхность `SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>`. Для продуктовой готовности КАЖДАЯ из 11 машин A-009 должна гонять held-out с фактическими метриками. Skill selection может быть первым проверяемым доменом, но НИ ОДИН host-пакет не проходит, пока все 11 доменов не имеют training+evaluation coverage.

**§8.2A Партиция строк по домену.** Каждая train/heldout строка обязана декларировать 6 полей партиции: `semantic_domain`, `semantic_unit_id`, `target_machine`, `target_training_adapter`, `row_split` (train|heldout), `row_kind` (semantic_unit_dpo | aggregate_flow_dpo | user_feedback_dpo | result_gap_dpo). Правила: первые 4 поля должны СОГЛАСОВАТЬСЯ ТОЧНО; каждый из 11 доменов — ровно 50 train + 50 heldout; aggregate-flow строки НЕ засчитываются домену без полей партиции; строки без домена-цели невалидны после Phase 4.

**§8.3 Форма DPO-пары.** Строка (`pair_id` вида `SS-001-0001`) несёт: 6 полей партиции + `prompt` + решения всех pre-машин + `user_project_skills`/`xiigen_skills`/`platform_adapter_skills` + `project_wiki_evidence` + `feedback_signals` + `personal_profile_id` + `skill_context_packet` + **`chosen`** (selected_skills, wiki_evidence_used, feedback_adaptation, reason, authority_preservation) + **`rejected`** (selected_skills, `wrong_reason` формата `[WRONG: конкретное последствие пропущенного/неверного скилла]`) + `expected_metrics` (must_include, must_exclude, must_cite_wiki_evidence, must_apply_feedback_profile). **Rejected НЕ имеет права на generic-метки типа `[WRONG: bad]`**. **Правило утечки промпта:** train- и heldout-промпты дизъюнктны после нормализации; точное совпадение → гейт ПРОВАЛИВАЕТСЯ; точные совпадения нельзя молча исключать и отчитываться `0.0`.

**§8.4 Типы пар (19 штук, SS-A..SS-S)** — что учат: SS-A first-prompt понимание→бандл; SS-B сохранение user/project скиллов; SS-C резолюция конфликта project↔XIIGen; SS-D second-pass восстановление пропущенного скилла после ответа AI; SS-E выбор platform adapter/routing; SS-F минимальный контекст (не перегружать нерелевантным); SS-G no-product-decision + границы architect/executor; SS-H review-находка→бандл скиллов; SS-I wiki/code-evidence→адаптация; SS-J user feedback→обновление персональной модели/patch; SS-K..SS-S — коррекции по каждой машине (prompt understanding, task understanding, task-type/subphase, routing, skill candidate, prompt/context, output validation, task completion, intermediate check).

### H. §9 ResultGapMachine и post-output обучаемые проверки

4 обучаемые машины: `ResultGapMachine`, `OutputValidationMachine`, `TaskCompletionMachine`, `IntermediateCheckMachine` (все `: ILearningMachine<*Domain>`). Вход: `{original_prompt, selected_skill_packet, ai_output, task_state}`. Выход: `{valid, missing_skills[], wrong_skills[], patch_required, full_redo_required, injection_for_next_prompt, dpo_signal}`. Классы сигнала второго прохода (label→missing-skill class): нет requirements-ledger→planning; нет Gate B→plan-review; реализация до plan-гейта→architect/executor governance; ответ делает продуктовое решение→no-product-decisions; фронт изменён без browser-проверки→browser/frontend verification; тесты поверхностны/только success-флаг→test-integrity; review без severity/file refs→code-review/plan-review; вывод игнорит user project skill→user-project preservation. **Ключ: это ЛЕЙБЛЫ для обучения/оценки, НЕ детерминированные рантайм-правила — рантайм не смеет детектить их keyword-матчингом, regex, фиксированными чеклистами, статическими картами задач или `if`-цепочками.**

### I. §10 Contradiction Sweep — 6 доказательств (`CONTRADICTION-SWEEP.md`)

(1) Ни один продуктовый семантический путь не использует BM25, AstRelevanceRanker, Jaccard, regex, keyword-maps, статические task-maps, `if`-routing, фиксированные prompt-роутеры, legacy-retrievers, детерминированные валидаторы/guards/inspectors. (2) Ни один рантайм fallback/candidate путь не обходит обучаемые машины. (3) `SkillSelector`, `SkillRetriever`, `CandidateSkillRetriever`, `TaskIntentBoundaryClassifier`, `OutputValidator`, `TaskCompletionGuard`, `ResultGapInspector` ОТСУТСТВУЮТ в продуктовых hook/extension путях (кроме явных legacy-migration заметок). (4) Каждый семантический шаг трейса называет `ILearningMachine<TDomain>` и пишет `machine_decision=true`. (5) Каждый шаг пишет `deterministic_semantic_source=false`. (6) Second-pass умеет создавать DPO-строки через обучаемые post-output машины.

### J. §11 Host Plan

- **11.1 Claude Code = продуктовый хост** (`claude-xiigen-smart-skills`): корректированная структура token-optimizer хука; `.claude-plugin/plugin.json`, `hooks/hooks.json`, `bin/` c 11 `*-machine-runner.exe`.
- **11.2 Codex = ПЕРВЫЙ измерительный гейт** (`codex-xiigen-smart-skills`): обязан отчитывать selected skills, вывод skill-candidate машины, machine-decision трейс для КАЖДОГО семантического юнита, token/context budget, результат output-gap recovery, DPO row count, deterministic semantic source count.
- **11.3 Cursor = поздний хост** (`cursor-xiigen-smart-skills`): `beforeReadFile`-компрессия ПОСЛЕ того как learning-machine гейт работает; не должен становиться первой зависимостью.

### K. §12 Gate B-1..B-15 — литеральные условия приёмки (executor не может подменить слабее)

Канонический run-root: `...\extensions reasearch\skill-selection-machine-runs\batch-001`. Все JSONL-пути абсолютные (в директории есть пробелы).

- **B-1 Learning machine contract** — `dotnet test Tests.csproj --filter "IMachineTraining_AdapterExtendsILearningMachine|SkillSelectionMachine_ImplementsILearningMachine|SkillSelectionMachine_ExposesIMachineTrainingAdapter|SkillSelectionOrchestrator_AcceptsRequestAndReturnsDecision|SkillSelectionRequestDecision_SerializeRoundTrip"` → **5/5 passed, 0 failed**.
- **B-2 50-pair static gate** — runner `--mode validate-pairset` → `semantic_domain_count=11`, `train_rows_per_semantic_domain=50`, `heldout_rows_per_semantic_domain=50`, все `rows_missing_*=0`, `semantic_domain_target_mismatch=0`, `row_split_invalid=0`, `row_kind_invalid=0`, `invalid_pairs=0`, `generic_wrong_labels=0`, `missing_semantic_domain_pairsets=0`.
- **B-3 First-prompt evaluation** — runner `--mode evaluate` → `first_prompt_skill_selection_accuracy >= 0.85`, `top_k_skill_recall >= 0.90`, `bundle_precision >= 0.80`, `wrong_skill_overload_rate <= 0.10`, `exact_prompt_leakage_rate = 0.0`, `exact_prompt_overlap_fails_gate=true`, `metric_mode=actual`.
- **B-4 User/project skill preservation** — `--mode preservation-gate` → `user_skill_preservation_accuracy >= 0.95`, `conflict_trace_missing=0`.
- **B-5 Second-pass recovery** — `--mode result-gap-eval` → `after_output_recovery_accuracy >= 0.85`, `critical_full_redo_accuracy >= 0.90`, `patch_vs_redo_route_accuracy >= 0.85`.
- **B-6 Deterministic semantic path ban** — `skill-selection-contradiction-sweep.ps1` → все 12 `semantic_*_paths=0` (bm25/ast_relevance_ranker/jaccard/regex/keyword_map/static_task_map/if_routing/fixed_prompt_router/legacy_retriever/deterministic_validator/deterministic_guard/deterministic_inspector) + `runtime_fallback_semantic_paths=0` + `runtime_candidate_semantic_paths_not_machine=0` + `mechanical_io_determinism_only=true`.
- **B-7 Codex measured hook trace** — `skill-selection-codex-trace-gate.ps1 -SessionRoot $env:USERPROFILE\.codex\sessions` → `skill_selection_trace_present=true`, `machine_decision_present=true`, `all_semantic_machine_decisions_present=true`, `deterministic_semantic_source_count=0`, `result_gap_trace_present=true`, `token_usage_reported=true`, `codex_cli_invocation_required=false` (потому что `codex.exe` дал Access denied — гейт валидирует JSON-трейсы сессий).
- **B-8 Claude Code package smoke** — `claude --plugin-dir "...\claude-xiigen-smart-skills" --debug` → `hooks_loaded=true`, все 11 `*_machine_called=true`, `context_packet_injected=true`, `deterministic_semantic_source_count=0`.
- **B-9 SkillSelection model asset readiness** — export-model → export-training-data → import-model `--fixture-probe` → publish-model → load-model → continue-training `--max-cycles 1`. Условия: `manifest.json exists`, `model_id=skill-selection-v1`, `domain=SkillSelectionDomain`, `semantic_domains_covered=11`, checkpoint/`source_base_commit`/`export_input_sha256` non-empty, `training-manifest.json` + `corpus.jsonl.gz` exist, **нет абсолютных приватных путей в коммит-файлах**, fresh import без приватного корпуса, locator резолвит `skill-selection-v1`, загруженная модель выбирает fixture-бандл + гоняет fixture-решения ВСЕХ доменов, continue-training использует коммитнутый `corpus.jsonl.gz` (не приватный prompt-log), старые строки исключены, новые обучены.
- **B-10 Core/host repo boundary** — `skill-selection-core-extension-boundary-gate.ps1` → `core_exe_manifest_exists=true`, `core_exe_count=11`, `host_extension_repo_count=3`, claude/codex/cursor repo exist, **`shared_extension_repo_exists=false`**, каждое host-репо: `bin` содержит копии core-exe, **НЕТ training-runner, НЕТ приватного корпуса, `semantic_decision_code_paths=0`, `canonical_skill_paths=0`, `skill_registry_source_of_truth=false`**, `json_boundary_contracts_present=true`.
- **B-11 Server library + project-local wiki + feedback** — `skill-selection-core-adaptation-gate.ps1 -ProfileId luba` → `server_skill_library_exists=true`, `canonical_skill_paths_under_server_library=true`, `registry_source_of_truth_xiigen_server_library=true`, `extension_canonical_skill_paths=0`, wiki index loaded, `wiki_evidence_rows>0`, project-local preferences/adaptation-context exist, `decisions_with_required_wiki_evidence>=0.80`, `feedback_events_recorded>=1` (санитизированы), `feedback_to_dpo_rows>=1`, `skill_selection_machine_pair_contract=true`, personal-model manifest + `profile_id=luba`, **`raw_private_feedback_committed=false`, `personal_to_global_sync_contains_raw_private_content=false`**.
- **B-12 In-plan user prompt source coverage** — `plan-markdown-source-evidence-gate.ps1` → section exists, все authority-requirements/feedback/questions/assistant-answers имеют source-entries, `assistant_answer_resolution_entries_present+mapped=true`, `pending_plan_update_entries=0`, `unmapped_user_prompt_source_entries=0`.
- **B-13 Server upgrade не перезаписывает project-local** — `...upgrade-boundary-gate.ps1` → version-manifest + upgrade-report exist, `project_local_skill_library_lock_exists=true`, wiki/preferences/feedback/adaptation-profile PRESERVED, `ambiguous_skill_migrations_create_preference_patch=true`, **`server_upgrade_overwrites_project_local_state=false`**.
- **B-14 Trainable semantic unit coverage** — `...trainable-semantic-unit-gate.ps1` → все 11 `*Machine_implements_ILearningMachine=true`, `IMachineTraining_contract_exists=true`, `MachineTrainingAdapter_delegates_to_ILearningMachine=true`, все 11 `*Training_implements_IMachineTraining=true`, `semantic_machine_count=11`, `semantic_machine_training_adapter_count=11`, `semantic_machine_runner_count=11`, все домены имеют train+heldout+actual-metrics, все runner эмитят `machine_decision` и `deterministic_semantic_source=false`, `no_semantic_unit_has_deterministic_fallback=true`, `no_semantic_unit_uses_legacy_retriever_or_classifier=true`.
- **B-15 Plan approval loop + bounded work-order ledger** — `plan-work-order-governance-gate.ps1 -PlanPath ... -V20PlanPath ...` → `review_rounds_completed>=1`, ВСЕ 4 категории ревью (requirements/v20_integration/trainable_semantic_unit/work_order_size) blocking=0 & minor=0, `unmapped_user_prompt_source_entries=0`, `unmapped_authority_requirements=0`, `v20_dependency_map_complete=true`, `phase37/phase38_dependency_marked_external=true`, `skill_selection_plan_does_not_modify_v20_phase37_or_phase38=true`, `host_extension_repo_count=3`, три `*_repo_required=true`, `shared_..._repo_allowed=false`, `work_order_ledger_exists=true`, **`all_work_orders_expected_minutes<=15`**, `work_orders_with_entire_phase_scope=0`, `..._multiple_gate_commands=0`, `..._commit_or_push_inside_implementation_slice=0`, `..._combining_phase37_phase38_and_skill_selection=0`, `runtime_explicit_authorization_policy_added=false`.

### L. §13 Фазы реализации 0–12 (что строится когда, и какой гейт закрывает)

- **Phase 0** План+authority repair → plan review FC-0..FC-12 pass.
- **Phase 1** Server skill library registry + conflict-policy (`skills/server-library/**/SKILL.md`, `skills-registry.json`, `conflict-policy.json`, `server-skill-library-manifest.json`) → у каждого скилла есть source_kind/source_of_truth/origin/version/sha256/authority/trigger/host-support; ни один canonical XIIGen скилл НЕ из host-репо и НЕ из user project-local.
- **Phase 2A** Project-local wiki+preferences+feedback substrate (`ISkillAdaptationWikiIndexer`/impl, `ISkillFeedbackRecorder`/impl, `SkillFeedbackAnalyzer`, `SkillPatchProposalWriter`, `{user_project}\.xiigen\{wiki-domain,skill-preferences,skill-feedback}`) → wiki через `ProjectEvidenceIndex` как память (не роутинг), feedback→DPO-сигналы, нет сырого приватного feedback в коммитах, **Gate B-12 pass**.
- **Phase 2** Trainable semantic unit contracts (`IMachineTraining<TDomain>`, `MachineTrainingAdapter<TDomain>`, 10 доменов + 10 машин [SkillSelection добавляется в Phase 3], 11 training-адаптеров, `TrainableSemanticUnitLedger`) → **Gate B-14 pass**, нет детерминированного fallback.
- **Phase 3** SkillSelectionMachine contracts (5 Decision-типов, `SkillSelectionDomain/PersonalDomain/GlobalDomain`, `SkillSelectionMachine`, `SkillSelectionMachinePair`, `SkillSelectionOrchestrator`, `SkillSelectionRequest/Decision`, 5 контракт-тестов) → **Gate B-1 + B-14 pass**.
- **Phase 4** DPO corpus + runner (`TRAIN-PAIRS.jsonl`, `HELDOUT-ROWS.jsonl`, per-domain pairsets ×11, `semantic-domain-pair-schema.json`, `PAIR-VALIDATION-LEDGER.jsonl`, `TRAINING-METRICS.json`, `IDpoSignalWriter`/impl, `SkillSelectionTrainingRunner.csproj`, `skill-selection-contradiction-sweep.ps1`) → **Gate B-2, B-3, B-6, B-14 pass с `metric_mode=actual`**.
- **Phase 5** First-prompt hook integration (7 pre-runner-exe + `skill-selection-decision.json` + `SkillContextPacket.json` + hook-trace) → каждое first-prompt решение из обучаемой машины, `deterministic_semantic_source_count=0`.
- **Phase 6** Result-gap second pass (4 post-runner-exe + second-pass write-path DpoSignalWriter + `result-gap-report.json` + `RESULT-GAP-HELDOUT.jsonl`) → **Gate B-5, B-6, B-14 pass**.
- **Phase 7** SkillSelection model asset readiness (`models/skill-selection/skill-selection-v1/{manifest,checkpoint,runner-report}.json`, `training-data/.../{training-manifest.json,corpus.jsonl.gz}`, `models/index.json`) → **Gate B-9 pass**.
- **Phase 7A** Project-local personal adaptation + preference repair loop (`SkillSelectionMachinePair : ILearningMachinePair<Personal,Global>`, `models/skill-selection-personal/luba/manifest.json`, `.xiigen/skill-adaptation-profile.json`, feedback-dpo-rows, patch-proposals, `skill-selection-core-adaptation-gate.ps1`) → **Gate B-11 pass**.
- **Phase 7B** Server library upgrade boundary (version-manifest, upgrade-report, `.xiigen/skill-library-lock.json`, migration/preference patch при смене skill-id) → **Gate B-13 pass**.
- **Phase 8** Core executable packaging (`core-exe-manifest.json` + 11 exe в `40.HookRunners\SkillSelection\`) → манифест перечисляет 11 runner-exe, все существуют.
- **Phase 9** Claude host repo packaging (`.claude-plugin\plugin.json`, `hooks\hooks.json`, `bin\` ×11 exe, `extension-package-manifest.json`, README) → **Gate B-10 + B-8 pass**.
- **Phase 10** Codex host repo packaging + measured verification (Codex manifest/adapter, копии exe, JSON-trace, token/context метрики, skill-decision report, `skill-selection-codex-trace-gate.ps1`, `codex-smoke-prompts.jsonl`) → **Gate B-7 + B-10 pass**.
- **Phase 11** Cursor host repo packaging (Cursor manifest/package, копии exe, smoke-prompts, README) → **Gate B-10 pass**, Cursor не обходит ни одной обучаемой машины.
- **Phase 12** Cross-host conformance (host-conformance report по трём репо + platform notes) → ни один порт не обходит обучаемую машину; ни одно host-репо не содержит training/semantic-decision/canonical-skill/private-corpus/project-local-user-state.

### M. §13A v20-интеграция + bounded work orders ≤15 мин (architect-owned ledger)

**Последовательность исполнения:** (1) v20 Phase 37 владеет `OrchestrationPlanMachine`, `PromptFlowGroundingModel`, orchestration-model export/import/locator + Gate B.0–B.7. (2) v20 Phase 38 владеет `TranslationMachine` asset readiness + translation-гейты. (3) Этот план может гонять plan-review, corpus-design, contract-prep ПАРАЛЛЕЛЬНО, только если слайсы НЕ редактируют файлы Phase 37/38. (4) SkillSelection model asset execution стартует ТОЛЬКО после того, как v20 model-lifecycle pattern доступен как reference-evidence. (5) Ни один work order не смеет заявить, что вся model-readiness работа завершена, пока Phase 37 ИЛИ 38 открыты.

Таблицы work-order (все ≤15 мин, один слайс): **13A.1** plan-review (WO-PLAN-01 requirements coverage, WO-PLAN-02 v20 integration, WO-PLAN-03 trainable-unit, WO-PLAN-04 work-order-size, WO-PLAN-05 architect repair первой ненулевой группы; цикл ROUND-02/03… до blocking=minor=0). **13A.2** v20 Phase 37 deps (V20-P37-WO-01..07: audit/corpus schema, remove stale retrieval contracts, PromptFlowGroundingModel learned state, remove runtime FlowReferenceDetector/index, checkpoint save/load 6 секций, CLI asset command, один Gate B repair). **13A.3** v20 Phase 38 deps (V20-P38-WO-01..04: TranslationModelAsset test slice, export/import repair, registry publish/load, continue-training exclusion). **13A.4** skill-selection executor (SS-WO-01..12C: registry schema, IMachineTraining adapter contracts, один domain-контракт, DTO+serialize, pair-schema validation, один domain 50-pair generator, один first-prompt runner, один result-gap runner, manifest/export skeleton, import/fresh-load probe, feedback recorder, упаковка одного runner в claude/codex/cursor bin). Если строка >15 мин на практике — executor возвращает пакет, architect дробит строку ДО следующей делегации.

### N. §14 FC Matrix / §15 Completion (29 пунктов) / §16 Verdict / §17 v20 Conflict

**§14 FC Matrix.** Прошлые провалы починены: FC-0 (requirements traceability), FC-3 (phantom components), FC-5 PARTIAL, FC-7 (phase placement), FC-8 (self-containment), FC-9 (literal Gate B), FC-10 (type contract), FC-11 PARTIAL, FC-12 (principles). Плюс NEW-строки: v20 compatibility, core/host boundary, server-library+personalization, in-plan source preservation, no-deterministic-semantic-units, конкретный `IMachineTraining` артефакт, per-domain training schema, assistant-answer source evidence, multi-agent zero-findings loop, v20 bounded work-order integration, separate extension repos.

**§15 Completion Definition — 29 пунктов** (план завершён ТОЛЬКО когда): 1 `SkillSelectionMachine:ILearningMachine`; 2 `SkillSelectionMachinePair:ILearningMachinePair`; 3 все 11 машин A-009; 4 `IMachineTraining<TDomain>` + 11 адаптеров; 5 ≥1 батч 50-пар + 50-heldout на КАЖДЫЙ из 11 доменов; 6 actual-метрики на каждый домен; 7 валидные 6 полей партиции на каждую строку; 8 first-prompt пороги; 9 second-pass пороги; 10 user/project preservation пороги; 11 sweep zero deterministic; 12 XIIGen скиллы из server-library, user-состояние в `.xiigen`; 13 контекст-зависимые решения цитируют wiki из `ProjectEvidenceIndex`; 14 feedback записан/санитизирован/анализирован/→DPO/→personal train или patch; 15 B-11; 16 Codex trace; 17 Claude smoke; 18 export/import checkpoint; 19 полный bundle-lifecycle (manifest/checkpoint/runner-report/training-manifest/corpus/locator/fresh-load/marketplace-load/continue-training); 20 personal bundle без сырого приватного feedback; 21 11 core runner-exe + manifest; 22 три host-репо содержат ТОЛЬКО упаковку; 23 все plan-shaping prompts/feedback/questions/answers в §1A + B-12 zero unmapped; 24 upgrade B-13 не перезаписывает; 25 B-15 zero findings; 26 work-order ledger ≤15 мин; 27 ни один WO не совмещает phase/gates/commit+push/Phase37+38; 28 v20 dependency map соблюдён; 29 нет shared-репо. **«Ни один отчёт не смеет заявить completion из registry presence, static validation или mechanical I/O evidence».**

**§16 Verdict.** Цель изменена с `token optimizer with deterministic SkillSelector` → `skill-loading extension with trained semantic machines and SkillSelectionMachine`. Прошёл архитектурное ревью trainable-архитектуры, но НЕ executor-approved до Gate B-15. Текущий вердикт: **`ARCHITECTURE_REPAIR_REQUIRED_FOR_V20_WORK_ORDER_INTEGRATION`**. Оставшаяся работа bounded: гонять независимые sub-agent review rounds против §13A и B-15, чинить все blocking+minor, затем выдавать по одному ≤15-мин work order. Вердикт НЕ авторизует parent-архитектора исполнять продуктовый/тестовый код.

**§17 v20 Conflict Check.** Verdict: **`NO KNOWN CONFLICT AFTER THIS PLAN REPAIR`**. Матрица совместимости: `SkillLoadSessionInit` passthrough сохранён (A-006); no deterministic production grounding; запрет `PromptFlowIndex/_flowIndex/FlowIndex/KnownFlowRetrieval/RetrievalSource/privateCorpusDir` в новых assets; приватный сырой корпус остаётся приватным (approved pairs/checkpoints/manifests/reports = project assets); exact-prompt-overlap проваливает (не прячется); trained-model lifecycle exportable/importable/locatable/loadable/continue-trainable; personal/global pair разрешён контрактами; wiki-evidence помогает адаптации без детерминированного роутинга; feedback улучшает без утечки сырого приватного; user-origin prompt source трассируем; parent-архитектор не исполняет prod/test код; Phase 37/38 — внешние зависимости; sub-agent WO bounded; review до zero findings; host-код вне `llm_mvp_core` разбит по платформам.

### O. §18 Mental Experiment — 9 сценариев (проверка «план как рабочая система, а не набор цитат»)

1. **First prompt = план без названных скиллов** → грузит server-library локатор + `.xiigen` wiki/prefs/profile; PromptUnderstanding/TaskUnderstanding/TaskType/Routing дают обучаемые решения; SkillCandidate предлагает пространство поиска ЧЕРЕЗ обученную машину (не BM25/regex/frontmatter/static-tags); SkillSelection выбирает planning/plan-review/product-decision/source-authority скиллы; PromptContext строит пакет, сохраняя user-скиллы и порядок авторитета. (R-001/002/004/006/015).
2. **Промпт неоднозначен, но вывод AI вскрыл пропущенный скилл** → stop-hook шлёт original prompt+trace+packet+output в ResultGap; OutputValidation/TaskCompletion/IntermediateCheck решают accept/patch/redo; recovery пишет DPO-сигнал; НЕ детерминированный чеклист/regex/if/валидатор. (R-003/005/006/015).
3. **Feedback: загружены неправильные скиллы** → хранится под user `.xiigen` (не в server-library, не в host-пакете); `SkillFeedbackAnalyzer`→DPO personal rows или reviewed preference-patch; personal-модель адаптируется через `SkillSelectionMachinePair.Personal`; сырые приватные промпты/feedback/wiki/пути НЕ синкаются в global. (R-011/012/014/015).
4. **Server library апгрейдится** → грузит новую версию по локатору/манифесту; project-local wiki/prefs/feedback/profile/lock остаются project-owned; при смене skill-id — patch-предложение, не перезапись; решения после апгрейда всё ещё из обучаемых машин. (R-010/014/015).
5. **Reviewer проверяет, не просто ли вставлены цитаты** → каждый prompt/feedback/question/answer/resolution в §1A + маппится в requirements/sections/gates/artifacts; план отвергается, если source-entry существует только как цитата без рантайм-эффекта. (R-013/015/016).
6. **Conditional-pass находки должны быть починены в плане** → `IMachineTraining` конкретный артефакт (не опциональное слово); per-domain partition schema; assistant-resolutions как in-plan evidence; Gate B проваливается, если любого из трёх нет. (R-004/005/013/015/016).
7. **Architect хочет sub-agent review до нуля даже мелких находок** → маленькие review-only WO независимым суб-агентам; проверка requirements/v20/trainable-unit/work-order-size раздельно; любая blocking/minor находка → architect repair; executor approval удержан до zero findings следующего раунда; review-loop НЕ добавляет рантайм-правило explicit tool authorization. (R-017/018/019).
8. **Work order слишком большой** → WO на целую фазу / несколько гейтов / Phase37+38 / impl+commit-push отвергается ДО делегации; architect дробит на ≤15-мин слайсы; executor возвращает пакет и останавливает слайс. (R-018/019).
9. **Host-расширения случайно спланированы как одно репо** → shared `xiigen-skill-selection-machine-extension` репо отвергнуто; Claude→только `claude-...`, Codex→только `codex-...`, Cursor→только `cursor-...`; три репо могут копировать core runner-exe + locator-config, но ни одно не владеет training/semantic-decisions/canonical-skill-source/private-corpus/project-local-state. (R-020).

**Review conclusion:** `REPAIR_REQUIRED_UNTIL_GATE_B15_ZERO_FINDINGS`. «Починенный план больше не трактует детерминированную логику как приемлемый candidate/fallback слой; требует обучаемые машины для КАЖДОГО семантического юнита и резервирует детерминированный код только для механического I/O и reproducibility-guards».

### P. Дельта «амбиция плана ↔ реальность» — критично для continue-плана VS/VSCode

- **Амбиция (этот план):** 11 отдельных обучаемых машин + 11 `IMachineTraining` адаптеров + 11 `*-machine-runner.exe`, каждая с 50+50 DPO-строк, actual-метриками, полным export/import/locator/fresh-load/continue-training lifecycle, тремя отдельными host-репо, 15 Gate B, 12 фаз, sub-agent review до zero findings, интеграция с v20 Phase 37/38.
- **Реальность (RAG-10 + Pass 1 §E):** 11 машин **сконденсированы в 2** (`PromptToTaskMachine` + `OrchestrationReasoningMachine`) в `43.SmartSkillsDomain` (36 .cs), реализовано как Phase 39 плана v20. Три host-репо СУЩЕСТВУЮТ, но на 99% состоят из evidence/reports прошлых прогонов (`phase40-*` трейсы), а собственно расширения — `cursor .../src/extension.ts` (1 файл, 1628 строк) + claude/codex `*-entrypoint.ps1` (246 строк каждый). Host-контракт (§C Pass 1) прямо признаёт `runtime_host_proof_performed:false`, `host_prompt_to_task_implementation:false`, `host_orchestration_reasoning_implementation:false` — то есть статическая упаковка есть, а живого runtime-доказательства хоста НЕТ.
- **Что это значит для continue-плана:** амбиция плана — исчерпывающий чеклист «полностью обученного расширения» (15 гейтов, 11 доменов, dual lifecycle). Реальность — 2-машинный домен + тонкие адаптеры + масса static-evidence. Разрыв, который continue-план должен закрыть: (а) сверить, покрывают ли 2 реальные машины 11 семантических доменов плана или часть просто отброшена; (б) есть ли реальные DPO-корпуса 50+50 на домен и actual-метрики (Gate B-2/B-3/B-14) или только static-контракты; (в) существуют ли 11 runner-exe и core-exe-manifest (Phase 8, Gate B-10) или host-репо копируют что-то иное; (г) выполнен ли live-host runtime proof (Gate B-7/B-8) — контракт говорит НЕТ. Эти четыре — приоритетные ряды доски continue-плана для VS/VSCode.

Метаданные Pass 2/10: RAG-файл дополнен разделами F–P; прочитан `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` строки 1729–3252 (вся вторая половина, deep, read-only). Ранние секции Pass 1 не тронуты.

## Pass 3/10 — marketplace-мастера: model-asset lifecycle + publish/install/upgrade + licensing + IModelRepository/locator + conflict-map

> Deep-read (read-only): `XIIGEN-MARKETPLACE-MASTER.md` (1319 строк), `XIIGEN-MARKETPLACE-MASTER-PLAN.md` (1169 строк), `STATE-marketplace-final.json`, `STATE-consolidation.json`, `CONSOLIDATION-INDEX.json`, `CONFLICT-MAP.md`. Ниже — честная сверка того, что бриф Pass 3 ожидал, против того, что в файлах есть на самом деле.

### Q. ГЛАВНОЕ — сверка scope: это НЕ внутренний marketplace модели-ассета

Обе «marketplace-master» доки — это **competitive / organic-traffic distribution strategy**: план публикации XIIGen-плагинов в **СТОРОННИЕ публичные app-store-витрины** (Figma Community, Canva Apps, Miro, Shopify App Store, VS Code Marketplace, Chrome Web Store, monday.com, Stripe, Atlassian, Google Workspace, Webflow, Framer, n8n, Salesforce). Слово «marketplace» здесь = чужие витрины плагинов; цель — гнать органический трафик на `xiigen.com` через freemium/account-gates. Это **НЕ** сеть направления #5, где аффилиат обучает модель, тестит её и шарит в сети, а модели импортируются/апгрейдятся/встраиваются в xiigen-продукты.

Прямое следствие для брифа (evidence-bound, без overclaim): **в этих файлах ОТСУТСТВУЮТ** — model-asset lifecycle обученной `ILearningMachine`, `IModelRepository`, `IRemoteUnitLocator`, manifest/checkpoint, publish/discover/install/upgrade/rollback ОБУЧЕННОЙ модели, лицензирование/роялти модели, affiliate revenue-share за расшаренную модель, `25.ModelServer`. Ни одна из этих сущностей ни разу не упоминается. Слово «upgrade» встречается только как «Workflow Upgrade» (импорт готового workflow → production-версия) и как platform-аналитика «installs/upgrades/uninstalls». Model-distribution backbone для «share/import/upgrade models» надо брать из core (`25.ModelServer` + Trainable Unit pattern), НЕ из этих research-доков (см. §X).

### R. Два master-документа — дубликат или эволюция?

Не эволюция, а **два параллельных прогона консолидации ОДНИХ И ТЕХ ЖЕ 12 источников**, оба помечены v1.0, 19 марта 2026, оба заявляют «supersedes all previous marketplace research».

| | `XIIGEN-MARKETPLACE-MASTER.md` | `XIIGEN-MARKETPLACE-MASTER-PLAN.md` |
|---|---|---|
| Заголовок | «Master Reference» | «Master Plan» |
| Строк | 1319 | 1169 |
| State-файл | `STATE-marketplace-final.json` (Phase 1-6, 15 конфликтов) | `STATE-consolidation.json` (Phase 1-6, 4 конфликта) |
| Структура | 5 секций: Platform Master Table (тиры) → Plugin Catalog → Research Intelligence → Strategy & Build Order → Source Index | 12 секций: Exec Summary → Strategic Framework → Platform Registry → Per-Platform Plans → Adaptation Network → Build Order → Product Architecture → Competitive → Research → Prompts → Grand Total → Sources |
| Фокус | Tier-модель органического открытия + verified indie examples + methodology/prompts | Per-platform build order (Waves 0-6) + adaptation network (10 ядер→80+) + product architecture (4 семейства + core engines) |
| Итог плагинов | ~64 (VIDEO **6**) | 65 (VIDEO **7**) |

Единственное числовое расхождение между близнецами: **VIDEO 6 vs 7, TOTAL 64 vs 65** (CONFLICT-MAP/STATE-final = 64/VIDEO6; CONSOLIDATION-INDEX/STATE-consolidation = 65/VIDEO7). Всё остальное совпадает. Для continue-плана: per-platform детали и build-order брать из `MASTER-PLAN.md`, tier-модель/methodology/verified-examples — из `MASTER.md`. Обе описывают ОДИН backend: 1 FastAPI + 1 React component library + 1 `@xiigen/plugin-sdk`.

### S. Единственный реальный «distribution backbone» здесь — Shared Infrastructure

Это то, что действительно относится к делу. Обе доки описывают одинаковый общий backend (critical path, 2-3 недели, «nothing ships without it»):

| Компонент | Что делает | Tech |
|---|---|---|
| `xiigen.com/plugins` | Landing всех плагинов, «Get Started Free» | React/Next.js |
| Auth service | Free-аккаунты + OAuth (Google/GitHub/Figma…) | FastAPI + JWT (`auth-jwt`) |
| AI Gateway | Rate-limited proxy к Claude/OpenAI/Gemini, freemium X calls/day | FastAPI + AI Engine Fabric |
| Usage tracker | Per-user/per-plugin счётчики, enforce freemium | Redis + Elasticsearch |
| Dashboard | Все плагины/использование/сохранённые выводы юзера | React + dynamic-documents |
| `@xiigen/plugin-sdk` | Общий npm-пакет: auth + API-вызовы для ВСЕХ плагинов | TypeScript |
| Template Library | Поисковые шаблоны на xiigen.com (SEO-движок) | React + Elasticsearch |
| Brand Kit Manager | Логотип/цвета/шрифты/voiceover per user | Dashboard + auth-jwt |

Это **ближайший аналог «model-distribution backbone»**: один backend обслуживает все хосты, хост-плагин — тонкий адаптер, импортирующий SDK ради auth + API-вызовов. Точно совпадает с R6 (простой agent boundary: API-вызовы task→answer) и с core/host split из Pass 1. SDK Commonalities: 10 из 14 платформ на React → shared component library покрывает 70%+ UI; OAuth 2.0 строится один раз в SDK; единый AI-gateway для всех 64/65 плагинов. Для direction #5 этот скелет (SDK + gateway + dashboard + account-gate) — правильная форма распространения, но конкретики модели (checkpoint/manifest/`IModelRepository`) в нём нет — её надо доложить из core.

### T. VS Code / JetBrains / MCP — прямо в цель continue-плана VS & VSCode

- **VS Code Marketplace** (в `MASTER.md` — Tier 1, четыре звезды): 73.6% всех разработчиков, 60,000+ расширений, 3.3B инсталлов; данные **публичные** (счётчики инсталлов); вход **полностью автоматический, без ручного ревью**; взрывной рост у AI-tools (Cline 5M+/~400K в мес, BLACKBOXAI 1.9M, Roo Code 901K, Continue 1.6M, Kilo Code 1.5M+$8M seed). Caveat: почти все успешные — AI coding tools; медиана очень низкая; 2 вредоносных AI-расширения (2024) набрали 1.5M инсталлов → «спрос на AI-плагины реален независимо от качества».
- **Важная оговорка авторов:** в `MASTER-PLAN.md` (§3, «20 EVALUATED but NOT in Plans») VS Code переведён в «Research only / Downloads≠Traffic — invisible after install» — как traffic-канал они его СНЯЛИ (инсталлы не гонят на xiigen.com). Но как **distribution-канал именно для расширения** он Tier-1: прозрачные данные + мгновенная публикация. Это ровно наш случай.
- **JetBrains** (три звезды): ~12M devs, 8,860+ плагинов, ревью 5-14 дней, данные непрозрачны (installs видит только автор), нужна совместимость с каждой версией IDE. «Good after VS Code.»
- **MCP как новый канал** (обе доки, в Risk/Opportunity): Model Context Protocol стал стандартом AI-agent↔external-data в 2025; к началу 2026 — **97M SDK-загрузок/месяц**; ОДИН MCP-сервер сразу доступен Claude Code, Cursor, GitHub Copilot одновременно → «agent traffic». Это ключевой рычаг для continue-плана: единый MCP/core-endpoint покрывает Claude+Cursor+Codex сразу — совпадает с направлением RAG-10 «extensions поверх наших данных» и с core/host split. Penpot тоже двигается к MCP-server+plugin-system+marketplace (upgrade из «watch»).

### U. Traffic-mechanism gate — переносимая бизнес-логика распространения

Иерархия механизмов конверсии (сильнейшие → нулевой):

`Account required` (STRONGEST) → `Dashboard on site` (STRONGEST) → `Freemium gate` (STRONG) → `«Powered by XIIGen» branding` (MEDIUM) → `Docs on site only` (MEDIUM) → `Export to XIIGen` (MEDIUM) → `Nothing` (ZERO — Clean Keyboard, JSON formatter).

Правило: каждый плагин обязан иметь ≥2 из top-4 механизмов. Traffic math: `Real traffic = installs × visibility × click-through × account-creation-rate` (Shopify 500×100%×100%×100%=500 аккаунтов ← BEST; Figma 5K×100%×15%×50%=375; Obsidian 50K×100%×0.5%×50%=125 ← WORST для трафика на сайт). FLOW-плагины особо ценны: ~80%+ account-creation vs ~15% у utility, 30+ мин на сайте, высокая virality/return.

Для direction #5: этот же gate объясняет, ПОЧЕМУ шаринг модели должен идти через account/dashboard (import/upgrade/embed → visit xiigen.com), а не как standalone-файл. Это логика распространения, не model-lifecycle.

### V. Licensing / monetization — что есть, чего нет

**Есть — монетизация ПЛАГИНОВ, не моделей.** Platform revenue-shares: Atlassian **0% для Forge apps до $1M lifetime** (с 1 янв 2026), Framer **100% автору** (нет комиссии), Canva **Innovation Fund + App Adoption Awards** (платформа платит разработчикам, >30% новых апп получили выплаты), Shopify 0% на первый $1M/yr, n8n free (npm), Chrome $5 one-time. Economics indie micro-SaaS: MRR $1K-15K (лидеры до $220K), gross margin ~76-90%, CAC ~$0 (organic), valuation 30-45× monthly profit. Freemium — единственная «лицензионная» механика: free-tier limits (X/day/week) → «Upgrade at xiigen.com».

**Чего нет (для брифа):** НЕТ лицензирования/монетизации обученной модели, НЕТ affiliate revenue-share за расшаренную модель, НЕТ marketplace-листинга модели, НЕТ per-model pricing/royalty. Направление #5 «аффилиат обучает→тестит→шарит модель; модели импортируются/апгрейдятся/встраиваются в xiigen-продукты» в этих доках **не спроектировано** — только косвенно (freemium/account gate как форма распространения).

**Risk, релевантный шарингу модели-ассета:** supply-chain (янв 2026 вредоносные npm в экосистеме n8n украли OAuth-токены) → mitigation: sandbox, минимум прав, OSS-код на GitHub для доверия. Это прямой аргумент за trust/provenance при шаринге модели (source/license gate из core).

### W. Что примирили CONFLICT-MAP и CONSOLIDATION

- **Правило авторитета:** `Addendum-v2 wins over main doc; ChatGPT/Gemini = источники, main doc уже перевёл их`. Разрешено 15 number-конфликтов (MASTER.md) / 4 (MASTER-PLAN.md).
- **Ключевые number-фиксы:** Miro 60M→**100M+**, apps ~220→**250+**; Shopify 8K→**16,000+**; Wix 250M→**278M+** (+6M premium sites); Lucid 70M→**100M+** и «no marketplace»→**100+ integrations**; Atlassian rev-share→**0% Forge до $1M**; Webflow →**300+**; Penpot «watch only»→**MCP server + plugin system + marketplace roadmap**; Atlassian apps 5,700/6,000+/8,000+ → **6,000+** (middle ground).
- **Obsidian-снапшоты — не ошибки, а разные даты:** диапазоны (Notebook Navigator 207K–375K, TaskNotes 40K–117K и т.д.) = ChatGPT-снимок (поздний) vs Gemini/main (ранний), обе валидны → показывать как range с пояснением.
- **Organic-tier upgrades:** Miro 3★→4★, Lucid 🔴→2★, Webflow 2★→4★, Framer 2★→3★, Penpot watch→2★.
- **Supersessions:** DOC-plans1 (v1) ← DOC-plans2 (v2 с adaptation network, 41 utility); DOC-consol1 execution-seq (§10) ← DOC-strat1 (traffic-strategy) + DOC-plans2 (wave schedule). Missing-data: traffic-mechanism столбец был только в DOC-strat1 (top-20), FLOW/AUTO/VIDEO жили в отдельных файлах — обе master-доки их слили в per-platform секции.
- **34 платформы** = 20 (main research directory) + 14 (B2B discoveries: monday/Stripe/Salesforce/HubSpot/Zoom/Zendesk/Zoho/Pipedrive/Freshdesk/ServiceNow/Magento) + addendum-корректировки. Семейства плагинов: UTILITY 41 + FLOW 8 + AUTO 9 + VIDEO 6/7 = 64/65. AVOID-лист: WordPress (алгоритмическая стена, новые плагины <10 инсталлов), WooCommerce, Notion (invite-only), Bubble (Zeroqode-монополия 60% выручки), Squarespace, Sketch (умирает), Lunacy (плагины закрыты), Make.com (закрытая).

### X. Мост к core (direction #5 / 25.ModelServer) — где реально брать backbone

Бриф Pass 3 просил model-asset lifecycle + `IModelRepository`/`IRemoteUnitLocator`/manifest. Этого в research-доках нет; по CLAUDE.md эти контракты живут в **core**:
- `25.ModelServer` публикует принятые checkpoints через `IModelRepository`;
- обучаемый unit discoverable через `IRemoteUnitLocator`;
- export/import через **manifest с checkpoint-refs**;
- **Trainable Unit pattern:** зарегистрировать flow-adapter в `15.OrchestratorModel/Logical/`, persist runner-report (accuracy/loss/reward-margin, CPU/RAM/elapsed/row-counts), project-specific учёные units держать в целевом репо, general XIIGen units — в этом репо; «unit, который нельзя export/import/locate/validate — ещё не часть проекта».

Вывод для continue-плана: **«model-distribution backbone» = Trainable Unit pattern из core (checkpoint/manifest/locator/repository/fresh-load/continue-training) + shared-backbone-скелет (§S) из этих доков (SDK/gateway/dashboard/account-gate) + MCP-channel (§T)**. Marketplace-research даёт форму КАНАЛА и бизнес-механику конверсии; сам lifecycle обученной модели — из `25.ModelServer`. Continue-план обязан сшить эти два, а не искать model-lifecycle в этих файлах (его там нет).

### Y. Что осталось на passes 4-10 (по инвентарю папки `extensions reasearch`)

Не тронуто и приоритетно для VS/VSCode-continue + direction #5:
1. **`PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v1-CLAUDE.md` + `-CODEX-PORT` + `-CURSOR-PORT` + `-LICENSING`** — token-optimizer plugin; вероятно ближайший к НАСТОЯЩЕМУ расширению (реальный host-integration + licensing-механика). Высокий приоритет.
2. **`PLAN-LOCAL-TESTING-CLAUDE-v1.md` / `-CODEX-v1.md` / `-CURSOR-v1.md`** — dual-path Local Desktop (Path B) сценарии тестирования в реальном хосте. Прямо к Dual-Path gate.
3. **`XIIGen-Desktop-Plugin-Adaptation-Report-v2.md`** (53KB) — desktop adaptation report.
4. **`claude-xiigen-smart-skills/`, `codex-xiigen-smart-skills/`, `cursor-xiigen-smart-skills/`** — реальные host-репо (сверить с §P Pass 2: 99% static evidence, `runtime_host_proof:false`).
5. `skill-selection-machine-runs/`, `Plans/`, `other/`, `FigmaToCode-*` (design-gen), `V2 files.zip`, PDF-планы (3D pipeline) — ниже приоритет, evidence-only.

**Метаданные Pass 3/10:** RAG-файл дополнен разделами Q–Y (append-only). Прочитаны read-only: `XIIGEN-MARKETPLACE-MASTER.md` (1319 стр., полностью), `XIIGEN-MARKETPLACE-MASTER-PLAN.md` (1169 стр., полностью), `STATE-marketplace-final.json`, `STATE-consolidation.json`, `CONSOLIDATION-INDEX.json`, `CONFLICT-MAP.md`. Ранние секции (Pass 1 A–E, Pass 2 F–P) не тронуты. Главный вывод прохода: эти «marketplace-master» — про СТОРОННИЕ app-store-витрины и organic-traffic, а НЕ про model-asset marketplace направления #5; реальный model-lifecycle backbone — в `25.ModelServer`/Trainable Unit pattern (core), сшивать в continue-плане.

---

## Pass 4/10 — token-optimizer plugin: механизм экономии токенов + per-host порты (Claude/Codex/Cursor) + измерение savings + licensing

> Deep-read (read-only) все четыре файла семейства: `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v1-CLAUDE.md` (535 строк — главный), `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CODEX-PORT-v1.md` (368 строк), `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CURSOR-PORT-v1.md` (293 строки), `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-LICENSING-v1.md` (324 строки). Это самый ранний (2026-05-07) и самый «близкий к настоящему расширению» слой папки: реальный host-integration через hooks + per-host порты + licensing-механика. Ниже — механизм экономии, per-host тонкие адаптеры, ЧЕСТНАЯ оценка «доказательства savings», лицензирование, и точный мост Hook 2 → SkillSelectionMachine.

### Z. Что это + ГЛАВНЫЙ дисклеймер: плагин детерминирован ПО ЗАМЫСЛУ (конфликт с обучаемым направлением)

Цель плагина (шапка главного плана, строки 12–13): «снизить потребление токенов в Claude Code сессиях на ~50-60% через **детерминированные хуки, без участия модели**». Это плагин для Claude Code, запускающий 4 детерминированных хука + 5-й компонент (Task Completion Validator). Каждый хук = отдельный `.exe`, скомпилированный Native-AOT single-file из УЖЕ существующего C#-кода в `llm_mvp_core`; создаётся новый проект `40.HookRunners/`. Модель «не тратит токены на то, что хук делает сам».

**Критический honesty-факт для continue-плана.** Три из четырёх файлов (все кроме LICENSING) несут одинаковую шапку-баннер (строки 1–5):
> **HOOK 2 SUPERSEDED — Phase 39 (2026-06).** Hook 2 (`SkillSelector` через BM25/`AstRelevanceRanker`) заменён. Phase 39 реализует `SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>` как слой решения. Структура упаковки хука (hooks.json, события SessionStart/UserPromptSubmit/Stop) остаётся валидной опорой для Phase 40. См. `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\`.

Формально баннер помечает устаревшим ТОЛЬКО Hook 2. Но по факту весь замысел «детерминированные хуки без модели» противоречит поздней trainable-архитектуре (§0/A-003/R-004 плана SkillSelectionMachine, разделы D/K/O этого RAG): там ЗАПРЕЩЕНЫ BM25, `AstRelevanceRanker`, Jaccard, regex, keyword-maps, фиксированные task-maps, `if`-роутинг, детерминированные валидаторы в ЛЮБОМ семантическом пути. А в этом плане детерминированы и Hook 1 (`ClassifyByKeywords` — keyword-роутинг), и Hook 4 (`OutputValidator` — фиксированные проверки = deterministic validator), и Hook 5 (`RequirementExtractor` — regex/императивные конструкции). Вывод для доски: **устарел не только Hook 2 как отдельный exe — устарела вся посылка «детерминированный семантический слой». Переиспользуема НЕ семантика хуков, а их УПАКОВКА (событийный каркас, exe-per-hook, per-host порт-маппинг, licensing).**

### AA. Механизм экономии токенов — 5 (+1 в Cursor) хуков, откуда именно берётся savings

Экономия складывается из двух источников: (1) **обрезка контекста** (грузить меньше в модель) и (2) **предотвращённые потери** (нет retry, нет повторных уточнений). Пять хуков главного плана:

| # | Хук | Событие | Что делает (механизм savings) | Источник кода в `llm_mvp_core` | Новый код `40.HookRunners/` | Время | Заявл. экономия |
|---|---|---|---|---|---|---|---|
| 1 | **Orchestration Router** | `UserPromptSubmit` | Классифицирует домен+субфазы ДО модели, инжектит минимальный orchestration-хинт → модель не тратит токены на разведку типа задачи | `29.SemanticCompiler/Routing/RouteClassifier.cs` (`ClassifyByKeywords`), `21.AgentInfrastructure/Logical/PromptDomainClassifier.cs` (`Classify`), `MultiSubPhaseRouterUnit.cs` | `OrchestrationRouter/Program.cs` → JSON `{domain, subPhases, orchestrationHint}` | ≤300ms | ~15-25% |
| 2 | **Skill Selector** ⚠️SUPERSEDED | `UserPromptSubmit` (`depends_on` хук1) | По домену+субфазам выбирает 3-5 скиллов, читает ТОЛЬКО frontmatter SKILL.md (~100 слов, не тело) → в контекст 3-5 файлов вместо 20+ | `22.AgentAlgorithms/Context/AstRelevanceRanker.cs` (`Rank`, **BM25 по описаниям скиллов**), `AstPackedContextBuilder.cs` | `SkillSelector/Program.cs` + `SkillFrontmatterReader.cs` → BM25-ранжирование, топ-5 | ≤400ms | **~30-40% (самый большой рычаг)** |
| 3 | **Context Injector** | `SessionStart` | Читает `STATE.json` (вверх по дереву, ≤5 уровней), формирует снэпшот ≤500 токенов (фаза+задачи+последние решения) → нет разведочных запросов в начале сессии | `21.AgentInfrastructure/Logical/LogicalContracts.cs` (`LogicalContextPacket`) | `ContextInjector/Program.cs` + `StateJsonReader.cs` → JSON `{context, tokenCount}` | ≤500ms | ~20% |
| 4 | **Output Validator** | `Stop` | После ответа проверяет DNA-правила: есть ли Gate B команды в плане? нет ли запрещённых паттернов (`ClsService` import, прямые SDK)? заявлен результат — есть ли доказательство? Нарушение → инжектит флаг в следующий промпт | DNA compliance guard skill, agent-constitution session-completeness | `OutputValidator/Program.cs` + `DnaRuleChecker.cs` → `{valid, violations[], injectionForNextPrompt}` | ≤600ms | непрямая (предотвращает 1-2 retry ×~500-1000 ток.) |
| 5 | **Task Completion Guard** (фокус) | `UserPromptSubmit` (CAPTURE) + `Stop` (VERIFY) | CAPTURE: из новой задачи извлекает требования → `.claude/task-state.json`. VERIFY: сравнивает покрытие; при `coverage<1.0` → exit code 1 + `injectionForNextPrompt` со списком незакрытого → Claude Code НЕ показывает финал, продолжает | agent-constitution session-completeness, no-product-decisions, `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs` (`ExtractPredicates`) | `TaskCompletionGuard/Program.cs` + `RequirementExtractor.cs` + `CoverageChecker.cs` | ≤400ms | непрямая (нет «ты забыл X» ×~300-500 ток., нет пересказа контекста) |

Механика `hooks.json`: только `task-completion-guard --mode verify` имеет `block_on_exit_nonzero: true`; все прочие `on_failure: continue` (сбой хука не ломает Claude Code). Сборка: `dotnet publish -c Release -r <RID> --self-contained -p:PublishSingleFile -p:PublishAot` для win-x64/osx-arm64/linux-x64. Порядок разработки: 1) TaskCompletionGuard (ценнее всего, standalone), 2) SkillSelector, 3) ContextInjector, 4) OrchestrationRouter, 5) OutputValidator, 6) упаковка. **Итог по замыслу: ~50-60% на типичную сессию.** Суть механизма: экономия ≈ skill-selection (топ-5 вместо 20+) + context-trimming (снэпшот вместо разведки, сжатие файлов) + предотвращённый retry/re-clarification. Именно как гипотеза брифа: skill-selection + context-trimming; крупнейший единичный рычаг — Hook 2 skill-selection (детерминированный BM25).

### AB. Мост Hook 2 (BM25 SkillSelector) → SkillSelectionMachine — точная supersession (совпадает с RAG-05/08 и Pass 1-2)

Точный маппинг, который continue-план обязан нести:

- **Было (Hook 2, этот план):** `SkillSelector/Program.cs` читает frontmatter всех `SKILL.md`, ранжирует их **BM25 по описаниям** (`AstRelevanceRanker.Rank(query, candidates)`), где query = `domain + subPhases` от Hook 1, возвращает топ-5. Это ДЕТЕРМИНИРОВАННЫЙ retrieval-скорер — ровно тот класс, который RAG-05/08 (по формулировке брифа) и §0/A-003 плана SkillSelectionMachine пометили как «старый детерминированный BM25 SkillSelector hook».
- **Стало (Phase 39):** `SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>` — обучаемый слой решения «какие скиллы поднять», DPO 50 train + 50 held-out на КАЖДЫЙ из 11 семантических доменов, Gate B-1..B-15, honesty-инвариант `machine_decision=true` / `deterministic_semantic_source=false` (разделы F–L этого RAG). Никакого BM25/regex/frontmatter-tags/static-tags в продуктовом пути.
- **Что осталось валидным от Hook 2:** ТОЛЬКО упаковка — событие `UserPromptSubmit`, `depends_on` цепочка, exe-per-hook, JSON stdin/stdout контракт. Т.е. `skill-selector.exe` превращается в один из 11 `*-machine-runner.exe`, вызывающий обученную машину вместо BM25; каркас hooks.json не меняется.

Это стыкуется с находкой Pass 1 §D/§E и Pass 2 §P: 11 машин плана сконденсированы в `PromptToTaskMachine` + `OrchestrationReasoningMachine` (`43.SmartSkillsDomain`, 36 .cs). Т.е. функция «выбрать скиллы/понять задачу» из детерминированного Hook 2 ушла в trained `PromptToTaskMachine`. (Оговорка честности: содержимое RAG-05/08 в этом проходе НЕ читалось — это RAG-файлы других папок; ссылка опирается на формулировку брифа + уже записанное в Pass 1-2 этого файла.)

### AC. Per-host тонкие адаптеры (Claude/Codex/Cursor) — ближайший к реальному пример core/host split

Центральный переиспользуемый актив семейства. **Ядро = те же байтовые exe на всех хостах** (бинарники host-независимы); хост = тонкий адаптер, отличающийся ТОЛЬКО манифестом, именами событий, путями (через env) и механизмом блокировки. Сводная таблица:

| Аспект | Claude Code | Codex | Cursor |
|---|---|---|---|
| Манифест/конфиг | `.claude-plugin/plugin.json` + `hooks/hooks.json` | `.codex-plugin/plugin.json` (содержимое идентично) + `hooks/hooks.json` | `.cursor/hooks.json` (НЕТ отдельного plugin-манифеста — просто папка) |
| SessionStart | `SessionStart` | `SessionStart` | `sessionStart` |
| Перехват промпта | `UserPromptSubmit` | `UserPromptSubmit` | `beforeSubmitPrompt` (нет `UserPromptSubmit`!) |
| Завершение | `Stop` | `Stop` | `stop` |
| Путь к скиллам | `.claude/skills/` | `./skills/` (env `SKILLS_DIR`) | `.cursor/skills/` (env `SKILLS_DIR`) |
| Путь task-state | `.claude/task-state.json` | `.codex/task-state.json` (env `TASK_STATE_PATH`) | `.cursor/task-state.json` (env `TASK_STATE_PATH`) |
| Механизм блокировки | exit code 1 | exit code 1 | JSON `{permission:"deny", message}` в stdout (exit code игнорируется) |
| OutputValidator режим | `--mode claude-code` (все проверки) | `--mode codex` (без Claude-специфичных; DNA+GateB работают) | `--mode cursor` |
| Установка | `--plugin-dir` или platform.claude.com | `marketplace.json` в `.agents/plugins/` (локальный) | копировать `.cursor/` в репо или `~/.cursor/` |
| Уникальное событие | — | — | **`beforeReadFile`** → Hook 6 File Content Compressor |

**Размер code-diff (доказательство «тонкости» адаптера):**
- Claude → **Codex ≈ 9 строк C#**: `OutputValidator` `--mode` флаг (~5), `TaskState.GetStatePath()` из env `TASK_STATE_PATH` (~4), `SkillSelector` уже читает `SKILLS_DIR`. Порт «~1 час после готовности Claude-версии». 3 полных порта (Context/Router/Guard), 2 частичных (SkillSelector — путь; OutputValidator — Claude-специфичные session-проверки отключаются в codex-режиме).
- Claude → **Cursor ≈ 55 строк**: `TaskCompletionGuard --output-format cursor` → JSON deny вместо exit code (~10), новый `FileContentCompressor` (~40), `OutputValidator --mode cursor` (~5). Порт «~2 часа после Codex». 5 полных портов, 1 частичный (Guard verify — иной механизм блокировки).

**Cursor-уникальный Hook 6 — File Content Compressor** (только через `beforeReadFile`, событие которого НЕТ в Claude/Codex): перед отправкой файла в модель убирает XML/блок-комментарии и пустые строки; код из `35.ExperimentsV2/Infrastructure/RegexPreprocessor.cs` (`Normalize`, по PLAN-E10 должен переехать в `12.SemanticCompiler/Compression/`); stdin `{content, filePath}` → если `.cs` → `Normalize` → `{content}`; ≤100ms; ~15-20% на чтение C#-файлов. Поэтому суммарная экономия Cursor выше: **~60-70%** vs ~50-60% у Claude/Codex.

Для continue-плана VS/VSCode это ПРЯМОЙ образец R5/R6 и контракта `xiigen-extension-contract.json` (Pass 1 §C): вся семантика — в ядре (exe/машины), хост — набор конфиг-различий (имя события + путь + механизм блокировки). **Порт-маппинг (имена событий, пути, блокировка) на 100% переиспользуем для trainable-версии; меняются только внутренности exe (детерминизм → обученная машина).** VS Code = тот же случай, что Cursor (Cursor есть форк VS Code): события `beforeSubmitPrompt`/`beforeReadFile`/`stop` и модель хуков через `.cursor/hooks.json` — это и есть VS Code-семейный контур.

### AD. Измерение/доказательство savings — ЧЕСТНАЯ находка: это ОЦЕНКИ, не измерения

Ключевое для целевого исхода «token savings are proven». **В этих четырёх планах НЕТ методологии измерения токенов.** Все проценты (~15-25%, ~30-40%, ~20%, ~50-60% итог) — это АРХИТЕКТОРСКИЕ ОЦЕНКИ в таблицах «Ожидаемая экономия», без:
- baseline-замера (сессия без хуков) vs treatment-замера (сессия с хуками);
- before/after счётчика токенов, A/B-харнесса, воспроизводимого прогона;
- какого-либо `token_usage_reported` артефакта в самом плагине.

Единственная реальная механика проверки токенов живёт НЕ здесь, а в плане SkillSelectionMachine (разделы J/K этого RAG): **Codex — «первый измерительный гейт»** (§11.2), потому что Codex отдаёт JSON session-метрики; **Gate B-7** требует `token_usage_reported=true` в codex-trace-gate. Т.е. «доказать savings» = построить измерительный контур на Codex-трейсах, а не поверить таблицам этого плана.

**Строка для доски continue-плана (evidence-bound):** token-savings здесь = *projected*, НЕ *proven*. Целевой исход «token savings proven» требует отдельного measurement-слайса: (а) зафиксировать baseline-токены на наборе эталонных сессий/промптов без плагина; (б) прогнать те же с плагином; (в) снять дельту из Codex JSON-метрик (`token_usage_reported`); (г) отчитать per-hook атрибуцию. Пока такого артефакта нет ни в планах, ни (по Pass 1-2 §P, контракт `runtime_host_proof:false`) в host-репо. Это самый важный незакрытый ряд для «savings proven».

### AE. Licensing / монетизация — freemium, 3 варианта ключа, ценообразование

Честный старт плана: ни Claude Code, ни Codex, ни Cursor НЕ имеют встроенного механизма платных лицензий на плагины (Cursor Marketplace с фев-2026 — только корпоративные team-marketplaces, монетизация не задокументирована). Лицензию надо строить самому. Модель — **freemium**:

- **Free** (публичный OSS-репо): все 5 хуков с лимитами — Guard ≤3 требований, SkillSelector топ-3 (не 5), Router только домен без субфаз, ContextInjector снэпшот без истории решений, OutputValidator базовые DNA-проверки.
- **Pro** (лицензионный ключ): лимиты сняты.

Три варианта реализации:
- **Вариант A — офлайн-ключ в env `XIIGEN_LICENSE_KEY` (РЕКОМЕНДОВАН для старта):** формат `xiigen-pro-{base64(payload)}-{hmac_sha256}`, payload `{email, tier, expires, issued}`, HMAC подписан приватным секретом, встроенным в exe при сборке. `Shared/LicenseChecker.cs` `Check()` → `LicenseTier.Free|Pro`. Продажа через Gumroad/Lemon Squeezy/Stripe (5-10% комиссии). БЕЗ сервера, офлайн, кроссплатформенно, 2-3 часа. Минус: ключ передаётся, нет отзыва без ребилда, нет аналитики. Отдельный приватный `OfflineKeyGenerator` CLI (не публикуется).
- **Вариант B — ключ + онлайн-активация:** первый запуск привязывает ключ к machine-id, дальше офлайн-кэш токена (30 дней). `POST /api/v1/activate` → `{valid, tier, token, expires}`; сервер хранит `{key, machine_ids[], activations_count, max_machines}`; Railway/Fly.io $5/мес или Cloudflare Workers бесплатно. Плюс: отзыв, аналитика, лимит машин. ~1 день.
- **Вариант C — Cursor Team Marketplace:** Pro = приватный team-marketplace; только Cursor, требует Cursor Teams у покупателя.

Рекомендация: A сейчас, B при масштабировании. Переключение поведения — внутри exe (`hooks.json` не меняется): `topN = Pro?5:3`, `maxRequirements = Pro?int.MaxValue:3`, Router `Pro?ClassifyWithSubPhases:ClassifyDomainOnly`. **Ценообразование:** Free $0; Pro Individual $49/год; Pro Team $149/год (до 5); Pro Unlimited $499/год; Lifetime Pro Individual $149. Итого работы ~7 часов. Связь с Pass 3 §V: там монетизация ПЛАГИНОВ (не моделей); этот licensing-план — конкретный per-plugin freemium-механизм под тот же вывод.

### AF. Дельта план↔реальность + переиспользуемое + остаток на passes 5-10

**Дельта план↔реальность (для continue-плана):** этот план описывает **Native-AOT single-file `.exe`-per-hook** (`bin/*.exe`). Но реальные host-репо (Pass 1 §B) содержат `cursor .../src/extension.ts` (1628 строк TS) и claude/codex `*-entrypoint.ps1` (по 246 строк **PowerShell**) + вызов `xiigen exe` core-API (контракт §C). Т.е. фактическая реализация пошла через PowerShell-entrypoint + единый core-API, а НЕ через 5-6 скомпилированных exe этого плана. Каркас событий (SessionStart/UserPromptSubmit-или-beforeSubmitPrompt/Stop) сохранился, транспорт — изменился.

**Что переиспользуемо (кладём в опору continue-плана):**
1. **Событийный каркас hooks.json** (SessionStart/UserPromptSubmit/Stop + `depends_on` + `block_on_exit_nonzero`) — валиден, баннер это подтверждает.
2. **Per-host порт-маппинг** (§AC): имена событий, env-параметризация путей (`SKILLS_DIR`/`TASK_STATE_PATH`), механизм блокировки exit-code-vs-JSON-deny, установка — готовая таблица для 3 хостов; VS Code = Cursor-контур.
3. **Cursor/VS Code `beforeReadFile` компрессия** (Hook 6) — уникальный доп-рычаг токенов, недоступный Claude/Codex.
4. **Licensing freemium** (§AE) — офлайн-HMAC + tiers + pricing.
5. **5 «функций фокуса/валидации»** как СИГНАЛЫ (не как детерминированные рантайм-правила): Task Completion Guard, Output Validator — в trainable-версии становятся `TaskCompletionMachine`/`OutputValidationMachine`/`ResultGapMachine` (раздел H этого RAG), а не regex/checklist.

**Что НЕ переиспользуемо (устарело):** детерминированная семантика хуков — BM25 SkillSelector (Hook 2, явно superseded), keyword-роутинг Hook 1, фиксированные валидаторы Hook 4, regex-извлечение требований Hook 5. Всё это заменяется обученными машинами `43.SmartSkillsDomain`.

**Приоритетные ряды доски (из этого прохода):** (a) построить measurement-контур savings на Codex JSON-метриках (Gate B-7) — «proven», не «projected» (§AD, самый важный); (b) перенести port-маппинг §AC на trainable exe/entrypoint без изменения событийного каркаса; (c) решить транспорт: PowerShell-entrypoint (как в реальных репо) vs AOT-exe (как в плане) — реальность выбрала PowerShell; (d) licensing §AE как отдельный поздний слайс.

**Остаток на passes 5-10** (по предложенному в Pass 1 распределению): Pass 5 — local-testing планы (`PLAN-LOCAL-TESTING-{CLAUDE,CODEX,CURSOR}-v1.md`, dual-path Path B) + `XIIGen-Desktop-Plugin-Adaptation-Report-v2.md`; Pass 6 — `cursor-xiigen-smart-skills/src/extension.ts` (1628 строк, реальный thin-adapter) + reports; Pass 7 — claude/codex `*-entrypoint.ps1` + phase40-трейсы + smoke; Pass 8-9 — FigmaToCode (design→code + skill-advisor); Pass 10 — `other/` marketplace-research + СИНТЕЗ + финальная сверка плана↔`43.SmartSkillsDomain`.

**Метаданные Pass 4/10:** RAG-файл дополнен разделами Z–AF (append-only). Прочитаны deep read-only: `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v1-CLAUDE.md` (535 стр., полностью), `-CODEX-PORT-v1.md` (368 стр.), `-CURSOR-PORT-v1.md` (293 стр.), `-LICENSING-v1.md` (324 стр.). Ранние секции (Pass 1 A–E, Pass 2 F–P, Pass 3 Q–Y) не тронуты. Главный вывод: token-savings здесь *projected* (оценки в таблицах), не *proven* — реальный measurement-контур строится на Codex-трейсах (Gate B-7); переиспользуема упаковка (событийный каркас + per-host порт-маппинг + beforeReadFile-компрессия + licensing), а не детерминированная семантика хуков (Hook 2 BM25 явно заменён `SkillSelectionMachine`).

---

## Pass 5/10 — local-testing (Path B) per host Claude/Codex/Cursor + desktop-adaptation-report: entrypoints + evidence + blockers

**Прочитано deep read-only (4 файла):** `PLAN-LOCAL-TESTING-CLAUDE-CODE-v1.md` (404 стр., целиком), `PLAN-LOCAL-TESTING-CODEX-v1.md` (482 стр., целиком), `PLAN-LOCAL-TESTING-CURSOR-v1.md` (450 стр., целиком), `XIIGen-Desktop-Plugin-Adaptation-Report-v2.md` (973 стр., v2.0, целиком). Все три testing-плана — 2026-05-07, ARCHITECT, версия 1.0. Общая цель каждого: запустить плагин в реальном установленном хосте локально, прогнать одинаковый набор из **5 задач** (BUG / PLAN / ARCH / CODE_Q / REFACTOR) сначала БЕЗ плагина (baseline), потом С плагином, и измерить экономию токенов/времени/retry. **Это ровно контур Path B (local desktop) из Dual-Path Same-Scenario Coverage** нового плана: как поднять реальный хост, вызвать расширение, снять доказательства (логи/транскрипты/скриншоты). Раздел append-only, ранние секции (Pass 1 A–E, Pass 2 F–P, Pass 3 Q–Y, Pass 4 Z–AF) не тронуты.

**ВАЖНАЯ ОГОВОРКА про scope этих планов (не путать хостовые модели).** Все три local-testing плана тестируют **token-optimizer HOOK-плагин** (5–6 хуков-exe: `context-injector`, `orchestration-router`, `skill-selector`, `task-completion-guard`, `output-validator`, + Cursor-only `file-content-compressor`), а НЕ панельное расширение `xiigenSmartSkills.panel` из реальных host-репо (Pass 1/4; сам `extension.ts` разбирается в Pass 6). То есть это Path B для **хуковой (event-driven) модели**. Методология (поднять хост → выключить для baseline → 5 задач → включить плагин → снять evidence → сравнить) полностью переносится на continue-план, но конкретные точки входа у панельного расширения VS Code/Cursor другие — см. раздел AK «Разрыв».

### AG. Claude Code — Path B (local desktop): entrypoint, процедура, evidence

**Точка входа / загрузка плагина (без установки в маркетплейс):**
- `claude --plugin-dir ./xiigen-token-optimizer` — **рекомендованный** способ для разработки (Способ 1);
- `claude --plugin-dir A --plugin-dir B` — несколько плагинов сразу (Способ 2);
- `claude --plugin-url https://.../xiigen-plugin-build.zip` — zip по URL для CI/CD (Способ 3).
- Локальная копия имеет приоритет над маркетплейс-версией с тем же именем на эту сессию.
- `/reload-plugins` — перезагрузить хуки без рестарта Claude Code.

**Реальный host-entrypoint = hook-события CLI**, а не UI-команда: `SessionStart`, `UserPromptSubmit`, `Stop`. Хук получает данные сессии через env-переменные: `HOOK_EVENT_NAME`, `CLAUDE_SESSION_ID`, `CLAUDE_TRANSCRIPT_PATH`. Проверка загрузки: `/plugins` (должен показать `xiigen-token-optimizer`).

**Инфраструктура измерения:** `HookLogger` пишет строку в `~/.xiigen/hook-log.jsonl` при каждом вызове exe. Отдельный `token-meter.exe` на `SessionStart` кладёт метку старта, на `Stop` — читает `CLAUDE_TRANSCRIPT_PATH` и оценивает токены (~4 символа = 1 токен) + размер транскрипта в байтах. Регистрируется в `hooks.json` как два хука (SessionStart/Stop, `on_failure: continue`).

**Процедура (5 фаз):**
1. Фаза 1 — собрать инфраструктуру логов (HookLogger + token-meter).
2. Фаза 2 — baseline: `disableAllHooks: true` в `~/.claude/settings.json` (или `.claude/settings.json`); прогнать 5 задач, каждая = отдельная сессия через `/new`; **вручную** записать время ответа, число сообщений до финала, были ли переспросы → `~/.xiigen/baseline.jsonl`.
3. Фаза 3 — с плагином: `claude --plugin-dir ...`, `/plugins`, `/reload-plugins`. Сначала **изолированная проверка каждого хука** через env + прямой запуск exe (заданы ожидаемые JSON-выводы): Orchestration Router → `{"domain":"BUG","subPhases":[...]}`; Skill Selector → `{"skills":[...],"count":3}`; Task Completion Guard capture → `{"captured":true,"requirementCount":3}`; verify (неполный) → `{"complete":false,"missing":[...]}`; Output Validator → `{"valid":false,"violations":[...]}`. Затем те же 5 задач → `~/.xiigen/with-plugin.jsonl`.
4. Фаза 4 — токены из транскриптов: `~/.claude/projects/*/transcripts/*.jsonl` (или `$CLAUDE_TRANSCRIPT_PATH`); `scripts/count-tokens.sh` (wc -c /4 + подсчёт ролей); `scripts/compare.sh` (jq-сравнение baseline vs with-plugin по 5 задачам).
5. Фаза 5 — отладка: `claude --plugin-dir ... --debug` — **в терминале видно строки хуков** вида `[hook] SessionStart: context-injector.exe → exit 0` и `[hook] UserPromptSubmit: orchestration-router.exe → { domain: BUG }` (это готовый источник Path-B-скриншота/лога); `"disabled": true` в hooks.json для точечного отключения; `time ./bin/orchestration-router.exe` — контроль < 300 мс.

**Ожидаемое evidence (Path B Claude):** заполненная итоговая таблица «Задача × Время без/с × Дельта% × Retry без/с»; из hook-log — «скилов загружено (Free) 3 вместо ~18 = **-83% скилов в контексте**», N захваченных требований, N нарушений OutputValidator. Файлы: `~/.xiigen/{hook-log,baseline,with-plugin,token-meter-start,token-meter-stop}.jsonl` + `scripts/{count-tokens,compare}.sh`. Зависит от `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v2-CLAUDE.md`.

### AH. Codex — Path B (local desktop): entrypoint, процедура, evidence

**Точка входа / загрузка (у Codex НЕТ `--plugin-dir`, только через marketplace):**
- `codex marketplace add ./xiigen-token-optimizer` — локальный `marketplace.json` (рекомендован для разработки); также `codex marketplace add github:you/repo` и `... <url>`.
- `codex plugins` — список установленных; `codex marketplace remove xiigen-token-optimizer` — удалить (используется для baseline).
- Локальный `marketplace.json` кладётся в корень плагина: `{"plugins":[{"name":"...","description":"...","path":"."}]}`.

**Real host-entrypoint = hook-события** (SessionStart / UserPromptSubmit / Stop; env `CODEX_SESSION_ID`). Плюс **неинтерактивный раннер** `codex exec --json "<task>"` — запускает задачу и возвращает **точный usage в stdout**: `{session_id, usage:{prompt_tokens, completion_tokens, reasoning_tokens, total_tokens}, result, hooks_fired[]}`.

**Главное преимущество Codex для измерений (самый богатый источник):** (1) `codex exec --json` — точные токены, не оценка; (2) JSONL-транскрипты в `~/.codex/sessions/`; (3) companion-CLI `ccusage` (`npm install -g ccusage`; `ccusage session --breakdown`, `--json`, `ccusage daily --since ... --breakdown` считает и $-стоимость). **Именно на этих Codex-JSON-метриках строится «proven» measurement-контур (Gate B-7 из Pass 4), а не projected-оценки.**

**Процедура:** Фаза 1 — ccusage + HookLogger (`~/.xiigen/codex-hook-log.jsonl`) + `codex exec --json | jq >> codex-token-log.jsonl`. Фаза 2 baseline — `codex marketplace remove` (или `mv hooks.json hooks.json.disabled`), `codex plugins` пуст; скрипт `run-baseline-codex.sh` гоняет 5 задач через `codex exec --json`, jq вытягивает usage → `~/.xiigen/codex-baseline.jsonl`; проверка через `ccusage session --since <дата> --breakdown`. Фаза 3 — установить плагин (`codex marketplace add .`), изолированно проверить каждый хук через `echo '{...}' | ./bin/<hook>.exe` (verify-неполный → **exit code 1**). Фаза 4 — `run-with-plugin-codex.sh` те же 5 задач → `~/.xiigen/codex-with-hooks.jsonl`. Фаза 5 анализ — `codex-compare.sh` (точная дельта токенов), `ccusage session/daily`, hook-log jq, чтение `~/.codex/sessions/` напрямую.

**Ожидаемое evidence (Path B Codex):** таблица «Prompt/Reasoning токены без/с × Дельта%» (точные числа, не оценка); «скилов 3 vs ~18 = -83%», N требований TaskCompletionGuard, N нарушений OutputValidator. Файлы: `~/.xiigen/{codex-hook-log,codex-baseline,codex-with-hooks}.jsonl`, транскрипты `~/.codex/sessions/`, `scripts/{run-baseline-codex,run-with-plugin-codex,codex-compare}.sh`. Зависит от `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CODEX-PORT-v2.md`.

### AI. Cursor — Path B (local desktop): entrypoint, процедура, evidence

**Важное ограничение Cursor:** Cursor **не передаёт token usage в хуки** (задокументированный feature request дек-2025, не реализован). Доступно для замера: `context_tokens` и `context_usage_percent` — только в хуке `preCompact`; размер файлов (`content.length`) — через `beforeReadFile`; время выполнения хуков; косвенные метрики (сообщения, retry, время). Стратегия = **proxy-метрики + контекстные токены из preCompact** (в отличие от точного Codex). Именно поэтому уникальный дополнительный рычаг Cursor — компрессия файлов через `beforeReadFile` (Hook 6, ~20% на C#-файлах), недоступная Claude/Codex.

**Точка входа / загрузка:**
- `.cursor/hooks.json` в корень репо (project-level, рекомендован — версионируется; Cursor подхватывает **автоматически** при открытии папки);
- `~/.cursor/hooks.json` — global-level;
- перезагрузка **автоматическая** — Cursor следит за файлом, рестарт не нужен;
- проверка загрузки: **Cursor Settings → Hooks** (там список активных хуков — готовый Path-B-скриншот).

**Real host-entrypoint = event-хуки в `.cursor/hooks.json`** (формат `version: 1`, объект `hooks`): `preCompact`, `sessionStart`, `beforeSubmitPrompt` (router + skill-selector + guard-capture), `beforeReadFile` (с `matcher: "\\.cs$"` → компрессия C#), `stop` (output-validator + guard-verify). Задачи запускаются как **новый Chat** (Cmd+Shift+L → New Chat). Формат вывода Cursor у guard: `{"permission":"allow"}` / `{"permission":"deny","message":"Task not complete. Missing: ..."}`.

**Процедура:** Фаза 1 — HookLogger (`~/.xiigen/cursor-hook-log.jsonl`) + `context-meter.exe` (preCompact, логирует contextTokens/usagePercent/windowSize/messageCount) + `file-content-compressor.exe` (beforeReadFile, логирует originalChars/compressedChars/reductionPct). Фаза 2 baseline — `mv .cursor/hooks.json .cursor/hooks.json.disabled` (или Cursor Settings → Features → Hooks → выкл.); 5 задач, каждый = новый Chat; вручную → `~/.xiigen/cursor-baseline.jsonl`; зафиксировать размеры читаемых файлов (`wc -c`), транскрипты в `~/.cursor/logs/*/` или `~/.cursor/workspacestorage/*/chat/`. Фаза 3 — `mv ... .disabled → hooks.json`; изолированно проверить каждый хук (включая File Content Compressor: вход с XML-комментами → выход без них; Context Meter preCompact). Фаза 4 анализ — `cursor-compare.sh` (время/retry), **уникальный анализ сжатия файлов** из beforeReadFile (`saved`, `pct`), анализ контекста из preCompact (`context_usage_percent`), анализ скилов.

**Ожидаемое evidence (Path B Cursor):** таблица «Среднее время / retry / C#-символов на сессию / скилов ~18 vs 3-5 (-83%) / context_usage% при компакции»; **уникальная таблица сжатия файлов** «Файл × До × После × Экономия ≈ ~20%». Отладка: `tail -f ~/.xiigen/cursor-hook-log.jsonl`, `jq . .cursor/hooks.json` (валидность), Cursor Settings → Hooks, временный `echo '{"user_message":"DEBUG"}'`. Файлы: `~/.xiigen/{cursor-hook-log,cursor-baseline,cursor-with-hooks}.jsonl`, `.cursor/{hooks.json,hooks.json.disabled,skills/,task-state.json}`. Зависит от `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CURSOR-PORT-v2.md`.

**Сводная таблица различий Path B по хостам (из плана Codex §«Отличия»):**

| Аспект | Claude Code | Cursor | Codex |
|---|---|---|---|
| Установка плагина | флаг `--plugin-dir` | `.cursor/hooks.json` в папку | `codex marketplace add` |
| Отключение для baseline | `disableAllHooks: true` | переименовать hooks.json | `codex marketplace remove` |
| Данные о токенах | из транскрипта ~4 chars/token (оценка) | proxy + `context_tokens` из preCompact | `codex exec --json` — **точные** |
| Инструмент анализа | ручной / ccusage | hook-log + ручной | ccusage companion |
| Перезагрузка хуков | `/reload-plugins` | автоматически (watch) | `codex marketplace add .` повторно |

### AJ. Desktop-Adaptation-Report v2.0 — sidecar/installer-архитектура, VS Code/Visual Studio, build order, blockers

Отчёт (v2.0, «Confidential», май-2026) маппит **14 core-движков × 19 desktop-платформ = 266 слотов адаптации**; 16 из 19 платформ — запуск exe подтверждён/тривиален. Это не про 3 CLI-хоста, а про широкую desktop-экспансию, но он даёт **фундамент, на котором стоит continue-план VS Code/Visual Studio**: sidecar-архитектуру, installer-паттерн и порядок сборки.

**Самое важное архитектурное решение (единственное, выделено в отчёте):** построить **ОДИН Local XIIGen Agent executable**, публиковать **один тонкий адаптер на платформу**. Вся интеллектуальность — в .NET Core. Адаптер взаимозаменяем и несёт **ноль бизнес-логики** (совпадает с R6 «Simple Agent Boundary» и границей `llm_mvp_core` из CLAUDE.md).

**Installer Pattern (Section 2, критическое предупреждение — governance для Path B):** НЕ «плагин молча скачивает и запускает произвольный exe» — это флагнёт review любого крупного маркетплейса (Adobe, Autodesk, Visual Studio, JetBrains). Правильный **двухшаговый** паттерн: `marketplace plugin активируется → проверяет, установлен ли Local XIIGen Agent → если нет: открывает installer-страницу на xiigen.com → пользователь ЯВНО ставит подписанный desktop-agent → agent поднимает local HTTP service на localhost (напр. порт 57432) → plugin коннектится по HTTP / WebSocket / named pipe → plugin остаётся тонким и marketplace-compliant`. Прецеденты: OmniSharp (VS Code), Claude Code, любой language server. Строже всего: Adobe UXP, Autodesk, Unity, SketchUp, ArcGIS, OBS. Мягче всего (dev-tools уже ждут локальные CLI/LSP/Docker/agents): **VS Code, JetBrains, Raycast, Obsidian, n8n**.

**VS Code (Tier 1, benchmark-платформа):** runtime = Node.js extension host, `child_process.spawn` запускает любой binary; .NET Core либо ship рядом, либо download при первой активации; **review gate: НЕТ**. OmniSharp и Claude Code используют паттерн «spawn a .NET binary». Один `.vsix` = тот же extension host, что у Cursor и Windsurf. **Cursor:** идентичный extension host, тот же `.vsix`, тот же exe, автоматический второй канал через VS Code Marketplace — **zero incremental cost**. **Windsurf / Open VSX:** VS Code-совместимый host; публикация в Open VSX (Eclipse Foundation) покрывает Cursor/Windsurf/VSCodium/Theia почти бесплатно.

**Visual Studio (full IDE) — архитектурно чистейшая из всех платформ:** extension = .NET-сборка (C#/VB.NET), `System.Diagnostics.Process.Start()` нативен; .NET Core **уже стоит на каждой VS-машине → zero distribution concern**; запуск .NET Core child-процесса = одна строка; новая модель **`VisualStudio.Extensibility` явно спроектирована под out-of-process/sidecar-паттерн**; review — light. Это прямое обоснование, почему continue-план кладёт Visual Studio ранней волной.

**Shared Architecture (Section 8) — контур, который проверяет Path A и запускает Path B:** протокол связи = **NDJSON (newline-delimited JSON)**, один JSON-объект на строку, без framing-оверхеда. Цикл .NET Core: `await foreach (line in Console.In.ReadLineAsync) { req = Deserialize(line); resp = await ProcessAsync(req); Console.WriteLine(Serialize(resp)); }`. Адаптеры по языкам: TypeScript (VS Code/Obsidian/Raycast/Adobe CEP — `spawn` + readline), Kotlin (JetBrains — `ProcessBuilder`), **C# (Visual Studio/Autodesk/ArcGIS — `Process.Start`)**, Python (Blender/QGIS — `subprocess.Popen`), Ruby (SketchUp — `Open3`). «One binary. One protocol. Five adapter languages.» Installer-паттерн: binary ставится подписанным installer'ом с xiigen.com, плагин проверяет наличие агента на активации и редиректит на installer при отсутствии.

**Build Order (Section 7) — прямая дорожная карта для continue-плана:**
- **Wave 1 (недели 1–4): один build → 4 платформы** (VS Code, Cursor, Windsurf, Open VSX из одного `.vsix`). Первые движки: AI Diagram Generator (Mermaid нативен в VS Code — сильнейший engine-platform fit во всей матрице), Content Populator, Meeting Summary, FLOW Engine, Alt-Text.
- **Wave 2 (недели 3–6, параллельно): Visual Studio** — кратчайший адаптер (extension уже .NET, `Process.Start()` = одна строка). Первые движки: Content Populator (XML-doc комментарии), Multilingual (`.resx`), Status Report (Azure DevOps), FLOW, AI Diagram.
- Далее: Wave 3 JetBrains/Rider, Wave 4 Raycast, Wave 5 Obsidian (Mermaid нативен), Wave 6 Adobe CEP, Wave 7 Autodesk, Wave 8 Rhino/GH, Wave 9 Blender, Wave 10 Unity/SketchUp/Unreal/OBS/QGIS/ArcGIS.

**Blockers/findings desktop-отчёта:**
- **Zed Editor:** runtime = WASM-песочница (Rust) — **не поддерживает произвольный subprocess**; плагин не может запустить exe; только связь по HTTP с ЗАРАНЕЕ установленным сервисом. Watchlist, переоценка Q4-2026.
- **Adobe UXP (Photoshop/Illustrator):** `spawn` напрямую не поддерживается → workaround: companion helper-app на локальном сокете через UXP file-system API. (After Effects/Premiere = CEP, там Node.js + spawn работают.)
- **Unreal:** C++, .NET runtime надо отдельно managed → heavier адаптация, после валидации Unity.
- **OBS:** discovery слабее, формального маркетплейса-поиска нет — community-distribution, не marketplace-SEO.
- Watch List: Zed, Penpot (Q3-2026), Lucid (Q4-2026), Beehiiv, HubSpot, DaVinci Resolve (Q2-2026, OpenFX/C++), Xcode (Q3-2026, строгий Mac App Store review), Google Stitch.

### AK. Как это ложится в Path B нового continue-плана + РАЗРЫВ + остаток на passes 6–10

**Маппинг на Dual-Path Same-Scenario Coverage:**
- **Path B (local desktop) = эти три local-testing плана.** Готовый blueprint: как поднять реальный хост, выключить плагин (baseline), прогнать один и тот же сценарий, включить плагин, снять evidence. Один и тот же набор из 5 задач (BUG/PLAN/ARCH/CODE_Q/REFACTOR) = удобная «same-scenario» ось для сравнения baseline vs plugin.
- **Path A (automated boundary)** для этого же контура = прямой запуск exe (`echo '{...}' | ./bin/<hook>.exe`, ожидаемые JSON-выводы из фаз изолированной проверки) + Codex `codex exec --json` (точные токены). То есть в этих планах Path A и Path B уже прописаны параллельно для одного сценария — это готовая «Path A + Path B» единица.
- **Evidence-моменты Path B по хостам:** Claude — `claude --debug` строки `[hook] ... → exit 0` в терминале (скриншот/лог) + `~/.xiigen/*.jsonl`; Codex — `codex exec --json` stdout + `ccusage` + `~/.codex/sessions/`; Cursor — **Cursor Settings → Hooks** (скриншот списка активных хуков) + `~/.xiigen/cursor-hook-log.jsonl` + таблица сжатия файлов.

**РАЗРЫВ (что Path B нового плана НЕ покрывает и надо добить):**
1. **Нет выделенного local-testing плана для «настоящих» хостов continue-плана — VS Code (proper) и Visual Studio.** Три плана покрывают Claude Code (CLI-хуки), Codex (CLI-marketplace) и Cursor (Extension Host хуки). Ближайший переносимый Path B для VS Code-расширения — **Cursor-план** (Cursor = форк VS Code, тот же Extension Host), но он про хуковую модель, а не про панель/команду.
2. **Хуковая модель ≠ панельное расширение.** Эти планы тестируют token-optimizer HOOK-exe; реальные host-репо (Pass 1/4/6) — панельное расширение с командой `xiigenSmartSkills.panel` (Extension Host command). Для панели Path B нужен вызов команды из Command Palette / клик по панели + скриншот панели + trace/correlation — эти шаги в текущих 3 планах **отсутствуют** (разбор реального `extension.ts` и его команд — задача Pass 6).
3. **Транспорт: реальность vs отчёт.** Desktop-отчёт предлагает NDJSON поверх child-process stdin/stdout ИЛИ local HTTP service (installer-паттерн); реальные репо (Pass 1/4) пошли через PowerShell-entrypoint + единый `xiigen exe` core-API. Path B continue-плана должен проверять фактический транспорт, а не идеальный NDJSON из отчёта.
4. **Savings всё ещё projected, не proven** (подтверждает Pass 4): в самих таблицах local-testing — плейсхолдеры (`xs`, `-N%`, `N`); реальные числа появятся только после прогона. «Proven»-контур = Codex `codex exec --json` (Gate B-7). До прогона это Path B **blueprint**, не Path B **evidence**.

**Ряды доски из этого прохода (Path B):** (a) написать выделенный **VS Code Extension-Host Path B** (Command Palette → `xiigenSmartSkills.panel`, скриншот панели, trace) — по образцу Cursor-плана, но для команды/панели, не для хуков; (b) написать **Visual Studio Path B** (Wave 2: `.vsix`/Experimental Instance, tool-window/command, `Process.Start` sidecar) — отчёт называет VS архитектурно чистейшей; (c) закрепить **installer/sidecar-паттерн** (двухшаговая установка + localhost HTTP/named pipe) как governance-требование Path B для marketplace-совместимости; (d) для «proven» savings — гнать Codex `codex exec --json` как источник Path A-чисел, не projected-таблицы.

**Остаток на passes 6–10** (по распределению из Pass 1): Pass 6 — `cursor-xiigen-smart-skills/src/extension.ts` (1628 строк, реальный thin-adapter + команда `xiigenSmartSkills.panel`) + reports; Pass 7 — claude/codex `*-entrypoint.ps1` (по 246 строк PowerShell) + phase40-трейсы + smoke; Pass 8–9 — FigmaToCode (design→code + skill-advisor); Pass 10 — `other/` marketplace-research + СИНТЕЗ + финальная сверка плана↔`43.SmartSkillsDomain`.

**Метаданные Pass 5/10:** RAG-файл дополнен разделами AG–AK (append-only). Прочитаны deep read-only целиком: `PLAN-LOCAL-TESTING-CLAUDE-CODE-v1.md` (404), `PLAN-LOCAL-TESTING-CODEX-v1.md` (482), `PLAN-LOCAL-TESTING-CURSOR-v1.md` (450), `XIIGen-Desktop-Plugin-Adaptation-Report-v2.md` (973). Ранние секции (Pass 1–4, A–AF) не тронуты. Главный вывод: три плана — это **готовый Path B blueprint для хуковой модели** (entrypoints: Claude `--plugin-dir` + hook-события; Codex `marketplace add` + `codex exec --json`; Cursor `.cursor/hooks.json` + Extension Host), с параллельным Path A (прямой exe / точный Codex-JSON); desktop-отчёт даёт sidecar/NDJSON/installer-архитектуру и build order (VS Code Wave 1, Visual Studio Wave 2 как чистейшая .NET-платформа). Ключевой разрыв: нет выделенного Path B для VS Code-панели/Visual Studio и для панельной команды `xiigenSmartSkills.panel` — это добивается в Pass 6 + отдельными VS/VSCode Path B слайсами.

## Pass 6/10 — cursor-xiigen-smart-skills: реальный extension.ts + manifest + panel + core-wiring + R6 thin-adapter проверка

Источник: `C:/Users/lubao/Documents/xiigen/extensions reasearch/cursor-xiigen-smart-skills/`.
Прочитано целиком: `src/extension.ts` (1629 строк), `package.json` (152 строки),
`README.md`, `xiigen-extension-contract.json`, `.cursor/xiigen-smart-skills.json`,
`scripts/debug-trace-helper.js`, `scripts/local-runtime-helper.js`; спот-чек `dist/extension.js`,
`reports/*`, `scripts/install-smoke.ps1`, `test/`.

Cursor = форк VS Code (`engines.vscode: ^1.90.0`), поэтому этот репозиторий — самая близкая
существующая модель для VS Code-расширения, которое продолжает новый план. Это реальный код,
а не скелет: `src/extension.ts` + скомпилированный `dist/extension.js` работают идентично.

### 1. Манифест расширения (package.json — VS Code/Cursor contributes)

- `name`: `cursor-xiigen-smart-skills`; `displayName`: "XIIGen Smart Skills for Cursor";
  `publisher`: `xiigen`; `main`: `./dist/extension.js`; `license`: UNLICENSED; `version`: `0.0.0-xiigen-wiring`.
- `activationEvents` (4): `onCommand:xiigen.smartSkills.select`, `onCommand:xiigenSmartSkills.panel`,
  `onCommand:xiigenSmartSkills.runtimeProof.invoke`, `onChatParticipant:xiigen-smart-skills`.
- `contributes.commands` (3 видимых пользователю):
  - `xiigen.smartSkills.select` — "XIIGen: Select Smart Skill" (основная рабочая команда);
  - `xiigenSmartSkills.panel` — "XIIGen: Open Smart Skills Panel" (webview-панель);
  - `xiigenSmartSkills.runtimeProof.invoke` — "XIIGen: Runtime Proof Harness (Test Only)".
- `contributes.chatParticipants`: id `xiigen-smart-skills`, name "XIIGen Smart Skills" — chat-участник
  (тот же путь, что Copilot/Cursor chat participant API).
- `contributes.configuration` (settings) — все host-neutral, ничего семантического:
  - `xiigenSmartSkills.coreRoot` (default `C:\Projects\llm_mvp_core`);
  - `xiigenSmartSkills.coreApiScript` (default `...\tools\xiigen-extension-core-api.ps1`) — «core-owned XIIGen API script; Cursor обязан звать core, а не встраивать семантику/skill-selection»;
  - `xiigenSmartSkills.promptSuite`, `xiigenSmartSkills.traceReport`;
  - `xiigenSmartSkills.debug` (boolean, default false) — **programmer-debug setting**: «Enable programmer debug trace showing the redacted xiigen exe method/input/output payload»;
  - QSG target-locator: `targetLocatorPath`, `targetLocatorJson`, `userAdaptiveAssetRoot`, `rollbackAssetRoot`, `locatorMode` (enum `target`|`rollback`, «llm_mvp_core owns interpretation»).
- Приватный блок `xiigenSmartSkills` в package.json объявляет самоограничения (это же — R6-контракт):
  `hostKind: "Cursor"`, `thinClientOnly: true`, `hostSemanticKitchenAllowed: false`,
  `externalAiOrUnrelatedToolsAllowed: false`, `commonAssetsOwnedByCore: true`,
  `adaptiveStateOutsideHostPackage: true`, `runtimeEvidence: false`, `fullHostImplementationEvidence: false`,
  плюс `programmerDebugSetting` -> helper `scripts/debug-trace-helper.js`.

### 2. Что реально делает extension.ts (activate() + транспорт до core)

`activate(context)` регистрирует НЕ 3, а 6 команд/хендлеров:
1. `xiigen.smartSkills.select` — если промпт не передан программно, показывает `vscode.window.showInputBox`;
   иначе берёт `providedPrompt.prompt/method/requestedSkills/availableSkills`; вызывает
   `runCursorSmartSkillsRequest(...,"command",...)`; при интерактивном вводе показывает `showInformationMessage`.
2. `xiigenSmartSkills.runtimeProof.invoke` — тест-only рантайм-пруф (positive/negative scenario).
3. chat-participant `xiigen-smart-skills` — регистрируется ТОЛЬКО если доступен `vscode.chat.createChatParticipant`
   (feature-detect через каст `vscode as unknown`); хендлер -> `handleCursorChatParticipantRequest` -> `stream.markdown(...)`.
4. `xiigenSmartSkills.__test.chatHandlerLevel` — тест-only адаптер уровня хендлера чата (не полный chat-UI).
5. `xiigenSmartSkills.__test.panelSubmitHandler` — тест-only адаптер submit-хендлера панели (handler-path only).
6. `xiigenSmartSkills.panel` — создаёт/ре-открывает webview-панель.

Общий рабочий конвейер — `runCursorSmartSkillsRequest()` (единый для command/chat/panel/runtime-proof):
читает config -> строит `XiigenExtensionRequest` (`buildCoreRequest`) -> пишет request.json ->
`invokeCoreApi()` -> нормализует ответ (`normalizeResponse`) -> строит display-текст -> пишет
entrypoint-response, host-trace, (для runtime-proof) redacted-trace и `xiigen-cursor-host-runtime-latest.json`.

**Транспорт до core = PowerShell entrypoint, НЕ прямой `xiigen exe` и НЕ HTTP.** `invokeCoreApi`:

    spawn("powershell.exe", ["-NoProfile","-ExecutionPolicy","Bypass","-File", coreApiScript,
      "-RequestPath", requestRef, "-ResponsePath", responseRef, "-TracePath", coreTraceRef,
      "-CoreRoot", coreRoot], {windowsHide:true})

Обмен через файлы JSON: хост пишет request.json, PS-скрипт core пишет response.json + trace.json,
хост читает response обратно (`readJsonFile`, снимает BOM). `coreApiScript` по умолчанию —
`C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`. Если скрипта нет -> `buildCoreMissingResponse`
(`CORE_ENDPOINT_MISSING`, `CONTRACT_ONLY_DOWNGRADE`, `passed:false`); ошибка spawn -> `CORE_API_SPAWN_ERROR`;
нет файла ответа/битый JSON -> `CORE_API_RESPONSE_MISSING`/`..._PARSE_FAILED`. Все негативы записывают
negative-artifacts (`writeCoreNegativeArtifacts`). Термин «xiigen exe» из контракта — концептуальное имя
core-API; фактическая обёртка = PS-скрипт (совпадает с находкой Pass 5: транспорт в реальных репо =
PowerShell entrypoint + xiigen exe).

Метод нормализуется в один из трёх лейблов: `prompt-to-task` (default) | `orchestration-plan` |
`skill-selection` (`normalizeMethod`) — это лишь метка, передаваемая core; host решения НЕ принимает.

### 3. Panel/Webview UI

`xiigenSmartSkills.panel` -> `vscode.window.createWebviewPanel("xiigenSmartSkills.panel","XIIGen Smart Skills",
ViewColumn.Active, {enableScripts:true, enableCommandUris:false, localResourceRoots:[]})`. Синглтон
`smartSkillsPanel`; повторный вызов — `reveal()` + фокус на textarea. HTML строит `buildPanelHtml`:
CSP с nonce (`default-src 'none'; style-src/script-src 'nonce-...'`, nonce = `crypto.randomBytes(24)` base64url),
тема через `--vscode-*` переменные, `<textarea id="xiigen-smart-skills-prompt">`,
`<button id="xiigen-smart-skills-submit">Submit to llm_mvp_core</button>`, aria-live results, response/trace refs.
Внутри — `acquireVsCodeApi()`, при клике постит `{type:"xiigenSmartSkills.submitPrompt", prompt, metadata}`.
Хост-сторона: `panel.webview.onDidReceiveMessage` -> `handleTrustedPanelSubmitMessage` ->
(валидирует type/непустой промпт) -> `runCursorSmartSkillsRequest(...,"panel-webview-submit",...)` ->
постит `submitResult` обратно в webview + пишет `writePanelUiArtifact`. **Открытие панели само по себе НЕ
зовёт core** (`panelOpenOnly:true`, `corePathInvoked:false`) — core-запрос только по Submit. HTML экранируется
(`escapeHtml`, `serializeForHtmlScript`), внешних скриптов нет.

### 4. Request/Response wiring (контракты)

`XiigenExtensionRequest`: `schemaVersion:1`, `HostName:"cursor-xiigen-smart-skills"`, `HostType:"Cursor"`,
`Method`, `UserPromptOrTaskText`, `HostFacts` (host_entrypoint, core_root/script, все refs, флаги
`host_semantic_ranking:false`/`host_training:false`/`host_corpus_parsing:false`/`external_ai:false`,
`host_skill_selection_binding:"pass_through_only"`, счётчики requested/available skills, target-locator поля),
`DebugEnabled`, `RequestedSkills[]`, `AvailableSkills[]`, `CheckSetInputs` (текстовые check-set-ассерты про
boundary/anti-masquerade/QSG19), `Provenance` (host_repo, dist/src entrypoints, core_endpoint, контракты),
`CorrelationId`, `TargetLocator?`. Ответ core нормализуется в `XiigenCursorHostEntrypointEvidence`
(`normalizeResponse`): `passed` = `response.passed===true || Success===true`; проставляет
`adapterFunctionalityPassed`, `coreBoundaryReached`, `sameRunCoreCorrelationProven` (сверка CorrelationId
хоста и core), и жёстко фиксирует **`trueHostRuntimeEvidence:false`, `fullCursorExtensionHostUiProof:false`,
`semanticQualityClaimed:false`, `hostProvidedSemanticDecision:false`, `externalAiOrUnrelatedToolUsed:false`**.

### 5. Debug-trace surface

Setting `xiigenSmartSkills.debug` (default false). При включении `buildCursorDisplayText` дописывает к ответу
блок `xiigen <Method> :` + `input: <redacted>` + `output: <redacted>`; `buildXiigenDebugTrace` кладёт shape в
`normalized.debugTrace` и host-trace (`rawPrivateContentExposed:false`, `secretsExposed:false`). В chat/UI сырой
приватный ввод/вывод не показывается — только `<redacted>` и метод/статус/correlationId. Есть отдельный
standalone helper `scripts/debug-trace-helper.js` (CLI `--method/--debug-enabled/--output`) и
`scripts/local-runtime-helper.js` (spawnSync того же PS core-api из Node, для локального прогона без Cursor).

### 6. R6 thin-adapter verdict — протекает ли логика в расширение?

**Вердикт: НЕТ, ядровая логика не протекает — адаптер верен thin-client дизайну (R6/R5/R7 соблюдены).**
Доказательства в коде: (a) весь «выбор» делегирован PS-скрипту core через файловый JSON-обмен; (b)
`RequestedSkills`/`AvailableSkills` только `normalizeStringArray` (trim/фильтр пустых) и передаются насквозь,
`hostSelectorOrScorerAdded:false`, `host_skill_selection_binding:"pass_through_only"`; (c) нет BM25/regex/
словаря/скоринга/тренировки в хосте; (d) нет внешнего AI (`external_ai:false`), нет сетевых вызовов; (e)
общие ассеты — за core (`commonAssetsOwnedByCore:true`), адаптивное состояние — вне пакета
(`adaptiveStateOutsideHostPackage:true`). Единственная «логика» хоста — нормализация метки метода в один из
трёх лейблов и оформление evidence/trace, что не является продуктовым решением.

Нюанс объёма: файл ~1629 строк, но >80% — это evidence/trace-машинерия (negative-artifacts, host-trace,
redacted-trace, panel-ui/handler-only артефакты, correlation-сверки). Настоящий thin-adapter (activate +
runCursorSmartSkillsRequest + invokeCoreApi + panel html) занимает малую часть; остальное — доказательная
балласт-обвязка. Для нового VS Code-плана: брать конвейер activate->request->PS-core->normalize->display,
а тяжёлую evidence-обвязку резать до минимума.

### 7. Находки/риски (для верификатора и плана)

- **Латентный баг**: в `writeRedactedTrace(filePath,result,options)` (src ~строка 1225; в `dist` строки 927 и
  1055) в ветке `options.evidenceMode==="test-only-handler-path"` используется `traceRefs.hostHandlerOnlyRef`,
  но `traceRefs` НЕ является параметром функции и не в её области видимости -> ReferenceError, если ветка
  выполнится. Ветка мёртвая при штатных путях (`writeRedactedTrace` зовётся только при
  entrypoint===runtimeProofCommand, а `test-only-handler-path` ставится на panel-submit-тесте), поэтому баг
  скрыт, но он проходит и в скомпилированный `dist/extension.js`. Для нового кода — исправить/удалить.
- **Дрейф манифестов**: старый `.cursor/xiigen-smart-skills.json` (`schemaVersion:"phase40.cursor.manifest.v1"`,
  `manifestSkeletonOnly:true`, `nextBoundedAction:"P40-WO-04b only"`, `runtimeEvidence:false`) указывает на
  `Phase40ExtensionContracts.cs` и `phase40-extension-core-api.ps1`, тогда как актуальные package.json/README/
  `xiigen-extension-contract.json` указывают на `XiigenExtensionApiContracts.cs` и `xiigen-extension-core-api.ps1`.
  То есть `.cursor`-манифест устарел относительно уже реализованного extension.ts.
- **Честность доказательств (совпадает с Pass 5)**: контракт `runtime_host_proof_performed:false`,
  `host_prompt_to_task_implementation:false`, `host_orchestration_reasoning_implementation:false`; README прямо
  говорит «does not claim full Cursor Extension Host/UI runtime proof». Спот-чек reports/: даже в папке
  `RUNTIME-03-cursor-live-runtime` по всем файлам `fullCursorExtensionHostUiProof:false` (~146 раз false против
  ~10 true — «true» относится к adapter/core-boundary evidence, не к полному UI). Path B (реальный Cursor-хост,
  видимый webview, ввод в textarea) НЕ доказан — есть только `test-only-handler-path` и handler-level QA
  (`HOST-08B/08C/08D/08H`, каждый `claim_scope: "...QA automation only; not final...not semantic quality"`).
- **Реальный vs балласт**: реальная референс-реализация = `src/extension.ts` + `dist/extension.js` + 3 скрипта
  (`install-smoke.ps1`, `debug-trace-helper.js`, `local-runtime-helper.js`) + `test/{host-qa,phase10,smoke}`.
  Балласт-доказательства = десятки `phase40-cursor-*.json` в корне, 6 папок
  `phase40-cursor-approved-host-runtime-rerun-*` и всё `reports/` (CU-01..04, HOST-08*, RUNTIME-03*, TOKEN-03*).

### 8. Что остаётся на проходы 7–10

- Pass 7: `test/` (host-qa/phase10/smoke) + `scripts/install-smoke.ps1` целиком — какой именно контракт
  проверяется и что считается «pass» на handler-уровне (донабрать точную QA-модель для VS Code-плана).
- Pass 8: `docs/PROJECT-DOC.md` (32 КБ) — заявленная детальная арх-документация адаптера; сверить с кодом.
- Pass 9: core-сторона в llm_mvp_core (`43.SmartSkillsDomain/XiigenExtensionApiContracts.cs`,
  `tools/xiigen-extension-core-api.ps1`) — контракт с точки зрения ядра (границы, что реально считает core).
- Pass 10: синтез — перенос thin-adapter спайна на VS Code-расширение нового плана: активити-эвенты, команда
  панели, chat-participant, PS-core транспорт, debug-setting; список того, что резать (evidence-балласт) и
  что чинить (traceRefs-баг, дрейф `.cursor`-манифеста, отсутствующий Path B).

---

## Pass 7/10 — claude + codex host-adapters + 3-way host-diff + унифицированный host-adapter контракт (шаблон для VS Code/Visual Studio)

Прочитано deep этим проходом (всё read-only, кроме этого RAG-файла):
- `claude-xiigen-smart-skills/src/claude-entrypoint.ps1` (247 строк) полностью; `.claude-plugin/plugin.json`; `hooks/hooks.json`; `README.md`; `package.json`; `prompts/claude-smoke-prompts.jsonl`; спот-чек трейсов `phase40-claude-entrypoint-trace-1.json`, `phase40-claude-core-request.json`, `phase40-claude-core-response.json` (60 строк), `phase40-claude-host-session-run.command.json`, `phase40-claude-host-session-run.stdout.log`, `phase40-claude-check-set-1.json`.
- `codex-xiigen-smart-skills/src/codex-entrypoint.ps1` (247 строк) полностью; `.codex/xiigen-smart-skills.json`; `README.md`; `package.json`; `prompts/codex-smoke-prompts.jsonl`; спот-чек `phase40-codex-host-session-run/entrypoint-trace.json`, `phase40-codex-host-session-run-4.stdout.jsonl`, `phase40-codex-host-session-run-4.last-message.txt`, `phase40-codex-check-set-2.json`.
- Сверка ядра (read-only): `C:\Projects\llm_mvp_core\tools\` — подтверждено существование ОБОИХ core-api скриптов (`phase40-extension-core-api.ps1` = 357 строк; `xiigen-extension-core-api.ps1` = 274 строки).

### L. Claude host-adapter — entrypoint + manifest + hook-wiring

**L.1 Entrypoint `src/claude-entrypoint.ps1` (7356 байт, 247 строк).** Чистый PowerShell thin-transport, БЕЗ семантической логики. Параметры: `-ConcreteTask` (Mandatory), `-Prompt`, `-CheckSetPath`/`-CheckSetJson`, `-CoreRoot` (default `C:\Projects\llm_mvp_core`), `-CoreApiScript`, `-OutputPath`, `-TracePath`, `-CommonAssetRef` (default `llm_mvp_core:phase40-common-skill-selection-set`), `-AdaptiveAssetRef` (default `claude-host:runtime-adaptive-ref-not-owned-by-extension`). `Set-StrictMode 2.0`, `$ErrorActionPreference=Stop`.

Хелперы (только механика): `Resolve-Phase40Path` (rooted → as-is, иначе `GetFullPath(Join-Path base rel)`), `Ensure-Phase40ParentDirectory` (mkdir -Force), `Read-Phase40CheckSetItems` (парс JSON: массив → as-is, иначе поля `checkSetItems`/`items`), `Write-Phase40Json` (`ConvertTo-Json -Depth 24 | Set-Content -Encoding UTF8`).

Поток (строки 102–246): `scriptRoot`→`extensionRoot`(родитель src)→резолв `CoreRoot`/`Output`/`Trace`; `coreRequestRef`/`coreResponseRef` кладутся в trace-директорию; default `CoreApiScript = <CoreRoot>\tools\phase40-extension-core-api.ps1`; читает check-set; собирает `$request` (см. L.3); `Write-Phase40Json request → coreRequestRef`; **IF `Test-Path coreApiScript`** → `& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $coreApi -RequestPath $coreRequestRef -ResponsePath $coreResponseRef -TracePath $trace`, ловит `$LASTEXITCODE`; `status=core_invocation_completed`,`passed=true`,`runtimeEvidence=true`,`coreApiAvailable=true` (или `core_invocation_failed`/`passed=false`, если exit≠0); **ELSE** → честный `status=contract_only_downgrade`,`passed=false`,`runtimeEvidence=false`,`coreApiAvailable=false`,`message`+`nextRequiredEvidence`. Пишет `$trace` (`contractOnlyDowngrade`,`externalAiUsed=false`,`hostSemanticDecisionAuthority=false`) и `$response→outputPath`. **`exit 0` если passed, иначе `exit 2`** — код выхода = гейт (то, что читает Codex/hook).

**L.2 Manifest `.claude-plugin/plugin.json` — это и есть НАСТОЯЩАЯ hook-проводка.** Схема Claude Code plugin: `name`,`displayName`,`version="0.0.0-phase40-skeleton"`,`description`,`author`, и `hooks.SessionStart[0].hooks[0]` = `{type:"command", command:"powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"<абсолютный путь>\\src\\claude-entrypoint.ps1\" -ConcreteTask \"...SessionStart non-AI component probe\" -Prompt \"...\" -CheckSetJson \"[]\" -CoreRoot \"C:\\Projects\\llm_mvp_core\" -OutputPath \"phase40-claude-plugin-component-response.json\" -TracePath \"phase40-claude-plugin-component-trace.json\"", timeout:60, statusMessage:"Xiigen Smart Skills Claude hook component"}`. Событие = **`SessionStart`** (lifecycle-hook, не пользовательская команда) — fires при старте сессии Claude Code.

**L.3 `$request` (контракт `Phase40ExtensionApiRequest`, schemaVersion 1)** — общий для обоих хостов, различия помечены `[Claude/Codex]`: `hostKind`=`[Claude/Codex]`; `evidenceScope`=`[user_facing_claude_hook / user_facing_codex_command]`; `entrypoint{type:[hook/command], name:"xiigen.smartSkills.select", file:"src\\*-entrypoint.ps1"}`; `core{root, contractFile:"43.SmartSkillsDomain\\Phase40ExtensionContracts.cs", apiScript, clientMode:"thin_api_check_set_client", coreOwnsSkillSelection:true, hostDecisionAuthority:false}`; `skillSelectionRequest{requestType:"SkillSelectionRequest", hostKind, concreteTask, prompt, checkSetItems}`; `assetReferences{common/adaptive, commonOwnedByCore:true, adaptiveOwnedByHostOrProject:true, separationRequired:true}`; `decisionEvidence{requiredDecisionSource:"llm_mvp_core_trainable_or_core_api", hostSemanticRankingAllowed:false, deterministicStaticDecisionAuthorityAllowed:false, deterministicMasqueradeProhibited:true}`; `externalToolEvidence{externalAiUsed:false, imageGenerationUsed:false, unrelatedToolUsed:false, approvalRef:null}`.

**L.4 ДЕФЕКТ: `hooks/hooks.json` = ПУСТОЙ `{"hooks":{}}`.** README называет его «Claude Code hooks manifest», но реальный hook живёт в `plugin.json`. Т.е. два hook-манифест-файла, провод несёт только `plugin.json`; `hooks/hooks.json` — рудимент. Для continue-плана: выбрать ОДИН дом манифеста на хост.

### M. Codex host-adapter — entrypoint + manifest + command-wiring

**M.1 Entrypoint `src/codex-entrypoint.ps1` (7348 байт, 247 строк) — БАЙТ-ПОЧТИ-ИДЕНТИЧЕН claude-entrypoint.** Отличается ТОЛЬКО в 6 местах: `hostKind="Codex"`; `evidenceScope="user_facing_codex_command"`; `entrypoint.type="command"`; `AdaptiveAssetRef` default = `codex-host:runtime-adaptive-ref-not-owned-by-extension`; default `OutputPath`/`TracePath` имена (`phase40-codex-*`); текст `message`. Вся механика (хелперы, поток, коды выхода 0/2, core-call) — одна и та же. Это ключевой факт: **host-адаптер = один шаблон, инстанцированный на хост через ~6 полей.**

**M.2 Manifest `.codex/xiigen-smart-skills.json` — своя (bespoke) схема, БОГАЧЕ claude.** `schema_version:1`, `name`,`display_name`,`version`,`description`, + блоки: `phase40{work_order:"P40-WO-03a", host_kind:"Codex", scope:"manifest-package-skeleton-only", implementation_status:"skeleton_only", runtime_evidence:false, full_implementation_evidence:false, install_smoke_status:"not_run", entrypoint_implemented:false}`; `entrypoint{type:"codex_command", name/command:"xiigen.smartSkills.select", implementation_file:"src/codex-entrypoint.ps1", implemented_in:"P40-WO-03b"}`; `core{root, contract_file, client_mode:"thin_api_check_set_client", must_call_core_api:true, must_not_embed_selection_logic:true}`; `asset_boundary{common_model_manifest_ref:"llm_mvp_core:models/skill-selection/common/manifest.json", common_training_data_manifest_ref:"llm_mvp_core:training-data/skill-selection/common/training-manifest.json", adaptive_state_root_ref:"codex-host:adaptive-state-runtime-root-outside-package", common_assets_repo_owned:true, adaptive_state_separated_from_host_package:true}`; `prohibitions{host_semantic_ranking_allowed:false, host_training_allowed:false, host_validation_completion_kitchen_allowed:false, external_ai_or_image_generation_allowed:false, unrelated_tool_use_allowed:false, deterministic_static_decision_authority_allowed:false}`. Событие = **команда `xiigen.smartSkills.select`**, вызывается через `codex exec` shell (модель Codex CLI, НЕ lifecycle-hook).

**M.3 КЛЮЧЕВОЕ различие манифестов (Claude vs Codex).** Codex кодирует границу ядро/хост, prohibitions и asset-split ПРЯМО В МАНИФЕСТЕ. Claude так НЕ может: Claude Code игнорирует неизвестные поля `plugin.json` при загрузке, поэтому те же ограничения вынесены в `README.md` + core-план (README прямо это объясняет). **Один контракт — два места хранения, выбор диктует терпимость схемы хоста к кастомным полям.** Это прямой урок для VS Code (`package.json` — тоже игнорирует чужие поля, как Claude) и Visual Studio (`.vsixmanifest` — строгая схема).

### N. Реальные host-session-run доказательства (сильнее, чем у Cursor Pass 6)

**N.1 Claude.** `phase40-claude-host-session-run.command.json`: реальный `"...\claude-code\2.1.160\claude.exe" --plugin-dir "<repo>" --print "Phase40 Claude SessionStart hook smoke. Print XIIGEN_PHASE40_SMOKE only."`, `exit_code:0`, `elapsed:22.2s`, `stdout="XIIGEN_PHASE40_SMOKE"`. Т.е. реальный Claude Code host загрузил plugin-dir. **Оговорка честности:** stdout доказывает лишь echo маркера моделью; отдельное доказательство срабатывания SessionStart-хука = `phase40-claude-plugin-component-response.json`; `claude plugin details` признаёт компонент как один hook (README). Entrypoint-trace: `contractOnlyDowngrade:false` (core-api найден и вызван), `hostSemanticDecisionAuthority:false`.

**N.2 Codex.** `phase40-codex-host-session-run-4.stdout.jsonl` — реальный `codex exec` thread (JSONL-события): `thread.started` → `item.started/completed`(`command_execution` запускает `powershell.exe -File .\src\codex-entrypoint.ps1 -ConcreteTask "P40-WO-07f Codex CLI host session smoke" ...`, `exit_code:0`) → `agent_message text:"Shell exit code 0; XIIGEN_CODEX_HOST_SMOKE_DONE"` → `turn.completed usage{input_tokens:53349, cached:44800, output:416, reasoning:223}`. **Т.е. Codex CLI реально исполнил entrypoint, и токен-usage измерен** — реализована задумка плана A-005 (Codex = первый ИЗМЕРИМЫЙ хост). **Важно:** это ОПРОВЕРГАЕТ раннюю заметку Pass 2 (§5.10) «`codex.exe --help` = Access is denied» — к run-4 codex CLI уже работал.

**N.3 Что реально считает core-call (честность, спот-чек `phase40-claude-core-response.json`).** `passed:true, runtimeEvidence:true`, НО `fullHostRuntimeEvidence:false`, `evidenceScope:"core_api_check_set_runtime_not_full_host"`, `semanticQualityClaimed:false`, `deterministicEvidenceUsedAsLearning:false`. Результат чек-сета: `contractCheckLabel:"deterministic_contract_check_not_learning"`, `completionDecision:"core_api_contract_execution_only"`, `concreteAnswer:"...semantic skill quality is not claimed by this script"`. **Вывод: phase40 core-api на этом этапе — КОНТРАКТ-харнесс, а НЕ обученная `SkillSelectionMachine`/`PromptToTaskMachine`.** Плюс `containsPrivateAbsolutePath:true` (portability-дефект). Это тот же honesty-разрыв, что и у Cursor: доказана ГРАНИЦА core-call, но не обученный выбор скиллов.

### O. 3-way host diff (Cursor vs Claude vs Codex) — ЭТО и есть шаблон host-адаптера

| Ось | Cursor (Pass 6) | Claude (этот проход) | Codex (этот проход) |
|---|---|---|---|
| Код адаптера | `src/extension.ts` (1628 стр, форк VS Code) → `dist/extension.js` | `src/claude-entrypoint.ps1` (247 стр) | `src/codex-entrypoint.ps1` (247 стр, ~=claude) |
| Язык | TypeScript (компилируемый) | PowerShell (скрипт) | PowerShell (скрипт) |
| Manifest-файл | `package.json` (VS Code ext) + устар. `.cursor/*.json` | `.claude-plugin/plugin.json` (Claude plugin schema) | `.codex/xiigen-smart-skills.json` (bespoke schema) |
| Где живёт boundary/prohibitions | `xiigen-extension-contract.json` + README | README + core-план (plugin.json игнорит чужие поля) | В САМОМ манифесте (`prohibitions`/`asset_boundary`) |
| Событие/триггер | `activationEvents` + команда + panel webview | `SessionStart` lifecycle-hook | команда `xiigen.smartSkills.select` через `codex exec` |
| Взаимодействие с юзером | видимая панель (webview), ввод в textarea | автоматический SessionStart (не user-invoked) | shell-команда в сессии Codex |
| Транспорт к ядру | `child_process.spawn powershell -File <core-api>` + JSON-файлы | `& powershell -File <core-api>` + JSON-файлы | `& powershell -File <core-api>` + JSON-файлы |
| Core-api скрипт | `tools\xiigen-extension-core-api.ps1` (274 стр) | `tools\phase40-extension-core-api.ps1` (357 стр) | `tools\phase40-extension-core-api.ps1` (тот же) |
| Core-контракт (.cs) | `XiigenExtensionApiContracts.cs` | `43.SmartSkillsDomain\Phase40ExtensionContracts.cs` | тот же Phase40-контракт |
| Debug/trace surface | setting `xiigenSmartSkills.debug` → redacted trace в чат | request/response/trace JSON + `contractOnlyDowngrade`/honesty-флаги | те же JSON + JSONL token-usage от `codex exec` |
| Real host-run proof | `runtime_host_proof:false`, Path B НЕ доказан | `claude.exe --plugin-dir` exit 0 (host загрузил), но full-host не доказан | `codex exec` реально запустил entrypoint + token-usage |
| R6 thin-adapter | PASS (но >80% evidence-балласта, есть логика redaction/panel) | PASS (0 логики решения — только транспорт) | PASS (0 логики решения — только транспорт) |
| Статус зрелости | самый полный (реальный UI-код) | skeleton (`entrypointImplemented:false`, но .ps1 работает) | skeleton (`implementation_status:"skeleton_only"`) |

**Чтение диффа:** три хоста оборачивают ОДНО ядро тремя разными способами именно по 5 осям, которые и образуют шаблон: (1) manifest-схема хоста, (2) событие/триггер хоста, (3) тонкий entrypoint, (4) вызов core-api (одинаков у всех), (5) trace/honesty. Меняется только «оболочка» (1,2,3,5-формат); ядро (4 + весь семантический слой) неизменно. Claude и Codex доказывают, что оболочку можно свести к ~6 полям над одним PS-шаблоном; Cursor доказывает полноценную UI-оболочку. VS Code ближе к Cursor (форк → переиспользовать `extension.ts`+`package.json`); Visual Studio ближе к «толстой» C#-оболочке (VSIX/VSPackage), но core-call и trace те же.

### P. Унифицированный host-adapter контракт (host vs core split) — blueprint для VS Code + Visual Studio

**ЧТО ОБЯЗАН дать КАЖДЫЙ хост (5 ног, host-owned):**
1. **MANIFEST** — нативный дескриптор пакета, который распознаёт загрузчик хоста. Claude: `.claude-plugin/plugin.json`; Codex: `.codex/*.json`; Cursor: `package.json` (VS Code). → **VS Code (host 4): `package.json` c `contributes`/`activationEvents`/`commands`**; → **Visual Studio (host 5): `source.extension.vsixmanifest` + VSPackage/`.vsct`**.
2. **EVENT/TRIGGER** — lifecycle- или user-поверхность, запускающая адаптер. Claude: `SessionStart` hook; Codex: команда через `codex exec`; Cursor: `activationEvents`+command+panel. → **VS Code: `activationEvents`/registerCommand/chat-participant/panel**; → **VS: команда/tool-window/menu-command**.
3. **ENTRYPOINT** — тонкий адаптер (скрипт или код), который СОБИРАЕТ `SkillSelectionRequest` из фактов хоста. Claude/Codex: PS-entrypoint; Cursor: `extension.ts`. **Инвариант: НОЛЬ семантических решений** (`hostDecisionAuthority:false`, `hostSemanticRankingAllowed:false`).
4. **CORE-CALL** — транспорт: `powershell -NoProfile -ExecutionPolicy Bypass -File <core-api>.ps1 -RequestPath -ResponsePath -TracePath`, обмен через JSON-файлы, `$LASTEXITCODE`=гейт. Форма ОДИНАКОВА у всех трёх; VS/VS Code берут её без изменений (VS может звать core-exe напрямую, но контракт JSON тот же).
5. **TRACE/HONESTY** — request/response/trace JSON + флаги: `machine_decision`/`deterministic_semantic_source:false` (продуктовый рантайм), `contractOnlyDowngrade`, `hostSemanticDecisionAuthority:false`, `externalAiUsed:false`, и `containsPrivateAbsolutePath` ДОЛЖЕН стать false. Плюс честный downgrade, когда core-api недоступен (`passed:false`, `exit 2`, `contract_only_downgrade`).

**ЧТО остаётся в ЯДРЕ `llm_mvp_core` (core-owned, ни один хост не дублирует):** `SkillSelectionMachine`/фактические `PromptToTaskMachine`+`OrchestrationReasoningMachine` (домен 43), контракт-типы (`Phase40ExtensionContracts.cs`), сам `<core-api>.ps1`, модель/датасет-ассеты + манифесты, обучение/оценка/DPO/held-out/export/import/locator/continue-training, авторитет split «common (в репо) vs adaptive (у пользователя)». Хост владеет ТОЛЬКО упаковкой + копиями exe + локатором + smoke-промптами.

**Как это применяется к host 4/5:** взять неизменное ядро, заполнить 5 ног. Риск-ноль путь: VS Code = переиспользовать cursor-спайн (`extension.ts`+транспорт), сменить только manifest-поля и брендинг; Visual Studio = новая C#-VSIX-оболочка, вызывающая тот же core-api/exe контракт. Оба обязаны пройти R6 (нет утечки логики) и честный trace.

### Q. R6 thin-adapter проверка по каждому хосту

- **Claude — R6 PASS.** Entrypoint = чистый транспорт (сборка JSON + резолв путей + парс check-set + exec core-api + маппинг exit-кода). Нет BM25/regex/ranking/training/if-роутинга. `coreOwnsSkillSelection:true`, `hostDecisionAuthority:false`, `hostSemanticDecisionAuthority:false`. Честный `contract_only_downgrade` fallback = анти-masquerade.
- **Codex — R6 PASS.** Байт-идентичная логика → те же гарантии; плюс token-usage через `codex exec` не является host-решением (это метрика хоста).
- **Общая оговорка:** у Claude/Codex R6 проходит ТРИВИАЛЬНО, потому что entrypoint вообще НЕ содержит логики выбора (в отличие от `extension.ts` Cursor, где есть redaction/trace/panel-логика — там R6 надо проверять внимательнее). Обратная сторона: эти два адаптера ТОНЬШЕ и МЕНЕЕ полны, чем Cursor (манифесты сами говорят `skeleton_only`/`entrypointImplemented:false`).

### R. Дефекты/риски для continue-плана (host 4/5)

1. **Фрагментация ядра-контракта:** ДВА core-api скрипта (`phase40-extension-core-api.ps1` для Claude/Codex vs `xiigen-extension-core-api.ps1` для Cursor) + ДВА .cs-контракта (`Phase40ExtensionContracts.cs` vs `XiigenExtensionApiContracts.cs`). Перед добавлением host 4/5 — унифицировать в ОДИН core-api + один контракт, иначе 5 хостов = 2 расходящихся ядра.
2. **Claude: пустой `hooks/hooks.json` vs реальный hook в `plugin.json`** — убрать/прояснить; план host 4/5 должен выбрать ОДИН дом манифеста на хост.
3. **Portability:** `containsPrivateAbsolutePath:true` + захардкоженные абсолютные пути в hook-команде `plugin.json` и в default-путях entrypoint. Новые хосты обязаны использовать относительные пути / `${workspaceFolder}` / переменные.
4. **core-call = контракт-харнесс, не обученная машина** (`deterministic_contract_check_not_learning`): нога «core-call» сейчас доказывает границу, а не обученный выбор скиллов. Тот же honesty-разрыв, что у Cursor — план host 4/5 не должен выдавать boundary-proof за trained-proof.
5. **Дрейф статус-метаданных:** манифесты говорят `entrypointImplemented:false`/`implementation_status:"skeleton_only"`, хотя `.ps1` существует и реально отрабатывает (exit 0 в host-run). Зеркало cursor-дефекта «устаревший `.cursor`-манифест». Синхронизировать статус с фактом.
6. **Claude host-run stdout доказывает лишь echo маркера**; доказательство срабатывания хука — отдельный component-response. Path B нового хоста обязан связать цепочку триггер→entrypoint→core, а не только `--print` маркер.

### S. Что остаётся на проходы 8–10

- **Pass 8:** FigmaToCode ядро — `src/plugin/core/{controller,op-executor}.ts`, `src/plugin/models/*`, `manifest.json`, `CLAUDE.md`/`AGENTS.md` (отдельный design→code продукт).
- **Pass 9:** FigmaToCode design-gen — `src/ui/components/design-gen/` (провайдеры Claude/OpenAI, skill-advisor: SkillAdvisor/SkillJudge/skill-pipeline/registry/MemoryStore), `ai-enhance/{AIService,SkillInjector,PromptTemplates}` + `Plans/` (framer/miro/webflow/canva).
- **Pass 10:** core-сторона (`43.SmartSkillsDomain` контракты + ОБА `tools\*-core-api.ps1`) + СИНТЕЗ: как «extensions reasearch» питает VS/VS Code continue-план; сверка/примирение двух core-api; открытые пробелы и рекомендации.


---

## Pass 8/10 — FigmaToCode engine: monorepo + SceneNode IR + conversion pipeline + target frameworks + связь с 30.DesignConversionDomain

> Проход 8/10, папка #11/12 «extensions reasearch», под-репо
> `FigmaToCode-design-gen-improve_group_css_user/` (346 файлов). Единственный
> изменённый файл — этот RAG. Всё остальное read-only.
> **Важная поправка к формулировке задания:** (1) реальная папка домена в
> `llm_mvp_core` называется **`44.DesignConversionDomain`**, а не `30.` — см.
> §8.9; (2) это под-репо — **НЕ монорепо хостов** (Figma/Canva/Miro/Webflow),
> которое RAG-09 видел у ОРИГИНАЛА `bernaferrari/FigmaToCode`. Это **один
> самостоятельный Figma-плагин** (форк/производная, `name:"figma-to-code"
> v2.0.0`), у которого детерминированные мульти-фреймворк билдеры оригинала
> ВЫРЕЗАНЫ и заменены LLM-слоем AI-enhance. Ниже — что реально в коде.

### 8.1. Структура репо (НЕ монорепо — один плагин, два изолированных контекста)

`manifest.json` (Figma plugin manifest): `main: dist/code.js`, `ui: dist/ui.html`,
`editorType:["figma","figjam"]`, `documentAccess:"dynamic-page"`,
`networkAccess.allowedDomains = [figma.com, **api.anthropic.com, api.openai.com**,
loremflickr, picsum]`. Т.е. плагин НАПРЯМУЮ ходит в Anthropic/OpenAI из iframe.

`package.json`: `figma-to-code` v2.0.0, «Generate HTML, CSS, and full-stack code
from your Figma designs — with AI enhancement». Стек: React 17 + lodash +
`@figma/plugin-typings`, сборка webpack 4 + ts-loader, тесты vitest + puppeteer.
Никаких ML-зависимостей (нет torch/tf) — «ML» здесь = самописные эвристики (§8.5).

Два изолированных контекста (из `docs/ARCHITECTURE.md`), общаются через `postMessage`:

```
src/
├── plugin/               # Figma sandbox VM: есть Figma API, НЕТ DOM/fetch/React
│   ├── core/
│   │   ├── controller.ts     (592 стр) — ENTRYPOINT: scan→IR→HTML/CSS + 20+ msg-хендлеров
│   │   └── op-executor.ts    (900 стр) — ОБРАТНОЕ направление: JSON-ops → мутации Figma-canvas
│   ├── models/               # IR-типы (все крошечные type-алиасы)
│   │   ├── Element.ts        (24 стр) — УЗЕЛ IR (дерево)
│   │   ├── Style.ts / Setting.ts / CSSDebugEntry.ts / IStorage.ts / ElementsCounter.ts
│   └── utils/
│       ├── styles.ts         (1998 стр) — ЯДРО: SceneNode → CSS, 18-шаговый pipeline
│       ├── element-code.ts   (292 стр)  — Element-дерево → строки HTML + CSS
│       ├── classifiers.ts    (396 стр)  — 4 эвристич. «ML»-классификатора layout/tag
│       ├── scene-memory.ts   (212 стр)  — session-store + like/dislike feedback (clientStorage)
│       ├── is-image.ts / setting-helpers.ts
└── ui/                   # React 17 iframe: есть DOM/fetch, НЕТ Figma API (8512 стр TSX)
    ├── components/App.tsx (592) — 6 табов; ai-enhance/; design-gen/ (providers + skill-advisor); …
```

### 8.2. Канонический IR — это **`Element` + `Style`**, а НЕ SceneNode/AltNode

Ключевое отличие от оригинала (который RAG-09 описал как AltNode/SceneNode IR):
у ЭТОГО форка своя промежуточная модель. Сырой Figma `SceneNode` лежит внутри
как поле `source`, но пайплайн работает над `Element`:

```ts
// models/Element.ts — УЗЕЛ дерева IR
type Element = {
    name, source: SceneNode,      // ← сырой Figma-узел спрятан здесь
    containingString: string|null, // текст (characters) для TEXT-узлов
    sourceType: string,            // Figma type: FRAME/TEXT/RECTANGLE/...
    type: string,                  // HTML-тег: div/p/img/input/button/... (см. §8.4)
    style: Style, improvedStyle: Style|null,  // improvedStyle = результат AI-enhance
    subElements: Element[],        // рекурсивные дети
    parentSettings, parentCssSettings,  // контекст родителя (для абсолютного→flex)
    code, classNames, placeholder, openTag, closeTag, subCode, codeCss, selected };

// models/Style.ts — набор CSS-свойств узла
type Style = { name, settings: Setting[], sceneSettings: Setting[], sceneSet, image, debugHistory? };
// models/Setting.ts — ОДНО CSS-свойство
type Setting = { name: string, value: string|number };   // напр. {name:'display', value:'flex'}
// models/CSSDebugEntry.ts — трасса ОДНОГО CSS-решения (step/property/value/reason)
```

Т.е. IR — это **дерево `Element`**, где семантика оформления = **плоский список
`Setting` (CSS name/value)** в `style.settings`, плюс параллельный
`sceneSettings` (служебные Figma-факты вроде `layoutMode`, `position`, `flex`,
`_classifierApplied` — контекст, передаваемый детям, но не CSS). `debugHistory`
(`CSSDebugEntry[]`) — самодокументируемая трасса: какой шаг pipeline поставил
какое свойство и ПОЧЕМУ (на основе данных Figma-узла). Это прямой аналог того,
что `44.DesignConversionDomain` называет `DesignTreePayload` / `DesignRenderArtifact`.

### 8.3. Конвейер конвертации (design → IR → код), `controller.ts`

Прямое направление (Figma → HTML/CSS), запускается на старте плагина по выделению
и по `generate-code-from-canvas`:

```
figma.currentPage.selection : SceneNode[]
  └─ scanElements(parentSettings, parentCssSettings, names, imagesToResolve, node)  [controller.ts:19]
       ├─ getStyleForElement(...) → Style           // 18-шаговый CSS-движок (§8.4)
       ├─ if 'children' in node: рекурсия по детям → subElements[]
       ├─ FIX-44: если ML-классификатор превратил layoutMode:NONE в flex-column,
       │          сортировать детей по visual-y (Figma хранит в z-order, не y-order)
       └─ вернуть Element{ source:node, style, subElements, sourceType:node.type, type:'' }
  └─ (async) резолв картинок: exportAsync(PNG) → base64 в style.image
  └─ prepareCodeBaseForElement(element,'html',original)  [element-code.ts:5]  → HTML-строка
  └─ prepareCssForElement(element,original)               [element-code.ts:163] → CSS-строка
  └─ figma.ui.postMessage({ scenes:[{element, code, css}] })
```

**`prepareCodeBaseForElement` (Element → HTML):** сначала `setTypeForElement`
(если `type==''`) назначает HTML-тег; чинит вложенные `<form>`; для `button`
сбрасывает браузерные дефолты (FIX-33); **FIX-G** сортирует flex-column детей по
`source.y`; рекурсивно строит `subCode`; выдаёт `<tag id=styleName class=…
data-node-id=…>text + subCode</tag>` (для `input/checkbox/img` — спец-формы).
`data-node-id` (FIX-41) нужен для реконсиляции дерева после AI-enhance.
**`prepareCssForElement` (Element → CSS):** `.className { name: value; … }` из
`style.settings` (или `improvedStyle.settings`, если AI дал улучшенный), рекурсивно.
Параметр `codeType='html'` существует, но **реализован только 'html'** — иных
детерминированных билдеров нет.

### 8.4. 18-шаговый CSS-движок `getStyleForElement` (`styles.ts:1635`, 1998 стр всего)

Главный per-node движок. Точный порядок шагов (каждый мутирует `settings:Setting[]`
и логирует в `debugHistory`):

1. **SCENE_INPUT capture** — сериализовать ~50 читаемых свойств узла (+геометрию
   детей) в первый debug-entry (для воспроизводимости в тестах, `buildMockFromSnapshot`).
2. **InitiatePropertiesFromScene** — скопировать факты узла в `sceneSettings`.
3. **CONTEXT:parent snapshot** — снять контекст родителя (type/position/flex/layoutMode/x/y/w/h/`_classifierApplied`).
4. **parentAdjustments** — правки относительно родителя.
5. **setBackGroundColor** — `fills` → `background`/`background-color` (SOLID/GRADIENT).
6. **setFilter** — `effects` → `filter`/`backdrop-filter` (blur и т.п.).
7. **SetOpacity** → `opacity`. 8. **SetTransform** → `rotation` → `transform:rotate`.
   Далее ветвление по `scene.type`:
   - **FRAME/INSTANCE/COMPONENT/GROUP:** adjustElementPlacingWithFlexParent (абсолют↔flex-ребёнок) → SetWidthHeight → SetBorderRadius → **AdjustPadding** → **BuildFlex** (Figma auto-layout → CSS flexbox: `display:flex`+`flex-direction`+`justify/align`+`gap`; `styles.ts:983`) → `clipsContent`→`overflow:hidden` → `position:relative`-якорь (FIX-16) → AdjustChildTopLeftNotFlexNotRelative → SetBorder.
   - **RECTANGLE:** SetBorderRadius → SetWidthHeight → SetBorder.
   - **TEXT:** **AdjustText** (`styles.ts:1523`: font-size/weight/family, line-height, letter-spacing, text-align, color) → SetBorder.
   - **LINE:** SetWidthHeight → SetBorder.
9. **MakeImage** — если узел = картинка, экспортировать в base64, отметить в `image`.
10. adjustChildThatHasNoTopLeft (позиц. картинок без top/left).
11. **checkIfElementNeedsToBeAdjusted** + **checkIfElementNeedsToBeAdjustedToPositionedParent** (абсолютное позиционирование к позиционированному родителю).
12. width/height fallback; text-align / vertical-align / align-self; font-size/font-family fallback.
13. **SetBorderRadiusByARC** (дуги/полукруги через arcData). outline:none при overflow+radius.
14. **fixKidToParent** + **fixFlexPostition** (финальная примирка ребёнок↔flex-родитель).
15. **FIX-C button reset** — `predictSemanticTag` (§8.5): если предсказан `<button>` conf>0.5 → сброс border/background/padding + `cursor:pointer` (не затирая явные Figma-значения).
16. **GetElementName** → вернуть `Style{name, settings, sceneSettings, image, debugHistory}`.
   (Пустые settings → `Style.name=''` → `scanElements` дропает узел.)

`README`/`docs/PIPELINE_FLOW.md` называют это «18-step CSS extraction pipeline»,
`Figma-to-CSS-Mapping-Matrix.xlsx` — «17-sheet single source of truth». Это тот
самый ~18-шаговый styles-pipeline, о котором говорил RAG-09.

### 8.5. Классификаторы `classifiers.ts` — «ML»-обёртка над ДЕТЕРМИНИРОВАННОЙ эвристикой

4 чистых (no Figma API) функции, названы как ML, но по факту — правила:
- **classifyLayoutKMeans** (назв. «K-Means»): по геометрии детей (centroid x/y-spread,
  coefficient-of-variation ширин, clusterByAxis) → `FLEX_ROW/FLEX_COL/TWO_COL_GRID/THREE_COL_GRID/SINGLE_CHILD/ABSOLUTE/UNKNOWN`. Внутри — пороги и if-ы, НЕ обученные центроиды.
- **computeGap** («linear-regression»): median edge-to-edge gap + R² «согласованности».
- **matchPattern**: эвристика `card/list/hero/form/none` по числу/типам/размерам детей.
- **predictSemanticTag** («Naïve-Bayes»): скоринг по regex-правилам над именем узла +
  именами детей + весами → `nav/header/footer/main/section/article/aside/button/form/ul/div` + confidence. Веса ЗАШИТЫ (`[/nav/i,'nav',3.0]…`), не обучены.

**Честность / релевантно R4 (no-hidden-determinism):** это регекс/эвристика с
ML-неймингом. Для `llm_mvp_core` при переносе их нельзя выдавать за trained-units;
это «eval-only/helper» baseline либо кандидаты на замену обучаемым
`ILogicalUnit` (guard против позиционного лика, семантический tag-selector и т.п.).

### 8.6. `op-executor.ts` — ОБРАТНОЕ направление (промпт/AI → Figma-узлы), 900 стр

Не design→code, а code/plan→design. `executeOps(ops, nodeMap)` [op-executor.ts:877]
исполняет массив JSON-ops (их генерирует Claude в Design Generator, §8.8) через
`executeOp` switch на **18 типов op**: `create-frame / create-text / create-rect /
set-fill / set-text / resize / set-corner-radius / set-auto-layout / delete-node /
set-image-fill / reorder / move-to-back / set-font-size / set-font-weight /
set-position / set-opacity / set-stroke`. `resolveNode`/`resolveParent`
мапят Claude-имена → реальные Figma-id (`designGenNodeMap`), `wouldOverlapSibling`
защищает от коллизий. Это движок «prompt → canvas», зеркальный к styles.ts.

`controller.ts` держит 20+ postMessage-хендлеров: ключи API (Anthropic/OpenAI в
`figma.clientStorage`), `init-design-gen-canvas`, `execute-figma-ops`,
`request-frame-export` (экспорт JPG 0.5x), `scene-memory-*` (load/add-session/
set-feedback/get-best/clear/list), `store/get-ai-history` (последние 20 генераций
с like/dislike).

### 8.7. Целевые фреймворки — ДЕТЕРМИНИРОВАННО только **HTML+CSS**; фреймворки Любы = через LLM

Прямой ответ на вопрос задания «покрывает ли angular/react/vue/react-native»:

- **Детерминированный движок (styles.ts + element-code.ts) выдаёт ТОЛЬКО HTML + CSS.**
  Нет нативных билдеров Tailwind/Flutter/SwiftUI/Angular/React/Vue (в ОТЛИЧИЕ от
  оригинала bernaferrari, где эти билдеры есть — здесь они вырезаны).
- **Все фреймворк-таргеты идут через AI-enhance LLM-слой** (`ui/components/ai-enhance/`):
  - `PromptTemplates.ts`: шаблоны `React Component`, `React+Express`,
    `React+Express+PostgreSQL`, `Vue3+FastAPI`, `Static Landing`, `Fix Design
    Fidelity` (vision), `Custom`.
  - `CLIENT_STACKS = [react, vue, **angular**, svelte, vanilla]`;
    `SERVER_STACKS=[express, fastapi, spring, go, **dotnet/ASP.NET Core**]`;
    `DATABASE_TYPES=[postgresql, mongodb, mysql, sqlite]`.
  - Механизм: детерминированный HTML+CSS + выбранный стек + (для Fix-Fidelity)
    JPG-картинка фрейма → промпт → **Claude/OpenAI vision** → framework-код в
    `improvedStyle`/`clientCode`. `AIService.ts` даёт template-fallback БЕЗ ключа
    (голый scaffold, напр. `dotnet` ASP.NET Core Program.cs).
  - `SkillInjector.ts` инжектит «скиллы» (react-best-practices, composition,
    **react-native-skills**, web-design-guidelines) в промпт по триггеру стека.

**Вывод по таргетам Любы:** angular/react/vue — покрыты, но **LLM-ом, не
детерминированно**; **react-native** — только косвенно (упомянут в SkillInjector,
НЕ в `CLIENT_STACKS`). Т.е. движок даёт надёжный HTML+CSS-фундамент, а
мульти-фреймворк — это качество/надёжность LLM-трансформации, которую в
`llm_mvp_core` должна давать СВОЯ обучаемая машина, а не внешний Claude/OpenAI
(иначе нарушение R7 external-tool-ban).

### 8.8. Design Generator + skill-advisor (обучающая петля через внешний AI) — вход в Pass 9

Таб 6 (`ui/components/design-gen/`): `OpenAI gpt-image-1` рисует reference-картинку
→ `Claude vision` (`ClaudeComparisonProvider.ts`, 1081 стр) сравнивает current-canvas
vs reference, планирует **delta-ops** → `op-executor` применяет к Figma →
`ScoreGraph` считает score → **цикл, пока score>80 или лимит итераций**.
`skill-advisor/` (`SkillAdvisor.ts` + `SkillJudge.ts` + `skill-pipeline.ts` +
`skill-registry.ts` + `MemoryStore.ts`): перед каждой итерацией Claude-советник
решает, какие 1–3 «скилла» (NAMING_TREE/3D_COMMIT/CONTEXT_BRIDGE/DELTA_OPS/
MEMORY_REPLAY) инжектить в промпт СЛЕДУЮЩЕЙ итерации, по правилам «чем меньше
активных скиллов — тем лучше фокус». `MemoryStore` + `scene-memory.ts` хранят
сессии и like/dislike — это **preference-сигнал** (потенциальные DPO-пары).
Детали — Pass 9.

### 8.9. Связь с `44.DesignConversionDomain` (`llm_mvp_core`) — ПОЧТИ 1:1 структурный аналог

Это и есть «реальный design→code движок для переиспользования». `44.DesignConversionDomain`
(домен из RAG-04; задание ошибочно назвало его «30.») уже реализует ту же
архитектуру, но на C# как правильную `ILearningMachine` с честным DPO/checkpoint:

| FigmaToCode (TS, хост Figma) | 44.DesignConversionDomain (C#, `llm_mvp_core`) |
|---|---|
| IR: `Element`-дерево + `Style.settings`(CSS) + `source:SceneNode` | `DesignTreePayload` / `DesignRenderArtifact` (`DesignConversionContracts.cs`) |
| `getStyleForElement` 18-шаг (детерминир. Figma→CSS) | `DesignConversionRoutingFlow` (`ILogicalFlow`) + trained units |
| `ClaudeComparisonProvider`/`OpenAIComparisonProvider` (visual score) | `DesignConversionVisualAiComparisonProvider` + `DesignConversionExternalVisualJudgeNetwork` + `…VisualAiProviderSelectionPolicy` |
| Fix-Design-Fidelity vision + render-check | `DesignConversionLocalRenderQaProbe` + `DesignConversionBrowserQaProviders` (+ `RenderQaResultRecord`/`VisualVerdict`/`VisualDefect`) |
| Design-Gen цикл (reference→delta-ops→score→advisor→повтор до 80) | `DesignConversionImprovementLoopCoordinator` + `…RuntimeImprovementCandidateStrategy` |
| Claude-критик (SkillJudge/Comparison) + Claude-улучшатель (delta-ops) | `IDesignConversionCriticUnit` + `IDesignConversionImproverUnit` (`DesignConversionUnitContracts.cs`); реализации `DesignConversionRuntimeScaffold{Critic,Improver}Unit` (`DesignConversionTrainedUnits.cs`) |
| `scene-memory` + like/dislike + MemoryStore (preference) | `DesignConversionDpoGenerator` + `…DpoDatasetBuilder` + `…DpoStorage` + `DesignConversionDpoTrainingRecord`/`…DpoProvenance`/`…LicenseScope`/`…QuarantineState` |
| нет (внешний Claude/OpenAI = «модель») | `DesignConversionMachine : ILearningMachine<DesignConversionDomain>` + `TrainingPipeline`/`TrainingRunner`/`Promotion`/`ModelActivationRegistry`/`ManifestPublisher` + fresh-load/continue-training пробы + `RenderEvidenceTrustBoundary` |

**Что это значит для continue-плана VS/VS Code (direction #4 Design→Code):**
переиспользовать из FigmaToCode **(а) детерминированный движок `styles.ts`
18-шаг + IR `Element/Style` + `element-code.ts`** как «Figma/design → HTML+CSS»
front-конвертер, и **(б) архитектуру петли критик→улучшатель→score→memory**; но
**заменить внешние Claude/OpenAI-вызовы на обучаемые `IDesignConversionCriticUnit`/
`IDesignConversionImproverUnit` из `44.DesignConversionDomain`** (R7 external-tool-ban,
R5 модель-в-репо). `44.DesignConversionMachine` УЖЕ честно помечает себя как
runtime-scaffold (`RuntimeScaffoldNotTrainedModelWarning`,
`…FEATURE_WEIGHTS_PERSISTED_NOT_TRAINED_MODEL`,
`…LOCAL_PREFERENCE_FIT_SIGNAL_NOT_HELD_OUT_ACCURACY`) — совпадает с паттерном
CLAUDE.md `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`: контракт есть, обученной модели ещё
нет. Т.е. `llm_mvp_core` = «правильный» скелет, FigmaToCode = богатый рабочий
референс-движок (детерминированная CSS-часть) + готовый UX петли.

### 8.10. Честность / оговорки Pass 8

- **`codeType` поддерживает только `'html'`** — прочих детерминированных билдеров в
  коде НЕТ; «full-stack / фреймворки» = исключительно LLM-слой (внешний AI).
- **«ML»-классификаторы (`classifiers.ts`) детерминированы** (зашитые пороги/веса,
  ML-нейминг). Не trained-units.
- **Плагин ходит во внешние Anthropic/OpenAI напрямую из iframe** (manifest
  `allowedDomains`, `anthropic-dangerous-direct-browser-access:true`). Под правила
  Любы (R7) это нельзя переносить как есть — только как pattern, с заменой на ядро.
- **Это форк-производная, не оригинальный монорепо-FigmaToCode.** Мульти-хост
  (Canva/Miro/Webflow) и мульти-фреймворк билдеры, о которых говорил RAG-09,
  относятся к ОРИГИНАЛУ; здесь их нет. `docs/` содержит богатый план-архив
  (GEOMETRIC-LEARNING-SYSTEM.md, PIPELINE_FLOW.md, MASTER-STATE.json,
  Figma-to-CSS-Mapping-Matrix.xlsx) — материал для Pass 9/10.
- **Соответствие `44.` подтверждено чтением** `DesignConversionMachine.cs`,
  `DesignConversionUnitContracts.cs`, `DesignConversionContracts.cs`,
  `DesignConversionTrainedUnits.cs`; связь — структурная (совпадение ролей),
  прямых import/ссылок между TS-репо и C#-доменом нет (это разные кодовые базы).

### 8.11. Что осталось на проходы 9–10 (уточнение к §S)

- **Pass 9:** design-gen детально — `providers/{ClaudeComparisonProvider(1081),
  OpenAIComparisonProvider(239),OpenAIImageProvider(189),IDesignAiProvider(305)}`,
  `skill-advisor/{SkillAdvisor,SkillJudge,skill-pipeline,skill-registry,MemoryStore}`,
  `ai-enhance/{AIService(402),SkillInjector(281),PromptTemplates}`,
  `export/SceneExporter(327)`, `DesignGeneratorPanel(2260)`; как like/dislike +
  score → DPO-пары для `44.DesignConversionDpoGenerator`.
- **Pass 10:** синтез — как «extensions reasearch» (все под-репо) питает
  VS/VS Code continue-план; сверка FigmaToCode-движка с `44.DesignConversionDomain`
  (что взять детерминированно, что обучать); открытые пробелы (react-native,
  единый core-api, замена внешнего AI на ядро) и рекомендации.

## Pass 9/10 — FigmaToCode design-gen + AI-enhance critic/improver + skill-advisor + like/dislike→DPO loop + Plans/other

> Проход 9/10, папка #11/12 «extensions reasearch», под-репо
> `FigmaToCode-design-gen-improve_group_css_user/` (346 файлов) + `Plans/` +
> `other/`. Единственный изменённый файл — этот RAG. Всё остальное read-only.
> Этот проход закрывает **обучающий контур design→code**: AI-enhance
> критик/улучшатель, skill-advisor (2 модели), и точный механизм
> like/dislike→score→scene-memory→(DPO), который отображается на
> `44.DesignConversionDpoGenerator`/`DpoStorage`. Все пути к файлам —
> относительно `.../FigmaToCode-design-gen-improve_group_css_user/src/`.
> Границы прохода: PLAN-файлы верхнего уровня (token-optimizer, skill-selection,
> local-testing, desktop-adaptation, marketplace-мастера) УЖЕ разобраны в
> проходах 2–5; здесь по Plans/other беру только НОВОЕ (под-папка `Plans/` с
> платформенными адаптациями + `other/` маркетплейс), см. §9.8–9.9.

### 9.1. Карта: в этом форке ДВА независимых AI-контура + ДВЕ поверхности feedback

Важно не путать два контура — они физически разные табы и разный код:

| Контур | Таб / файлы | Что делает | Модель | Выход | Feedback-хранилище |
|---|---|---|---|---|---|
| **A. AI-enhance** (код) | `ui/components/ai-enhance/` | HTML+CSS (из детерминированного движка, pass 8) → full-stack код (React/Express/…) | Claude Sonnet 4 (по API-ключу) | `clientCode/serverCode/databaseCode` или `fixedHtml/fixedCss` | `HistoryRecord.liked` 👍/👎 + `comment`, last-20, `clientStorage['ai_enhance_history']` |
| **B. design-gen** (canvas) | `ui/components/design-gen/` | текст-промпт → референс-мокап (картинка) → итеративно пересобрать Figma-canvas дельта-опами до совпадения | генерация: gpt-image-1/dall-e-3; критик+улучшатель: Claude Sonnet 4 **или** GPT-4o | `ComparisonResult{score, ops[], notes, annotation, tokens, regionScores}` | `SessionEntry.userFeedback` good/bad/unrated + `userComment` + `corrections[]` + `score`, per-scene MAX-10, `clientStorage['scene_memory_*']` |

Обе поверхности хранят **сырьё для DPO** (что человеку понравилось / не
понравилось + числовой score + правки), но **ни одна не строит DPO-пары и не
обучает** — обучение здесь делает ВНЕШНЯЯ модель по API. Именно это место
заменяется на `44.DesignConversionDomain` (см. §9.10). Ключевой вывод прохода:
контур B (`compareAndPlan` критик→улучшатель + scene-memory good/bad + corrections)
— это near-1:1 «внешний» прототип пары `DesignConversionBrowserQaProviders` +
critic/improver + `DpoGenerator/DpoStorage` из `44.`.

### 9.2. Контур A — AI-enhance слой (простой critic/improver над кодом)

Файлы: `ai-enhance/AIService.ts` (402), `PromptTemplates.ts` (158),
`SkillInjector.ts` (281), `AIEnhancePanel.tsx` (577).

**AIService.ts — постройка промпта и вызов API.**
- `buildPrompt(config)` собирает секции: system-context → **`previousResult`**
  (если задан — «Current Implementation (Iteration — refine this, do NOT start
  from scratch)», обрезка до 4000 симв/секцию) → исходная HTML/CSS-выгрузка →
  target-инструкции (client/server/database) → пользовательские инструкции →
  `skillContext`. Итеративность (=улучшатель) реализована именно через
  `previousResult` — это «improver»-петля на уровне кода.
- `generate(config)`: `DEFAULT_MODEL='claude-sonnet-4-20250514'`,
  `DEFAULT_ENDPOINT='https://api.anthropic.com/v1/messages'`, прямой `fetch` из
  iframe с `anthropic-dangerous-direct-browser-access:true`. Мультимодально:
  если есть `designImage` (base64 исходного Figma-фрейма) — картинка первым
  блоком, промпт вторым (шаблон `fix-design`). `max_tokens`: 16000 для
  full-stack/fix-design, иначе 6000; таймаут 4/3 мин. **Fallback без ключа**:
  `generateFallback` отдаёт статические скелеты (react/express/dotnet/postgres).
- `parseResponse(rawText)`: режет по маркерам `// === CLIENT CODE ===` и т.п.,
  ловит голые ```html```/```css``` блоки (выход `fix-design`), fallback-разбор
  по последовательным код-блокам.
- **History/feedback** (низ файла): `HistoryRecord{…, liked?:boolean,
  comment?:string}` — `true`→👍, `false`→👎, `undefined`→нет голоса.
  `persistHistory` держит **последние 20**, шлёт `{type:'store-ai-history'}` →
  clientStorage.

**PromptTemplates.ts — 7 шаблонов**: `react-component`, `react-express`,
`react-express-postgres`, `vue-fastapi`, `static-landing`, **`fix-design`** («AI
сравнивает Figma-фрейм с кодом и чинит», 10-шаговая инструкция, сохраняя
`data-node-id`, два блока html/css), `custom`. Справочники стеков: client
(react/vue/angular/svelte/vanilla), server (express/fastapi/spring/go/**dotnet**),
db (postgres/mongo/mysql/sqlite).

**SkillInjector.ts — ВНИМАНИЕ: статический детерминированный словарь, НЕ
обучаемый.** 4 хардкод-набора (`REACT_BEST_PRACTICES`, `COMPOSITION_PATTERNS`,
`REACT_NATIVE_SKILLS`, `WEB_PERFORMANCE`), правило `{id,title,impact:CRITICAL|
HIGH, correct, avoid}`. Активация по `clientStack/serverStack` (`triggersOn`),
`getSkillsForConfig` — простой `Set.has` фильтр (для `fix-design` всегда `[]`).
`formatSkillsForPrompt` рендерит «## Code Quality Rules» (~800 токенов). По
R4/allowed-trainable-gate это **deterministic helper**, не «обученный скилл».

### 9.3. Контур B — провайдеры design-gen (критик→улучшатель через vision)

Fabric-интерфейс `providers/IDesignAiProvider.ts` (305) — **весь AI в design-gen
идёт только через него, никаких сырых fetch в UI**. Два метода:
- `generateDesignImage(prompt, apiKey)` — текст → референс-мокап (base64).
- `compareAndPlan(referenceB64, figmaB64, iteration, apiKey, prevScore?, …)` —
  **это и критик, и улучшатель**: возвращает `ComparisonResult{score:number,
  ops:FigmaOperation[], notes, annotation?, tokens?:DesignTokens, regionScores?,
  rawJson, promptSent}`.

`FigmaOperation` — union из **17 оп** (`create-frame/text/rect`, `set-fill/text/
font-size/font-weight/position/opacity/stroke/image-fill/corner-radius/
auto-layout`, `resize`, `delete-node`, `reorder`, `move-to-back`) — те самые
дельта-опы, которые `op-executor.ts` (pass 8) применяет к canvas.
`DesignTokens{fonts[],colors[],spacing}` извлекаются в КАЖДОМ ответе (критик
заодно вытягивает токены) и реинжектятся в следующую итерацию.

**Три провайдера:**
1. `ClaudeComparisonProvider.ts` (1081) — основной. `CLAUDE_MODEL='claude-
   sonnet-4-20250514'`, `MAX_TOKENS=16384`. Огромный `SYSTEM_PROMPT`:
   персистентная модель («именованные фреймы живут между итерациями»),
   фазовость по score (EARLY <20 строим зоны; MID 40-70 наполняем; LATE >70
   точечные правки), строгий layer-order (bg→sub-frames→image→text, иначе
   `move-to-back`), 3-слойная naming-конвенция + 3D-commit. Экспортирует
   `scoreOnly(...)` — независимый критик (score+deductions).
2. `OpenAIComparisonProvider.ts` (239) — альтернативный тот же контракт на
   `gpt-4o`; тот же дедакт-рубрик; тоже экспортирует `scoreOnly`
   (`SCORE_RUBRIC_SYSTEM`=«strict visual QA reviewer»).
3. `OpenAIImageProvider.ts` (189) — генерация референса: `gpt-image-1` →
   авто-фолбэк `dall-e-3`; `PROMPT_PREFIX` стирает промпт в «clean UI mockup».

### 9.4. Скоринг: рубрика вычитания + независимый кросс-модельный пере-скор

Критик считает score **не «сколько опов применили», а «что видит человек»**.
Рубрика (`ClaudeComparisonProvider` SYSTEM_PROMPT, старт 100, вычитаем):
`-15` крупная секция из REFERENCE отсутствует в CURRENT; `-10` секция не на
месте/не того размера; `-10` перекрытие; `-5` неверный текст-лейбл; `-5`
неверный цвет/размер шрифта; `-3` отсутствующий image-placeholder. Явно: **«Do
NOT award points for applying design tokens if the visual result diverges»**,
«The score must reflect what a human sees». Плюс `regionScores` (0-100 по зонам)
и `needs_reexport` (пустой canvas → score=0, переэкспорт).

**Анти-инфляция self-report** (`DesignGeneratorPanel.tsx:1009-1024`): при
`useIndependentScoring` ПОСЛЕ применения опов запускается отдельный `scoreOnly`
(можно ДРУГОЙ моделью: `aiProvider==='openai' ? openaiScoreOnly :
claudeScoreOnly`) — оценивается финальный canvas, а не самоотчёт улучшателя.
Прямой аналог требования `Acceptance-signal independence rule` из CLAUDE.md.

### 9.5. Skill-advisor: две модели решают, КАКИЕ скиллы инжектить каждую итерацию

Папка `design-gen/skill-advisor/` — слой ПЕРЕД `compareAndPlan`. Идея: не пихать
все правила всегда, а адаптивно выбирать 1-3 нужных скилла (экономия токенов +
фокус модели).

- `skill-registry.ts` (155) — **5 инжектируемых текст-блоков** `SkillKey`:
  `NAMING_TREE` (18-строчное дерево именования, «card-1/text-3 = wrong»),
  `3D_COMMIT` (name+parent+z-order перед каждым create-*), `CONTEXT_BRIDGE`
  (строгий оверрайд имён из карты кодовой базы), `DELTA_OPS` («структура есть,
  только правки, create-* запрещён»), `MEMORY_REPLAY` (динамич. из повторных
  ошибок). `DEFAULT_COLD_START_SKILLS=['NAMING_TREE','3D_COMMIT']`.
- `SkillAdvisor.ts` (147) — **AI-1**, `claude-sonnet-4`. Вход: 2 картинки +
  `sessionSummary` + iteration + score + hasBridge. Выход строгий JSON
  `{recommended[], skip[], reasoning, confidence, warning?}`. Правила: iter 1-2
  всегда NAMING_TREE+3D_COMMIT; iter6+ добавить DELTA_OPS если score≥50; «max 3».
- `SkillJudge.ts` (161) — **AI-2**, **`gpt-4o`** (специально ДРУГАЯ модель — «даёт
  реальный кросс-чек, а не самовалидацию»). Получает те же 2 картинки +
  рекомендацию advisor'а, отдаёт `{verdict:APPROVE|OVERRIDE, finalSkills[],
  issues[], judgeNote, overrideReason?}`. Ловит over/under-injection.
- `MemoryStore.ts` (192) — **сессионная память в React-state (НЕ persist!)**.
  `IterationMemory{iteration, skillsUsed, scoreBefore/After, delta, nodesFailed[],
  nodesFixed[], userFeedback, advisorReason, judgeOverride, judgeReason}`.
  `getSummary()` — сводка последних 3 итераций + `REPEATED FAILURES:` (узлы,
  провалившиеся 2+ итерации подряд, `_updateRepeatedMistakes`) + `USER FEEDBACK:`.
  `buildIterationContext` — чистая функция, focus-блок в USER-сообщение.
- `skill-pipeline.ts` (164) — оркестратор `runSkillAdvisor→runSkillJudge→
  buildIterationContext`. Режимы `full|advisor-only|fallback`; на сбой — фолбэк
  на дефолтные скиллы. `recordIteration` пишет результат в память ПОСЛЕ score.

Итог: самостоятельный `ILogicalOrchestrator`-подобный контур выбора скиллов
(advisor=selector, judge=critic, memory=state), но реализован как **два внешних
API-вызова + in-RAM память**, без обучаемого состояния/чекпоинта. Кандидат на
`SkillSelectionMachine`/`ILogicalOrchestrator` (ср. `43.SmartSkillsDomain`, pass 2).

### 9.6. Точный механизм like/dislike → score → scene-memory → (DPO-сырьё)

Ядро задания прохода. По шагам, где именно feedback становится парой-кандидатом.

**(1) Хранилище — `plugin/utils/scene-memory.ts` (212).** `SceneMemory(storage:
IStorage)` поверх `figma.clientStorage` (DI, тестируемо). Единица —
`SessionEntry`:
```ts
SessionEntry = { id, timestamp, source:'claude'|'openai'|'gemini'|string,
  css, html, score:number,
  corrections: Array<{className, original, corrected, timestamp, kept:boolean, provider}>,
  userFeedback: 'good'|'bad'|'unrated', userComment: string }
```
`SceneData{sceneName, sessions[], bestSessionId, classifierTrace, metadata}`.
Операции:
- `addSession` — newest-first, тримминг до `MAX_SESSIONS=10`, **первыми
  удаляются oldest UNRATED** (`findOldestUnrated`) — оценённые (good/bad) сессии
  защищены от вытеснения (осознанное сохранение обучающего сырья).
- `setFeedback(scene, id, 'good'|'bad'|'unrated', comment?)` — ставит ярлык.
- `computeBest` — «лучшая»= сначала пул `userFeedback==='good'`, внутри максимум
  по `score`; если good нет — максимум по score среди всех. Т.е. **человеческий
  лайк перекрывает числовой score** при выборе эталона.
- `getByFeedback(scene,'good'|'bad')` — прямой отбор chosen/rejected пула.

**(2) Проводка — `plugin/core/controller.ts:347-402`.** 6 message-хендлеров:
`scene-memory-load / add-session / set-feedback / get-best / clear / list`;
`const sceneMemory = new SceneMemory(figma.clientStorage)` (стр.12).

**(3) Contour-A аналог — `AIEnhancePanel.tsx:312-321`.** `onFeedback(isLiked)`:
тоггл `liked` (повторный клик снимает голос), правит ПОСЛЕДНЮЮ `HistoryRecord`
(`liked`+`comment`), `persistHistory` (last-20). UI: кнопки 👍/👎 +
textarea-комментарий (стр.491-537).

**(4) Как рождается chosen/rejected сигнал (это и есть DPO-сырьё):**
- `chosen`  ← `SessionEntry` с `userFeedback==='good'` (или `getBest`), либо
  `HistoryRecord.liked===true`; несёт `css/html`, `score`, `corrections[kept:true]`.
- `rejected` ← `userFeedback==='bad'` / `liked===false`; та же сцена, худший вариант.
- `corrections[]` (className/original/corrected/kept) — готовый дифф «было→стало»
  внутри одной сессии, ещё один источник пар на уровне отдельного CSS-класса.
- `score` + `regionScores` — числовой reward/margin-сигнал.

**(5) Чего здесь НЕТ (главный разрыв):** в под-репо **нет кода, который из
good/bad-сессий собирает JSONL-пары, считает reward-margin и обучает**. Нет
`DpoGenerator`, нет чекпоинта, нет held-out. Улучшение делает внешняя модель по
API «на лету». FigmaToCode даёт **сырьё и контур измерения**, но не тренируемый
юнит. Это ровно то, что достраивает `44.` (см. §9.10). Термина «DPO» в коде
под-репо НЕТ (grep пусто) — DPO — это НАШЕ отображение на `44.`.

### 9.7. Итерационная петля (critic→improver→score→memory→restore)

`DesignGeneratorPanel.tsx:825-1096` `runIterationLoop(refB64, startIter,
seedBestScore)`. По итерации: экспорт фрейма → blank-guard → `runSkillPipeline`
(advisor/judge) → `compareAndPlan` (score+ops) → `recordIteration` в память →
пересчёт `focusRegions` (зоны <75 передаются дальше, ≥75 «залочены») →
**downgrade-detect**: если `score < bestScore-10` → откат к `bestOps` (полный
replay), стоп → иначе `executeOpsAndGetFrame(ops)` → опц. независимый `scoreOnly`
→ трекинг `bestScore/bestOps/bestIteration` → выход по `score>=threshold` или
исчерпании (тогда тоже восстановить best). «Лучший результат» защищён откатом —
встроенная anti-regression дисциплина.

### 9.8. `Plans/` — под-папка платформенных адаптаций (НОВОЕ; PLAN-* верхнего уровня см. pass 2/4/5)

Под-папка `extensions reasearch/Plans/` — порты FigmaToCode-пайплайна на ДРУГИЕ
design/canvas-платформы (не путать с PLAN-*.md верхнего уровня про
token-optimizer/skill-selection — те в pass 2/4/5). Содержимое (zips + m1):
- `C5-PIPELINE-ADAPTATION.zip` — **C5 Canva** (design-tool → текст с типизир.
  стилями; адаптер = трансляция property-энумов).
- `figma to code adapt to canva.zip` + `…canva files.zip` — исходники canva.
- `framer/f1-framer-plan.zip` — **F1 Framer** (парсинг CSS-строк; review ~7 дней+).
- `w1-webflow-plan.zip` — **W1 Webflow** (реальный CSS → валидация точности пайплайна).
- `m1-miro-plan.zip` + развёрнутая `m1-miro-plan/m1-miro-plan/` — **M1 Miro**,
  7 файлов: `REFERENCE-PLAN-M1.md`, `SESSION-0…6` (plan-review → API-map/scaffold
  → pipeline-copy → shape-heuristics → adapter-wiring → UI-writeback →
  tests/package) + `STATE.json`. Суть M1: Miro **не имеет layout-модели**
  (объекты на бесконечном canvas по x/y), поэтому адаптер ДОЛЖЕН **выводить
  HTML-структуру из пространственных отношений** (shape heuristics) прежде чем
  гнать через существующий CSS-пайплайн — принципиально тяжелее C5/F1/W1.
  Таблица различий портов: C5=translate enums, F1=parse CSS strings, W1=validate,
  **M1=infer layout from spatial positions**.

Вывод для VS/VSCode: паттерн адаптации = **общий core-пайплайн + тонкий
platform-adapter** (тот же R6 «thin agent boundary»). Для code-хостов (VS Code/
Cursor/Codex) адаптер иной (не canvas→CSS, а prompt→task→core), но принцип «ядро
в llm_mvp_core, адаптер тонкий» тот же. Отдельно
`XIIGen-Desktop-Plugin-Adaptation-Report-v2.md` (pass 5) — паттерн **sidecar:
локальный .NET Core-бинарь** рядом с хостом; 19 desktop-платформ включая **Open
VSX** (=VS Code/VSCodium), 14 движков × 19 платформ — прямой аргумент за
VS-расширение = тонкий адаптер к локальному core-бинарю.

### 9.9. `other/` — маркетплейс/трафик-исследование (категоризация; частично покрыто pass 3)

6 файлов, чистая distribution/traffic-стратегия (НЕ про обучающий контур):
- `XIIGEN-20-PLUGIN-TRAFFIC-STRATEGY.md` — 20 freemium-плагинов как воронка на
  xiigen.com.
- `XIIGEN-PER-PLATFORM-PLUGIN-PLANS.md` + `…-v2.md` — идеи плагинов по платформам
  + кросс-платформенные порты + тех-требования.
- `MARKETPLACE-ORGANIC-TRAFFIC-RESEARCH.md` — консолидация 3 ресёрчей (ChatGPT/
  Gemini deep research + брейншторм).
- `MARKETPLACE-RESEARCH-ADDENDUM-v2.md` — draw.io-альтернативы, Figma-подобные,
  e-commerce app stores; **источник авторитетных чисел** (побеждает по `CONFLICT-MAP.md`).
- `MARKETPLACE-NEW-PLATFORM-DISCOVERIES.md` — что упустили.

Масштаб (из `STATE-marketplace-final.json` + `CONFLICT-MAP.md`): 34 платформы,
**64 уникальных плагин-концепта** (UTILITY 41 + FLOW 8 + AUTO 9 + VIDEO 6), 15
числовых конфликтов разрешено. Платформы «избегать»: WordPress new entry, Notion,
Bubble, Make, Sketch, Lunacy, Squarespace. Для VS-задачи релевантно косвенно: VS
Code/Open VSX как канал дистрибуции + модель freemium/licensing (детали — в
PLAN-…-LICENSING, pass 4). Маркетплейс-мастера (`XIIGEN-MARKETPLACE-MASTER.md/
-PLAN.md`, model-asset lifecycle) разобраны в pass 3 — не дублирую.

### 9.10. Отображение обучающего контура FigmaToCode → `44.DesignConversionDomain`

| FigmaToCode (внешний прототип) | Роль | Аналог в `44.` (что достраивается) |
|---|---|---|
| `compareAndPlan` score-часть + `scoreOnly` | критик/визуальный QA | `DesignConversionBrowserQaProviders` + critic (score + deductions) |
| `compareAndPlan` ops-часть + `op-executor` | улучшатель (дельта-опы) | improver-юнит (генерация правок) |
| независимый `scoreOnly` другой моделью | арбитр независим от улучшателя | Acceptance-signal independence (R) — уже правильно |
| `SessionEntry` good/bad/unrated + `corrections[]` + `score` | СЫРЬЁ для пар | вход `DesignConversionDpoGenerator` (chosen=good, rejected=bad, diff=corrections) |
| `computeBest` (good перекрывает score) | выбор эталона | Y_best-selection (классифицировать: eval-baseline vs trainable authority) |
| `scene-memory` (clientStorage, MAX-10) | локальное хранилище сессий | `DpoStorage` / персист чекпоинта |
| SkillInjector (статич.) + skill-advisor (advisor+judge) | выбор правил/скиллов | `ILogicalOrchestrator`/`SkillSelectionMachine` (детерминизм → обучаемый юнит) |
| **ОТСУТСТВУЕТ**: сборка JSONL-пар, reward-margin, обучение, чекпоинт, held-out | — | НЕДОСТАЮЩАЯ часть, которую даёт `44.` + `11.DPO/DpoTrainer` |

**Главный вывод (для плана «continue»):** взять из FigmaToCode детерминированно
— (a) движок SceneNode/Element→CSS (pass 8), (b) 17-оп дельта-модель +
op-executor, (c) рубрику score и regionScores, (d) схему scene-memory
good/bad/corrections как формат сырья. Заменить обучаемым ядром `44.` —
(e) critic/improver как `ILogicalUnit`, (f) `DpoGenerator` из good/bad/corrections,
(g) `DpoStorage`/чекпоинт/held-out (сейчас `RUNTIME_SCAFFOLD_NOT_TRAINED`).
Внешние Claude/OpenAI-вызовы (Sonnet 4 критик, GPT-4o judge, gpt-image-1
генерация) — то, что по R4/R5 должно уехать в общий core-модель llm_mvp_core.

### 9.11. Что осталось на pass 10 (синтез)

- Собрать из pass 1-9 единый ответ: как «extensions reasearch» (design→code
  контур **B** + token-optimizer/skill-selection из pass 2/4 + host-адаптеры
  Cursor/Claude/Codex из pass 6/7) питает VS/VS Code continue-план.
- Явно сопоставить ДВА направления расширений: (#4 design→code + DPO-loop, этот
  проход) vs (skill-selection/token-optimizer, pass 2/4) — какое из них «continue
  VS/VSCode» и как они делят общий core-boundary (R6).
- Свести открытые пробелы: (a) нет тренера/чекпоинта (общий разрыв 44 + форка);
  (b) внешний AI вместо ядра; (c) единый host-adapter контракт (шаблон из pass 7)
  для VS Code/Visual Studio; (d) react-native скиллы в SkillInjector без примеров;
  (e) Y_best/authority-классификация в DPO-сырье.
- Рекомендации по последовательности: тонкий VS-адаптер → core-boundary →
  scene-memory-формат → DpoGenerator → held-out метрики.

---

## Pass 10/10 — навигатор RAG-11 + transfer-checklist (VS Code/Visual Studio) + финальный вердикт + честное покрытие 2200 файлов

> Финальный сборочный проход (10/10), папка #11/12 «extensions reasearch».
> Новый корпус НЕ читался — этот проход перечитал весь RAG-11 (1410 строк, проходы
> 1–9) и сделал быстрый top-level sweep папки (`find`/`unzip -l`) для честного
> покрытия. Единственный изменённый файл — этот RAG. Всё остальное read-only.
> Назначение секции: дать АВТОРУ continue-плана VS Code/Visual Studio (а) карту
> «что где в RAG-11», (б) чеклист переносимых активов с отображением на
> расширение, (в) итоговый вердикт папки, (г) честное покрытие всех 2200 файлов.

### 10.1. НАВИГАТОР по RAG-11 — одна страница: что где искать

| Тема (что нужно автору плана) | Секции RAG-11 | Первичный источник в папке | Одной строкой |
|---|---|---|---|
| **Амбиция «полностью обученного расширения»** | D–P (Pass 1–2) | `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` (3252 стр) | 11 обучаемых машин + `IMachineTraining` + 11 runner-exe, 50+50 DPO/домен, Gate B-1..B-15, Phases 0–12, v20-интеграция; **реальность = сконденсировано в 2 машины `43.SmartSkillsDomain`** (§E/§P) |
| **Marketplace / дистрибуция** | Q–Y (Pass 3) | `XIIGEN-MARKETPLACE-MASTER(-PLAN).md` | это стратегия публикации в СТОРОННИЕ app-store-витрины (VS Code Marketplace, Open VSX…), **НЕ** model-asset marketplace; model-lifecycle backbone брать из `25.ModelServer` (§X); MCP = единый канал Claude+Cursor+Copilot (§T) |
| **Token-optimizer (детерминир. хуки)** | Z–AF (Pass 4) | `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-*-v1.md` (4 файла) | 5–6 хуков-exe, per-host порт-диф (Claude→Codex ≈9 строк, →Cursor ≈55 + Hook6 `beforeReadFile`); **savings = projected, не proven** (§AD); Hook2 BM25 → superseded машиной |
| **Path B (локальное тестирование в реальном хосте)** | AG–AK (Pass 5) | `PLAN-LOCAL-TESTING-{CLAUDE,CODEX,CURSOR}-v1.md` + Desktop-Adaptation-Report v2 | blueprint «поднять хост → baseline без плагина → 5 задач → включить → снять evidence»; Claude `--plugin-dir`; Codex `codex exec --json` (точные токены); Cursor `.cursor/hooks.json`; sidecar/NDJSON/installer + build order (VS Code Wave1, VS Wave2) |
| **Реальный thin-adapter (эталон кода)** | 1–8 (Pass 6) | `cursor-xiigen-smart-skills/src/extension.ts` (1629 стр) | реальное VS Code/Cursor-расширение: activationEvents+3 команды+chat-participant+webview-панель; транспорт = `spawn powershell -File core-api.ps1` + JSON-файлы; **R6 PASS**; 3 дефекта (traceRefs-баг, дрейф `.cursor`-манифеста, Path B не доказан) |
| **Унифицированный host-adapter контракт (шаблон)** | L–S (Pass 7) | claude/codex `*-entrypoint.ps1` (по 247 стр) + core `tools/*.ps1` | 5 ног хоста (manifest/event/entrypoint/core-call/trace) + ядро-owned семантика; Claude `claude.exe --print` exit0, Codex `codex exec` token-usage; **ДЕФЕКТ: два core-api скрипта + два .cs-контракта — унифицировать перед host 4/5** |
| **FigmaToCode: детерминированный движок design→code** | 8.1–8.11 (Pass 8) | `FigmaToCode-.../src/plugin/` | IR = `Element`+`Style(Setting[])`; `getStyleForElement` 18-шаг Figma→CSS (styles.ts 1998 стр); `op-executor` 18 op обратно; **только HTML+CSS детерминированно**, фреймворки — через внешний LLM; near-1:1 на `44.DesignConversionDomain` |
| **FigmaToCode: обучающая петля (критик→улучшатель→score→memory)** | 9.1–9.11 (Pass 9) | `FigmaToCode-.../src/ui/components/design-gen/` + `ai-enhance/` | 2 AI-контура; `compareAndPlan` критик+улучшатель, независимый `scoreOnly` другой моделью (Acceptance-independence); skill-advisor (advisor+judge); scene-memory good/bad/corrections = **DPO-сырьё**, но тренера/чекпоинта НЕТ — достраивает `44.` |

**Как пользоваться навигатором:** два направления расширения делят один core-boundary
(R6). Направление **skill-selection/token-optimizer** (Pass 1–7) — это «host-адаптер
поверх ядра выбора скиллов»; направление **design→code #4** (Pass 8–9) — «design-плагин
поверх ядра `44.`». Общий шаблон адаптера (Pass 7 §P) и реальный код (Pass 6) — одни и
те же для обоих; отличается только ядро, которое зовёт core-call.

### 10.2. TRANSFER CHECKLIST — активы папки #11 → отображение на VS Code / Visual Studio расширение

| # | Переносимый актив (где в RAG-11) | Как ложится в continue-расширение VS Code/Visual Studio |
|---|---|---|
| T1 | **Реальный thin-adapter спайн `extension.ts`** (Pass 6 §2–§4) | Cursor = форк VS Code (`engines.vscode ^1.90`) → **host 4 (VS Code) = переиспользовать спайн 1:1**: `activate()`→`runSmartSkillsRequest`→`buildCoreRequest`→`invokeCoreApi`(spawn powershell)→`normalizeResponse`→display. Срезать >80% evidence-балласта; починить traceRefs-баг (§7). |
| T2 | **Унифицированный host-adapter контракт «5 ног»** (Pass 7 §P) | Заполнить 5 ног для 2 новых хостов: **host 4 VS Code** = `package.json contributes/activationEvents/commands` + registerCommand/chat-participant/webview; **host 5 Visual Studio** = `source.extension.vsixmanifest` + VSPackage/`.vsct` + `Process.Start()` тот же core-call. Инвариант: `hostDecisionAuthority:false`. |
| T3 | **PowerShell core-api транспорт + `XiigenExtensionRequest/Response`** (Pass 6 §4, Pass 7 §L.3) | Транспорт неизменен для всех хостов: `powershell -NoProfile -ExecutionPolicy Bypass -File <core-api>.ps1 -RequestPath -ResponsePath -TracePath`, обмен JSON-файлами, `$LASTEXITCODE`=гейт. VS может звать core-exe напрямую — контракт JSON тот же. **Сначала унифицировать 2 core-api → 1** (Pass 7 §R.1). |
| T4 | **SkillSelectionMachine амбиция ↔ 2-машинная реальность** (Pass 1 §E, Pass 2 §P) | Ядро выбора скиллов = `PromptToTaskMachine` + `OrchestrationReasoningMachine` (`43.SmartSkillsDomain`, 36 .cs), НЕ 11 отдельных машин. Continue-план обязан сверить: покрывают ли 2 машины 11 семантических доменов плана или часть отброшена; есть ли реальные 50+50 DPO и actual-метрики (Gate B-2/B-3/B-14) или только static-контракты. |
| T5 | **Token-optimizer детерминированные хуки + Codex token-usage** (Pass 4 §AA/§AD, Pass 5 §AH) | Переиспользуема УПАКОВКА (событийный каркас hooks.json, per-host порт-маппинг, `beforeReadFile`-компрессия, licensing), НЕ детерминированная семантика. **Гейт «savings proven»** = отдельный measurement-слайс на `codex exec --json` (точные `prompt/completion/reasoning_tokens`) baseline vs plugin, а не таблицы-оценки. |
| T6 | **Path B per-host blueprint + Wave1 `.vsix` build order** (Pass 5 §AG–§AK) | Готовый Path B: baseline (выкл. плагин) → 5 задач (BUG/PLAN/ARCH/CODE_Q/REFACTOR) → включить → evidence. **Разрыв: нет выделенного Path B для VS Code-панели и Visual Studio** — писать по образцу Cursor-плана, но для команды/панели. Build: Wave1 один `.vsix` → VS Code+Cursor+Windsurf+Open VSX; Wave2 Visual Studio (чистейшая .NET). Installer/sidecar-паттерн = governance-требование. |
| T7 | **FigmaToCode 18-шаг движок + IR `Element/Style` + `op-executor`** (Pass 8 §8.2–§8.6) | Направление #4: взять детерминированно (а) `styles.ts` 18-шаг Figma→HTML+CSS, (б) IR `Element`-дерево + `Style.settings`, (в) 17-оп дельта-модель + `op-executor`, (г) `debugHistory` трассу. НЕ переносить: «ML»-классификаторы (§8.5, зашитые пороги) — это eval-only helper, не trained-unit (R4). |
| T8 | **Петля критик→улучшатель→scoreOnly→scene-memory** (Pass 9 §9.3–§9.7) | Взять архитектуру петли (`compareAndPlan` score+ops, независимый `scoreOnly` ДРУГОЙ моделью = Acceptance-independence, downgrade-detect+replay best), но **заменить внешние Claude/OpenAI на `IDesignConversionCriticUnit`/`IDesignConversionImproverUnit` из `44.`** (R7 external-tool-ban, R5 модель-в-репо). |
| T9 | **like/dislike → corrections → DPO-сырьё** (Pass 9 §9.6, §9.10) | `SessionEntry{userFeedback:good/bad/unrated, corrections[], score}` + `HistoryRecord.liked` = формат сырья: chosen←good/liked, rejected←bad, diff←`corrections[kept]`, reward←score/regionScores. Скормить `44.DesignConversionDpoGenerator`→`DpoDatasetBuilder`→`DpoStorage`. **Недостающая часть (тренер/чекпоинт/held-out) — из `44.` + `11.DPO/DpoTrainer`.** |
| T10 | **MCP единый канал + VS Code Marketplace/Open VSX дистрибуция** (Pass 3 §T, Pass 5 §AJ) | Один MCP/core-endpoint покрывает Claude+Cursor+Copilot сразу (97M SDK-загрузок/мес). Дистрибуция: VS Code Marketplace (вход автоматический, без ревью, данные публичные) + Open VSX (Cursor/Windsurf/VSCodium/Theia почти бесплатно). Freemium/licensing (Pass 4 §AE) = поздний слайс. |

### 10.3. ФИНАЛЬНЫЙ ВЕРДИКТ папки #11 «extensions reasearch»

Папка = **источник ТРОЙНОГО назначения**: (1) первые ПЛАНЫ расширений (skill-selection
амбиция, marketplace, token-optimizer, local-testing, desktop-adaptation), (2) РЕАЛЬНЫЙ
код расширений (3 host-репо: cursor `extension.ts` + claude/codex `*-entrypoint.ps1`),
(3) движок design→code (FigmaToCode: детерминированный CSS-движок + обучающая петля).
Это самая высокорелевантная папка для continue-плана VS/VSCode во всём корпусе — тут
одновременно и чертежи, и рабочий код, и engine-референс.

**Что папка ПРЕДОСТАВЛЯЕТ (готово к переиспользованию, доказано чтением кода):**
- **Реальный thin-adapter эталон** (`extension.ts` 1629 стр, R6 PASS) — не скелет, работает; готовый спайн VS Code-расширения.
- **Шаблон host-адаптера «5 ног»** (Pass 7 §P) — доказано на 3 хостах, что оболочка сводится к ~6 полям над одним ядром; прямой blueprint для host 4 (VS Code) и host 5 (Visual Studio).
- **Детерминированный design→code движок** (18-шаг styles.ts + IR + op-executor) — богатый рабочий референс для направления #4, near-1:1 на `44.DesignConversionDomain`.
- **Обучающий контур-петля с feedback** (критик→улучшатель→независимый score→scene-memory good/bad/corrections) — готовое DPO-сырьё и контур измерения качества.
- **Path B blueprint + build order** (VS Code Wave1 один `.vsix`→4 хоста, VS Wave2) + sidecar/installer governance-паттерн.
- **Реальные host-run доказательства границы** — Claude `claude.exe --plugin-dir` exit0; Codex `codex exec` реально исполнил entrypoint + измерил token-usage.

**Что ОТКРЫТО (риски/пробелы, которые continue-план ОБЯЗАН закрыть):**
- **O1 — Два расходящихся core-api + два .cs-контракта** (`phase40-extension-core-api.ps1`/`Phase40ExtensionContracts.cs` для Claude/Codex vs `xiigen-extension-core-api.ps1`/`XiigenExtensionApiContracts.cs` для Cursor). Унифицировать в ОДИН перед host 4/5, иначе 5 хостов = 2 ядра.
- **O2 — `runtime_host_proof:false` во всех host-репо** — доказана только ГРАНИЦА core-call (контракт-харнесс, `deterministic_contract_check_not_learning`), а НЕ обученный выбор скиллов и НЕ полный host-UI (Path B). Boundary-proof ≠ trained-proof ≠ host-readiness.
- **O3 — Нет тренера/чекпоинта/held-out** — общий разрыв `44.` + FigmaToCode-форка: обе стороны честно помечены `RUNTIME_SCAFFOLD_NOT_TRAINED`/`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`. Контракт есть, обученной модели нет.
- **O4 — Savings projected, не proven** — все проценты (~50-70%) = архитектурные оценки; реальный measurement-контур (Codex JSON) ещё не прогонялся.
- **O5 — react-native не проэкземплярён** (упомянут в `SkillInjector`, но НЕ в `CLIENT_STACKS`); фреймворки Любы (angular/react/vue) покрыты внешним LLM, а не детерминированно/ядром → нарушение R7 при переносе как есть.
- **O6 — Дрейф статус-метаданных/манифестов** (`.cursor`-манифест устарел; claude пустой `hooks/hooks.json` vs реальный hook в `plugin.json`; `entrypointImplemented:false` при работающем `.ps1`; `containsPrivateAbsolutePath:true`).
- **O7 — Нет выделенного Path B для VS Code-панели и Visual Studio** — 3 существующих плана про хуковую модель, не про панельную команду `xiigenSmartSkills.panel`.

**Итоговая формулировка для доски continue-плана:** папка #11 даёт **готовый спайн +
шаблон + движок + сырьё**, но НЕ даёт **обученную модель, единое ядро-контракт, доказанный
host-runtime и измеренные savings**. Приоритетные ряды: O1 (унификация core-api) → O2/O7
(Path B для VS Code/VS панели) → O3 (тренер/чекпоинт через `44.`+`11.DPO`) → O4
(measurement на Codex) → O5 (react-native/фреймворки в ядре).

### 10.4. Честное покрытие всех 2200 файлов

`find . -type f | wc -l` = **2200** (подтверждено этим проходом). Распределение и статус:

| Область | Файлов | Статус покрытия | Проход |
|---|---|---|---|
| Top-level PLAN-*.md (skill-selection v1, token-optimizer×4 v1, local-testing×3 v1) | 8 | **Прочитано deep целиком** | 1,2,4,5 |
| Top-level marketplace (2 master + 4 STATE/INDEX/CONFLICT) | 6 | **Прочитано deep целиком** | 3 |
| `XIIGen-Desktop-Plugin-Adaptation-Report-v2.md` | 1 | **Прочитано deep целиком** | 5 |
| `cursor-xiigen-smart-skills/` (1617) | 1617 | **Реальный код прочитан** (`extension.ts` 1629 стр, `package.json`, contract, 3 скрипта, README); **evidence-балласт спот-чек** (~628 `reports/`, phase40-rerun-папки, `dist/`, `test/`) | 6 |
| `claude-xiigen-smart-skills/` (102) | 102 | **Код+манифесты прочитаны** (`claude-entrypoint.ps1` 247 стр, `plugin.json`, `hooks.json`, README, smoke); **phase40-трейсы спот-чек** | 7 |
| `codex-xiigen-smart-skills/` (93) | 93 | **Код+манифесты прочитаны** (`codex-entrypoint.ps1` 247 стр, `.codex/*.json`, README, smoke); **phase40-трейсы спот-чек** | 7 |
| `FigmaToCode-design-gen-improve_group_css_user/` (346) | 346 | **Движок+design-gen прочитаны deep** (controller/op-executor/styles/element-code/classifiers/scene-memory/models + design-gen providers/skill-advisor/ai-enhance + manifest); `docs/`-архив (GEOMETRIC-LEARNING/PIPELINE_FLOW/MASTER-STATE/xlsx) — упомянут, не поштучно | 8,9 |
| `Plans/` (15) | 15 | **Категоризировано** (C5 Canva/F1 Framer/W1 Webflow/M1 Miro); M1 (7 файлов) разобран; остальные — zip-архивы (перечислены, не распакованы) | 9 |
| `other/` (6) | 6 | **Прочитано/категоризировано** (marketplace-traffic research) | 3,9 |
| `skill-selection-machine-runs/` (0) | 0 | **ПУСТА** — зарезервирована под run-root Gate B (Pass 2 §K); рантайм-прогонов нет | 1 |
| **НЕ прочитано — off-topic PDF:** `PLAN--3D-Lesson-Generation-Pipeline-v1.md.pdf` (810KB), `XIIGEN - 3D Hero Interaction Pipeline.pdf` (1.3MB) | 2 | **Пропущено осознанно** — 3D-контент-пайплайны, вне темы расширений IDE (Pass 1 flag «шум») | — |
| **НЕ прочитано — дубликаты-экспорты:** `XIIGen-Desktop-Plugin-Adaptation-Report.docx` (27KB) + `...-v2.md.pdf` (999KB) + `FigmaToCode-....zip` (3.8MB) | 3 | **Пропущено** — форматные экспорты/архив уже прочитанного .md/папки (`unzip -l` подтвердил идентичность) | — |
| **НЕ прочитано — `V2 files.zip` (7 файлов внутри):** token-optimizer v2/v3 (Claude v3, Codex/Cursor v2, Licensing v3) + local-testing (Claude v2, Codex/Cursor v1) | 1 (архив) / 7 внутри | **ЧАСТИЧНЫЙ ПРОБЕЛ** — читались только v1 loose-файлы; **v2/v3 (это те самые «зависит от v2/v3» из Pass 4/5) внутри архива НЕ прочитаны**. Не блокер (v1 разобраны полностью, дельты минорны), но continue-план при точной сверке версий должен распаковать `V2 files.zip` | — |

**Честный итог покрытия:** из 2200 файлов **весь смысловой корпус прочитан или
категоризирован**: все планировочные `.md` верхнего уровня, реальный код 3 host-репо,
движок+петля FigmaToCode, Plans/other. Осознанно НЕ читалось: 2 off-topic 3D-PDF, 3
формат-дубликата (docx/pdf/zip уже прочитанного), масса evidence-балласта в
`cursor/reports/` + phase40-rerun-папки (спот-чек репрезентативной выборки, не поштучно —
это машинные JSON-трейсы одного класса). **Единственный содержательный пробел** —
`V2 files.zip` (7 файлов, v2/v3 планов): при continue-планировании их стоит распаковать
для точной сверки версий, но выводы RAG-11 на v1 полностью валидны. `skill-selection-
machine-runs/` пуста — рантайм-прогонов не было (подтверждает O2/O4: нет живого
host-runtime и measured savings).

**Метаданные Pass 10/10 (ФИНАЛ):** RAG-файл дополнен секцией 10 (append-only, разделы
10.1–10.4); ранние секции (Pass 1–9, A–9.11) НЕ тронуты. Новый корпус не читался —
перечитан весь RAG-11 (1410 стр) + top-level sweep (`find`=2200, `unzip -l` для
`V2 files.zip`/FigmaToCode-zip). Это закрывает папку #11/12. Открытые ряды для
continue-плана: O1 (единый core-api) → O2/O7 (Path B VS Code/VS панель) → O3 (тренер via
`44.`+`11.DPO`) → O4 (measurement на Codex) → O5 (react-native/фреймворки в ядре).


## Pass 10b/10 — V2 files.zip (token-optimizer v2/v3 + local-testing v2): дельта vs v1 + что суперсидит

> GAP-CLOSE проход (10b/10): закрывает единственный содержательный пробел, честно помеченный в §10.4 (строка 1507) — `V2 files.zip` (7 файлов) НЕ читался; проходы 4/5 разобрали только v1 loose-файлы, а «зависит от v2/v3»-ссылки (§AG стр.482, §AH стр.497, §AI стр.513) остались непроверенными. Этот проход распаковал архив read-only в scratchpad, сделал построчный `diff` каждого v1 (loose в папке) против его v2/v3 (в архиве) и извлёк ТОЛЬКО дельту. Корпус папки не менялся; изменён только этот RAG (append). Ранние секции (Pass 1–10, A–10.4) не тронуты.

### 10b.0. Состав архива + честная сверка идентичности (7 файлов)

`unzip -l "V2 files.zip"` → 7 файлов, все с меткой 2026-05-07 13:12:

| # | Файл в архиве | Версия | Против чего дельта | Идентичность |
|---|---|---|---|---|
| 1 | `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v3-CLAUDE.md` (25429 б) | **3.0** | v1-CLAUDE (loose) | реальная дельта |
| 2 | `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CODEX-PORT-v2.md` (14940) | **2.0** | CODEX-PORT-v1 | реальная дельта |
| 3 | `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CURSOR-PORT-v2.md` (15021) | **2.0** | CURSOR-PORT-v1 | реальная дельта |
| 4 | `PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-LICENSING-v3.md` (13790) | **3.0** | LICENSING-v1 | реальная дельта |
| 5 | `PLAN-LOCAL-TESTING-CLAUDE-CODE-v2.md` (18323) | **2.0** | LOCAL-TESTING-CLAUDE-v1 | реальная дельта |
| 6 | `PLAN-LOCAL-TESTING-CODEX-v1.md` (17082) | 1.0 | — | **`diff -q` = БАЙТ-В-БАЙТ идентичен** loose v1 (Pass 5 §AH уже прочитан) |
| 7 | `PLAN-LOCAL-TESTING-CURSOR-v1.md` (17203) | 1.0 | — | **`diff -q` = БАЙТ-В-БАЙТ идентичен** loose v1 (Pass 5 §AI уже прочитан) |

Значит настоящая дельта — в 5 версионированных файлах (#1–5); #6/#7 — дубликаты v1, ничего нового. Отсутствует `v2-CLAUDE` (архив несёт сразу v3 — накопительную); нумерация ИСПРАВЛЕНИЙ сквозная по семейству: fix 1–3 (v3-CLAUDE), fix 4 (local-testing-claude v2), fix 5 (licensing v3).

### 10b.1. КРИТИЧЕСКИЙ guard: архив ПРЕДШЕСТВУЕТ июньскому Phase-39 баннеру — Hook 2 остаётся superseded

Ловушка версий, которую continue-план обязан не проглотить. `diff` показывает, что v2/v3-файлы «удаляют» шапку `> HOOK 2 SUPERSEDED — Phase 39 (2026-06)` (строки 1–8). Это **НЕ** решение v3 «отменить supersession». Доказательство таймлайна:
- loose v1-файлы в папке датированы **Jun 4** и НЕСУТ июньский баннер (`head -1` = `> **HOOK 2 SUPERSEDED — Phase 39 (2026-06)**`);
- архивные v2/v3-файлы датированы **May 7** и баннера НЕ содержат (`grep -c "Phase 39\|SkillSelectionMachine\|SUPERSEDED"` = **0**).

Т.е. архив — более РАННИЙ снимок (May 7), сделанный ДО того, как в июне в loose-v1 допечатали баннер. Оси разные: v2/v3 новее по оси «исправления упаковки хуков», но старше по оси «июньская supersession». **Итог: находка Pass 4 §AB (Hook 2 BM25 `SkillSelector` → обучаемая `SkillSelectionMachine`, Phase 39) ПОЛНОСТЬЮ В СИЛЕ и v2/v3 её не отменяют.** Более того, сам июньский баннер говорил «структура упаковки хука остаётся валидной опорой для Phase 40» — а v3 именно эту упаковку и чинит (ниже). Так что v3-фиксы УЛУЧШАЮТ переиспользуемую упаковку, поверх которой встанет обученная машина.

### 10b.2. ДЕЛЬТА v3-CLAUDE (token-optimizer, главный план) — 3 исправления упаковки хуков

Diff v1-CLAUDE → v3-CLAUDE. Три содержательных фикса (плюс шапка `Version: 3.0` и комментарий-сводка «v3 исправления» в конце файла):

**ИСПРАВЛЕНИЕ 1 — `depends_on` УБРАН, заменён shared-state-файлом (СУПЕРСИДИТ переиспользуемый актив).** v1 связывал Hook 2 `Skill Selector` с Hook 1 через поле `depends_on: "orchestration-router"` на общем событии `UserPromptSubmit` (Pass 4 §AA стр.368 «depends_on хук1», §AB стр.381 «depends_on цепочка», §AF стр.440 перечисляет «depends_on» как валидную переиспользуемую упаковку). **v3: Claude Code НЕ поддерживает зависимости между хуками на одном событии — хуки на `UserPromptSubmit` запускаются НЕЗАВИСИМО.** Новый механизм обмена: `Orchestration Router` пишет результат в `.claude/hook-state.json` (`{domain, subPhases, ts}`), `Skill Selector` читает этот файл при старте (env `HOOK_STATE_PATH`, дефолт `.claude/hook-state.json`). Если файла нет / domain пуст — Skill Selector не падает, возвращает топ-3 скилла по алфавиту. Новый файл в структуре проекта: `HookState.cs` (структура shared-состояния, shared между хуками). → **Правит доску: в T5/§AF «событийный каркас hooks.json (…+ depends_on…)» переиспользуемый актив нужно ЗАМЕНИТЬ — `depends_on` невалиден, корректный паттерн обмена между хуками одного события = общий `hook-state.json`. Прямой урок для VS Code/Visual Studio-версии, если она сохранит event-hook модель: не закладывать depends_on, использовать shared-файл состояния.**

**ИСПРАВЛЕНИЕ 2 — `block_on_exit_nonzero` + `injectionForNextPrompt` помечены КАК ТРЕБУЮЩИЕ ВЕРИФИКАЦИИ; введена Phase 0 (СУПЕРСИДИТ уверенное утверждение §AA).** v1/§AA (стр.371-373) уверенно описывал Hook 5 Task Completion Guard: «verify при coverage<1.0 → exit code 1 + injectionForNextPrompt → Claude Code НЕ показывает финал, продолжает», и порядок разработки «1) TaskCompletionGuard (ценнее всего, standalone)». **v3: этот механизм НЕ подтверждён — если Claude Code при exit code 1 из `Stop`-хука только ЛОГИРУЕТ предупреждение (а не блокирует и не инжектит), TaskCompletionGuard не работает как описано.** Введена **Phase 0 — обязательная верификация до TaskCompletionGuard**: собрать минимальный `test-stop-hook` (возвращает `{complete:false, injectionForNextPrompt:"VERIFICATION TEST…"}` + `Environment.Exit(1)`), запустить `claude --plugin-dir ./test-stop-hook`, проверить, появилось ли инжектированное сообщение в следующем ходе. Если ДА — строить Guard как описано; если НЕТ — **Вариант B**: хук пишет в `.claude/incomplete-task.md`, следующий `SessionStart`-хук читает и инжектит через `additionalContext`. **Порядок разработки ПЕРЕВёрнут:** Phase 0 (верификация Stop-хука) → Phase 1 `SkillSelector + HookState` (устанавливает паттерн shared-state) → Phase 2 `TaskCompletionGuard` (после подтверждения) — а НЕ «Guard-first» из v1. → **Правит доску: O4/T5 «TaskCompletionGuard как detection-сигнал» несёт скрытую зависимость от НЕподтверждённого поведения хоста; VS Code/Visual Studio-версия обязана начать с эквивалента Phase 0 — проверить, реально ли host блокирует/инжектит на nonzero-exit, прежде чем закладывать блокирующий гейт завершения.**

**ИСПРАВЛЕНИЕ 3 — `SkillFrontmatterReader` получил явный fallback (робастность).** v1 читал ТОЛЬКО YAML frontmatter (`name` + `description`) из SKILL.md. **v3: двойной fallback** — если YAML frontmatter отсутствует: берёт первый `# заголовок` как name + первый абзац как description; если и того нет: имя файла как name + пустое описание. Не роняет пайплайн на скиллах без frontmatter. Минорно, но снимает крэш-риск.

### 10b.3. ДЕЛЬТА LICENSING v3 — честная оговорка о безопасности HMAC (fix 5)

Diff LICENSING-v1 → v3. Одно содержательное добавление (Вариант A, офлайн-ключ). v1/§AE (стр.429) утверждал «Пользователь не может сгенерировать валидный ключ — только расшифровать payload». **v3 добавляет честную оговорку:** секрет встроен в скомпилированный AOT-бинарник; ILSpy/Ghidra позволяют извлечь строки из нативного бинарника — «нетривиально, но выполнимо для опытного человека». Точная формулировка: ключ нельзя подделать без реверс-инжиниринга бинарника — «достаточная защита для продукта за $49, стоимость обхода значительно превышает стоимость лицензии». **НЕ позиционировать как «криптографически защищённое»; при первых продажах перейти на Вариант B (онлайн-активация), не зависящий от секрета в бинарнике.** → Правит §AE/T10: Вариант A остаётся рекомендацией для старта, но с явным ограничением безопасности; это honesty-уточнение, не смена архитектуры.

### 10b.4. ДЕЛЬТА CODEX-PORT v2 + CURSOR-PORT v2 — additive: zero-cost licensing + notification (+Cursor beforeReadFile)

Diff v1 → v2 для обоих портов. Ни одна старая находка §AC не отменена; в каждый добавлены две OPTIONAL-секции (доп. работа ≈ ноль, т.к. те же байтовые exe, что в Claude-версии):
- **Лицензирование:** те же exe — лицензирование уже встроено; ключ читается из env `XIIGEN_LICENSE_KEY` одинаково во всех хостах (Codex/Cursor подставляют `${XIIGEN_LICENSE_KEY}` из окружения через `env`-блок в hooks.json). Доп. работа: **0 строк**.
- **Нотификация в чате:** те же exe показывают `systemMessage` в stdout хука — Codex и Cursor читают JSON stdout идентично Claude Code. Пример ссылки: `💡 XIIGen Free: loaded top 3 skills. Pro: https://you.lemonsqueezy.com/xiigen-pro`. Доп. работа: **0 строк**.
- **Cursor-УНИКАЛЬНОЕ (дополняет §AC Hook 6):** через `beforeReadFile` можно показать Free-tier нотификацию ДО чтения файла — единственный хук, где нотификация появляется до ответа модели: `FileContentCompressor` при Free-tier отдаёт `content = RegexPreprocessor.Normalize(...)` (базовое сжатие) + `systemMessage = "…basic comment stripping. Pro: semantic compression (-40% more)."`. Доп. работа: **~5 строк** в FileContentCompressor (только Cursor).

→ Дополняет §AC/§AE: подтверждает «core = те же exe, host = конфиг»; монетизация и нотификация переносятся между хостами за 0 строк, кроме Cursor-уникального `beforeReadFile`-хука (~5 строк). Для continue-плана VS Code/Visual Studio: licensing/notification — не per-host работа, а env + один общий exe-механизм.

### 10b.5. ДЕЛЬТА LOCAL-TESTING-CLAUDE v2 — ТОЧНЫЕ токены из JSONL (fix 4, СУПЕРСИДИТ метод измерения)

Diff LOCAL-TESTING-CLAUDE-v1 → v2. Самая важная дельта для целевого исхода «savings proven». v1/§AG (стр.473) + сводная таблица §AJ (стр.521) описывали измерение токенов Claude как **оценку**: `token-meter.exe` на `Stop` «оценивает токены (~4 символа = 1 токен)»; таблица §AJ прямо помечала Claude «из транскрипта ~4 chars/token (оценка)» в отличие от точного Codex. **v2 СУПЕРСИДИТ это:** `TokenMeter` теперь читает РЕАЛЬНЫЕ счётчики из JSONL-транскрипта — Claude Code пишет `usage.input_tokens`, `usage.output_tokens`, `usage.cache_read_input_tokens` в каждое сообщение; TokenMeter парсит построчно (`JsonSerializer.Deserialize<JsonElement>` на строку, `try/catch` на битых строках) и суммирует `inputTokens/outputTokens/cacheReadTokens/totalTokens`. `compare.sh` переписан на `jq` — точный per-message и суммарный вывод (`Input tokens / Output tokens / Cache read tokens / Total`), а не `wc -c / 4`. Итоговая таблица результатов получила НОВЫЕ колонки «Input токены без / Input токены с / Дельта%» (токены — точные из JSONL; время/retry — ручные). Плюс: **задачи рекомендовано подавать скриптом (pipe / `--print`), не вручную** — ручной ввод даёт вариативность. Fallback: если `usage`-полей в транскрипте нет — использовать `token-meter.exe`-хук, пишущий в `hook-log.jsonl`; явно сказано «оценка ~4 chars/token недостаточно точна: 10% дельта baseline↔plugin может быть в пределах погрешности оценки».

→ **Правит доску (важно для O4/T5/T6):**
- Сводная таблица §AJ (стр.521), строка «Данные о токенах: Claude = ~4 chars/token (оценка)» — **УСТАРЕЛА**: Claude теперь ТОЖЕ даёт ТОЧНЫЕ токены (из `usage.input_tokens` JSONL), как Codex. «Точный measurement-контур» больше НЕ только-Codex — он **Claude + Codex** (у обоих точный источник токенов).
- §AG «evidence Path B Claude» и §AD/§AK «savings projected, не proven»: статус «proven» по-прежнему НЕ достигнут (прогон не выполнялся — `skill-selection-machine-runs/` пуста, O2/O4), НО инструмент измерения на Claude доведён до точного; остаётся только ПРОГНАТЬ baseline vs plugin и снять дельту. Т.е. O4-разрыв сузился с «нет точного инструмента на 2 из 3 хостов» до «инструмент точный на Claude+Codex, не хватает самого прогона».

### 10b.6. Итог 10b: что суперсидит v1 + меняет ли transfer-checklist

**Суперсидит конкретные v1-находки RAG (continue-план обязан взять НОВЕЙШУЮ версию):**
1. §AA/§AB/§AF «`depends_on` цепочка» как валидная переиспользуемая упаковка → **v3: depends_on невалиден на одном событии; корректный паттерн = shared `hook-state.json`** (10b.2 fix 1).
2. §AA (стр.371-373) уверенное «Guard verify блокирует через exit code 1 + injectionForNextPrompt» и «Guard-first порядок» → **v3: механизм НЕ верифицирован; сначала Phase 0-проверка Stop-хука; порядок Phase0→SkillSelector+HookState→Guard; иначе Вариант B (incomplete-task.md + SessionStart additionalContext)** (10b.2 fix 2).
3. §AG (стр.473) + §AJ-таблица (стр.521) «Claude токены = ~4 chars/token оценка» → **v2: точные `usage.input_tokens` из JSONL; Claude+Codex оба точные** (10b.5 fix 4).

**Уточняет (не отменяет):** §AE licensing — честная security-оговорка про HMAC-секрет в бинарнике (10b.3 fix 5); §AC per-host — zero-cost licensing/notification в Codex/Cursor + Cursor `beforeReadFile`-нотификация (10b.4).

**Меняет ли transfer-checklist (§10.2)?** Да, две правки — обе уточнения, не новые ряды:
- **T5** (переиспользуемая упаковка token-optimizer): вычеркнуть `depends_on` из списка валидной упаковки, вписать «shared `hook-state.json` как межхуковый обмен»; добавить оговорку «блокирующий гейт завершения (exit-nonzero) требует host-верификации (Phase 0) перед закладкой».
- **T6** (Path B blueprint + measurement): Claude-нога Path B теперь несёт ТОЧНЫЙ токен-инструмент (JSONL `usage.*`), не оценку; «proven»-контур = Claude+Codex, не только Codex. O4 сузился до «нужен сам прогон», инструмент готов.

**Не меняет:** финальный вердикт §10.3 и ряды O1/O2/O3/O5/O6/O7 (core-api унификация, host-runtime proof, тренер/чекпоинт, react-native, дрейф манифестов, Path B для панели) — дельты v2/v3 их не касаются. Phase-39 supersession Hook 2 (10b.1) — В СИЛЕ.

**Метаданные Pass 10b/10:** RAG дополнен секцией 10b (10b.0–10b.6, append-only); ранние секции (1–10.4) не тронуты. Прочитаны через построчный `diff` v1↔v2/v3 все 5 версионированных файлов архива `V2 files.zip`; #6/#7 подтверждены байт-идентичными loose-v1 (`diff -q`). Это ЗАКРЫВАЕТ единственный содержательный пробел §10.4 (стр.1507) — папка #11 теперь без пренебрежения. Источник read-only; архив распакован в scratchpad, корпус папки не изменён.
