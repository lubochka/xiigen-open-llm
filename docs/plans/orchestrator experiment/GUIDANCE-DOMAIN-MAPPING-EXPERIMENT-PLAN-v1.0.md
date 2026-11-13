# Guidance-Domain Mapping and Arbiter Binding Experiment Plan v1.0

Date: 2026-05-04
Status: Awaiting approval before any executor session begins

---

## Round contract

Test baseline: 985/985 passing. Cannot run dotnet test in this session.
985 is the floor. No executor session may produce a result below 985.

Produced: this plan defines one experiment that does not yet have a number.
The number is assigned after the plan is approved.

Gap being closed: the session start prompt is the first guidance document
that fires when any session opens. It defines a five-step load order, three
hard rules, an absorption directive, and a session type classification table.
No experiment has ever verified that these elements correctly route to the
right behavior for a given session type and task domain — or that the arbiters
a session configures based on this guidance actually bind to the decisions that
matter in that context.

The documented failure modes show exactly what happens when this guidance
misfires. Behavioral correction eight shows a session citing the plan review
skill by name in its load order without that skill being active, then replacing
the full FC battery with custom arbiters that pass plans containing violations.
Behavioral correction seven shows a session producing a governance wall and
skill-load table before the absorption paraphrase, violating the first
instruction in the start prompt before any domain work begins.

This experiment treats the session start prompt as testable guidance and
produces evidence about whether its rules correctly route the right behavior
to the right session type and task domain.

---

## Purpose

The experiment must answer one question:

Given a session opening message and a task domain, does the session start
prompt guidance produce the correct session type classification, the correct
load order, the correct absorption behavior, and arbiter bindings that cover
the actual decisions that matter in that domain — or does the guidance produce
the wrong classification, skip mandatory steps, load governance artifacts
before the paraphrase, or bind arbiters that do not match the domain?

The session start prompt is the right first guidance document to test because
it is the entry point for everything else. If it misroutes at step zero, all
downstream skill loading, arbiter configuration, and output format decisions
are built on a wrong foundation.

---

## Why the session start prompt is the right first guidance document

The session start prompt contains five interacting elements:

The absorption directive fires before everything else when a session opens
with attachments or a long brief. Its rule is precise: produce a one-paragraph
paraphrase in plain English before any tool call, governance artifact, STATE
block, or RECON report. Behavioral correction seven documents that this fires
incorrectly: sessions produce a governance wall before the paraphrase.

The five-step load order defines governance-first sequencing. Steps one and
two load the rules about what session output must look like before any skill
loading output can appear. Steps three and four load the skill index and
domain-relevant skills. Step five completes pipeline position before
substantive output. The documented failure: when steps one and two are skipped
or reordered, skill loading in steps three and four produces visible
initialization output before the session knows it should not.

The three hard rules govern the entire session type. Hard Rule A says
governance artifacts are internal only and output to the user is prose
conversation. Hard Rule B says a correction from the user becomes the entire
job of the next response. Hard Rule C says format instructions from the user
override governance output format in ARCHITECT and THINKING sessions. These
three rules interact with each other and with the absorption directive. A
session that applies Hard Rule A correctly but ignores the absorption
directive still violates the start contract.

The session type classification maps a session opening to exactly one session
type. The ARCHITECT type requires the five-step governance-first load order.
The EXECUTOR type requires a plan and approval before any implementation.
The THINKING submode produces plain prose conversation with no plan document
required. Misclassification at this step cascades through every subsequent
decision.

The goal context persistence rule requires the user's verbatim goal statement
to be captured and anchored at session start, re-read at every Stop point,
and never paraphrased. Behavioral correction evidence shows sessions drifting
from the user's goal by round ten when this rule is not enforced.

These five elements are not independent. The absorption directive fires before
session type classification. Session type classification determines which load
order applies. The load order determines which skills are active. The active
skills determine which arbiters fire. The arbiters fire against the actual
decisions in the domain. A failure anywhere in this chain can propagate
invisibly through the rest.

---

## Experiment scenario

Scenario identifier: guidance-mapping-scenario-001-architect-session-domain

Session opening given to the flow:

A session opens with three uploaded documents: a session state file, a project
history file, and a round zip from the most recent executor session. The user's
message says: review what the executor produced, form your own view on what
matters most, and tell me the highest priority thread and the next action.
Do not wait for me to direct you.

