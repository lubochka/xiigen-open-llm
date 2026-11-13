# Learning Cycle Implementation Plan — v1
# Date: 2026-05-05
# Precondition: Corpus v4 complete (3,100 training docs + 620 held-out + 60 transition)
# Baseline at plan entry: 1172/1172 passing, full nine-node security flow proven

---

## What This Plan Covers

This plan describes how to replace the current deterministic rule-based
ILogicalUnit implementations with genuinely trained units — units whose
behavior emerges from the pair corpus rather than from rules a developer
wrote by hand.

The plan has five stages. Stages 1 through 3 teach one unit and verify
that learning works. Stage 4 extends the same pattern to all nodes across
all sub-phases. Stage 5 proves that the trained system can improve from
new evidence without full retraining.

The plan is written for execution after Corpus v4 is complete. Nothing
in Stages 1 through 5 starts until all ten v4 completion conditions are
satisfied and the completion ledger says so.

---

## What "Trained" Means in This Architecture

A trained ILogicalUnit is one whose RunAsync method calls a learned
model rather than a hardcoded rule. The learned model was fit on the
pair corpus for that unit's node role. When the unit receives an input,
the model scores it and returns a confidence value that reflects real
learned distance — not a constant like 0.94.

The model type chosen for this architecture is KNN. The reasons:

KNN is inspectable. You can look at the training neighbors for any
decision and understand why the model returned that score. This matters
for a system where correctness is more important than performance at scale.

KNN requires no hyperparameter tuning beyond the neighbor count and the
distance metric. It works correctly with small corpora — a few hundred
pairs per node is sufficient.

KNN already has a home in this codebase. KnnSkillFilter in project 22
does KNN scoring. The 60-slot feature vector and the predicate vocabulary
are already defined. The missing piece is connecting them to the pair
corpus as training data and calling the scorer from inside a unit.

KNN models are lightweight. A serialized model for one node is a JSON
file containing the training vectors and their labels. It loads in
milliseconds and consumes negligible memory. This matters given the
resource constraints observed during the inspector work.

---

## The Feature Vector

Every input to a unit is converted to a 60-slot feature vector before
the KNN scorer sees it. This conversion already exists in the codebase.

Slots 1 through 22: predicate vocabulary hits. Each slot is 1 if the
corresponding predicate appears in the input, 0 otherwise. The 22
predicates are the existing vocabulary (VALIDATE_OP, GET_OP, AUTH_OP, etc.).

Slots 23 through 40: domain label hits. Each slot is 1 if the
corresponding domain label appears in the input's territory set.

Slots 41 through 52: source family hits. Each slot is 1 if the input
contains evidence from the corresponding source family (OWASP, NIST,
IETF, CWE, etc.).

Slots 53 through 60: structural signals. Presence of a reprocess
indicator, concern indicator, conflict marker, knowledge-edge marker,
confidence floor breach flag, cross-sub-phase marker, multi-territory
flag, and boundary-case flag.

The label for each training pair is the verdict the B side specifies:
PASS, REPROCESS, or CONCERN for arbiter nodes; the selected domain
labels for translator nodes; the selected arbiter identifiers for
selector nodes; and so on per node role.

---

## Stage 1 — Training Step for One Unit (Node 08, Security Arbiter)

This stage produces the first genuinely trained ILogicalUnit in the
system. Node 08 is chosen because it has the most pairs, the clearest
verdict vocabulary, and the most precise oracle gate.

### Phase L1 — Training pipeline for node 08

The executor writes a standalone training program:
`LlmInspector.Training/TrainSecurityArbiterUnit.cs`

The program reads every pair file in the CB4 stage-3 folder that
contributes to node 08 (the arbiter set files, all extensions, and the
pipeline chain files for the arbiter station). For each pair it extracts
the A side, converts it to a 60-slot feature vector, and records the
verdict from the B side as the label. It then fits a KNN model with
k=7 and cosine distance, serializes the model to a JSON file at a
known path, and prints a training summary: pair count, label
distribution, and the three nearest neighbors for each oracle scenario
input.

Gate B: the training program runs without error, produces a model file
larger than zero bytes, the label distribution includes at least one
PASS, one REPROCESS, and one CONCERN, and the nearest neighbor for the
S1 oracle input is the CB4-3.4-001 pair.

### Phase L2 — Wire the trained model into SecurityArbiterUnit

The executor replaces the hardcoded rule inside SecurityArbiterUnit with
a call to the KNN scorer. The unit loads the model file at construction
time. RunAsync converts the input to a feature vector, calls the scorer,
receives the top-k neighbors and their distances, computes confidence as
the proportion of the top-k neighbors with the winning label, and returns
that confidence value.

If the model file is absent the unit returns a knowledge-edge result
with error code MODEL_NOT_TRAINED rather than throwing an exception.

Gate B: the three oracle scenarios produce the correct verdicts. The
confidence values are now different numbers — no longer 0.94 for all
three. S1 confidence is above 0.90. S2 confidence is above 0.85. S3
confidence is above 0.70 (borderline cases are harder). The suite stays
at 1172/1172.

