# LlmFromScratch — Project Development History
## A complete record of what was built, why, how open-source sources were adapted, and what changed at each step

---

## Part 1 — The Origin Question (April 29, 2026)

The project did not begin with a plan to build a language model. It began with a question about open-source compilers and transpilers. Luba was exploring tools that translate high-level language into lower-level representations — the same class of problem that an LLM needs to solve internally when it processes text.

From that exploration came a second question: what are the algorithmic families that make these transformations work? The answer organized around four families — tree rewriting (Babel, TypeScript, SWC), pattern matching and unification (Drools, RETE algorithm), formal grammar recognition (ANTLR4, Nearley), and template substitution (Handlebars, Jinja2). Each family handles the same problem differently. Tree rewriting works when the source has explicit structure. Pattern matching works when you want rules to fire on facts. Grammars work when you want to define a language formally. Templates work when the transformation is mostly substitution.

This taxonomy became the conceptual foundation for everything that followed, because the LLM pipeline is fundamentally a sequence of transformations — and understanding which transformation family is appropriate at each stage is what separates a principled design from an ad-hoc one.

The same session surfaced the idea of building everything from scratch in C# with no external machine-learning frameworks. The reasoning was direct: TorchSharp (the .NET binding for PyTorch) wraps a closed binary. You can call its API but you cannot read or change the internal code. This contradicts the project's goal, which is to understand every algorithm from the inside. A replacement called TensorCore would implement the same operations from first principles in pure C#.

---

## Part 2 — The Architecture Decision: 12 Subprojects (April 30, 2026)

The first major plan organized the project as 12 sequential subprojects, each a self-contained C# class library inside a single solution.

The dependency order was: Tokenizer (01) → Dataset Pipeline (02) → Transformer Model (03) → Training Loop (04) → Embeddings (05) → Vector Search (06) → Graph Engine (07) → Graph RAG (08) → Inference (09) → LoRA (10) → DPO (11) → Search Engine (12). Each project could start only when its prerequisites were complete.

Every subproject followed the same gate structure. Gate A was automated — all unit tests green, build clean, no compiler warnings. Gate B was a cross-check — the implementation matches the reference algorithm from a named open-source source, verified by at least one correctness test. Gate C was explicit approval from Luba before moving to the next subproject.

The reference sources selected at this stage were all MIT-licensed Python implementations:

**Tokenizer (01)** — adapted from Andrej Karpathy's minbpe (github.com/karpathy/minbpe). minbpe is a minimal BPE tokenizer in roughly 150 lines of Python per file. The C# adaptation kept the same algorithmic structure (vocabulary training, pair frequency counting, merge application, encode/decode round-trip) but replaced Python's dictionary operations with C# Dictionary and added OperationResult<T> wrappers on every public method.

**Transformer Model (03)** — adapted from Karpathy's nanoGPT (github.com/karpathy/nanoGPT). nanoGPT implements GPT-2 in clean PyTorch. The C# adaptation replaced PyTorch tensor operations with TensorCore (the from-scratch tensor library), kept the same attention mechanism structure (scaled dot-product attention, causal masking, multi-head projection), and adapted the training loop to work with the C# async pattern.

**TensorCore (00)** — adapted from Karpathy's micrograd (github.com/karpathy/micrograd, MIT) for the automatic differentiation core, and from George Hotz's tinygrad (github.com/tinygrad/tinygrad, MIT) for the tensor operations layer. micrograd is roughly 150 lines of Python implementing scalar-valued automatic differentiation. The C# adaptation extended this from scalar to multi-dimensional arrays and added the BLAS-level operations that the transformer model requires.

**LoRA (10)** — adapted from the original LoRA paper's reference implementation. Low-Rank Adaptation freezes the base model weights and trains small low-rank adapter matrices. The C# adaptation implemented the rank decomposition (rank=8, alpha=16) and the parameter injection pattern without any external framework.

