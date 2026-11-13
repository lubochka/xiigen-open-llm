# Реальные цепочки оркестрации из чатов и документов XIIGen

Статус: reference-набор для обучения оркестрации.

Задача документа: показать не шаблонные “кейсы”, а реальные пользовательские ситуации, где чат доводил задачу от человеческого запроса до архитектуры, review, gap-плана, implementation plan и проверяемых артефактов.

Главное правило: правильная оркестрация не отвечает одним списком задач. Она ведет работу как цепочку: понять цель, найти evidence, разложить систему, выбрать существующие patterns, сделать plan, проверить plan, перевести gaps в executable work, сохранить state и показать человеку следующий результат.

## 1. Набор skills для нового XIIGen проекта и n8n-like flow engine

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`, `9fb65b6f-8403-4548-b901-bdc420b30771`, `94a17951-aeb5-4b53-8d6a-71b383f65043`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\skills\INDEX.md`
- `C:\Users\lubao\Documents\xiigen\skills\25-react-native-client\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\skills\39-figma-plugin-bridge\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\skills\40-content-generation-pipeline\SKILL.md`

### Точный фрагмент из чата

```text
I would like to create a skills set for my new project, described in attached xml Technologies : Server - core .net latest technologies hosted on kubernetees Client react native - latest approaches
I would like a simple like n8n flow management -> will trigger a process call with trace id and body, and will pull once in X seconds if the result is ready.  The initial simple flow will get a figma nodes tree array with very raw css html code, the flow will apply to several ai models with the array and previous results positive and negative feedbacks, then provide to additional review to another model and return a result to the client. On each step middle results with prompts will be stored on elastic database. The user can give feedback good, neutrile, negative and some text input - it will be saved and provided later to the machine
```

### Смысловой разбор

Это один длинный prompt про skill set и flow engine. Его нельзя сокращать до “хочет набор skills”, потому что внутри уже есть требования к execution lifecycle, trace id, polling, Figma input, multi-model AI processing, intermediate storage, review model, feedback и future learning.

### Правильная оркестрация

1. Сначала понять, что это не “создать пару skills”, а спроектировать платформу оркестрации AI-flow.
2. Разделить запрос на слои: fabric interfaces, хранилище, очередь, AI providers, описание flow, исполнение flow, клиентский интерфейс, feedback loop, RAG, библиотека skills, prompts для реализации.
3. Превратить n8n-like часть в жизненный цикл исполнения: запустить процесс, создать trace id, сохранить request body, выполнить nodes, сохранить input/output/prompt каждого node, опрашивать status, показать result, принять feedback.
4. Превратить Figma часть в bridge: plugin отправляет Figma tree, bridge переводит plugin data в общий FlowInput, flow engine запускает figma-to-code route, client опрашивает result по trace id.
5. Превратить feedback в обучающий asset: не просто сохранить оценку, а использовать ее в будущих prompts, сравнении моделей, review model и future planning.
6. Создать переиспользуемую библиотеку skills для AI tools, а не одноразовый документ: у каждого skill есть цель, interface, dependencies, implementation prompt и alternatives там, где они нужны.
7. Проверить недостающие skills: если existing skills пустые или тонкие, расширить их до того, как говорить, что система готова к реализации.
8. Довести до deliverables: skill files, `context/interfaces`, phased implementation prompts, guides для Claude, Cursor, Copilot, Cline, и документы продолжения state.

### Что должно выйти человеку

Не “список skills”, а понимание системы: XIIGen принимает задачу, строит flow, хранит каждый шаг, показывает progress, сохраняет feedback и на основании этого улучшает следующие AI results.

## 2. Figma -> whole system: от screens до архитектуры как набора flows

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\skills\39-figma-plugin-bridge\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\skills\25-react-native-client\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\Architecture plan\DRAWIO_TO_PROJECT_PLAN_GUIDE.md`

### Точный фрагмент из чата

```text
Later usages:
1. User enters figma, select client side and server side technologies, database -> the system orchestrates the whole system:
 a. Goes over the screens
 b. Use ai models to analyze each screen components
  c. Use ai model to analyze the sceens to understand the system 
   d. Generates architecture as a set of flows in our architecture
   e. For each screen create a tech design how the UI interact with flows, and gather feedback and process the input -> 
	- may show the result, change some subscreens, move to other screens...
