# Actual XIIGen prompt orchestration outputs

## 01. Create skill set for new project

Промпт:

```text
I would like to create a skills set for my new project, described in attached xml Technologies : Server - core .net latest technologies hosted on kubernetees Client react native - latest approaches
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
I would like to create a skills set for my new project, described in attached xml Technologies : Server - core .net latest technologies hosted on kubernetees Client react native - latest approaches

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
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

---

## 02. n8n-like flow management

Промпт:

```text
I would like a simple like n8n flow management -> will trigger a process call with trace id and body, and will pull once in X seconds if the result is ready. The initial simple flow will get a figma nodes tree array with very raw css html code, the flow will apply to several ai models with the array and previous results positive and negative feedbacks, then provide to additional review to another model and return a result to the client. On each step middle results with prompts will be stored on elastic database. The user can give feedback good, neutral, negative and some text input - it will be saved and provided later to the machine
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
I would like a simple like n8n flow management -> will trigger a process call with trace id and body, and will pull once in X seconds if the result is ready. The initial simple flow will get a figma nodes tree array with very raw css html c...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
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
11. Для части "neutral" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
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
- What project evidence is needed before planning: neutral?
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

---

## 03. Figma to whole system

Промпт:

```text
User enters figma, select client side and server side technologies, database -> the system orchestrates the whole system: goes over the screens, uses ai models to analyze each screen components, uses ai model to analyze the screens to understand the system, generates architecture as a set of flows in our architecture, for each screen create a tech design how the UI interact with flows, and gather feedback and process the input.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
User enters figma, select client side and server side technologies, database -> the system orchestrates the whole system: goes over the screens, uses ai models to analyze each screen components, uses ai model to analyze the screens to under...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "select client side" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "server side technologies" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "database" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "the system orchestrates the whole system" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "goes over the screens" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "uses ai models to analyze each screen components" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "uses ai model to analyze the screens to understand the system" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "generates architecture as a set of flows in our architecture" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "for each screen create a tech design how the UI interact with flows" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "gather feedback" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "process the input" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
15. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: select client side?
- What project evidence is needed before planning: server side technologies?
- What project evidence is needed before planning: database?
- What project evidence is needed before planning: the system orchestrates the whole system?
- What project evidence is needed before planning: goes over the screens?
- What project evidence is needed before planning: uses ai models to analyze each screen components?
- What project evidence is needed before planning: uses ai model to analyze the screens to understand the system?
- What project evidence is needed before planning: generates architecture as a set of flows in our architecture?
- What project evidence is needed before planning: for each screen create a tech design how the UI interact with flows?
- What project evidence is needed before planning: gather feedback?
- What project evidence is needed before planning: process the input?

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

---

## 04. Integrate Figma design into existing system

Промпт:

```text
User wants to integrate a figma design into his own system. The system needs to orchestrate documentation creation, document all system parts, use microservices and orchestrator to make documentation and skillsets, create based on design system theme, and orchestrate applying the style on the system if that is not a new module.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
User wants to integrate a figma design into his own system. The system needs to orchestrate documentation creation, document all system parts, use microservices and orchestrator to make documentation and skillsets, create based on design sy...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "document all system parts" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "use microservices" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "orchestrator to make documentation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "skillsets" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "create based on design system theme" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "orchestrate applying the style on the system if that is not a new module" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
10. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: document all system parts?
- What project evidence is needed before planning: use microservices?
- What project evidence is needed before planning: orchestrator to make documentation?
- What project evidence is needed before planning: skillsets?
- What project evidence is needed before planning: create based on design system theme?
- What project evidence is needed before planning: orchestrate applying the style on the system if that is not a new module?

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

---

## 05. Implement whole system from explanation

Промпт:

