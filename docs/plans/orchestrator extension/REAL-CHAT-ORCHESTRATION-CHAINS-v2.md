# Реальные цепочки оркестрации из чатов XIIGen — v2

Статус: исправленная версия после ошибки v1.

Что было неправильно в v1: один длинный пользовательский message был разрезан и пересказан так, будто это отдельные готовые prompt-и. Это смешало разные задачи и исказило оркестрацию.

Как читать v2:

- `Точный фрагмент` — реальный текст из чата или его непрерывный фрагмент.
- `Граница задачи` — что именно этот фрагмент просит сделать.
- `Не смешивать с` — соседняя задача из того же message, которую нельзя подтягивать в эту оркестрацию.
- `Правильная оркестрация` — как XIIGen должен вести задачу сверху вниз и до рабочих подуровней.
- `Результат для человека` — что пользователь должен увидеть, если оркестрация правильная.

## 1. Skill set для нового проекта

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
I would like to create a skills set for my new project, described in attached xml Technologies : Server - core .net latest technologies hosted on kubernetees Client react native - latest approaches
```

### Граница задачи

Пользователь просит не код и не flow engine, а reusable skill set для нового проекта с указанным технологическим контекстом: .NET server, Kubernetes, React Native client.

### Не смешивать с

Не смешивать с n8n-like flow management. Это следующая задача в том же message, но здесь нужна именно библиотека skills.

### Правильная оркестрация

1. Прочитать attached XML и вытащить из него не имена файлов, а архитектурные обязанности проекта.
2. Разделить skill set по слоям: architecture, infrastructure, flow execution, AI integration, client, testing, documentation, safety.
3. Для каждого skill определить: зачем он нужен, какой проектный pain закрывает, какие входы получает, какой output дает, какие другие skills нужны рядом.
4. Зафиксировать, какие skills являются generic для всех проектов, а какие специфичны для этого проекта.
5. Вынести .NET/Kubernetes/React Native в constraints: DI, hosting, async job handling, mobile feedback UI, deployment.
6. Добавить examples: positive example, negative example, “когда этот skill не применять”.
7. Добавить state/resume: что уже описано, какие skills ждут attached docs, что нельзя считать готовым.
8. Завершить не списком названий, а картой skill library: folders, skill purpose, dependencies, implementation prompts, validation.

### Результат для человека

Пользователь должен получить понятный план библиотеки skills: какие skills нужны новому проекту, где они живут, как их использовать разными AI tools, что проверять перед реализацией.

## 2. n8n-like flow management

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
I would like a simple like n8n flow management -> will trigger a process call with trace id and body, and will pull once in X seconds if the result is ready.  The initial simple flow will get a figma nodes tree array with very raw css html code, the flow will apply to several ai models with the array and previous results positive and negative feedbacks, then provide to additional review to another model and return a result to the client. On each step middle results with prompts will be stored on elastic database. The user can give feedback good, neutrile, negative and some text input - it will be saved and provided later to the machine
```

### Граница задачи

Это задача про runtime flow engine: запуск процесса, trace id, polling, node execution, AI calls, intermediate storage, review model, feedback.

### Не смешивать с

Не смешивать с skill library как deliverable. Skill library может быть supporting artifact, но сама задача здесь — execution system.

### Правильная оркестрация

1. Выделить пользовательскую цель: пользователь хочет видеть управляемый AI process, а не один API endpoint.
2. Спланировать lifecycle: submit request -> create trace id -> store body -> enqueue job -> execute nodes -> store node outputs -> review -> return result.
3. Описать polling: client asks every X seconds, backend returns status, partial node states, final result or failure.
4. Описать node model: each node has input, prompt, model provider, output, status, timestamps, errors, retry policy.
5. Для Figma input не генерировать архитектуру сразу: сначала принять raw Figma tree и raw CSS/HTML как evidence.
6. Для multi-model step: fan out to several AI providers, collect outputs, compare against prior feedback, send to review model.
7. Для feedback: store good/neutral/negative + text, link it to trace id, node id, prompt and output.
8. Для future learning: feedback becomes context for future prompts and model selection, not just user comment history.
9. Для storage: Elasticsearch stores request, node execution, prompt, output, feedback, final result as dynamic documents.
10. Для debugging UI: user can inspect what each node received, what it did, what result it produced.