```

### Смысловой разбор

Это не отдельный короткий prompt. Это первый later usage внутри большого prompt-а о XIIGen: Figma становится входом, AI анализирует screens/components, система понимает product, создает architecture as flows и technical design per screen.

### Правильная оркестрация

1. Не начинать с code generation. Сначала принять Figma input через bridge и сохранить raw tree как evidence.
2. Разобрать screens: возможные routes, components, forms, buttons, user actions, data widgets, permissions, state transitions.
3. Разобрать каждый screen как business flow: user action -> validation -> API -> service -> persistence -> result -> UI feedback.
4. Сравнить screens между собой: где одна сущность повторяется, где один flow продолжается на другом screen, где нужен shared component.
5. Выбрать technologies как planning context, а не как hardcoded output: client stack, server stack, database, queue, hosting, AI providers.
6. Сгенерировать architecture as flows: не только diagrams, а flow definitions, node responsibilities, input/output, errors, feedback and test surfaces.
7. Для каждого screen создать technical design: UI component, API contract, backend service, storage document, validation, loading state, error state, e2e scenario.
8. Добавить feedback loop: пользователь может поменять screen, subscreen, theme или flow; XIIGen пересчитывает только затронутые части, а не всю систему вслепую.
9. Перед implementation проверить evidence: missing backend rules, unclear entity lifecycle, missing auth rules, unclear database ownership.

### Что должно выйти человеку

Пользователь видит маршрут создания системы: сначала экранная карта, потом business flows, потом architecture, потом per-screen technical design, потом implementation plan with checks.

## 3. Figma design into existing system: переиспользовать перед переписыванием

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`, `64051905-0315-480b-9416-91e59372c8a0`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\skills\39-figma-plugin-bridge\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\skills\45-design-patterns\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\business flows\21 - forms and flows.md`

### Точный фрагмент из чата

```text
2. User wants to integrate a figma design into his own system
	- the system need to orchestrate a documentation creation -> to document all the system parts
	- the system uses microservices and orchestrator to make a documentation and skillsets
	- the system creates based on design system theme
	- the system orchestrates applying the style on the system -> if that's not a new module
```

### Смысловой разбор

Это второй later usage из того же большого prompt-а. Он требует не “применить дизайн”, а сначала понять existing system, создать documentation/skillsets, построить theme и безопасно решить, style это или new module.

### Правильная оркестрация

1. Сначала определить, что это adaptation существующей системы, а не greenfield generation.
2. Собрать документацию текущей системы: screens, routes, components, backend flows, data entities, auth, theme, shared components.
3. Сопоставить Figma screens с existing screens: где та же business function, где new module, где только visual restyling.
4. Разделить change classes: theme-level, shared component-level, screen-level, business-flow-level, backend-impacting.
5. Если изменение только theme, построить design tokens и применить theme безопасно.
6. Если изменение затрагивает component behavior, plan component replacement with regression tests.
7. Если изменение создает new business flow, потребовать backend and data evidence перед implementation.
8. Если current system lacks documentation, сначала спланировать создание documentation, и только потом code changes.
9. Создать skillset для будущего reuse: design adaptation skill, existing-system documentation skill, theme migration skill, regression test skill.
10. Gate: реализация начинается только после того, как известны affected screens, affected backend flows и rollback path.

### Что должно выйти человеку

Ответ должен сказать: “Это можно встроить безопасно вот так: сначала документируем current system, потом сравниваем design, потом классифицируем changes, потом применяем минимально нужное изменение и тестируем affected flows.”

## 4. Content generation pipeline: text/audio -> publishable packages

Источник чата: `9fb65b6f-8403-4548-b901-bdc420b30771`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\skills\40-content-generation-pipeline\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\skills\40-content-generation-pipeline\prompts\implement.md`
- `C:\Users\lubao\Documents\xiigen\business flows\18 - devops platforms.md`

