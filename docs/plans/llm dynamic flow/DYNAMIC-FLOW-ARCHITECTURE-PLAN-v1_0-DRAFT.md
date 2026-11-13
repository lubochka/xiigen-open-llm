# LlmFromScratch — Dynamic Flow Architecture Plan v1.0 (DRAFT)

**Status:** Draft for Luba's review. No implementation has started under this plan. Subproject numbering and registry-split decisions are proposed here and finalized at approval time.

**Scope:** This document defines the architecture of LlmFromScratch as a self-modifying network of small specialized internal LLM units operating over an editable flow graph. It establishes principles, names the cast of components, names the registries, names the feedback paths, and names the testing-philosophy shift the architecture requires. It deliberately does not contain schemas, implementation steps, file lists, or test counts — those belong in the documents that follow this one.

**What this document replaces:** The implicit "static eight-stage pipeline" assumption that has been carried in the existing subproject plans (14, 15, 16, 17). After approval of this document, those plans are not invalidated, but they get reframed as topologies-and-cells operating inside the dynamic flow runtime rather than as standalone monolithic subprojects.

**What this document does not replace:** The locked decisions D-07, D-08, D-09, D-11. The 22-predicate vocabulary (the seventeenth experiment). The Genie DNA and Fabric-First discipline from CLAUDE.md. The OperationResult<T> envelope. The BaseService(ISessionStorage) inheritance pattern. Those are foundational to LlmFromScratch and survive unchanged.

---

## 1. The Seven Principles

### Principle 1 — Universal Continuous Learning

Every unit in the system learns from operational feedback. There is no hardcoded judge, no boundary-code rule that gates unit behavior, no "use a static rule until enough data accumulates." Every unit starts with whatever priors it has from its initial training corpus and improves from observed outcomes.

The four-tier decision classifier (boundary-code, configurable, learnable, AI-generated) still applies, but the threshold for "boundary-code" is much narrower than in conventional systems. Only true definitions are immutable: the predicate vocabulary itself, the unit interface contract shape, the iron rules from CLAUDE.md, the locked experiment-derived decisions D-07/D-08/D-09/D-11. Everything else is learnable from day one — including decisions about which units to invoke, which arbiters to fire, whether two units are swappable, how to fall back on low confidence, when to retrain, when to roll back.

Decisions that would conventionally be "hardcoded with later upgrade to learning" are instead Q4-AI-pipeline decisions from day one. They become Q3-graph-RAG decisions automatically as confidence accumulates. The graph ages the AI-pipeline cost out on its own. No transitional code is written and later removed; the same edges in the same registry are read by either the AI-pipeline path (low confidence) or the direct lookup path (high confidence) depending on what the data supports.

### Principle 2 — Dynamic Flow Topology

The pipeline graph is data, not code. It is stored in its own database. The orchestrator reads the topology at runtime. Modifying the flow is editing the database, not writing a new method.

A topology is a contract document declaring nodes, edges, iron rules, and quality gates. Adding a new unit to the system is registering a unit cell and adding a node-edge pair to the topology. Replacing a unit is moving a pointer. Removing a unit is detaching its edges. Forking a flow for an experiment is copying its topology document and modifying the copy. Rolling back a flow change is restoring a prior topology version.

The flow graph supports modification at edge-level granularity from day one. This means the system can insert new nodes between existing nodes, add new edges, remove edges, replace nodes — but does not perform whole-subgraph replacements automatically. Subgraph-level rewriting may be added later if evidence supports it; cell-level (only contents-of-a-box change) is too restrictive because it cannot handle the case the user has already encountered ("we discovered we need to add an arbiter selector unit between two existing stages"), which is a structural edit, not a content swap.

### Principle 3 — Self-Similar Units

Every unit conforms to the same external interface: a predicate-shape input, a predicate-shape output, plus metadata. The interface is uniform regardless of what the unit does internally. A domain splitter and a result formatter look the same from the orchestrator's point of view. The orchestrator does not have N different code paths for N unit roles.

