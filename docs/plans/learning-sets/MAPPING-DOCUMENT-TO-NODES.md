# Learning Set — Master Mapping: Source Documents to UML Flow Nodes

## What This Document Is

This document traces every major piece of knowledge from the xiigen architecture documents and product sessions to the UML flow node that should be trained on it. It is the basis for creating the individual learning set batches.

The UML diagram ("improve algo.drawio.xml") shows a flow of specialized language model units. Each unit does one job. Each unit is trained on a specific set of examples. This document defines which examples go to which unit.

---

## The UML Flow — Node Reference

The flow runs left to right. Reading the diagram, there are nine active processing units and one data store:

| Node ID | Name Used Here | UML Label |
|---------|---------------|-----------|
| NODE-01 | Domain Splitter | "LLM unit to split user prompt to domains d1...dm" |
| NODE-02 | Domain Translator | "LLM Unit to Work With user prompt and translate it to domain language" |
| NODE-03 | Decomposer | "LLM Unit to configure scheme plan A → db1 db2 db3" |
| NODE-04 | Orchestrator | "LLM Unit to process db1...dbn" |
| NODE-05 | Applier | "LLM unit to apply on fbi(bi) → Rq..i" (with self-loop) |
| NODE-06 | Result Translator | "LLM unit to translate Rb1..n → user language domain" |
| NODE-07 | Formatter | "LLM unit to format translated results into one prompt in user 'likable' form" |
| NODE-08 | Arbiter Selector | "Unit that decides which arbitrs are required" |
| NODE-09 | Arbiter Units | "Arbitr units" |
| DATA-01 | Prompt Library | "promts b1..bn / transformational prompts: fb1...fbn" (rounded box = data, not a processing unit) |

There are also domain-specific shadow boxes below each node (for example "Domain orchestration in db1...dbn"). These represent the domain-specialized versions of each node — the same node trained specifically for one domain. The learning sets created here feed both the general node and the domain-specific versions.

---

## Source Documents Examined

The following documents were read and are mapped below:

1. **DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md** — The primary design document for this UML. Directly names and describes each node.
2. **architecture-plan v1.md** (Older/) — First version of XIIGen architecture (Feb 2026). Figma-to-Code flow engine.
3. **architecture-plan-v2.md** (Older/) — Refined version. Named the project XIIGen. Added five skill areas.
4. **XIIGEN_ENHANCED_ARCHITECTURE_V2.md** (Older/) — Added six new capabilities, 28 skills total.
5. **XIIGEN_ENHANCED_ARCHITECTURE_v3.md** (Older/) — Went generic: IDatabaseService, IQueueService, IAiService interfaces.
6. **ARCHITECTURE_v4_USER_ALIGNED.md** (Older/) — Aligned with Luba's existing code patterns. Four microservices.
7. **XIIGEN_COMPLETE_ARCHITECTURE_PLAN.md** (Older/) — v5.0, 12 microservices, complete system.
8. **Product Session 1** (session_files/) — Compiler/interpreter research, then algorithm families, then planning the learning subprojects.
9. **Product Session 2** (session_files/) — Open source agent research (OpenCode, Aider, Cline), architectural comparisons.
10. **Product Session 3** (session_files/) — Session startup, OrchestratorModel + ContinuousLearning analysis, dataset collection plan.
11. **Product Session 4** (session_files/) — Skill adaptation work, translating XIIGen governance to LlmFromScratch.
12. **Product Session 5** (session_files/) — Another session startup, governance skill verification.

---

## The Mapping: Which Document Content Goes to Which Node

### NODE-01 — Domain Splitter

**What this node does:** Receives the raw user request and identifies which areas of the product the request involves. Produces a list of domain labels (d1...dm) and the portion of the user's request that belongs to each domain.

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Domain splitter. Takes the raw user prompt, emits a multi-label vector over candidate domains. Identifies which fragments of the prompt belong to which domains."

From **architecture-plan v1.md section 4, Step 1**:
> "Step 1: FigmaParser — parse nodes → raw CSS/HTML array"
Mapping rationale: The FigmaParser reads the incoming design and identifies what kind of element each part is (button, text, container, navigation). This is the original concrete version of what the Domain Splitter does — it reads the incoming request and categorizes its parts.