### Точный источник

Это не найдено как отдельная пользовательская фраза в выбранном чате. Это scenario из последующего skill/library work и артефакта:

```text
Skill 40: Content Generation Pipeline
Multi-Platform Content Generation with AI
trigger -> [generate-script] -> [generate-images] -> [generate-audio] -> [compose-video] -> [publish]
Each step is a queue event. Steps can run in parallel (images + audio simultaneously).
Each step's output stored as dynamic document with traceId correlation.
```

### Смысловой разбор

Этот сценарий нельзя выдавать как “точный prompt пользователя”. Его честный статус: артефактная оркестрация, появившаяся из работы по расширению skill library.

### Правильная оркестрация

1. Понять цель как publishing pipeline, а не как один вызов “generate video”.
2. Принять inputs: raw text, audio, brand material, target platforms, audience, tone, publishing calendar.
3. Если есть audio, сначала создать transcript и сохранить original audio + transcript.
4. Улучшить text: clarify message, extract campaign goal, create platform-specific script variants.
5. Создать scene plan: image prompts, video shots, timing, captions, voice, avatar, music mood.
6. Запустить generation как queue events: script -> images -> audio -> video -> packaging -> publish.
7. Сохранить каждый artifact по traceId: prompt, provider, output, approval state, revision state.
8. Использовать dynamic platform templates: добавление TikTok/Instagram/YouTube/Threads должно быть document change, а не code rewrite.
9. Добавить user approval and revision loops: пользователь может заменить script, пересоздать только audio, поменять captions или отклонить platform package.
10. Завершить publish-ready packages и scheduler plan, плюс missing evidence list, если отсутствуют platform credentials или brand policy.

### Что должно выйти человеку

Не “создать контент”, а маршрут производства: от raw input до checked packages for platforms, with stored prompts, approvals, generated media, revisions and schedule.

## 5. WhatsApp diet coaching flow: questionnaire -> plan -> daily meal loop

Источник чата: `432c5e3f-6b5e-43cd-93b6-b40eb1b99123`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\skills\41-whatsapp-diet-flow\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\skills\41-whatsapp-diet-flow\prompts\implement.md`
- `C:\Users\lubao\Documents\xiigen\skills\24-notification-service\SKILL.md`

### Точный фрагмент из чата

```text
Let's extend our current solution with additional optional flow - user wants to create a flow that will help his clients to manage their diets. He has a questionnaire - and a prompt which translate a questionnaire to the diet plan. And to follow up the diet plan he wants the system to ask on a daily basis - even on a meal basis, depends on user choise - what did you eat? And the system will use an orchestrator that translates each meal to calories and sums up user daily diet and then suggest what he needs to do with his diet - or maybe consume more calories of some type - or maybe will suggest meals changes in the future.
```

### Смысловой разбор

Это реальный prompt о diet coaching flow. Важная часть не “diet app”, а orchestration points: plan creation, follow-up nodes, prompts/connectors, AI calorie translation, summary, suggestions and possible code injection.

### Правильная оркестрация

1. Распознать это как coaching flow на существующем XIIGen flow lifecycle, а не как отдельную chatbot subsystem.
2. Принять questionnaire и создать user nutrition profile как dynamic document.
3. Generate initial diet plan from questionnaire, preferences, restrictions, goals and schedule.
4. Создать follow-up policy: daily, per meal, or user-selected cadence.
5. Использовать WhatsApp или notification channel, чтобы спросить, что пользователь съел.
6. Разобрать free text meal через AI provider into structured food items, calories, protein, carbs, custom fields.
7. Сохранить every meal under user scope и enforce user isolation, потому что diet data is personal.
8. Aggregate daily totals and compare against the plan.
9. Generate advice: consume more/less calories, change meal type, suggest replacement meals.
10. Вернуть actual behavior обратно в plan: если deviations повторяются, plan adapts.
11. Если missing parsing or nutrition tool, generate a node/tool extension that obeys microservice architecture expectations.

### Что должно выйти человеку

Пользователь получает не просто diet plan, а living coaching loop: plan, reminders, meal parsing, daily summary, adaptive advice, privacy isolation and future learning.

## 6. Multi-stack skill alternatives: same XIIGen concept, many implementation targets

Источник чата: `9fb65b6f-8403-4548-b901-bdc420b30771`, `c14029f2-9537-4001-bcda-dc00b57d61df`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\skills\03-elasticsearch-datastore\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\skills\04-redis-queue-service\SKILL.md`
- `C:\Users\lubao\Documents\xiigen\platform development\extracted_v18\MASTER_PLAN.md`

