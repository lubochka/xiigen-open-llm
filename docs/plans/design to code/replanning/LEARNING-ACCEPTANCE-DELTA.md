# Learning acceptance delta: what changes, why it helps, and what disproves it

**2026-10-13 · CP-A of five authoring checkpoints for this amendment. PLAN, not an executed experiment.**

This amendment supplements the current PLAN-25-RUNS and LEARNING-WORK-ORDERS; it does not replace source decisions, create another queue, restart R01–R03, or authorize training/code/CI. The dispatcher includes only the relevant section in one effective Rxx packet. Do not load this entire document in every worker. Existing evidence carriers and round IDs remain unchanged.

## 1. Evidence and precedence

The reviewed current planning sources are README, AGENTS, SOURCE-ROUTES, PARALLEL-R01-R03-JOIN, the R01–R10 work-order sections, and original task.md (initial roles and stated generator/judge separation). Previous-round findings remain evidence with their own refs, not verified runtime. Source inspection during this amendment is targeted; it is not completion of the repository-wide R01/R03 work. Earlier file-name inventories or different revisions must not be used to overwrite the current branch.

Resolve these sources using actual repository/ref/path/blob/span and authority, not latest filename alone:

- [Original task](../original%20task.md): Unit/Flow/Orchestrator/DynamicGraph, typed many-to-many, asynchronous alternative combinations, graph-generated unit teaching, own Builder, distinct permitted generator and reviewer roles.
- [Source routes](SOURCE-ROUTES.md): original/task prompts, goal changes, all discovered WorkLeft/LeftToWork and RAG/NDJSON families, three product directions, S00–S10 plus newly discovered scope.
- [Recovered contract](RECOVERED-DESIGN-CONTRACT.md) and [recovered prompts](RECOVERED-DESIGN-PROMPTS.md): recovery leads requiring primary-source tracing, not proof every recovered mechanism runs today.
- [Hybrid work](HYBRID-ALGORITHMS-AND-RUNS.md), [learning evaluation](LEARNING-EVALUATION-CONTRACT.md), [examination](LEARNING-EXAMINATION-CONTRACT.md): retain unique obligations. This delta specifies causal attribution and efficient experiment handoff, not a replacement policy.
- [Parallel join](PARALLEL-R01-R03-JOIN.md): each worker owns its own results/Rxx; J03 is an integration responsibility within R10 unless a separately executed call must be honestly accounted for.

Current source-preserved one-pair/no-repeat/full-state rollback must not silently become replay or batch training. Here **bulk means a reporting group of distinct, admitted lessons**. Standard DPO mathematics does not by itself establish this project's update/admission policy. A model score, hash change, saved pair, vector shape, successful CI job, or confident reviewer does not establish learning gain.

## 2. Minimal record for every mechanism and connection

Put the record in the existing FINDINGS/CHANGE-MAP, not a new reporting bureaucracy:

`mechanism_id; requirement_id; primary_source; current_symbol_or_NOT_TRACED; producer; consumer; domain/intent/stack; input_meaning; output_meaning; schema_and_coordinate_versions; deterministic_operations; learned_decisions; mutable_state; state_owner; admitted_signal; objective; intervention; predicted_observation; falsifier; cost_boundary; next_smallest_implementation; examination_owner`.

Use NOT_TRACED rather than invented method names. A method map must follow producer → transformation → mutation/selection → downstream consumer; finding a similarly named class is insufficient. Distinguish the logical role from the physical model/training owner. Reproducible inference may still use learned parameters; randomness alone proves nothing.

**Connection manipulation is an explicit deliverable:** for every participating connection identify what can be frozen, bypassed, rerouted, replaced by an oracle input, or restored without changing the other parts. Record the legal scope of these offline research interventions. Disabling a law/authorization guard is not an admissible production alternative.

## 3. Four preserved roles: separate scientific questions

