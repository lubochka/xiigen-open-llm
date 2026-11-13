# External AI Knowledge Edge Amendment v1

## Post-E51 And Projects 21-23 Validation Note

E51B CH-01 validates the knowledge-edge behavior: the biometric passport signup
case found registration neighbors, but their scores stayed below the 0.50
confidence threshold, so the system created a context pack and learning gap
packet instead of inventing an answer.

The first projects 21-23 implementation slice translates this amendment into
runtime pieces: PermissionGate single-use approval for `call_external_ai`,
ExternalAiRequestPackage as the concrete context pack, ExternalAiResultAdapter
as the return path into local structure, InternalResultExaminer as the internal
quality check, and LearningEventScheduler as the idle-learning handoff.

Round 07 hardens the ExternalAiResultAdapter return path. The adapter now
checks external answers through a named arbiter rule table keyed by the
approved package's `ArbiterConstraints`. The first enforced rules are
`identity_consistency` for duplicate-account rejection and `token_safety`
for unsafe token exposure. Unknown rule names do not reject by default;
they require explicit table entries and focused tests before they become
blocking arbiters.

## Decision

The dynamic flow needs an explicit external-AI help mechanism.

This is not a replacement for the small local units.

It is a controlled escape path for the moments where the local flow can prove that it has reached the edge of its own knowledge, evidence, or judgment.

The rule is:

Local units do the work when they are competent.

External AI is asked for help when the local system can explain why it is not competent enough.

External AI answers are then adapted back into the dynamic flow and checked by arbiters before they are accepted.

## Why This Is Needed

Even with very small accurate units, the system will sometimes meet a request that is outside its learned structure.

Examples:

- The local registry has no trusted unit for the selected role and domain.
- The nearest neighbors are weak, rejected, or only warning examples.
- The logical structure contains a meaning pattern that no current unit understands.
- The selected arbiters reject the result repeatedly for the same reason.
- The source evidence is incomplete or contradictory.
- The request needs knowledge outside the gathered corpus.
- The user asks for a judgment that requires broader reasoning than the local unit is allowed to perform.
- Multiple local units disagree and the convergence gate cannot resolve the conflict.

When this happens, guessing is worse than asking for help.

The system should be able to say:

I reached a knowledge edge. I need outside judgment for this exact part.

## New High-Level Path

The added path is:

knowledge edge detector -> external help planner -> external context pack -> external AI call chain -> external result adapter -> normal dynamic flow station -> arbiters -> learning gap packet -> idle learning queue

The path can run once or as a chain.

It can use one external AI supplier or several suppliers.

It can ask one question, compare answers, ask a follow-up, or request a narrower judgment.

But the answer never enters the final output directly.

It returns through the dynamic flow.

## New Station 1: Knowledge Edge Detector

The Knowledge Edge Detector answers:

Have we reached the boundary of local competence?

It receives:

- the checked logical structure
- the selected route
- selected unit cells
- KNN neighbor quality
- missing evidence notes
- arbiter warnings
- failed attempts
- confidence scores
- source authority
- user constraints

It emits:

- no edge: continue locally
- soft edge: continue locally with warning and stronger arbiters
- hard edge: ask external AI before continuing
- user edge: ask the user because the missing information is personal or preference-based
- corpus edge: search local documents again before asking external AI
- learning edge: record a gap for idle learning

The detector must give a reason.

It should never say only "low confidence."

It should say what is missing.

Examples:

- No trusted processor exists for this role and domain.
- The closest examples were rejected by edge-preservation arbiters.
- The logical graph contains a dependency shape not present in the learning set.
- The result failed semantic fidelity twice after reprocessing.
- The request requires current outside knowledge not found in the corpus.

### Failed Attempt Rule

A failed attempt is one complete pass through the affected station that ends with a blocking result.

Blocking results include:

- the same arbiter rejects for the same reason
- the same dependency remains missing
- the same convergence conflict remains unresolved
- the same required evidence remains unavailable
- the same external result adapter returns unusable
- the same user clarification remains unanswered

The v1 threshold is:

- immediate hard edge when no trusted unit exists, no allowed evidence exists, or outside current knowledge is explicitly required
- two consecutive failures with the same blocking reason after one local repair attempt
- three total failures in the same request path at the same station, even if the reasons are related but not identical

After the threshold is reached, the detector must stop ordinary local retry and choose one of:

- ask the user
- search the local corpus again
- ask external AI
- record a learning gap

## New Station 2: External Help Planner

The External Help Planner answers:

What kind of outside help is needed?

It decides:

- whether to ask one external AI or several
- which supplier is best for the missing judgment
- whether the help should be broad or narrow
- whether the request should be split into several external questions
- whether answers should be cross-checked
- what context is allowed to leave the local system
- what output shape the external AI must return

This station should be small.

It does not solve the problem.

It only designs the external help chain.

## New Station 3: External Context Pack Builder

The external AI should not receive the whole corpus.

It should receive a compact structured context pack.

The pack should include:

- the original user request or source fragment
- the compiled logical structure
- the selected route
- the exact local station that failed or needs help
- the local result so far, if any
- the arbiter warnings or rejection reasons
- the source evidence that is allowed to be shared
- the question the external AI must answer
- the required answer shape
- the boundaries: what not to assume, what not to change, what must be preserved

If sensitive material exists, the pack should be reduced, masked, or blocked.

External AI should receive only the context needed for the missing judgment.

## New Station 4: External AI Chain

The external AI chain may contain:

- one call to one supplier
- several calls to one supplier
- parallel calls to different suppliers
- a comparison call
- a follow-up call
- a challenge call that asks another supplier to find gaps
- a final synthesis call

The chain must preserve:

- the original question
- each external answer
- supplier identity
- context used
- cost or budget note
- confidence or uncertainty expressed by the supplier
- whether the answer was used, rejected, or only kept as warning evidence

External AI is not a final authority.

It is outside help.

The local dynamic flow still decides how to use the help.

## New Station 5: External Result Adapter

The external result usually arrives in a different shape from the dynamic flow.

The External Result Adapter translates it back into the local flow.

It may produce:

- a corrected logical structure
- a new domain translation
- a repaired planned piece
- a missing dependency
- a new arbiter warning
- a result fragment
- a candidate learning example
- a request to ask the user
- a statement that the external answer is not usable

The adapter must preserve:

- which external answer produced the adapted result
- what was changed
- what was added
- what was not trusted
- which local station should receive the adapted result

This station prevents external AI from writing directly into the final answer.

## New Station 6: External Result Arbiter Gate

After adaptation, the result returns to the normal flow and is checked.

The relevant arbiters must judge it.

External AI does not bypass:

- semantic fidelity
- coverage
- edge preservation
- source authority
- domain correctness
- reverse fidelity
- safety
- user constraints

If the arbiters reject the adapted result, it does not continue as accepted work.

It can still become a negative or warning learning example.

## New Station 7: Learning Gap Packet

Every external-help event should create a Learning Gap Packet.

The packet should include:

- original request or source text
- logical structure before help
- station that reached the edge
- edge reason
- local attempts and failures
- KNN neighbors used
- arbiter warnings and verdicts
- external context pack
- external questions
- external answers
- adapted result
- final arbiter verdict
- whether the packet is positive, negative, or warning material
- which unit role and domain should learn from it later

This packet is the bridge between runtime help and future small-unit teaching.

## New Station 8: Idle Learning Queue

The system should not retrain during the hot path.

The IdleDetector from autonomous learning should trigger this queue when background work is allowed.

The trigger should check:

- no active user request needs the same resources
- compute budget allows it
- thermal and power conditions allow it
- the learning queue has enough packets for a useful batch
- required source evidence is available
- rollback and conformance comparison can be performed

