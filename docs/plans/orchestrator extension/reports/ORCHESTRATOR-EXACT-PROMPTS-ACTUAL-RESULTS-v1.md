# Exact original prompts -> actual XIIGen results

Source: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\orchestrator prompts.txt

## 01. Original prompt line 1

Промпт из файла:

```text
I would like to create a skills set for my new project, described in attached xml Technologies : Server - core .net latest technologies hosted on kubernetees Client react native - latest approaches
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
I would like to create a skills set for my new project, described in attached xml Technologies : Server - core .net latest technologies hosted on kubernetees Client react native - latest approaches

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "I would like to create a skills set for my new project" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "described in attached xml Technologies" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "Server - core .net latest technologies hosted on kubernetees Client react native - latest approaches" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
7. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: I would like to create a skills set for my new project?
- What project evidence is needed before planning: described in attached xml Technologies?
- What project evidence is needed before planning: Server - core .net latest technologies hosted on kubernetees Client react native - latest approaches?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 02. Original prompt line 2

Промпт из файла:

```text
I would like a simple like n8n flow management -> will trigger a process call with trace id and body, and will pull once in X seconds if the result is ready.  The initial simple flow will get a figma nodes tree array with very raw css html code, the flow will apply to several ai models with the array and previous results positive and negative feedbacks, then provide to additional review to another model and return a result to the client. On each step middle results with prompts will be stored on elastic database. The user can give feedback good, neutrile, negative and some text input - it will be saved and provided later to the machine
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
I would like a simple like n8n flow management -> will trigger a process call with trace id and body, and will pull once in X seconds if the result is ready. The initial simple flow will get a figma nodes tree array with very raw css html c...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "I would like a simple like n8n flow management" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "will trigger a process call with trace id" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "will pull once in X seconds if the result is ready. The initial simple flow will get a figma nodes tree array with very raw css html code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "the flow will apply to several ai models with the array" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "previous results positive" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "negative feedbacks" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "provide to additional review to another model" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "return a result to the client. On each step middle results with prompts will be stored on elastic database. The user can give feedback good" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "neutrile" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "negative" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "some text input - it will be saved" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "provided later to the machine" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
16. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: I would like a simple like n8n flow management?
- What project evidence is needed before planning: will trigger a process call with trace id?
- What project evidence is needed before planning: will pull once in X seconds if the result is ready. The initial simple flow will get a figma nodes tree array with very raw css html code?
- What project evidence is needed before planning: the flow will apply to several ai models with the array?
- What project evidence is needed before planning: previous results positive?
- What project evidence is needed before planning: negative feedbacks?
- What project evidence is needed before planning: provide to additional review to another model?
- What project evidence is needed before planning: return a result to the client. On each step middle results with prompts will be stored on elastic database. The user can give feedback good?
- What project evidence is needed before planning: neutrile?
- What project evidence is needed before planning: negative?
- What project evidence is needed before planning: some text input - it will be saved?
- What project evidence is needed before planning: provided later to the machine?

Что нельзя замазывать:
- External input required before final implementation detail: will pull once in X seconds if the result is ready. The initial simple flow will get a figma nodes tree array with very raw css html code.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- MissingEvidenceAbstentionUnit
```

## 03. Later usage 1

Промпт из файла:

```text
1. User enters figma, select client side and server side technologies, database -> the system orchestrates the whole system:
 a. Goes over the screens
 b. Use ai models to analyze each screen components
  c. Use ai model to analyze the sceens to understand the system 
   d. Generates architecture as a set of flows in our architecture
   e. For each screen create a tech design how the UI interact with flows, and gather feedback and process the input -> 
	- may show the result, change some subscreens, move to other screens...
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
1. User enters figma, select client side and server side technologies, database -> the system orchestrates the whole system: a. Goes over the screens b. Use ai models to analyze each screen components c. Use ai model to analyze the sceens t...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "1. User enters figma" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "select client side" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "server side technologies" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "database" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "the system orchestrates the whole system" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "a. Goes over the screens b. Use ai models to analyze each screen components c. Use ai model to analyze the sceens to understand the system d. Generates architecture as a set of flows in our architecture e. For each screen create a tech design how the UI interact with flows" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "gather feedback" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "process the input" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "may show the result" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "change some subscreens" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "move to other screens" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
15. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 1. User enters figma?
- What project evidence is needed before planning: select client side?
- What project evidence is needed before planning: server side technologies?
- What project evidence is needed before planning: database?
- What project evidence is needed before planning: the system orchestrates the whole system?
- What project evidence is needed before planning: a. Goes over the screens b. Use ai models to analyze each screen components c. Use ai model to analyze the sceens to understand the system d. Generates architecture as a set of flows in our architecture e. For each screen create a tech design how the UI interact with flows?
- What project evidence is needed before planning: gather feedback?
- What project evidence is needed before planning: process the input?
- What project evidence is needed before planning: may show the result?
- What project evidence is needed before planning: change some subscreens?
- What project evidence is needed before planning: move to other screens?

