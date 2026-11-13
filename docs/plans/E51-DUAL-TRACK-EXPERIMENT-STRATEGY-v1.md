# E51 Dual-Track Experiment Strategy v1

## Short answer

Yes, we should do both.

But they must not be merged into one experiment.

They answer different questions, and if we merge them, the result will become too large to interpret.

## The two tracks

### E51A: code-context pipeline proof

Use the plan:

`E51-END-TO-END-PIPELINE-PLAN-v1.md`

This experiment proves a technical pipeline question:

Can the existing codebase pipeline take a human request plus C# code context, translate the code into logical predicates, route the request, retrieve a nearby unit by K-NN, and produce a better predicate-aligned output than a weak baseline?

This is a component-chain proof.

It is useful because it exercises real existing code:

1. stack detection,
2. semantic tokenization,
3. logical predicate extraction,
4. domain routing,
5. K-NN retrieval,
6. template beautification,
7. metric and result writing.

It should be named:

`E51A-CODE-CONTEXT-PIPELINE`

It should not claim to prove the whole dynamic-flow architecture.

### E51B: dynamic-flow mini e2e proof

Use the plan:

`DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md`

This experiment proves the architecture question:

Can a small teaching set and a small testing set pass through the same logical-structure path, choose the correct local dynamic-flow station, produce concrete station outputs, apply arbiters as a hard gate, detect a knowledge edge, and emit learning packets as evidence?

This is the real dynamic-flow proof.

It should be named:

`E51B-DYNAMIC-FLOW-MINI-E2E`

It should remain based on the Flow-01 registration micro-set.

## Why both are valuable

E51A proves that the existing code machinery can support the theory.

E51B proves that the theory works as a flow.

E51A is closer to the current codebase.

E51B is closer to the desired architecture.

If E51A passes and E51B fails, the low-level compiler/K-NN pipeline is usable, but the dynamic-flow station design is incomplete.

If E51B passes and E51A fails, the architecture shape is valid, but the current code pipeline needs adaptation.

If both pass, we have the first strong evidence that the implementation path and the architecture path are converging.

## Execution order

Run E51A first.

Reason:

E51A mostly uses existing code and should expose API mismatches quickly. It will also produce evidence about the compiler, routing, and K-NN foundation before E51B depends on those pieces.

Then run E51B.

Reason:

E51B depends on the idea that logical structure can be used consistently for teaching and runtime. E51A gives us confidence that the lower-level mechanics are not fantasy before we test the higher-level flow.

## Shared rules

Both experiments must use deterministic data.

Both experiments must produce JSON and markdown evidence.

Both experiments must have held-out tests.

Both experiments must name every failure reason.

Both experiments must keep all risk inside `35.ExperimentsV2` and `Tests`.

Neither experiment may change the locked compiler domain enum.

Neither experiment may change the 22-predicate vocabulary without a separate decision.

Neither experiment may call an external AI supplier in the first version.

## Separate pass gates

### E51A pass gate

E51A passes when:

1. enhanced predicate overlap beats the baseline,
2. domain accuracy reaches the plan threshold,
3. evidence files are written,
4. focused E51A tests pass,
5. full solution tests remain green.

The exact numeric gate can remain the one in the E51A plan:

`enhanced_predicate_overlap >= 0.60 AND domain_accuracy >= 0.70`

### E51B pass gate

E51B passes when:

1. all 18 records compile into logical features,
2. all 4 held-out registration prompts route to the expected local subject,
3. all 4 held-out prompts produce the specified concrete station trace,
4. expected arbiter selections match ground truth,
5. the duplicate-account challenge is rejected by arbiter authority,
6. the biometric challenge creates a knowledge-edge context pack and learning packet,
7. evidence files are written,
8. focused E51B tests pass,
9. full solution tests remain green.

The exact numeric gate remains:

`e2e_trace_success_rate = 6/6`

## Relationship between the evidence files

E51A evidence should answer:

What did the compiler and K-NN machinery do?

E51B evidence should answer:

What did each dynamic-flow station decide and why?

Do not combine their evidence into one file.

Expected evidence folders:

1. `experiments-v2/E51A-CODE-CONTEXT-PIPELINE/results/`
2. `experiments-v2/E51B-DYNAMIC-FLOW-MINI-E2E/results/`

## What not to do

Do not rename E51B into a code-context pipeline experiment.

Do not let E51A become the proof of arbiter authority.

Do not let E51B depend on C# code snippets.

Do not use one shared pass/fail result for both.

Do not allow E51A's metric success to hide E51B flow failures.

Do not allow E51B's architecture success to hide E51A API or code-pipeline failures.

## First executor session shape

The first executor session should implement only E51A.

It should end with:

1. `E51A-CODE-CONTEXT-PIPELINE` result JSON,
2. `E51A-CODE-CONTEXT-PIPELINE` result markdown,
3. focused E51A tests passing,
4. full solution tests passing,
5. a short note listing which existing APIs had to be adapted from the plan.

The second executor session should implement E51B.

It should end with:

1. `E51B-DYNAMIC-FLOW-MINI-E2E` result JSON,
2. `E51B-DYNAMIC-FLOW-MINI-E2E` result markdown,
3. focused E51B tests passing,
4. full solution tests passing,
5. a per-record trace for all four held-out tests and both challenges.

## Final interpretation

Doing both is stronger than choosing one.

E51A tells us whether the current codebase can carry the logical-structure mechanics.

E51B tells us whether the designed dynamic-flow architecture can be proven with a tiny, inspectable learning set.

Together they form the first real bridge between the existing implementation and the new dynamic-flow design.

