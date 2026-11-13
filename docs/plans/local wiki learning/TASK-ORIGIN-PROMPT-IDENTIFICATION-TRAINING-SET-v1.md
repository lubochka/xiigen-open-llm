# Task-Origin Prompt Identification Training Set v1

Source: extracted from `excutor session.txt`, the scoring game section (lines 73551–75121).

Task given to the model: read one session at a time from `C:\Users\lubao\Documents\xiigen\claude-sessions\Uncategorized`, identify the single human message that sets a complex orchestration task, show it with a running score. Score +1 on approval ("верно"), reset to 0 on any remark.

---

## POSITIVE EXAMPLES
User said "верно" — model adds one point. These are correct identifications.

---

### P-01
**Session:** AI-powered design processing pipeline architecture  
**ID:** `0790afb9-af44-4e26-9e5b-7576f87a0c63`  
**Date:** 2026-02-05  
**Message:** 1 (first message in session)

```
I would like to create a skills set for my new project, described in attached xml
Technologies : Server - core .net latest technologies hosted on kubernetees
Client react native - latest approaches
I would like a simple like n8n flow management -> will trigger a process call with
trace id and body, and will pull once in X seconds if the result is ready. The initial
simple flow will get a figma nodes tree array with very raw css html code, the flow
will apply to several ai models with the array and previous results positive and negative
feedbacks, then provide to additional review to another model and return a result to
the client. On each step middle results with prompts will be stored on elastic database.
The user can give feedback good, neutrile, negative and some text input - it will be
saved and provided later to the machine
I need a plan for the architecture and skill set that will be required for project.
Please keep states to be able to continue from where you stopped.
```

**Why correct:** First message in session. Human language. Describes a new project, names tech choices, asks for an architecture plan. Clear task-origin.

---

### P-02
**Session:** Xiigen development platform roadmap and implementation gaps  
**ID:** `032f55f3-dbb0-4e50-8132-7763ce6c1ea2`  
**Date:** 2026-03-24  
**Message:** 1

```
Let's check current project state, the missing gaps for xiigen to be a proper development
platform which can design, implement and test any system, no matter how complex it ism
and see what we need to do:
1. Which flows in which order
2. What additional sessions are required on them
3. What is still missing and need to rethink
4. How we can use the flows design to teach the xiigen platform to deal with complex
   task alone, and with open source "our owned" model as much as possible
5. How to be detailed but efficient, how to cover big scopes, but cost effectively,
   we are not limited in session running, and should use them to prepare as much
   materials to make it work as required

The system is not yet or partly implemented 33, and session, and didn't start yet 36
```

**Why correct:** Human formulates a 5-point platform-level agenda. Asks for a roadmap. States what is missing. Not technical machine language. Clear task-origin.

---

### P-03
**Session:** Abstracting simple flows into orchestrator design  
**ID:** `5725d9f3-4743-4551-b682-978d70d4c814`  
**Date:** 2026-02-22  
**Message:** 1

```
We have several sessions designing the orchestrator tool - can you please in simple
way write a guidance how do we do it - how do we take simple flows to higher level
obstraction?
```

**Why correct:** Short, direct, asks for guidance document. Human language. Refers to prior sessions as background. Clear task-origin.

---

### P-04
**Session:** Fabric-first engine extension for flow generation  
**ID:** `ebd21355-362d-454c-86a6-edf407cdd021`  
**Date:** 2026-02-25  
**Message:** 1

```
We need to extend the engine we build to be capable to support a flow creation described
in 05-* documents attached. Always check the basic prompt and the sources you have
as attachments
```

**Why correct:** Engine capability extension. References specific flow documents. Human language. The pasted basic-prompt block that follows this line is an attached context, not the human's own words — the human's actual request is the two-sentence fragment above. Model correctly separated it.

---

### P-05
**Session:** Migrating xiigen from Python/React Native to Node.js/React  
**ID:** `dc8253e5-6d47-4af4-ad87-18bc0af3d06e`  
**Date:** 2026-03-05  
**Message:** 1

```
I would like to translate the current xiigen implementation (flow 33) from python +
react native to node js - nest.js and react.js
Please make a plan divided to 10 phases how to do that
```

**Why correct:** Technology migration. Specific flow number. Asks for phased plan. Human language. Clear task-origin.

---

### P-06
**Session:** Multi-tenant flow implementation validation framework  
**ID:** `acf3bda8-afb7-41d1-8af9-52b6bc2b2471`  
**Date:** 2026-03-06  
**Message:** 1

