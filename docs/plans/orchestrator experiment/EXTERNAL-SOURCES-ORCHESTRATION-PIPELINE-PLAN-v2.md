# External Sources — Orchestration-First Pipeline Plan
# Version: 2.0 — 2026-05-05
# Replaces: EXTERNAL-SOURCES-FULL-PIPELINE-PLAN-v1.md
# Focus: User prompt splitting, domain routing, and correct orchestration handling
# Code-level behavior is secondary and deferred to later waves

---

## What changed from v1 and why

Version 1 of this plan treated all nine pipeline nodes with equal priority.
The result was a corpus (CB4) that is extremely deep on Node 8 (arbiter verdict
production) for one sub-phase — security and authorization — but has zero coverage
for the thing the orchestrator must do first: correctly split a user prompt into
the domains it touches and route each domain fragment through the right pipeline.

The orchestrator cannot produce a correct verdict for something it misrouted to
the wrong domain. And it cannot route correctly if Node 1 and Node 2 have not
been trained on the variety of ways users actually phrase requests.

This plan reorders the priority. Nodes 1, 2, 3, and 7 are the primary targets
for the next three waves. Nodes 4, 5, 6, and 9 follow. Node 8 (arbiter verdict
production) already has substantial coverage in CB4 and is not the bottleneck.

---

## The orchestration problem being solved

A user types a prompt. That prompt arrives at Node 1. Node 1 must identify every
functional domain the prompt touches and emit a territory array. Node 2 must
translate each territory into the precise vocabulary its downstream nodes expect.
Node 3 must sequence the work across those territories into an ordered plan with
dependencies, parallel branches, wait states, and recovery paths. Node 7 must
select which review units fire before the result reaches the user.

These four nodes are the orchestration core. They determine whether the right
work happens in the right order. Getting any of them wrong means the downstream
nodes — no matter how well trained — are working on the wrong problem.

The current corpus has this problem in its pair shape. The CB4 Node 1 pairs were
written with security-domain inputs only. A prompt like "help me plan this feature
end to end" does not appear anywhere in the training data. The orchestrator has
never seen a prompt that spans Phase 1 (UI/UX definition), Phase 3 (technical
design), and Phase 4 (implementation) simultaneously. It has no training for the
most common class of real development request: the multi-phase, multi-domain
prompt that a developer actually types.

---

## The three things Node 1 must learn

**Thing 1 — Splitting a multi-domain prompt correctly.**

When a user says "I need to add authentication, update the API contract, and make
sure the UI shows the right error states," Node 1 must produce three territory
labels: one for authentication (sub-phase 3.4), one for API contract (sub-phase
3.3), and one for UI/UX error states (sub-phase 1.2 or 1.3). It must not collapse
these into one territory. It must not add territories that are not present. It must
not assign them to the wrong sub-phase.

**Thing 2 — Recognizing when a prompt implies a phase without naming it.**

Developers do not say "I need sub-phase 3.4 work." They say "the tokens are not
being validated correctly." Node 1 must recognize the vocabulary signals that
indicate which sub-phase the prompt belongs to, even when the user is describing
symptoms rather than declaring a work category.

**Thing 3 — Refusing to split when the prompt is genuinely ambiguous.**

When a prompt could belong to multiple sub-phases and there is not enough signal
to decide, Node 1 must surface the ambiguity rather than guessing. The output in
this case is not a territory array — it is a clarification request. Training Node 1
on ambiguous inputs that were resolved by assumption teaches it to assume. Training
it on ambiguous inputs that were resolved by clarification teaches it to ask.

---

## Wave 2 — Orchestration routing corpus (the priority wave)

Wave 2 targets the four orchestration-core nodes across all 31 sub-phases using
real development request phrasing as the A side of every pair.

### Target sub-phases for Wave 2

Wave 2 does not go deep into any one sub-phase. It goes wide across all 31,
with enough pairs per sub-phase that the router and splitter can distinguish
adjacent sub-phases from each other.

The hardest adjacent-sub-phase pairs to distinguish are:

Sub-phase 3.3 (API contract definition) versus sub-phase 3.4 (security and
authorization model). Both involve thinking about what the API accepts and
rejects. The difference: 3.3 is about the contract shape — what fields, what
formats, what responses. 3.4 is about who is allowed to call the contract and
what they can do once they do.

Sub-phase 5.1 (unit and contract tests) versus sub-phase 5.2 (integration tests).
Both involve test writing. The difference: 5.1 tests one unit in isolation with
mocked dependencies. 5.2 tests the interaction between two or more units with
real connections.