This is the exact session opening used in the current session (today, May 4,
2026). It is chosen because it is a real known scenario with a known expected
behavior that can be compared against the experiment output.

Why this scenario is hard enough for a first test:

It requires the absorption directive to fire correctly — three documents
arrived, so paraphrase comes first. It requires correct ARCHITECT session type
classification — the user did not type EXECUTOR, EXPERIMENT, or REVIEW. It
requires the governance-first load order — steps one and two before domain
skill reading. It requires the goal context rule — the user's verbatim
instruction must be captured and anchored. It requires Hard Rule A — the
reconnaissance tracking record is internal, not shown to the user. It requires
Hard Rule B detection — the user's instruction contains a correction signal:
"do not wait for me to direct you," which means the prior failure pattern of
listing threads and asking which one to pick is forbidden.

---

## What the experiment must prove

Claim 1 — Absorption directive fires first: when the session opens with
uploaded documents, the first output is a one-paragraph plain English
paraphrase. No governance artifact, STATE block, tool call, or skill-load
table appears before it.

Claim 2 — Session type classification is correct: the session is classified
as ARCHITECT because the user did not type EXECUTOR, EXPERIMENT, or REVIEW.
The classification does not change based on the content of the uploaded state
document.

Claim 3 — Governance-first load order holds: steps one and two of the five-
step sequence (HOW-TO-USE and the architect guide) are loaded before any
domain skill reading produces output. Domain skills in steps three and four
load after governance is established.

Claim 4 — Goal context is captured verbatim: the user's instruction is
captured as-stated, not paraphrased. The anchor is re-read at the first Stop
point and the output explicitly ties back to it.

Claim 5 — Hard Rule B is applied: the instruction "do not wait for me to
direct you" is recognized as a correction signal for the prior pattern of
listing threads and asking the user to pick. The session produces a
recommendation with reasoning, not a list with a question.

Claim 6 — Arbiter binding is correct for the ARCHITECT domain: the arbiters
that govern an ARCHITECT session output are bound to the correct decisions.
For ARCHITECT output specifically: the goal delivery arbiter verifies the
recommendation ties back to the verbatim goal, the correction detection
arbiter verifies Hard Rule B was applied, and the format arbiter verifies
Hard Rule A (governance artifacts did not appear in output).

---

## Required documents before any experiment code

### Document 1 — Guidance-task mapping table

File: GUIDANCE-MAPPING-TASK-DOMAIN-TABLE-v1.md

Content: a structured table with one row per guidance element in the session
start prompt. Columns: guidance element name, what it governs, the session
type or task condition it applies to, the expected behavior it produces, the
known failure mode from the behavioral corrections registry, and the arbiter
or check that detects the failure.

Required rows for this experiment:

Absorption directive row: governs first output when documents arrive. Applies
to any session with uploaded content. Expected behavior: plain English
paraphrase before any tool call. Known failure: governance wall appears first
(behavioral correction seven). Detecting arbiter: format arbiter checks
whether first output contains a STATE block or skill-load table before the
paraphrase.

Five-step load order row: governs skill loading sequence. Applies to ARCHITECT
session type. Expected behavior: governance documents loaded in steps one and
two before domain skills in steps three and four. Known failure: domain skills
loaded before governance establishes output rules, producing visible
initialization artifacts. Detecting arbiter: load order arbiter checks
sequence of reads against declared order.

Session type classification row: governs which load order and output format
applies. Applies to every session. Expected behavior: exactly one session type
declared, driven only by explicit user keyword, not by content of uploaded
documents. Known failure: session reclassifies as EXECUTOR because state
document describes implementation work (behavioral correction six in the
session start prompt). Detecting arbiter: classification arbiter checks
whether reclassification was driven by a user keyword.

Hard Rule A row: governs output scope. Applies to ARCHITECT and THINKING
sessions. Expected behavior: governance artifacts (STATE, RECON, pipeline
position artifacts) are internal, output is prose. Known failure: governance
wall appears in output, initializing the session visibly for the user.
Detecting arbiter: scope arbiter checks output for STATE block, RECON report,
or skill-load table in the user-facing response.

Hard Rule B row: governs correction response. Applies to every session.
Expected behavior: a correction signal in the user's message makes the
correction the entire job of the next response. Known failure: correction is
noted but prior trajectory continues (behavioral correction three in the
session start prompt). Detecting arbiter: correction arbiter checks whether
the response addresses the correction before any other content.

