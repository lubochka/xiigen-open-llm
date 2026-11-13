# PLAN WIKI GAP VALIDATION EXAMPLES v1

Status: REQUIRED REFERENCE.
Language: English.
Depends on: PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md
Also governed by: PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md
Purpose: For every gap that PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md defines,
this document provides: the concrete validation task that proves the gap is
closed, exact prompt examples from orchestrator_prompts.txt and real developer
work, required answer variations that must all pass, and open licensed sources
for pre-training the relevant wiki units.

Rule: a gap is not closed until all variations in its validation block pass.
One variation passing does not count.

## Source And Row Rules

These examples validate training gap closure. They do not replace the 50-row
training loop. For each gap, the executor builds as many 50-row batches as
needed, then validates the trained unit against all listed variations.

Every row built from this document must name:

```text
gap_id
unit_under_test
prompt_scenario_id
variation_id
source_authority_state
a_side
chosen
rejected
wrong_reason
expected_trace_fields
```

Accepted source states:

```text
ACCEPT: MIT, Apache-2.0, BSD, CC-BY with attribution recorded.
ACCEPT_SUPPORT_ONLY: use for validation prompts or inspiration, not training rows.
QUARANTINE: ShareAlike, unclear license, or policy risk; isolated only with explicit approval.
BLOCK: NC, GPL/AGPL for this purpose, source-available restrictions, copyright books/blogs, or terms that restrict LLM training.
```

Do not use Stack Overflow, FEVER, n8n, commercial books, company blogs, MIT OCW,
or Wikipedia as default commercial LLM training rows. If a quarantined source is
ever used, it must be isolated and explicitly approved before training.

---

## How To Read This Document

Each gap section has four parts:

1. GAP — what is missing in current training.
2. VALIDATION TASK — the specific test that proves the gap is closed.
3. PROMPT EXAMPLES — drawn from orchestrator_prompts.txt and real developer
   patterns. Every example must pass before the unit is promoted.
4. OPEN SOURCES — licensed datasets and repositories for pre-training.

---

## Gap 1: Entity Impact Reasoning

### GAP

The wiki cannot answer questions about what must change when a developer adds,
renames, or removes a field from an entity or DTO. It lists the entity but does
not trace its full impact path.

### VALIDATION TASK

Given a project with a documented entity, ask five variations of the impact
question. All five must return answers that name every touch point in the
entity impact contract (entrypoint, validator, mapper, service/calculation,
repository, serializer, test). Any answer that stops at controller+DTO fails.

Minimum passing threshold: entity_impact_path_accuracy >= 0.85 across all
five variations.

### PROMPT EXAMPLES FROM orchestrator_prompts.txt

The orchestrator prompts describe several systems. Each generates concrete
entity impact questions.

**From scenario 4 — diet app:**

The user wants a diet generation app where clients fill a registration form
and daily food intake is submitted via WhatsApp. The AI calculates calories
and supplements. This creates entities like ClientProfile, DietPlan, DailyEntry,
and Supplement.

Concrete validation prompts (all five must pass):

```
V1: "I want to add a FoodAllergyList field to ClientProfile.
     Where does this field travel through the system and what calculations
     will need to use it?"

V2: "The DailyEntry entity currently has Calories as an int. I need to
     change it to a decimal for precision. What breaks?"

V3: "I am adding a new field MealTime to DailyEntry.
     Which services read DailyEntry, and will any of them need to change?"

V4: "Where is DietPlan stored and what APIs expose it?
     If I add a PlanVersion field, what do I need to update?"

V5: "ClientProfile is submitted at registration. If I add a PhoneNumber
     field, what is the path from the form to the database?"
```

**From scenario 1 — Figma architecture generation:**

The system generates architecture from Figma screens. Entities like
FlowDefinition, ScreenComponent, and ArchitectureResult are created.

```
V6: "I want to add a confidence_score field to ScreenComponent.
     Which steps of the analysis pipeline will need to read or write it?"

V7: "FlowDefinition is generated from screen analysis. What happens to it
     after generation? Where is it persisted and who reads it?"
```

**From scenario 5 — content generation pipeline:**

Entities like ContentRequest, MediaAsset, ScheduledPost flow through the
pipeline.

```
V8: "I want to add a platform_constraints field to ContentRequest.
     The video generator and post scheduler both need to know about it.
     What is the path this field must travel?"

V9: "MediaAsset has a file_url field. I need to add a cdn_url field.
     What consumers of MediaAsset will need to be updated?"
```