### Точный фрагмент из чата

```text
1. some of the skills are empty
2. Implementation - there is only example on .Net - I think we need an example for all the alternatives in sub directories
```

### Смысловой разбор

Это не общий “multi-stack” prompt, а correction request после просмотра skill library. Он требует проверить пустые skills и расширить implementation examples across alternatives.

### Правильная оркестрация

1. Сначала проверить current state: какие skills complete, thin, empty, duplicate or missing.
2. Определить canonical pattern per skill: interface, dependencies, DataProcessResult/DataProcess equivalent, dynamic document handling, queue/event semantics.
3. Для каждого language сохранить тот же concept, а не копировать C# syntax: Node async service, Python async provider, Java service, Rust trait, PHP service.
4. Каждая alternative должна сохранять тот же architecture contract: dynamic documents, provider abstraction, non-empty filters, structured result, logging, tests.
5. Implementation prompts должны идти по phases: create interface, implement provider, add DI/config, add tests, add docs.
6. Отклонить alternatives, которые создают другое product behavior только из-за смены language.
7. Создать continuation state: какая phase complete, какой skill range next, какая verification done.

### Что должно выйти человеку

Человек видит, что “multi-stack” означает одну XIIGen architecture, выраженную на нескольких implementation languages, а не шесть unrelated rewrites.

## 7. FLOW-07 friend request and social feed: flow generation with factories, conflicts and tests

Источник чата: `26e4b685-6f0e-4017-94b7-c320d85e21e1`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\old\TASK_TYPES_CATALOG_MERGED.md`
- `C:\Users\lubao\Documents\xiigen\old\ENGINE_ARCHITECTURE_MERGED.md`
- `C:\Users\lubao\Documents\xiigen\old\SKILLS_FACTORY_RAG_MERGED.md`
- `C:\Users\lubao\Documents\xiigen\old\V62_BFA_STRESS_TEST_MERGED.md`
- `C:\Users\lubao\Documents\xiigen\.net angular translation\DOTNET-ANGULAR-PER-FLOW-IMPLEMENTATION-RUNBOOKS.md`

### Точный фрагмент из чата

```text
We need to support a system that is capable of generating complex flows by user demand, and to generate nodes, inject code, this code needs to stand our architecture requirements being easy to test and later be a part of our core architecture. For that we are extending the system plan to support the expected complexity using ai, skill set, Rag. Please examing the following approaches and the tasks that we were examining so far and extend - combine into the improved, flexible plan. On the UI please take approach fabric first no matter which platform we will use We need to extend the engine we build to be capable to support a flow creation described in 07-* documents attached.
```

### Смысловой разбор

Это реальный prompt про engine extension. Его нельзя трактовать как “сделай FLOW-07 feature”; нужно расширить engine so it can generate this class of flow.

### Правильная оркестрация

1. Понять, что работа является engine extension, а не manual implementation of friend request features.
2. Загрузить basic prompt, source documents and existing flow conventions.
3. Выделить FLOW-07 domain: friend request, compatibility scoring, social feed integration, notifications, privacy, rollback.
4. Превратить flow в task type contracts and factory interfaces.
5. Построить conflict rules: duplicate request, blocked user, privacy filter, feed ordering, optimistic rollback.
6. Построить RAG and source index artifacts so later agents can locate why a task type exists.
7. Построить stress tests и BFA rules, которые ловят architectural conflicts.
8. Смержить в master execution artifacts: task catalog, architecture, source index, session state, master plan.
9. Только потом prepare implementation runbook: server API, application, infrastructure, tests, Angular pages, CI, deployment, evidence.

### Что должно выйти человеку

Человек получает не feature list, а engine-capability plan: какие factories, task types, conflict rules, tests and source references allow XIIGen generate this class of social flow again.

## 8. FLOW-08 multi-tenant or marketplace flow: policy, compliance, migration, deployment

Источник чата: `f9641473-896f-4684-856b-5d1d5e5167dc`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\old\MASTER_EXECUTION_PLAN_MERGED.md`
- `C:\Users\lubao\Documents\xiigen\old\UNIFIED_SOURCE_INDEX_MERGED.md`
- `C:\Users\lubao\Documents\xiigen\old\V62_BFA_STRESS_TEST_MERGED.md`
- `C:\Users\lubao\Documents\xiigen\.net angular translation\DOTNET-ANGULAR-FLOW-IMPLEMENTATION-BLUEPRINTS.md`

