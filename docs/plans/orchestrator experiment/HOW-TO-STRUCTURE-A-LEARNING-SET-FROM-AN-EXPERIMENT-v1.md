# How to Structure a Learning Set Based on an Experiment
# A method for converting experiment findings into training pairs
# Version: 1.0 — 2026-05-04

---

## What This Document Is

When an experiment passes, it produces three kinds of output: findings about
specific pairs that need correction, a training gap map naming which nodes
need new pairs, and pass gate evidence showing what the orchestrator detected
correctly. Each of these translates directly into learning set work. This
document defines how to do that translation — how to take experiment output
and produce training pairs that are correctly shaped, correctly assigned, and
honest about what they teach.

This document is the companion to HOW-TO-TRANSLATE-A-DOCUMENT-INTO-AN-EXPERIMENT.
That document describes how to turn a source document into an experiment.
This document describes what to do with the experiment results.

---

## The Three Learning Set Actions

Every passing experiment produces work across three actions, always in this
order. Skipping to action two before completing action one means the corpus
contains pairs that train wrong behavior alongside pairs that train correct
behavior. The actions do not run in parallel.

**Action 1 — Correction pass.** Apply all CONCERN findings to the existing
pairs in the source document. No new pairs are written. Only existing pair
content is modified.

**Action 2 — Gap closure pass.** Write the new pairs identified in the
experiment's training gap map. These pairs teach the examination domain
vocabulary and the examination plan structure that the experiment found
missing.

**Action 3 — Coverage extension pass.** Write pairs for nodes that were
proxied during the experiment's first pass. These are the nodes that used
human output rather than trained model output because no matching pair
existed. Coverage extension pairs follow from the gap closure pairs — they
cannot be written before action 2 is complete.

---

## Action 1 — The Correction Pass

### What qualifies as a correction

A correction applies to a pair that produced a CONCERN verdict. The experiment
found specific content in the pair that needs to change — a phrase that belongs
in the wrong node, a target that is not traceable to the A input, a wait state
whose resolution check is not self-contained, a Reason field that does not
explain why two related entries exist separately. The pair is usable but not
safe to train from without the correction applied.

A correction does not apply to a pair that produced a REPROCESS verdict.
A REPROCESS verdict means the pair's structure is wrong enough that it must
be rebuilt, not patched. REPROCESS pairs are removed from the corpus and
replaced with new pairs in action 2.

### What a correction looks like

A correction is a specific text change to a specific field of a specific pair.
It is never "improve the clarity of pair CB2-04-002." It is always:

"In pair CB2-04-002 Failure state 2, add the following sentence after the
existing condition description: 'Observable signal: Collector metrics show
incomplete trace or spans from unknown trace counters incrementing after the
routing change is deployed. The load balancer does not log a parsing error —
the failure is only visible downstream.'"

The correction names the pair ID, the section within the pair, and the exact
text change. If the correction cannot be stated at this level of specificity,
the experiment's finding was not specific enough and must be revisited before
the correction pass begins.

### How to execute the correction pass

One correction round per source document. The executor reads the source pair
file and the experiment's list of CONCERN findings. For each finding, the
executor applies the specific text change. No other content in the pair changes.
The pair ID, the A input, the B output structure, the node assignment, the
source citation, and the license annotation all remain unchanged.

After all corrections are applied, the executor runs the pre-promotion checks
against the corrected file:

```
- Pair count unchanged from before correction (no pairs added or removed)
- All pair IDs present and unchanged
- All source citations present and unchanged
- All license annotations present and unchanged
- No new content added beyond the specific correction text
```

The corrected file replaces the original in the corpus. The correction round
is documented in the batch registry with a note: "Corrections applied per
[experiment name] findings, [date]."

### What the correction pass does not do

The correction pass does not rewrite pairs. If fixing a pair requires changing
the A input, changing the node assignment, or restructuring the B output, the
pair is not a correction candidate — it is a replacement candidate. Stop the
correction pass for that pair, note it as a replacement need, and address it
in action 2.

