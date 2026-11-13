# RP11 — dynamic hybrid credit architecture (design candidate, not product proof)

## Human diagram

~~~text
User task + project/version/rights
    | domain / intent / stack / UNKNOWN-MIXED / semantic law
    v
[Deterministic eligibility + typed contracts + mandatory arbiters]
    | frozen lawful candidate set (invalid candidates are excluded)
    +--> deterministic-only path ---> final arbiter
    +--> BM25 + domain kNN -------> LearningUnit many->many result with input-associations
    +--> learned Student/LoRA ----> Flow input->output correction
    +--> async Orchestrator: fanout, fanin, cancel, all-path total cost
                +--> DynamicGraph: typed order-2/order-3 pattern relation
    | origin + evidence + warnings + law verdict + all costs
    v
RP11 causal provenance ledger (no raw private examples by default)
    | fixed independent outcome & bounded matched contrasts
    v
RP11 credit estimator: per-factor effect / interaction / uncertainty / total PC
    | admitted outcome, non-claimed attribution when unidentifiable
    v
RP11 small online contextual credit state (version + parent + no-update)
    | lawful path desirability hints (never acceptance authority)
    v
RP04 route policy + final arbiter -> real ordinary selected-state consumer
    |
    +--> optional RP05 weakness evidence -> CODEX/Astra ultra chooses lesson
          -> external approved open Chinese Teacher writes BOTH final DPO sides
          -> CODEX/Astra ultra separately judges exact immutable pair
          -> existing RP01 Student trainer; RP09 independent student evaluation
~~~

## Four separate learning roles (not four trainer stacks)

**Unit:** input includes domain, input vector schema/version, expected output type; returns 0..n typed results for 0..n inputs (many→many) with stable sourceInputIds, association/resultIds, law confidence and warnings. Never flatten to first result. RP11 records contributing associations, does not rewrite the Unit.

**Flow:** versioned lawful OutputRef alignment and correction from prior Unit's output to next Unit's input; RP11 observes whether correction helped independent downstream quality with frozen Units. Parent/child association preserved under reorder, duplicate, partial and cancellation.

**Orchestrator:** asynchronously executes multiple eligible combinations under explicit concurrency/side-effects/budget limits; owns scheduling/selection. RP11 records every launched/failed/cancelled branch's elapsed/resource cost and counterfactual constraints; credit feedback is informative, not permission to bypass ordering, budget or arbiter.

**DynamicGraph:** higher-order pattern relationships, semantic OrderedPatternIds and legal composition; RP11 tests structural generalization to unseen combinations, attributing a learned graph effect only with frozen lower-level components. Pattern-count growth is not graph learning.

Existing ILearningMachine remains owner of real Student weights, DPO and permitted adapted state. A deterministic transform over learned weights is compatible with learning; a fixed rule with no parameter change is not new learned credit state.

## Versioned typed contracts — candidates, not invented existing C# APIs

~~~text
CausalTraceV1:
  traceId; requestFamilyId; domainPassportId; intent; stack; projectVersion
  rightsScopeId; panelVersion; candidateSetVersion; lawAndArbiterVersion
  semanticRepresentationId; tokenCodecVersion; vectorSpaceVersion; projectionId?
  knnIndexVersion?; neighborIdsOrdered[]; actualNeighborUsageIds[]
  unitInputIds[]; unitOutputAssociationIds[]; flowCorrectionIds[]
  orchestratorBranchIds[]; graphOrderedPatternIds[]; studentAdapterVersion?
  candidateRoutes[]: {routeId, lawful, arbiterVerdict, sourceAssociations,
     stateOwnersAndVersions, startStopAndCancelReason, executionBudget,
     wallMs, cpuMs, peakRssBytes, peakVramBytes?, energy?}
  selectedRouteId; independentOutcomeRef?; evaluatorVersion?
  consentAndDataRightsRef; piiRedactionStatus; codeSha; immutableEventHash

CreditObservationV1:
  traceId; eligibleRouteIds[]; comparedRouteIds[]; pairedPanelFamilyId
  interventionFactor; frozenOtherFactors[]; assignmentMethod; propensity?
  qualityScoresByRoute?; domainLawFailures[]; uncertaintyEvidence
  totalCostByRouteAndInstrumentation; C0NoUpdateState; trialStateVersion?
  attributionStatus: IDENTIFIED | ASSOCIATION_ONLY | UNIDENTIFIABLE | INVALID
  owner: Unit|Flow|Orchestrator|DynamicGraph|Student|Representation|KnnMemory
  noAutoLessonAdmission: true

CreditStateV1:
  parentHash; creditSchemaVersion; domainCodebookVersion; featureExtractorHash
  lawVersion; onlineEstimatorVersion; allowedWeightsAndSufficientStats
  sampleCountByDomainRoute; effectiveSampleSizes; uncertaintyByDomainRoute
  consentLedgerRef; replayDispositions; lastAcceptedIteration; stateHash
~~~

Do not store complete user prompt, sealed exam contents, Teacher sides or hidden answer in trace. For source-family-disjoint panel, use non-reversible family IDs. Rights to share a trace are separate. Mismatched domain vector versions fail closed or use controlled baseline, never "best effort" reinterpretation of coordinates.