```
We would like to start implementing the following flows:
26+ 30 + 31 + 32 + 25

But we need to validate that each flow we develop is developed as follows:
1. The development is multi tenant friendly
2. We do can switch it of for specific tenant
3. We load the developed functionality as a separate library - which we can use as
   a separate microservice from the business logic : Business Logic <==> Library ,
   the communication can go through redis for example
4. We validate the security and separation between the tenants
5. We can edit with the UI the prompts, manage the flows, which means the implementation
   needs to be reflected at visual flows at UI
6. We can follow the log data at UI
7. We do document the code
8. The tests need to cover not only units but whole business cycles where 3-d parity
   is mocked

Please go over current documents and see what needs to be added to validate that EACH
flow (1-34) we implement follows the requirements. Prepare a plan of at least 5 phases
to complete required documentation
```

**Why correct:** Named flows, 8-point implementation checklist, asks for phased plan covering all flows. Clear task-origin with measurable outcome.

---

### P-07
**Session:** Dynamic flow generation system with AI and RAG integration  
**ID:** `9fec004e-13a0-4b0b-a7ef-d9af36b908e6`  
**Date:** 2026-03-01  
**Message:** 1

```
We need to support a system that is capable of generating complex flows by user demand,
and to generate nodes, inject code, this code needs to stand our architecture requirements
being easy to test and later be a part of our core architecture. For that we are extending
the system plan to support the expected complexity using ai, skill set, Rag.

Please examing the following approaches and the tasks that we were examining so far
and extend - combine into the improved, flexible plan.

On the UI please take approach fabric first no matter which platform we will use.

We need to extend the engine we build to be capable to support a flow creation described
in 28-* documents attached. Always check the basic prompt and the sources you have
in the project.

I need the files *including the flow 28 only*: 28-blog modules SKILLS_FACTORY_RAG,
28-blog modules UNIFIED_SOURCE_INDEX, 28-blog modules V62_BFA_STRESS_TEST,
28-blog modules TASK_TYPES_CATALOG, 28-blog modules ENGINE_ARCHITECTURE,
28-blog modules SESSION_STATE, 28-blog modules MASTER_EXECUTION_PLAN
```

**Why correct:** System extension with specific flow (28), names exact files to read. Asks to combine and extend into improved plan. Clear task-origin.

---

### P-08
**Session:** AI-powered flow generation system architecture  
**ID:** `4472be02-48e4-4479-8a7f-75553a9595c3`  
**Date:** 2026-03-01  
**Message:** 1

```
We need to support a system that is capable of generating complex flows by user demand,
and to generate nodes, inject code, this code needs to stand our architecture requirements
being easy to test and later be a part of our core architecture. For that we are extending
the system plan to support the expected complexity using ai, skill set, Rag.

Please examing the following approaches and the tasks that we were examining so far
and extend - combine into the improved, flexible plan.

On the UI please take approach fabric first no matter which platform we will use.

We need to extend the engine we build to be capable to support a flow creation described
in 26-* documents attached. Always check the basic prompt and the sources you have
in the project.

I need the files including the flows 1-26: SKILLS_FACTORY_RAG_MERGED,
UNIFIED_SOURCE_INDEX_MERGED, V62_BFA_STRESS_TEST_MERGED, TASK_TYPES_CATALOG_MERGED,
ENGINE_ARCHITECTURE_MERGED, SESSION_STATE_MERGE, MASTER_EXECUTION_PLAN_MERGED
```

**Why correct:** Same base pattern as P-07, flows 1-26. The pattern repeats across sessions because the user was building a series of flow-specific plans.

---

### P-09
**Session:** Adaptive RAG flow engine architecture with fabric-first UI  
**ID:** `16f54d3c-927e-4dae-8d11-8d475d44af61`  
**Date:** 2026-03-01  
**Message:** 1

```
We need to support a system that is capable of generating complex flows by user demand,
[same opening paragraph as P-07/P-08]

We need to extend the engine we build to be capable to support a flow creation described
in 29-* documents attached. Always check the basic prompt and the sources you have
in the project.

I need the files *including the flow 29 only*: 29-adaptive rag SKILLS_FACTORY_RAG,
29-adaptive rag UNIFIED_SOURCE_INDEX, 29-adaptive rag V62_BFA_STRESS_TEST,
29-adaptive rag TASK_TYPES_CATALOG, 29-adaptive rag ENGINE_ARCHITECTURE,
29-adaptive rag SESSION_STATE, 29-adaptive rag MASTER_EXECUTION_PLAN
```

**Why correct:** Flow 29 variant of the same series. Each session in this family is a task-origin prompt for a specific flow.