The correction pass does not add new vocabulary to a pair. If the finding
says "the pair needs to name the observable signal for this failure type" and
the observable signal would require adding domain knowledge not present in
the original source, the correction is out of scope for the correction pass.
The pair is annotated with a note — "observable signal not named; source
search required before training" — and the gap is added to the action 2 scope.

---

## Action 2 — The Gap Closure Pass

### Reading the training gap map

Each experiment produces a training gap map. The map has one row per node.
Each row says one of three things: human proxy (no pair needed for the first
pass), new pair needed (a pair must be written before the experiment can run
with trained units), or existing pair usable (a prior pair covers this domain
sufficiently).

The gap closure pass writes pairs only for nodes marked "new pair needed."
It does not write pairs for nodes marked "human proxy" — those nodes will be
covered in action 3 if the experiment shows the human proxy output was correct.
It does not rewrite pairs for nodes marked "existing pair usable."

### The vocabulary constraint

Every new pair in the gap closure pass must use the domain vocabulary defined
in the experiment's oracle Document 1 — the Examination Domain Map. This is
the most important rule in this section. A pair that describes what the
orchestrator should find using general language rather than the domain
vocabulary defined in the map will not generalize — the trained node will
produce findings in general language rather than in the vocabulary the
downstream nodes expect.

Before writing any pair, read Document 1 of the experiment's oracle. Copy the
vocabulary table into working notes. Every B output in every new pair must
use terms from that table rather than inventing synonyms.

### Pair shapes by node

The pair shape requirements come from the UML frame. These do not change
across experiments. What changes is the domain vocabulary used inside each
shape.

**Node 02 — Domain Translator pairs.** A is one domain packet: a prompt
fragment plus a domain label. The domain label is the examination domain,
not the original technical domain. For an experiment examining scheme planner
pairs, the domain label is something like "scheme-planner-examination" or
a more specific label derived from the failure type. B is the formal domain
statement in the examination vocabulary — it names the pathology, the
observable evidence, and the required state. B does not describe what a
formal statement would contain. It contains it.

Example shape:

```
A:
Fragment: Step 3 of the OTel tail sampling plan says "positioned after the
memory_limiter (which will be added in Step 4)" while Step 3 and Step 4 are
stated as parallel.
Label: scheme-planner-examination

B:
The step contains a parallel claim contradiction. The parallel independence
claim states that Steps 3 and 4 are independent and can run concurrently.
The transformational prompt body contains a cross-reference to Step 4's
output as a prerequisite for Step 3's configuration. A parallel step must
not reference a sibling step's output in its own transformational prompt
body — a cross-reference in the prompt body creates an apparent execution
dependency that contradicts the stated independence. The formal statement
is: parallel independence is claimed in the Reason field; a content
reference to the sibling step is present in the prompt body; the two claims
are contradictory; the resolution is to reword the prompt body to describe
the final pipeline state rather than the execution sequence.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
```

**Node 03 — Scheme Planner pairs.** A is the complete set of translated
examination statements from node-02 for this experiment's domain. B is the
ordered examination plan — the actual sequence of examination steps, named
in order, with dependencies stated, parallel groups identified, and wait
states named. The plan must reflect the dependency structure of the
examination, not just list what to look for. If the experiment requires
finding enforcement failures before coverage failures because coverage
findings may be partially explained by enforcement gaps, that dependency
must appear in the plan as an explicit ordering, not as an implied sequence.

The critical property of node-03 pairs for examination domains: the plan
must contain a step for each scenario in the experiment. The three scenarios
are not discovered during the plan execution — they are the fixed examination
targets. The plan specifies how to examine each one, in what order, and what
to do if an early examination changes the context for a later one.

**Node 07 — Arbiter Selector pairs.** A is the full collection of formal
examination results from the result translator, plus the flow context
identifying which review units are available for this examination domain.
B names which arbiters fire and why, and which do not fire and why. The
pass-through case must be present in the file — a selector trained only on
fire cases dispatches arbiters for everything.

The experiment's pass gates often reveal what the arbiter selector must
catch. If GATE-1A requires the arbiter output to contain the phrase
"applier crossover," the selector pair must produce a formal result that
causes the arbiter to fire on the applier-crossover domain. The selector
pair's A must contain a result with an applier-crossover finding, and the
selector's B must name the arbiter responsible for that finding class.

