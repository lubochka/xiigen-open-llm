> **RP04 V3 runnable examiner binding:** [Direct four-domain + mixed/unknown starter cases, raw result schema, causal P/C0/U discipline and risk-based check table](RP04-V3-LEARNING-POINT-AND-EXAMS.md). Existing scientific requirements below remain; V3 is a single compact execution index, not extra model runs or a new exam board. A 2-domain first pilot is provisional, never a generalization PASS.

# RP04 — scientific learning examination, domain passports and counterexamples

**This is an experiment specification, NOT actual measurements.** Learning is not deterministic, so no set of coverage tests proves a better policy. A tiny set of focused engineering checks still protects deterministic law, authorization, persistence, cancellation and typed data. Independent outcome grading, controlled interventions and fresh held-out families establish or falsify the *learning* claim.

## 1. A fair before/after study (no invented success)
Freeze and hash before trials: training pool P, model parent, inference profile, law sources and dates, domain/intent map, RP02 vector+tokenizer+LoRA projection and kNN version, RP03 Unit/Flow state, candidate generator, initial graph topology, arbiter rubric, hardware, limits, seeds and evaluation panels C0 and sealed T. Training examples/case families never enter T; near duplicates, same law problem transformed superficially, answer snippets and structural graph motifs leaking across families must be partitioned. Maintain same pre/post pipeline; old-code diagnostic is a separate experiment, not a factor inside learning-gain comparison.

Cases have law/intent/domain labels, question, permitted actions, source version, typed output requirements, candidate pool, route/pattern alternatives, outcomes/unknowns, independent blind grading rubric and expected structural family. Sample sizes N and uncertainty intervals are chosen and **reported** before examining T; no fake numbers, fixed universal 95% target or after-the-fact changing a rubric. Pairwise within-case comparisons where possible.

Run P0 frozen parent without update; C0 no-update control with identical state management; U isolated update candidate. C0/U receive the same eligible opportunities and equal budget. Post-update score each by the ordinary API/host under a cold reload, not the trainer's test shortcut. Holdout T remains sealed until final decision.

## 2. Three distinct causal questions
**E3-R:** Freeze graph/Unit/Flow/kNN, same candidate plans, same law/arbiter. D=fixed legal scheduler; L=learned route ranking within mandatory law guards; H=hybrid safe scheduler + learned ranking, optional pre-existing deterministic optimizer where applicable; H-no-E3=hybrid with frozen/no-update route state. Examine law-valid final end-task success, route quality/regret when an oracle is available, wait and critical-path behavior, all branch resources and learner state attribution. Falsifier: learned routing adds delay/cost without held-out improvement or violates a safety rule.

**E4-G:** Freeze RP02, RP03, E3 and constituent primitives; compare flat/order-1 graph vs admitted order-2 vs admitted order-3 vs each trained/no-update version, using *unseen structural combinations of seen primitives*. Acceptance can require equal or better quality with lower cost under matched budgets, or significant new-composition success with bounded extra cost; threshold is documented ahead of execution. Falsifier: graph helps only memorized motifs, edge count increases but final accepted correctness or structural transfer does not.

**E3×E4:** only after separate E3 and E4 evidence, one targeted 2×2 factor contrast with frozen law, candidate inventory, time/memory and evaluation panels: route old/new × graph old/new. Report interaction and confounding, do not claim two improvements if only one changed factor. RP02 domain mapping/kNN ablations remain separately owned; RP04 logs domain errors but does not redefine RP02.

## 3. Metrics measured per example, domain and bulk
| Layer | Required actual metric | Failure/unknown treatment |
|---|---|---|
| Domain | correct domain+intent and mixed/unknown identification; law source applicability, version; mapping confidence/calibration | reject or ask clarification; never force single domain label |
| Path | valid typed dependency path, requested output payload, constraint/arbiter adherence, path regret vs verified alternative, missing prerequisite detection | invalid path=not accepted even if fluent |
| Result | independent rubric quality, accepted solution rate, judge agreement, error categories, baseline retention | TIE/REGRESS/UNJUDGEABLE explicit |
| Async | wall ms p50/p95/time-to-first-lawful-accepted-output, task-level latency, each launched/cancelled/failed/time-out branch CPU/GPU ms and tokens, peak concurrency | do not count only winning branch |
| Graph | unseen structural-family accepted success, correctness of order/ports/output refs, number of admissible patterns, pattern search/preload and cold-run costs | edge count and graph-size alone NOT utility |
| PC | peak RAM/VRAM, disk bytes weights+index+codebook+graph+journal, CPU/GPU power/energy if measured, load/reload/rollback time, idle-training interference | NOT_MEASURED if instrument missing |
| Learning | effective new accepted signals, unique pair_id, before/after deltas with CI, no-update control, retention/drift per earlier domain, update time | optimizer loss or SHA cannot alone prove improvement |

Report for bulk b: task family composition, count, acceptance/time/cost denominators, comparator identity, confidence intervals and differences Δquality_b, Δlatency_b, Δresources_b, change versus previous bulk under SAME frozen panel; monitor learning curve and data efficiency (accepted problems per valid new training example and per elapsed training minute). If dataset composition changes, stratify and do not misattribute bulk drift to learning. Preserve negative outcomes as learning evidence, never re-run repeatedly until positive without pre-registration.

**Compactness hypothesis:** for a matched accepted-solution threshold, hybrid D+L consumes less total active compute/storage or solves more unseen tasks under one PC budget than D and L alternatives. Account for inference, failed branches, training, vector indexes, graph/cache/journal and cold-load. Report the actual Pareto frontier (quality/time/bytes), not an assumed 20× or a single FLOPS estimate.