### Точный фрагмент из чата

```text
We need to support a system that is capable of generating complex flows by user demand, and to generate nodes, inject code, this code needs to stand our architecture requirements being easy to test and later be a part of our core architecture. For that we are extending the system plan to support the expected complexity using ai, skill set, Rag. Please examing the following approaches and the tasks that we were examining so far and extend - combine into the improved, flexible plan. On the UI please take approach fabric first no matter which platform we will use We need to extend the engine we build to be capable to support a flow creation described in 28-* documents attached.
```

### Смысловой разбор

Это тот же engine-extension pattern, но с другим attached flow family. Отличие должно идти из `28-*` docs and f08 artifacts, а не из generic wording.

### Правильная оркестрация

1. Классифицировать это как another engine extension round, а не UI-only flow.
2. Прочитать previous flow numbering, artifact conventions and current merged artifacts.
3. Выделить new task archetypes: compliance, policy, telemetry, migration, deployment.
4. Разложить их по architecture stations, а не по одному service.
5. Создать BFA conflict rules: tenant boundary, policy override, compliance state, migration safety, deployment readiness.
6. Добавить prompt-management and security isolation там, где flow uses AI decisions.
7. Смержить new f08 material into existing merged artifacts, preserving numbering and source references.
8. Обновить session state, чтобы next agent knew current factory/task/rule numbers.
9. Строить final execution plan только после того, как merged artifacts agree.

### Что должно выйти человеку

Ответ должен показать, как XIIGen расширяет engine vocabulary: не “build marketplace”, а “teach the engine tenant/policy/compliance/migration/deployment shapes and prove them through merged catalogs and stress tests.”

## 9. RAG benchmark and plan review: from weak plan to reusable skills

