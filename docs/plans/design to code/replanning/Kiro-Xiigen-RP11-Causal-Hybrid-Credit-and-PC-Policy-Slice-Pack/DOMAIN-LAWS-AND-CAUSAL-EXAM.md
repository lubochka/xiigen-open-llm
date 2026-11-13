# RP11 — laws, admissible route contrasts and causal science

## Always identify subject and legal context before route credit

Domain law is not a bag of lexical hints. Produce a passport with subject, intent (CODE_Q / BUG / PLAN / ARCH / REFACTOR when applicable), language, project/release, source rights, representation/token-vector schema versions, core invariant, mandatory arbiter/verification, negative examples, UNKNOWN/MIXED behavior and a versioned independent evaluator. A correctly abstaining domain mapper may be better than a confident wrong mapper. RP02 owns implementation of mapper/kNN; RP11 uses its real output as evidence, never refactors that module silently.

| Subject / independent family | What a correct answer must preserve | GOOD illustrative held-out probe | BAD and failure / abstention edge |
|---|---|---|---|
| C# code documentation | nullable, async/await, exceptions, generic constraints, caller→callee behavior | Document method whose nullable return differs from empty collection; retain meaning and contract | Claim it always returns a non-null value because nearest example described another method |
| EN↔RU translation | negation scope, aspect/time, entity/numeric identity, register; not literal similarity | Translate "must not retry unless permitted", correctly scoped permission | Drop not/unless; vector nearest looks semantically close but violates obligation |
| SQL documentation | JOIN cardinality, NULL, WHERE/HAVING, transaction isolation, schema/version | Explain LEFT JOIN missing rows without claiming INNER JOIN effect | Treat NULL as false without SQL 3-valued logic, or invent a schema |
| Angular documentation | actual Angular/version/signals/RxJS semantics, change detection, DI boundary | State whether unsubscribe is required for the actual observable pattern | Use deprecated API or conflate Angular release with AngularJS |
| Figma→HTML/CSS | spatial, responsive, layout semantics and real browser screenshot quality at specified viewport(s) | Render two breakpoints and preserve intended visual hierarchy | "Correct" CSS text that was never rendered or breaks mobile layout |
| Async orchestration | dependency, side effects, cancellation, idempotence, timeout, rights | Fanout lawful read-only branches and cancel one without losing typed provenance | Compare a 4-branch GPU route on winner's milliseconds only; allow double side effect |
| Builder / teaching policy | legal weakness diagnosis, useful pair opportunity, independent downstream Student improvement | A new domain-legal lesson selected by CODEX after actual weakness signal | Mark Builder better because it produced many pairs without improved Student |
| Extras | source-discovered additional original subjects/languages/intents | Create family passport and owner after direct source read | Erase extras because seven passports exist |

**Ordering:** C# and EN→RU are first two *contrasting* pilot subjects where rights and existing ports permit; no seven-domain exhaustive acceptance gate before first credit experiment. Keep extras with owner/trigger. Each family must have source-family-disjoint train/dev/exam and independent scoring; do not leak exam question or reference to Teacher/Builder/selector. Frozen population difficulty and no-test-set tailoring.

## What exactly is contrasted

RP11 must reuse R22's pre-registered C00–C11 factors and RP09's independent exam definitions where present, without renaming or re-executing those owners. Its new **algorithm intervention** is causal credit feedback and compact online prediction, **not** raw tokenizer, kNN, Unit, Flow, Graph or Student implementations.

| Pair | Frozen/common | Single changed factor | Valid claim |
|---|---|---|---|
| P (existing safe route) vs N (same pipeline, credit disabled) | candidate set, domain passport, code/evaluator version, budget | optional credit receipt/instrumentation only | estimation overhead / non-interference |
| N vs U (credit state promoted after independent evidence) | same lawful pool, model state and scoring rules, action budget | bounded learned credit state used by same RP04 selector | incremental credit-policy value |
| U vs U-no-learning (sham no-update with identical telemetry) | same feature schema and logs | actual online sufficient stats updated vs frozen | distinguishes learning from static heuristic |
| U vs fixed rule vs stochastic route ranking | same eligible candidate IDs and safety | selection method | dynamic policy benefit/time/PC |
| Relevant component arm X0/X1 | freeze other components, learned state, dataset | only X (e.g., actual multi-neighbor use or Graph higher-order policy) | conditional contribution of X, **not** cumulative shared gain |
| If required X/Y 2x2 | freeze all else | two interacting component switches | interaction, not assumed additive |
| Historical L vs N | only if compatible and authorized | engineering pipeline bug fix | engineering delta only, **not** credit-learning gain |
| Novel bulk B_t vs B_(t+1) | equivalent anchored independent families, same measurement policy | one or more truly novel admitted lessons | continued learning and retention, not replay |