### Результат для человека

Пользователь должен увидеть рабочую логистику flow engine: как процесс стартует, как отслеживается, где хранятся промежуточные данные, как feedback возвращается в будущие AI runs.

## 3. Figma -> whole system

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
1. User enters figma, select client side and server side technologies, database -> the system orchestrates the whole system:
 a. Goes over the screens
 b. Use ai models to analyze each screen components
  c. Use ai model to analyze the sceens to understand the system 
   d. Generates architecture as a set of flows in our architecture
   e. For each screen create a tech design how the UI interact with flows, and gather feedback and process the input -> 
	- may show the result, change some subscreens, move to other screens...
```

### Граница задачи

Это greenfield generation from Figma: Figma is the product source, and XIIGen must build system understanding, architecture as flows and per-screen technical design.

### Не смешивать с

Не смешивать с “integrate Figma into existing system”. Здесь нет existing codebase; это generation of a whole system.

### Правильная оркестрация

1. Принять Figma file/screens and selected technologies as input.
2. Составить screen inventory: screens, components, actions, forms, repeated entities, navigation.
3. Для каждого screen выделить user intent: что пользователь делает, какую business event запускает.
4. Сопоставить screens между собой: общие entities, shared flows, repeated components, dependency between screens.
5. AI analyzes components, then another AI/model stage analyzes screens together to infer the system.
6. Построить architecture as flows: screen -> user action -> API -> service -> data -> result -> feedback.
7. Для каждого flow определить nodes, inputs, outputs, errors, retries, feedback and tests.
8. Для каждого screen создать technical design: UI responsibilities, backend contract, storage needs, validation, error states.
9. Добавить feedback loop: user can inspect, change subscreens, move to other screens, and XIIGen updates only affected parts.
10. Перед implementation выдать missing decisions: auth, roles, database ownership, external integrations, deployment.

### Результат для человека

Пользователь должен увидеть не “мы сделаем приложение”, а route from Figma to system architecture: screen map, business flows, per-screen tech design, implementation phases and review points.

## 4. Figma design -> existing system

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
2. User wants to integrate a figma design into his own system
	- the system need to orchestrate a documentation creation -> to document all the system parts
	- the system uses microservices and orchestrator to make a documentation and skillsets
	- the system creates based on design system theme
	- the system orchestrates applying the style on the system -> if that's not a new module
```

### Граница задачи

Это adaptation task: есть existing system, и Figma design нужно встроить без разрушения текущей архитектуры.

### Не смешивать с

Не смешивать с whole-system generation from Figma. Здесь first action — understand current system.

### Правильная оркестрация

1. Сначала создать documentation of current system: screens, routes, modules, APIs, services, entities, theme.
2. Определить, какие parts of the system impacted by Figma design.
3. Сравнить Figma design with existing screens: same screen, changed layout, new flow, new module, theme-only change.
4. Если это theme/style: extract design tokens, compare with current theme, plan safe style application.
5. Если это component-level change: identify reusable components and regression tests.
6. Если это new module: stop treating it as style application and create new flow plan.
7. Если backend behavior changes: require backend/service/data evidence before code plan.
8. Создать skillsets: documentation skill, theme migration skill, existing-system adaptation skill, regression testing skill.
9. Plan implementation only after current system map and Figma delta are both visible.
10. Add rollback path and visual checks.

### Результат для человека

Пользователь должен увидеть: что в существующей системе меняется, что переиспользуется, что является новым module, что нельзя трогать без backend evidence.