Что нельзя замазывать:
- External input required before final implementation detail: 1. User enters figma.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- MissingEvidenceAbstentionUnit
```

## 04. Later usage 2

Промпт из файла:

```text
2. User wants to integrate a figma design into his own system
	- the system need to orchestrate a documentation creation -> to document all the system parts
	- the system uses microservices and orchestrator to make a documentation and skillsets
	- the system creates based on design system theme
	- the system orchestrates applying the style on the system -> if that's not a new module
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
2. User wants to integrate a figma design into his own system - the system need to orchestrate a documentation creation -> to document all the system parts - the system uses microservices and orchestrator to make a documentation and skillse...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "2. User wants to integrate a figma design into his own system - the system need to orchestrate a documentation creation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "to document all the system parts - the system uses microservices" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "orchestrator to make a documentation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "skillsets - the system creates based on design system theme - the system orchestrates applying the style on the system" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "if that's not a new module" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
9. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 2. User wants to integrate a figma design into his own system - the system need to orchestrate a documentation creation?
- What project evidence is needed before planning: to document all the system parts - the system uses microservices?
- What project evidence is needed before planning: orchestrator to make a documentation?
- What project evidence is needed before planning: skillsets - the system creates based on design system theme - the system orchestrates applying the style on the system?
- What project evidence is needed before planning: if that's not a new module?

Что нельзя замазывать:
- External input required before final implementation detail: 2. User wants to integrate a figma design into his own system - the system need to orchestrate a documentation creation.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- MissingEvidenceAbstentionUnit
```

## 05. Later usage 3

Промпт из файла:

```text
3. User wants to implement the whole system from scratch, and a design is not covering what he needs
We need to generate system by user explanation
We need to make a design theme based on what he already have
We need to make an architecture
skills
implementation plan
Implement the plan 
Each one of these can be a flow involving AI -> when AI is used for generation, planning, orchestrating, and the system gives a space to test the design, deploy, visualize, debug, and eventually get the source code, to the user. He will be able to keep it on our platform, or on his own...
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
3. User wants to implement the whole system from scratch, and a design is not covering what he needs We need to generate system by user explanation We need to make a design theme based on what he already have We need to make an architecture...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "3. User wants to implement the whole system from scratch" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "a design is not covering what he needs We need to generate system by user explanation We need to make a design theme based on what he already have We need to make an architecture skills implementation plan Implement the plan Each one of these can be a flow involving AI" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "when AI is used for generation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "planning" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "orchestrating" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "the system gives a space to test the design" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "deploy" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "visualize" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "debug" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "eventually get the source code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "to the user. He will be able to keep it on our platform" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "or on his own" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
16. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 3. User wants to implement the whole system from scratch?
- What project evidence is needed before planning: a design is not covering what he needs We need to generate system by user explanation We need to make a design theme based on what he already have We need to make an architecture skills implementation plan Implement the plan Each one of these can be a flow involving AI?
- What project evidence is needed before planning: when AI is used for generation?
- What project evidence is needed before planning: planning?
- What project evidence is needed before planning: orchestrating?
- What project evidence is needed before planning: the system gives a space to test the design?
- What project evidence is needed before planning: deploy?
- What project evidence is needed before planning: visualize?
- What project evidence is needed before planning: debug?
- What project evidence is needed before planning: eventually get the source code?
- What project evidence is needed before planning: to the user. He will be able to keep it on our platform?
- What project evidence is needed before planning: or on his own?