This uniformity is what enables hot-swap, what enables the swappability judge to operate over any pair, what enables the marketplace concept, what enables the gap-detection-and-fetch loop to install new units without recompiling the orchestrator. A new unit role is not a new orchestrator code path; it is a new entry in the registry with the same schema as every other entry.

The mirror pattern in the architecture — the domain splitter at the front emitting a multi-label vector over candidate domains, the arbiter selector at the back emitting a multi-label vector over candidate judges, the skill retriever in the middle emitting a multi-label vector over candidate transformations, the swappability judge emitting a verdict over candidate cells — is recognized as a single structural primitive: a small trained gate over a registry of candidates. Not four different unit kinds, one parameterized one.

### Principle 4 — Versioning and Rollback by Construction

Every unit cell has a version chain. Every flow topology has a version chain. Every training run has a version record. Every dataset slice has a version record. The current state of the system is a set of pointers into these chains. Rollback is moving the pointers. History is reading the chains backward.

Version chains are append-only. Cells, topology versions, and dataset slices that get superseded are not deleted; they are unlinked from the active pointers but preserved in the registry. This is universal across the architecture; it is not added per-feature.

This implies three storage shapes that are distinct and must not be conflated. The schema (what counts as a valid graph) lives as a contract document in code. The current state (the live, mutable graph) lives in the registry storage. The historical trail (the append-only log of every edit) lives in its own append-only event store. Three different artifacts, three different access patterns. The schema is read-only at runtime. The state is read-write but with versioning. The trail is append-only and never rewritten.

### Principle 5 — Internal AI Throughout

External AI providers are not part of this architecture. Every decision that requires intelligence is made by an internal trained unit resolved from the registry. There is no Claude call, no GPT call, no Anthropic API key, no OpenAI provider — neither in the runtime hot path nor in any background process the user is aware of.

The single exception is offline training-data preparation that may use external sources for the *content* (Stack Overflow, GitHub, Wikipedia, Microsoft Docs) but never for the *judgments*. Once data is downloaded, every decision about it — should it be kept, what does it teach, which slice does it belong to, is this answer correct — is made by internal trained units. External AI is not in any control path of the operating system.

This is what the locked decision D-11 (no external ML frameworks) means at the architectural level, extended from "no external libraries" to "no external intelligences."

**2026-05-04 qualification.** Later E51/E52 work keeps the internal-first rule
but narrows the prohibition. External AI is still not part of the ordinary
operational control path. It is accessible only through a named knowledge-edge
path after the local system has attempted the internal units first, can state
the specific missing competence, and receives explicit single-use user approval.
External answers are advisory material only. They must be adapted back into the
local logical structure, examined by internal arbiters, and stored as learning
signals before any future local model can learn from them.

### Principle 6 — Provenance End-to-End

Every unit installation, every graph edit, every dataset import, every training run, every cell promotion, every cell rollback, every confidence update is recorded with timestamp, source, parent state, child state, trust score at time of action, and the cause that initiated it (user, gap detector, scheduled retraining, marketplace fetch). The cost is small compared to the cost of debugging a self-modifying system without it. The substrate is the same event-sourcing pattern used by the FeedbackRouter and the GraphRAG store.

Provenance is full from day one. Bolting it on later is much more expensive than building it in. Every typed mutation API in the registries enforces the event-write side-effect; direct writes to storage are not allowed even for internal callers.

The user is able to trace any output back through every unit that contributed to it, every edge that routed it, every cell version that was active at the time, every dataset that trained those cells. The "examine" affordance you flagged in your third answer is enabled by this trail; without it, that affordance is not implementable.

### Principle 7 — Testing Adapts to a Self-Modifying System

Behavioral tests that expect a specific orchestration sequence become anti-patterns under this architecture. The sequence is data, the data is mutable, the test is asserting against a snapshot that may have changed since the test was written.

Three test categories stay viable and become the standard:

**Contract tests** — every unit at every boundary conforms to its declared input and output predicate shape and confidence calibration band, regardless of which cell is currently filling that slot. Run automatically on every cell installation as part of the conformance suite. The cell does not become active in the registry until its conformance suite passes.

**Invariant tests** — the graph never develops cycles, every node has at least one trained version, every edge has a confidence band within the valid range, every active cell has a non-empty training-corpus record, version chains are append-only, the FILLS pointer for every (role, domain) slot resolves to a real cell. Run continuously, alarm on violation.

**Replay tests** — given a recorded graph state at time T and a recorded input, the system reproduces the recorded output. This is the regression-test surface. New regressions add new (state, input, output) triples; rollback to a prior state for any reason is verified by replaying the test suite for that prior state.

The current LlmFromScratch test suite (877 tests at last baseline) is mostly behavioral. A non-trivial fraction will need re-classification — some become contract tests, some become invariant tests, some become replay tests, some are deleted because the behavior they pinned no longer exists. This re-classification is part of the work and is acknowledged here, not hidden in a later document.

---

## 2. The Cast of Unit Roles

The architecture has the following unit roles. Each role is filled by one or more cells per domain. A cell is a specific trained instance of a role — its weights, its training-corpus summary, its performance baseline, its version, its provenance. The roles are listed in approximate execution order through the pipeline; the selector and judge roles operate alongside the operational pipeline rather than as a stage in it.

**Domain splitter.** Takes the raw user prompt, emits a multi-label vector over candidate domains. Identifies which fragments of the prompt belong to which domains.

**Domain translator.** Takes a user-prompt fragment plus a target domain, emits the same fragment expressed in the formal predicate language of that domain. This is the existing five-level SemanticCompiler pipeline, viewed as a unit.

**Decomposer.** Takes a domain-language predicate sequence, emits an ordered or partially-ordered set of sub-tasks plus the transformational prompts that should be applied to each. Holds the dependency graph between sub-tasks.

**Orchestrator.** Drives the execution of the decomposer's plan — topological scheduling, parallel execution where dependencies allow, iteration where dependencies cycle.

**Applier.** For one (sub-task, transformational-prompt) pair, runs the actual operation in the relevant domain and produces a per-task result. The unit cell loaded depends on the (applier-role, domain) pair, so each domain has its own specialized applier.

**Aggregator.** Takes the set of per-task results, emits a single composed result in the predicate language. Handles cross-task consistency and reconciliation.

**Result translator.** Takes the composed predicate-language result, emits the same content in the user's natural language. The reverse of the domain translator. Currently incomplete in the codebase — the forward path is built and tested, the reverse path needs its own treatment.

**Formatter.** Takes the natural-language result, emits a polished version conforming to general user-presentation conventions (code blocks, headers, structure).

**Style adapter.** Takes the formatted result, emits a version adapted to the specific user's learned style preferences (terse vs verbose, formal vs casual, code-first vs prose-first). The new unit your ninth-step description introduced.

**Arbiter pool.** A set of small trained judges, each specialized to one kind of judgment (null safety, tenant isolation, code quality, factual accuracy, style coherence, scope respect, etc.). Each arbiter is itself a cell. The pool grows as new failure types accumulate enough operational data to train a new arbiter on.

**Arbiter selector.** Mirrors the domain splitter at the back of the pipeline. Takes a result plus its execution context, emits a multi-label vector over the arbiter pool — which K judges should fire on this result. Trained on the history of (result, which-arbiters-caught-issues) pairs.

**Swappability judge.** Takes a (current-cell, candidate-cell, observed-context) triple, emits a verdict — swappable, needs additional training, not swappable — with confidence. Operates over the unit registry. Used by the gap-detection-and-fetch loop and by the rollback machinery.

**Gap detector.** Watches for sustained low-confidence patterns at any node of the operational pipeline. When a pattern crosses threshold, initiates the create-or-fetch loop: query registry for nearest existing cell, query the local repo, query the marketplace (trust-scored), as a last resort kick off dataset discovery and training. Itself an internal unit, not a hardcoded rule.

