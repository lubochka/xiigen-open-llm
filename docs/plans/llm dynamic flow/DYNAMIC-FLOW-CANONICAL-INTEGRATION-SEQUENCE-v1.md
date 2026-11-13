# Dynamic Flow Canonical Integration Sequence v1

## Purpose

This document gives one canonical end-to-end order for the dynamic flow after the architecture amendments.

It connects:

- the original dynamic flow architecture
- the high-level narrowing amendment
- the logical structure compiler
- arbiter-aware KNN memory
- the knowledge edge and external AI path
- learning gap packets
- idle learning

The purpose is to prevent later implementation plans from choosing different integration orders.

## Validation And First Implementation Notes

The sequence is no longer only design intent. E51B validated the runtime shape
in a closed mini flow: 6/6 traces passed, CH-01 produced a named knowledge-edge
context pack instead of inventing an answer, and CH-02 kept arbiter authority
above a high-similarity neighbor.

The first implementation slice for projects 21-23 maps this sequence into five
runtime components: InternalLlmGateway, InternalResultExaminer,
ExternalAiRequestPackage, ExternalAiResultAdapter, and LearningEventScheduler.
E52 then compares the before/after cycle and shows 0% baseline contract success
versus 100% enhanced contract success across V06, CH-01, CH-02, and IDLE-01.

## Locked Authority Decision

This decision should be treated as locked:

Priority decides what to attempt.

KNN uses arbiter history to choose better memory.

Arbiters decide what is accepted.

External AI is an advisory help chain, not an accepting authority.

If priority, KNN confidence, route confidence, budget pressure, style preference, historical similarity, or external AI advice disagrees with an arbiter verdict, the arbiter verdict wins unless the user explicitly corrects the system.

## Canonical Runtime Sequence

### 1. Runtime Input Arrives

Input:

- user request
- optional attached documents
- current conversation context
- user constraints

Output:

- raw request packet

The request is not split into domains yet.

### 2. Source Role Gate

The system identifies what kind of input it is reading.

Examples:

- runtime user request
- document source
- session evidence
- correction
- feedback
- generated artifact
- failed result
- approved result

Output:

- source role
- source authority
- source location
- allowed use

### 3. Logical Structure Compiler

The request enters the shared compiler.

The compiler chain is:

1. language and pattern translator
2. context marker detector
3. structural role tagger
4. semantic normalizer
5. family collapser
6. logic predicate extractor
7. meaning graph assembler
8. fidelity and coverage checker

The marker detector must run before normalization.

Output:

- checked meaning graph
- logical predicates
- source evidence
- missing or ambiguous meaning
- confidence reasons

The topology router receives this checked meaning graph, not raw text.

### 4. Intake And Topology Narrowing

The system performs staged narrowing.

The order is:

1. intake gate
2. system topology router
3. sub-subject router array
4. evidence pack selector
5. local domain splitter array

Each stage uses the checked meaning graph and the previous stage output.

The goal is to keep every label world small.

### 5. Evidence Pack Selection

The evidence pack selector chooses which source material is allowed to influence the next stage.

It may select:

- final documents
- session history
- old versions plus final version
- generated artifacts
- feedback records
- arbiter history
- external-help packets

It also names forbidden evidence.

### 6. Arbiter-Aware KNN Memory

Before generation, the KNN layer retrieves memory.

It retrieves:

- positive neighbors
- negative neighbors
- warning neighbors
- arbiter approval history
- arbiter rejection history
- rejection reasons
- similar learning gap packets

KNN chooses useful memory.

KNN does not approve work.

### 7. Budget And Confidence Gate

The budget and confidence gate decides:

- how many units may run
- which route should be attempted first
- whether local retry is worth trying
- whether the user should be asked
- whether the knowledge edge detector must inspect the path

This gate may control cost and order.

It cannot override arbiters.

### 8. Unit Registry Lookup

For each selected role and local domain, the system resolves:

role + topology area + sub-subject + local domain -> smallest trusted unit

If no trusted unit exists, the knowledge edge detector receives that fact.

### 9. Local Dynamic Flow

The local flow runs:

1. domain translator array
2. topology planner
3. local scheme planner array
4. domain processor array
5. applier array
6. result convergence gate

The convergence gate checks whether formal fragments agree, conflict, duplicate, or leave gaps.

It does not make final semantic-quality judgments.

### 10. Knowledge Edge Detector

The knowledge edge detector watches several points:

- after evidence pack selection
- after unit registry lookup
- after convergence
- after arbiter rejection
- during dataset curation

It receives:

- route confidence
- unit availability
- KNN neighbor quality
- negative and warning neighbor density
- source evidence gaps
- convergence conflicts
- arbiter warnings
- failed attempt count
- repeated failure reason
- user constraints

It emits:

- continue locally
- continue locally with stronger arbiters
- ask the user
- search the local corpus again
- ask external AI
- record a learning gap only

Low confidence alone is not enough.

The detector must name the missing competence.

### 11. External AI Help Path

