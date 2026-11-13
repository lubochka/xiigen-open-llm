# Опись skill-библиотеки как AI extension

Дата: 2026-06-02.
Источник: `C:\Projects\llm_mvp_core\docs\skills`, `docs\HOW-TO-USE-SKILLS.md`, `AGENTS.md`, `CLAUDE.md`, `docs\SESSION-START-PROMPT.md`.
Живой счётчик на момент анализа: 164 файла `*-SKILL.md`.
Важно: в `docs\HOW-TO-USE-SKILLS.md` заголовок всё ещё говорит про 159 skills, а живой каталог содержит 164. Это не ошибка анализа, а дрейф каталожного текста относительно файловой системы.

## 1. Короткий вывод

Skill-библиотека уже выглядит не как набор подсказок, а как слой управления агентом. Её можно разрезать на две части.

Первая часть универсальна для любого серьёзного проекта: режимы сессии, архитектор, планирование, product decisions, review, арбитры, доказательства перед completion claim, управление scope, диагностика, anti-soft-stop правила, matrix coverage, state tracking, audit, escalation и работа с learning signals. Эти вещи не зависят от C#, .NET, конкретных подпроектов или структуры XIIGen.

Вторая часть уникальна для этого проекта: правила `OperationResult<T>`, `BaseService`, "каждая строка production code имеет комментарий", `.NET 9`, `xUnit`, `dotnet test`, запрет внешних ML frameworks, `TensorCore`, деление на подпроекты `00..43`, смыслы `42.ProjectWikiDomain`, `43.LearningLocalWikiData`, `27.TranslationDomain`, Windows Forms UI, конкретные manifest/checkpoint/locator правила и текущие ветки/коммиты/планы.

Для AI extension это значит: универсальное ядро должно хранить skill-механику и арбитров, а проектный адаптер должен подставлять stack rules, project map, commands, artifact schema и domain-specific guards.

## 2. Что такое skill в этой системе

Skill здесь не просто "инструкция как сделать задачу". В проекте skill выполняет одну из ролей:

- роутер поведения: когда включается архитектор, executor, reviewer, investigation или translation mode;
- gate: что надо проверить до планирования, до кода, до коммита или до completion claim;
- arbiter: отдельная перспектива проверки, например goal delivery, principles, scope isolation, test integrity;
- protocol memory: записывает уже найденные failure modes, anti-patterns и запрещённые loopholes;
- context package: даёт агенту компактный набор правил для конкретного класса задач;
- project adapter: привязывает универсальную дисциплину к C#/.NET/XIIGen реалиям.

## 3. Главная граница: universal core vs project adapter

### Универсальное ядро

Эти идеи можно переносить в любой проект:

- режим сессии должен быть явным;
- требования пользователя идут первыми, затем план;
- plan review начинается с requirements ledger;
- product decisions нельзя самовольно выбирать, если они меняют поведение, API или архитектуру;
- completion нельзя объявлять по наличию отчёта;
- review должен искать holes, а не красоту текста;
- evidence scope должен совпадать с claim scope;
- state, matrix и next action должны сходиться;
- "я могу продолжить", "следующий шаг", "commit сделан" не являются stop gate;
- execution должен быть bounded, если parent session является architect-only;
- арбитры проверяют разные классы риска раздельно;
- найденный gap является work order, а не поводом остановиться, если next action известен.

### Проектный адаптер

Эти части надо заменять при переносе в другой проект:

- `.NET 9`, C#, `xUnit`, `dotnet test`, `dotnet build`;
- `OperationResult<T>` и namespace rules;
- `BaseService(ISessionStorage storage)`;
- правило "каждая production строка имеет комментарий";
- запрет PyTorch/TensorFlow/ML.NET и разрешение только `TensorCore`;
- структура подпроектов `00.TensorCore`, `15.OrchestratorModel`, `27.TranslationDomain`, `42.ProjectWikiDomain`, `43.LearningLocalWikiData` и так далее;
- `D-01..D-15`, `A-LM-01..A-LM-08` как конкретные locked decisions;
- project-local wiki цель: Angular + .NET + SQL stored procedures как единая бизнес-система;
- TranslationDomain checkpoint writer guard;
- Windows Forms UI rules;
- конкретные commit message conventions и current branch rules;
- конкретные report/state/matrix файлы и artifact paths.

## 4. Универсальная архитектура AI extension

Если превращать это в расширение для AI, я бы делил его на такие слои.

### 4.1 Skill registry