When triggered, the idle learning queue should:

- group learning gap packets by unit role and domain
- keep accepted packets as positive examples
- keep rejected packets as negative or warning examples
- create small new learning sets
- run the conformance suite
- compare the new unit against the previous unit
- install only if the new unit passes
- keep rollback information

This turns external help into local competence over time.

The goal is that the same knowledge edge should happen less often after the system has learned from it.

## External AI Authority Rules

External AI can:

- explain a missing concept
- propose a repair
- compare alternatives
- challenge a local assumption
- identify a missing edge
- suggest a better wording
- help convert an unknown structure into a known structure
- provide outside knowledge when allowed

External AI cannot:

- override an arbiter rejection
- become accepted training truth without validation
- bypass source authority
- write directly to the final answer
- replace the local topology
- silently change the user request
- erase failed local attempts
- hide uncertainty

If external AI and local arbiters disagree, the arbiters win.

If external AI and source authority disagree, source authority wins unless the source itself is being examined as possibly wrong.

If external suppliers disagree with each other, the disagreement is recorded and routed to arbiters or the user.

## External AI Supplier Registry

The system should keep a registry of external AI suppliers.

Each supplier should be described by:

- what kind of judgment it is good at
- what kind of material it may receive
- cost or budget level
- privacy permission level
- reliability history
- known failure patterns
- output formats it can follow
- whether it is allowed for runtime use, idle learning only, or both

The External Help Planner uses this registry to choose the supplier chain.

## Knowledge Edge Signals

The Knowledge Edge Detector should watch for these signals:

- no trusted unit found
- weak or missing KNN neighbors
- nearest neighbors are mostly negative or warning examples
- unknown predicate pattern
- unknown dependency shape
- route conflict
- repeated arbiter rejection
- repeated reprocessing loop
- evidence conflict
- missing source evidence
- user request requires current outside knowledge
- user request requires expert judgment outside the local training set
- convergence gate cannot resolve result fragments
- budget gate says the local path is too expensive without better guidance
- confidence is low and the reason is known

Low confidence alone is not enough.

The detector must identify the actual edge.

## Relation To User Clarification

Sometimes the right external help is not an AI supplier.

Sometimes the missing information belongs to the user.

If the missing information is preference, intent, permission, private context, or business choice, the system should ask the user.

If the missing information is general knowledge, reasoning support, translation support, or a broader expert view, the system may ask external AI.

This keeps the user chain and the external AI chain separate but structurally similar.

Both chains ask for help with a clear context pack.

Both chains return through adaptation and validation.

## Topology Position

The knowledge edge path should connect to several stations:

- after evidence pack selection, when allowed evidence is insufficient
- after unit registry lookup, when no trusted local unit exists
- after convergence, when formal fragments cannot be reconciled
- after arbiter rejection, when repeated repair fails
- during dataset curation, when a learning gap needs outside explanation

The path should return to the specific station that needs the adapted result.

It should not always return to the beginning.

Examples:

- If the missing piece is a domain translation, return to the domain translator array.
- If the missing piece is a dependency, return to the local scheme planner.
- If the missing piece is result wording, return to the result translator array.
- If the missing piece is source evidence, return to the evidence pack selector.
- If the missing piece is a user decision, ask the user and then continue from the affected station.

## Revised High-Level Flow Addition

The new addition is:

local flow station -> knowledge edge detector -> external help planner -> external context pack -> external AI chain -> external result adapter -> affected local station -> arbiters -> learning gap packet -> idle learning queue

This keeps the small-unit architecture intact.

External AI becomes a teaching and repair channel, not a replacement brain.

## Final Rule

The system should ask for external help when it can explain the knowledge edge.

It should adapt the answer into the local logical structure.

It should validate the adapted answer with arbiters.

It should store the whole event as a small future learning set.

When the computer is idle, the local units should learn from approved external-help packets so the same edge becomes local knowledge next time.