```text
User wants to implement the whole system from scratch, and a design is not covering what he needs. Generate system by user explanation, make a design theme based on what he already has, make architecture, skills, implementation plan, and implement the plan.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
User wants to implement the whole system from scratch, and a design is not covering what he needs. Generate system by user explanation, make a design theme based on what he already has, make architecture, skills, implementation plan, and im...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "a design is not covering what he needs. Generate system by user explanation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "make a design theme based on what he already has" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "make architecture" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "skills" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "implementation plan" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "implement the plan" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
10. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: a design is not covering what he needs. Generate system by user explanation?
- What project evidence is needed before planning: make a design theme based on what he already has?
- What project evidence is needed before planning: make architecture?
- What project evidence is needed before planning: skills?
- What project evidence is needed before planning: implementation plan?
- What project evidence is needed before planning: implement the plan?

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

---

## 06. Client app with AI diet generation

Промпт:

```text
User wants to create an app that interacts with other users. Clients register and fill a form, form data is saved, AI diet generation plan flow applies several AI models using user data, analyzes all models, runs several rounds if required, generates a diet plan, then each day gathers what client ate through WhatsApp, calculates calories and supplements, suggests adjustments, and sends daily feedback by push and WhatsApp.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
User wants to create an app that interacts with other users. Clients register and fill a form, form data is saved, AI diet generation plan flow applies several AI models using user data, analyzes all models, runs several rounds if required,...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "fill a form" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "form data is saved" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "AI diet generation plan flow applies several AI models using user data" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "analyzes all models" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "runs several rounds if required" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "generates a diet plan" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "each day gathers what client ate through WhatsApp" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "calculates calories" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "supplements" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "suggests adjustments" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "sends daily feedback by push" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "WhatsApp" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
16. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: fill a form?
- What project evidence is needed before planning: form data is saved?
- What project evidence is needed before planning: AI diet generation plan flow applies several AI models using user data?
- What project evidence is needed before planning: analyzes all models?
- What project evidence is needed before planning: runs several rounds if required?
- What project evidence is needed before planning: generates a diet plan?
- What project evidence is needed before planning: each day gathers what client ate through WhatsApp?
- What project evidence is needed before planning: calculates calories?
- What project evidence is needed before planning: supplements?
- What project evidence is needed before planning: suggests adjustments?
- What project evidence is needed before planning: sends daily feedback by push?
- What project evidence is needed before planning: WhatsApp?

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

---

## 07. Content generation pipeline

Промпт:

```text
User wants XIIGen to orchestrate a content generation pipeline: intake text or audio, convert audio to text, improve text, create image plan, video plan, music plan, voice/avatar generation, package results for multiple platforms, create descriptions/captions, schedule publishing, store prompts/assets/feedback, and support user revisions.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
User wants XIIGen to orchestrate a content generation pipeline: intake text or audio, convert audio to text, improve text, create image plan, video plan, music plan, voice/avatar generation, package results for multiple platforms, create de...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "intake text or audio" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "convert audio to text" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "improve text" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "create image plan" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "video plan" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "music plan" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "voice/avatar generation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "package results for multiple platforms" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "create descriptions/captions" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "schedule publishing" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "store prompts/assets/feedback" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "support user revisions" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
16. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: intake text or audio?
- What project evidence is needed before planning: convert audio to text?
- What project evidence is needed before planning: improve text?
- What project evidence is needed before planning: create image plan?
- What project evidence is needed before planning: video plan?
- What project evidence is needed before planning: music plan?
- What project evidence is needed before planning: voice/avatar generation?
- What project evidence is needed before planning: package results for multiple platforms?
- What project evidence is needed before planning: create descriptions/captions?
- What project evidence is needed before planning: schedule publishing?
- What project evidence is needed before planning: store prompts/assets/feedback?
- What project evidence is needed before planning: support user revisions?

Что нельзя замазывать:
- External input required before final implementation detail: intake text or audio.
- External input required before final implementation detail: convert audio to text.
- External input required before final implementation detail: package results for multiple platforms.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- MissingEvidenceAbstentionUnit

---

## 08. Design pattern skill library

Промпт:

```text
There are different design patterns explanations. User needs a skill for each one of them: when it is good to use, how to use it, for each language, common combinations.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
There are different design patterns explanations. User needs a skill for each one of them: when it is good to use, how to use it, for each language, common combinations.

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "There are different design patterns explanations. User needs a skill for each one of them" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "when it is good to use" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "how to use it" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "for each language" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "common combinations" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
9. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: There are different design patterns explanations. User needs a skill for each one of them?
- What project evidence is needed before planning: when it is good to use?
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

---

## 09. UML automations entity relations

Промпт:

```text
Our model needs to support advanced scenarios like described in possible umls.drawio.xml, both to document and validate the full cycle, including automations and entity relations.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Our model needs to support advanced scenarios like described in possible umls.drawio.xml, both to document and validate the full cycle, including automations and entity relations.

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Our model needs to support advanced scenarios like described in possible umls.drawio.xml" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "both to document" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "validate the full cycle" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "including automations" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "entity relations" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
9. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Our model needs to support advanced scenarios like described in possible umls.drawio.xml?
- What project evidence is needed before planning: both to document?
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

---

## 10. Use project base flow examples

Промпт:

```text
project base.drawio.xml has example flow for each scenarios 1 through 5. The model must match examples to scenarios, choose the relevant example, adapt it only after evidence check, not copy blindly, and show differences from the current project.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
project base.drawio.xml has example flow for each scenarios 1 through 5. The model must match examples to scenarios, choose the relevant example, adapt it only after evidence check, not copy blindly, and show differences from the current pr...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "project base.drawio.xml has example flow for each scenarios 1 through 5. The model must match examples to scenarios" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "choose the relevant example" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "adapt it only after evidence check" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "not copy blindly" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "show differences from the current project" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
9. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: project base.drawio.xml has example flow for each scenarios 1 through 5. The model must match examples to scenarios?
- What project evidence is needed before planning: choose the relevant example?
- What project evidence is needed before planning: adapt it only after evidence check?
- What project evidence is needed before planning: not copy blindly?
- What project evidence is needed before planning: show differences from the current project?

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

---

## 11. Generate deploy debug full system from spec

Промпт:

```text
Our model needs to be able to generate, deploy and debug a system like Small Business Networking App - Functional Specifications and Architecture.docx.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Our model needs to be able to generate, deploy and debug a system like Small Business Networking App - Functional Specifications and Architecture.docx.

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Our model needs to be able to generate" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "deploy" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "debug a system like Small Business Networking App - Functional Specifications" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "Architecture.docx" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
8. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Our model needs to be able to generate?
- What project evidence is needed before planning: deploy?
- What project evidence is needed before planning: debug a system like Small Business Networking App - Functional Specifications?
- What project evidence is needed before planning: Architecture.docx?

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

---

## 12. Dynamic code examples and AI microservices

Промпт:

```text
Dynamic code examples from code skills zip genie must be used to generate dynamic code skills, processing, and approach to code. AI microservices must get prompts from previous jobs, show orchestrated flow on UI, debug each node execution, generate code, start microservice with new internal code, test it, and integrate it into the flow.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Dynamic code examples from code skills zip genie must be used to generate dynamic code skills, processing, and approach to code. AI microservices must get prompts from previous jobs, show orchestrated flow on UI, debug each node execution, ...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Dynamic code examples from code skills zip genie must be used to generate dynamic code skills" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "processing" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "approach to code. AI microservices must get prompts from previous jobs" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "show orchestrated flow on UI" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "debug each node execution" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "generate code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "start microservice with new internal code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "test it" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "integrate it into the flow" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
13. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Dynamic code examples from code skills zip genie must be used to generate dynamic code skills?
- What project evidence is needed before planning: processing?
- What project evidence is needed before planning: approach to code. AI microservices must get prompts from previous jobs?
- What project evidence is needed before planning: show orchestrated flow on UI?
- What project evidence is needed before planning: debug each node execution?
- What project evidence is needed before planning: generate code?
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

---

## 13. Architecture and skill set for project

Промпт:

```text
User needs a plan for architecture and skill set required for the project. Examine attached solution suggestions carefully. Prefer generic interfaces reusable for databases, queues and AI. Prefer generic code: repository gets dynamic document and saves it, gets dynamic filter and filters by non-empty values, using latest .NET 7, 8, 9 principles. Manage queue of jobs at each level, for example async call for 4 different models. Create generic microservice base with architectural capabilities.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
User needs a plan for architecture and skill set required for the project. Examine attached solution suggestions carefully. Prefer generic interfaces reusable for databases, queues and AI. Prefer generic code: repository gets dynamic docume...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "User needs a plan for architecture" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "skill set required for the project. Examine attached solution suggestions carefully. Prefer generic interfaces reusable for databases" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "queues" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "AI. Prefer generic code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "repository gets dynamic document" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "saves it" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "gets dynamic filter" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "filters by non-empty values" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "using latest .NET 7" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "9 principles. Manage queue of jobs at each level" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "for example async call for 4 different models. Create generic microservice base with architectural capabilities" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
15. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: User needs a plan for architecture?
- What project evidence is needed before planning: skill set required for the project. Examine attached solution suggestions carefully. Prefer generic interfaces reusable for databases?
- What project evidence is needed before planning: queues?
- What project evidence is needed before planning: AI. Prefer generic code?
- What project evidence is needed before planning: repository gets dynamic document?
- What project evidence is needed before planning: saves it?
- What project evidence is needed before planning: gets dynamic filter?
- What project evidence is needed before planning: filters by non-empty values?
- What project evidence is needed before planning: using latest .NET 7?
- What project evidence is needed before planning: 9 principles. Manage queue of jobs at each level?
- What project evidence is needed before planning: for example async call for 4 different models. Create generic microservice base with architectural capabilities?

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

---

## 14. Alternative stacks

Промпт:

```text
Support alternative possible ways: other queues like kafka and sqs and other open source common, server can be node.js, rust, Java, python or php so each skill needs subfolder of appropriate technology, client can be native, angular or vue so each skill needs subfolder, database fabric can be mongo, prisma, redis, Azure AI Search, Graph AI and other common models.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Support alternative possible ways: other queues like kafka and sqs and other open source common, server can be node.js, rust, Java, python or php so each skill needs subfolder of appropriate technology, client can be native, angular or vue ...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Support alternative possible ways" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "other queues like kafka" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "other open source common" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "server can be node.js" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "python or php so each skill needs subfolder of appropriate technology" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "client can be native" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "angular or vue so each skill needs subfolder" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "database fabric can be mongo" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "prisma" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "redis" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "Azure AI Search" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "Graph AI" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Для части "other common models" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
16. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
17. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Support alternative possible ways?
- What project evidence is needed before planning: other queues like kafka?
- What project evidence is needed before planning: other open source common?
- What project evidence is needed before planning: server can be node.js?
- What project evidence is needed before planning: python or php so each skill needs subfolder of appropriate technology?
- What project evidence is needed before planning: client can be native?
- What project evidence is needed before planning: angular or vue so each skill needs subfolder?
- What project evidence is needed before planning: database fabric can be mongo?
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

---

## 15. RAG and GraphRAG around AI

Промпт:

```text
Need good graph and RAG capabilities to supply AI stages with appropriate context. Each time AI is applied, make a plan with AI for which requests to apply on current RAG. At the end of each execution and feedback, plan with AI how and where to store data on RAG. RAG can be azure ai, cosmos db, graph ai, pinecone, open source, gremlin.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Need good graph and RAG capabilities to supply AI stages with appropriate context. Each time AI is applied, make a plan with AI for which requests to apply on current RAG. At the end of each execution and feedback, plan with AI how and wher...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Need good graph" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "RAG capabilities to supply AI stages with appropriate context. Each time AI is applied" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "make a plan with AI for which requests to apply on current RAG. At the end of each execution" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "feedback" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "plan with AI how" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "where to store data on RAG. RAG can be azure ai" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "cosmos db" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "graph ai" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "pinecone" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "open source" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "gremlin" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
15. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Need good graph?
- What project evidence is needed before planning: RAG capabilities to supply AI stages with appropriate context. Each time AI is applied?
- What project evidence is needed before planning: make a plan with AI for which requests to apply on current RAG. At the end of each execution?
- What project evidence is needed before planning: feedback?
- What project evidence is needed before planning: plan with AI how?
- What project evidence is needed before planning: where to store data on RAG. RAG can be azure ai?
- What project evidence is needed before planning: cosmos db?
- What project evidence is needed before planning: graph ai?
- What project evidence is needed before planning: pinecone?
- What project evidence is needed before planning: open source?
- What project evidence is needed before planning: gremlin?

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

---

## 16. Advanced skills library

Промпт:

```text
Advanced skills: creating prompts to implement phases, unit testing high coverage, e2e testing, UI testing components and cases, DevOps integration with hosting and CI/CD, documentation, Swagger, MCP access to APIs, logging including UI and exception middleware, safe code with key vaults and vulnerability checks, optimization and resource management.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Advanced skills: creating prompts to implement phases, unit testing high coverage, e2e testing, UI testing components and cases, DevOps integration with hosting and CI/CD, documentation, Swagger, MCP access to APIs, logging including UI and...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Advanced skills" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "creating prompts to implement phases" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "unit testing high coverage" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "e2e testing" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "UI testing components" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "cases" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "DevOps integration with hosting" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "CI/CD" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "documentation" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "Swagger" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "MCP access to APIs" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "logging including UI" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Для части "exception middleware" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
16. Для части "safe code with key vaults" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
17. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
18. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Advanced skills?
- What project evidence is needed before planning: creating prompts to implement phases?
- What project evidence is needed before planning: unit testing high coverage?
- What project evidence is needed before planning: e2e testing?
- What project evidence is needed before planning: UI testing components?
- What project evidence is needed before planning: cases?
- What project evidence is needed before planning: DevOps integration with hosting?
- What project evidence is needed before planning: CI/CD?
- What project evidence is needed before planning: documentation?
- What project evidence is needed before planning: Swagger?
- What project evidence is needed before planning: MCP access to APIs?
- What project evidence is needed before planning: logging including UI?
- What project evidence is needed before planning: exception middleware?
- What project evidence is needed before planning: safe code with key vaults?

