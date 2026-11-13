# Project-Local Wiki Model Plan v1

Status: REFERENCE - DO NOT EXECUTE.

This document is an architecture and execution reference. Claude Code or any
other executor must not run the full implementation sequence directly from this
file. Execution must use the session files listed in `PROJECT-LOCAL-WIKI-MODEL-STATE.json`.
Each session file has an explicit stop gate.

Verification must follow `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md`.
The accepted result is an orchestrated project-documentation/wiki run. Internal
JSONL files are snapshots, not the public API and not proof of completion.
Training and coverage must follow
`PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md`: every batch updates a
code trace and every ten batches must explain coverage improvement or the exact
reason coverage did not improve.
Training gap closure must follow `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md`: all
wiki units are pre-trained generically before project validation. Project code
provides evidence and validation, not the generic training source.
Executor behavior must follow `EXECUTOR-DISCIPLINE-RULES-v1.md`: do not stop
after a passed gate, do not ask whether to run the required next step, and do
not claim done until real training and real-project gates pass.
Metric reporting must follow
`ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md`: report raw accuracy,
reward margin, CPU, RAM, time, and per-row/per-50-row cost from runner output.
Gap validation examples must follow `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`.
Wiki question planning must follow
`PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`: developer questions are
first converted into a multi-step evidence investigation plan before retrieval
or answer composition.
Protocol expansion must follow
`ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md`: every promoted or
rejected run updates the unit solution library and language coverage state.
Stage execution prompts must follow `EXECUTOR-STAGE-PROMPT-v2.md`: before each
sub-stage, the executor must restore the stage goal, required reads, and
decision dilemmas instead of improvising from memory.

Purpose: turn generated project documentation into a project-owned wiki domain.
The wiki is not just a static document folder and not just a RAG index. It is a
small project-local trained model unit, saved with the project, registered as a
local `ILogicalOrchestrator`, and tested by asking real questions about that
project.

This plan follows the diagram in `orchestrator wiki.drawio.xml`:

1. User asks to make documentation.
2. Main orchestrator routes the work into the documentation domain.
3. `DocumentationOrchestrator` reads code and builds bottom-up docs.
4. Project code produces a project-local trace and evidence graph for
   validation.
5. Pre-trained wiki units use that evidence graph to generate/verify
   documentation, negative DPO, and developer-reasoning answers.
6. Project-local training or adaptation may run only after the generic wiki
   units pass their held-out gates.
7. The accepted local documentation wiki becomes
   `ProjectLocalWikiOrchestrator`.
8. Later project questions pass through `ProjectWikiRoutingFlow` and are routed
   to the correct project-local wiki orchestrator.
9. Inside that project wiki, `ProjectWikiQueryPlanningFlow` converts each
   developer question into a sequence of evidence queries before any answer is
   composed.

## Phase Boundary

Phase 1 is the green box in the diagram:

```text
Make documentation
  -> Main IOrchestrator
  -> Code
  -> DocumentationOrchestrator
  -> Documentation
  -> NegativeDtoDocumentationOrchestrator
  -> teach local wiki LLM
  -> ProjectLocalWikiOrchestrator
```

Phase 1 does not build the user question database. It may create documentation
training rows, documentation negative DTO rows, and local documentation model
weights.

Phase 2 is only the red box in the diagram:

```text
Question about code
  -> Main IOrchestrator
  -> ProjectWikiRoutingFlow
  -> ProjectWikiQueryPlanningFlow
  -> execute planned wiki evidence queries
  -> get answers from local logical orchestrator
```

The question database, question selection, question seeds, and question-answer
evaluation belong to Phase 2. They are not prerequisites for implementing the
Phase 1 documentation pipeline.

Name boundary:

- `DocumentationOrchestrator`: creates documentation from code.
- `ProjectLocalWikiOrchestrator`: answers questions from accepted project docs
  and project-local wiki weights.
- `ProjectWikiRoutingFlow`: Level 0 learned router that decides whether a
  question belongs to a specific project wiki.
- `ProjectWikiQueryPlanningFlow`: learned planning flow that decides what the
  wiki must search, in what order, and what evidence is sufficient before the
  answer unit may respond.

These are four separate responsibilities. One class must not do all four.

## Core Idea

Each customer project gets its own wiki domain:

```text
project/
  .xiigen/
    wiki-domain/
      domain-manifest.json
      documentation/
      evidence/
      relations/
      question-bank/
      training/
      heldout/
      eval/
      weights/
      model-manifest.json
      question-ledger.jsonl
```

`question-bank/` is created in Phase 2 only. It is shown in the complete target
layout for future compatibility, not as a Phase 1 scaffold requirement.

The global model learns how to document code and how to route questions. The
project-local model learns facts about one project only: symbols, workflows,
business rules, stack versions, call chains, generated documentation, and
accepted relation facts.

The local weights must not leak across projects. The unit is project-scoped by
`project_id`, `repository_root`, `source_commit`, and `documentation_version`.

## Phase 1 Documentation DTO And Relation Graph

Phase 1 documentation is not a loose prose summary. It is a bottom-up relation
system. Every accepted documentation row must explain the code unit itself and
how meaning moves through the call graph.

For a leaf method or low-level function, document:

- the exact code symbol;
- what it means in the project;
- where it is located;
- which parameters it accepts;
- what each parameter means;
- who calls it, if known;
- what it returns;
- what the return value means;
- which evidence proves each claim.

For a caller, component, workflow, or higher-level unit, document the same
fields plus the called functions:

- which functions it calls;
- what each called function means;
- which parameters are sent to each callee;
- where those sent parameters came from;
- what each callee returns;
- what the caller does with returned values;
- which branch, state change, persistence write, response, or side effect is
  produced from those return values.

This is the core compiler-like propagation rule:

```text
callee meaning + callee parameters + callee return
  -> caller receives value
  -> caller transforms/branches/persists/responds
  -> caller meaning
  -> next caller
```

The documentation pipeline must therefore write both human-readable docs and
machine-readable relation rows:

```text
.xiigen/wiki-domain/documentation/
.xiigen/wiki-domain/evidence/evidence-index.jsonl
.xiigen/wiki-domain/relations/documentation-units.jsonl
.xiigen/wiki-domain/relations/call-edges.jsonl
.xiigen/wiki-domain/relations/value-flow-edges.jsonl
```