When the detector emits external help needed, the path is:

1. external help planner
2. external context pack builder
3. external AI chain
4. external result adapter

The external AI chain may be one call, many calls, or multiple suppliers.

The external context pack contains only the allowed context and the exact question.

The external result adapter converts outside advice back into local dynamic-flow structure.

External AI does not write directly into the final answer.

### 12. Return To Affected Local Station

The adapted result returns to the station that needed help.

Examples:

- adapted evidence returns to evidence pack selector
- adapted translation returns to domain translator array
- adapted dependency returns to local scheme planner
- adapted formal result returns to convergence gate
- adapted wording returns to result translator array

It does not always return to the beginning.

### 13. Arbiter Selector

The arbiter selector receives:

- formal result fragments
- original meaning graph
- route context
- KNN warning neighbors
- known risk markers
- external-help use, if any

It emits:

- required arbiter list
- mandatory arbiter list
- reason each arbiter is required

### 14. Arbiter Unit Array

Each arbiter checks one risk type.

Possible checks include:

- semantic fidelity
- coverage
- edge preservation
- source authority
- domain correctness
- reverse fidelity
- safety
- external-help adaptation correctness

Output:

- accept
- flag
- return for reprocessing
- ask user
- record learning gap

### 15. Arbiter Verdict Boundary

Only approved fragments may continue.

Rejected fragments return to the affected station with the rejection reason.

No priority, KNN result, budget need, style preference, or external AI answer can turn a rejected fragment into accepted work.

### 16. Result Translation And Formatting

Approved formal fragments continue to:

1. result translator array
2. formatter
3. style adapter
4. user-facing answer

The formatter folds approved translated fragments.

It does not revive rejected content.

### 17. Event Trail

Every route, selected unit, KNN neighbor, external call, arbiter verdict, return, and final result is recorded.

The event trail feeds later learning.

### 18. Learning Gap Packet

When the request used external help, repeated repair, failed arbitration, or missing competence, the system creates a learning gap packet.

The packet contains:

- original input
- logical structure
- route
- station that reached the edge
- failed attempts
- KNN neighbors
- arbiter verdicts
- external context pack
- external answers
- adapted result
- final verdict
- positive, negative, or warning status
- target unit role and domain

### 19. Idle Learning Queue

The IdleDetector from autonomous learning triggers the idle learning queue when system conditions allow background work.

The queue:

- groups learning gap packets by unit role and domain
- creates small learning batches
- keeps failed examples as negative or warning material
- runs the conformance suite
- compares the candidate unit against the previous active unit
- installs only if the candidate preserves old competence and passes new checks
- keeps rollback information

The queue does not run in the hot path unless the user explicitly requests a wait-for-learning flow.

### 20. Confidence Updater

After learning or runtime outcomes, the confidence updater adjusts:

- route confidence
- unit trust
- KNN neighbor quality
- supplier reliability
- arbiter selection confidence
- evidence pack trust

It improves future attempts.

It does not change the verdict for the already completed request.

## Canonical Dataset Preparation Sequence

The offline document-learning path follows the same front half:

document/session/artifact -> source role gate -> logical structure compiler -> fidelity and coverage checker -> topology and domain routing -> evidence pack selector -> dataset slicer

The dataset slicer first partitions by:

- unit role
- domain or topology area
- source role

It stores the rest as metadata until enough examples exist:

- predicate profile
- dependency shape
- required arbiter type
- previous arbiter verdict
- reverse translation requirement
- external-help status

Every generated learning pair keeps:

- natural input
- natural output
- logical input
- logical output
- source evidence
- arbiter status
- usage status: positive, negative, or warning

## Repeated Failure Rule

A failed attempt is one complete pass through the affected station that ends with a blocking result.

Blocking results include:

- same arbiter rejects for the same reason
- same dependency remains missing
- same convergence conflict remains unresolved
- same required evidence remains unavailable
- same external result adapter returns unusable
- same user clarification remains unanswered

The v1 threshold is:

- one immediate hard edge when no trusted unit exists, no allowed evidence exists, or outside current knowledge is explicitly required
- two consecutive failures with the same blocking reason after one local repair attempt
- three total failures in the same request path at the same station, even if the reasons are related but not identical

After the threshold is reached, the knowledge edge detector must stop ordinary local retry and choose one of:

- ask the user
- search the local corpus again
- ask external AI
- record a learning gap

## Idle Trigger Rule

Idle learning is triggered by the IdleDetector.

The IdleDetector may allow idle learning when:

- no active user request needs the same resources
- compute budget allows it
- thermal and power conditions allow it
- the learning queue has enough packets for a useful batch
- required source evidence is available
- rollback and conformance comparison can be performed

Manual trigger is allowed.

Scheduled trigger is allowed.

Hot-path retraining is not the default.

## Final Integration Rule

The system may ask for help from the user, local corpus, KNN memory, external AI, or idle learning.

But every path must return through local structure, local station adaptation, and arbiter validation before the result is accepted.
