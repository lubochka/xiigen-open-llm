# Dynamic Flow High-Level Design Amendment v1

## Post-Compiler Integration Note

The corrected high-level shape now has the logical structure compiler as a
pre-stage before the intake gate. The intake gate should receive a checked
meaning structure, not raw text alone.

The budget and confidence gate is a named responsibility between the
sub-subject router and the evidence pack selector. It decides how much local
work may run, whether the local confidence is enough to proceed, and when a
knowledge-edge check is needed.

Arbiter return paths are typed by failure source. A translation-level failure
can return to the local domain translator or domain splitter. An area-routing
failure must be allowed to return to the topology router or sub-subject router.

## Decision

Yes, the high-level design should change, but not by replacing the current dynamic flow.

The current design has the right foundation:

- small specialized units
- editable topology
- registries
- provenance
- arbiters before final formatting
- feedback into future learning

The change should be a narrowing layer around the current flow.

The original flow begins too broadly:

> user prompt -> domain splitter

After examining the full XIIGen document landscape, this is too wide for the goal:

> very small, very accurate LLM units that work fast together

A single first splitter that must understand every possible XIIGen subject will either become large, slow, and vague, or it will make weak early choices. The system needs to choose the problem area first, then choose the smaller subject inside that area, and only then split the request into local domains.

The corrected design should become:

> user request -> intake gate -> system topology router -> sub-subject routers -> local domain splitters -> existing dynamic flow

This keeps the current UML shape, but it stops asking one early unit to understand the whole universe.

## Locked Authority Decision

This decision should be carried into the final architecture as a locked rule:

Priority decides what to attempt.

KNN uses arbiter history to choose better memory.

Arbiters decide what is accepted.

External AI is an advisory help chain, not an accepting authority.

If priority, KNN confidence, route confidence, budget pressure, style preference, historical similarity, or external AI advice disagrees with an arbiter verdict, the arbiter verdict wins unless the user explicitly corrects the system.

## What Changes

### Change 1 - Add An Intake Gate Before The Domain Splitter

The first unit should not split domains yet.

It should answer:

> What kind of request is this?

Examples:

- a user wants a product or business flow
- a user wants to examine a document
- a user wants to build training data
- a user wants to compare versions
- a user wants to generate implementation work
- a user wants to validate a result
- a user wants to repair a previous answer
- a user wants to inspect why the system made a decision

This unit is small because it only recognizes request shape.

It does not need to understand architecture, design, retrieval, security, implementation, or training details. It only decides what kind of journey should start.

### Change 2 - Add A System Topology Router

After intake, the next unit should choose one or more high-level XIIGen engineering areas.

The first working vocabulary should be the topology already derived from the corpus:

1. Source provenance and corpus formation
2. System constitution and non-negotiable rules
3. Dynamic LLM flow pipeline
4. Flow runtime and orchestration
5. Knowledge memory: retrieval and graph
6. Business capability library
7. Design intelligence and UI understanding
8. Implementation factories
9. Data, identity, security, and tenancy
10. Quality, arbiters, simulation, and gates
11. Self-learning and prompt improvement
12. Deployment, environment, and operations
13. Platform translation and alternative targets
14. History, sessions, versions, and repair evidence

This router is still small.

It does not produce a plan. It does not translate the prompt. It does not search all documents. It only says:

> this request belongs in these high-level system areas

### Change 3 - Add Sub-Subject Routers Under Each Topology Area

Each high-level area gets its own smaller router.

Example:

If the system topology router chooses:

> Design intelligence and UI understanding

then a design sub-subject router may choose:

- Figma ingestion
- screen meaning
- component identity
- layout structure
- missing design information
- mobile rendering
- user-facing visual output

If the system topology router chooses:

> Dynamic LLM flow pipeline

then a dynamic-flow sub-subject router may choose:

- prompt splitting
- domain translation
- scheme planning
- parallel unit processing
- result translation
- arbiter selection
- feedback loop
- training-set generation