### Documentation Unit DTO

Each accepted documentation unit must have a DTO like this:

```json
{
  "documentation_unit_id": "askapediaweb::Campaign.GetCampaign",
  "project_id": "askapediaweb",
  "documentation_version": "doc-batch-860",
  "source_path": "askapedia/Askapedia/BL/Campaign.cs",
  "symbol_kind": "method",
  "symbol_name": "Campaign.GetCampaign",
  "signature": "CampaignViewModel GetCampaign(int campaignId)",
  "target_framework": "v4.6.1",
  "csharp_language_version": "CSharp5",
  "meaning": "Builds the campaign view model by loading the campaign and mapping its question and answers.",
  "parameters": [
    {
      "name": "campaignId",
      "type": "int",
      "meaning": "Identifier of the campaign to load.",
      "source": "caller supplied"
    }
  ],
  "return_value": {
    "type": "CampaignViewModel",
    "meaning": "View model returned to the UI layer.",
    "possible_states": ["campaign_found", "campaign_missing"]
  },
  "callers": [
    {
      "caller_id": "askapediaweb::CampaignController.Details",
      "call_site": "CampaignController.cs:42",
      "why_calls": "Needs the display model for the campaign details page."
    }
  ],
  "callees": [
    {
      "callee_id": "askapediaweb::CampaignRepository.GetById",
      "meaning": "Loads the campaign aggregate.",
      "arguments_sent": [
        {
          "parameter_name": "id",
          "value_expression": "campaignId",
          "value_meaning": "The requested campaign identifier."
        }
      ],
      "return_received": {
        "variable": "campaign",
        "meaning": "The loaded campaign aggregate."
      },
      "caller_uses_return_by": "Passing it to CampaignExtensions.ToViewModel."
    }
  ],
  "side_effects": [],
  "evidence_ids": [
    "askapediaweb::Campaign.GetCampaign",
    "askapediaweb::CampaignRepository.GetById"
  ],
  "confidence": 0.0
}
```

### Relation Edge DTOs

`call-edges.jsonl` records who calls whom. `value-flow-edges.jsonl` records
what values are sent into the callee and what the caller does with the returned
value. These two files are the future substrate for questions, but they are
created in Phase 1 as documentation evidence, not as the Phase 2 question
database.

## Phase 2 Trainable Question Bank

Every accepted documentation output can later produce possible questions about
that code. This is a Phase 2 artifact, not a Phase 1 prerequisite. The goal is
to let the system gradually shift from asking a large external AI to explain the
project toward using its own local wiki and learned orchestrator.

When the documentation orchestrator writes a method, caller, workflow, feature,
or system-boundary document in Phase 1, it does not have to write questions
immediately. Phase 2 reads accepted documentation and creates question
candidates into a separate trainable question bank.

Storage:

```text
Question database: project-wiki-questions
Portable export: project/.xiigen/wiki-domain/question-bank/question-bank-export.jsonl
Batch seed export: project/.xiigen/wiki-domain/question-bank/question-seeds-batch-NNN.jsonl
```

The question database is not a passive table. It is managed by a trainable
selection flow:

```text
Accepted documentation + evidence index + prior wiki failures
  -> TrainableQuestionBankFlow : ILogicalFlow<ProjectQuestionBankRequest>
  -> question candidates
  -> question utility scoring
  -> train/held-out seed selection
```

The question bank learns:

- which question types expose weak wiki behavior;
- which symbols, callers, workflows, and features are under-tested;
- which questions are too easy and should be down-weighted;
- which questions are ambiguous or unhelpful as evaluation prompts;
- which held-out questions best predict real project Q/A quality;
- when to ask for missing evidence instead of creating a misleading question.

Each database row:

```json
{
  "question_id": "QB-ASK-001-0001",
  "project_id": "askapediaweb",
  "documentation_version": "doc-batch-860",
  "source_symbol": "askapediaweb::Campaign.GetCampaign",
  "source_doc_path": ".xiigen/wiki-domain/documentation/Campaign.GetCampaign.md",
  "question_level": "L2_WORKFLOW",
  "question": "How does Campaign.GetCampaign build the campaign view model?",
  "expected_evidence_ids": [
    "askapediaweb::Campaign.GetCampaign",
    "askapediaweb::CampaignExtensions.ToViewModel",
    "askapediaweb::AnswerExtensions.ToViewModel"
  ],
  "router_target": "wiki-askapediaweb-v1",
  "boundary_question_type": "wrong_symbol",
  "must_abstain_if_missing_evidence": true,
  "question_status": "candidate",
  "utility_score": 0.0,
  "selection_policy_version": "question-bank-flow-v1",
  "times_used_for_training": 0,
  "times_used_for_heldout": 0,
  "last_eval_result": "not_evaluated"
}
```

Required question seed classes:

- `symbol_question`: ask what one method/class does;
- `caller_question`: ask why a caller invokes a callee;
- `workflow_question`: ask what happens across a call chain;
- `feature_question`: ask what user-visible feature a workflow supports;
- `system_boundary_question`: ask about externally visible project behavior;
- `stored_procedure_impact_question`: ask how SQL stored procedure logic,
  result shape, side effects, and .NET callers affect a developer change;
- `stored_procedure_trace_question`: ask how procedure parameters and result
  values flow between .NET code and SQL;
- `version_question`: ask which .NET/C# version constraint matters;
- `evidence_question`: ask where the answer is proven;
- `missing_evidence_question`: ask something that must be abstained until more
  evidence is available;
- `wrong_project_question`: plausible question that belongs to another project;
- `wrong_level_question`: question that should not be answered at the requested
  level yet.

This is the canonical question taxonomy. Later implementation steps must use
these ten names. Natural-language examples are allowed, but they do not define a
second taxonomy.

The question bank must keep train and held-out usage separate. A question used
for training in a given documentation version cannot be reused as held-out for
that same wiki adapter version.

The question-bank generator writes question candidates only. It does not own
negative DTO generation and it does not create rejected answers. Questions may
be answerable, missing-evidence, wrong-project, or wrong-level boundary checks,
but those are question classes, not documentation-negative training rows.