| Role | Functional expectation | Deterministic contribution | Learned contribution to locate or implement | Discriminating examination | Failure that changes the plan |
|---|---|---|---|---|---|
| ILearningUnit | Domain and typed input/output vector structures; many inputs can yield several valid requested-type results | Shape/schema/units checks, exact lawful operations, candidate decoding | Compact parameterized transformation/ranking, uncertainty or selection under the actual contract | Give correct domain and fixed candidate pool; compare frozen and updated unit on unseen input families; then pass both through the same downstream consumer | Gain exists only on memorized training inputs; requested type or semantic meaning is lost; state changes but real consumer uses old state |
| ILearningFlow | Relations between units, correction/transfer from input to output | Compatible port binding, legal conversions, bounded correction protocol | Which compatible relation/transformation to use, when correction is useful, composition preferences | Keep units frozen; vary only flow policy/connection; count successful lawful corrections, unnecessary corrections, total cost and retained skills | Individual units improve but composition fails; hidden rescaling changes meaning; correction cycles consume budget without useful output |
| ILearningOrchestrator | Async processing of one input through alternative combinations | Task lifetime, cancellation, isolation, version-consistent reads, resource caps | Budget-aware route/branch/ranking choice where the design assigns learned authority | Give identical candidate universe/budget; compare fixed routing, learned routing, and hybrid; include failed/cancelled branches in cost and use controlled completion-order probes | Faster apparent winner merely omits losing work; completion order selects invalid answer; old/new training state gets mixed |
| ILearningDynamicGraph | Higher-order pattern relations, useful new compositions and own Builder; support teaching required units | Structural admissibility, versioning, cycle/resource constraints, final arbiter | Useful relations/compositions at higher orders; mapping failures to an admissible teaching target | Hold primitives and representations fixed; compare unchanged graph with learned graph on unseen compositions, not just unseen surface text; then compare graph plus learned components | More nodes/edges without useful held-out compositions; graph-level reward is sent to unrelated UnitId; deterministic shortcut replaces the required learned decision |

These are roles, not a mandatory linear chain for every input. Do not add Train to every interface or pretend IQueryable is an implemented async contract. Resolve the actual C#/generic/ILogical* correspondence from code and record a migration only where necessary.

## 4. Domain identification is not solver quality

Domain, intent, source/target language, stack, architecture level and physical model are separate axes. A SQL bug is not a choice between SQL and BUG: they can describe different axes. Support multiple applicable domains, unknown/OOD input, ambiguous routing, and cross-domain requests without forcing everything into one existing label.

For each domain family prepare three matched conditions:

1. **Oracle-domain input:** supply the independently established applicable domain(s); measures representation/solver capability conditional on correct routing.
2. **Predicted-domain input:** normal system route; measures end-to-end capability.
3. **Controlled domain perturbation:** plausible wrong/ambiguous domain, with the same request and lawful fallback; diagnoses sensitivity and abstention behavior.

Report exact/specified-set routing correctness where a justified reference exists, confusion by domain/intent/stack, unknown-domain behavior, abstention coverage and error among accepted answers. Report solver quality separately for correctly and incorrectly routed cases. Similar oracle and predicted scores do not prove mapping is good when both solvers fail.

Retain the seven baseline WorkLeft domains/directions and all additional source-recovered domains. Structural cohorts such as algebra, SQL, language and layout are **example probe families, not a replacement closed domain list**. A fast pilot may use a small source-supported subset, while a requirement register preserves every remaining domain with a concrete owner and next experiment.

## 5. Laws → representation → domain-specific retrieval

A domain descriptor must identify: domain/intent scope, raw cues required before normalization, type/units, entities/relations, hard constraints, conditional/soft preferences, applicable context/version, allowed transformations, output contract, token schema, representation transform and distance/ranking candidates. Hard laws are not ordinary embedding coordinates that a high similarity score can outweigh.

Preserve source-recovered StackDetector-before-normalization where its primary decision applies. For different domains investigate different lawful invariances: bound-variable renaming versus changed binding; SQL row semantics versus merely similar text; translation of negation/quantities versus stylistic paraphrase; layout constraints versus pixel resemblance. Examples illustrate examination design; they are not automatic engine rules or claims of present implementation.