## 5. System from explanation when design is incomplete

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
3. User wants to implement the whole system from scratch, and a design is not covering what he needs
We need to generate system by user explanation
We need to make a design theme based on what he already have
We need to make an architecture
skills
implementation plan
Implement the plan
```

### Граница задачи

Это product discovery and system generation from user explanation, not Figma-first work.

### Не смешивать с

Не смешивать с Figma whole-system generation. Here design is incomplete or absent.

### Правильная оркестрация

1. Принять user explanation as primary source.
2. Extract product scope: users, roles, workflows, data, external systems, success criteria.
3. Identify what existing design/theme material exists and what is missing.
4. Build design direction from existing material: tokens, tone, screen style, interaction patterns.
5. Build architecture: flows, services, data model, integrations, deployment shape.
6. Build skill plan: which skills are needed to generate, test, document and maintain the system.
7. Build implementation plan in phases: foundation, core flows, UI, backend, tests, deployment, feedback.
8. Identify missing evidence: unclear product rules, missing roles, missing data lifecycle, missing integrations.
9. Add review gates: product scope review, architecture review, technical plan review, implementation gate.
10. Implement only after the plan is reviewable and the missing evidence is named.

### Результат для человека

Пользователь получает route from explanation to product: scope, design direction, architecture, skills, implementation plan and evidence gaps.

## 6. AI-assisted diet app flow

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
4. User want to create an app that intercats with other users (his potential clients)
These client on registration will fill up a form
form data will be saved 
AI diet generation plan flow will apply -> AI will orchestrate the generation-> will apply several AI using user data -> will analyze all models, run several round if required-> generate a diet plan
Each day will be triggered a flow of gathering what client ate, client will make a list and submit it in WhatsApp
AI will use a generated flow to calculate his callories and supplements, and suggest adjustments -> again several rounds with ai and analyze -> the daily feedback will be sent to the client (on push on app and on WhatsApp interaction
```

### Граница задачи

Это user-facing product flow for diet coaching: registration, questionnaire, AI plan, daily WhatsApp input, calorie/supplement analysis, feedback to client.

### Не смешивать с

Не смешивать с generic WhatsApp skill. Здесь важен full product flow: app users, registration, form, AI plan, daily loop, push/WhatsApp feedback.

### Правильная оркестрация

1. Определить actors: business owner, clients, AI planner, WhatsApp channel, mobile app.
2. Спланировать registration flow: user fills form, form data saved, profile created.
3. Спланировать diet plan generation: user data -> several AI models -> compare/analyze -> several rounds if needed -> plan.
4. Спланировать daily trigger: scheduler asks what client ate.
5. Спланировать WhatsApp intake: free text meal list -> AI parser -> calories and supplements.
6. Спланировать daily summary: compare actual meals with diet plan.
7. Спланировать adjustment suggestions: more/less calories, meal changes, supplement warnings.
8. Спланировать delivery: push in app and WhatsApp interaction.
9. Сохранить traceability: plan prompt, meal prompt, model outputs, decision rationale, feedback result.
10. Add privacy and user-scope isolation because this is personal health-related data.

### Результат для человека

Пользователь видит complete coaching flow, not one calculator: onboarding, plan generation, daily interaction, analysis, feedback, persistence and privacy.

## 7. Content generation flow

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
5. User want's to generate content, he will read loud or write a text -> the flow of content generation will be applied -> first audio to text if required, text improvements with ai , create plan for images, and videos of 3 seconds -> will apply to image generator with image prototype and generated prompts. Will use the images, and generated video prompts to generate a video. Will use ai to generate some music. Will generate audio based on user sound and avatar. Will use a video generator by a template for each platform based on short videos, music and captions. Will generate video description for each platform. Will schedule a post on each platform.
```

### Граница задачи

Это content production pipeline from text/audio to platform-specific scheduled posts.

### Не смешивать с

Не смешивать с n8n engine itself. Flow engine is infrastructure; this is one product flow running on it.

### Правильная оркестрация

1. Определить content input: spoken audio or written text.
2. If audio exists, run audio-to-text and store transcript with original audio reference.
3. Improve text with AI: clarity, tone, target audience, platform variants.
4. Create image plan: scenes, visual prompts, style reference, image prototype.
5. Create video plan: short clips, timing, transitions, 3-second video fragments.
6. Generate images from prompts and image prototype.
7. Generate video using images and generated video prompts.
8. Generate music and voice/avatar audio based on user sound/avatar.
9. Package per platform: template, aspect ratio, captions, music, description, hashtags.
10. Schedule posts per platform and store publication status.
11. Keep review points: user approves script, images, video, music, final package.

### Результат для человека

Пользователь видит production route: from raw voice/text to platform-ready scheduled posts, with approvals and reusable artifacts.

## 8. AI microservices, UI flow visibility, debugging and code injection

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
So for these purposes we need the AI microservices to be able to get prompts from previos jobs 
We need to be able to see the orchestrated flow on the UI
We need to be able to debug each node execution -> what input he got, what he did in the middle, what were the results
We need to be able to generate the code, start microservice with new internal code, test it, and integrate it into the flow
```