**DPO (11)** — adapted from the Direct Preference Optimization paper's algorithm. DPO converts human preference pairs (chosen response, rejected response) into a training signal without requiring a reward model. The C# adaptation implemented the DPO loss function and connected it to the existing training loop infrastructure.

The key adaptation rule across all subprojects was stated once and held throughout: read the reference source, understand the algorithm, implement it in C# from understanding rather than line-by-line translation. Every algorithm was reimplemented with OperationResult<T> error handling (no exceptions), Fabric-First interface injection (no concrete type instantiation in service code), plain-language comments on every logic line, and xUnit unit tests with behavioral assertions.

The constraint that no external NuGet packages could be used (except for Roslyn, SQLite bindings, and YAML parsing which were infrastructure, not ML) forced every mathematical operation to be implemented from first principles. This constraint was not relaxed at any point. It is recorded as D-11 in the locked decisions.

---

## Part 3 — The Experiment Framework (Early sessions, Product session 1)

Before the subprojects were fully implemented, an experiment framework was built inside project 35 (ExperimentsV2). The framework was designed around a principle from research design: a hypothesis should be testable before the full system is built.

The experiment structure had three tiers of scale. Tier 1 used only the smallest subprojects of the codebase as training data — roughly 9,100 tokens. Tier 2 expanded to six subprojects — roughly 30,000 tokens. Tier 3 used everything. Each experiment ran at Tier 1 first and scaled up only when it passed a promotion gate: accuracy improvement of at least 5% over the baseline, or at least three metrics showing at least 5% improvement with accuracy non-negative.

The baseline was always a fair comparison — not "predict the most common token always" but a keyword classifier or a simple n-gram predictor. The enhancement was always tested against the same baseline on the same data. The improvement percentage was computed honestly.

The first six experiments (E01 through E06) explored different approaches to the tokenization and prediction problem:

**E01 — K-Means semantic families.** Instead of BPE's 2,000 tokens, cluster tokens into 50 semantic families. The matrix is 50×50 instead of 2,000×2,000 — 40× smaller. Hypothesis: clustering by meaning is more information-dense than counting individual tokens. Result: passed at Tier 1 with meaningful accuracy improvement.

**E02 — Shannon entropy compression.** Measure how much the token stream can be compressed using entropy coding. A more predictable stream compresses better. Hypothesis: better compression correlates with better model performance. Result: provided a useful compression signal.

**E03 — Sparse attention window.** Instead of full O(n²) attention over the entire context, use only a local window of 40 positions. Hypothesis: local context is sufficient for the patterns in this codebase. Result: the sparse window lost too much non-local information.

**E04 — PMI initialization.** Initialize embedding weights using Pointwise Mutual Information statistics rather than random Xavier initialization. Hypothesis: starting from semantically informed weights speeds convergence. Result: failed because with only 4,587 tokens, the PMI matrix was mostly zeros and the argmax always returned index 0 — a data volume problem, not a conceptual problem.

**E05 — Hierarchical prediction.** Two-level Markov chain: predict the cluster first, then predict the token within the cluster. Hypothesis: the hierarchy makes the prediction problem smaller at each step. Result: the two-level indirection compounded errors at this vocabulary size.

**E06 — AST nodes via Roslyn.** Tokenize code via Roslyn's syntax tree rather than BPE. Hypothesis: AST nodes carry more semantic meaning per token. The comparison was initially unfair because baseline and enhanced used different dataset sizes. After the measurement was corrected to use the same prediction count, E06 showed the right direction — AST tokenization preserves structural meaning that BPE loses.

The lesson from E01 through E06 was that bigram and Markov models have a hard ceiling — they accumulate counts, and changing the training order, weighting, or caching does not change the accumulated counts. Six experiments (E28 through E31, run later) confirmed that hypotheses requiring gradient-based learning could not be tested with a bigram proxy. Those experiments were correctly classified as SCALE_UP — the hypothesis was sound but the tool was wrong. The gradient learning infrastructure in projects 04 and 10 was the correct tool.