### REQUIRED ANSWER SHAPE FOR ENTITY IMPACT

Every passing answer must name:

```
1. Where this entity first enters the system (controller/handler/webhook).
2. Which validators check this entity or field.
3. Which mappers/converters touch this entity.
4. Which services perform calculations using this entity.
5. Where this entity is persisted and what the storage mapping looks like.
6. Which serializers, APIs, or messages carry this entity outward.
7. Which tests cover this entity.
8. Any part of the path that is unresolved and needs manual inspection.
```

A rejected answer stops at step 2 or 3. A rejected answer says "update all
references" without naming them. A rejected answer invents a database migration
without evidence.

### OPEN SOURCES FOR PRE-TRAINING

The following open repositories and datasets contain entity impact patterns
suitable for pre-training EntityImpactMapper and WikiAnswerComposer before
testing on project code.

**GitHub repositories with rich entity flows:**

- `dotnet/eShopOnWeb` — ASP.NET Core reference application. Contains Order,
  BasketItem, CatalogItem entities flowing through controller, service,
  repository, and EF Core mapping layers. License: MIT.
  URL: https://github.com/dotnet-architecture/eShopOnWeb

- `ardalis/CleanArchitecture` — Clean architecture template with full entity
  layer separation. License: MIT.
  URL: https://github.com/ardalis/CleanArchitecture

- `JasonGT/NorthwindTraders` — Full NorthWind Traders sample with CQRS,
  entities, and domain events. License: MIT.
  URL: https://github.com/JasonGT/NorthwindTraders

- `dotnet/aspnetcore` samples directory — contains entity/DTO examples across
  multiple architectural styles. License: MIT.

**Developer Q&A source rule:**

- Stack Overflow / Stack Exchange is BLOCK for default training input unless
  legal policy explicitly approves the current dump terms and ShareAlike
  implications for LLM training.

- GitHub Issues may be used only when repository license and contribution terms
  are checked and recorded. Prefer first-party XIIGen prompts and permissively
  licensed repository examples before issue text.

**Synthetic generation approach:**

For each entity in the five validation projects, generate 10 variations of
the impact question using the pattern library in Required Developer Task
Patterns section of PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md. Mark all as
synthetic and use for pre-training only, not for held-out evaluation.

---

## Gap 2: Multi-Step Flow Trace

### GAP

The wiki cannot trace what happens to data across multiple steps when a
developer asks about a workflow. It answers about individual methods but
not about the full data journey.

### VALIDATION TASK

Given a project with a documented multi-step flow, ask four variations of
the flow trace question. All four must return answers that name every step
from entry to final output, including what data each step receives and
produces.

Minimum passing threshold: flow_trace_question coverage >= 0.85.

### PROMPT EXAMPLES FROM orchestrator_prompts.txt

**From scenario 4 — diet app, daily WhatsApp flow:**

```
V1: "A client submits their daily food list via WhatsApp.
     Walk me through every step from WhatsApp message receipt to the
     daily feedback being sent back."

V2: "The AI calculates calories in several rounds.
     What data enters each round and what does the round produce?
     Where is the intermediate result stored between rounds?"

V3: "The diet plan generation runs multiple AI models in sequence.
     What does each model receive and what does it output?
     What happens if one model returns a low-confidence result?"

V4: "I want to add a step that checks for food allergies before the
     calorie calculation. Where exactly in the flow does this step
     belong and what data would it need?"
```

**From scenario 5 — content generation pipeline:**

```
V5: "A user submits audio. Walk me through every step until a video
     is scheduled on all platforms."

V6: "The image generation step receives prompts. Where do those prompts
     come from and what happens to the generated images next?"

V7: "I want to add a human review step before the video is scheduled.
     What is the current flow at that point and where would the review
     step fit?"
```

**From scenario 1 — Figma analysis:**

```
V8: "The system analyzes Figma screens and generates architecture.
     What data does the screen analysis step produce and who consumes it?"
```

### REQUIRED ANSWER SHAPE FOR FLOW TRACE

```
1. Entry point: what triggers this flow and what data arrives.
2. Each step: name, input, output, side effects, storage writes.
3. Decision points: conditions that change the path.
4. Error/low-confidence handling at each step.
5. Final output and how it reaches the end consumer.
6. Any steps that are unresolved in the current documentation.
```

### OPEN SOURCES FOR PRE-TRAINING

**Workflow and pipeline repositories:**

- `n8n-io/n8n` is ACCEPT_SUPPORT_ONLY at most. Do not use it as default
  commercial LLM training input unless a specific permissive subset is verified.
  Use it only as a conceptual comparison after source policy approval.