### Граница задачи

Это platform capability requirement. Оно поддерживает все flows above, но само по себе является отдельной архитектурной задачей.

### Не смешивать с

Не смешивать с конкретным diet/content/Figma flow. Здесь надо строить common execution/debug/code-injection substrate.

### Правильная оркестрация

1. Define previous-job context contract: what prompt/result/feedback can be reused by later AI microservice.
2. Define flow visualization model: nodes, edges, status, input/output preview, errors, retries.
3. Define node debugging record: input, intermediate processing, prompt, provider, output, duration, error.
4. Define code generation boundary: when XIIGen generates code, what architecture rules it must obey.
5. Define microservice startup lifecycle: generate code -> compile/build -> run tests -> start service -> register in flow.
6. Define integration back into flow: generated tool becomes node type or provider behind fabric interface.
7. Define safety gates: no generated code becomes active without tests and architecture compliance.
8. Define storage: every generated code artifact, test result and integration decision is persisted.
9. Define UI: user sees flow, node debug, generated code status and integration status.
10. Define rollback: if generated microservice fails, flow returns to previous safe node/tool.

### Результат для человека

Пользователь получает architecture for AI-generated microservices as safe flow extensions, not uncontrolled code injection.

## 9. Generic interfaces, database fabric, queue jobs and microservice base

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
Several requests:
I prefer a generic interfaces which I can reuse for databases, queues, 3d parity like AI
I prefer a generic code - the repository that gets dynamic document and saves it. Get a dynamic filter and filter by not empty values - like in attached c  - Data Base Fabric described in doc - but using the latest .net 7,8,9 principles
I need to manage a queue of jobs that done in each level - for example async call for 4 different models
I prefer to create a generic microservice base which will have basically architectural capabilities like in attached document
```

### Граница задачи

Это foundation architecture request: database fabric, queue fabric, AI provider fabric, dynamic documents, non-empty filter, queue jobs, microservice base.

### Не смешивать с

Не смешивать с concrete flows. These are platform foundations that flows depend on.

### Правильная оркестрация

1. Define fabric interfaces: database, queue, AI provider, secrets, logging, flow.
2. Define database repository contract: accepts dynamic document, stores dynamic document, no fixed schema.
3. Define dynamic filter behavior: only non-empty fields become query filters.
4. Define queue job model: job id, trace id, level, node id, status, retry, result.
5. Define fan-out execution for AI calls: async jobs for several models, then collect and compare.
6. Define MicroserviceBase capabilities: config, logging, health, queue, database, AI provider, cancellation, result wrapper.
7. Use .NET 9 principles without hardcoding provider-specific logic into services.
8. Add tests: non-empty filter, provider swap, queue lifecycle, fan-out/fan-in, failure path.
9. Add documentation: which service uses which fabric and what must never bypass the fabric.

### Результат для человека

Пользователь sees reusable platform foundation that can support many future flows without rewriting storage, queues or AI provider logic.

## 10. Alternative stacks and RAG/GraphRAG context planning

Источник чата: `c02db858-8191-46c7-adc4-14f7c1b41ed1`.

### Точный фрагмент

```text
I need to support an alternative possible ways
1. Use other queues like kafka and sqs
2. Server can be implemented on node.js , rust , Java, python or php
3. Client can be implemented in native, angular or vue
4. Database Fabric can be mongo, prisma, redis, Azure AI Search, Graph AI
Important notice - we need a good graph and rag capabilities to supply these AI stages with appropriate context - so each time we apply to AI we need to make a plan with AI which requests we need to apply on our current RAG. And on each end of execution and feedback - we need to plan with AI how and where store the data on RAG - (Rag can be azure ai, cosmo db, graph ai, pinecore, some other open source, gremlin)
```

### Граница задачи

Это portability and context architecture: alternative queues, server/client/database options, plus RAG/GraphRAG planning before and after AI calls.

### Не смешивать с

Не смешивать with implementation of all alternatives. The immediate orchestration is to design abstraction and context-planning capability.

### Правильная оркестрация

1. List alternative axes: queue, server language, client platform, database/search/graph.
2. For each axis define stable interface and provider-specific adapter.
3. Separate product behavior from stack implementation: same flow must behave the same across providers.
4. Define RAG-before-AI: before each AI step, plan what context questions are needed.
5. Define context package: source docs, previous jobs, feedback, graph entities, user/project scope.
6. Define RAG-after-AI: decide what output, prompt, evidence, feedback and derived knowledge should be stored.
7. Compare GraphRAG alternatives: Azure AI, Cosmos DB, Graph AI, Pinecone, Gremlin/open source by capability and constraints.
8. Add provider selection criteria: cost, latency, graph traversal, privacy, tenant isolation, deployment.
9. Add tests: same AI step with different RAG providers gets correct context and stores feedback correctly.
10. Add missing-evidence gates: no provider decision without project constraints and deployment constraints.

### Результат для человека

Пользователь sees how XIIGen remains portable while still giving AI steps correct project context and storing learning signals back into RAG/GraphRAG.

## 11. CSS generation debug and matrix correction

Источник чата: `6d216319-7ea8-41e2-acc6-e2025d1235a8`.

### Точный фрагмент

```text
I would like to debug the css that we get, please load the code documentation and matrix to help me up to improve the css created.