If Phase 2 later needs preference training for answers, it must define a
separate answer-contrast generator. That future unit must not reuse the Phase 1
`NegativeDtoDocumentationOrchestrator`, because Phase 1 negative DTOs are about
documentation structure, not user Q/A answers.

This is how the system becomes less dependent on large AI requests:

1. early passes use AI/code analysis to produce documentation;
2. documentation must pass its own acceptance gate;
3. accepted documentation enters the evidence index;
4. accepted documentation plus code excerpts create validated negative DTO
   documentation rows;
5. accepted documentation and validated negative DTO rows train the local
   documentation wiki model;
6. Phase 2 creates question candidates in the trainable question bank;
7. project-local wiki answers common questions directly;
8. only unknown or low-confidence questions are escalated to a larger AI;
9. escalated answers, after verification, become new documentation and new
   question seeds.

The local wiki therefore improves as a flywheel:

```text
code -> documentation -> negative DTO -> local documentation wiki training
     -> accepted local documentation orchestrator
```

Phase 2 later adds the question loop:

```text
code -> accepted documentation -> question database
     -> learned question selection -> Q/A train and held-out batches
     -> wiki training/evaluation -> question utility updates
```

## Phase 1 Negative DTO Documentation Orchestrator

Project-local wiki training requires a second learned helper beside the
documentation model:

```text
Accepted documentation + source code excerpt + evidence index
  -> NegativeDtoDocumentationOrchestrator : ILogicalOrchestrator
  -> 3-5 negative documentation DTO candidates
  -> internal evidence validation step
  -> selected negative documentation DTO
  -> documentation training pair assembler
```

The generator's job is not to write random bad answers. It must create
plausible wrong documentation DTOs that teach the local documentation model the
exact boundary it must learn. A good negative DTO is close enough to be
tempting, but wrong in a specific, evidence-checkable way.

Diagram naming note: if a diagram labels this unit as "Negative DTO", it means
this Phase 1 documentation-negative stage. It is not the Phase 2 question-bank
layer.

The generator may read the original source code excerpt to create realistic
near-miss answers. It may not use source code to override the accepted
documentation gate. Validation is an internal responsibility of
`NegativeDtoDocumentationOrchestrator`, not a separate runtime class. A
generated negative is usable only when the wrongness is provable against
accepted docs and `evidence-index.jsonl`.

`NegativeDtoDocumentationOrchestrator` should be trained as its own small
model/adapter over time. Initial manually written negative examples are allowed
only as bootstrapping rows. After that, the generator must produce candidate
negative DTOs automatically and the verifier must decide which are usable.

### Negative Candidate Contract

Each generated candidate must include:

```json
{
  "candidate_id": "NEGDTO-ASK-001-0001-A",
  "project_id": "askapediaweb",
  "documentation_id": "askapediaweb::QuestionsController.DeleteQuestion",
  "source_excerpt_id": "askapediaweb::QuestionsController.DeleteQuestion",
  "negative_type": "wrong_symbol",
  "negative_documentation": "QuestionsController.DeleteQuestion saves a lead and sends a notification. [WRONG: wrong_symbol - the DTO uses SaveLead behavior instead of DeleteQuestion evidence, so the documentation model learns the wrong feature boundary.]",
  "why_wrong": "Uses evidence from another symbol.",
  "expected_failure_gate": "evidence_id_mismatch",
  "plausibility_reason": "Both methods are in QuestionsController and both return ActionResult lists.",
  "must_not_be_ambiguous": true
}
```

### Negative DTO Taxonomy

The negative generator must cover these types:

- `wrong_project`: answer from another project's wiki.
- `wrong_symbol`: answer from a different class or method in the same project.
- `wrong_level`: method-level answer when workflow/system-level answer is
  required.
- `missing_callee`: answer before all required callee summaries are available.
- `stale_doc`: answer from an older documentation version or source commit.
- `stack_version_hallucination`: wrong .NET, C#, WebForms, MVC, WPF, or ASP.NET
  Core version.
- `source_free_answer`: answer without evidence citation.
- `idealized_architecture`: desired/refactored architecture instead of actual
  legacy behavior.
- `overconfident_unknown`: confident answer when evidence is absent.
- `citation_hallucination`: cites evidence ids that do not exist.
- `evidence_mismatch`: cites real evidence ids that do not support the answer.
- `callee_direction_reversal`: reverses caller/callee meaning.
- `local_delegated_merge`: mixes local method behavior with delegated callee
  behavior.

### Negative DTO Verifier

No generated negative DTO may enter training until it passes the verifier:

- negative DTO is not equal to the accepted documentation DTO;
- negative DTO contains a specific `[WRONG: ...]` annotation;
- annotation names the weakness and the functional consequence;
- negative DTO is plausible for the code/documentation unit, not trivial
  nonsense;
- wrongness is checkable against `evidence-index.jsonl`;
- negative DTO does not accidentally become a second valid documentation;
- negative DTO does not rely on facts outside accepted documentation;
- overlap with the accepted DTO is below the configured maximum unless the
  difference is a precise evidence/citation defect;
- negative type is balanced across the batch.

Rejected candidates that fail validation go to:

```text
project/.xiigen/wiki-domain/training/batch-NNN/negative-dto-repair-required.jsonl
```

Validated candidates go to:

```text
project/.xiigen/wiki-domain/training/batch-NNN/negative-dto-validation-ledger.jsonl
```

### Generator Quality Metrics

Every 50-row wiki batch must report:

- `negative_candidate_count`;
- `negative_candidate_acceptance_rate`;
- `ambiguous_negative_rejection_count`;
- `trivial_negative_rejection_count`;
- `wrong_annotation_failure_count`;
- `negative_type_distribution`;
- `verifier_disagreement_count`;
- local documentation model held-out accuracy by negative type.

If wiki held-out failures concentrate in one negative type, the next batch must
reserve rows for that type. This is how the system learns what to improve,
instead of relying on manual intuition.

## Design Pattern

The model becomes a design pattern:

```text
Main IOrchestrator
  -> DocumentationOrchestrator : ILogicalOrchestrator
      -> accepted documentation
      -> documentation unit DTOs
      -> call/value relation graph
      -> NegativeDtoDocumentationOrchestrator : ILogicalOrchestrator
      -> negative DTO validation ledger
      -> documentation training pairs
      -> project-local wiki training
  -> ProjectWikiRoutingFlow : ILogicalFlow<ProjectQuestion>
      -> ProjectLocalWikiOrchestrator : ILogicalOrchestrator
          -> ProjectWikiQueryPlanningFlow : ILogicalFlow<ProjectWikiUserQuestion, ProjectWikiQueryPlan>
          -> WikiContextRetriever
          -> ProjectLocalWikiModelUnit
          -> ProjectAnswerVerifier
```

Important rule: the local wiki model may answer only after the question plan
has identified the required evidence and the retriever has executed that plan.
Accepted project documentation and evidence are the answer source. If evidence
is missing, it must say that the answer requires source confirmation.

## Retrieval Modes And Dependencies

The project-local wiki has two different retrieval modes. They must not be
collapsed into one implementation.

### Mode 3: Lexical Evidence Lookup

Mode 3 is BM25 over accepted wiki evidence. It is used for deterministic
lookup, audit, validation, citation checking, and evidence id recovery.

Implementation contract:

- use `SearchEngineFacade` from `C:\Projects\llm_mvp_core\12.SearchEngine`;
- index/read `.xiigen/wiki-domain/evidence/evidence-index.jsonl`;
- treat this as lexical BM25 search only;
- do not claim vector embeddings are used by `12.SearchEngine`;
- do not require `EmbeddingModel` for Mode 3;
- return evidence ids, documentation paths, source paths, symbol ids, and
  relation ids.

Mode 3 is the correct path when the system needs to prove that a documentation
unit or citation exists.

### Mode 2: Semantic Project Wiki Retrieval

Mode 2 is used when `ProjectLocalWikiOrchestrator` answers user questions in
Phase 2. It needs semantic and graph-aware context selection.

Implementation contract:

- use `GraphRag.HybridRetriever` from `C:\Projects\llm_mvp_core\08.GraphRag`;
- require an `EmbeddingModel` dependency from the model/embedding project that
  is actually connected in the current solution, either project 07 or project
  09 depending on the local tree;
- combine graph relations from:
  - `relations/documentation-units.jsonl`;
  - `relations/call-edges.jsonl`;
  - `relations/value-flow-edges.jsonl`;
  - Mode 3 evidence hits from `SearchEngineFacade`;
- if the embedding dependency is missing, Mode 2 must not silently fall back
  and pretend to be semantic search. It must report
  `semantic_retrieval_unavailable` and answer only with Mode 3 evidence or
  abstain.

### WikiContextRetriever

`WikiContextRetriever` lives in `42.ProjectWikiDomain` and owns the routing
between these retrieval paths. `ProjectEvidenceIndex` is an internal dependency
of `WikiContextRetriever`; it is not an external pre-orchestrator service.
It executes the query steps produced by `ProjectWikiQueryPlanningFlow`; it must
not invent an investigation plan by itself.

```text
Mode 3 deterministic evidence lookup
  -> ProjectEvidenceIndex
  -> SearchEngineFacade
  -> evidence-index.jsonl BM25

Mode 2 semantic/wiki answering
  -> GraphRag.HybridRetriever
  -> EmbeddingModel
  -> relation graph + BM25 evidence hits
```

No other component should call `SearchEngineFacade`, `ProjectEvidenceIndex`, or
`HybridRetriever` directly for project-local wiki answers.
`ProjectLocalWikiOrchestrator` should call `ProjectWikiQueryPlanningFlow`, pass
the plan to `WikiContextRetriever`, then pass the retrieved context to
`ProjectLocalWikiModelUnit` and `ProjectAnswerVerifier`.

## Documentation Level Names

The plan uses these documentation levels:

- Level 0: project-domain routing decision.
- Level 1: symbol wiki, one method/class/file.
- Level 2: caller/component wiki, one caller or component boundary.
- Level 3: workflow wiki, multi-method use-case path.
- Level 4: feature/system wiki, feature boundary or externally visible system
  surface.

If implementation rows use compact codes such as `D3` or `D4`, they must map to
these names:

- `D3`: Level 3 workflow documentation.
- `D4`: Level 4 feature/system or caller-chain documentation.

## Prerequisite: Base Wiki Model

`xiigen-small-doc-wiki-base` is not assumed to already exist. It is a required
pre-project prerequisite.

Before any project-local adapter is trained, the system must train and validate
the base wiki model:

```text
xiigen-small-doc-wiki-base
  -> learns generic documentation Q/A behavior
  -> learns citation discipline
  -> learns abstention when evidence is missing
  -> learns stack/version awareness
  -> learns wrong-project rejection
```

Base model acceptance gate:

- generic documentation Q/A accuracy >= 0.90;
- citation format accuracy >= 0.98;
- missing-evidence abstention accuracy >= 0.95;
- hallucination rate == 0.00 on held-out adversarial questions;
- can consume `evidence-index.jsonl` rows;
- can answer from symbol/workflow/feature/system docs without project-specific
  weights.

Only after this gate passes may project-local adapters be trained.

## Four Learning Levels

### Level 0: Project Domain Router

Goal: decide whether a prompt belongs to this project wiki.

Chosen behavior:

- detect project-specific names, classes, controllers, methods, workflows, and
  stack/version vocabulary;
- route matching questions to the project-local wiki unit;
- reject unrelated project, generic .NET, translation, or orchestration-only
  questions.

Rejected examples:

- route a GenieDynamicServer question to askapediaweb wiki;
- route a generic C# syntax question to the project wiki;
- answer without checking whether the project wiki exists.

Metrics:

- project routing accuracy;
- cross-project isolation accuracy;
- unknown-project abstention accuracy.

### Level 1: Symbol Wiki

Goal: answer questions about one method/class/file using generated bottom-up
documentation.

Input authority:

- generated method documentation;
- source locator;
- stack/version fields;
- callee summaries for Level 3 workflow and Level 4 feature/system rows.

Chosen behavior:

- answer from the documented symbol;
- cite the source symbol and documentation path;
- preserve target framework and C# language version;
- separate local behavior from delegated behavior.

Rejected examples:

- answer from source code that was not documented;
- ignore target framework;
- merge caller and callee behavior into one invented sentence.

Metrics:

- answer correctness;
- citation coverage;
- stack/version accuracy;
- local-vs-delegated separation accuracy.

### Level 2: Caller/Component Wiki