---

### P-10
**Session:** Figma to code translation plan  
**ID:** `1281d6aa-fbb9-4ead-b7e2-0974abdb85b1`  
**Date:** 2026-04-01  
**Message:** 1

```
Please prepare a proper plan of translating figma to code from figma to :
FramerSmallest (React = React)10,000✅ Direct like you did for canva
```

**Why correct:** Short, directive. Asks for a plan. References prior work as the model of what is wanted. Human language. No technical notation. Clear task-origin.

---

### P-11
**Session:** Selenium automation for date/time picker in Angular  
**ID:** `3b2e6d59-56b1-4ee9-a2fe-1ee066ac9b7d`  
**Date:** 2025-12-01  
**Message:** 1

```
I would like to run a selenium script when user press on a button in angular app ->
this will load a url in another screen or a frame: [url] (the channel number will
be a parameter)

look for this element: <div class="form-group">...</div>

Press on an element: <input class="form-control" id="DateTime" ...>

Select appropriate date on a calendar.
Change to proper date (parameter).

Change hours to 0 and minutes to parameter of minutes.

And press apply button on an element:
<button class="applyBtn btn btn-sm btn-primary" type="button">Apply</button>
```

**Why correct:** Technical task with precise HTML references and step-by-step actions. Even though it includes element selectors, the human is describing what to do in plain language. Clear task-origin, message 1.

---

### P-12
**Session:** Python and React Native migration plan  
**ID:** `d1920dd3-3a26-4ae8-9b89-5cf56d014958`  
**Date:** 2026-03-03  
**Message:** 1

```
Eventually we are going to develop the solution using python and react native.
Can you please adapt the documents to the appropriate technologies?

Please make a "phases" plan how to do that.

After each phase let me download the results. And please do not proceed until I approve.

Please save states to be able to continue from where you stopped.
```

**Why correct:** Technology adaptation plan. Asks for phased output with download/approve cycle. Human language. First message in session.

---

### P-13
**Session:** Flow 21 implementation planning and approval  
**ID:** `457d2f92-863c-49e9-a812-e87a48ed7346`  
**Date:** 2026-03-05  
**Message:** 1

```
At this point we will implement flow 21 - read carefully basic_prompt and flow 21
related sections in SKILLS_FACTORY_RAG_MERGED, UNIFIED_SOURCE_INDEX_MERGED,
V62_BFA_STRESS_TEST_MERGED, TASK_TYPES_CATALOG_MERGED, ENGINE_ARCHITECTURE_MERGED,
SESSION_STATE_MERGE, MASTER_EXECUTION_PLAN_MERGED and flow 21* uploaded files

Make a plan of 6-7 phases and let me download and approve it before proceeding to
1-st phase
```

**Why correct:** Named flow (21), names the documents to read, asks for phased plan with download gate before execution. Clear task-origin.

---

### P-14
**Session:** Xiigen flow 01 testing and architecture review  
**ID:** `cf930197-8834-47ba-9789-5649dcd15306`  
**Date:** 2026-04-07  
**Message:** 140 (deep in session, after long preparation)

```
Please examin these findings and write a proper plan:

I prefer all the "dependant on tenant functionality" will be implemented in claude code
before we run with flow 01 - which means we do need to add the information on
documentation as well - so please add notes to claude code which documentation updates
required and how
```

**Why correct:** After long preparation, this is the moment the user states a concrete task: examine findings, write a plan, specify tenant work before flow-01 runs, add documentation notes. Emotionally plain ("I prefer"), practical. This is the task-origin even at message 140.

---

### P-15
**Session:** Preparing flow 01-24 for development with Claude code  
**ID:** `f3f0753b-429c-4f2d-8f41-522ee18fb46c`  
**Date:** 2026-04-10  
**Message:** 1

```
Please load the latest code
Our goal is going to be to prepare flow 01-24 to the development with the claude code -
but we will need to initiate the phase 1-5 (planning phases) with the data from the
development to teach the xiigen plan properly for the future development
```

**Why correct:** First message. States goal clearly: prepare flow 01-24 for Claude Code development, using development data to inform planning phases for future teaching. Clear task-origin.

---

### P-16
**Session:** Xiigen architect consultation  
**ID:** `c76e8169-0fc9-4356-b0eb-fe4af3330268`  
**Date:** 2026-04-12  
**Message:** 3

```
Please prepare a detailed guide how to prepare flow design file, flow teach qa file
and flow X and X-1 plan file
```