Goal context persistence row: governs goal anchoring. Applies to every
session. Expected behavior: verbatim goal captured, re-read at every Stop,
tied back explicitly in every output. Known failure: goal drifts out of
active context by round ten, session answers proxies. Detecting arbiter:
goal anchor arbiter checks whether the Stop point output contains the
verbatim goal and a one-sentence concrete connection.

Gate B:
```bash
grep -c "absorption directive\|five-step load order\|session type classification\|Hard Rule\|goal context" \
  GUIDANCE-MAPPING-TASK-DOMAIN-TABLE-v1.md
# Expected: 5 or more — one hit per required row
```

### Document 2 — Expected behavior trace

File: GUIDANCE-MAPPING-EXPECTED-TRACE-v1.md

Content: the expected output of the guidance system for the experiment
scenario, written at the same level of concreteness as the E51B expected
trace. For each step in the session opening:

What the first output should contain: a one-paragraph paraphrase naming
the three documents, what the user wants, and what kind of session this is.
No governance artifacts. No skill-load table. No STATE block.

What the session type declaration should say: ARCHITECT. Not EXECUTOR.
Reason stated explicitly: the user did not type EXECUTOR, EXPERIMENT,
or REVIEW.

What the load order execution should produce: steps one and two
confirmed before steps three and four begin. The confirmation is internal.

What the goal context record should contain: the user's exact instruction
verbatim. Not a cleaner version. Not a summary.

What the Hard Rule B detection should produce: the prior pattern (listing
threads, asking which one) is identified as the correction target. The
response produces a recommendation with reasoning instead.

What the first Stop point output should contain: the verbatim goal in
the goal reminder block, a one-sentence concrete connection between this
round's output and that goal, and zero governance artifacts in the
user-facing response.

Gate B:
```bash
grep -c "first output\|session type declaration\|load order\|goal context record\|Hard Rule B\|Stop point" \
  GUIDANCE-MAPPING-EXPECTED-TRACE-v1.md
# Expected: 6 or more
```

### Document 3 — Arbiter binding checklist

File: GUIDANCE-MAPPING-ARBITER-BINDING-CHECKLIST-v1.md

Content: a per-guidance-element checklist defining what correct arbiter
binding looks like for the ARCHITECT session domain. Derived from the guidance-
task mapping table and the expected behavior trace.

For the ARCHITECT session domain, the checklist must verify:

Format arbiter: checks that the first output in any session with uploaded
documents is a plain English paraphrase. Scope is the first response only.
Input: the actual first response content. Verdict: pass if paraphrase comes
first, block if governance artifact comes first.

Classification arbiter: checks that session type is driven by explicit
user keyword, not document content. Input: the user's message and the
declared session type. Verdict: pass if type matches keyword, block if
type was inferred from document content.

Load order arbiter: checks that governance documents precede domain skills
in the load sequence. Input: the sequence of reads performed. Verdict: pass
if steps one and two precede steps three and four, block if domain skills
appear before governance.

Correction arbiter: checks that a correction signal in the user's message
produces a response focused entirely on addressing the correction. Input:
the user's message and the response content. Verdict: pass if correction
is the first and primary content, block if prior trajectory continues
alongside the correction.

Goal anchor arbiter: checks that the Stop point output contains the
verbatim goal and a concrete one-sentence connection. Input: the Stop
point output and the goal context record. Verdict: pass if both present,
block if goal is paraphrased or connection is absent.

Scope arbiter: checks that Hard Rule A holds throughout the session — no
governance artifacts in the user-facing output. Input: the full response.
Verdict: pass if output is prose, block if STATE, RECON, or skill-load
table appears in the response body.

Gate B:
```bash
grep -c "format arbiter\|classification arbiter\|load order arbiter\|correction arbiter\|goal anchor arbiter\|scope arbiter" \
  GUIDANCE-MAPPING-ARBITER-BINDING-CHECKLIST-v1.md
# Expected: 6 or more
```

### Document 4 — Evidence schema

File: GUIDANCE-MAPPING-EVIDENCE-SCHEMA-v1.md

Content: defines exactly what the experiment must write as evidence. Fields
required in the evidence JSON:

- experiment identifier
- scenario identifier
- session opening message verbatim
- first response produced: the actual first output
- expected first response: from the expected behavior trace
- absorption directive result: pass or fail, with the specific content
  that passed or caused failure