---

## Part 4 — The Semantic Compiler (E15 through E20)

The experiments that had the largest architectural impact were E15 through E20, which built the semantic compiler pipeline. This pipeline converts raw C# code and text into a stable logical structure that the rest of the system can reason about.

**E15 — SemanticNormalizer.** CamelCase identifiers are split into words. Decorator words (prepositions, articles, auxiliary verbs) are removed. Verb stems are extracted by removing inflectional suffixes (-ing, -ed, -s, -er, -or). The result is a compact set of root concepts. `getUserById` becomes `get user id`. `validateUserToken` becomes `validate user token`. The vocabulary shrinks from roughly 512 tokens to 80-120 meaningful concepts. Adaptation source: algorithmic — no direct open-source reference, derived from standard NLP lemmatization techniques.

**E16 — FamilyCollapser.** Root concepts are grouped into semantic families. Authentication-related roots (authenticate, login, credential, permission) all collapse to AUTH_OP. Data retrieval roots (get, fetch, load, retrieve, find) collapse to GET_OP. Creation roots collapse to CREATE_OP. Error handling roots collapse to FAIL. The result is a 22-item predicate vocabulary that covers the semantic content of almost any C# method. This vocabulary became locked as D-04's companion artifact — it is not reopened without experimental evidence.

**E17 — LogicExtractor.** The predicate vocabulary is used to extract the logical predicates from a code sample or prompt. `if (user == null) return null` extracts `[NULL_GUARD, FAIL, RETURN]`. A loop with an assignment inside extracts `[ITERATE, ASSIGN]`. The output is a predicate profile — a small set of predicates that characterizes the logical structure of the input regardless of surface wording. This was the key insight: two prompts with completely different surface words might have the same predicate profile, and that profile is what routes them to the same domain.

**E18 — DomainClassifier.** The predicate profile from E17 is used to classify which of five domains the prompt belongs to: CODE_QUESTION, BUG_FIX, CODE_CHANGE, ORCHESTRATION, or ARCHITECTURE. The classifier uses K-NN over the predicate space. The five domain labels are locked in D-04 and do not change. The result: 95% classification accuracy at Tier 1. This accuracy came from predicate profiles, not surface keywords — a prompt asking "why does getUserById return null" classified correctly as BUG_FIX because of the predicate profile (NULL_GUARD + GET_OP), not because of the word "why."

**E19 — ComplexityEstimator.** Estimates whether a prompt describes a single task or a composite task requiring decomposition. A single GET_OP predicate suggests a simple query. A combination of CREATE_OP + VALIDATE_OP + ITERATE suggests a multi-step operation.

**E20 — StackDetector.** Detects whether the code context is C#, TypeScript, or Python before any normalization runs. This became a locked ordering constraint (D-07): the stack detector must run before the semantic normalizer, because normalization destroys the language markers that stack detection depends on. If normalization runs first, `async Task<bool>` (the C# async pattern) gets normalized before its C#-specific markers are detected.

The chain E15 → E16 → E17 → E18 → E19 → E20 became the Semantic Compiler Pipeline — the first stage of the full dynamic flow architecture. Every subsequent experiment and implementation referenced this pipeline.

---

## Part 5 — The Routing and Retrieval Experiments (E21 through E38)

With the classifier in place, the next set of experiments addressed how to route a classified prompt to the right context and how to retrieve the most relevant training material.

**E21 — Task Decomposer.** Splits a compound prompt into independent subtasks. "Add null check and update tests" is two tasks: add the null check, update the tests. The challenge was conjunction disambiguation — "find nulls and log them" looks like two tasks (find, log) but "them" refers back to the nulls found in the first half, making it one task. The false-conjunction guard was implemented using VERB+NOUN structural detection, later refined by predicate profile boundary detection in E51A.