Goal: answer questions about one caller or one component boundary using accepted
method and callee documentation.

Input authority:

- caller documentation;
- component documentation;
- accepted method summaries;
- callee propagation evidence;
- source evidence graph.

Chosen behavior:

- explain the caller or component boundary;
- list participating methods/callees;
- preserve the bottom-up evidence order;
- state missing evidence when a callee edge is not documented.

Rejected examples:

- top-down component explanation without internal callee evidence;
- answer using only controller names;
- skip internal helper semantics that change the component meaning.

Metrics:

- caller/component score;
- bottom-up propagation score;
- missing-evidence abstention score.

### Level 3: Workflow Wiki

Goal: answer questions about workflows/use cases that cross multiple callers or
components.

Input authority:

- workflow documentation;
- Level 4 caller-chain summaries;
- callee propagation evidence;
- source evidence graph.

Chosen behavior:

- explain the user-visible workflow;
- list participating methods/components;
- preserve the bottom-up evidence order;
- state missing evidence when a workflow edge is not documented.

Rejected examples:

- top-down workflow explanation without internal callee evidence;
- answer using only controller names;
- skip internal helper semantics that change the workflow meaning.

Metrics:

- workflow/use-case score;
- bottom-up propagation score;
- missing-evidence abstention score.

### Level 4: Feature/System Wiki

Goal: answer higher-level project questions using feature-boundary and
system-boundary documentation.

Input authority:

- feature-boundary documentation;
- system-boundary documentation;
- project stack/version manifest;
- accepted workflow docs.

Chosen behavior:

- explain what the feature/system surface does;
- show which workflows support it;
- avoid architecture claims not present in docs;
- separate actual legacy behavior from desired refactor ideas.

Rejected examples:

- describe an ideal architecture instead of actual project behavior;
- hallucinate missing modules;
- reuse facts from another project;
- answer from old documentation after the source commit changed.

Metrics:

- feature-boundary score;
- system-boundary score;
- hallucination rejection score;
- stale documentation rejection score.

## Project Artifacts

## Integration With Existing Model Infrastructure

This plan extends the existing distributed model architecture. It must not
create a parallel manifest system.

Existing contracts:

- `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IModelRepository.cs`
- `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IRemoteUnitLocator.cs`
- `C:\Projects\llm_mvp_core\25.ModelServer\Remote\RemoteUnitManifest.cs`
- `C:\Projects\llm_mvp_core\26.TrainableRouter\Contracts\ITrainableRoutingFlow.cs`

Decision:

- `model-manifest.json` is the project-local view of `RemoteUnitManifest`.
- The promoted wiki unit must be published through `IModelRepository`.
- The main orchestrator locates it through `IRemoteUnitLocator`.
- `ProjectWikiRoutingFlow` extends the routing pattern in `26.TrainableRouter`;
  it does not replace `ITrainableRoutingFlow`.
- `ProjectWikiQueryPlanningFlow` is the next trainable flow after routing. It
  extends the same distributed model pattern but owns investigation planning,
  not project selection and not answer composition.
- New implementation code should use project slot `42.ProjectWikiDomain`
  unless that slot is already taken when implementation begins.

Certification lifecycle:

- portability status must be recorded in `model-manifest.json`;
- tenant/project certification tier must be recorded in `domain-manifest.json`;
- authentication status must be explicit: `required`, `deferred`, or `not_applicable`;
- no project-local wiki unit becomes `ACTIVE` until model-server publication,
  router integration, documentation acceptance, and Q/A evaluation all pass.

### `domain-manifest.json`

Required fields:

```json
{
  "project_id": "askapediaweb",
  "repository_root": "C:/Projects/askapediaweb",
  "source_commit": "local-validation-only",
  "documentation_version": "doc-batch-860",
  "wiki_model_unit_id": "wiki-askapediaweb-v1",
  "base_model_id": "xiigen-small-doc-wiki-base",
  "adapter_type": "lora",
  "status": "PAIRSET_READY",
  "source_authority": "first_party_internal_or_customer_approved",
  "tenant_project_certification_tier": "LOCAL_VALIDATION_ONLY",
  "authentication_status": "deferred",
  "allowed_source_roots": ["C:/Projects/askapediaweb"],
  "blocked_source_roots": ["C:/Projects/GenieDynamicServer"]
}
```

### `model-manifest.json`

Required fields:

```json
{
  "unit_id": "wiki-askapediaweb-v1",
  "unit_type": "project_local_wiki",
  "project_id": "askapediaweb",
  "weight_artifact_ref": "model-artifacts/wiki-askapediaweb-v1",
  "weight_checksum": "sha256:<artifact-checksum>",
  "local_cache_path": ".xiigen/wiki-domain/weights/wiki-askapediaweb-v1",
  "remote_unit_manifest_id": "wiki-askapediaweb-v1",
  "portability_status": "LOCAL_ONLY_UNTIL_PUBLISHED",
  "documentation_source": ".xiigen/wiki-domain/documentation",
  "evidence_index": ".xiigen/wiki-domain/evidence/evidence-index.jsonl",
  "question_database": "project-wiki-questions",
  "question_bank_export": ".xiigen/wiki-domain/question-bank/question-bank-export.jsonl",
  "accepted_batches": ["batch-001"],
  "heldout_accuracy": 0.0,
  "citation_accuracy": 0.0,
  "abstention_accuracy": 0.0,
  "created_at": "ISO-8601"
}
```

`source_path` fields are relative to `repository_root` from
`domain-manifest.json`.

### `evidence-index.jsonl`

One row per documented source unit:

```json
{
  "evidence_id": "askapediaweb::Campaign.GetCampaign",
  "doc_path": ".xiigen/wiki-domain/documentation/Campaign.GetCampaign.md",
  "source_path": "askapedia/Askapedia/BL/Campaign.cs",
  "documentation_level": "D4",
  "known_callees": ["AnswerExtensions.ToViewModel", "CampaignExtensions.ToViewModel"],
  "target_framework": "v4.6.1",
  "csharp_language_version": "CSharp5"
}
```

## Training Data Format

Every project-local wiki batch has exactly 50 train rows and 50 held-out rows.
Fifty is the batch size, not the full stream size.

