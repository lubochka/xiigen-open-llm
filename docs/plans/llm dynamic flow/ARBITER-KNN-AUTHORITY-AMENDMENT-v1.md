# Arbiter KNN Authority Amendment v1

## Post-E51 Validation Note

E51B CH-02 validates the authority rule in a closed proof: a duplicate-account
instruction still reached an arbiter rejection even though nearby registration
examples were similar. This is the concrete evidence that KNN memory may guide
retrieval but cannot accept a result.

E51A Phase 2 adds the retrieval-shape rule: KNN should first scope the neighbor
pool to the predicted domain, then rank by similarity. If the top neighbor in
that scoped pool remains below 0.50, the system should treat the retrieval as
low-confidence and feed the knowledge edge detector rather than pretending the
best wrong-pool match is reliable.

## Decision

The review is only partly right.

The arbiter checks should not be collapsed into one hidden compiler-quality step, and they should not be treated only as ordinary routing metadata.

The correct design is two-level:

1. Arbiter signals are used before generation to improve KNN retrieval, evidence selection, dataset slicing, and neighbor choice.
2. Arbiter verdicts are used after generation as hard quality gates. No other priority can override them.

In short:

Arbiters guide memory before work is produced.

Arbiters govern acceptance after work is produced.

## Why Both Levels Are Needed

KNN retrieval needs to know what kind of quality risk belongs to an example.

For example, two examples may look close in meaning, but one was previously rejected because it lost an edge, skipped a required condition, changed the user intent, or translated a formal result into misleading human language.

If the nearest-neighbor layer ignores that history, it may retrieve examples that are close in words but unsafe in behavior.

So the KNN layer should carry arbiter-aware fields.

But KNN closeness is still only memory.

It cannot decide that an output is acceptable.

Only the selected arbiters can decide whether the generated result is good enough to continue.

## Arbiter Signals At KNN Level

At the nearest-neighbor level, arbiter information should be used as a retrieval signal.

Each learning example, source fragment, previous output, and failed output should carry:

- Which arbiter types were required.
- Which arbiter types approved it.
- Which arbiter types rejected it.
- The reason for rejection, if rejected.
- Whether the example should be used as a positive example, negative example, or warning example.
- Which edge, dependency, meaning unit, or translation step the arbiter checked.
- Whether the example is allowed to influence generation or only allowed to warn.

This means KNN does not only ask:

Which examples are closest to this request?

It also asks:

Which close examples survived the same kind of quality checks?

And:

Which close examples failed the same kind of quality checks and should warn the system?

## Positive, Negative, And Warning Neighbors

The KNN layer should not throw away failed examples.

Failed examples are valuable, but they must be used correctly.

Positive neighbor:

- A close example that passed the relevant arbiters.
- It can teach the node what to produce.

Negative neighbor:

- A close example that failed a relevant arbiter.
- It can teach the node what not to produce.

Warning neighbor:

- A close example that is not directly used for generation, but warns that this kind of input often fails in a specific way.
- It can trigger extra arbiter selection, a lower confidence score, or a request for clarification.

This lets the system learn from failures without treating failed outputs as good training material.

## Arbiter Verdicts After Generation

After a unit produces a result, the arbiter selector chooses the required arbiters.

The selected arbiters then judge the formal result.

Their output has authority:

- accept
- flag
- return for reprocessing
- ask for missing information
- record a learning gap

If an arbiter rejects or returns a result, the result cannot continue to final translation or final formatting as accepted work.

This remains true even when:

- KNN confidence is high.
- The route confidence is high.
- The result came from a high-priority path.
- The budget gate wants to finish quickly.
- The user preference model prefers a shorter answer.
- A previous similar example passed.
- The formatter can make the answer sound good.

No priority can override an arbiter rejection.

## Priority Boundary

Priority can decide:

- what to try first
- which evidence to load first
- which examples to retrieve first
- how many units to run
- which route is cheaper
- which fallback should be attempted
- whether to ask the user early

Priority cannot decide:

- that a failed result is acceptable
- that missing meaning can be ignored
- that a broken dependency can be hidden
- that an unsafe result can continue
- that a misleading translation can be formatted anyway
- that a rejected result should be treated as approved training data

The arbiter verdict is the quality boundary.

## Relation To Logical Structure Compiler

The Logical Structure Compiler still needs its own fidelity and coverage checker.

That checker protects the compiled logical structure before it enters the main dynamic flow.

But the arbiter concerns should also exist as retrievable quality history and as selected runtime validators.

So the design has three uses of arbiter knowledge:

1. Compiler check:
   - Is the logical structure faithful to the original input?

2. KNN signal:
   - Which examples are safe, risky, or rejected for this kind of meaning?

3. Runtime verdict:
   - Is this generated result allowed to continue?

These are not the same job.

The compiler check prepares the input.

The KNN signal improves selection.

The runtime verdict protects the output.

## Required Arbiter-Aware Fields

Each compiled item should be able to carry:

- Required arbiter types.
- Arbiter verdict history.
- Arbiter rejection reasons.
- Arbiter-approved edges.
- Arbiter-rejected edges.
- Known risk markers.
- Positive/negative/warning usage status.
- Reprocess target if rejected.
- Whether the item can be used for generation, validation, or warning only.

These fields should be human-readable. A reviewer should be able to understand why a neighbor was retrieved and why it is safe or unsafe to use.

## Effect On Dataset Slicing

The dataset slicer should include arbiter fields, but they should not always become hard partition keys.

Recommended first slice keys:

- unit role
- domain or topology area
- source role

Recommended metadata fields:

- predicate profile
- dependency shape
- required arbiter type
- previous arbiter verdict
- rejection reason
- reverse translation requirement

Reason:

If the first dataset batch is sliced by too many keys, each cell becomes too small. Arbiter data should influence retrieval and validation from the start, but only become a hard slice key when enough examples exist.

## Effect On KNN Ranking

KNN ranking should combine:

- meaning closeness
- domain closeness
- source-role closeness
- dependency-shape closeness
- arbiter-risk match
- positive approval history
- negative failure warning

But the ranking result remains advisory.

It chooses useful neighbors.

It does not approve final work.

## Effect On Arbiter Selector

The arbiter selector should receive:

- formal result fragments
- original meaning graph
- retrieved neighbor warnings
- known risk markers
- previous failure patterns

It should emit:

- the required arbiter list
- why each arbiter is required
- whether any arbiter is mandatory

If a neighbor warning says that similar examples often fail edge preservation, then the edge-preservation arbiter should be selected more often.

## Effect On Feedback

Every arbiter verdict should feed back into the learning memory.

Accepted result:

- Store as positive evidence.

Rejected result:

- Store as negative or warning evidence.

Returned result:

- Store the failed fragment, the reason, and the upstream node that should repair it.

Corrected result:

- Store the before and after pair, plus the arbiter reason that caused the correction.

This feedback improves future KNN retrieval without weakening arbiter authority.

## Final Rule

The system may use KNN, confidence, priority, budget, route score, user style preference, and historical similarity to decide what to attempt.

The system must use arbiters to decide what is accepted.

If these disagree, the arbiter wins.