### Phase L3 — Held-out evaluation for node 08

The executor runs the trained SecurityArbiterUnit against the 20 CB4
held-out instances. It records the verdict for each held-out input,
compares to the ground truth label, and writes an evidence file with
the accuracy rate, the full result per held-out instance, and any
incorrect classifications with their top-k neighbors shown.

Gate B: accuracy is at least 17 out of 20. If it is below 17 the
learning plan is paused and the pair corpus for node 08 is reviewed
before continuing.

This is the first real accuracy measurement in the project.

---

## Stage 2 — Extend Training to All Nine Nodes

Once Stage 1 passes, the same training pattern applies to nodes 01
through 07 and node 09. Each node has its own pair files, its own
feature-vector-to-label mapping, and its own held-out evaluation.

The node-specific label vocabularies:

Node 01 (domain splitter): labels are domain territory identifiers.
Node 02 (domain translator): labels are domain label strings.
Node 03 (scheme planner): labels are plan step identifiers.
Node 04 (domain processor): labels are processing outcome codes.
Node 05 (applier): labels are application result codes.
Node 06 (result translator): labels are translated result categories.
Node 07 (arbiter selector): labels are arbiter role identifiers.
Node 08 (arbiter): labels are verdict codes — PASS, REPROCESS, CONCERN.
Node 09 (formatter): labels are output format codes.

### Phase L4 — Train and wire nodes 07 and 09

Node 07 (arbiter selector) is trained next because it feeds node 08.
Node 09 (formatter) is trained alongside because it does not depend on
the selector and can run in parallel.

Gate B per node: oracle scenarios stable, confidence values are real
distances, held-out accuracy at or above 17/20.

### Phase L5 — Train and wire nodes 02 and 06

Translator nodes use multi-label classification — an input may belong
to more than one domain label. The KNN scorer for these nodes returns
the union of labels present in the top-k neighbors above a 0.60
frequency threshold.

Gate B per node: oracle scenarios stable, no label that should be
present is missing from the output, no label that should be absent
appears in the output for the three oracle scenarios.

### Phase L6 — Train and wire nodes 03, 04, 05

Planner, processor, and applier nodes operate on the output of the
translator. Their training data includes the domain territory from the
translator output as a feature signal (four additional slots in a
64-slot extended vector for these nodes only).

Gate B per node: oracle scenarios stable, held-out accuracy at or
above 17/20.

### Phase L7 — Train and wire node 01

The domain splitter is trained last because its output feeds every
downstream node. A regression in node 01 propagates through the entire
pipeline. The gate is therefore stricter: held-out accuracy must be at
least 19 out of 20, and the full oracle plus held-out evaluation runs
for all nine nodes after node 01 is wired.

Gate B: node 01 accuracy at least 19/20. All nine oracle scenarios
stable across all three verdict types. Full suite green.

---

## Stage 3 — Full Security Flow Evaluation

With all nine nodes trained, the security-verification-3.4 flow runs
entirely on learned units. This stage measures the end-to-end accuracy
of the complete pipeline, not individual nodes.

### Phase L8 — End-to-end evaluation on held-out inputs

The executor constructs end-to-end test inputs that were not used in any
node's training data. These are the 20 CB4 held-out instances run
through the full nine-node pipeline rather than through individual node
units in isolation.

For each held-out input the executor records: the node 01 territory
assignment, the node 07 arbiter selected, the node 08 verdict, the
node 09 formatted output, and whether the final verdict matches the
ground truth.

Gate B: end-to-end accuracy on the 20 held-out instances is at least
16 out of 20. This threshold is lower than the per-node threshold
because errors in earlier nodes compound through later nodes. If it is
below 16, the per-node evaluation results identify which node is the
weakest link, and that node's pair corpus is expanded before
re-evaluation.

### Phase L9 — Regression test against oracle scenarios

After the end-to-end evaluation, the three oracle scenarios are run
one final time and the confidence values are recorded. These confidence
values become the new reference baseline — they replace the hardcoded
constants from the deterministic phase.

Gate B: S1 verdict PASS with confidence above 0.90. S2 verdict
REPROCESS with confidence above 0.85. S3 verdict CONCERN with
confidence between 0.70 and 0.89 (the borderline band). Full suite
green. Solution build clean.

---

## Stage 4 — Extend to All Sub-Phase Flows

The pattern proven in Stages 1 through 3 for sub-phase 3.4 is applied
to every other sub-phase flow registered in the system. Each sub-phase
flow gets its own training run, its own held-out evaluation, and its
own oracle scenarios.

The order follows the v4 quota priority: center sub-phases first (3.1,
3.3, 3.4 already done, 4.1, 4.3), then the remaining 26 sub-phases
in phase order.

Each sub-phase requires:

A source ledger with at least 100 training pairs and 20 held-out pairs.
A completed Stage 0 through 4 pipeline pass.
A registered logical flow with deterministic units passing three oracle
scenarios before the learning phase begins.
Training passes for all nine node roles.
Held-out accuracy at or above the per-node thresholds.
End-to-end accuracy at or above 16/20.