The 50/50 split is intentional for the early project-local wiki phase. It is a
diagnostic ratio, not the final production training ratio. The local wiki model
is learning project facts from a small evidence surface; early overfitting and
cross-project leakage are more dangerous than slow data accumulation. Therefore
each 50-row training step is paired with an equally large disjoint held-out set.

After a project wiki passes 10 batches with stable metrics, the ratio may move
to a larger training share, for example 50 train / 25 held-out or 100 train / 50
held-out, but only if:

- citation accuracy remains >= 0.98;
- cross-project isolation remains 1.00;
- hallucination rate remains 0.00;
- no stale-document acceptance appears.

### Phase 2 Question Row Shape

```json
{
  "question_row_id": "WIKI-ASK-Q-001-0001",
  "project_id": "askapediaweb",
  "level": "L2_WORKFLOW",
  "question": "How does Campaign.GetCampaign build the campaign view model?",
  "expected_answer": "It loads Campaign with Question and Answers, then returns CampaignExtensions.ToViewModel(...).",
  "expected_evidence_ids": [
    "askapediaweb::Campaign.GetCampaign",
    "askapediaweb::CampaignExtensions.ToViewModel",
    "askapediaweb::AnswerExtensions.ToViewModel"
  ],
  "expected_answer_type": "workflow_explanation",
  "must_cite": true,
  "must_abstain_if_missing_evidence": false,
  "source_commit": "local-validation-only"
}
```

This shape is for questions only. It has no rejected answer. Phase 1 negative
DTO rows are defined in the `Phase 1 Negative DTO Documentation Orchestrator`
section and must stay separate.

## Training Cycle

### Step 1: Documentation Freeze

Before training a wiki batch, freeze the current documentation version:

```text
documentation_version = doc-batch-NNN
source_commit = current project source hash or local-validation-only
```

No wiki training row may mix documentation from different frozen versions unless
the row is explicitly a stale-document test row.

### Step 1.5: Documentation Acceptance Gate

Documentation is not allowed to enter wiki training until it passes the
documentation acceptance gate.

Required gate:

```text
documentation_quality_score >= 0.95
stack_version_score == 1.00
source_evidence_score == 1.00
bottom_up_propagation_score == 1.00
workflow_use_case_score == 1.00 for workflow docs
feature_boundary_score == 1.00 for feature docs
system_boundary_score == 1.00 for system docs
hard_failure_count == 0
```

Only accepted documentation may be copied to:

```text
.xiigen/wiki-domain/documentation/
.xiigen/wiki-domain/evidence/evidence-index.jsonl
.xiigen/wiki-domain/relations/documentation-units.jsonl
.xiigen/wiki-domain/relations/call-edges.jsonl
.xiigen/wiki-domain/relations/value-flow-edges.jsonl
```

Rejected documentation must go to:

```text
.xiigen/wiki-domain/eval/rejected-documentation-ledger.jsonl
```

and may not produce wiki training or question rows.

### Step 2: Build Phase 1 Relation Rows And Negative DTOs

For every accepted documentation unit:

1. write or update `documentation-units.jsonl`;
2. write direct caller/callee links to `call-edges.jsonl`;
3. write argument and return-value propagation to `value-flow-edges.jsonl`;
4. create 3-5 plausible negative documentation DTO candidates;
5. verify negatives against accepted docs and evidence;
6. write accepted negatives to the documentation training batch;
7. write failed negatives to `negative-dto-repair-required.jsonl`.

The relation rows are not optional. They are what lets later documentation move
from "this function does X" to "this caller does Y because it invokes X, sends
these values, receives that result, and uses it this way."

### Step 3: Train Phase 1 Local Documentation Wiki Adapter

Train the project-local documentation adapter from:

- accepted documentation DTOs;
- relation graph rows;
- validated negative documentation DTOs.

No question rows are required in this phase.

### Phase 2 Step 1: Build Question Candidates

Generate or update question database candidates from:

- symbol docs;
- caller/workflow docs;
- feature/system docs;
- source evidence index;
- user-style prompts collected during documentation review.

Use the canonical question taxonomy from the `Phase 2 Trainable Question Bank`
section. Examples:

- `symbol_question`: "What does this method do?"
- `evidence_question`: "Where is this behavior implemented?"
- `caller_question`: "Why does this controller call this helper?"
- `workflow_question`: "What happens when this workflow runs?"
- `version_question`: "Which version/framework constraints matter here?"
- `wrong_project_question`: "Can this project answer this question?"

The trainable question bank then selects the exact 50 train seeds and 50
held-out seeds. Selection must consider utility score, under-covered evidence,
previous held-out failures, and train/held-out leakage prevention.

### Phase 2 Step 2: Build 50 Question Rows

Every Phase 2 question row has two trainable parts:

1. query planning: `ProjectWikiQueryPlanningFlow` produces the evidence
   investigation plan;
2. answer composition: the local wiki answers only from evidence retrieved by
   the accepted plan.

For every row:

- `expected_query_plan` must list the required query steps and sufficiency
  checks from `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`;
- `expected_answer` must be derived from accepted documentation;
- evidence ids must exist in `evidence-index.jsonl`;
- cross-project leakage is forbidden;
- source/version mismatch is forbidden;
- if evidence is missing, the expected behavior must be abstention.

Process:

1. Select a question from the trainable question bank.
2. Build the expected query plan: intent, entities, query steps, evidence
   types, and sufficiency checks.
3. Build the expected answer or abstention from accepted documentation.
4. Attach expected evidence ids and expected query-plan evidence categories.
5. Write the row to the train or held-out export.
6. Update the question selection ledger.

No Phase 1 negative DTO generator, verifier, or ledger is used in this
question-row path.

### Phase 2 Step 3: Train Or Evaluate Project Q/A Behavior

When Phase 2 is active, train or evaluate only the project-local Q/A behavior
from question rows. Query planning and answer composition are separate unit
metrics even when trained in the same 50-row cycle. This does not train the
Phase 1 negative DTO generator.

```text
base model: xiigen-small-doc-wiki-base
adapter: project wiki LoRA/adapters
output: .xiigen/wiki-domain/weights/<unit-id>/
```

Current runner:

```powershell
dotnet run --project "C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\InternalTrainReadyDpoRunner.csproj" -- --source "<project-wiki-train-jsonl>" --run-id "<wiki-run-id>" --context 96 --batch-size 16 --save-every 25
```