**Node 08 — Arbiter Units pairs.** A is one review responsibility plus
one result or result group. B is exactly one of three outputs: pass, concern,
or reprocess. The rules for verdict type follow from the experiment's binding
failure taxonomy — Document 2 of the oracle. Before writing any node-08 pair,
read Document 2 and confirm which verdict type applies to the finding type
this pair covers.

The pass case is mandatory in every node-08 file. A file with only concern
and reprocess cases trains the arbiter to flag everything. The pass case
shows a result that correctly demonstrates the property under review — a
parallel step with genuine independence and no cross-references, a wait state
that is self-contained, a target that is traceable to the A input. The pass
case B is empty or contains only the word "pass." Silence is the pass signal.

This is the specific shape for each verdict type:

Pass: `B: pass` — no explanation, no elaboration.

Concern: `B: Concern: [the specific rule this result violates]. [The exact
location in the result where the violation occurs]. [What the correct version
looks like]. [What must change before this result can be used safely.]`

Reprocess: `B: Reprocess: [the exact fragment to send back]. Required change:
[what must be different in the reprocessed version]. Do not proceed until the
reprocessed version returns.`

### The pair count rule

Each experiment's gap closure pass should produce the minimum number of pairs
that closes the identified gaps. This is not a suggestion — it is a quality
constraint. Adding pairs beyond the minimum dilutes the training signal by
introducing variance in the examples before the minimum is validated. The
minimum is:

One original pair plus one phrasing B companion per scenario per node that
requires a new pair. If an experiment has three scenarios and node-02 requires
a new pair, the gap closure pass produces two node-02 pairs: one original and
one phrasing B. If node-03 also requires a new pair, the gap closure pass
produces two more: one per the same pattern. The total for a three-scenario
experiment with four nodes requiring new pairs is eight pairs, not twenty.

Add more pairs only after the minimum set has passed the experiment's gate
checks with trained units. The experiment's pass gates are the evidence that
the minimum is sufficient or insufficient.

### Phrasing B requirements

Every original pair in the gap closure pass has a phrasing B companion. The
phrasing B must rephrase the A input at the same depth — not summarize it,
not simplify it, not add new content. The B output must be the same as the
original pair's B output in substance, expressed differently in form. If the
phrasing B produces a different finding than the original pair, the two pairs
are covering different cases and both should be named as original pairs, not
as one original plus one phrasing B.

---

## Action 3 — The Coverage Extension Pass

### When to start

Action 3 starts after the gap closure pairs from action 2 have been validated
by running the experiment's pass gates with the new pairs active. If any gate
fails, the gap closure pairs need correction before coverage extension begins.
Do not start action 3 while action 2 pairs are still under correction.

### What coverage extension covers

Coverage extension writes pairs for nodes that were proxied during the
experiment's Round A and Round B. These are nodes marked "human proxy" in
the training gap map. The human proxy provided correct output — the experiment
passed — but that output came from a human rather than from a trained model.
Coverage extension converts the correct human proxy output into training pairs
so a trained model can produce the same output.

The process is direct: for each node where human proxy was used, take the
human-authored output from the experiment's evidence trace and convert it into
a pair. The A is what the node received as input. The B is what the human
authored. The Reason field explains why this output is correct for this input
— naming the specific property it demonstrates.

This is the only case in learning set work where the B output is derived from
evidence rather than authored from scratch. The pair is grounded in a real
run of the orchestration pipeline, which makes it more reliable than a pair
written speculatively.

### Coverage extension does not apply to all proxied nodes

Nodes 00 and 01 are proxied in almost every experiment because their steps
are mechanical — compiling the input and splitting it into a single examination
domain. These nodes do not need new pairs from the coverage extension pass.
They are covered by the existing Flow-01 training set for the registration
domain and the existing clean-batch corpus for the external domains. The
examination domain is a new input shape for these nodes but not a new
capability — the same splitting behavior applies regardless of whether the
domain being split is an infrastructure scenario or an examination scenario.