- `Netflix/conductor` — workflow orchestration engine with step definitions,
  state transitions, and data flow between tasks. License: Apache 2.0.
  URL: https://github.com/Netflix/conductor

- `temporalio/samples-dotnet` — .NET workflow samples with activity chains
  and data passing patterns. License: MIT.
  URL: https://github.com/temporalio/samples-dotnet

- `MassTransit/MassTransit` — .NET messaging/saga patterns. Contains many
  examples of data flowing through ordered processing steps. License: Apache 2.0.
  URL: https://github.com/MassTransit/MassTransit

**Datasets:**

- Stack Overflow / Stack Exchange is BLOCK for default training input unless
  legal policy explicitly approves the current dump terms and ShareAlike risk.
---

## Gap 3: Change Location Identification

### GAP

The wiki cannot tell a developer exactly where in the codebase to make a
change. It describes the system but cannot say "the correct place to add
this validation is ServiceX.MethodY, not ControllerZ, because..."

### VALIDATION TASK

Ask five "where should I add/change" questions. All five must return the
correct layer and method, not just the component name, with a reason based
on architectural evidence.

### PROMPT EXAMPLES FROM orchestrator_prompts.txt

**From scenario 2 — integrating Figma design into existing system:**

```
V1: "The system applies a design theme to existing modules. I need to
     add support for dark mode. Where in the codebase is the theme
     applied and which file should I change?"

V2: "The system generates documentation using microservices. I want to
     add a new documentation format. Where does the format selection
     happen?"
```

**From scenario 3 — generate system from scratch:**

```
V3: "I am implementing an architecture that the system generated.
     The implementation plan has a step for 'add authentication'.
     Where is the current authentication logic and where should new
     auth rules go?"

V4: "The skill sets are organized by language subfolder.
     I want to add a new language. What files must I create and in
     what location?"
```

**Generic developer patterns:**

```
V5: "Email validation currently runs somewhere in the registration flow.
     Is it in the controller or the service? If I need to add a stricter
     rule, where is the correct place?"

V6: "I want to add logging to every AI model call. Where is the model
     call abstraction and where would logging be added without
     duplicating it in every caller?"

V7: "The queue job processor handles four different AI models.
     I want to add a fifth. Where is the routing logic that decides
     which model handles which job type?"
```

### REQUIRED ANSWER SHAPE FOR CHANGE LOCATION

```
1. The exact file and method where the change belongs.
2. Why this location and not alternatives (architectural reason).
3. What the caller/callee contract at this location looks like.
4. What breaks if the change is made in the wrong layer.
5. Nearest test file that covers this location.
```

### OPEN SOURCES FOR PRE-TRAINING

- `davidfowl/TodoApi` — minimal .NET API with clean separation of concerns.
  Demonstrates correct layer decisions. License: MIT.
  URL: https://github.com/davidfowl/TodoApi

- `microsoft/dotnet-podcasts` — full .NET reference app with multiple layers.
  License: MIT. URL: https://github.com/microsoft/dotnet-podcasts

- Stack Overflow / Stack Exchange is BLOCK for default training input unless
  legal policy explicitly approves the current dump terms and ShareAlike risk.
---

## Gap 4: Impact Analysis and Regression Risk

### GAP

The wiki cannot answer "what breaks or changes if I rename X / remove Y /
change the return type of Z". It knows what X is but not what depends on it.

### VALIDATION TASK

Ask four rename/remove/type-change questions. All four must list every
caller and downstream consumer that must be updated, and estimate what
breaks at compile time vs runtime.

### PROMPT EXAMPLES FROM orchestrator_prompts.txt

**From the generic skills and implementation requirements:**

```
V1: "I want to rename the flow trigger event from 'job_ready' to
     'task_completed'. What consumers listen to this event and what
     must be updated?"

V2: "The generic repository interface has a method GetByFilter.
     I want to rename it to QueryByFilter. How many callers will break
     and where are they?"

V3: "The AI model interface currently returns a string result. I want
     to change it to return a structured ResultDto. What consumers of
     this interface will need to be updated?"

V4: "I want to remove the feedback_text field from the user feedback
     entity. The field is used somewhere for training. What must be
     updated before I can safely remove it?"
```

**From scenario 5 — content pipeline:**

```
V5: "The MediaAsset.file_url field is used in several places.
     I want to make it nullable. What callers assume it is always set?"
```