If `42.ProjectWikiDomain` later provides a dedicated wiki runner, it must expose
the same minimum metrics: query plan accuracy, required touchpoint recall,
multi-hop sequence accuracy, answer accuracy, citation accuracy, abstention
accuracy, project isolation accuracy, hallucination rate, CPU, RAM, elapsed
time, and adapter artifact path.

The base model is shared. The project facts live in the adapter and evidence
index. This keeps the model small and allows each project wiki to be moved with
the project.

### Phase 2 Step 4: Run Held-Out Q/A Evaluation

Every 50-row batch must evaluate 50 held-out questions.

Metrics:

- `query_plan_accuracy`;
- `required_touchpoint_recall`;
- `multi_hop_sequence_accuracy`;
- `missing_evidence_detection`;
- `irrelevant_query_rate`;
- `answer_accuracy`;
- `citation_accuracy`;
- `project_isolation_accuracy`;
- `stack_version_accuracy`;
- `workflow_answer_accuracy`;
- `feature_boundary_accuracy`;
- `system_boundary_accuracy`;
- `abstention_accuracy`;
- `hallucination_rate`;
- `cpu_ms_per_question`;
- `elapsed_ms_per_question`;
- `peak_ram_mb`;
- `adapter_size_mb`.

Escalation threshold:

- `confidence >= 0.85` and citations pass verifier: answer locally;
- `0.65 <= confidence < 0.85`: return cautious answer plus "needs review" flag,
  unless the user requested a definitive answer;
- `confidence < 0.65`: abstain or escalate to a larger AI;
- any missing required evidence id: abstain, do not answer;
- any project mismatch: route to the correct project wiki or abstain;
- any source commit/documentation version mismatch: trigger stale-doc workflow,
  do not answer as current truth.

Promotion gate:

```text
answer_accuracy >= 0.90
citation_accuracy >= 0.98
project_isolation_accuracy == 1.00
hallucination_rate == 0.00
abstention_accuracy >= 0.95
no hard failures
```

### Phase 2 Step 5: Register Local Wiki Orchestrator

After a batch passes:

1. update `model-manifest.json`;
2. mark accepted batch id;
3. write evaluation report;
4. expose the unit as `ProjectLocalWikiOrchestrator`;
5. update the global router candidate list with this project-local unit.

## Query Flow

```text
Question about code
  -> Main IOrchestrator
  -> ProjectWikiRoutingFlow
  -> project match?
       yes -> load ProjectLocalWikiOrchestrator
       no  -> route to generic documentation/orchestration unit
  -> ProjectLocalWikiOrchestrator
       -> ProjectWikiQueryPlanningFlow creates ProjectWikiQueryPlan
          -> intent, entities, required query steps, sufficiency checks
       -> WikiContextRetriever executes the planned steps and selects Mode 2 or Mode 3
          -> ProjectEvidenceIndex is used internally for Mode 3 evidence ids
       -> ProjectLocalWikiModelUnit answers
       -> ProjectAnswerVerifier checks citations and project boundary
  -> final answer with evidence ids
```

## Answer Contract

Every answer from the local wiki must include:

- short answer;
- evidence ids;
- source/documentation paths;
- confidence;
- "missing evidence" note when applicable;
- project id;
- documentation version.

Example:

```json
{
  "project_id": "askapediaweb",
  "documentation_version": "doc-batch-860",
  "answer": "Campaign.GetCampaign loads a campaign with its question and answers, then maps it to a CampaignViewModel.",
  "evidence_ids": [
    "askapediaweb::Campaign.GetCampaign",
    "askapediaweb::CampaignExtensions.ToViewModel"
  ],
  "confidence": 0.92,
  "missing_evidence": []
}
```

## Test Projects And Source Consent

Initial project-local wiki domains should be created for:

- `C:\Projects\wizGameserver`
- `C:\Projects\GenieDynamicServer`
- `C:\Projects\geniesoftGameMVPServer`
- `C:\Projects\genietasks`
- `C:\Projects\askapediaweb`

Each project must train independently. Cross-project questions are Phase 2
boundary/evaluation rows. They are not Phase 1 negative documentation DTOs.

Source consent rule:

- These paths are local validation projects only.
- Before project-local wiki weights are trained from a customer project, the
  project owner must approve local training on that code/documentation.
- If a project is first-party/internal, record that in
  `domain-manifest.json.source_authority`.
- If consent is missing, documentation may be generated for local review, but no
  project-local wiki weights may be trained.

## Acceptance Gates

### Phase 1 Documentation Batch Gate

Every batch:

- starts from one orchestrated documentation/wiki request;
- returns a populated `ProjectDocumentationKnowledgeGraph`;
- updates the project code trace;
- documentation acceptance gate passed for every source doc;
- `documentation_quality_score >= 0.95`;
- relevant level scores equal 1.00 for the docs used in the batch;
- all evidence ids exist;
- every documentation unit records code symbol, meaning, location, accepted
  parameters, callers, and return value;
- caller/component/workflow docs record callees, callee meaning, arguments sent,
  returns received, and how returns are used;
- the in-memory documentation knowledge graph is internally consistent;
- persisted JSONL files, if written, are snapshots only and are not the gate;
- no `chosen == rejected`;
- every rejected has `[WRONG: ...]`;
- every rejected is a documentation negative DTO, not a question row;
- no question-bank rows are required for Phase 1;
- all metrics recorded.

### Phase 2 Q/A Batch Gate

Every Phase 2 Q/A batch:

- 50 question train rows;
- 50 held-out question rows;
- all selected questions come from the trainable question bank;
- question rows contain expected answers or expected abstention behavior;
- question rows do not contain rejected answer fields;
- no cross-project accepted answer;
- no stale documentation accepted answer;
- all metrics recorded.

### Ten-Batch Gate

Every 10 batches:

- run held-out Q/A suite for all active project wiki domains;
- run documentation acceptance gate for new docs before ingestion;
- run full-project coverage over all five validation projects;
- compare methods, call edges, value-flow edges, and question coverage against
  the previous trace snapshot;
- report missing methods, missing call edges, missing relation types, and the
  reason each area remains uncovered;