From **architecture-plan-v2.md section 3, FigmaParserExecutor**:
> "Parse figma node tree → raw CSS/HTML fragments"
Mapping rationale: Same as above. The parser separates the compound input into categorized fragments.

From **XIIGEN_ENHANCED_ARCHITECTURE_V2.md, three data flows**:
> "Simple Figma-to-Code flow: Parse nodes, separate by type"
Mapping rationale: The first step of the three described flows is always separation by type — which is what the Domain Splitter does.

From **Product Session 1, algorithm families discussion**:
> "Семья 3 — Формальные грамматики... Описываешь язык через формальную грамматику" (Family 3 — Formal Grammars: you describe the language through a formal grammar)
Mapping rationale: The Domain Splitter uses grammar-like recognition to identify domain signals in user language. The session research into open-source grammar-based tools (ANTLR4, Lark) represents the kind of recognition the Domain Splitter performs.

From **architecture-plan-v2.md, Feedback Injection Format**:
> "stepType: AiTransform... Query for relevant past feedback → injects into System Prompt"
Mapping rationale: The Domain Splitter must also account for feedback about past domain assignments. If a previous split assigned something to the wrong domain, the corrective feedback should inform future splits.

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 3, unit registry**:
> "FILLS (this cell is currently the active occupant of this role-domain slot)"
Mapping rationale: The Domain Splitter learns which domain slots currently have active cells — it only assigns to domains where processing is available.

**Learning set file:** NODE-01-DOMAIN-SPLITTER.md

---

### NODE-02 — Domain Translator

**What this node does:** Receives one fragment of the user's request plus its domain label. Produces the same request expressed in the formal language that the target domain uses internally. This is the translation from "what the user said" to "what the domain understands."

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Domain translator. Takes a user-prompt fragment plus a target domain, emits the same fragment expressed in the formal predicate language of that domain. This is the existing five-level SemanticCompiler pipeline, viewed as a unit."

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2, aggregator**:
> "Takes the set of per-task results... Handles cross-task consistency"
Mapping rationale: The Domain Translator must be consistent — the same concept expressed through two different domain translations must be reconcilable when results are aggregated. The translator therefore has an implicit responsibility for translation consistency across domains.