Nodes 04, 05, and 06 are proxied in all five current experiments because
their content is deterministic given the scheme plan. These are the nodes
that currently have zero coverage in the external corpus. Coverage extension
for these three nodes is the highest-value work in the entire learning set
structure — without pairs for these nodes, any scenario that reaches the
processing stage falls through to human proxy permanently.

For nodes 04, 05, and 06, the coverage extension pairs chain directly from
the node-03 pairs that were written in action 2. Take each step in the
node-03 examination plan as the A for node-04. Derive the success, failure,
and wait-state outcomes for that step as node-04's B. Then take node-04's B
as part of the A for node-05, and write node-05's B as the concrete action
and observable outcome. Then take node-05's B as node-06's A, and write
node-06's B as the natural-language fragment. This chain must be followed in
sequence — node-05 pairs cannot be written before node-04 pairs are complete,
because node-05's A includes node-04's output.

---

## File Organization After All Three Actions

The learning set folder structure after all three actions are complete has
a specific shape. The source pair files are updated with corrections in place.
New pairs are in new files organized by experiment. The index reflects the
current state.

```
external-sources-training-sets/
  00-INDEX.md                    — updated with all new pairs and corrections
  01-UML-FRAME.md                — unchanged (the edge inventory does not change)
  clean-batch-001/               — corrected by Experiment 1 findings
    node-01-domain-splitter-set.md
    node-03-scheme-planner-set.md    — corrections applied
    node-08-arbiter-units-set.md
    node-09-formatter-set.md
  clean-batch-002/               — corrected by Experiments 2, 3, 4 findings
    node-02-domain-translator-set.md — corrections applied
    node-04-domain-processor-set.md  — corrections applied
    node-07-arbiter-selector-set.md  — corrections applied
  clean-batch-003/               — corrected by Experiment 5 findings
    node-03-scheme-planner-set.md    — corrections applied
  examination-domain-pairs/      — new folder, produced by gap closure passes
    node-02-examination-domain-set.md
    node-03-examination-domain-set.md
    node-07-examination-domain-set.md
    node-08-examination-domain-set.md
  coverage-extension-pairs/      — new folder, produced by coverage extension
    node-04-examination-outcomes-set.md
    node-05-examination-applier-set.md
    node-06-examination-result-translator-set.md
```

The examination-domain-pairs folder contains all pairs produced by action 2
across all five experiments. They are organized by node, not by experiment,
because nodes accumulate pairs across multiple experiments. Node-02 will have
pairs from all five experiments' gap closure passes — they all belong in the
same node-02 file so the trained model sees the full vocabulary across all
examination domains.

The coverage-extension-pairs folder contains all pairs produced by action 3.
These are organized by node for the same reason.

---

## The Index Update Rule

After every action — correction, gap closure, coverage extension — the index
is updated before any other work continues. The index update is not optional
and is not deferred to the end of all three actions. Each action's output
is recorded in the index before the next action begins.

The index update must contain:

For corrections: the batch ID, the pair IDs that were corrected, the
experiment that drove the correction, the date, and a brief description of
what changed.

For new pairs: the file name, the pair count added, the nodes covered, the
experiment that produced the gap, and whether the pairs are from action 2
or action 3.

The index is the ground truth for what the corpus contains. Any pair that
exists in a file but is not in the index does not exist for training purposes.
Any gap listed in the index has not been closed until the corresponding pairs
appear in the index as well.

---

## The One Rule That Connects This Guidance to the Experiment

The learning set work is grounded in the experiment's oracle. The oracle
defines what correct output looks like at every node boundary. The pairs
produced in actions 2 and 3 must be consistent with the oracle — their B
outputs must match what the oracle says the node should produce for inputs
of this type. If a pair's B output is inconsistent with the oracle, the pair
will train the node to produce output that fails the experiment's pass gates.

Before writing any pair in action 2 or action 3, read the relevant section of
the expected orchestration trace — Document 3 of the oracle. The pair you are
about to write is a teaching example for behavior that the oracle already
specifies. The oracle is the senior partner. The pair is the junior's
implementation of what the oracle defined.

If you cannot find the relevant oracle section, the oracle is incomplete and
must be extended before pair writing continues. Pairs written without an
oracle section to validate against are speculation, not training.