**Dataset curator.** Takes raw operational feedback plus external corpus material, emits curated training slices keyed by (unit-role, domain). Lives in 16.AutonomousLearning and is the engine that produces the corpora cells are trained on.

The architecture is open. New roles get added by registering them. The existing roles are the cast we know we need; we expect more as operational evidence reveals gaps.

**2026-05-04 cast extension.** The high-level design amendment adds narrowing
and evidence roles before the original domain splitter: intake gate, system
topology router, sub-subject router, evidence pack selector, result convergence
gate, and budget/confidence gate. These roles keep units small by narrowing the
label world before local domain work begins. The logical structure compiler is
also a named pre-stage: it converts runtime prompts and dataset sources into
the shared predicate/meaning-graph structure before topology routing.

**2026-05-04 style-adapter note.** Unlike shared domain units, the style adapter
trains primarily on per-user preference history. Its storage, update cadence,
rollback, and privacy rules are therefore different from shared domain corpora.

---

## 3. The Four Registries

Each registry has its own storage, its own schema, its own access pattern, its own access API. They live in 08.GraphRAG using its substrate but each is a distinct collection.

**The flow registry.** Stores topology contracts — the nodes, edges, iron rules, and quality gates of every operational flow. Versioned. Append-only edit history. Editing a flow means writing a new version of its topology contract; the active version is a pointer. The orchestrator reads the active version at startup and on subscribe-to-changes.

**The unit registry.** Stores unit cells — weights, metadata, version chains, training-corpus references, performance baselines, conformance suite, trust score. Stores the relationships between cells: FILLS (this cell is currently the active occupant of this role-domain slot), SUPERSEDES (this cell replaced that one), SWAPPABLE_WITH (these cells are interchangeable per the swappability judge), TRAINED_FROM (this cell was trained on that dataset slice). The four edge types are learnable in their own confidence-and-observation-count sense.

**The decision graph.** Stores routing knowledge — entities like ARCHETYPE, ROLE, SCORE_BRACKET, CONDITION, edges like REQUIRES_MINIMUM, OPTIONAL_ARBITER, ROUTES_TO, AVOIDS, with confidence and observation counts and promotion paths from optional to required. This is the XIIGen-shaped graph for routing decisions, brought across to LlmFromScratch with the same schema patterns. Sibling to the unit registry, not merged.

**The dataset registry.** Stores training corpus slices keyed by (unit-role, domain), with provenance for every example (what source, when collected, what trust score, which user feedback signal generated it, which cell it has been used to train). Also stores the conformance suites attached to each cell — the recorded input-output pairs that any future cell claiming the same role-domain slot must reproduce within tolerance.

The four registries share an event-sourcing mechanism for their edit histories. Every typed mutation API on every registry writes the change to the corresponding event log. Direct writes to storage are forbidden by convention; the typed APIs are the only path.

---

## 4. The Feedback Paths

Three feedback paths run continuously through the system. They are recognized as distinct here because they have different semantics, different latencies, different audiences, and different effects on the registries.

**Path 1 — Operational feedback (real-time).** When a result is produced, the arbiter selector picks K arbiters, the arbiters return verdicts, the orchestrator either accepts the result or initiates a reroute (the loopback arrow on the diagram). This path runs at request time, milliseconds-scale. It does not directly update any registry; its outputs are recorded in the event log and consumed asynchronously by Path 2.

**Path 2 — Learning feedback (scheduled retrospective).** At configured intervals (idle-time-driven via the IdleDetector in 16.AutonomousLearning, or scheduled, or manual), the retrospective process reads the event log, attributes outcomes to the decisions that led to them, and updates: the confidence of routing edges in the decision graph, the trust score of cells in the unit registry, the conformance suite extensions for cells whose recent operations validated new (input, output) pairs, the training-corpus slices in the dataset registry that feed the next training cycle. The graph learns here, not in Path 1. This is the same two-step feedback pattern from the XIIGen confidence lifecycle.