### REQUIRED ANSWER SHAPE FOR IMPACT ANALYSIS

```
1. Every direct caller that must change (compile-time break).
2. Every indirect consumer (runtime break, test break).
3. Every serialization or API contract that exposes this symbol.
4. Every test that will fail.
5. Whether the change is safe to make atomically or requires a migration step.
```

### OPEN SOURCES FOR PRE-TRAINING

- `dotnet/roslyn` issue tracker — real developer discussions about API
  breaking changes, rename impacts, and migration paths. License: MIT.

- GitHub search: `is:issue label:"breaking change"` in C# repos.

- Microsoft .NET API compatibility analyzer docs — contains examples of
  what breaks at binary/source level for each change type. Open documentation.
  URL: https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/overview

---

## Gap 5: Architecture and Boundary Reasoning

### GAP

The wiki cannot explain architectural decisions — why the system is
structured in a particular way, what the boundaries are, and what
the consequences of crossing them are.

### VALIDATION TASK

Ask three "why is it structured this way" questions and two "what happens
if I cross this boundary" questions. All five must answer with architectural
evidence, not just description.

### PROMPT EXAMPLES FROM orchestrator_prompts.txt

The orchestrator prompts explicitly require generic interfaces reusable for
databases, queues, and AI models; a generic microservice base; and support
for alternative implementations (Kafka/SQS, Node.js/Rust/Java, multiple DB
fabrics). This creates concrete architecture questions.

```
V1: "The system uses IRepository<T> for all data access. Why does the
     AI model caller not use the same abstraction? What would happen
     if I called the database directly from the AI model handler?"

V2: "There is a generic queue interface that supports Kafka and SQS.
     If I add a new queue provider, which file defines the contract
     and what must the new implementation satisfy?"

V3: "The microservice base has architectural capabilities built in.
     Which of those capabilities are mandatory for all services and
     which are optional? Where is this decided?"

V4: "The RAG system needs to be swappable between Neo4j, CosmosDB, and
     Pinecone. Where is the RAG interface defined and what would a new
     provider need to implement?"

V5: "The skill sets have subfolders for each technology. If I add a
     Rust implementation of the server, what contract must it satisfy
     to be treated as equivalent to the .NET version by the orchestrator?"
```

### REQUIRED ANSWER SHAPE FOR ARCHITECTURE

```
1. The interface or contract that enforces the boundary.
2. The concrete rule that the boundary encodes.
3. What breaks architecturally if the boundary is crossed.
4. Where a new implementation must register itself.
5. Which tests verify the boundary.
```

### OPEN SOURCES FOR PRE-TRAINING

- `dotnet/samples` architecture section — shows layered architecture with
  explicit dependency rules. License: MIT.
  URL: https://github.com/dotnet/samples

- `StackExchange/StackExchange.Redis` and other open adapters — show the
  pattern of implementing a swappable interface for infrastructure.
  License: MIT.

- Architecture Decision Records (ADR) repositories on GitHub:
  Search `topic:architecture-decision-records language:markdown`.
  Many are CC0 or MIT.

- Michael Feathers "Working Effectively with Legacy Code" is BLOCK for commercial LLM training input. Do not use its text or derived examples as rows. Legacy-code contrast rows must come from first-party XIIGen examples or permissively licensed repositories instead.

---

## Gap 6: Missing Evidence and Abstention

### GAP

The wiki answers questions even when the documentation does not contain
enough evidence to answer correctly. It should abstain and explain what
is missing.

### VALIDATION TASK

Ask three questions where the answer requires evidence that is not in the
documentation. All three must trigger abstention with a specific explanation
of what is missing, not a guessed answer.

### PROMPT EXAMPLES

```
V1: "I want to add CalorieGoal to DietPlan. What does the nightly
     batch job do with DietPlan?" (Nightly batch is not in the docs.)
     Expected: "The documentation does not contain a nightly batch job
     that reads DietPlan. The path from DietPlan to any nightly process
     is unresolved. Inspect scheduler configuration and background
     service registrations."

V2: "What happens to ContentRequest after the video is uploaded to S3?"
     (S3 integration is not documented.)
     Expected: "The documentation shows ContentRequest up to the upload
     step. What happens after the upload completes is not covered in
     the current documentation. Inspect the upload completion handler
     or webhook configuration."

V3: "Why does the AI model selector choose GPT-4 over Claude for
     image descriptions?" (Model selection logic is not documented.)
     Expected: "The model selection logic is not in the current
     documentation. The evidence index does not contain a model
     selector that explains this choice. This requires direct code
     inspection of the model routing component."
```