From **architecture-plan v1.md section 3, AiTransformStep**:
> "Send to AI Model A with: figma nodes, raw CSS/HTML, relevant past feedback, prompt template"
Mapping rationale: This is the first generation of the Domain Translator — it takes parsed nodes (domain fragments) and transforms them using a template (the domain's formal language template). The structure is the same.

From **architecture-plan-v2.md section 3, AiTransformExecutor**:
> "Build prompt + feedback context → call IAiProvider"
Mapping rationale: Building the prompt that performs the domain translation is the concrete implementation of what the Domain Translator does. The "prompt template" here is the domain language itself.

From **XIIGEN_ENHANCED_ARCHITECTURE_V2.md, AI Context Management**:
> "Builds context from previous jobs and similar past jobs"
Mapping rationale: The Domain Translator also uses historical translations to learn the formal language of each domain. Previous (input fragment, translated output) pairs are the training data for this node.

From **XIIGEN_ENHANCED_ARCHITECTURE_v3.md, IAiService**:
> "SendPromptAsync with domain-specific template"
Mapping rationale: Each domain has its own translation template. The Domain Translator selects the right template for the assigned domain and applies the user's fragment to it.

From **Product Session 3, SemanticCompiler analysis**:
> "Compression pipeline... RouteClassifier... domain routing"
Mapping rationale: The SemanticCompiler's five-level compression pipeline is the concrete implementation of domain translation in the existing codebase. The Domain Translator node is a wrapper around this pipeline that makes it addressable as a flow unit.

**Learning set file:** NODE-02-DOMAIN-TRANSLATOR.md

---

### NODE-03 — Decomposer

**What this node does:** Receives a domain-language expression (what the user wants, expressed in the target domain's formal terms). Produces a plan: an ordered set of concrete sub-tasks, along with the dependency relationships between them. Each sub-task will be handled independently.

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Decomposer. Takes a domain-language predicate sequence, emits an ordered or partially-ordered set of sub-tasks plus the transformational prompts that should be applied to each. Holds the dependency graph between sub-tasks."

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2, orchestrator**:
> "Orchestrator. Drives the execution of the decomposer's plan — topological scheduling, parallel execution where dependencies allow, iteration where dependencies cycle."
Mapping rationale: The Decomposer and Orchestrator are co-designed. The Decomposer produces the plan; the Orchestrator executes it. The plan must be structured so the Orchestrator can read it — sub-tasks with dependency labels.

From **architecture-plan v1.md section 4, Step 2**:
> "Step 2: AiTransform — send to AI Model A with figma nodes, raw CSS/HTML, prompt template"
Mapping rationale: In the original architecture, Model A was responsible for breaking down the parsed input into a plan of transformation steps. This is the first concrete version of the Decomposer.

From **architecture-plan-v2.md section 3, FlowDefinition**:
> "FlowDefinition: DAG: list of FlowStep + edges"
Mapping rationale: The FlowDefinition (Directed Acyclic Graph) is precisely what the Decomposer produces — a set of steps and the edges connecting them (dependencies).

From **architecture-plan-v2.md section 3, FlowOrchestrator**:
> "Walk DAG, resolve next step, delegate to executor"
Mapping rationale: The FlowOrchestrator is the Orchestrator (NODE-04). For it to walk the DAG, the Decomposer must have produced a valid DAG first.

From **XIIGEN_ENHANCED_ARCHITECTURE_v3.md, Flow Orchestrator Service**:
> "Manages flow execution DAG"
Mapping rationale: The DAG is the shared contract between Decomposer and Orchestrator.

From **Product Session 3, OrchestratorModel analysis**:
> "EpisodeDecomposer (splits tasks by conjunction pattern and domain)"
Mapping rationale: EpisodeDecomposer (from subproject 15.OrchestratorModel) is the concrete C# implementation of the Decomposer node. It uses conjunction patterns and domain labels to split a compound request into atomic sub-tasks.

From **Product Session 3, experiment results**:
> "E21 (BoundaryDetect, plus 143% boundary_accuracy)"
Mapping rationale: Boundary detection is the key skill of the Decomposer — finding the boundaries between sub-tasks in a compound request. E21's result validates the algorithm.

From **Product Session 3, experiment results**:
> "E32 (EpisodeDecomposer, plus 11% count_accuracy)"
Mapping rationale: Direct experiment validating the Decomposer algorithm. Count accuracy measures whether the right number of sub-tasks was identified.

From **architecture-plan v1.md section 6, Phase 2: Flow Engine Core**:
> "FlowDefinition graph model, FlowOrchestrator — DAG traversal, step resolution"
Mapping rationale: Building the DAG (Decomposer output) was identified as Phase 2 core work. The architectural prominence of this step reflects its centrality.

**Learning set file:** NODE-03-DECOMPOSER.md

---

### NODE-04 — Orchestrator

**What this node does:** Receives the plan produced by the Decomposer. Decides the execution order, identifies which sub-tasks can run at the same time (in parallel) and which must wait for others (sequential). Drives the actual running of each sub-task through the Applier.

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Orchestrator. Drives the execution of the decomposer's plan — topological scheduling, parallel execution where dependencies allow, iteration where dependencies cycle."

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 6**:
> "Asynchronous (default). The user request completes immediately using the closest existing cell as fallback. Training is queued."
Mapping rationale: The Orchestrator decides whether to run synchronously or asynchronously. The default behavior and the exception case are both decision points for this node.

From **architecture-plan v1.md section 4, Worker lifecycle**:
> "FOR EACH step in DAG order: Set step.status=Running, update status, resolve IStepExecutor, query feedback, build context, Execute step"
Mapping rationale: This is the concrete procedural description of what the Orchestrator does, written as a step-by-step sequence in the original architecture.

From **architecture-plan-v2.md section 3, FlowOrchestrator.cs**:
> "Walk DAG, resolve next step, delegate to executor"
Mapping rationale: Same as above — the Orchestrator walks the DAG structure (the Decomposer's output) and delegates each step to the appropriate executor (Applier).

From **architecture-plan-v2.md, design decisions table**:
> "API ≠ Worker. Separate Kubernetes deployments. API pod scales on request count. Worker pod scales on queue depth."
Mapping rationale: The Orchestrator is the "Worker" side — it holds the execution logic. The separation between receiving requests and executing them is a fundamental Orchestrator design decision.

From **XIIGEN_COMPLETE_ARCHITECTURE_PLAN.md, AI Worker Pool Service**:
> "Four instances. Parallel execution. Queue-driven."
Mapping rationale: The Orchestrator manages parallel execution across multiple worker instances. It must track which workers are busy and route sub-tasks accordingly.

From **Product Session 3, OrchestratorModel analysis**:
> "Topological scheduling, AgentLauncher, parallel execution where dependencies allow"
Mapping rationale: Topological scheduling (running tasks in dependency order) is the core algorithm. The AgentLauncher is the component that starts each sub-task.

From **Product Session 3, experiment results**:
> "E35 (DependencyDetector, plus 30% dependency_accuracy)"
Mapping rationale: Dependency detection is the skill that makes topological scheduling possible. E35 validates this capability.

From **Product Session 3, experiment results**:
> "E39 (QualityOrchestrator, plus 20% check_f1)"
Mapping rationale: The Orchestrator also runs quality checks. E39 validates the quality-check component of the orchestration.

**Learning set file:** NODE-04-ORCHESTRATOR.md

---

### NODE-05 — Applier

**What this node does:** Receives one sub-task plus the transformational prompt that applies to it. Runs the actual operation in the domain and produces a result for that sub-task. May repeat (self-loop in UML) if the result does not meet quality criteria.

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Applier. For one (sub-task, transformational-prompt) pair, runs the actual operation in the relevant domain and produces a per-task result. The unit cell loaded depends on the (applier-role, domain) pair, so each domain has its own specialized applier."

From **architecture-plan v1.md section 3, AiTransformStep**:
> "Send to AI Model A/B with figma nodes, raw CSS/HTML, relevant past feedback, prompt template"
Mapping rationale: This is the Applier. In the original architecture, Models A and B each took one sub-task and applied it. The "prompt template" is the transformational prompt; the "figma nodes, raw CSS/HTML" is the sub-task input.

From **architecture-plan-v2.md section 3, AiTransformExecutor**:
> "Build prompt + feedback context → call IAiProvider → StepResult"
Mapping rationale: The Applier builds its working prompt from the transformational template plus context, calls the processing unit, and produces a result.

From **architecture-plan-v2.md, StepContext**:
> "StepContext (TraceId, StepDefinition, PreviousStepOutput, FeedbackContext)"
Mapping rationale: The Applier receives a context package (what the Orchestrator assembled for it) and produces a result. The context includes what previous steps produced and any relevant feedback.

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN, UML self-loop on Applier**:
> "(The self-loop arrow on the diagram)"
Mapping rationale: The Applier can repeat. If the first attempt does not meet quality criteria, it runs again with adjusted parameters. The node must know when to stop iterating.

From **XIIGEN_ENHANCED_ARCHITECTURE_V2.md, AI Context Management**:
> "Enriching prompts with job history — pulls similar past successful results"
Mapping rationale: The Applier uses past successful results for similar sub-tasks to improve its output. This is the feedback-informed application.

From **architecture-plan-v2.md, Feedback Injection Format**:
> "Negative feedback → inject as explicit DO NOT corrections. Positive feedback → inject as CONTINUE doing reinforcements."
Mapping rationale: The Applier reads past feedback about how to apply transformations. This is specifically the Applier's feedback — not the whole flow's feedback.

From **architecture-plan v1.md section 8, key decisions**:
> "AI provider pattern — Strategy pattern. IAiProvider interface → easy swap/add models"
Mapping rationale: The Applier can use different AI models depending on the domain and sub-task type. The strategy pattern means the Applier selects the right model for the job.

**Learning set file:** NODE-05-APPLIER.md

---

### DATA-01 — Prompt Library (Transformational Prompts)

**What this is:** Not a processing unit but a data store. Contains the set of transformational prompts (b1...bn) and their function definitions (fb1...fbn). The Decomposer assigns prompts from this library to each sub-task. The Applier uses these prompts when executing sub-tasks.

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2, Decomposer**:
> "Emits an ordered or partially-ordered set of sub-tasks plus the transformational prompts that should be applied to each."

From **architecture-plan v1.md section 5, Skill 3: ai-prompt-orchestration**:
> "Prompt engineering patterns for multi-model pipeline... Prompt templates with {{placeholders}} for dynamic injection"
Mapping rationale: The prompt library is what this skill describes. The templates with placeholders are the transformational prompts.

From **architecture-plan-v2.md section 7, Skill 3**:
> "figma-to-code-prompts.md — Specific prompt templates for each step"
Mapping rationale: The prompt templates per step are the DATA-01 content.

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 3, dataset registry**:
> "Stores training corpus slices keyed by (unit-role, domain)... also stores the conformance suites"
Mapping rationale: The dataset registry contains the prompt library as one of its components.

**Learning set file:** DATA-01-PROMPT-LIBRARY.md (describes what training data populates this store, not the store's logic)

---

### NODE-06 — Result Translator

**What this node does:** Receives the domain-language results from all sub-tasks after the Applier has finished. Converts those results from the domain's formal language back into the user's natural language. This is the reverse of what NODE-02 (Domain Translator) did.

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Result translator. Takes the composed predicate-language result, emits the same content in the user's natural language. The reverse of the domain translator. Currently incomplete in the codebase — the forward path is built and tested, the reverse path needs its own treatment."

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN, open item O-6**:
> "The result translator role requires reverse-direction predicate-to-text generation, which the current codebase does not have. Is the design and training of the reverse path part of the scope?"
Mapping rationale: This is explicitly the gap to fill. The learning set for this node is particularly important because the current codebase has no examples yet.

From **architecture-plan-v2.md, AiReviewExecutor**:
> "Build review prompt → call IAiProvider → score/approve"
Mapping rationale: In the original architecture, the review step translated structured results back into human-readable evaluation. This is the closest existing analog to Result Translation.

From **Product Session 3, SemanticCompiler analysis**:
> "The reverse path: predicate sequence back to natural language explanation"
Mapping rationale: The session discussion explicitly identified that the reverse SemanticCompiler is needed and unbuilt. The learning set for this node must teach the translation from structured predicates back to human language.

From **XIIGEN_ENHANCED_ARCHITECTURE_V2.md, Documentation Generation**:
> "SystemAnalyzerService analyzing repositories, DocumentationGeneratorService generating architecture docs"
Mapping rationale: Generating documentation is a form of Result Translation — taking structured system knowledge and expressing it in human-readable form. These examples are training material.

**Learning set file:** NODE-06-RESULT-TRANSLATOR.md

---

### NODE-07 — Formatter

**What this node does:** Receives translated results in natural language (possibly from multiple domains). Combines them into a single, coherent, well-structured response that the user will want to read. Applies presentation conventions: appropriate structure, reading flow, completeness.

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Formatter. Takes the natural-language result, emits a polished version conforming to general user-presentation conventions (code blocks, headers, structure)."

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2, style adapter**:
> "Style adapter. Takes the formatted result, emits a version adapted to the specific user's learned style preferences (terse vs verbose, formal vs casual, code-first vs prose-first). The new unit."
Mapping rationale: The Style Adapter is an extension of the Formatter. The Formatter applies general conventions; the Style Adapter personalizes. Both feed the same learning set (NODE-07) with the distinction between general and personal formatting noted.

From **architecture-plan v1.md section 4, Step 4**:
> "AiReview — send combined results from steps 2-3, evaluation criteria, feedback history"
Mapping rationale: The review step in the original architecture received combined results and evaluated them as a whole — this is early Formatter logic.

From **architecture-plan-v2.md section 3, AiReviewExecutor**:
> "Score 0-100, pass threshold configurable"
Mapping rationale: The Formatter also has quality thresholds — it should not output a result that does not meet minimum quality requirements.

From **Product Session 1, open source feedback on code quality**:
> "The generated CSS uses absolute positioning for everything. Use flexbox instead."
> "Font sizes are in px. Convert to rem units."
Mapping rationale: These are examples of formatting corrections from real user feedback. The Formatter learns from this kind of feedback what "good format" means.

From **architecture-plan-v2.md section 3, Feedback Injection Format**:
> "Positive feedback → CONTINUE doing. Negative → explicit corrections."
Mapping rationale: The Formatter receives the same feedback mechanism and learns formatting preferences over time.

**Learning set file:** NODE-07-FORMATTER.md

---

### NODE-08 — Arbiter Selector

**What this node does:** Receives the final result plus its execution context. Decides which judges (from the Arbiter pool) should evaluate this particular result. Not all arbiters fire on every result — this node selects the right subset.

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Arbiter selector. Mirrors the domain splitter at the back of the pipeline. Takes a result plus its execution context, emits a multi-label vector over the arbiter pool — which K judges should fire on this result. Trained on the history of (result, which-arbiters-caught-issues) pairs."

From **architecture-plan-v2.md, design decision table**:
> "AiReview rubric scoring — pass/fail thresholds"
Mapping rationale: Deciding which review criteria apply to a given result is what the Arbiter Selector does. In the original architecture, all criteria applied to all results; in the dynamic flow, the Selector routes each result to the relevant criteria only.

From **Product Session 3, OrchestratorModel analysis**:
> "QualityOrchestrator with 16 checks"
Mapping rationale: QualityOrchestrator (from subproject 15) is the existing concrete implementation of multi-criteria evaluation. The Arbiter Selector is the routing layer above it that decides which of the 16 checks to run.

**Learning set file:** NODE-08-ARBITER-SELECTOR.md

---

### NODE-09 — Arbiter Units

**What this node does:** Evaluates a result against one specific quality criterion. Returns a verdict: acceptable, needs revision, or reject. Multiple arbiter instances run in parallel, each specialized for one concern (null safety, scope respect, factual accuracy, style coherence, etc.).

**Source lines and sections mapped here:**

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 2**:
> "Arbiter pool. A set of small trained judges, each specialized to one kind of judgment (null safety, tenant isolation, code quality, factual accuracy, style coherence, scope respect, etc.). Each arbiter is itself a cell. The pool grows as new failure types accumulate enough operational data to train a new arbiter on."

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 4, Path 1**:
> "Operational feedback (real-time). When a result is produced, the arbiter selector picks K arbiters, the arbiters return verdicts, the orchestrator either accepts the result or initiates a reroute."
Mapping rationale: This describes the Arbiter's runtime role. The reroute path leads back to the Domain Splitter (the loop-back arrow in the UML).

From **architecture-plan v1.md section 4, Step 4**:
> "AiReview — evaluation criteria, feedback history"
Mapping rationale: The original AiReview step is the single-arbiter version. The dynamic flow expands this to a pool of specialized arbiters.

From **architecture-plan-v2.md section 7, Skill 3, review-rubric.md**:
> "AiReview scoring criteria, pass/fail thresholds"
Mapping rationale: Each arbiter has a rubric — a specific set of criteria it applies. The rubric file content is training data for the Arbiter unit specialized in that domain.

From **Product Session 3, OrchestratorModel analysis**:
> "QualityOrchestrator with 16 checks: null safety, tenant isolation, scope respect, etc."
Mapping rationale: The 16 checks are the current version of the Arbiter pool. Each check becomes a separate trained Arbiter in the dynamic flow.

From **Product Session 3, experiment results**:
> "E39 (QualityOrchestrator, plus 20% check_f1)"
Mapping rationale: This experiment validates the quality-checking approach. The 20% improvement in f1 score shows that learned quality checking outperforms static rules.

From **DYNAMIC-FLOW-ARCHITECTURE-PLAN section 4, Path 1, reroute arrow**:
> "We may send some of fbx1...fbxn back to reprocessing with apx1...abxn"
Mapping rationale: When an Arbiter rejects a result, it provides a reprocessing prompt — the correction to apply. The Arbiter therefore produces two things: a verdict AND a corrective prompt for failed results.

**Learning set file:** NODE-09-ARBITER-UNITS.md

---

## Document Version History — Which Session Created Which Document

| Document | Approximate Date | Creating Session Evidence |
|----------|-----------------|--------------------------|
| architecture-plan v1.md | Feb 5, 2026 | Product Session 1 (open-source research → architecture planning) |
| architecture-plan-v2.md | Feb 5, 2026 | Product Session 1 (same day, refining the first version) |
| XIIGEN_ENHANCED_ARCHITECTURE_V2.md | Feb–March 2026 | Product Session 2 (extending capabilities after initial build) |
| XIIGEN_ENHANCED_ARCHITECTURE_v3.md | March 2026 | Product Session 2 or 3 (genericizing the architecture) |
| ARCHITECTURE_v4_USER_ALIGNED.md | April 2026 | Product Session 3 (alignment with existing code patterns) |
| XIIGEN_COMPLETE_ARCHITECTURE_PLAN.md | April 2026 | Product Session 3 (comprehensive plan for all 12 services) |
| DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md | May 2026 | Claude Code sessions (the current document, describes the UML nodes) |

---

## What Is Not Yet Mapped

The following content from the source documents did not map to a specific flow node — it represents shared infrastructure or cross-cutting concerns. It is not wasted; it belongs in the documentation that sits alongside the learning sets.

- **Fabric-first interfaces** (IDatabaseService, IQueueService, IAiService, IObjectProcessor) — These are the infrastructure each node uses, not the nodes themselves. They go into a separate INFRASTRUCTURE-CONTRACTS document.
- **Trust hierarchy tiers 1-5** — This is the metadata system for managing which cells are trustworthy. It feeds the unit registry, not any single flow node.
- **Versioning and rollback** — Cross-cutting concern. Every node benefits from it but it is not specific training content for any single node.
- **Event sourcing and provenance** — Same: infrastructure, not node-specific training content.
- **Synchronous vs asynchronous training escape hatch** — Orchestrator parameter, not a learning example.
- **Open items O-1 through O-6** — These are decisions to be made, not training content.

---

## Processing Order for Batch Creation

The batches will be created in flow order — first the node that fires first, then the next one, and so on:

1. **Batch 1**: NODE-01 Domain Splitter — first node to fire after user prompt
2. **Batch 2**: NODE-02 Domain Translator — second in flow
3. **Batch 3**: NODE-03 Decomposer — third in flow
4. **Batch 4**: NODE-04 Orchestrator — fourth in flow
5. **Batch 5**: NODE-05 Applier — fifth in flow (handles the actual domain work)
6. **Batch 6**: DATA-01 Prompt Library — data that feeds NODE-03 and NODE-05
7. **Batch 7**: NODE-06 Result Translator — sixth in flow
8. **Batch 8**: NODE-07 Formatter — seventh in flow
9. **Batch 9**: NODE-08 Arbiter Selector — eighth in flow (runs in parallel with formatter)
10. **Batch 10**: NODE-09 Arbiter Units — ninth in flow (pool of judges)

Each batch is its own file. Each batch is complete and testable independently.

---

## Self-Check Questions Before Writing Each Batch

Before writing each learning set file, these questions are checked:

1. Does every example in this file use only natural language — no code syntax, no technical shorthand?
2. Does every example explain the reasoning, not just the result — does the model know WHY it does what it does?
3. Are there at least three different phrasings of each core concept?
4. Does the learning set cover the failure cases — what the node should NOT do, not just what it should do?
5. Is the input described at the right level of specificity — detailed enough to train on, general enough to generalize?
6. Does the output tell the model exactly what to produce — not "decide" or "determine" but "produce a list of...", "state which...", "write out..."?
7. Have the examples been checked against what would happen if you simulated this node — would the flow produce the correct result?