Sub-phase 6.2 (regression coverage verification) versus sub-phase 5.3 (end-to-end
scenario tests). Both involve checking whether the system works. The difference:
5.3 proves a user flow works. 6.2 proves a previously passing flow did not break.

Sub-phase 4.1 (backend services) versus sub-phase 4.3 (integration and provider
connectors). Both involve server-side code. The difference: 4.1 is internal
business logic. 4.3 is the boundary code that talks to an external system.

These pairs are the training priority because they are where the router will
most often make mistakes, and where a mistake has the highest cost.

### Sources for Wave 2

The sources for Wave 2 are different from Wave 1. Wave 1 used standards documents
(OWASP ASVS, NIST SSDF, CWE, IETF RFCs) because those documents define what
correct security controls look like. Wave 2 needs sources that produce natural
development request language — the kind of prompt a developer actually types.

**Primary sources for Wave 2 Node 1 and Node 2 pairs:**

XIIGen project history. The XIIGen session files, plan documents, and architecture
decision records contain thousands of real development requests phrased the way
Luba actually phrases them. These are the highest-fidelity source for Node 1
training because they are real prompts from the system being built. License: this
is first-party material.

Kubernetes Enhancement Proposals. Each KEP begins with a user story section
phrased as: "As a [role], I want [capability] so that [outcome]." These are
real development requests phrased by real engineers. The KEP then moves through
design, implementation, testing, and production considerations — spanning multiple
sub-phases in one document. License: Apache 2.0, confirmed. Source group: 1.

Google AIP documents. Each AIP describes an API design requirement in terms of
what the API must do from the perspective of someone calling it. The language is
close to how a developer describes what they need. License: CC-BY 4.0, attribution
required. Source group: 1.

OpenAPI Specification. The OpenAPI documents describe API contract requirements —
directly relevant to sub-phase 3.3 territory detection. License: Apache 2.0. Source
group: 1.

OWASP Cheat Sheet Series. Each cheat sheet describes what a developer must do to
implement a specific security control correctly. The phrasing is often imperative
and task-oriented — closer to a development request than an abstract standard.
License: CC-BY-SA 4.0, attribution required. Source group: 2.

### Pair count target for Wave 2

Minimum 20 pairs per sub-phase for Node 1, covering at least:
- 5 pairs where the prompt maps cleanly to one sub-phase (clean routing)
- 5 pairs where the prompt spans two adjacent sub-phases (split routing)
- 5 pairs where the prompt spans a non-adjacent pair of sub-phases (cross-phase routing)
- 5 pairs where the prompt is ambiguous and Node 1 must surface a clarification

Total minimum for Node 1: 620 pairs across 31 sub-phases.

Node 2 pairs are derivative of Node 1 — for each territory that Node 1 identifies,
Node 2 must produce the formal vocabulary translation. Target: 3 pairs per
territory type per sub-phase. Many sub-phases share territory types, so the
effective count is lower once common territories are covered.

Node 3 pairs are the most complex — the scheme planner must produce a full ordered
plan from the translated territories. Target: 5 pairs per sub-phase, each showing
the full plan structure including wait states and recovery paths.

Node 7 pairs teach the arbiter selector which review units fire for which sub-phase
result. Target: 3 pairs per sub-phase.

---

## Stage 0 — Source selection and license gate (unchanged from v1)

Before any document is examined, its source ID must appear in the registry with a
confirmed license class. The two-question phase classification runs before any pair
is written.

Question 1: What does this document define, decide, or constrain?
Question 2: Which phase of the development lifecycle produces this kind of output?

The anti-patterns from v1 remain in force. Classifying by title alone is
forbidden. Treating VERIFY_BEFORE_USE as TRAINING_OK is forbidden. Skipping phase
classification and writing pairs without a sub-phase assignment is forbidden.

---

## Stage 1 — Source document translation to experiment design (orchestration edition)

The six-step method from the examination method document applies, with one
critical restatement for the orchestration focus.

**The central question for orchestration-focused experiments** takes this form:

"Given [a user prompt that belongs to sub-phase X], does the orchestrator correctly
split it into the right territory array, translate each territory into the correct
formal vocabulary, sequence the work in the right order — or does it collapse
multiple territories into one, assign a territory to the wrong sub-phase, or
produce a plan that omits required sequencing constraints?"

The three scenarios for each orchestration experiment are:

Scenario S1 — Clean single-sub-phase routing: a prompt that clearly belongs to
one sub-phase. Node 1 produces one territory label. Node 2 produces one formal
translation. Node 3 produces a plan with the correct sequencing for that sub-phase.

Scenario S2 — Multi-sub-phase routing: a prompt that spans two or more sub-phases.
Node 1 must produce multiple territory labels. Node 2 must produce a separate
formal translation for each. Node 3 must produce a plan that correctly sequences
the work across sub-phases, including the dependencies between them.

Scenario S3 — Ambiguous routing: a prompt where the sub-phase cannot be determined
without more information. Node 1 must surface the ambiguity rather than guessing.
The B output is a clarification request, not a territory array.

The acceptance criteria for orchestration experiments are always measurable:

For S1: Node 1 produces exactly N territory labels where N is the number of
domains in the prompt. Each label maps to the correct sub-phase. No label appears
that does not have a grounding in the prompt.

For S2: Node 1 produces at least two territory labels. Each label maps to a
different sub-phase. The sub-phases are adjacent or related in the development
lifecycle (not random). Node 3's plan shows an explicit dependency between the
work in each sub-phase — it does not treat them as independent parallel tasks
if they have ordering constraints.

For S3: Node 1 produces zero territory labels and one or more clarification
questions. The questions are specific — they name the dimension along which the
ambiguity exists. Generic clarification ("can you tell me more?") is a failure.

---

## Stage 2 — Baseline recording (unchanged from v1)

The baseline records what the current orchestrator produces for each scenario
before any training pairs are added. For orchestration experiments, the baseline
failure is usually one of three types:

Type A — Routing failure: Node 1 assigns the prompt to the wrong sub-phase. The
downstream work is done correctly but on the wrong problem.

Type B — Split failure: Node 1 collapses a multi-domain prompt into one territory.
The plan does not address all the dimensions the user needed.

Type C — Ordering failure: Node 1 splits correctly and Node 2 translates correctly,
but Node 3 produces a plan that treats ordered dependencies as parallel steps, or
parallel steps as ordered dependencies.

Type A failures are the most consequential. Type B failures are the most common.
Type C failures are the hardest to detect because the plan looks complete.

---

## Stage 3 — Pair production (orchestration edition)

### The A side of every pair

The A side is always a user prompt written in the language a real developer uses.
Not a formal specification. Not a structured request. A sentence or paragraph a
developer would actually type.

Sources of real developer language:
- XIIGen session files — real prompts from the project being built
- Kubernetes KEP user story sections — real engineer requests from the KEP format
- GitHub issue descriptions from confirmed-license repositories
- Stack Overflow questions (these are CC-BY-SA 4.0, attribution required)

The A must not be a document line. The A must not be a formal requirement
statement beginning with "The system shall." The A must not be a structured
technical specification. If the A reads like it was written by a requirements
analyst rather than a developer typing into a chat interface, it is wrong.

### The B side of Node 1 pairs

The B side is the complete territory array the node must produce. Not a description
of what the array should contain. The actual array, in the actual format:

```
territories: [
  {
    label: "security-and-authorization-model",
    sub_phase: "3.4",
    signal_words: ["token", "validation", "unauthorized"],
    confidence: HIGH
  },
  {
    label: "api-contract-definition",
    sub_phase: "3.3",
    signal_words: ["endpoint", "response schema", "400 error"],
    confidence: HIGH
  }
]
```

When ambiguity is the correct output, the B side is the clarification request:

```
clarification_needed: true
questions: [
  "Are you asking about how the API endpoint is defined (what it accepts and
   returns), or about who is allowed to call it and what they can do?",
  "Is the 400 error you mentioned a contract violation (wrong input shape) or
   an authorization rejection (caller not permitted)?"
]
territories: []
```

### The B side of Node 2 pairs

The A side is one territory label plus the relevant fragment of the user's prompt.
The B side is the formal translation of that fragment into the vocabulary the
downstream nodes for that sub-phase will use.

For a sub-phase 3.4 territory, the formal vocabulary includes: authentication
control type, authorization model type, specific standard identifiers (ASVS
requirement IDs, RFC section references), and the verification claim being made.

For a sub-phase 3.3 territory, the formal vocabulary includes: endpoint identifier,
HTTP method, request body schema summary, response code and body schema, validation
rules, and any breaking-change indicators.

The B must not say "a formal description of the token validation requirement." It
must show the formal description itself.

### The B side of Node 3 pairs

The A side is the complete set of formal territory translations from Node 2.
The B side is the ordered plan. The plan must include:

- Steps numbered in execution order
- Explicit parallel markers for steps that can run simultaneously
- Explicit dependency markers for steps that cannot run until a prior step completes
- Wait states where the plan must pause for external input or confirmation
- Recovery paths for each step that can fail, naming what triggers the recovery
  and what the recovery step does

A plan that lists steps without these structural elements is not a Node 3 B output.
It is a summary. The node trained on summaries learns to summarize.

### Node 7 pairs

The A side is the complete formal result set from Nodes 4 and 5. The B side is
the set of arbiter units that must fire before formatting, with the reason each
was selected:

```
selected_arbiters: [
  {
    unit: "ASVS-V10-OAuth-Arbiter",
    reason: "Output contains an OAuth authorization code flow claim",
    trigger_signal: "authorization_code_flow present in T-security territory result"
  },
  {
    unit: "API-Contract-Consistency-Arbiter",
    reason: "Output modifies an existing API endpoint — contract regression check required",
    trigger_signal: "endpoint_modification flag in T-api-contract territory result"
  }
]
```

---

## Stage 4 — Dataset compilation (unchanged from v1 with one addition)

All pairs must carry their sub-phase identifier. The pair ID format is:
[BATCH]-[SUB-PHASE]-[NODE]-[SEQ], for example: CB5-3.4-N01-001.

The node component is new in v2. In v1 the node was implicit from the file name.
In v2 the sub-phase and node are both explicit in the pair ID because orchestration
pairs often cover multiple nodes in one experiment, and the node must be traceable
from the pair ID alone.

---

## Stage 5 — Orchestration learning test

The evaluation test for orchestration pairs has two distinct measurements.

**Routing accuracy:** Given a held-out prompt, does Node 1 produce the correct
territory array? The correctness criterion is defined in the experiment oracle:
which territories are required, which are forbidden, and which are optional.

**Ordering correctness:** Given a held-out multi-territory prompt, does Node 3
produce a plan with the correct dependency structure? The correctness criterion
is defined in the oracle's expected trace: which steps must precede which, which
can run in parallel, and which wait states are required.

The learning signal test for orchestration is: find a prompt where the current
system routes to the wrong sub-phase, add one new pair that corrects it, and
verify the routing is correct after the pair is added — without regressing on any
previously correct routing.

---

## Stage 6 — Adaptation and anti-regression (unchanged from v1)

The regression check for orchestration pairs is: after each new sub-phase's
pairs are added, verify that the routing accuracy for all prior sub-phases stays
at or above 17/20. A drop below 17/20 on any prior sub-phase is a regression
and takes priority over adding more pairs for new sub-phases.

---

## Stage 7 — Registry update and next batch selection

The next batch selection rule for orchestration: the batch that covers the
sub-phase pair that is currently hardest to distinguish. The hardest-to-distinguish
pair is identified by the routing accuracy on the held-out prompts that span two
adjacent sub-phases. The pair with the lowest routing accuracy gets the next
batch of disambiguation pairs.

---

## What is explicitly deferred

Code review pairs (how to evaluate whether code correctly implements a security
control) are deferred to Wave 3. This includes pairs where Node 8 evaluates
code artifacts rather than design claims.

Implementation guidance pairs (how to implement a specific pattern in a specific
language or framework) are deferred to Wave 4. The orchestrator must know where
a request belongs before it can give implementation guidance.

Cross-phase context carry-forward pairs (how a decision in one sub-phase
constrains the next) are deferred to Wave 5. The orchestrator must route
single-phase requests correctly before it can reason about phase transitions.

---

## Complete pipeline summary (v2)

| Stage | Input | Gate | Output |
|---|---|---|---|
| 0 | External source registry | License confirmed, sub-phase assigned | Confirmed source entry |
| 1 | Source entry | Five oracle documents, orchestration central question, measurable criteria | Experiment design |
| 2 | Experiment design | Baseline routing failure type recorded (A, B, or C) | Evidence file |
| 3 | Evidence file | Pair shape rules satisfied — A is real developer language, B is actual output | Pair files |
| 4 | Pair files | Sub-phase and node in pair ID, stratum assigned | Updated batch registry |
| 5 | Batch registry | Routing accuracy ≥ 17/20 per sub-phase, ordering correctness verified | Comparison table |
| 6 | Comparison table | No routing regression below 17/20 on prior sub-phases | Adaptation test result |
| 7 | Adaptation test | Hardest adjacent sub-phase pair identified | Registry update, next batch selected |