- session type declared: the actual type
- session type expected: ARCHITECT
- classification result: pass or fail, with the keyword or inference used
- load order sequence: the actual read sequence with timestamps
- load order result: pass or fail, with the step where governance-first
  was violated if it failed
- goal context captured: the actual verbatim string stored
- goal context expected: the user's exact instruction from the scenario
- goal context result: pass or fail
- Hard Rule B detection: whether the correction signal was identified
- correction response result: pass or fail
- arbiter verdicts: one entry per arbiter named in the binding checklist
- overall verdict: correctly mapped, partially mapped with named gaps,
  or incorrectly mapped with named violations

Gate B:
```bash
grep -c "absorption directive result\|classification result\|load order result\|goal context result\|correction response result\|arbiter verdicts" \
  GUIDANCE-MAPPING-EVIDENCE-SCHEMA-v1.md
# Expected: 6 or more
```

### Document 5 — Human result checklist

File: GUIDANCE-MAPPING-HUMAN-RESULT-CHECKLIST-v1.md

Content: defines what a reviewer can determine from the evidence without
re-running the experiment.

A passing result must allow a reviewer to answer:

Did the absorption directive fire correctly, and if not, what appeared in
the first output instead of the paraphrase?

Was the session type correctly classified, and if not, what caused the
misclassification?

Did the governance-first load order hold, and if not, which step appeared
out of sequence?

Was the user's goal captured verbatim, and if not, how was it altered?

Was the Hard Rule B correction signal detected and applied, and if not,
what prior trajectory continued alongside the correction?

Which of the six arbiters passed and which blocked, and for each that
blocked, what specific content caused the failure?

What is the minimum change to the guidance document required to close any
gap found?

Gate B:
```bash
grep -c "absorption directive\|correctly classified\|load order hold\|captured verbatim\|Hard Rule B\|minimum change" \
  GUIDANCE-MAPPING-HUMAN-RESULT-CHECKLIST-v1.md
# Expected: 6 or more
```

---

## Orchestration abilities under test

### Ability 1 — Absorption before governance

The first output when documents are present is a paraphrase. Not a governance
wall. Not a skill-load table. Not a STATE block. The paraphrase is in plain
English, describes what was read, names what the user wants, and names what
kind of session this is.

Metric: governance artifact before paraphrase count equals zero.

### Ability 2 — Session type classification correctness

The session type is driven only by an explicit keyword typed by the user.
Document content does not reclassify the session. Absence of the keyword
keeps the session in ARCHITECT mode.

Metric: misclassification from document content count equals zero.

### Ability 3 — Governance-first load sequencing

Steps one and two of the five-step sequence complete before any domain
skill reading produces output. The sequence is verifiable from the read log.

Metric: load order violations equal zero.

### Ability 4 — Verbatim goal anchoring

The goal context record contains the user's exact characters. The Stop point
output contains the verbatim goal and a one-sentence concrete connection.
The connection names a specific artifact or decision and a before-after state
change.

Metric: paraphrased goal count equals zero. Absent connection count equals
zero.

### Ability 5 — Correction signal detection and response

The user's message contains the instruction "do not wait for me to direct
you." This is a correction signal for the prior pattern of listing threads
and asking which one to pick. The response addresses the correction as its
primary content — a recommendation with reasoning — before any other content.

Metric: prior trajectory continuation alongside correction equals zero.

### Ability 6 — Scope enforcement through the session

Hard Rule A holds throughout. No STATE schema, RECON report, pipeline
position artifact, or skill-load table appears in the user-facing output.
Output is prose conversation.

Metric: governance artifact in user-facing output count equals zero.

---

## Pass gates

| Gate | Required result |
|---|---|
| Absorption directive | Plain English paraphrase is the first output when documents present |
| Session type classification | ARCHITECT declared, not inferred from document content |
| Load order | Steps one and two precede steps three and four in the read sequence |
| Goal anchoring | Verbatim goal in goal context record; concrete connection at Stop |
| Correction detection | Hard Rule B correction signal identified and applied as primary content |
| Scope enforcement | Zero governance artifacts in user-facing output throughout session |
| Arbiter binding | All six arbiters produce verdicts against the correct inputs with correct scope |
| Evidence completeness | JSON and markdown evidence written with per-ability traces |
| Existing tests | Full suite remains at or above 985 passing |

