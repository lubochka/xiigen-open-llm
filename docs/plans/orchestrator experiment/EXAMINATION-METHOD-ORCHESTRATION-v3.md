# Document Examination To Training Sets
# Complete orchestration edition
# Version: 3.0 - 2026-05-05
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v2.md

---

## Purpose

This document defines how to examine source material and turn it into learning
sets for the full orchestration system.

The focus is prompt-level orchestration:

```text
user prompt -> domains -> phases -> sub-phases -> transitions -> arbiters -> sub-tasks -> human result
```

Node 1, Node 2, Node 3, and Node 7 are still important. They are no longer the
whole target. They are internal stations inside a larger orchestration learning
cycle.

---

## What A Correct Orchestration Pair Teaches

A correct pair teaches relationships.

It teaches:

```text
which domains are present;
which phases and sub-phases are required;
which selected items can run in parallel;
which items must wait;
which upstream decisions constrain downstream work;
which missing source must stay a knowledge edge;
which contradiction must be rejected;
which arbiter has authority;
which sub-tasks should be produced;
what the human result must honestly say.
```

Mechanical word matching is not enough. A pair that names the right sub-phases
but loses the dependency between them is wrong.

---

## The Main A Side

The A side is a real user prompt or a faithful prompt derived from a real source.

It must be the kind of text a developer, architect, QA reviewer, or product owner
would actually type:

```text
I need to add multi-tenant authentication to the API and make sure unsafe
releases cannot pass when token isolation is missing.
```

It must not be a formal requirement sentence unless the source itself is being
used to teach formal-document routing:

```text
The system shall implement multi-tenant authentication per section X.
```

The first sentence is prompt-splitter material. The second is formal vocabulary
or source-authority material.

---

## The Main B Side

The B side is the actual orchestration result object.

It contains:

```text
selected_domains:
selected_phases:
selected_sub_phases:
parallel_groups:
sequential_dependencies:
transition_points:
context_carry_forward:
conflicts:
knowledge_edges:
required_arbiters:
sub_tasks:
human_result:
resource_measurement_required:
```

It must not describe the result. It must show the result.

---

## Required Pair Families

Each real scenario produces five pair families.

### Family 1 - Prompt Split

Teaches the prompt splitter.

Input is the full user prompt. Output is the lifecycle split graph.

### Family 2 - Phase Sequence

Teaches the phase sequencing unit.

Input is the split graph. Output is ordered execution with parallel groups,
wait states, and dependency rules.

### Family 3 - Context Carry-Forward

Teaches the context carry-forward unit.

Input is an upstream phase result and a downstream phase request. Output is what
must be carried forward and why.

### Family 4 - Transition Verification

Teaches the transition verifier.

Input is a carry-forward packet and downstream planned work. Output is PASS,
PASS_WITH_GAPS, or BLOCK with named findings.

### Family 5 - Arbiter And Sub-Task Planning

Teaches arbiter selection and actionable sub-task production.

Input is the accepted lifecycle plan with risks and source authority. Output is
the arbiter list, sub-tasks, blocked tasks, and human decisions.

---

## Orchestration Failure Modes

### OF-1 - Under-split prompt

The prompt spans multiple phases or sub-phases, but the B side selects only one.

Correction: add every grounded phase/sub-phase, with evidence words from the A.

### OF-2 - Over-split prompt

The B side adds likely follow-up work that the prompt did not ask for and no
source context requires.

Correction: remove invented branches. Planning likely follow-up work belongs in
the sub-task planner only after it is justified as a dependency.

### OF-3 - Wrong sub-phase

The B side assigns a grounded concern to the wrong sub-phase.

Correction: move the branch and update all downstream transitions.

### OF-4 - Missing transition

The B side selects correct sub-phases but does not connect them.

Correction: add transition edges and carry-forward rules.

### OF-5 - Context loss

An upstream decision, constraint, rejection, or knowledge edge disappears in a
downstream phase.

Correction: add carry-forward packet and transition verification row.

### OF-6 - False conflict

The B side blocks work that is only a different artifact type or build-time
sequence.

Correction: add artifact-type distinction. This is the same lesson as build
order versus runtime topology.

### OF-7 - Missed conflict

The B side accepts downstream work that contradicts an upstream constraint.

Correction: add transition-verifier and arbiter rows.

### OF-8 - Missing arbiter

The B side produces sub-tasks but does not name the review authority that must
judge the risky branch.

Correction: add arbiter-selection row with selected and not-selected arbiters.

### OF-9 - Invented knowledge-edge solution

The B side solves missing source authority instead of naming the gap.

Correction: change output to knowledge edge and add human decision.