Что нельзя замазывать:
- External input required before final implementation detail: to the user. He will be able to keep it on our platform.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- MissingEvidenceAbstentionUnit
```

## 06. Later usage 4

Промпт из файла:

```text
4. User want to create an app that interracts with other users (his potential clients)
These client on registration will fill up a form
form data will be saved 
AI diet generation plan flow will apply -> AI will orchestrate the generation-> will apply several AI using user data -> will analyze all models, run several round if required-> generate a diet plan
Each day will be triggered a flow of gathering what client ate, client will make a list and submit it in WhatsApp
AI will use a generated flow to calculate his callories and supplements, and suggest adjustments -> again several rounds with ai and analyze -> the daily feedback will be sent to the client (on push on app and on WhatsApp interaction
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
4. User want to create an app that interracts with other users (his potential clients) These client on registration will fill up a form form data will be saved AI diet generation plan flow will apply -> AI will orchestrate the generation-> ...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "4. User want to create an app that interracts with other users (his potential clients) These client on registration will fill up a form form data will be saved AI diet generation plan flow will apply" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "AI will orchestrate the generation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "will apply several AI using user data" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "will analyze all models" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "run several round if required" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "generate a diet plan Each day will be triggered a flow of gathering what client ate" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "client will make a list" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "submit it in WhatsApp AI will use a generated flow to calculate his callories" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "supplements" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "suggest adjustments" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "again several rounds with ai" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "analyze" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Для части "the daily feedback will be sent to the client (on push on app" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
16. Для части "on WhatsApp interaction" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
17. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
18. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 4. User want to create an app that interracts with other users (his potential clients) These client on registration will fill up a form form data will be saved AI diet generation plan flow will apply?
- What project evidence is needed before planning: AI will orchestrate the generation?
- What project evidence is needed before planning: will apply several AI using user data?
- What project evidence is needed before planning: will analyze all models?
- What project evidence is needed before planning: run several round if required?
- What project evidence is needed before planning: generate a diet plan Each day will be triggered a flow of gathering what client ate?
- What project evidence is needed before planning: client will make a list?
- What project evidence is needed before planning: submit it in WhatsApp AI will use a generated flow to calculate his callories?
- What project evidence is needed before planning: supplements?
- What project evidence is needed before planning: suggest adjustments?
- What project evidence is needed before planning: again several rounds with ai?
- What project evidence is needed before planning: analyze?
- What project evidence is needed before planning: the daily feedback will be sent to the client (on push on app?
- What project evidence is needed before planning: on WhatsApp interaction?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 07. Later usage 5

Промпт из файла:

```text
5. User want's to generate content, he will read loud or write a text -> the flow of content generation will be applied -> first audio to text if required, text improvements with ai , create plan for images, and videos of 3 seconds -> will apply to image generator with image prototype and generated prompts. Will use the images, and generated video prompts to generate a video. Will use ai to generate some music. Will generate audio based on user sound and avatar. Will use a video generator by a template for each platform based on short videos, music and captions. Will generate video description for each platform. Will schedule a post on each platform. 
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
5. User want's to generate content, he will read loud or write a text -> the flow of content generation will be applied -> first audio to text if required, text improvements with ai , create plan for images, and videos of 3 seconds -> will ...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "5. User want's to generate content" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "he will read loud or write a text" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "the flow of content generation will be applied" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "first audio to text if required" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "text improvements with ai" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "create plan for images" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "videos of 3 seconds" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "will apply to image generator with image prototype" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "generated prompts. Will use the images" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "generated video prompts to generate a video. Will use ai to generate some music. Will generate audio based on user sound" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "avatar. Will use a video generator by a template for each platform based on short videos" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "music" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Для части "captions. Will generate video description for each platform. Will schedule a post on each platform" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
16. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
17. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 5. User want's to generate content?
- What project evidence is needed before planning: he will read loud or write a text?
- What project evidence is needed before planning: the flow of content generation will be applied?
- What project evidence is needed before planning: first audio to text if required?
- What project evidence is needed before planning: text improvements with ai?
- What project evidence is needed before planning: create plan for images?
- What project evidence is needed before planning: videos of 3 seconds?
- What project evidence is needed before planning: will apply to image generator with image prototype?
- What project evidence is needed before planning: generated prompts. Will use the images?
- What project evidence is needed before planning: generated video prompts to generate a video. Will use ai to generate some music. Will generate audio based on user sound?
- What project evidence is needed before planning: avatar. Will use a video generator by a template for each platform based on short videos?
- What project evidence is needed before planning: music?
- What project evidence is needed before planning: captions. Will generate video description for each platform. Will schedule a post on each platform?

Что нельзя замазывать:
- External input required before final implementation detail: first audio to text if required.
- External input required before final implementation detail: generated video prompts to generate a video. Will use ai to generate some music. Will generate audio based on user sound.
- External input required before final implementation detail: avatar. Will use a video generator by a template for each platform based on short videos.
- External input required before final implementation detail: captions. Will generate video description for each platform. Will schedule a post on each platform.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- MissingEvidenceAbstentionUnit
```

## 08. Design pattern skills

Промпт из файла:

```text
6. There are different design patterns explanations - I need a skill for each one of them -> When is good to use,  how to use it, for each language, common combinations...
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
6. There are different design patterns explanations - I need a skill for each one of them -> When is good to use, how to use it, for each language, common combinations...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "6. There are different design patterns explanations - I need a skill for each one of them" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "When is good to use" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "how to use it" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "for each language" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "common combinations" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
9. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 6. There are different design patterns explanations - I need a skill for each one of them?
- What project evidence is needed before planning: When is good to use?
- What project evidence is needed before planning: how to use it?
- What project evidence is needed before planning: for each language?
- What project evidence is needed before planning: common combinations?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 09. Possible UMLs

Промпт из файла:

```text
7. Our model need to support advanced scenarios like described in "possible umls.drawio.xml" both to be able to document and validate the full cycle, including automations, entity relations
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
7. Our model need to support advanced scenarios like described in "possible umls.drawio.xml" both to be able to document and validate the full cycle, including automations, entity relations

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "7. Our model need to support advanced scenarios like described in "possible umls.drawio.xml" both to be able to document" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "validate the full cycle" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "including automations" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "entity relations" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
8. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 7. Our model need to support advanced scenarios like described in "possible umls.drawio.xml" both to be able to document?
- What project evidence is needed before planning: validate the full cycle?
- What project evidence is needed before planning: including automations?
- What project evidence is needed before planning: entity relations?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 10. Project base examples

Промпт из файла:

```text
8. "project base.drawio.xml" has example flow for each 1...5 scenarios
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
8. "project base.drawio.xml" has example flow for each 1...5 scenarios

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "8. "project base.drawio.xml" has example flow for each 1...5 scenarios" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
5. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 8. "project base.drawio.xml" has example flow for each 1...5 scenarios?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 11. Small Business Networking App spec

Промпт из файла:

```text
9. our model needs to be able to generate, deploy and debug a system like "Small Business Networking App - Functional Specifications & Architecture.docx"
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
9. our model needs to be able to generate, deploy and debug a system like "Small Business Networking App - Functional Specifications & Architecture.docx"

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "9. our model needs to be able to generate" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "deploy" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "debug a system like "Small Business Networking App - Functional Specifications & Architecture.docx"" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
7. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 9. our model needs to be able to generate?
- What project evidence is needed before planning: deploy?
- What project evidence is needed before planning: debug a system like "Small Business Networking App - Functional Specifications & Architecture.docx"?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 12. Dynamic code examples and AI microservices

Промпт из файла:

```text
10. Dynamic code examples - our code skills zip - "genie*" - Please use these to be able to generate dynamic code skills, processing, approach to code. Always check with these skill sets implementation - it generic dynamic approach is the DNA of our company.
So for these purposes we need the AI microservices to be able to get prompts from previos jobs 
We need to be able to see the orchestrated flow on the UI
We need to be able to debug each node execution -> what input he got, what he did in the middle, what were the results
We need to be able to generate the code, start microservice with new internal code, test it, and integrate it into the flow
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
10. Dynamic code examples - our code skills zip - "genie*" - Please use these to be able to generate dynamic code skills, processing, approach to code. Always check with these skill sets implementation - it generic dynamic approach is the D...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "10. Dynamic code examples - our code skills zip - "genie*" - Please use these to be able to generate dynamic code skills" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "processing" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "approach to code. Always check with these skill sets implementation - it generic dynamic approach is the DNA of our company. So for these purposes we need the AI microservices to be able to get prompts from previos jobs We need to be able to see the orchestrated flow on the UI We need to be able to debug each node execution" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "what input he got" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "what he did in the middle" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "what were the results We need to be able to generate the code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "start microservice with new internal code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "test it" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "integrate it into the flow" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
13. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: 10. Dynamic code examples - our code skills zip - "genie*" - Please use these to be able to generate dynamic code skills?
- What project evidence is needed before planning: processing?
- What project evidence is needed before planning: approach to code. Always check with these skill sets implementation - it generic dynamic approach is the DNA of our company. So for these purposes we need the AI microservices to be able to get prompts from previos jobs We need to be able to see the orchestrated flow on the UI We need to be able to debug each node execution?
- What project evidence is needed before planning: what input he got?
- What project evidence is needed before planning: what he did in the middle?
- What project evidence is needed before planning: what were the results We need to be able to generate the code?
- What project evidence is needed before planning: start microservice with new internal code?
- What project evidence is needed before planning: test it?
- What project evidence is needed before planning: integrate it into the flow?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 13. Architecture and skill set

Промпт из файла:

```text
I need a plan for the architecture and skill set that will be required for project.
In the files attached we have some solution suggestions - you need to examine each file carefully and see if it can help with the plan
Several requests:
I prefer a generic interfaces which I can reuse for databases, queues, 3d parity like AI
I prefer a generic code - the repository that gets dynamic document and saves it. Get a dynamic filter and filter by not empty values - like in attached c  - Data Base Fabric described in doc - but using the latest .net 7,8,9 principles
I need to manage a queue of jobs that done in each level - for example async call for 4 different models
I prefer to create a generic microservice base which will have basically architectural capabilities like in attached document
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
I need a plan for the architecture and skill set that will be required for project. In the files attached we have some solution suggestions - you need to examine each file carefully and see if it can help with the plan Several requests: I p...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "I need a plan for the architecture" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "skill set that will be required for project. In the files attached we have some solution suggestions - you need to examine each file carefully" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "see if it can help with the plan Several requests" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "I prefer a generic interfaces which I can reuse for databases" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "queues" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "3d parity like AI I prefer a generic code - the repository that gets dynamic document" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "saves it. Get a dynamic filter" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "filter by not empty values - like in attached c - Data Base Fabric described in doc - but using the latest .net 7" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "9 principles I need to manage a queue of jobs that done in each level - for example async call for 4 different models I prefer to create a generic microservice base which will have basically architectural capabilities like in attached document" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
13. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: I need a plan for the architecture?
- What project evidence is needed before planning: skill set that will be required for project. In the files attached we have some solution suggestions - you need to examine each file carefully?
- What project evidence is needed before planning: see if it can help with the plan Several requests?
- What project evidence is needed before planning: I prefer a generic interfaces which I can reuse for databases?
- What project evidence is needed before planning: queues?
- What project evidence is needed before planning: 3d parity like AI I prefer a generic code - the repository that gets dynamic document?
- What project evidence is needed before planning: saves it. Get a dynamic filter?
- What project evidence is needed before planning: filter by not empty values - like in attached c - Data Base Fabric described in doc - but using the latest .net 7?
- What project evidence is needed before planning: 9 principles I need to manage a queue of jobs that done in each level - for example async call for 4 different models I prefer to create a generic microservice base which will have basically architectural capabilities like in attached document?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 14. Alternative implementation paths

Промпт из файла:

```text
I need to support an alternative possible ways
1. Use other queues like kafka and sqs and other open source common 
2. Server can be implemented on node.js , rust , Java, python or php  - which means each skill need to has a subfolder of appropriate technology
3. Client can be implemented in native, angular or vue - which means each skill need to has a subfolder of appropriate technology
4. Database Fabric can be mongo, prisma, redis, Azure AI Search, Graph AI and other common models
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
I need to support an alternative possible ways 1. Use other queues like kafka and sqs and other open source common 2. Server can be implemented on node.js , rust , Java, python or php - which means each skill need to has a subfolder of appr...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "I need to support an alternative possible ways 1. Use other queues like kafka" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "other open source common 2. Server can be implemented on node.js" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "python or php - which means each skill need to has a subfolder of appropriate technology 3. Client can be implemented in native" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "angular or vue - which means each skill need to has a subfolder of appropriate technology 4. Database Fabric can be mongo" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "prisma" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "redis" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "Azure AI Search" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "Graph AI" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "other common models" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
13. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: I need to support an alternative possible ways 1. Use other queues like kafka?
- What project evidence is needed before planning: other open source common 2. Server can be implemented on node.js?
- What project evidence is needed before planning: python or php - which means each skill need to has a subfolder of appropriate technology 3. Client can be implemented in native?
- What project evidence is needed before planning: angular or vue - which means each skill need to has a subfolder of appropriate technology 4. Database Fabric can be mongo?
- What project evidence is needed before planning: prisma?
- What project evidence is needed before planning: redis?
- What project evidence is needed before planning: Azure AI Search?
- What project evidence is needed before planning: Graph AI?
- What project evidence is needed before planning: other common models?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
```

## 15. Graph and RAG capabilities

Промпт из файла:

```text
Important notice - we need a good graph and rag capabilities to supply these AI stages with appropriate context - so each time we apply to AI we need to make a plan with AI which requests we need to apply on our current RAG. And on each end of execution and feedback - we need to plan with AI how and where store the data on RAG - (Rag can be azure ai, cosmo db, graph ai, pinecore, some other open source, gremlin)
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Important notice - we need a good graph and rag capabilities to supply these AI stages with appropriate context - so each time we apply to AI we need to make a plan with AI which requests we need to apply on our current RAG. And on each end...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Important notice - we need a good graph" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "rag capabilities to supply these AI stages with appropriate context - so each time we apply to AI we need to make a plan with AI which requests we need to apply on our current RAG" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "on each end of execution" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "feedback - we need to plan with AI how" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "where store the data on RAG - (Rag can be azure ai" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "cosmo db" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "graph ai" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "pinecore" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "some other open source" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "gremlin)" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
14. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Important notice - we need a good graph?
- What project evidence is needed before planning: rag capabilities to supply these AI stages with appropriate context - so each time we apply to AI we need to make a plan with AI which requests we need to apply on our current RAG?
- What project evidence is needed before planning: on each end of execution?
- What project evidence is needed before planning: feedback - we need to plan with AI how?
- What project evidence is needed before planning: where store the data on RAG - (Rag can be azure ai?
- What project evidence is needed before planning: cosmo db?
- What project evidence is needed before planning: graph ai?
- What project evidence is needed before planning: pinecore?
- What project evidence is needed before planning: some other open source?
- What project evidence is needed before planning: gremlin)?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- WikiContextRoutingUnit
```

## 16. Advanced skills

Промпт из файла:

```text
I would like to supply these skill sets as a part of code implementation
Advanced skills:
1. Creating prompts to implement different phases of the system
2. Unit testing - each implemented part need to have a high test coverage
3. e2e testing for each implementation part
4. UI testing - components creation, and actual test cases
5. Devops skills -> integrate with the existing infrastructure on different hosting type + automatic CI CD creation
6. Documentation skill - document evyrithing for future support
7. Swagger skill
8. mcp skill - give ai access to all the apis to be able to integrate the different parts of the system , debug and adjust the code
9. log skills - generate logging  especially in exception middleware -> log the Ui as well
10. Safe code - > use key vaults, check vulnerabilities - > upgrade the code
11. Optimization - resource management and optimizations
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
I would like to supply these skill sets as a part of code implementation Advanced skills: 1. Creating prompts to implement different phases of the system 2. Unit testing - each implemented part need to have a high test coverage 3. e2e testi...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "I would like to supply these skill sets as a part of code implementation Advanced skills" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "1. Creating prompts to implement different phases of the system 2. Unit testing - each implemented part need to have a high test coverage 3. e2e testing for each implementation part 4. UI testing - components creation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "actual test cases 5. Devops skills" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "integrate with the existing infrastructure on different hosting type + automatic CI CD creation 6. Documentation skill - document evyrithing for future support 7. Swagger skill 8. mcp skill - give ai access to all the apis to be able to integrate the different parts of the system" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "debug" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "adjust the code 9. log skills - generate logging especially in exception middleware" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "log the Ui as well 10. Safe code - > use key vaults" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "check vulnerabilities - > upgrade the code 11. Optimization - resource management" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "optimizations" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
13. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: I would like to supply these skill sets as a part of code implementation Advanced skills?
- What project evidence is needed before planning: 1. Creating prompts to implement different phases of the system 2. Unit testing - each implemented part need to have a high test coverage 3. e2e testing for each implementation part 4. UI testing - components creation?
- What project evidence is needed before planning: actual test cases 5. Devops skills?
- What project evidence is needed before planning: integrate with the existing infrastructure on different hosting type + automatic CI CD creation 6. Documentation skill - document evyrithing for future support 7. Swagger skill 8. mcp skill - give ai access to all the apis to be able to integrate the different parts of the system?
- What project evidence is needed before planning: debug?
- What project evidence is needed before planning: adjust the code 9. log skills - generate logging especially in exception middleware?
- What project evidence is needed before planning: log the Ui as well 10. Safe code - > use key vaults?
- What project evidence is needed before planning: check vulnerabilities - > upgrade the code 11. Optimization - resource management?
- What project evidence is needed before planning: optimizations?

Что нельзя замазывать:
- External input required before final implementation detail: integrate with the existing infrastructure on different hosting type + automatic CI CD creation 6. Documentation skill - document evyrithing for future support 7. Swagger skill 8. mcp skill - give ai access to all the apis to be able to integrate the different parts of the system.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- WikiContextRoutingUnit
- MissingEvidenceAbstentionUnit
```

## 17. Advanced RAG alternatives

Промпт из файла:

```text
Advanced Rag alternatives: Here's a quick summary of each GraphRAG alternative:
Neo4j GraphRAG Ã¢â‚¬â€ The industry standard. Enterprise-grade graph database with the best Claude integration (dedicated SDK + MCP connector). Rich ecosystem, Cypher query language, extensive documentation. Free tier available, paid starts ~$65/mo.
LightRAG Ã¢â‚¬â€ Lightweight, cost-efficient alternative from HKU. Uses dual-level retrieval (entities + themes) with 80-90% fewer API calls than Microsoft GraphRAG. Fully open source (MIT). Best for budget-conscious teams.
nano-graphrag Ã¢â‚¬â€ Minimal, hackable reimplementation of Microsoft GraphRAG. Clean, readable codebase you can easily modify. Open source, no dependencies beyond Python. Best for developers who want full control.
LangChain KG RAG Ã¢â‚¬â€ Popular orchestration framework with GraphCypherQAChain for graph queries. First-class Claude support. Huge ecosystem (95K+ GitHub stars). Best if you're already in the LangChain world.
LlamaIndex GraphRAG Ã¢â‚¬â€ Data-focused framework with 160+ connectors. Strong AWS Bedrock integration with official Neptune + Claude tutorial. Best for data-heavy pipelines and rapid prototyping.
Graphlit Ã¢â‚¬â€ Fully managed SaaS platform. Auto-extracts knowledge graphs from documents, audio, video. Zero infrastructure to manage. Free up to 1GB, then $49/mo. Best for teams wanting zero-ops.
Memgraph Ã¢â‚¬â€ In-memory graph database optimized for real-time streaming. Integrates with Kafka/Pulsar. Works as a backend for LightRAG or LangChain. Best for real-time analytics and fraud detection.
Neptune + Bedrock Ã¢â‚¬â€ AWS-native fully managed stack. Official Claude integration via Bedrock. No free tier. Best for enterprises already on AWS who want managed infrastructure.
Contextual AI Ã¢â‚¬â€ Agentic approach that skips building a static knowledge graph. Uses metadata indexing + agent-driven traversal. Enterprise pricing. Best for teams wanting graph-like reasoning without graph maintenance.
HybridRAG (DIY) Ã¢â‚¬â€ Not a product Ã¢â‚¬â€ an architecture pattern. Combine any vector DB + graph DB + re-ranking. Maximum flexibility and cost control. Best for teams needing full customization.
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Advanced Rag alternatives: Here's a quick summary of each GraphRAG alternative: Neo4j GraphRAG Ã¢â‚¬â€ The industry standard. Enterprise-grade graph database with the best Claude integration (dedicated SDK + MCP connector). Rich ecosystem,...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Advanced Rag alternatives" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "Here's a quick summary of each GraphRAG alternative" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "Neo4j GraphRAG Ã¢â‚¬â€ The industry standard. Enterprise-grade graph database with the best Claude integration (dedicated SDK + MCP connector). Rich ecosystem" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "Cypher query language" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "extensive documentation. Free tier available" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "paid starts ~$65/mo. LightRAG Ã¢â‚¬â€ Lightweight" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "cost-efficient alternative from HKU. Uses dual-level retrieval (entities + themes) with 80-90% fewer API calls than Microsoft GraphRAG. Fully open source (MIT). Best for budget-conscious teams. nano-graphrag Ã¢â‚¬â€ Minimal" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "hackable reimplementation of Microsoft GraphRAG. Clean" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "readable codebase you can easily modify. Open source" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "no dependencies beyond Python. Best for developers who want full control. LangChain KG RAG Ã¢â‚¬â€ Popular orchestration framework with GraphCypherQAChain for graph queries. First-class Claude support. Huge ecosystem (95K+ GitHub stars). Best if you're already in the LangChain world. LlamaIndex GraphRAG Ã¢â‚¬â€ Data-focused framework with 160+ connectors. Strong AWS Bedrock integration with official Neptune + Claude tutorial. Best for data-heavy pipelines" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "rapid prototyping. Graphlit Ã¢â‚¬â€ Fully managed SaaS platform. Auto-extracts knowledge graphs from documents" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "audio" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Для части "video. Zero infrastructure to manage. Free up to 1GB" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
16. Для части "$49/mo. Best for teams wanting zero-ops. Memgraph Ã¢â‚¬â€ In-memory graph database optimized for real-time streaming. Integrates with Kafka/Pulsar. Works as a backend for LightRAG or LangChain. Best for real-time analytics" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
17. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
18. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Advanced Rag alternatives?
- What project evidence is needed before planning: Here's a quick summary of each GraphRAG alternative?
- What project evidence is needed before planning: Neo4j GraphRAG Ã¢â‚¬â€ The industry standard. Enterprise-grade graph database with the best Claude integration (dedicated SDK + MCP connector). Rich ecosystem?
- What project evidence is needed before planning: Cypher query language?
- What project evidence is needed before planning: extensive documentation. Free tier available?
- What project evidence is needed before planning: paid starts ~$65/mo. LightRAG Ã¢â‚¬â€ Lightweight?
- What project evidence is needed before planning: cost-efficient alternative from HKU. Uses dual-level retrieval (entities + themes) with 80-90% fewer API calls than Microsoft GraphRAG. Fully open source (MIT). Best for budget-conscious teams. nano-graphrag Ã¢â‚¬â€ Minimal?
- What project evidence is needed before planning: hackable reimplementation of Microsoft GraphRAG. Clean?
- What project evidence is needed before planning: readable codebase you can easily modify. Open source?
- What project evidence is needed before planning: no dependencies beyond Python. Best for developers who want full control. LangChain KG RAG Ã¢â‚¬â€ Popular orchestration framework with GraphCypherQAChain for graph queries. First-class Claude support. Huge ecosystem (95K+ GitHub stars). Best if you're already in the LangChain world. LlamaIndex GraphRAG Ã¢â‚¬â€ Data-focused framework with 160+ connectors. Strong AWS Bedrock integration with official Neptune + Claude tutorial. Best for data-heavy pipelines?
- What project evidence is needed before planning: rapid prototyping. Graphlit Ã¢â‚¬â€ Fully managed SaaS platform. Auto-extracts knowledge graphs from documents?
- What project evidence is needed before planning: audio?
- What project evidence is needed before planning: video. Zero infrastructure to manage. Free up to 1GB?
- What project evidence is needed before planning: $49/mo. Best for teams wanting zero-ops. Memgraph Ã¢â‚¬â€ In-memory graph database optimized for real-time streaming. Integrates with Kafka/Pulsar. Works as a backend for LightRAG or LangChain. Best for real-time analytics?

Что нельзя замазывать:
- External input required before final implementation detail: rapid prototyping. Graphlit Ã¢â‚¬â€ Fully managed SaaS platform. Auto-extracts knowledge graphs from documents.
- External input required before final implementation detail: audio.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- WikiContextRoutingUnit
- MissingEvidenceAbstentionUnit
```

## 18. Advanced devops skills

Промпт из файла:

```text
Advanced devops skills :
Kubernetes Hosting Alternatives Ã¢â‚¬â€ Quick Overview
PaaS (Easiest)
PlatformPriceBest ForRailwayfrom $5/mo + usageFast GitHub deploy, visual UIRenderfrom $7/mo, free tierHeroku replacement, built-in DBs & monitoringFly.iofrom $2/mo per VMGlobal edge network, low latencyVercelfree / $20 ProFrontend & Next.js only
Managed Kubernetes
PlatformControl PlaneFeaturesGKE (Google)freeBest K8s integration, automationEKS (AWS)$0.10/hrIAM, CloudWatch, Fargate supportAKS (Azure)freeAzure AD, Windows containersDigitalOceanfreeSimpler & cheaper than big three
Lightweight Orchestrators
PlatformTypeFeaturesNomadOrchestratorSingle binary, containers + VMsDocker SwarmOrchestratorBuilt into Docker, very simpleK3s / MicroK8sMini-K8sFor edge, IoT, dev environments
Enterprise
PlatformPriceFeaturesOpenShiftper CPU coreGitOps, security, complianceRancheropen-sourceMulti-cluster, hybrid cloud
Quick Pick:
* Prototype Ã¢â€ â€™ Railway / Render
* Production NestJS Ã¢â€ â€™ Render or AKS
* Global service Ã¢â€ â€™ Fly.io
* Enterprise Ã¢â€ â€™ OpenShift
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Advanced devops skills : Kubernetes Hosting Alternatives Ã¢â‚¬â€ Quick Overview PaaS (Easiest) PlatformPriceBest ForRailwayfrom $5/mo + usageFast GitHub deploy, visual UIRenderfrom $7/mo, free tierHeroku replacement, built-in DBs & monitor...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Advanced devops skills" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "Kubernetes Hosting Alternatives Ã¢â‚¬â€ Quick Overview PaaS (Easiest) PlatformPriceBest ForRailwayfrom $5/mo + usageFast GitHub deploy" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "visual UIRenderfrom $7/mo" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "free tierHeroku replacement" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "built-in DBs & monitoringFly.iofrom $2/mo per VMGlobal edge network" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "low latencyVercelfree / $20 ProFrontend & Next.js only Managed Kubernetes PlatformControl PlaneFeaturesGKE (Google)freeBest K8s integration" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "automationEKS (AWS)$0.10/hrIAM" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "CloudWatch" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "Fargate supportAKS (Azure)freeAzure AD" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "Windows containersDigitalOceanfreeSimpler & cheaper than big three Lightweight Orchestrators PlatformTypeFeaturesNomadOrchestratorSingle binary" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "containers + VMsDocker SwarmOrchestratorBuilt into Docker" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "very simpleK3s / MicroK8sMini-K8sFor edge" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Для части "dev environments Enterprise PlatformPriceFeaturesOpenShiftper CPU coreGitOps" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
16. Для части "security" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
17. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
18. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Advanced devops skills?
- What project evidence is needed before planning: Kubernetes Hosting Alternatives Ã¢â‚¬â€ Quick Overview PaaS (Easiest) PlatformPriceBest ForRailwayfrom $5/mo + usageFast GitHub deploy?
- What project evidence is needed before planning: visual UIRenderfrom $7/mo?
- What project evidence is needed before planning: free tierHeroku replacement?
- What project evidence is needed before planning: built-in DBs & monitoringFly.iofrom $2/mo per VMGlobal edge network?
- What project evidence is needed before planning: low latencyVercelfree / $20 ProFrontend & Next.js only Managed Kubernetes PlatformControl PlaneFeaturesGKE (Google)freeBest K8s integration?
- What project evidence is needed before planning: automationEKS (AWS)$0.10/hrIAM?
- What project evidence is needed before planning: CloudWatch?
- What project evidence is needed before planning: Fargate supportAKS (Azure)freeAzure AD?
- What project evidence is needed before planning: Windows containersDigitalOceanfreeSimpler & cheaper than big three Lightweight Orchestrators PlatformTypeFeaturesNomadOrchestratorSingle binary?
- What project evidence is needed before planning: containers + VMsDocker SwarmOrchestratorBuilt into Docker?
- What project evidence is needed before planning: very simpleK3s / MicroK8sMini-K8sFor edge?
- What project evidence is needed before planning: dev environments Enterprise PlatformPriceFeaturesOpenShiftper CPU coreGitOps?
- What project evidence is needed before planning: security?

Что нельзя замазывать:
- External input required before final implementation detail: Kubernetes Hosting Alternatives Ã¢â‚¬â€ Quick Overview PaaS (Easiest) PlatformPriceBest ForRailwayfrom $5/mo + usageFast GitHub deploy.
- External input required before final implementation detail: low latencyVercelfree / $20 ProFrontend & Next.js only Managed Kubernetes PlatformControl PlaneFeaturesGKE (Google)freeBest K8s integration.
- External input required before final implementation detail: Windows containersDigitalOceanfreeSimpler & cheaper than big three Lightweight Orchestrators PlatformTypeFeaturesNomadOrchestratorSingle binary.
- External input required before final implementation detail: dev environments Enterprise PlatformPriceFeaturesOpenShiftper CPU coreGitOps.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- MissingEvidenceAbstentionUnit
```

## 19. Reusable AI skill library and basic prompt instructions

Промпт из файла:

```text
I need the skills orginized in a library that diferent ai can use: copilot, claude ai, claude code, cursor...
I need prompts to implement the system ordered by phases
I need a chat user can interact with a system
Please follow carefully "basic prompt.txt" instructions. 
Please first make a no code explanation of what you are going to do - devide to phases
Then validate the plan - it actually covers all the requirements in the prompt
Then make a positive and negative examples of the expected results
For each phase - it needs to be short enough to save the current state and to be able to recover with minimal time lost.
If needed, make a small rag of documents you have - or a small skills factory to be able to accomplish the task
Do not reset files - first check what is already created and continue from there. But first make what is already done to be available to download.
Please save states to be able to continue from where you stopped.
```

Результат:

```text
Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
I need the skills orginized in a library that diferent ai can use: copilot, claude ai, claude code, cursor... I need prompts to implement the system ordered by phases I need a chat user can interact with a system Please follow carefully "ba...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicApp: wiki evidence says # GenieDynamicApp local wiki evidence
- GenieDynamicApp: wiki evidence says UI surface: GenieDynamicApp can host a content generation flow screen with intake, review, revision, and publishing status panels.
- GenieDynamicApp: wiki evidence says Client integration: Angular app should call backend job endpoints instead of generating media or orchestration decisions inside the client.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "I need the skills orginized in a library that diferent ai can use" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "copilot" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "claude ai" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "claude code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "cursor... I need prompts to implement the system ordered by phases I need a chat user can interact with a system Please follow carefully "basic prompt.txt" instructions. Please first make a no code explanation of what you are going to do - devide to phases" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "validate the plan - it actually covers all the requirements in the prompt" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "make a positive" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "negative examples of the expected results For each phase - it needs to be short enough to save the current state" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "to be able to recover with minimal time lost. If needed" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "make a small rag of documents you have - or a small skills factory to be able to accomplish the task Do not reset files - first check what is already created" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "continue from there. But first make what is already done to be available to download. Please save states to be able to continue from where you stopped" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
15. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: I need the skills orginized in a library that diferent ai can use?
- What project evidence is needed before planning: copilot?
- What project evidence is needed before planning: claude ai?
- What project evidence is needed before planning: claude code?
- What project evidence is needed before planning: cursor... I need prompts to implement the system ordered by phases I need a chat user can interact with a system Please follow carefully "basic prompt.txt" instructions. Please first make a no code explanation of what you are going to do - devide to phases?
- What project evidence is needed before planning: validate the plan - it actually covers all the requirements in the prompt?
- What project evidence is needed before planning: make a positive?
- What project evidence is needed before planning: negative examples of the expected results For each phase - it needs to be short enough to save the current state?
- What project evidence is needed before planning: to be able to recover with minimal time lost. If needed?
- What project evidence is needed before planning: make a small rag of documents you have - or a small skills factory to be able to accomplish the task Do not reset files - first check what is already created?
- What project evidence is needed before planning: continue from there. But first make what is already done to be available to download. Please save states to be able to continue from where you stopped?

Что нельзя замазывать:
- Критичных missing evidence на этом уровне не найдено.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- WikiContextRoutingUnit
```