Хранит список skills, триггеры, зависимости, conflict rules и portability flag.

Поля:

- skill id;
- title;
- when to load;
- lifecycle stage;
- role: planner, architect, reviewer, executor, arbiter, verifier, domain guard;
- universal / mixed / project-specific;
- required upstream docs;
- forbidden overrides;
- output contract.

### 4.2 Session router

Решает, какие skills загрузить по текущему запросу.

Универсальные inputs:

- пользователь просит план;
- пользователь просит review;
- пользователь просит implementation;
- пользователь спрашивает про completion или holes;
- пользователь указывает домен;
- пользователь просит checkpoint/training/model/plugin/runtime доказательства.

Проектные inputs:

- путь внутри конкретного repo;
- target stack;
- test/build commands;
- active plan/state paths;
- project-specific domains.

### 4.3 Arbiter panel

Арбитры должны быть универсальными как типы, но project-specific как критерии.

Универсальные арбитры:

- Goal Delivery Arbiter: доставляет ли работа исходную цель пользователя;
- Requirements Ledger Arbiter: все ли требования mapped;
- Product Decision Arbiter: нет ли самовольного выбора между архитектурными вариантами;
- Scope Isolation Arbiter: не полезли ли в чужую область;
- Principles Arbiter: соблюдены ли базовые engineering rules;
- Evidence Scope Arbiter: доказательство покрывает именно заявленный claim;
- Completion Coverage Arbiter: matrix закрывает все literal requirements;
- Runtime Evidence Arbiter: runtime claim требует runtime proof;
- Test Integrity Arbiter: тесты не были ослаблены ради зелёного результата;
- Output Readability Arbiter: ответ человеку понятный, без внутренних кодов там, где они не нужны;
- Meta Arbiter: когда арбитры конфликтуют, решает, какой уровень проверки нужен;
- Escalation Orchestrator: поднимает спор на Luba только когда safe next action нельзя вывести локально.

Проектные критерии для этих арбитров подставляются адаптером: например, в этом repo Principles Arbiter проверяет `OperationResult<T>`, Fabric First, comments, no external ML frameworks.

### 4.4 Evidence engine

Универсальная логика:

- не доверять `passed: true` без scope check;
- отличать static contract, build proof, runtime proof и product proof;
- хранить matrix rows;
- сравнивать expected literal set и actual artifact;
- запрещать completion, если state/matrix/report расходятся.

Проектный адаптер:

- где лежит `STATE.json`;
- где runner reports;
- какие поля обязательны: CPU, RAM, elapsed time, accuracy, loss, reward margin;
- какие команды можно запускать;
- какие команды запрещены без Gate B.

## 5. Категории skills

### 5.1 Session governance и output contracts

Используются, чтобы сессия не расползалась, не теряла цель и не завершалась на checkpoint.

Skills:

- `session-constitution-SKILL.md`: верхний протокол старта/завершения сессии, subordinate to AGENTS/CLAUDE.
- `session-mode-declaration-SKILL.md`: явный режим ARCHITECT, PLANNER, REVIEWER, EXECUTOR или INVESTIGATION.
- `session-scope-resolution-SKILL.md`: выбор типа сессии и governance chain.
- `session-output-contract-SKILL.md`: что именно должен произвести раунд.
- `goal-context-persistence-SKILL.md`: verbatim goal и защита от drift.
- `session-state-crystallization-SKILL.md`: короткий snapshot состояния.
- `session-execution-log-SKILL.md`: структурированный execution log.
- `session-handoff-SKILL.md`: self-contained handoff.
- `session-file-authoring-SKILL.md`: правила для session files.
- `agent-output-format-SKILL.md`: формат plan/reference/state/execution файлов.
- `context-overflow-SKILL.md`: поведение при нехватке контекста.
- `investigation-handoff-SKILL.md`: передача discovery результатов.
- `phase-git-report-SKILL.md`: git report на границе фазы.

Универсальность: высокая. Проектными являются только имена файлов, state schema, commit/push conventions и команды.

### 5.2 Planning, architecture и product decisions

Используются до кода: извлечь требования, разложить work, найти prerequisites, запретить самовольные архитектурные решения.

Skills:

- `planning-session-startup-SKILL.md`: requirements ledger и стартовые вопросы.
- `intent-to-plan-SKILL.md`: перевод намерения в план.
- `phase-planning-SKILL.md`: 9-gate pre-code validation.
- `phase-preflight-SKILL.md`: проверка предположений перед execution.
- `plan-review-SKILL.md`: FC checks перед approval/execution.
- `plan-self-validation-SKILL.md`: Phase 0 для multi-phase plan.
- `plan-restructure-SKILL.md`: перестройка старого плана под новую инфраструктуру.
- `plan-execution-feedback-SKILL.md`: обратная связь о точности плана.
- `architect-behavior-classifier-SKILL.md`: failure modes архитекторского ответа.
- `architectural-decision-testing-SKILL.md`: проверка и переоценка decisions.
- `decision-capture-SKILL.md`: запись design reasoning.
- `decision-reopening-SKILL.md`: когда можно reopening locked decision.
- `no-product-decisions-SKILL.md`: запрет самовольного выбора product/architecture direction.
- `assumption-registry-SKILL.md`: список предположений и проверки.
- `context-package-design-SKILL.md`: формирует переносимый context package для capability или домена.
- `prerequisite-chain-SKILL.md`: prereq list и порядок.
- `solution-scope-gate-SKILL.md`: не over-engineer и не under-specify.
- `gap-to-proposal-SKILL.md`: перевод gap в proposal.
- `artifact-numbering-SKILL.md`: номера только из live canonical docs.
- `naming-conventions-enforcer-SKILL.md`: naming rules.
- `document-hierarchy-SKILL.md`: порядок документов и conflict resolution.

Универсальность: высокая. Проектная часть: Gate B commands, locked decisions, file paths, project numbering.

### 5.3 Review, claims, evidence и arbiters

Используются, чтобы не принять красивый отчёт за реальную готовность.

Skills:

- `audit-protocol-SKILL.md`: 10-point audit.
- `claim-verification-SKILL.md`: verified/inferred/assumed claims.
- `claim-as-hypothesis-SKILL.md`: пользовательский или модельный claim как гипотеза.
- `output-skepticism-SKILL.md`: skeptic questions перед result claim.
- `output-readability-gate-SKILL.md`: человекочитаемость и запрет внутренних кодов в prose.
- `verification-before-completion-SKILL.md`: evidence before completion.
- `coverage-completeness-gate-SKILL.md`: evidence scope equals claim scope.
- `goal-delivery-completeness-SKILL.md`: все элементы цели доставлены.
- `principles-arbiter-SKILL.md`: проверка базовых принципов.
- `meta-arbiter-SKILL.md`: арбитр над арбитрами.
- `meta-escalation-router-SKILL.md`: escalation when local decision tree не сходится.
- `escalation-orchestrator-SKILL.md`: route conflicts and undecided cases.
- `multi-reviewer-design-SKILL.md`: несколько reviewer roles, BLOCK overrides approved.
- `challenger-round-design-SKILL.md`: challenger prompts для проверки blind spots.
- `scope-isolation-arbiter-SKILL.md`: PRIVATE/SHARED/PLATFORM scope boundaries.
- `product-scope-validation-SKILL.md`: post-wave product validation.
- `specificity-calibration-SKILL.md`: насколько ответ конкретен.
- `self-verification-SKILL.md`: самопроверка результата.
- `self-questioning-SKILL.md`: обязательные вопросы к своему ответу.

Универсальность: очень высокая. Проектные критерии подключаются как policy pack.

### 5.4 Execution, code safety, tests и debugging

Используются, когда уже есть право на implementation или bounded work order.

Skills:

- `code-execution-SKILL.md`: реализация фаз и разрешённые команды.
- `code-examination-SKILL.md`: read before changing, callers/consumers.
- `dev-safety-SKILL.md`: safety перед commit и code changes.
- `blast-radius-SKILL.md`: оценка propagation.
- `blast-radius-tagger-SKILL.md`: tag deliverables по риску.
- `api-shape-verification-SKILL.md`: проверить signature перед тестами/кодом.
- `infrastructure-discovery-SKILL.md`: Gate 0 discovery.
- `change-propagation-SKILL.md`: что ещё надо обновить после change.
- `implementation-integrity-SKILL.md`: integrity checks нового class/interface.
- `dna-compliance-guard-SKILL.md`: project DNA compliance.
- `bug-to-tests-SKILL.md`: bug сначала превращается в failing tests.
- `test-integrity-SKILL.md`: не ослаблять тесты ради pass.
- `test-failure-triage-SKILL.md`: классификация failures.
- `three-level-verification-SKILL.md`: unit/integration/suite proof.
- `integration-testing-SKILL.md`: integration tests.
- `e2e-test-matrix-SKILL.md`: сценарии end-to-end.
- `user-journey-testing-SKILL.md`: пользовательские сценарии.
- `systematic-debugging-SKILL.md`: no fixes without root cause.
- `root-cause-tracing-SKILL.md`: trace backward from symptom.
- `root-cause-ladder-SKILL.md`: immediate/structural/architectural cause.
- `mental-debug-SKILL.md`: mental model for silent errors.
- `debug-session-SKILL.md`: persistent debug session.
- `event-driven-debugging-SKILL.md`: event bus chain debugging.
- `defense-in-depth-SKILL.md`: entry/business/environment/debug layers.
- `diagnostic-escalation-SKILL.md`: escalation after repeated diagnosis failure.
- `pipeline-bug-taxonomy-SKILL.md`: bug classes for pipeline failures.