This prevents one global router from becoming a giant uncertain model.

Each sub-subject router is trained only on the area it owns.

### Change 4 - Move The Domain Splitter Down One Level

The domain splitter should still exist, but it should operate inside the selected sub-subjects.

Old meaning:

> split the whole user prompt across all possible domains

New meaning:

> split the user prompt across the local domains that are relevant inside the selected sub-subject

Example:

For a registration-flow request, the local splitter may emit:

- authentication
- user profile
- email verification
- welcome experience
- audit and events
- failure handling

For a document-examination request, the local splitter may emit:

- source role
- version evidence
- runtime situation
- node training pair
- missing edge
- validation simulation

This is much more accurate because each splitter has a smaller label world.

### Change 5 - Add An Evidence Pack Selector Before Translation

The corpus is large. The system should not let every translator see the whole corpus.

Before domain translation, add a small unit that chooses the evidence pack for this request.

It answers:

> Which source slices are allowed to teach this step?

Examples:

- final documents only
- session history only
- old versions plus final version
- architecture-rule sources
- business-flow sources
- design sources
- generated training-set outputs
- zip-entry metadata only
- readable source content only

This unit protects accuracy.

It prevents a translator from mixing source truth, old drafts, generated outputs, and session repair notes as if they were all the same kind of evidence.

### Change 6 - Split Planning Into Two Levels

The current scheme planner should not carry every planning burden.

Use two planning levels:

1. topology planner
2. local scheme planner

The topology planner connects selected sub-subjects into a flow.

Example:

> document examination -> source role -> version trail -> runtime pairs -> node sets -> simulation -> missing-edge report

The local scheme planner works inside one selected subject.

Example:

> for email verification, create tasks for request verification, token validation, idempotent completion, event emission, and failure messaging

This keeps planning small.

The first planner decides how subject areas connect. The second planner decides the internal work for one area.

### Change 7 - Keep Arrays Explicit

The high-level design must say clearly that these are arrays:

- domain translators
- local scheme planners where multiple sub-subjects are active
- domain processors
- appliers
- result translators
- arbiters

The design should not describe them as one large unit.

Each active domain or sub-task receives its own small unit.

### Change 8 - Add A Result Convergence Gate Before Final Formatting

Parallel work needs a place to meet.

This should not be a large writer unit.

It should be a small convergence gate that answers:

> Do these result fragments agree, overlap, conflict, or leave a gap?

It emits a structured result bundle:

- approved result fragments
- conflict notes
- missing fragments
- fragments that must return to earlier units
- fragments that can be translated for the user

The arbiter selector should still operate before final formatting and should see formal result fragments, not polished user prose.

### Change 9 - Add A Budget And Confidence Gate

Fast small units need a control point.

The system should have a tiny gate that chooses:

- how many units may run
- which units must run now
- which units can wait
- which low-confidence route needs fallback
- when to ask the user instead of guessing
- when to record a learning gap

This is not a reasoning unit that solves the user problem.

It is a speed and confidence controller.

Its job is to keep the system small, fast, and honest.

### Change 10 - Add Arbiter-Aware KNN Memory And A Hard Verdict Boundary

The system should remember arbiter results at the KNN level.

This means nearest-neighbor retrieval should know:

- which examples were approved by relevant arbiters
- which examples were rejected by relevant arbiters
- why rejected examples failed
- which examples are safe positive neighbors
- which examples are negative examples
- which examples are warning examples only

This improves retrieval before generation.

But arbiter history at the KNN level is only guidance. It helps choose better examples and better warnings.

After generation, selected arbiter units judge the produced result. Their verdict is a hard quality boundary.

No route priority, budget pressure, speed preference, style preference, KNN confidence, or similar previous example may override an arbiter rejection.

Priority decides what to attempt first.

Arbiters decide what is accepted.

### Change 11 - Make Unit Registry Lookup Part Of Every Array Stage