Что нельзя замазывать:
- External input required before final implementation detail: DevOps integration with hosting.

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

---

## 17. GraphRAG alternatives

Промпт:

```text
Advanced RAG alternatives: compare Neo4j GraphRAG, LightRAG, nano-graphrag, LangChain KG RAG, LlamaIndex GraphRAG, Graphlit, Memgraph, Neptune plus Bedrock, Contextual AI, and HybridRAG DIY; explain which alternative fits and why.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Advanced RAG alternatives: compare Neo4j GraphRAG, LightRAG, nano-graphrag, LangChain KG RAG, LlamaIndex GraphRAG, Graphlit, Memgraph, Neptune plus Bedrock, Contextual AI, and HybridRAG DIY; explain which alternative fits and why.

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Advanced RAG alternatives" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "compare Neo4j GraphRAG" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "LightRAG" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "nano-graphrag" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "LangChain KG RAG" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "LlamaIndex GraphRAG" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "Graphlit" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "Memgraph" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "Neptune plus Bedrock" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "Contextual AI" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "HybridRAG DIY" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "explain which alternative fits" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
16. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Advanced RAG alternatives?
- What project evidence is needed before planning: compare Neo4j GraphRAG?
- What project evidence is needed before planning: LightRAG?
- What project evidence is needed before planning: nano-graphrag?
- What project evidence is needed before planning: LangChain KG RAG?
- What project evidence is needed before planning: LlamaIndex GraphRAG?
- What project evidence is needed before planning: Graphlit?
- What project evidence is needed before planning: Memgraph?
- What project evidence is needed before planning: Neptune plus Bedrock?
- What project evidence is needed before planning: Contextual AI?
- What project evidence is needed before planning: HybridRAG DIY?
- What project evidence is needed before planning: explain which alternative fits?

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

---

## 18. Advanced DevOps hosting alternatives

Промпт:

```text
Advanced devops skills: Kubernetes hosting alternatives including Railway, Render, Heroku replacement, Fly.io, Vercel, GKE, EKS, AKS, DigitalOcean, Nomad, Docker Swarm, K3s, MicroK8s, OpenShift, Rancher; choose hosting path for prototype, production, global service, and enterprise.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
Advanced devops skills: Kubernetes hosting alternatives including Railway, Render, Heroku replacement, Fly.io, Vercel, GKE, EKS, AKS, DigitalOcean, Nomad, Docker Swarm, K3s, MicroK8s, OpenShift, Rancher; choose hosting path for prototype, p...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "Advanced devops skills" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "Kubernetes hosting alternatives including Railway" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "Render" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "Heroku replacement" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "Fly.io" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "Vercel" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "DigitalOcean" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "Nomad" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "Docker Swarm" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "MicroK8s" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "OpenShift" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "Rancher" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Для части "choose hosting path for prototype" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
16. Для части "production" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
17. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
18. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: Advanced devops skills?
- What project evidence is needed before planning: Kubernetes hosting alternatives including Railway?
- What project evidence is needed before planning: Render?
- What project evidence is needed before planning: Heroku replacement?
- What project evidence is needed before planning: Fly.io?
- What project evidence is needed before planning: Vercel?
- What project evidence is needed before planning: DigitalOcean?
- What project evidence is needed before planning: Nomad?
- What project evidence is needed before planning: Docker Swarm?
- What project evidence is needed before planning: MicroK8s?
- What project evidence is needed before planning: OpenShift?
- What project evidence is needed before planning: Rancher?
- What project evidence is needed before planning: choose hosting path for prototype?
- What project evidence is needed before planning: production?