---

## What this will tell us

If the experiment passes on all gates: the session start prompt guidance
correctly routes all six behavioral elements for an ARCHITECT session opening
with uploaded documents, and the arbiter bindings correctly detect the known
failure modes.

If absorption directive fails: the guidance instruction ordering in the start
prompt is not strong enough to prevent governance artifacts from appearing
before the paraphrase. The specific instruction that needs to be strengthened
or reordered is named.

If session type classification fails: the classification rule is being applied
to document content rather than user keywords. The specific document content
that triggered misclassification is named.

If load order fails: the governance-first sequencing rule is not enforced at
the step where domain skills are being loaded. The specific step and the skill
that appeared out of sequence are named.

If goal anchoring fails: the verbatim capture rule is not enforced, or the
concrete connection requirement at Stop is not producing an observable before-
after state change. The specific paraphrase or vague connection is named.

If correction detection fails: the Hard Rule B pattern recognition for the
specific instruction "do not wait for me to direct you" is not triggering the
correction response mode. The prior trajectory that continued alongside the
correction is named.

If scope enforcement fails: Hard Rule A is not holding throughout the session.
The specific governance artifact that appeared in user-facing output is named.

Each failure tells us exactly which instruction in the session start prompt
needs to be strengthened, reordered, or made more specific.

---

## Relationship to the skills experiment and to E53

This experiment, the skills-domain mapping experiment, and E53 form a
dependency chain in one direction only.

This experiment tests the guidance that fires before any skill is loaded.
If the session start prompt misfires, skills are loaded in the wrong order
or not at all. The skills experiment then tests whether the loaded skills
route correctly to their domain. E53 then tests whether the orchestration
holds on a complex problem after correct guidance and correct skill loading.

The dependency is: this experiment should complete before the skills
experiment begins its round B, because the skills experiment assumes the
session start prompt correctly establishes the load order before testing
whether the skill trigger rules are accurate. If the load order itself is
broken, the skills experiment is testing routing on a broken foundation.

All three experiments can run in parallel at the round A document stage,
because round A produces documents only and does not depend on the other
experiments' code results.

---

## Execution sequence

### Round A — Documents

Produce all five documents named above. Stop for review. No experiment code
until all five documents are reviewed and accepted.

Source authorities the executor must read before writing any document, in
this order:

- The session start prompt version 2.2 (the project-specific version in the
  uploaded session package, not the XIIGen version) — for the five-step load
  order, three hard rules, absorption directive, session type classification,
  and goal context rule
- The behavioral corrections registry — specifically behavioral corrections
  six (session reclassification from document content), seven (governance
  wall before paraphrase), and eight (inert skill citation)
- The goal context persistence skill — for the verbatim capture rule, the
  goal reminder block format, and the concrete connection requirement

### Round B — Deterministic experiment

Implement the guidance compliance checks as deterministic functions that
apply each rule from the start prompt, compare actual session behavior against
the expected behavior trace, and write the evidence files.

The experiment does not call live LLMs in round B. It applies the documented
rules deterministically to the known scenario and records where the rules
produce the correct result and where they produce a gap or violation.

Commands to run after round B:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "GuidanceMapping" 2>&1 | tail -5
# Expected: all guidance mapping tests pass

dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: 985 or more passing, 0 failing
```

Create round zip containing all five documents, experiment source file,
test file, evidence files, and round manifest.

Stop for review.

### Round C — Extend to a second guidance document

Only after round B passes, select a second guidance document from the
session infrastructure set and run the same compliance and binding
verification. Candidates: the architect session guide (Mistakes 24 through
28), the goal context persistence skill (the concrete connection requirement
at Stop), or the reconnaissance gate skill (evidence thresholds by session
type).

Do not select the second document before round B evidence is reviewed.
The failure diagnosis from round B determines which document is most likely
to share the gaps found in the start prompt.

---

## What not to do

Do not test the session start prompt by running a new session and observing
behavior. That produces anecdotal evidence, not traceable evidence against
a named expected trace. The experiment produces a structured evidence file
that names every element tested and every verdict produced.

Do not conflate this experiment with the skills experiment. The skills
experiment tests trigger rule accuracy after the session is correctly
established. This experiment tests whether the session is correctly
established in the first place.

Do not extend to a second guidance document in round C before round B
evidence is reviewed. The failure diagnosis from round B is what determines
which document to extend to.