## 4. DPO pair science and correct operator order
Lesson request source must have approved provenance and lawful source; CODEX GPT-6 Astra UltraCode chooses one narrow learning weakness from the teach-only pool. External approved open Chinese model authors BOTH final chosen and rejected for the exact frozen x/context (not just paraphrases or teacher labels for Student outputs). Separate CODEX GPT-6 Astra UltraCode judges those exact bytes with law reference, preference margin, meaningful negative, ambiguous answer, unsafe rejection and license. TIE/BOTH_BAD/UNJUDGEABLE goes to quarantine; accepted and rejected responses must remain unchanged to optimizer. Unique pair checksum includes domain+law+prompt+context+teacher bytes+model/version. Trainer's frozen reference policy is not CODEX.

Route/graph cases can *inform* CODEX lesson choice, but cannot turn an observed failure into an automatic final rejected answer or silently train directly from unjudged branch logs. Only declared authorized recipient receives a change (Student LoRA in RP01; an E3/E4 scorer only if that distinct mechanism genuinely consumes and updates the admitted signal). Training feedback from user input is evaluated **before** any update and cannot bypass this exact S04 teacher/judge contract.

## 5. Eight law-aware domain passports (templates, not proved domain models)
Each executable passport must record [id, definition, examples, counterexamples, jurisdiction/temporal version where applicable, atomic semantic units, input/output types, source/evidence, expected domain+intent, compatible vector/tokenizer+LoRA projection version, dynamic kNN candidate policy from RP02 or frozen comparator, allowed patterns, hard laws, abstention path, useful typed output, planned held-out family]. Never claim universal best kNN from one domain.

| Domain family | Distinguishing invariant and token/representation axis | E3 route and E4 unseen composition exam |
|---|---|---|
| 1. Account identity/onboarding | repeated provider callback idempotence; expiry/consent-before-activation; token: event/order, actor, policy version | async identity + questionnaire + consent; order-3 duplicate+expiry+resume under conflicting stakeholder request |
| 2. Programming C#/Angular/SQL | typed signature, nullability, version/stack markers before normalization, transaction correctness | eligible compiler/reviewer/unit fan-in; previously unseen safe C#–SQL–UI migration dependency |
| 3. Design-to-code/DTC | page state, component hierarchy, accessibility and permitted target platform, authored visual/layout semantics | async screenshot/requirements/component output; novel design-to-code with partial evidence and a source correction |
| 4. HE↔EN translation | negation, polarity, numbers, entities, RTL/direction, semantic meaning not exact wording | translator + legal term reviewer + style correction; unseen bilingual structure with two plausible forms |
| 5. Quantitative units/physical world | dimensional units, conversion direction, value and precision; exact hard law | async independent conversion+consistency checks; novel two-step quantity chain where fluent answer violates units |
| 6. Real-world legal/procedural rules | jurisdiction, effective date, applicable rule and exception; source/citation + uncertainty; do not invent legal rule | source verification→qualification→arbiter; unseen exception+effective-date+cross-jurisdiction combination; abstain on stale law |
| 7. Local knowledge/IDE project wiki | project version, file provenance, graph reference, offline/online permission; corpus boundary | retrieval→unit→project-specific correction; new cross-file dependency and stale local index detection |
| 8. Mixed/unknown or constrained external knowledge | multiple domain candidates, confidence, privacy/permission and missing-knowledge warning | identify underdetermination, request clarification, quarantine forbidden branch; unseen conflict with adversarial prompt |

Start a bounded cross-domain pilot using at least identity, programming and mixed/unknown plus one multilingual family; expand to all eight with balanced seen/unseen structural families without waiting for perfect corpus. Count domain law violations individually and preserve unknown labels. Domain-specific learned kNN/tokenization/LoRA changes are RP02/RP01 factors and must be frozen when attributing E3 or E4 gain.

## 6. GOOD/BAD/EDGE scenarios and falsifiers
**GOOD E3:** a user's onboarding request yields two lawful alternatives; learned E3 picks consent check and expiry verification before activation, runs independent evidence in parallel, preserves one failed branch, saves wall time at equal accepted correctness; independent arbiter accepts result and cold path shows route version changed.

**BAD E3:** picks fastest finished branch which creates duplicate account or skips consent; marks unlaunched branch as negative reward or hides cancelled cost; emits PASS from green Windows CI. Must not be accepted.

**EDGE E3:** timeout arrives after one branch published; cancellation while fan-in holds an OutputRef; partial stale coordinate schema; authorization withdrawn mid-run; idempotent retry vs non-idempotent mutation; two branches disagree. Return stable error/wait/rework with cost and provenance, never silent continuing.

**GOOD E4:** familiar validated "identity" and "consent" order-2 relations compose with a new resume/expiry pattern (order-3), preserving actor, rule, output associations and final arbiter; held-out structural success improves versus order-1 with frozen underlying model.

**BAD E4:** new edge/NodeIds boost hit counts but actual next Unit still receives wrong OutputRef; structural family from training leaks to holdout; graph promotion infers "law" from LLM score. Must not be accepted.

**EDGE E4:** cycle, mixed law versions, equal vector dimensions but different coordinate spaces, cross-tenant edge, vanished referent, graph update concurrent with a running consumer, rollback after power loss. Reject/abstain atomically and run on last accepted snapshot.

## 7. Review and stop/go
An independent architecture and learning reviewer (explicit CODEX GPT-6 Astra UltraCode invocation, NOT asserted by these files) receives frozen exact final SHA, source-to-consumer traces, D/L/H panel split, negative examples, feature/authority map, all actual measurements and unrepaired gaps. Verdict: PASS for demonstrated capability; NEEDS_FIX, NOT_MEASURED or BLOCKED for actual gaps. No state promotion with unknown quality or rights. A future replay/new feedback cycle changes one declared factor and logs retention; not a synthetic test-coverage expansion.