The architecture already defines a unit registry. The high-level flow should make its use visible.

At every array stage, the system should resolve:

> role + topology area + sub-subject + local domain -> smallest active unit that is trusted enough

This means the system does not call a generic translator, generic processor, or generic applier.

It calls the smallest unit that fits the selected work.

### Change 12 - Add A Knowledge Edge Path For External AI Help

Small local units should not pretend to know everything.

The architecture needs a controlled path for the moment where the local flow can explain that it reached its knowledge edge.

This path should begin with a Knowledge Edge Detector.

It answers:

> Is this still inside local competence, or do we need outside help?

The detector can be triggered by:

- no trusted local unit
- weak KNN neighbors
- mostly negative or warning neighbors
- unknown predicate pattern
- unknown dependency shape
- missing source evidence
- repeated arbiter rejection
- unresolved convergence conflict
- a request that needs knowledge outside the local corpus
- a request that needs broader expert judgment than the current unit is allowed to provide

Low confidence alone is not enough.

The detector must explain what is missing.

A failed attempt means one complete pass through the affected station that ends with a blocking result, such as the same arbiter rejecting for the same reason, the same dependency still missing, the same convergence conflict unresolved, or the same required evidence unavailable.

The v1 threshold is:

- immediate hard edge when no trusted unit exists, no allowed evidence exists, or outside current knowledge is explicitly required
- two consecutive failures with the same blocking reason after one local repair attempt
- three total failures in the same request path at the same station, even if the reasons are related but not identical

After that threshold, the detector must stop ordinary local retry and choose user help, local corpus search, external AI help, or learning-gap capture.

If outside help is needed, the flow becomes:

> knowledge edge detector -> external help planner -> external context pack -> external AI chain -> external result adapter -> affected local station -> arbiters

The external AI chain may use one supplier, several suppliers, one call, or a sequence of calls.

External AI is not a final authority.

It gives help that must be adapted back into the local logical structure and validated by the normal arbiters.

The external-help event should also create a learning gap packet.

When the IdleDetector reports that background work is allowed, those packets should become small learning sets for the local units, after validation.

## Revised High-Level Flow

The proposed high-level flow is:

1. User request
2. Intake gate
3. System topology router
4. Sub-subject router array
5. Evidence pack selector
6. Local domain splitter array
7. Domain translator array
8. Topology planner
9. Local scheme planner array
10. Unit registry lookup for each planned piece
11. Domain processor array
12. Applier array
13. Result convergence gate
14. Arbiter selector
15. Arbiter unit array
16. Arbiter verdict boundary
17. Result translator array for approved fragments
18. Formatter
19. Style adapter
20. Event trail
21. Arbiter-aware KNN memory
22. Knowledge edge detector
23. External help planner
24. External context pack builder
25. External AI chain
26. External result adapter
27. Learning gap packet
28. Idle learning queue
29. Dataset curator and confidence updater

The current dynamic-flow pipeline remains inside this design.

The main difference is that it no longer starts blind.

## Why This Better Fits Small Accurate Units

Small units are accurate when their label space is small.

A global splitter has too many possible meanings to choose from. It must know about sessions, zips, Figma, business flows, architecture plans, runtime orchestration, retrieval, security, tests, frontend, backend, deployment, and learning. That is not small.

A staged router keeps every decision narrow:

- the intake gate only knows request kind
- the topology router only knows high-level areas
- each sub-subject router only knows its own area
- each local domain splitter only knows local domains
- each translator only translates one selected domain
- each processor only processes one planned piece
- each applier only applies one transformation
- each arbiter only checks one risk type
- KNN memory uses arbiter history without replacing arbiter judgment
- external AI is only called when the local system can name the knowledge edge
- external results return through adaptation and arbitration instead of bypassing the flow

This gives the system speed through parallelism and accuracy through narrowness.

## What Should Not Change

Do not remove the original dynamic flow.