I will attach improved html css code, and then our original css and css log, you will need to try to follow the css creation and suggest ideas how to improve the code, without ruining it.
Please be careful on each suggestion check out the code documentation and the matrix. If matrix is not correlated to what actually happening - fix the matrix and let me download. If the participating mix applies on other places on the matrix, suggest fix. We will test the suggestions one by one.
Let me know when the loading is done
```

### Граница задачи

Это debugging orchestration: load docs, trace CSS generation, compare expected/actual/logs, fix matrix if it lies, propose one safe change at a time.

### Не смешивать с

Не смешивать с generic Figma generation. Здесь работа идет от observed regression and evidence.

### Правильная оркестрация

1. Load code documentation and matrix before analysis.
2. Read improved HTML/CSS, original CSS and CSS log as three evidence streams.
3. Trace where each CSS decision was born in the pipeline.
4. Compare matrix claims with actual code behavior.
5. If matrix is wrong, update matrix first; do not build fixes on false documentation.
6. Identify one failure class at a time.
7. Propose minimal code change and state affected surfaces.
8. Test one suggestion at a time with visual/code outputs.
9. If the same mix affects other matrix places, flag those related fixes separately.
10. Protect working behavior; avoid broad rewrites.

### Результат для человека

Пользователь sees controlled debug work: evidence first, trace second, matrix correction third, one safe fix at a time.

## 12. Planning/review workflow with uploaded skills

Источник чата: `2d19ed25-93dc-438d-8c02-dda9a5fc7c9e`.

### Точный фрагмент

```text
Please review this plan using the uploaded planning and reviewing skills
```

### Граница задачи

Это plan review, not implementation and not summary writing.

### Не смешивать с

Не смешивать with execution. Review must first decide whether plan can pass.

### Правильная оркестрация

1. Load uploaded planning and reviewing skills before judging the plan.
2. Read the plan as a gate artifact: what it promises, what it verifies, what it omits.
3. Compare plan coverage against actual corpus/artifacts.
4. List blocking findings first.
5. Separate blocking issues from write-time improvements.
6. Check Gate B conditions: measurable, executable, complete, not vague.
7. Require plan revision where counts, gates, state, agent-executable steps or coverage are wrong.
8. If the task scope changes during review, update plan architecture instead of merely editing text.
9. Package only relevant files after review is complete.

### Результат для человека

Пользователь sees whether the plan is safe to execute, what is broken, what must be fixed, and what artifact proves the fix.

## Что v2 исправляет

1. Один длинный message больше не притворяется одним prompt.
2. Atomic tasks separated.
3. Each scenario has “do not mix with”.
4. Оркестрация строится от exact fragment, not from my invented summary.
5. Supporting infrastructure is separated from product flows.