**Path 3 — Gap-driven feedback (event-triggered).** When the gap detector observes a sustained low-confidence pattern at some node, this initiates: the create-or-fetch loop (search registry, search repo, search marketplace, search the web for a dataset, train a new cell, install it under probationary trust, promote it after observations accumulate). This path is asynchronous to user requests by default — a gap detected during user request T does not block T's completion; T uses the closest existing cell as fallback, and the new cell becomes available for T+N.

The user is aware of which path produced any visible effect — the system records this in the audit trail and surfaces it in the examination affordance. A user who wonders why a result changed between two requests can trace it to either a Path 2 promotion of a previously-training cell, a Path 3 installation of a new cell, or an operational change in the active flow topology.

---

## 5. The Trust Hierarchy for External Sources

The marketplace concept is overloaded. It must be decomposed into a trust hierarchy with explicit fallback ordering. Higher-trust sources are tried first; lower-trust sources are tried only if higher ones do not satisfy the request.

**Tier 1 — Locally trained, currently active.** Full trust. The cell was created by this system from operational data and is the active occupant of its slot. The default for every routine operation.

**Tier 2 — Locally trained, currently inactive.** High trust. The cell was created by this system, was once active or could become active, and is preserved for rollback or A/B comparison.

**Tier 3 — Organization-shared registry.** Moderate trust. The cell was created by another instance of this system in the same organization (a different developer's setup, a different team, a different tenant). Cryptographically signed, trust-scored on installation by the swappability judge against the conformance suite for its declared role-domain slot.

**Tier 4 — External public marketplace.** Low trust. The cell is from outside the organization. Probationary period required: outputs are computed and recorded but votes do not count toward FILLS until enough observations accumulate to push trust past threshold. Conformance suite must pass on installation.

**Tier 5 — Train from web-discovered corpus.** No trust until trained and validated. The dataset curator pulls a corpus from a high-quality source (Stack Overflow, GitHub, MS Docs), compresses it through SemanticCompiler, slices it for the target (unit-role, domain), trains a new cell, runs the conformance suite, installs the cell at probationary trust. Only used when Tiers 1-4 do not satisfy the gap.

The TrustScorer in 14.ContinuousLearning already implements per-source trust scoring for training data. It is extended in this architecture to also score per-cell trust at installation. The same scoring axis applies; only the input changes.

---

## 6. Synchronous vs Asynchronous Training

When the gap detector fires and a new cell needs to be trained, two operating modes are possible:

**Synchronous.** The user request waits for training to complete. First response on novel inputs is slow (minutes for a small LoRA pass on a single 4090, longer for a fully-trained small cell), but uses the freshly-trained cell.

**Asynchronous (default).** The user request completes immediately using the closest existing cell as fallback. Training is queued. The new cell becomes available for subsequent requests. The user is informed via the audit trail that a fallback was used and a better cell is being trained.

Asynchronous is the default for this architecture because synchronous would degrade user-visible latency in unpredictable ways and would make the latency contract for the system unstable. The audit trail is the mechanism that makes asynchronous behavior comprehensible to the user — they see "this answer used a fallback cell because the (decomposer, LEGACY) slot has not been trained yet; training has been queued."

Synchronous mode is available as an escape hatch when the user explicitly requests it ("retry this with a fresh cell when ready") and when system idle-state permits.

---

## 7. What Comes After This Document

In order:

**The unit interface specification.** Defines what every unit looks like from the outside — the precise predicate-input shape, the precise predicate-output shape, the metadata schema, the conformance suite format, the version semantics. One small, foundational document. This is the contract that lets everything else be self-similar. Everything else assumes it.

**The four registry designs.** One document each: the flow registry, the unit registry, the decision graph, the dataset registry. Each defines its own schema, its own access API, its own event-log format, its own bootstrap-discovery rule for entities referenced before they exist.

**The topology runtime subproject.** A new subproject (proposed numbering: 18.TopologyRuntime) implementing the orchestrator that reads topology contracts and walks them. Includes the nine-handler set adapted for internal-AI execution: rag-retrieve, decompose, internal-unit (replaces ai-generate), internal-multi-unit (replaces multi-generate), arbiter-pool (with arbiter-selector inside), validate, score, route, feedback. Sized as several phases following the FC-1..FC-12 reviewed format.

**The unit registry subproject.** A new subproject (proposed numbering: 19.UnitRegistry) implementing the cell storage, version chains, FILLS / SUPERSEDES / SWAPPABLE_WITH / TRAINED_FROM relationships, conformance suite attachment, trust-score-per-cell. Lives alongside the existing 08.GraphRAG schema patterns.

**Re-framing of the existing planned work.** The arbiter pool, the arbiter selector, the domain splitter, the prompt-and-transformation library — each is a (unit-role, domain) cell or set of cells in the new architecture, registered through 19.UnitRegistry, executed through 18.TopologyRuntime. They are not standalone subprojects; they are entries in the registry produced by the dataset curator and the training pipeline. The planned subproject 17.DynamicDomains is reframed as a use of the gap detector plus the marketplace fetch path, not a standalone system. The planned subproject 16.AutonomousLearning is reframed as the dataset curator role plus the IdleDetector role plus the training pipeline that feeds them. The planned subproject 14.ContinuousLearning is reframed as the FeedbackRouter role plus the TrustScorer role plus the LoRA-trainer role; its cells become entries in 19.UnitRegistry, its FeedbackRouter signal feeds Path 2 of the feedback paths. The planned subproject 15.OrchestratorModel is reframed as the decomposer role plus the orchestrator role plus the aggregator role; its components become cells in 19.UnitRegistry, registered as fillers for the corresponding (role, domain) slots.

**The testing-philosophy migration plan.** A small document that walks through the existing 877 tests and re-classifies them: which become contract tests, which become invariant tests, which become replay tests, which are deleted. Done after 18.TopologyRuntime ships its first phase, because the test substrate it provides is the prerequisite for the migration.

---

## Open Items at Approval Time

These are the items I believe still need explicit confirmation from you before this document is locked. They are matters of preference and project-specific scope, not architectural questions.

**O-1 — Subproject numbering.** Are the new subprojects numbered 18.TopologyRuntime and 19.UnitRegistry, or do you prefer different numbering or different decomposition?

**O-2 — Storage substrate split.** Are the four registries four separate collections in 08.GraphRAG (proposed default), or some other arrangement?

**O-3 — Synchronous-training escape hatch.** Is "asynchronous default with synchronous escape hatch" the right user-experience commitment, or do you prefer asynchronous-only or synchronous-default?

**O-4 — User UI for graph examination.** The architecture supports per-user examination of cells, versions, training corpora, and recent failures. Where does the UI live? My default assumption is the Visual Studio extension (subproject 13). Confirm or specify alternative.

**O-5 — Trust-tier authorization.** Tier 4 (external public marketplace) is described above. Is this in scope for the first version of the architecture or deferred until Tier 1-3 are operational?

**O-6 — The reverse SemanticCompiler.** The result translator role requires reverse-direction predicate-to-text generation, which the current codebase does not have. Is the design and training of the reverse path part of the scope of this architecture's first phase, or deferred to a separate session?

---

## What This Document Does Not Yet Decide

Deliberately not in scope here:

- The schema for the unit-cell data structure
- The schema for the topology contract document
- The schema for any of the four registry edge types
- The conformance suite format
- The exact predicate-input and predicate-output shapes per role
- The training-pipeline implementation details
- The IdleDetector triggering rules
- The exact thresholds for confidence promotion, probationary periods, decay protection
- The exact event-log format
- The marketplace authentication and signing scheme
- The user-UI affordance designs
- The execution order of the post-document subproject plans

Each of these belongs in one of the documents named in §7.

---

**End of draft.**