### REQUIRED ANSWER SHAPE FOR ABSTENTION

```
1. Explicit statement that the answer is not supported by current evidence.
2. What specifically is missing from the documentation.
3. Where in the codebase to look for the answer.
4. What the developer must inspect manually.
```

Passing: answers that abstain correctly.
Failing: any answer that invents content or gives a partial answer without
flagging that evidence is missing.

### OPEN SOURCES FOR PRE-TRAINING

Abstention behavior is best trained from:

- `TruthfulQA` dataset — questions where the correct answer is "this is
  not knowable from the available information". License: Apache 2.0.
  URL: https://github.com/sylinrl/TruthfulQA

- `FEVER` is QUARANTINE for default training input because it is CC BY-SA 3.0.
  Use only in an isolated ShareAlike-approved experiment, not in the default
  commercial training path.
- Synthetic generation: take documented entities and ask about undocumented
  behavior (batch jobs, external integrations, async callbacks that were
  excluded from the trace). These are natural abstention test cases for
  every validation project.

---

## Gap 7: Query Plan Orchestration

### GAP

The wiki can retrieve or answer too early. It does not first decide what it
must search for, in what order, and which evidence would be sufficient. This is
why entity-impact questions degrade into a single symbol lookup.

### VALIDATION TASK

Given a developer question, `ProjectWikiQueryPlanningFlow` must produce a
multi-step query plan before retrieval. The plan must include intent, entities,
ordered query steps, sufficiency checks, and missing-evidence policy.

Minimum passing threshold:

```text
query_plan_accuracy >= 0.90
required_touchpoint_recall >= 0.95
multi_hop_sequence_accuracy >= 0.90
missing_evidence_detection >= 0.95
irrelevant_query_rate <= 0.05
```

### PROMPT EXAMPLES

Required variations:

1. "I want to add DiscountCode to CampaignCreateDto. What path does it travel
   and what can break?"
2. "Before changing ClientProfile, what do we need to inspect in validation,
   mapping, storage, outputs, and tests?"
3. "Where should we look to understand how DailyEntry affects diet plan
   calculations?"
4. "Trace MediaAsset through ingestion, processing, storage, publication, and
   failure paths before answering."
5. "Find the safest place to add a new response field and what nearby code must
   be checked."
6. "Do not answer yet; tell me what evidence is needed to know if this rename
   is safe."
7. "This looks like askapediaweb, but the question names a GenieDynamicServer
   entity. What should the wiki do?"
8. "The documentation is stale for this branch. What should be searched before
   trusting the answer?"

### OPEN SOURCES

- `dotnet-architecture/eShopOnWeb` (MIT): use for entity-to-service-to-storage
  investigation plans.
- `ardalis/CleanArchitecture` (MIT): use for use-case boundary and entity
  change planning.
- `temporalio/samples-dotnet` (MIT): use for workflow-step investigation
  planning.
- `conductor-oss/conductor` (Apache-2.0): use for workflow dependency and
  task-routing query-plan examples.

---

## Gap 8: Stored Procedure Logic

### GAP

Legacy .NET projects can put business logic in SQL Server stored procedures.
The wiki must not document only the C# caller or only the procedure name. It
must connect SQL parameters, tables read and written, result shape,
transaction behavior, side effects, .NET callers, and value flow.

This gap is required when the project wiki contains SQL/procedure evidence.
DNN, old nopCommerce, and SmartStoreNET may be used only as validation
surfaces unless a separate source authority explicitly approves them for
training. Do not use those repositories as training rows from this section by
default.

### VALIDATION TASK

Given a project with accepted stored procedure evidence, ask eight variations
of stored-procedure impact questions. `ProjectWikiQueryPlanningFlow` must
route them to `stored_procedure_impact` and plan all SQL and .NET evidence
families before answer composition.

Minimum passing threshold:

```text
stored_procedure_query_plan_accuracy >= 0.90
sql_parameter_recall >= 0.95
sql_table_dependency_recall >= 0.95
sql_result_shape_recall >= 0.95
dotnet_caller_value_flow_recall >= 0.95
missing_evidence_detection >= 0.95
```

### PROMPT EXAMPLES

Required variations:

1. "A legacy .NET workflow calls dbo.GetTabModules. What should the wiki
   search before explaining the stored procedure logic?"
2. "Before changing stored procedure dbo.UpdateClientProfile, which SQL
   parameters, tables read and written, result set, and .NET callers must be
   inspected?"