**E22 — Attributed Segmenter.** Routes different segments of a prompt to different domain specialists. A prompt about both performance and correctness has a BUG_FIX segment and an ARCHITECTURE segment. E22 exposed a data problem: with a single-codebase dataset (llm_mvp_core), there was not enough domain diversity to make K-NN routing meaningful — every example was within one architectural style. The fix was to expand the dataset in later experiments.

**E23 — Template Beautifier.** Converts logical predicate sequences back into human-readable code patterns. A predicate sequence `[NULL_GUARD, FAIL, RETURN]` maps to the null-check-and-early-return template. A sequence `[ITERATE, ASSIGN, LOG_OP]` maps to the logging-loop template. This was the first reverse step in the pipeline — going from predicates back toward human language.

**E33 through E35 — DependencyGraph, StopSignalDetector, and related.** These experiments built the planning infrastructure: how to recognize when a task is complete (the stop signal), how to model dependencies between subtasks, and how to schedule parallel execution.

**E38 — KnnSkillFilter.** Uses K-NN over the predicate space to select which skills (domain-specific patterns) are most relevant for a given prompt. The key finding: scoping the K-NN candidate pool to the predicted domain before similarity ranking gives much cleaner results than running global K-NN. An architecture-domain example will have a higher similarity to other architecture examples than to bug-fix examples, but without domain scoping, the retrieval can pull from the wrong pool and look confident. This finding was later calibrated in E51A Phase 2.

---

## Part 6 — The Learning Infrastructure (E40 through E46, Projects 14-17)

With the pipeline established, the focus shifted to how the system learns and improves over time.

**E40V2 — TrustScorer.** Scores training examples by how trustworthy their source is. Code from the repo itself (which has tests and review history) scores higher than synthetic examples. The trust score uses a weighted formula that does not use a constant multiplier — constant multipliers caused catastrophic forgetting in E44 (the older weights were overwritten because new examples with a ×10 weight overwhelmed them). The correct formula is total_samples / (num_classes × rare_class_count), locked as D-09.

**E41 — IdleDetector.** Checks whether the machine is safe to train on: CPU below threshold, battery charging or above threshold, disk space available, no active user work detected. The four-condition check prevents the system from starting a training run during an active session that would compete for resources. Training runs only when all four conditions are met.

**E43 — ReplayBuffer.** Implements a 70/30 replay buffer: 70% of each training batch is new examples, 30% is randomly sampled from prior training data. This prevents catastrophic forgetting — when the system learns new examples, it does not lose the patterns it learned before. Locked as D-06.

**E44 — WeightingFix.** Discovered that using a constant ×10 multiplier for rare class examples causes catastrophic forgetting. New rare examples get so much weight that they overwrite all prior learning. The fix (D-09 formula) keeps the weight proportional to the dataset size rather than fixed.

**Project 14 — ContinuousLearningPipeline.** Wires the learning infrastructure together: index the codebase with ProjectIndexer, cache labeled examples with DomainDatasetCache, route user feedback to DPO pairs with FeedbackRouter (Accepted → positive example, Edited → DPO pair with user correction as chosen, Rejected → negative DPO pair), run LoRA fine-tuning with IncrementalTrainer, register new adapters with DomainModelRegistry, flush the cache with FlushAsync.

**Project 16 — AutonomousLearningPipeline.** The eight-step overnight learning cycle: IdleDetector check → baseline accuracy measurement → data collection → D-09 weighting → LoRA training with replay buffer → enhanced accuracy measurement → register adapter only if enhanced > baseline → return CycleResult. The before/after accuracy comparison gate was added in round 06 of the projects 21-23 transfer work, completing the rollback protection. A new adapter that regresses accuracy (enhanced ≤ baseline) fails with ACCURACY_NOT_IMPROVED and the registry is not changed.

---

## Part 7 — The Dynamic Flow Architecture (Product sessions 5-8, May 2026)

The experiments up to E50 established the component-level algorithms. The dynamic flow architecture organized those components into a principled runtime design.