The training step, wiring step, and evaluation step are identical across
all sub-phases — only the pair files and the oracle scenarios change.

This stage has no fixed phase count because the number of sub-phases
with complete pair corpora is not known until Corpus v4 is finished.
Each sub-phase that completes its pair corpus becomes eligible for its
training phase immediately. Sub-phases do not need to wait for each
other.

---

## Stage 5 — Adaptation Test

This is the most important gate in the learning plan. It proves the
system can improve from new evidence without full retraining.

### Phase L-ADAPT — Single-pair adaptation for node 08

The executor identifies one verdict type where the trained node 08 makes
a mistake on an input that was not in its training data. It adds exactly
one new pair for that input-verdict combination to the training corpus.
It retrains node 08 with the expanded corpus. It verifies three things:

The new input now produces the correct verdict.
The three oracle scenarios still produce the same verdicts as before.
The 20 held-out accuracy does not decrease.

Gate B: all three conditions hold. If the oracle scenarios change or the
held-out accuracy decreases, the feature representation or the k value
needs adjustment before the system can be considered genuinely adaptive.

This phase cannot be simulated. It requires a real input that the
current model gets wrong. If no such input exists in the CB4 corpus,
a new held-out input must be sourced from the external registry.

---

## What the Evidence Files Must Contain

Every training phase produces one evidence file. The evidence file must
contain:

The node role and sub-phase identifier.
The pair file paths used for training.
The training pair count and label distribution.
The model file path and size in bytes.
The nearest neighbors for each oracle scenario input (top 3).
The confidence values for each oracle scenario.
The held-out accuracy rate and per-instance results.
The full suite test result after wiring.
The solution build result.

No training phase is accepted without this evidence file present and
complete.

---

## Decisions to Lock Before Stage 1 Begins

Decision L1 — Model serialization format: JSON file containing an array
of training vectors with their labels and the k value. The file is named
by node role and sub-phase: `model-node-08-security-3.4.json`. Stored
in `LlmInspector.Training/models/`.

Decision L2 — Training step timing: the training step runs as an
explicit offline command, not at build time and not at startup. The
developer runs `dotnet run --project LlmInspector.Training` to produce
or update model files. The unit loads the model file from disk at
construction time. A missing model file produces a knowledge-edge result,
not a crash or a fallback to deterministic behavior.

Decision L3 — k value: 7 neighbors for arbiter nodes, 5 neighbors for
translator and selector nodes. These values may be adjusted after Phase
L3 based on the held-out accuracy results, but they are the starting
point and must not be changed during a training run.

Decision L4 — Distance metric: cosine similarity for all nodes. This
measures the angle between feature vectors rather than their magnitude,
which means a short document and a long document with the same predicate
pattern produce the same distance. This is the correct behavior for pair
inputs that vary significantly in length.

Decision L5 — Held-out accuracy threshold: 17/20 for nodes 02 through
09, 19/20 for node 01. If any node falls below its threshold, that
node's pair corpus is expanded before the next stage begins. The
expansion uses the nearest-neighbor output from the failed held-out
instances to identify which areas of the pair space are under-covered.

---

## Acceptance Condition for the Full Learning Cycle

The learning cycle is complete when:

All registered sub-phase flows have all nine nodes trained and wired.
All trained nodes meet their held-out accuracy thresholds.
End-to-end accuracy on held-out inputs is at or above 16/20 for every
registered sub-phase flow.
The adaptation test passes for at least one node in at least one
sub-phase.
The full test suite is green.
The solution build is clean.
The corpus completion ledger is updated to reflect the trained status
of every registered flow.

---

## What This Plan Does Not Cover

This plan does not cover DPO fine-tuning of a base language model. DPO
requires a base model to update, and this project has deliberately built
a pure C# system with no external model dependencies. If a future session
decides to introduce a base model, DPO training would be a separate plan
layered on top of this one.

This plan does not cover online learning — updating model weights
continuously as new inputs arrive at runtime. The adaptation test in
Stage 5 is an offline single-pair retrain, not online learning. Online
learning introduces non-determinism that is incompatible with the current
test suite design.

This plan does not cover the nested registry orchestrator (Phase 20 of
the inspector plan). That phase is independent and can proceed in
parallel with any stage of this learning plan.

---

# Summary Table

| Stage | What it proves | Phases | Precondition |
|---|---|---|---|
| Stage 1 | One unit can be trained and produces real confidence | L1, L2, L3 | Corpus v4 complete |
| Stage 2 | All nine nodes in one flow can be trained | L4, L5, L6, L7 | Stage 1 passes |
| Stage 3 | End-to-end pipeline accuracy on held-out data | L8, L9 | Stage 2 passes |
| Stage 4 | All sub-phase flows trained | One training cycle per sub-phase | Stage 3 passes |
| Stage 5 | System can improve from one new pair | L-ADAPT | Stage 4 started, one failure found |

---

# Last updated: 2026-05-05
# Status: awaiting Corpus v4 completion before Stage 1 begins
# Locked decisions: L1 through L5 (pending Luba approval)