Универсальность: mixed. Debugging/testing patterns универсальны, но `.NET`, C#, `xUnit`, project DNA и exact commands являются адаптером.

### 5.5 Capability design, service boundaries и system architecture

Используются, когда нужно понять, что именно строить, где граница capability и как она включается в систему.

Skills:

- `capability-design-review-SKILL.md`: intent, constraints, quality, portability.
- `capability-decomposition-SKILL.md`: leaf vs expand.
- `capability-convergence-SKILL.md`: proposer + challengers до convergence.
- `capability-routing-gate-SKILL.md`: manual code vs trainable pipeline routing.
- `capability-implementation-gate-SKILL.md`: exhaustive bootstrap list и routing declaration.
- `capability-state-reader-SKILL.md`: ACTIVE/STUB/MISSING state snapshot.
- `capability-measurement-SKILL.md`: score completeness.
- `capability-fleet-completion-SKILL.md`: fleet-level denominator.
- `subproject-examination-SKILL.md`: pre-implementation examination.
- `subproject-completeness-checker-SKILL.md`: completeness checks for subproject.
- `work-scope-inventory-SKILL.md`: denominator before numerator.
- `system-intake-SKILL.md`: extract structure, intent, constraints, quality.
- `problem-decomposition-SKILL.md`: break ambiguous problem.
- `service-boundary-design-SKILL.md`: service boundary tests.
- `shared-service-design-SKILL.md`: shared vs per-subproject service.
- `interface-design-craft-SKILL.md`: intent-first interface design.
- `domain-event-design-SKILL.md`: domain event shape.
- `event-contract-design-SKILL.md`: event payload, correlation, privacy.
- `experiment-design-SKILL.md`: design/run protocol для E-N experiments и scale-up решений.
- `data-flow-integrity-SKILL.md`: producer/consumer schema integrity.
- `pipeline-position-check-SKILL.md`: receives/produces/consumer contract.
- `pipeline-trace-reader-SKILL.md`: trace pipeline run.
- `feature-prioritization-SKILL.md`: prioritization formula.
- `wave-assignment-SKILL.md`: foundation vs wave sequencing.
- `parallel-execution-coordinator-SKILL.md`: safe parallel execution boundaries.
- `per-entity-examination-SKILL.md`: all-N examination before synthesis.
- `temporal-behavior-SKILL.md`: anchor event and temporal phases.

Универсальность: высокая в методе, mixed в конкретных contracts. Если другой проект не имеет subprojects, "subproject" заменяется на module/package/service.

### 5.6 AI pipeline, learning, scoring и training data

Используются для capability improvement, model/router/prompt evolution, DPO data и learning loops.

Skills:

- `ai-pipeline-design-SKILL.md`: when AI activates, roles, DPO structure.
- `ai-pipeline-topology-SKILL.md`: node handler topology.
- `ai-model-selection-SKILL.md`: model tier hierarchy and replacement signal.
- `algorithm-as-service-SKILL.md`: scoring algorithms as services.
- `four-tier-decision-classification-SKILL.md`: boundary/freedom/learnable/AI pipeline.
- `freedom-machine-SKILL.md`: machine constants vs configurable freedom.
- `dpo-triple-format-SKILL.md`: DPO triple schema.
- `planning-dpo-authoring-SKILL.md`: planning decisions as DPO signals.
- `learning-signal-capture-SKILL.md`: outcome, calibration, arbiter verdict signals.
- `learning-loop-closure-SKILL.md`: decide/observe/update/promote.
- `learning-path-audit-SKILL.md`: are generated signals actually learning.
- `training-data-gap-audit-SKILL.md`: contaminated/stub-era training data.
- `quality-improvement-cycle-SKILL.md`: score band iteration.
- `improvement-measurement-SKILL.md`: behavioral delta, not just test count.
- `capability-improvement-cycle-SKILL.md`: five-stage improvement cycle.
- `capability-adaptation-map-SKILL.md`: expected failure modes by class.
- `archetype-failure-map-SKILL.md`: score bracket action map.
- `score-interpretation-SKILL.md`: score brackets and decision tree.
- `score-zero-investigation-SKILL.md`: why a score is zero.
- `confidence-lifecycle-SKILL.md`: seed/learn/promote/decay/graduate.
- `confidence-seeding-SKILL.md`: evidence-based confidence seed.
- `cycle-budget-prediction-SKILL.md`: cycle cost prediction.
- `cycle-visibility-record-SKILL.md`: sent/received/decided/changed record.
- `engine-independence-tracking-SKILL.md`: when local model replaces expensive model.
- `negative-example-patching-SKILL.md`: patch with wrong/correct examples.
- `prompt-patch-authoring-SKILL.md`: prompt patch discipline.
- `skill-injection-SKILL.md`: selecting skills as context.
- `skill-graph-sync-SKILL.md`: sync skill docs to graph registry.
- `graph-backed-routing-SKILL.md`: graph-backed routing and feedback.
- `graph-entity-schema-SKILL.md`: node/edge/property schema.
- `graph-extension-protocol-SKILL.md`: graph mutation governance.

Универсальность: mixed to high. DPO and learning loop concepts are portable. Concrete `DomainModelRegistry`, XIIGen graph, `ContinuousLearningPipeline`, checkpoint layout and local wiki training assets are project adapter.

### 5.7 Documentation, materialization, QA и lifecycle packaging

Используются вокруг реализации: docs sync, final packaging, QA sessions, remediation plans.

Skills:

- `documentation-sync-SKILL.md`: docs follow code.
- `design-artifact-completeness-SKILL.md`: exists/populated/specific/matches/reachable.
- `materialization-session-type-SKILL.md`: design to concrete surface.
- `qa-session-type-SKILL.md`: QA review protocol.
- `remediation-session-design-SKILL.md`: repair session design.
- `self-extension-session-SKILL.md`: extending the skill/system itself.
- `phase-completion-packager-SKILL.md`: package phase completion.
- `phase-retrospective-SKILL.md`: plan accuracy and phase retrospective.
- `mission-progress-SKILL.md`: what progress means.
- `mvp-round-trip-verification-SKILL.md`: authored to visible/callable/observable round trip.
- `reconnaissance-gate-SKILL.md`: evidence thresholds before synthesis.
- `design-artifact-completeness-SKILL.md`: artifact completeness.
- `level-correction-response-SKILL.md`: respond to level correction.
- `retroactive-development-SKILL.md`: fix engine, not generated output.

Универсальность: высокая. Project-specific parts are doc filenames, exact artifact names and allowed commands.

### 5.8 UI-specific skills

Используются для UI design/review/testing. Идеи переносимы, но текущие реализации часто привязаны к Windows Forms.

Skills:

- `ui-quality-standard-SKILL.md`: UI quality and anti-slop rules.
- `ui-screen-grammar-SKILL.md`: grammar type before controls.
- `ui-examination-round-SKILL.md`: Form x Feature x Role x State cells.
- `ui-testing-SKILL.md`: Windows Forms testing protocol.

Универсальность: design/review principles are universal; Windows Forms specifics are project adapter.

### 5.9 TranslationDomain-specific skills

Это самые проектно-специфичные skills.

Skills:

- `translation-domain-mindset-reset-SKILL.md`: behavioral anchor for translation sessions.
- `translation-domain-reality-check-SKILL.md`: technical FAQ and protocol for TranslationDomain.

Универсальность: низкая для предметной части, но высокая для паттерна "domain guard". В другом проекте нужен аналогичный domain guard для самого рискованного домена.

### 5.10 Miscellaneous operational skills

Skills:

- `tracker-SKILL.md`: `IProjectTrackerService` observability.
- `chain-arithmetic-audit-SKILL.md`: count arithmetic across sessions.
- `engine-independence-tracking-SKILL.md`: already listed under learning, also operational.
- `meta-escalation-router-SKILL.md`: already listed under arbiters, also operational.

Универсальность: mixed. The idea is portable, interface names are project-specific.

## 6. Полный реестр файлов по группам