The architecture was proposed after the team looked at the static eight-stage pipeline (split → translate → plan → process → apply → translate back → arbitrate → format) and identified its limitation: the pipeline is code, not data. Changing the pipeline means changing code. Adding a new stage means modifying the orchestrator.

The dynamic flow replaces this with a topology stored in a database. The orchestrator reads the topology at runtime. Adding a new stage is adding a node to the database. Moving a stage is editing an edge. Rolling back a change is restoring a prior topology version.

**The seven principles** were locked at architecture approval:

Principle 1 — Universal Continuous Learning: every unit learns from feedback. Nothing is hardcoded as a permanent static rule except the predicate vocabulary, the unit interface contract, and the 10 codebase rules.

Principle 2 — Dynamic Flow Topology: the pipeline graph is data, not code. Stored in a registry. Modified without recompilation.

Principle 3 — Self-Similar Units: every unit has the same external interface regardless of its role. Domain splitter, formatter, and arbiter selector all look identical to the orchestrator.

Principle 4 — Versioning and Rollback by Construction: every unit cell has a version chain. Every topology has a version chain. Rollback is moving a pointer.

Principle 5 — Internal AI Throughout: external AI providers are not in any operational control path. This principle was later qualified to allow a controlled knowledge-edge path (see Part 9).

Principle 6 — Provenance End-to-End: every action is recorded with timestamp, source, and cause.

Principle 7 — Testing Adapts to Self-Modification: behavioral tests that pin specific sequences become anti-patterns. Contract tests, invariant tests, and replay tests are the right categories.

**The high-level design amendment** (produced in session with Luba) added staged narrowing before the domain splitter. The revised flow: intake gate → system topology router → sub-subject routers → evidence pack selector → local domain splitters → existing pipeline stages. Each stage narrows the label world so each unit operates on a small, well-defined problem rather than the full universe of possible inputs.

**The logical structure compiler** was added as a pre-stage before the intake gate. It converts any input (user prompt, document line, session fragment, prior output) into the shared predicate/meaning-graph representation that all pipeline units communicate in. The E15-E17 semantic compiler experiments provided the validated implementation. The 60-slot feature vector — 22 predicate count slots, 5 domain one-hot slots, up to 32 normalized root slots, 1 decomposition count slot — was calibrated in E51B.

**The canonical integration sequence** documented the 20-step runtime flow from raw input to idle learning, making it the authoritative reference for all implementation work.

---

## Part 8 — The E51 Experiments: First Closed-Loop Proof (May 4, 2026)

Before transferring the design to the implementation projects, two experiments validated the architecture in a controlled environment.

**E51A — Code Context Pipeline (Tier 2).** Proved that the existing codebase pipeline — stack detection, semantic tokenization, predicate extraction, domain routing, K-NN retrieval, template beautification — works as a chain when given C# code context alongside the natural language prompt.

The experiment exposed three thin spots in Tier 1 that metrics alone had hidden: V07 had only one architecture neighbor (a single-point attractor), V03/V04/V09 shared the same loop-shaped predicate signature despite belonging to different domains, and V10's code context was null-check-heavy when it should have been implementation-heavy for an IEquatable interface example. These were fixed before Tier 2 expansion.

Tier 2 added domain-scoped K-NN retrieval: the candidate pool is scoped to the predicted domain before similarity ranking. This was the key finding from Phase 2. The wrong-domain retrieval guard showed that when the pool is wrong, all neighbor scores stay below 0.50 — a calibrated confidence floor. V06 (compound diagnostic prompt "why and how to fix") routed to BUG by Bug score ≥ CodeQ score, not by surface keywords alone.

Final Tier 2 result: baseline overlap 0.205, enhanced overlap 1.000, domain accuracy 100%, trace success 100%, zero failures.

**E51B — Dynamic Flow Mini E2E.** Proved the full dynamic flow architecture on 18 records from the user registration domain: 12 teaching records, 4 held-out test records, 2 challenge records.