Each representation experiment includes:

- Meaning-preserving transformations that should stay equivalent under the declared task contract.
- Surface-similar counterexamples differing in a decisive type, unit, scope, quantifier, order or relation.
- Cases where multiple outputs are valid and exact string equality is the wrong target.
- Cross-domain projection and unknown schema/version, including incompatible dimension/coordinate spaces.

Keep tokenizer/codebook, vector encoder, domain classifier, projection and retrieval separate in the method map. Changing one may invalidate an index or pair encoding. Charge re-encoding/reindexing and compatibility management to total learning cost.

### Dynamic kNN: hypothesis families, not a guessed universal optimum

Inspect the real BM25+kNN/domain-memory/final-arbiter design before proposing replacements. Record independent choices: domain pool, feature construction, distance/similarity, k or adaptive neighborhood size, reranking, positive/negative/warning memory influence, index maintenance, uncertainty/fallback and trainable policy owner.

A possible candidate family, **only a hypothesis**, is `s_d(x,z)=sum_j w_dj(x)*phi_dj(x,z)`, with domain-dependent structural/semantic features and constrained weights when that is meaningful. An adaptive k policy can be proposed from neighborhood confidence/density/budget. A weighted score is not automatically a mathematical metric; approximate-index compatibility must be checked against the chosen retrieval method. Do not lock this family or a universal cosine distance into requirements before evidence.

Compare a source-faithful fixed baseline, domain-aware fixed retrieval, and domain-adaptive retrieval with identical requests, allowed knowledge and state budgets. Where compatible, examine BM25-only, kNN-only and their combination as attribution conditions, not mandatory production rewrites. Log neighbor relevance, hard-negative exposure, downstream lawful accuracy, retrieval p50/p95, total latency, memory and update/reindex cost. Tune only on development families; verify on held-out families. A larger memory can improve retrieval without improving a learned policy: report both interventions separately.

**Falsifier:** more relevant-looking neighbors but no downstream benefit, wrong-domain neighbors dominating, latency/memory outweighing benefit, or the same gain obtained by a simpler fixed policy. Select the best observed trade-off for the specified domain and budget, never claim global best kNN.

## 6. Positive/negative pairs and the actual update target

A preference record needs the same input/context/constraints for both candidates, compatible representation versions, complete candidate outcomes (and routes when route preference is learned), provenance/permission, producer IDs, independent admissibility judgement, preference reason/category, source family, split, maturity and update target. Preserve TIE, BOTH_BAD, INCOMPARABLE and UNKNOWN instead of manufacturing a winner. Warnings/negative memory are not automatically trainable preference pairs.

Construct a positive candidate because it better satisfies the declared task under its laws; a negative should be a meaningful competing failure, not random noise or a secretly different task. Model identity and verbosity are not quality labels. Speed wins only among acceptably correct/admissible alternatives under a declared quality–cost rule. Keep generator/pair judge/Student/evaluator permissions separate; confidential or restricted judge outputs must not be laundered into training targets. Illustrative examples in this document are not an authorized training dataset.

For a probabilistic policy over the actual candidate representation, standard DPO uses:

`L = -log sigmoid(beta * [(log pi_theta(y+|x)-log pi_ref(y+|x)) - (log pi_theta(y-|x)-log pi_ref(y-|x))])`.

The execution packet must name the actual log-probability-producing component, reference policy, conditioning, candidate unit/sequence, masks/aggregation, trainable parameters and permitted update. If the component only emits distances or arbitrary scores, do not relabel that as standard DPO. Specify a justified pairwise ranking objective or explicitly designed normalized policy instead, and preserve the original project preference requirement. A graph success label does not establish local credit for every traversed unit: trace which decision differed, which state may change, and whether confounded examples must be withheld.