Что нельзя замазывать:
- External input required before final implementation detail: Kubernetes hosting alternatives including Railway.
- External input required before final implementation detail: choose hosting path for prototype.

Внутри XIIGen для этого используются:
- TaskOrchestrationUnit
- ProjectUnderstandingUnit
- EvidencePlanner
- UnitSelector
- HumanRouteComposer
- VerificationAbstentionUnit
- CrossStackBusinessFlowGraphUnit
- MissingEvidenceAbstentionUnit

---

## 19. Deliver reusable AI skill library and phased prompts

Промпт:

```text
User needs skill sets organized in a library different AI can use: copilot, claude ai, claude code, cursor. User needs prompts to implement the system ordered by phases and a chat user can interact with. First make no-code explanation divided into phases, validate plan covers all requirements, make positive and negative examples, each phase short enough to save state and recover, make small RAG or skills factory if needed, do not reset files, check what already exists, make current work available, and save state.
```

Status: ORCHESTRATED

Конечный ответ XIIGen:

Я понял задачу так:
User needs skill sets organized in a library different AI can use: copilot, claude ai, claude code, cursor. User needs prompts to implement the system ordered by phases and a chat user can interact with. First make no-code explanation divid...

Что я вижу в проекте:
- GenieDynamicApp: JavaScript/TypeScript package evidence found.
- GenieDynamicServer: .NET solution/project evidence found.
- GenieDynamicApp: CI workflow evidence found.
- GenieDynamicServer: backend controller evidence found.
- GenieDynamicServer: service layer evidence found.

Как вести работу:
1. Сначала подтвердить текущую систему: active repo, linked repos, UI/backend/storage/flow surfaces.
2. Разложить пользовательскую задачу на рабочие части и для каждой части проверить reuse before rewrite.
3. Для части "User needs skill sets organized in a library different AI can use" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
4. Для части "copilot" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
5. Для части "claude ai" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
6. Для части "claude code" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
7. Для части "cursor. User needs prompts to implement the system ordered by phases" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
8. Для части "a chat user can interact with. First make no-code explanation divided into phases" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
9. Для части "validate plan covers all requirements" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
10. Для части "make positive" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
11. Для части "negative examples" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
12. Для части "each phase short enough to save state" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
13. Для части "recover" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
14. Для части "make small RAG or skills factory if needed" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
15. Для части "do not reset files" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
16. Для части "check what already exists" найти evidence в проекте, определить owner surface, нужные inputs, expected output и риск missing evidence.
17. Собрать общий маршрут: что делаем сначала, что зависит от evidence, где нужен feedback loop, что можно отдать executor-у.
18. Перед реализацией проверить, что ответ связан с реальным кодом, а не является generic планом.

Какие вопросы XIIGen должен задать project wiki/RAG:
- What existing screens, routes, services, flows, jobs, storage, and deployment surfaces already exist in the active and linked projects?
- Which requested work items can be grounded in code evidence, and which require attached external input or user decision?
- Where can the system reuse existing project structure before planning new implementation?
- What project evidence is needed before planning: User needs skill sets organized in a library different AI can use?
- What project evidence is needed before planning: copilot?
- What project evidence is needed before planning: claude ai?
- What project evidence is needed before planning: claude code?
- What project evidence is needed before planning: cursor. User needs prompts to implement the system ordered by phases?
- What project evidence is needed before planning: a chat user can interact with. First make no-code explanation divided into phases?
- What project evidence is needed before planning: validate plan covers all requirements?
- What project evidence is needed before planning: make positive?
- What project evidence is needed before planning: negative examples?
- What project evidence is needed before planning: each phase short enough to save state?
- What project evidence is needed before planning: recover?
- What project evidence is needed before planning: make small RAG or skills factory if needed?
- What project evidence is needed before planning: do not reset files?
- What project evidence is needed before planning: check what already exists?

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

---