The four held-out records (TE-01 through TE-04) all routed to the correct local subject. The teaching and runtime records compiled through the same feature path, confirmed by the symmetry inspection (TR-01 and TE-01 both produced 60-wide vectors with the same 22-predicate vocabulary and predicate overlap above 0.50).

Challenge record CH-01 (biometric passport verification during signup) triggered knowledge-edge detection: neighbors were found (TR-09 at 0.3162, TR-04 at 0.2158, TR-03 at 0.1984) but all scored below the 0.50 confidence floor. The system correctly created a context pack naming the specific missing competence ("does not yet know how biometric passport verification changes the registration flow") and a learning gap packet. This is low-confidence retrieval — the designed behavior — not zero-neighbor retrieval.

Challenge record CH-02 (deliberate instruction to create a second account for a repeated provider callback) had a nearest neighbor with 0.857 similarity — high enough to normally proceed — but the identity_consistency arbiter rejected it anyway. This proved the core architectural rule: KNN similarity cannot override an arbiter verdict. This rule became locked decision D-15.

**E52 — Projects 21-23 Cycle Comparison.** Proved that the five implementation components (described in Part 9) correctly reproduced the E51A and E51B findings in the production agent infrastructure. Baseline cycle success 0%, enhanced cycle success 100%, across four cycle traces: V06 routing, CH-01 approval package, CH-02 arbiter authority, IDLE-01 idle learning queue.

---

## Part 9 — Projects 21-23 Transfer (May 4, 2026, rounds 01-07)

With both experiment tracks validated, the findings were transferred to the three implementation projects.

**What projects 21-23 are.** These projects implement the agent runtime in C# — the equivalent of what LangChain, LangGraph, and related Python frameworks provide, but built from scratch with no external framework dependencies. Project 21 (AgentInfrastructure) is the execution substrate. Project 22 (AgentAlgorithms) is the planning and routing layer. Project 23 (OssEquivalents) implements the open-source framework equivalents in C#.

**The five transfer components:**

InternalLlmGateway (project 21) routes requests to the internal model by domain, loads the matching LoRA adapter, and enforces the compound-diagnostic routing rule from E51A V06: when Bug score is positive and at least equal to CodeQ score, the prompt routes to Bug regardless of how many question-type keywords appear. ExternalCallCount always returns 0. The gateway never calls any external AI provider.

LearningEventScheduler (project 21) queues DPO pairs when a domain accumulates at least 5 pairs and releases them only when the IdleDetector reports the machine is safe to train. Accepted feedback (no DPO pair) does not trigger training. Only actual DPO pairs (from Edited or Rejected signals, or from external AI adaptation) trigger the queue.

PermissionGate (project 21) was extended with call_external_ai in the RequiresApproval set. The gate is stateless — two sequential checks both return RequiresApproval. Single-use approval semantics are enforced by the ExternalAiStateGuards, not by the permission gate itself.

InternalFirstAiSupport (project 22) contains three components: ExternalAiRequestPackage (the structured approval prompt: original request, compiled logical structure, internal attempt, confidence score, named missing competence, required answer shape, arbiter constraints); InternalResultExaminer (checks a response against required predicates and arbiter constraints, returns confidence score, named missing competence, and whether external AI is needed); ExternalAiResultAdapter (converts an approved external answer into PreprocessedLearningData for the FeedbackRouter, running arbiter constraint checks before accepting the answer).

ExternalAiStateGuards (project 23) provides the state machine guard that fires external AI approval requests. It requires three conditions simultaneously: examiner confidence below 0.50, missing competence string non-empty (a named gap, not just low confidence), and approval not already requested in this run (single-use enforcement).

**Round-by-round changes:**

Round 01 established the five components and E52 comparison. 916/916 tests.

Round 02 applied the document adaptation across all 11 dynamic flow architecture documents, adding D-14 and D-15, the node-00 compiler pre-stage to the examination method, the B-entries-are-hints note to the Flow-01 plan, and validation references to all amendment documents.