Follow one fresh admitted pair → pre-update full snapshot → specified update once → operational assessment → accept/restore under the current source-backed policy. Pair storage, parameter mutation and measured utility are separate milestones. Do not invent successful acceptance, repeat a gradient under a new pair ID, or replay all historical pairs. Research comparisons of alternative policies need separately authorized isolated branches and must not be silently applied to live state.

## 7. LoRA and projection: what transposition must mean

First establish whether the requirement means mapping representations, adapting a transformation, or mathematically transposing a matrix. These are not equivalent. For an applicable layer, low-rank adaptation has `W'=W+sBA`, where shapes and rank must match the actual input/output mapping. This is not a tokenizer, a new codebook, or proof the output vector has the required semantic meaning. A rectangular projection can be designed if the source contract requires it; transposing weights alone does not align two semantic spaces.

Name the exact layer/projection, frozen base, A/B dimensions/rank, scaling, dtype, output schema, decoder/consumer, loss and compatible index versions. Compare no adaptation, the source-faithful static projection and the candidate adaptation on matched held-out relation/meaning tasks. Report downstream typed-result quality, near-neighbor semantic confusions, cross-domain transfer and retention, train/inference memory, adapter/base cost and reindex effects. A shape check, cosine gain or lower loss alone is insufficient. Rank/quantization are experimental choices within real PC constraints, not guessed optimal constants.

## 8. Bulks, causality, efficiency and continuous learning

Before an authorized experiment, freeze a manifest: code/state refs; admitted lesson IDs and unique source families; domain descriptor/representation/graph/index versions; reference policy; hardware/software profile; warm/cold-cache condition; evaluation family split; rubric; quality floor; meaningful change/noise rule; timeout/resource caps; comparison and stopping rule. Unknown values remain unresolved prerequisites, not fabricated numbers.

Use two levels of observation:

- **Operational lesson decision:** the current project accept/rollback policy for one admitted lesson. Do not add an automatic statistical-significance barrier after each pair, or call repeated operational assessment untouched certification.
- **Research bulk report:** compare the retained state before/after a reporting group of distinct lessons on a fixed matched development/monitoring panel, plus a separately reserved confirmation panel at declared checkpoints. Rotating fresh operational cases and reserving confirmation families reduce repeated-selection leakage; do not teach from confirmation cases. Reporting groups do not change the no-repeat update rule.

Record per-domain sample/family counts, admitted/rejected/tied pairs, actual updates, accepted/restored states, quality delta, routing delta, lawful path completion, latency distribution, full training/inference cost, retained old-domain quality, intervention configuration, uncertainty and missingness. Quality, throughput, memory and retained skills form a trade-off; do not hide them in an unexplained scalar.

Evaluate user input before learning from its correction. Separate permission, authenticity, domain-law validity, source duplication and malicious/contradictory feedback from utility. Asynchronous inference reads an identifiable consistent accepted snapshot. Full recovery includes all causally relevant mutable state (as applicable: adapters/weights, optimizer, scheduler/RNG, graph/index/memory/cache versions, counters and admission transaction). External costs or disclosures are not reversible by restoring weights.

Cost per useful admitted/accepted improvement must account for failed attempts and rejected lessons, not only successful forward passes. Do not divide by zero when no improvements were accepted: report no accepted improvements and total cost. Compactness includes base model, adapters, optimizer/training activations, embeddings/index, graph, caches, snapshots and host overhead. Measure the actual PC profile; do not infer today's capacity from historical hardware notes. A small adapter is not proof the whole learner fits locally.

For uncertainty, define the sampling unit as independent problem/source family rather than many correlated paraphrases. Estimate uncertainty for paired changes with an analysis appropriate to those units and outcome type; explain small-sample limitations. Choose repetitions/seeds when algorithm/hardware stochasticity requires them. Do not mandate arbitrary repeat counts or exact gain thresholds before the baseline. Outcomes remain IMPROVE, REGRESS, INCONCLUSIVE, INVALID_EVIDENCE; undocumented measurements are NOT_MEASURED.

## 9. Efficient intervention ladder and early implementation handoff