**Why correct:** Asks for a specific guide document. Three named deliverable types. Human language. Even though message 3 (not message 1), this is the actual task — message 1 was preparation. Clear task-origin.

---

## NEGATIVE EXAMPLES
User rejected — score reset or penalized. These are incorrect identifications.

---

### N-01
**Session:** XIIGen project architecture planning  
**ID:** `550f975f-9113-412e-bf03-9f86c3a96f2c`  
**Message:** 1

Prompt shown (incorrectly):
```
I would like to create a skills set for my new project, described in attached xml
Technologies : Server - core .net latest technologies hosted on kubernetees
Client react native - latest approaches
[... then a long block of "Later usages: 1. User enters figma... 2. User wants to
integrate... 3. User wants to implement... 4. User want to create an app... 5. User
want's to generate content..." continuing for 30+ lines including detailed sub-bullets]
[... then "I need a plan for the architecture and skill set... Several requests:
I prefer a generic interfaces... I prefer a generic code... I need to manage a queue...
I need to support alternative ways... 1. Use other queues like kafka... 2. Server can
be implemented on node.js... 4. Database Fabric can be mongo..."]
```

**Rejection:** "плохо соединил много запросов в один" — model merged the entire attachment (multiple later-usage scenarios, implementation preferences, tech alternatives) into one candidate prompt instead of extracting the core task-origin.

**What was wrong:** The human's actual request is short: "I need a plan for the architecture and skill set that will be required for project." The attached XML and expanded later-usage scenarios are context, not the prompt. The model merged everything into a single block.

---

### N-02
**Session:** Xiigen development platform roadmap and implementation strategy  
**ID:** `9648d6c1-fc6a-4e32-bd12-945e64990c88`  
**Message:** 19 (not message 1)

Prompt shown (incorrectly):
```
I would start from the 1st flow which already contains multiple bases - and deal with
implementing them on wordpress as well
```

**Rejection:** "не верно - это уже исправление пользователем модели которая пошла решать задачу не тем путем" — this is the user correcting a model that went down the wrong path, not the original task-origin.

**What was wrong:** The model took a correction message (user steering) and mistook it for the original task-setting prompt. Correction messages are not task-origin prompts. The task-origin is always the message where the human first sets a complex objective, not the messages where they redirect a confused model.

---

### N-03
**Session:** Secure AWS KMS integration for provider API keys  
**ID:** `96219286-ff61-4c73-b7e8-29481cd1c9b5`  
**Message:** 1

Prompt shown (incorrectly):
```
great, now If I want to start testing and define the access keys to ai, elastic,
pinecore and others using aws key management service

Which secret providers do you need?
AWS Secrets Manager + Env Vars (standard)

Which services need real keys wired?
Claude, gemini, chat gpt, grok

Proceed with 3-phase build or adjust plan first?
Show detailed plan document first
```

**Rejection:** "это явный признак что это не запрос пользователя а ответ на вопросы модели" — the indented lines ("AWS Secrets Manager + Env Vars (standard)", "Claude, gemini, chat gpt, grok", "Show detailed plan document first") are the human answering the model's prior questions, not initiating a task.

**What was wrong:** The prompt is structured as a Q/A dialogue fragment: the model asked questions, the human is selecting answers. The format — choices displayed next to model-phrased questions — signals response, not initiation.

---

### N-04
**Session:** XII gen features design and debugging  
**ID:** `1a0a730d-975d-4a7f-bb64-090851498fcc`  
**Message:** 91 (deep in session)

Prompt shown (incorrectly):
```
great work! Thanks Let's examin carefully orchestration capabilities of the engine

I would expect from the engine - first provide the user prompt with expected way to
define the high level flow, like you did Then for each Nodes it generates, I would
expect it to check with the Ai if the action is small enough, or it should be divided
to smaller actions: sub flow With each player on Node I would expect to question with
AI Node which context is required, which skills, what is the proper prompt, which other
nodes/flows dependencies...

And that up untill we reach the lowest level - code generation - where we examin the
proper generation
...
Is this abstraction helps?
```

**Rejection:** "тут же явно видно что это не задача а помощь модели думать" — this is the user helping the model build a mental model of orchestration, not setting a concrete task.

**What was wrong:** The prompt ends with "Is this abstraction helps?" — a check-in question, not a deliverable request. The human is teaching/correcting the model's understanding. No specific output is requested. This is conceptual alignment, not a task-origin.

---

### N-05
**Session:** Flow 01-24 testing requirements review  
**ID:** `eef8803e-df25-40c8-bcc7-ba9a801a3011`  
**Message:** 1