Round 04 closed the small negative-path test gaps: sequential approval (two calls both require approval), high-confidence state guard returns false, empty-missing-competence guard returns false, accepted-signals-only does not trigger the scheduler. Fixed project 16 compile ambiguity by qualifying ContinuousLearning.IDomainModelRegistry.

Round 06 implemented the rollback gate in AutonomousLearningPipeline. Steps 6 and 7 are now distinct: train the candidate adapter, measure enhanced accuracy, register the adapter only if enhanced > baseline. Fails with ACCURACY_NOT_IMPROVED if the candidate regresses.

Round 07 generalized ExternalAiResultAdapter from a hardcoded identity_consistency check to a named rule table. FindArbiterRejection iterates the ArbiterConstraints array, looks up each constraint in KnownArbiterRules (StringComparer.OrdinalIgnoreCase), and short-circuits on the first matching rejection. identity_consistency rejects duplicate-account external guidance. token_safety rejects unsafe token storage guidance. Unknown constraint names pass without rejection — the table is a whitelist, not a blacklist.

**What round 07 still needs before closing.** Rule 3 requires a plain-language comment on every logic line in InternalFirstAiSupport.cs. The FindArbiterRejection method, the KnownArbiterRules dictionary, both static detection methods, and the predicate extraction in Adapt are uncommented. Four documents in the dynamic flow folder also need to be updated to reflect the rule-table design. Round 07 is written and 985/985 tests pass, but the round is not committed until these items are complete.

---

## Part 10 — The Training Set Work (May 4, 2026, in progress)

With the architecture designed, the experiments validated, and the implementation projects built, the remaining work is producing the actual training pair files that will teach each node in the dynamic flow pipeline.

No per-node training pair files exist yet. The examination method is complete and correct. The Flow-01 plan contains 357 rows of source line ledger entries. The node-00 logical structure compiler specification exists in the examination method. Nothing has been produced in the training set folders.

**The examination method** (EXAMINATION-METHOD-FINAL.md) defines the correct approach, superseding all prior attempts. Its core finding, derived from examining failed training sets produced under earlier methods:

A is the exact input the node receives at runtime — a user prompt, or a domain fragment, or a translated description, or a processed result. Never a source document line. No node receives document lines at runtime.

B is the exact output the node must produce, written in full. Not a label. Not a category name. Not a description of what the output contains. The actual text the node would emit.

Every training pair appears at least twice with different phrasing. The same situation described in different words, at the same depth of detail.

Array nodes (domain translator, domain processor, applier, result translator) each receive one element and produce one result. Writing one large pair that handles all elements together breaks the architectural contract.

The arbiter selector and the result translator are separate node files. Both complete before the formatter runs. The formatter receives only approved translated results.

**What the first training set file should contain.** The first file is node-00-logical-structure-compiler-set.md for the Flow-01 registration domain. A is a raw registration prompt (a real sentence a user might type: "Can I sign up with Google instead of creating a password?"). B is the compiled logical structure written in full: source role, predicate list from the 22-item vocabulary, normalized roots, meaning units, confidence score, and any missing-coverage signal. Every pair appears twice with different phrasing.

The registration domain is the right starting point because E51B already validated it. The 12 teaching records and 6 challenge records from E51B are evidence that the domain is well-defined and that the compiler produces the right output. Building the training set from this validated domain gives the first file a known-good foundation.

---

## Open Work as of May 4, 2026

**Thread 1 — Round 07 close-out.** Add plain-language comments to every logic line in InternalFirstAiSupport.cs. Update four documents. Recreate the round 07 zip. Commit as a single clean phase.

**Thread 2 — Topology visual update.** The prose documents reflect the logical structure compiler and budget/confidence gate as named nodes. The CSV and Mermaid topology files do not. Requires uploading the two files to update them.

**Thread 3 — First training set pairs.** node-00-logical-structure-compiler-set.md for the Flow-01 registration domain. The entire architecture has been built toward this. Nothing blocks starting.