If independent quality outcomes don't exist, **no causal learning claim**. If alternatives weren't both eligible to execute (safety/rights), do not fill missing counterfactual with invented data; return NOT_IDENTIFIABLE. If route is never reached by ordinary consumer, not adopted.

## Minimum pre-registration before actual run

~~~yaml
hypothesis: "credit policy reduces wrong-law path choices at no unacceptable whole-PC cost"
owner: "RP11 credit estimator; RP04 is route selection owner"
code_sha: ACTUAL_SHA_AT_RUN
state_P: ACTUAL_PARENT_ID
state_N: ACTUAL_NO_UPDATE_ID
state_U: ACTUAL_TRIAL_ID_IF_GRANTED
representation_versions: ACTUAL_IDS
candidate_set_and_arbiter_version: ACTUAL_IDS
domain_family_manifest: SOURCE_FAMILY_DISJOINT_MANIFEST
law_and_blind_evaluator: ACTUAL_ADMITTED_ASSESSOR
sampling_and_assignment: PREREGISTER_BEFORE_OUTCOMES
arms: ACTUALLY_EXECUTED_ARMS_ONLY
rights_and_effect_permits: ACTUAL_GRANTS_OR_NOT_AUTHORIZED
time_and_full_pc_budget: ACTUAL_EXISTING_QUOTA_NO_GUESS
effect_and_uncertainty_rule: REUSE_R13_R22_OR_OWNER_DECISION
stopping_and_rollback: REUSE_R23_AND_GOVERNANCE
ordinary_consumer: NAMED_K_CALLER_OR_NOT_OBSERVED
~~~

## Metrics — functional, not coverage

- Domain ID and law compliance: confusion including UNKNOWN/MIXED, severity/critical failures, uncertainty; code/translation/visual arbiter specific.
- Route: regret versus **best enumerated permitted path within same budget**, not unobservable global optimum; top-route selection rate, alt paths considered, untried branches and cancellation.
- Credit: predicted utility calibration, calibration drift, per-factor conditional uplift, confidence/effective-N and detected interaction. If features leak outcomes or policy selection, invalidate.
- Learning: actual distinct accepted state delta and novel pair IDs, Student quality before/after on independent families, retention/worst domain and cold state selection.
- Efficiency: wall p50/p95 plus time to first *useful and lawful* solution; CPU time, peak RAM, VRAM if observable, disk/checkpoints, total branch GPU/CPU and retries/cancels, encoder/index/model overhead, credit instrumentation overhead, total cost per correct task, novel lessons per bulk. Clearly label not observable units UNKNOWN; don't invent Joules.
- Meaningful progress: no-update vs intervention delta with paired uncertainty and failure taxonomy. Quality gain after cost explosion may be a loss; use pre-declared Pareto/admissibility, not silently reweight afterward.

## Synthetic pedagogic cases — NOT runtime data

**Illustration GOOD:** Both A (fast deterministic) and B (slightly slower hybrid) obey C# nullable law. On held-out independent C# family, blind assessor labels B more complete; with all other factors frozen the credit update increases selection of B on similar unseen contracts. On EN→RU negation prompts A may remain better. Record separate family statistics, no "overall 95%" from these stories.

**Illustration BAD:** Student got a correct answer, but kNN retrieved 10 examples and Graph has 20 nodes; the report distributes one success equally among all algorithms. This is *not causal credit*. If a wrong SQL example is close, do not train with it as a preferred output or override mandatory arbiter.

**Illustration EDGE:** unknown version/stack, mixed Angular+C# documentation, two lawful paths with exact tie and one cancelled expensive branch. Return abstain/tie, assign no spurious learning credit, bill launched branch, preserve association and select safe lawful deterministic fallback.

## Experiment verdict

Every run produces IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE / NOT_MEASURED, separately for route quality, Student learning, retention, cost and adoption; explicit causal threats (selection bias, contamination, mutable candidate pool, multi-factor change, missing legal control, examiner leakage). Task completion ≠ guaranteed positive outcome. Only independent exact-SHA Astra UltraCode review can elevate review status.