Источник чата: `2d19ed25-93dc-438d-8c02-dda9a5fc7c9e`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\.claude\worktrees\adoring-davinci-058d7f\1st delivery tests\Test Rag\XIIGEN_RAG_BENCHMARK_STRATEGY.md`
- `C:\Users\lubao\Documents\xiigen\.claude\worktrees\adoring-davinci-058d7f\1st delivery tests\Test Rag\GATE-B-FINDINGS.md`
- `C:\Users\lubao\Documents\xiigen\Planning sessions\old\skills_flat\planning--plan-review-SKILL-v2.1.0.md`

### Точный фрагмент из чата

```text
Please review this plan using the uploaded planning and reviewing skills
```

### Смысловой разбор

Это короткий prompt, но его правильная оркестрация большая: load skills, inspect plan, find blocking issues, force Gate B, then package only relevant corrected files.

### Правильная оркестрация

1. Относиться к “review” как к active architecture gate, а не как к polishing text.
2. Загрузить plan и relevant planning/review skills.
3. Compare plan claims against corpus reality: counts, coverage, expected artifacts, agent-executable format.
4. Найти issue types: stale counts, missing gate, missing state, missing executable session format, principle violations.
5. Вернуть сначала findings, а не summary.
6. Force plan revision until Gate B passes.
7. Когда scope shifts from one RAG benchmark to reusable skill sets, менять архитектуру plan, а не только wording.
8. Отдать только relevant files и summary, отражающее реальные decisions conversation.

### Что должно выйти человеку

Человек получает review loop, который prevents “looks good” completion: plan is checked against evidence, revised, gated, and only then packaged.

## 10. CSS/Figma-to-code regression: debug pipeline, а не случайные CSS fixes

Источник чата: `6d216319-7ea8-41e2-acc6-e2025d1235a8`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\Architecture plan\FigmaToCode-with-skills`
- `C:\Users\lubao\Documents\xiigen\Adobe adaptation plan\FIGMA_TO_EXPRESS_API_COMPARISON.md`
- `C:\Users\lubao\Documents\xiigen\Orchestrator MVP\extracted_temp\translated\EPISODE-MAP-EXTENDED.md`

### Точный фрагмент из чата

```text
I would like to debug the css that we get, please load the code documentation and matrix to help me up to improve the css created.

I will attach improved html css code, and then our original css and css log, you will need to try to follow the css creation and suggest ideas how to improve the code, without ruining it.
Please be careful on each suggestion check out the code documentation and the matrix. If matrix is not correlated to what actually happening - fix the matrix and let me download. If the participating mix applies on other places on the matrix, suggest fix. We will test the suggestions one by one.
```

### Смысловой разбор

Это реальный debugging prompt. Главное: не “починить CSS”, а trace CSS creation, correlate with matrix, suggest one change at a time, and preserve working behavior.

### Правильная оркестрация

1. Начать с загрузки code documentation and matrix; не угадывать CSS только по output.
2. Compare improved output, original output and debug log.
3. Trace the CSS creation pipeline function by function.
4. Найти exact failure class: например, layoutMode=NONE nodes generating wrong absolute positioning instead of useful layout inference.
5. Проверить, matches ли matrix actual code; если нет, update matrix before proposing fixes.
6. Propose one fix at a time and state blast radius.
7. Добавить tests per scene: login, form, travel, mail, FAQ, cart, news or other available scenes.
8. Использовать classifier logic только там, где evidence supports it; не превращать каждый node в ML problem.
9. Запустить privacy/security review, когда implementation stores user correction memory.
10. Если architecture stores private user data in shared Figma plugin data, reject sanitization and reverse the architecture.

### Что должно выйти человеку

Человек получает controlled debugging route: trace, classify, patch one issue, test visually, update documentation/matrix, reject privacy-breaking shortcuts.

## 11. Flow-prep -> simulation -> gap-prep -> gap-translate -> execution