- run cross-project isolation suite;
- run stale documentation suite;
- report CPU/RAM/time per row and per question;
- report adapter size growth;
- if coverage improved, create the next 50-row batch from the remaining
  weakest gap;
- if coverage did not improve, stop blind training and analyze the trace gap
  before creating the next batch.

### Project Promotion Gate

A project-local wiki unit is usable by the main orchestrator only after:

- at least 500 train rows and 500 held-out rows;
- at least one full-project Q/A pass for every target project;
- documentation acceptance gate passed for the documentation version;
- citation accuracy >= 0.98;
- hallucination rate == 0;
- cross-project isolation == 1.0;
- no source/version hard failures.

## Why This Is Not Just RAG

RAG retrieves documentation. The local wiki model learns project-specific
question-answer behavior:

- how users ask about this project;
- which symbols answer which question;
- when to abstain;
- how to preserve bottom-up meaning;
- how to avoid cross-project leakage;
- how to cite the correct evidence.

The evidence index remains mandatory. The model does not replace source
evidence. It learns how to use project documentation correctly.

## Storage Rule

The adapter weights are stored in the project because the wiki is part of the
project's operational knowledge:

```text
project/.xiigen/wiki-domain/weights/
```

Final storage decision:

- manifests and evaluation ledgers live in git;
- weights are not committed to git;
- weights are stored through the model-server artifact pattern
  (`IModelRepository` / model artifact storage);
- `model-manifest.json` contains the artifact reference and checksum;
- local development may cache weights under `.xiigen/wiki-domain/weights/`, but
  that cache is a build artifact.

The base model may be shared on the model server. The project adapter manifest
travels with the project. If a company cannot send code to an external AI
service, the artifact storage and model server can run on-premise.

## Failure Modes

1. Cross-project leakage
   - Cause: wiki model sees similar controller names in another project.
   - Fix: add wrong-project Phase 2 question rows and, if documentation itself
     leaks project boundaries, add Phase 1 negative documentation DTO rows.

2. Source-free answer
   - Cause: model answers from memory.
   - Fix: require evidence ids and reject unsupported answers.

3. Stale documentation
   - Cause: source changed after documentation freeze.
   - Fix: source commit mismatch blocks answer or triggers re-documentation.

4. Top-down invention
   - Cause: model writes architecture from class names.
   - Fix: require bottom-up callee/workflow evidence.

5. Version hallucination
   - Cause: model assumes modern .NET.
   - Fix: stack/version rows and rejected examples for wrong framework.

## First Implementation Sequence

Phase 1:

1. Train and validate `xiigen-small-doc-wiki-base`.
2. Create `.xiigen/wiki-domain/` scaffold for one project, preferably
   `askapediaweb` because it has rich legacy .NET Framework transit coverage.
3. Run the documentation acceptance gate.
4. Copy accepted generated documentation from the latest documentation quality
   run into `documentation/`.
5. Build `evidence-index.jsonl`, `documentation-units.jsonl`,
   `call-edges.jsonl`, and `value-flow-edges.jsonl` from accepted documentation.
6. Generate and verify Phase 1 negative documentation DTOs.
7. Train the local documentation wiki adapter from accepted documentation DTOs,
   relation graph rows, and validated negative documentation DTO rows.
8. Register the local documentation wiki model as a candidate unit.

Phase 2:

9. Insert/update question candidates in the separate question database from
   accepted docs and relation graph rows only.
10. Export selected question seeds for `batch-001` with 50 train and 50 held-out
    wiki Q/A rows.
11. Run question-row structural gates.
12. Train or evaluate Q/A behavior with the runner command defined in the
    Phase 2 training step, or with the dedicated `42.ProjectWikiDomain` runner
    only after it exposes the same metrics contract.
13. Run held-out project Q/A evaluation.
14. Ask 20 real project questions and verify citations through
    `ProjectAnswerVerifier` using the real-question verification rule below.
15. Repeat in 50-row batches until the project wiki reaches promotion gate.

Real-question verification rule:

- all 20 answers must cite at least one existing evidence id;
- at least 19 of 20 answers must match the expected answer type;
- 20 of 20 answers must stay inside the correct project;
- any missing evidence must trigger abstention, not hallucination;
- failure writes a row to `eval/real-question-failures.jsonl`.

## Phase 2 Next Question Data To Build

For the first 50-row project wiki batch:

| Count | Question seed type(s) |
|---:|---|
| 8 | `symbol_question` |
| 6 | `caller_question` |
| 6 | `workflow_question` |
| 5 | `feature_question` |
| 5 | `system_boundary_question` |
| 5 | `version_question` |
| 5 | `evidence_question` |
| 4 | `missing_evidence_question` |
| 3 | `wrong_project_question` |
| 3 | `wrong_level_question` |

Every row in this 50-row batch must include an expected
`ProjectWikiQueryPlan`; the answer part is not accepted unless the plan part
passes first. Held-out must mirror the same distribution but use different
symbols/questions.

## Current Open Decisions

1. Whether the first adapter is stored as LoRA, delta weights, or another small
   trainable head.
2. Whether project-local wiki inference runs inside the same process as the
   orchestrator or through `RemoteLogicalUnitProxy`.
3. Whether the first Q/A suite should start with `askapediaweb` only or all five
   validation projects in parallel.

Recommended first choice:

- start with `askapediaweb`;
- store manifest and training/eval data in git;
- store weights in model artifact storage through `IModelRepository`, with a
  local cache only when needed;
- load through the same model-server pattern used by distributed model units.

## Base Wiki Training Plan Content

`base-wiki-training-plan.md` is not free-form prose. It must contain:

- base model id;
- approved source families;
- train/held-out split;
- pair taxonomy;
- runner command;
- expected metrics;
- acceptance gate;
- artifact output paths;
- reason why the base model contains no project-local facts.

## Concrete Model Repository Publication

The first implementation should use
`C:\Projects\llm_mvp_core\25.ModelServer\Local\LocalModelRepository.cs` as the
concrete `IModelRepository` implementation unless a remote artifact repository
is configured.

Publication call:

```text
LocalModelRepository.UploadAsync(manifest, serverBaseUrl, ct)
```

The returned `RemoteUnitManifest` becomes the source for
`model-manifest.json.remote_unit_manifest_id`, `weight_artifact_ref`, and
`weight_checksum`.