Ниже полный список живых `*-SKILL.md` файлов, сгруппированный по основной роли. Некоторые skills естественно относятся к двум группам; здесь они помещены в главный usage bucket.

### Session and governance

- `agent-output-format-SKILL.md`
- `context-overflow-SKILL.md`
- `debug-session-SKILL.md`
- `goal-context-persistence-SKILL.md`
- `investigation-handoff-SKILL.md`
- `output-readability-gate-SKILL.md`
- `phase-git-report-SKILL.md`
- `session-constitution-SKILL.md`
- `session-execution-log-SKILL.md`
- `session-file-authoring-SKILL.md`
- `session-handoff-SKILL.md`
- `session-mode-declaration-SKILL.md`
- `session-output-contract-SKILL.md`
- `session-scope-resolution-SKILL.md`
- `session-state-crystallization-SKILL.md`
- `tracker-SKILL.md`

### Planning and architecture control

- `architect-behavior-classifier-SKILL.md`
- `architectural-decision-testing-SKILL.md`
- `artifact-numbering-SKILL.md`
- `assumption-registry-SKILL.md`
- `context-package-design-SKILL.md`
- `decision-capture-SKILL.md`
- `decision-reopening-SKILL.md`
- `document-hierarchy-SKILL.md`
- `gap-to-proposal-SKILL.md`
- `intent-to-plan-SKILL.md`
- `naming-conventions-enforcer-SKILL.md`
- `no-product-decisions-SKILL.md`
- `phase-planning-SKILL.md`
- `phase-preflight-SKILL.md`
- `plan-execution-feedback-SKILL.md`
- `planning-session-startup-SKILL.md`
- `plan-restructure-SKILL.md`
- `plan-review-SKILL.md`
- `plan-self-validation-SKILL.md`
- `prerequisite-chain-SKILL.md`
- `solution-scope-gate-SKILL.md`
- `work-scope-inventory-SKILL.md`

### Review, arbiters and evidence

- `arbiter-panel-design-SKILL.md`
- `arbiter-registration-SKILL.md`
- `arbiter-type-coverage-SKILL.md`
- `audit-protocol-SKILL.md`
- `challenger-round-design-SKILL.md`
- `claim-as-hypothesis-SKILL.md`
- `claim-verification-SKILL.md`
- `coverage-completeness-gate-SKILL.md`
- `escalation-orchestrator-SKILL.md`
- `goal-delivery-completeness-SKILL.md`
- `meta-arbiter-SKILL.md`
- `meta-escalation-router-SKILL.md`
- `multi-reviewer-design-SKILL.md`
- `output-skepticism-SKILL.md`
- `principles-arbiter-SKILL.md`
- `product-scope-validation-SKILL.md`
- `scope-isolation-arbiter-SKILL.md`
- `self-questioning-SKILL.md`
- `self-verification-SKILL.md`
- `specificity-calibration-SKILL.md`
- `verification-before-completion-SKILL.md`

### Execution, safety, testing and debugging

- `api-shape-verification-SKILL.md`
- `blast-radius-SKILL.md`
- `blast-radius-tagger-SKILL.md`
- `bug-to-tests-SKILL.md`
- `change-propagation-SKILL.md`
- `code-examination-SKILL.md`
- `code-execution-SKILL.md`
- `defense-in-depth-SKILL.md`
- `dev-safety-SKILL.md`
- `diagnostic-escalation-SKILL.md`
- `dna-compliance-guard-SKILL.md`
- `e2e-test-matrix-SKILL.md`
- `event-driven-debugging-SKILL.md`
- `generated-code-review-SKILL.md`
- `implementation-integrity-SKILL.md`
- `infrastructure-discovery-SKILL.md`
- `integration-testing-SKILL.md`
- `mental-debug-SKILL.md`
- `pipeline-bug-taxonomy-SKILL.md`
- `root-cause-ladder-SKILL.md`
- `root-cause-tracing-SKILL.md`
- `systematic-debugging-SKILL.md`
- `test-failure-triage-SKILL.md`
- `test-integrity-SKILL.md`
- `three-level-verification-SKILL.md`
- `user-journey-testing-SKILL.md`

### Capability, product and system design