## Algorithm A — lawful candidates before statistics

1. Compile domain passport + intent + stack/version and collect typed candidates from existing pipeline. Treat domain UNKNOWN/MIXED as a valid abstention with scoped fallback; do not force one label.
2. Reject paths violating permissions, type/flow law, hard budget, mandatory required arbiter or source-law invariants. Record rejection reason; excluded paths are never scored as trainable negatives solely for being deterministic.
3. Freeze candidate IDs/state/version, evaluator, eligible set and budget before outcome labels. Any later KNN reindex, tokenizer/vector change or model update creates another cohort/experiment.

## Algorithm B — paired causal credit instead of naive success counts

For a comparison of paths A and B within the same legal candidate set, same input-family distribution and equal measurement policy, compute deltaQuality = Q(A)-Q(B), deltaCost = C(A)-C(B). Q is domain-law rubric plus independent semantic result, not training loss. An observational A/B difference is association unless assignment/covariates support identification.

For component X, compare treatment and control with every *other relevant factor frozen*: X0 vs X1; if X interacts with Y, run a small preregistered 2x2 for X/Y when legal and affordable; interaction = [Q(X1,Y1)-Q(X0,Y1)]-[Q(X1,Y0)-Q(X0,Y0)]. Do not sum isolated improvements as an additive "hybrid gain" when interactions are unmeasured.

Use paired family-level statistics and report effect distribution, effective N, interval/uncertainty, ties, abstentions, contamination, assignment route, invalid runs and per-domain worst-case. No arbitrary 95% acceptance from a single tiny bulk: 95% is a separate long-run product target requiring the owner-defined domain rubric and noise-aware sample.

## Algorithm C — compact learned credit state, not a static heuristic dressed up

Start safe from existing deterministic policy, then maintain small domain/route-feature online estimates. Features may include domain-law passport, kNN neighbor-disagreement, actual multi-hit usage, Flow corrections, graph order and pattern novelty, Student selected adapter, uncertainty and full PC load. For lawful evaluated actions, update *only* independently admitted training-view sufficient statistics or a bounded regularized online predictor on a stable feature schema:

~~~text
prediction = clipped(dot(weights_domain, features_route), permitted_range)
residual = independentUtility - prediction
weights_domain = bounded_regularized_update(weights_domain, residual, features_route)
value(route) = qualityEstimate - lambdaCost*normalizedWholePcCost
safeSelection = rank(eligibleRoutes, value, confidence, explorationCap)
if no credible evidence, unknown domain, law fail, or budget risk: fallback existing lawful arbiter path
~~~

This sketch is a hypothesis, **not a chosen optimizer/hyperparameter or a claim of shipped training**. Task 03A compares constant/fixed, online linear, contextual bandit/UCB/Thompson under the same frozen panels, chooses minimal data/PC and records why; actual numeric thresholds/budgets must come from R13/R22 and current owner. Exploration never invokes unauthorized model or side effects. Low sample requires conservative interval/abstention, not aggressive policy changes.

## Algorithm D — learning allocation does not impersonate human DPO choice

RP11 can emit a **ranked diagnosis** such as "EN→RU negation loss likely at semantic projection; Flow correction versus kNN disagreement undecidable without contrast". The learning scheduler suggests an owner and expected benefit/cost; CODEX GPT-6 Astra UltraCode alone selects the legal teaching *case*. A Chinese open Teacher creates both final chosen and rejected; CODEX separately assesses the exact pair. RP11 must not manufacture final rejected from an invalid deterministic answer, user correction or heldout exam. Actual state updates use existing RP01/RP05 at-most-once and PC rollback; route-credit learning and Student-weight learning are distinct parameters/outcomes.

## Example, counterexample, boundary

**GOOD:** C# documentation task where two lawful paths produce different explanations of nullable type, both pass permissions, blind adjudicator prefers path B, total PC B fits budget; paired B vs A credit favors Graph correction only after a frozen-ablation establishes its contribution; new credit state changes cold route selection on an unseen same-family task without increasing wrong SQL cases.

**BAD:** Route uses 4 parallel branches but only reports winner's 240 ms and score; treats k=8 index hits as 8 used neighbors though KnnLogicalUnit returns hits[0]; awards graph learning because OrderedPatternIds count rose; labels deterministic A as DPO rejected despite correctness; equates new state hash with quality increase.

**EDGE:** Unknown mixed C#/SQL prompt; domain classifier abstains, legal fallback returns correct deterministic candidate; budget overrun cancels GPU path; partial Unit results arrive out of order. Preserve association IDs, record all launched cost, keep canonical lawful result, and return UNKNOWN for unidentifiable learned credit.

## Acceptance falsifiers

A positive RP11 claim is false if: route wasn't used by ordinary consumer, policy was unchanged or no cold reload; compared arms altered encoder/evaluator/driver; a law/arbiter rejection was overridden; coverage/tests used as quality; no paired independent heldout panels; user feedback was sent to Teacher without consent; PC total excludes instrumentation/training; or only operator notes with no real code/state were produced.