Do not run the full Cartesian product of every domain × tokenizer × k × rank × graph × seed. Start with the smallest source-supported pilot and diagnostic ladder:

1. Existing lawful baseline, oracle/predicted-domain contrast, one usable typed route and real consumer.
2. Frozen-state versus one new admitted lesson at the identified state owner.
3. Targeted interventions for the suspected bottleneck: representation, retrieval, local transformation, flow connection, orchestration policy or graph relation; unrelated state stays fixed.
4. Only after a candidate demonstrates useful evidence, examine the interaction with its immediate neighbors and a structurally different domain.
5. Extend to all retained domain/consumer obligations through R21–R23, measuring compactness and ongoing input.

If a mechanism cannot be meaningfully isolated, document confounding and design a controlled joint comparison rather than claim attribution. This ladder schedules experiments; it is not a claim they ran or that improvement is guaranteed.

R13 creates the first executable examination recipe; R18's **first deliverable** is the smallest implementation handoff, not a giant rewritten roadmap. A verified narrower handoff may be consumed earlier by a separately authorized implementation task using R04–R08 findings and R12/R13 decisions. Do not wait for R25 or all-domain success when the remaining open questions are unrelated. An unresolved state owner/objective/schema/data permission blocks that dependent experiment; unknown future gain is precisely what the experiment should resolve.

Every such handoff contains: actual current code and consumer; one absent seam; existing alternatives to reuse; exact intended change; lawful input/output example and counterexample; admitted data route; state/update owner and rollback; one baseline/challenger recipe; PC envelope; acceptance/negative-outcome decision; scope exclusions; deliverable and author/reviewer roles. No product implementation is authorized by this planning amendment itself.

## 10. Astra Ultra examination without imaginary independence

The requested reviewer is Astra Ultra. Record requested label and actual available model/version/configuration; do not silently substitute a model or claim that this authoring session ran that reviewer. A separate review is useful only if it inspects the real packet/results and can reject the proposal; a second heading produced by the same session is not an independent run.

Reviewer prompt to include in R15/R24/R25 as applicable:

> Examine this evidence packet, not the author's confidence. Recover the intended user outcome and the exact four-role decisions. For each claimed gain identify the controlled intervention, data leakage route, admissibility evidence, uncertainty and total cost. Try to produce a valid counterexample where the proposed positive candidate should not win, where the learned route loses to the source-faithful deterministic baseline, or where a compact local module hides an unaffordable dependency. Check representation/projection/index compatibility and credit assignment to the true training owner. Separate a design contradiction from an unmeasured hypothesis that has a ready experiment. Return each finding with source, consequence, smallest correction or discriminating experiment, owner, and whether it actually blocks the first lesson. Permit INCONCLUSIVE. Do not generate product tests, expand to a new 30/50-review campaign, or infer efficacy from prose. Explain the recommendation in Russian.

Use blinded candidate labels and, on judgement-sensitive/disputed cases, reversed presentation order; check law-sensitive cases against applicable domain evidence. Reviewer agreement supplements measured task outcomes; it is not an infallible oracle or an automatic numerical acceptance threshold.

## 11. Scientific references and limited applicability

These primary publications establish method distinctions, not that XIIGEN already implements them or that this design will outperform alternatives:

- Rafailov et al., Direct Preference Optimization (2023): https://arxiv.org/abs/2305.18290 — preference objective for suitable probabilistic policies; not an equivalence between arbitrary kNN scores and DPO.
- Hu et al., LoRA (2021): https://arxiv.org/abs/2106.09685 — low-rank adaptation of weights; not automatic domain tokenization or semantic-space alignment.
- Zheng et al., Judging LLM-as-a-Judge (2023): https://arxiv.org/abs/2306.05685 — evaluator limitations motivate bias-aware examination; no assertion about availability or measured quality of the requested Astra Ultra model.

All proposed XIIGEN interventions, pilot sequencing and acceptance records above are plan design, not reported experimental findings. User-provided project policies retain their own source/authority; this document makes no current model-license or legal-admissibility conclusion.