Источник чата: `638643f1-319e-4820-971d-4bb1373cf57f`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\Flow preparation\FLOW-PREP-R0\FLOW-PREP-MASTER-PLAN.md`
- `C:\Users\lubao\Documents\xiigen\Flow preparation\SIMULATION-R0\SIMULATION-MASTER-PLAN.md`
- `C:\Users\lubao\Documents\xiigen\Flow preparation\GAP-PREP-PROCESS-R0\GAP-PREP-MASTER-PLAN.md`
- `C:\Users\lubao\Documents\xiigen\Flow preparation\GAP-TRANSLATE-PROCESS-R0\GAP-TRANSLATE-MASTER-PLAN.md`

### Точный фрагмент из чата

```text
SIMULATION SESSION — FIXED PROMPT v1.0
SESSION-START GATE (complete before any output)
Please put this prompt as the top priority and come back to it - - it has to be part of the plan on each phase
Q1 — Session type: PLANNING (primary). INVESTIGATION follows only after the plan is complete and approved. These are sequential, not simultaneous. The plan governs the first 10+ rounds. Simulation starts only after Luba approves the completed plan.
```

### Смысловой разбор

Это process-governance prompt. Его смысл: сначала planning, потом investigation, потом simulation, и нельзя перескакивать layers процесса.

### Правильная оркестрация

1. Establish the process chain: FLOW-PREP creates session files.
2. SIMULATION uses those session files to trace actual behavior.
3. GAP-PREP consumes simulation findings and builds a gap list.
4. GAP-TRANSLATE consumes gap list and writes executable Claude Code session files.
5. Execution начинается только после того, как эти sessions реально существуют.
6. Если step skipped, next step loses evidence and becomes fake.
7. У каждого round есть one output и stop/wait cadence там, где этого требует plan.
8. Every output is self-contained: no “see previous doc”, no undefined variables, no missing gates.
9. Alternative stacks используются только to discover engine gaps; fix stays in the primary engine.

### Что должно выйти человеку

Человек видит logistical chain of delivery: prepare, simulate, classify gaps, translate gaps to fix sessions, then execute. Shortcut “I’ll just implement” is not valid.

## 12. Static proposal -> dynamic AI decision architecture

Источник чата: `e492ed55-92dd-41c7-8e61-3c558bed0fd5`.

Артефакты:

- `C:\Users\lubao\Documents\xiigen\Planning sessions\old\skills_flat\planning--ai-decision-pipeline-SKILL.md`
- `C:\Users\lubao\Documents\xiigen\Planning sessions\old\skills_flat\planning--planning-dpo-authoring-SKILL.md`
- `C:\Users\lubao\Documents\xiigen\Planning sessions\old\skills_flat\planning--top-manager-extension-protocol-SKILL.md`

### Точный фрагмент из чата

```text
Please examin flow ready flows by the attached text important points of xiigen value proposition
```

### Смысловой разбор

Это короткий вход, но в чате он привел к важной architecture critique: static integration components violated XIIGen value proposition, so decisions had to move into dynamic, learnable, AI-arbitrated architecture.

### Правильная оркестрация

1. Относиться к static proposal как к suspicious, пока оно не proves it can learn.
2. Найти места, где code used constants, switch statements or fixed tables для decisions, которые должны обучаться.
3. Reframe decisions as graph entities: evidence, weighted edges, confidence, lifecycle, arbitration and feedback.
4. Split decisions into categories: mechanical helper, configurable policy, learned decision, AI-arbitrated decision.
5. For learned decisions, require DPO triples or equivalent training signal.
6. Добавить confidence lifecycle: initial score, runtime evidence, review, promotion, demotion.
7. Добавить AI decision pipeline: context assembly, candidate generation, arbiter review, result persistence, feedback return.
8. Превратить architecture critique into phased implementation sessions.
9. Review outputs again to prevent static logic from returning under new names.

### Что должно выйти человеку

Человек получает core XIIGen principle: system should not hide business intelligence in hardcoded code; meaningful decisions become inspectable, trainable, reviewable project assets.

## Итоговая модель правильного сценария

Правильный сценарий для обучения оркестратора должен содержать всю цепочку:

1. Человеческая цель пользователя.
2. Тип задачи: greenfield, existing-system adaptation, engine extension, review, simulation, gap translation, debugging, learning architecture.
3. Evidence intake: chats, source docs, code, diagrams, state, logs, previous plans.
4. High-level plan: крупные этапы и зависимости.
5. Sub-level plan: конкретные responsibilities, units, docs, routes, tests, reviews.
6. Review loop: кто проверяет, что считается провалом, что возвращается на переработку.
7. Implementation route: какие файлы/skills/sessions/gates появляются.
8. Verification: tests, simulation, Gate B, visual checks, source counts, missing evidence.
9. State and continuation: что сделано, что next, какие номера/артефакты продолжать.
10. Human answer: не counters, а понятный маршрут, по которому человек может реально вести работу.