Do not merge the arbiter selector into the formatter.

Do not make one large master unit that reads the whole corpus.

Do not train units on source-document lines when runtime inputs are human requests.

Do not let generated files, old sessions, final documents, zip entries, and binary assets enter training with the same source authority.

Do not let KNN confidence, route confidence, budget pressure, or style preference override an arbiter rejection.

Do not let external AI override an arbiter rejection.

Do not let external AI write directly into the final answer.

Do not turn every low-confidence result into an external AI call. The system must name the missing competence first.

Do not cut the feedback edge. Failed fragments must be able to return to the earlier selected point in the flow.

Do not make result translation a single monolithic step. Approved result fragments should be translated by the smallest fitting result translator, then folded by the formatter.

## Training-Set Consequences

The training sets should be divided by the new topology.

Each unit receives examples shaped like its runtime input.

The intake gate receives human requests and emits request kind.

The topology router receives human requests plus request kind and emits high-level system areas.

Each sub-subject router receives the same request plus one chosen area and emits sub-subjects inside that area.

Each local domain splitter receives a request plus selected sub-subject and emits local domain labels.

The evidence pack selector receives the selected subject path and emits allowed source packs.

The translator receives a user fragment, selected local domain, and selected evidence pack, and emits the formal domain wording.

The planner receives translated formal wording and emits planned pieces with dependencies.

The processor receives one planned piece and emits its processed result.

The applier receives one processed piece plus one transformation request and emits the applied result.

The convergence gate receives all formal result fragments and emits agreement, conflict, gap, and approved-fragment markings.

The arbiter selector receives formal result fragments and context, and emits the arbiter list.

The arbiter receives one formal result fragment and one judgment type, and emits accept, flag, or return-for-reprocessing with a reason.

The result translator receives an approved formal fragment and emits user-language content.

The formatter receives approved translated fragments and emits one readable answer.

The style adapter receives the readable answer plus user style preference and emits the final style-adjusted answer.

The knowledge edge detector receives route, unit, KNN, convergence, arbiter, source-evidence, and confidence signals, and emits local-continue, user-help-needed, external-help-needed, or learning-gap-only.

The external help planner receives a named knowledge edge and emits the supplier plan, call sequence, allowed context, and required answer shape.

The external context pack builder receives the selected help plan and emits the exact structured context that may be sent outside.

The external AI chain receives the context pack and emits one or more outside answers with supplier identity and uncertainty.

The external result adapter receives outside answers and emits a local logical structure, local station input, warning, or unusable-result notice.

The learning gap packet receives the full event and emits future learning material for idle training.

The IdleDetector triggers the idle learning queue when no active user request needs the same resources, compute budget allows it, and rollback plus conformance comparison can be performed.

The idle learning queue groups accepted packets by unit role and domain, then creates small learning sets outside the hot path.

## Minimal Amendment To The Existing Draft

The current draft should get one new section:

> Locality And Narrowing Before Domain Splitting

That section should state:

> The domain splitter is not the first intelligence in the system. A request first passes through intake, topology routing, sub-subject routing, and evidence-pack selection. The domain splitter operates locally inside the selected subject area. This preserves small unit size and prevents a single splitter from becoming a large general-purpose classifier.

The cast of roles should add:

- intake gate
- system topology router
- sub-subject router
- evidence pack selector
- result convergence gate
- budget and confidence gate
- knowledge edge detector
- external help planner
- external context pack builder
- external result adapter
- idle learning queue

The cast of roles should also clarify:

- translators are arrays
- processors are arrays
- appliers are arrays
- result translators are arrays
- arbiters are arrays
- the arbiter selector operates before final formatting
- external AI is an advisory chain, not an accepting authority
- external-help outputs must be adapted back to the local flow and arbitrated

## Bottom Line

The design should change from:

> one broad dynamic pipeline

to:

> a narrow staged topology that activates many tiny pipelines

That is the better shape for fast, accurate, small LLM units.