- `algorithm-as-service-SKILL.md`
- `capability-convergence-SKILL.md`
- `capability-decomposition-SKILL.md`
- `capability-design-review-SKILL.md`
- `capability-fleet-completion-SKILL.md`
- `capability-implementation-gate-SKILL.md`
- `capability-measurement-SKILL.md`
- `capability-routing-gate-SKILL.md`
- `capability-simulation-SKILL.md`
- `capability-state-reader-SKILL.md`
- `data-flow-integrity-SKILL.md`
- `domain-event-design-SKILL.md`
- `event-contract-design-SKILL.md`
- `experiment-design-SKILL.md`
- `feature-prioritization-SKILL.md`
- `four-tier-decision-classification-SKILL.md`
- `freedom-machine-SKILL.md`
- `interface-design-craft-SKILL.md`
- `parallel-execution-coordinator-SKILL.md`
- `per-entity-examination-SKILL.md`
- `pipeline-position-check-SKILL.md`
- `pipeline-trace-reader-SKILL.md`
- `problem-decomposition-SKILL.md`
- `service-boundary-design-SKILL.md`
- `shared-service-design-SKILL.md`
- `subproject-completeness-checker-SKILL.md`
- `subproject-examination-SKILL.md`
- `system-intake-SKILL.md`
- `temporal-behavior-SKILL.md`
- `wave-assignment-SKILL.md`

### AI pipeline, learning and graph

- `ai-model-selection-SKILL.md`
- `ai-pipeline-design-SKILL.md`
- `ai-pipeline-topology-SKILL.md`
- `archetype-failure-map-SKILL.md`
- `capability-adaptation-map-SKILL.md`
- `capability-improvement-cycle-SKILL.md`
- `confidence-lifecycle-SKILL.md`
- `confidence-seeding-SKILL.md`
- `cycle-budget-prediction-SKILL.md`
- `cycle-visibility-record-SKILL.md`
- `dpo-triple-format-SKILL.md`
- `engine-independence-tracking-SKILL.md`
- `graph-backed-routing-SKILL.md`
- `graph-entity-schema-SKILL.md`
- `graph-extension-protocol-SKILL.md`
- `improvement-measurement-SKILL.md`
- `iron-rule-derivation-SKILL.md`
- `learning-loop-closure-SKILL.md`
- `learning-path-audit-SKILL.md`
- `learning-signal-capture-SKILL.md`
- `negative-example-patching-SKILL.md`
- `planning-dpo-authoring-SKILL.md`
- `prompt-patch-authoring-SKILL.md`
- `quality-improvement-cycle-SKILL.md`
- `score-interpretation-SKILL.md`
- `score-zero-investigation-SKILL.md`
- `skill-graph-sync-SKILL.md`
- `skill-injection-SKILL.md`
- `training-data-gap-audit-SKILL.md`

### Documentation, QA, lifecycle and materialization

- `design-artifact-completeness-SKILL.md`
- `documentation-sync-SKILL.md`
- `level-correction-response-SKILL.md`
- `materialization-session-type-SKILL.md`
- `mission-progress-SKILL.md`
- `mvp-round-trip-verification-SKILL.md`
- `phase-completion-packager-SKILL.md`
- `phase-retrospective-SKILL.md`
- `qa-session-type-SKILL.md`
- `reconnaissance-gate-SKILL.md`
- `remediation-session-design-SKILL.md`
- `retroactive-development-SKILL.md`
- `self-extension-session-SKILL.md`

### UI

- `ui-examination-round-SKILL.md`
- `ui-quality-standard-SKILL.md`
- `ui-screen-grammar-SKILL.md`
- `ui-testing-SKILL.md`

### Project/domain-specific guards

- `translation-domain-mindset-reset-SKILL.md`
- `translation-domain-reality-check-SKILL.md`

## 7. Какие части skills универсальны

Универсальные механики:

- role declaration: архитектор, planner, reviewer, executor, investigation;
- requirements ledger before plan;
- Gate B как measurable condition;
- completion matrix;
- state file как продолжение памяти;
- no product decision without Luba;
- claim verification;
- audit/review as hole finding;
- bounded executor work order;
- anti-soft-stop firewall;
- no branch switch without explicit instruction;
- evidence before completion;
- runtime evidence firewall;
- plan self-validation;
- source hierarchy and contradiction handling;
- doc-first reconnaissance;
- skill injection by lifecycle stage;
- arbiter panel and escalation.

Эти части можно вынести в reusable AI extension почти без изменения.

## 8. Какие части skills уникальны для LlmFromScratch/XIIGen

Проектные привязки:

- `LlmFromScratch` as pure C# from-scratch LLM;
- `.NET 9`, C#, `xUnit`, Roslyn, Windows Forms;
- `TensorCore` as the only tensor primitive;
- `OperationResult<T>` everywhere;
- Fabric First in the exact local meaning;
- `BaseService(ISessionStorage storage)`;
- no external ML frameworks;
- every production line comment rule;
- concrete subproject map `00..43`;
- `ILearningMachine<TDomain>`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`;
- `42.ProjectWikiDomain` and `43.LearningLocalWikiData`;
- project-local wiki over Angular + .NET + SQL stored procedures;
- `27.TranslationDomain` and its checkpoint/corpus rules;
- DPO triple classes and namespace distinctions;
- `25.ModelServer`, `IRemoteUnitLocator`, manifests, checkpoints;
- Visual Studio and VS Code plugin entrypoint evidence rules;
- current branch and one-commit-per-phase convention;
- exact state/report/matrix fields used by the active plans.

Эти части должны быть extracted into `project_profile` or `adapter_policy`, not hardcoded into universal skill engine.

## 9. Какие арбитры работают и как их переносить

### Core arbiters

- Goal Delivery: проверяет, что цель пользователя доставлена целиком.
- Requirements Ledger: проверяет, что каждое требование mapped to plan/gate/artifact.
- Plan Review: проверяет plan holes, Gate B, self-containment, path errors.
- Product Decision: ловит архитектурные/product choices, которые нельзя делать без Luba.
- Scope Isolation: не даёт смешать private/shared/platform work.
- Principles/DNA: проверяет базовые engineering rules.
- Test Integrity: защищает тесты от ослабления.
- Claim Verification: разделяет verified, inferred, assumed.
- Evidence Scope: проверяет, что доказательство соответствует claim.
- Completion Coverage: требует matrix по всем literal requirements.
- Runtime Evidence: не принимает static/build proof за runtime capability.
- Output Readability: делает ответ человеческим.
- Meta Arbiter: разбирает конфликт арбитров.
- Escalation Orchestrator: решает, когда действительно нужен Luba decision.

### Что в них универсально

- разделение проверок по независимым ролям;
- BLOCK только для correctness-propagating holes;
- no self-approval;
- exact evidence scope;
- escalation only when no safe next action exists.

### Что проектно

- набор principles;
- test commands;
- artifact schemas;
- acceptable metrics;
- domain-specific forbidden patterns;
- plugin host definitions;
- model/checkpoint lifecycle.

## 10. Как я бы разложил это в AI extension

Минимальная структура:

```text
skills/
  universal/
    session/
    planning/
    review/
    arbiters/
    evidence/
    debugging/
    capability/
    learning/
  adapters/
    dotnet-csharp/
    xiigen-llm-from-scratch/
    translation-domain/
    windows-forms/
  registry.json
  load-rules.json
  conflict-policy.json
```

`universal` хранит саму дисциплину мышления.
`adapters` хранит команды, имена интерфейсов, project map, domain rules.
`registry.json` отвечает на вопрос "какой skill грузить".
`conflict-policy.json` говорит, что инструкция пользователя выше skills, а project constitution выше старого wording внутри skill.

## 11. Рекомендованные portability labels

Для каждого skill стоит добавить поле:

```json
{
  "portability": "universal | mixed | project_specific",
  "universal_core": [],
  "project_adapter_requirements": [],
  "activation_triggers": [],
  "conflicts_with_project_policy": []
}
```

Примеры:

- `no-product-decisions-SKILL.md`: `universal`.
- `plan-review-SKILL.md`: `mixed`, потому что FC checks универсальны, но Gate B commands и .NET rules проектные.
- `translation-domain-reality-check-SKILL.md`: `project_specific`, но может служить шаблоном для future domain guard.
- `ui-testing-SKILL.md`: `mixed`, потому что UI testing discipline универсальна, а Windows Forms детали проектные.
- `dna-compliance-guard-SKILL.md`: `mixed`, потому что "project DNA" универсальная идея, но сами DNA rules локальные.

## 12. Практический вывод

Главная ценность этой библиотеки не в том, что она знает C# проект. Главная ценность в том, что она создаёт управляемую cognitive architecture для AI agent:

- кто сейчас говорит;
- что считается доказательством;
- когда нельзя выбирать самому;
- какие проверки независимы друг от друга;
- когда отчёт не доказывает capability;
- как не остановиться на checkpoint;
- как превращать review findings в next work order.

Именно это универсально. Конкретный LlmFromScratch код, подпроекты, `.NET` правила и TranslationDomain guard должны быть переносимым adapter layer, а не частью общего ядра.