3. "If a repository calls dbo.GetOrderTotals, how should the wiki trace
   parameters from .NET into SQL and result columns back out?"
4. "What evidence is needed to prove whether dbo.RecalculateDietPlan has
   transaction behavior or side effects?"
5. "How should the wiki answer a developer who wants to add a field returned
   by dbo.GetCampaignSummary?"
6. "Which table dependencies and side effects must be found before saying a
   stored procedure used by CampaignRepository is safe to change?"
7. "Where should the wiki abstain if procedure body, SQL parameters, or .NET
   caller evidence is missing?"
8. "What query plan connects stored procedure result shape to repository and
   service callers?"

### REQUIRED ANSWER SHAPE FOR STORED PROCEDURES

The accepted answer must include:

```text
procedure name and schema
SQL parameter list including output/default parameters
tables read
tables written
result set columns or return code
transaction/isolation/error behavior
SQL side effects: audit rows, queues, status changes, cascading writes
.NET callers: repository/provider/DAL/service/controller/job
argument value flow from .NET into SQL
result value flow from SQL back into .NET mappings and outputs
tests or missing test evidence
explicit missing evidence
```

### BAD REJECTED TYPES

Use these as DPO rejected patterns:

```text
procedure_name_only
missing_sql_parameters
missing_result_shape
missing_table_side_effects
missing_dotnet_callers
```

Each rejected side must remain plausible and near-correct, but must contain
one evidence-checkable defect and a specific `[WRONG: ...]` consequence.

### VALIDATION-ONLY PROJECTS

These projects are validation surfaces, not default training sources:

- DNN Platform / DotNetNuke: strong stored-procedure legacy validation
  candidate.
- old nopCommerce versions: validation only unless license/source authority is
  separately approved.
- SmartStoreNET: AGPL, validation only; do not use for commercial training
  rows.

---

## Summary: Gap-to-Validation Mapping

| Gap | Validation task | Variations required | Threshold |
|---|---|---|---|
| Entity impact | 9 prompt variations from scenarios 1, 4, 5 | All 9 pass | >= 0.85 |
| Multi-step flow trace | 8 prompt variations from scenarios 4, 5 | All 8 pass | >= 0.85 |
| Change location | 7 prompt variations from scenarios 1, 2, 3 | All 7 pass | >= 0.88 |
| Impact analysis | 5 prompt variations, rename/remove/type-change | All 5 pass | >= 0.85 |
| Architecture boundary | 5 prompt variations, interface/boundary questions | All 5 pass | >= 0.80 |
| Missing evidence / abstention | 3 undocumented-area prompts | All 3 abstain correctly | 100% |
| Query plan orchestration | 8 prompt variations before retrieval | All 8 pass | query_plan_accuracy >= 0.90 |
| Stored procedure logic | 8 SQL/procedure variations when SQL evidence exists | All 8 pass | stored_procedure_query_plan_accuracy >= 0.90 |

Total required passing variations before any unit is promoted:

```text
base wiki scope: 45
stored-procedure wiki scope: 53 (45 base + 8 stored-procedure variations)
```

A unit that passes on easy variations but fails on the specific orchestrator
prompt scenarios is not promoted. The scenarios are the reference.

---

## What "Passes" Means For Each Variation

A variation passes when:

- the query plan names the required search steps before any answer is composed;
- the answer names all required path elements for the question type;
- every named element has a cited evidence_id;
- no named element was invented without evidence;
- the answer stays within the correct project;
- if evidence is missing, the answer abstains rather than guesses.

A variation fails when:

- the answer stops at an early step and does not continue the path;
- the answer says "update all references" without naming them;
- the answer invents a module that is not in the evidence index;
- the answer uses evidence from a different project;
- the answer gives a confident conclusion where the documentation is absent.

---

## Notes on Prompt Variations

The same underlying gap must be validated with multiple phrasings because
developers ask the same question in different ways:

Entity impact variations of "add a field to X":

```
"I want to add DiscountCode to CampaignCreateDto."
"We need a new required field DiscountCode on the campaign creation form."
"Campaign creation is missing a discount code input. How do I add it end to end?"
"Add DiscountCode: string to CampaignCreateDto. What else needs to change?"
"The campaign entity needs a DiscountCode property. Where do I add it and
 what do I need to update?"
```

All five ask the same thing. All five must pass. Training on only one phrasing
is not sufficient.

The wiki unit must recognize the developer's intent regardless of how they
phrase it, route to entity_impact_question, and return the same complete
impact path.