### OF-10 - Unmeasured learning

The row claims teaching occurred but no teaching time, prompt time, CPU, RAM, or
model size is recorded.

Correction: add resource metrics evidence. A teaching cycle without measurements
is not a learning-cycle artifact.

---

## Source Examination Procedure

### Step 1 - Identify source authority

Record source id, path or URL, license, attribution, and source state.

### Step 2 - Identify prompt material

Find user-facing request language. For XIIGen sessions, copy Luba's prompt
verbatim. For engineering proposals, use motivation, goals, user stories, and
problem statements.

### Step 3 - Identify formal authority

Find the sections that define correctness. These sections become B-side evidence
or arbiter authority, not prompt wording.

### Step 4 - Identify lifecycle span

Mark every phase and sub-phase the prompt actually touches.

Do not include likely work unless the source states it as required or the
dependency is necessary.

### Step 5 - Identify relationships

Name:

```text
dependencies;
parallel branches;
transition points;
source gaps;
conflicts;
required arbiters;
human decisions.
```

### Step 6 - Write B as the object

Write the actual graph, packet, verdict, arbiter list, or sub-task list.

Do not write a description of what the object should include.

---

## O1 Minimum Example Shape

### A

```text
Add multi-tenant authentication to the API, preserve internal-first execution,
avoid duplicate identity creation, enforce token and tenant isolation, test it,
review it for security, and add automation so an unsafe release cannot pass.
```

### B

```yaml
selected_domains:
  - identity-and-account-resolution
  - api-contract-definition
  - security-and-authorization-model
  - backend-services
  - integration-and-provider-connectors
  - unit-and-contract-tests
  - integration-tests
  - security-and-compliance-review
  - ci-cd-gate-configuration
  - coverage-and-reporting
selected_sub_phases:
  - "3.1"
  - "3.3"
  - "3.4"
  - "4.1"
  - "4.3"
  - "5.1"
  - "5.2"
  - "6.3"
  - "7.3"
  - "7.4"
sequential_dependencies:
  - "3.1 before 3.3 because topology defines identity boundaries"
  - "3.3 before 4.1 because implementation must honor contract"
  - "3.4 before 4.1 because security constraints bind backend logic"
  - "4.1 and 4.3 before 5.2 because integration tests need implemented boundaries"
  - "5.2 before 6.3 because security review must inspect evidence"
  - "6.3 before 7.3 because release gate must encode review verdict"
parallel_groups:
  - ["3.3", "3.4"]
context_carry_forward:
  - "tenant isolation constraint"
  - "token validation constraint"
  - "duplicate identity rejection"
  - "internal-first execution"
conflicts:
  - "duplicate identity creation is rejected"
knowledge_edges:
  - "missing project-specific tenant model if no source document provides it"
required_arbiters:
  - "identity-consistency-arbiter"
  - "tenant-isolation-arbiter"
  - "token-safety-arbiter"
  - "source-authority-arbiter"
  - "release-gate-arbiter"
sub_tasks:
  - "decide tenant identity boundary"
  - "define API contract changes"
  - "define token and tenant security controls"
  - "implement backend enforcement"
  - "connect provider identity flow"
  - "write unit and integration tests"
  - "perform security review"
  - "add CI/CD blocking gate"
human_result:
  can_proceed:
    - "split and plan the lifecycle work"
  blocked:
    - "project-specific tenant model if source is absent"
  rejected:
    - "duplicate identity creation"
  unknown:
    - "any missing tenant-source authority"
  next_decisions:
    - "choose tenant identity source"
    - "choose token validation authority"
resource_measurement_required: true
```

---

## Rephrasing Rule

Every prompt has at least one Phrasing B companion.

The companion changes perspective, not meaning.

Developer perspective:

```text
I need to add multi-tenant authentication to the API...
```

System-owner perspective:

```text
We need the authentication lifecycle to enforce tenant isolation from design
through release automation...
```

The output graph should remain logically equivalent.

---

## Held-Out Rule

Each scenario needs held-out variants:

```text
compact;
long developer-session;
ambiguous;
planted contradiction;
safety/compliance boundary.
```

Held-out rows are not used to train the first model. They measure whether the
learned splitter generalizes beyond the exact training prompt.

---

## Measurement Rule

Every teaching-cycle artifact includes resource metrics.

Required fields are defined in:

```text
ORCHESTRATION-RESOURCE-METRICS-SPEC-v1.md
```

If timing, CPU, RAM, allocation, and model-size data are absent, the artifact is
not a real learning-cycle artifact.

---

## Last Updated

2026-05-05. Version 3 expands the method from node-focused routing pairs to
complete orchestration learning rows.