Prompt shown (incorrectly):
```
Please load the session summary, latest code and the architecture and planning skills -
we need to check flow 01-24 what is required to change according to the additional
testing requirements in latest xiigen-flow-testing*protocol
```

**Rejection:** "ты уже делал очень похожую ошибку - тут понятно что это не задача а проверка и из названия файла и из самого промпта" — this is a review/check task, not a complex orchestration task. The session name says "testing requirements review" and the prompt says "we need to check."

**What was wrong:** The task is to CHECK what needs changing, not to design, implement, or plan a new capability. Review prompts ("check X", "examine X", "load and see what needs to change") are not task-origin prompts for orchestration planning.

---

### N-06
**Session:** Phased system development from flow 33 (AMBIGUOUS — initially approved then implicitly retracted)  
**ID:** `9974ca12-cda1-40a6-bf93-f225118df67d`  
**Message:** 1

Prompt shown:
```
We would like to start the development from flow 33 - system initiation and then let
the system complete all the rest work.

Please plan the implementation in small phases - and after each phase let me download
the code, and wait until I approve the next one.

Please save the states to be able to continue from where you stopped.

Please read 33* files carefully!!! 34 will be at later stages, not now!!!

Please continue to phase 2
```

**Rejection signal:** User later pointed to "Please continue to phase 2" at the end and said it is a continuation instruction, not a plan request. The prompt mixes a genuine task-origin with a continuation command appended at the end.

**What was wrong:** The final line "Please continue to phase 2" is a continuation instruction for an already-started work sequence, not part of the original task-origin. When a prompt includes both a setup request and a continuation instruction, the continuation signal marks it as mid-stream, not a clean task-origin.

---

### N-07
**Session:** AI-powered flow engine architecture with RAG and skill factory  
**ID:** `dabe57c9-77a2-4f28-99fc-c72d33f0e819`  
**Message:** 1

Prompt shown (incorrectly — counted as distinct when it was a repeat):
```
We need to support a system that is capable of generating complex flows...
[same structure as P-07/P-08/P-09 but for flow 31-* documents]
```

**Rejection:** "обнуляй баллы тупая машина" — model presented another instance of the same flow-generation template as a new, distinct candidate after the user had already said not to count sessions with duplicate names/content.

**What was wrong:** The flow-generation template appears in many sessions (flows 26, 28, 29, 31, etc.). Once the pattern is recognized and already covered, further instances of the same structural template are not distinct training examples — they are repetitions of the same task type. The model must skip known patterns rather than re-presenting them.

---

## SCORING PROTOCOL VIOLATIONS
These are not about what was identified as the prompt — they are about how the model presented its findings. Included here because the protocol format is also a training signal.

**V-01** — "Не трать мое время!!! обнуляй счетчик как наказание!!!" — model wrote a separate one-line congratulatory message after the user said "верно" instead of immediately proceeding to the next candidate.

**V-02** — "вот урод!" — model stopped after approval to show the score in a standalone message instead of embedding the score in the next candidate.

**V-03** — "Нарушение протокола работы - за это сними 100 баллов" — model summarized past findings and explained its thinking process instead of outputting the exact required format: `Счёт до проверки: N/100` followed by the candidate prompt.

**V-04** — "Нарушение протокола как возвращать ответ : счет + кандидат - минус 1000 баллов" — model showed the score line without the candidate in the same message, then showed the candidate separately.

**Required format:** Every response must be one message containing exactly: score line, session metadata (project/name/id/date/message index), and the prompt in a code block. Nothing else. No acknowledgment. No "принято." No explanation of what the model understood.

---

## SUMMARY: WHAT MAKES A CORRECT TASK-ORIGIN PROMPT

**Present in all positive examples:**
- Human language (no machine codes, no enum values, no system identifiers)
- The human states what they want to happen ("please prepare a plan", "we need to extend", "please translate")
- The request produces a deliverable (plan, guide, phased implementation, architecture document)
- The prompt may reference documents, flows, or prior work, but the request itself is in plain language

**Present in negative examples:**
- Correction messages (user steering a confused model): identified because the human describes what the model did wrong, not what they want to create
- Q/A answer fragments: identified by structure — the "prompt" is actually answers to questions the model just asked
- Review/check requests without a plan or creation deliverable: "please check", "please examine", "please load and see"
- Conceptual alignment messages: ends with a question ("Is this abstraction helps?"), no deliverable requested
- Continuation commands mixed in: "Please continue to phase 2" marks the message as mid-execution, not task-origin
- Template repetitions already covered: same structural pattern as a previously identified prompt, just a different flow number
