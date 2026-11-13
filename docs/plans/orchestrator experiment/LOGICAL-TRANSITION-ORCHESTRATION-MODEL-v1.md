# Logical Transition Orchestration Model
# Version: 1.0 - 2026-05-05

---

## Purpose

This document corrects the orchestration model after reviewing:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\llm_pipeline_corrected.svg
C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\improve algo.drawio.xml
```

The human prompt does not go directly to expert splitting.

The first step is transition normalization:

```text
human prompt -> logical transition request
```

Only after that does the machine split the work between development expertise
areas and specific tasks.

---

## Core Principle

Every ILogicalUnit works on a functional principle, not on broad human wording.

The core shape is:

```text
A -> B
```

Where:

```text
A = source/current/problem state
B = target/solution/result state
X = transformation/action/rule that moves A toward B
```

A decomposed orchestration becomes:

```text
A -> B
A1 -> B1 + X1
A2 -> B2 + X2
A3 -> B3 + X3
...
An -> Bn + Xn
```

The whole dynamic flow is a controlled decomposition and recomposition of these
logical transitions.

---

## Correct High-Level Flow

```text
1. Human prompt
2. Transition normalizer
3. Expertise/task splitter
4. Prompt simplifier
5. Domain-language translator
6. Scheme planner
7. Parallel logical units
8. Applier units
9. Result translator
10. Arbiter selection and arbiter review
11. Reprocess loop when arbiter rejects or requests repair
12. Final human formatter
```

The final output is translated back into human language only after the logical
results have been reviewed and approved.

---

## Transition Normalizer

### Role

```text
logical-transition-normalizer
```

### Input

```text
human prompt
available context
known topology
source authority
default flags
```

### Output

```text
source_state_A:
target_state_B:
transformation_intent_X:
constraints:
unknowns:
rejected_intents:
allowed_scope:
initial_confidence:
```

### Example

Human prompt:

```text
Add multi-tenant authentication to the API, preserve internal-first execution,
avoid duplicate identity creation, enforce token and tenant isolation, test it,
review it for security, and add automation so an unsafe release cannot pass.
```

Transition output:

```yaml
source_state_A:
  - API currently lacks the requested multi-tenant authentication orchestration.
  - Tenant isolation, token isolation, tests, security review, and release gate
    evidence are not yet established in this request.
target_state_B:
  - API authentication lifecycle enforces tenant and token isolation.
  - Duplicate identity creation is rejected.
  - Security evidence blocks unsafe release.
transformation_intent_X:
  - design security and identity boundaries
  - implement backend and provider enforcement
  - test isolation and failure paths
  - review security evidence
  - automate release blocking
constraints:
  - internal-first execution
  - no duplicate identity creation
  - no invented security source authority
unknowns:
  - project-specific tenant source if absent from context
rejected_intents:
  - creating a second account for an existing identity
allowed_scope:
  - orchestration plan and sub-task production before code implementation
```

This object is now the source A for expert splitting.

---

## Expertise And Task Splitter

The splitter no longer receives the raw human prompt alone.

It receives:

```text
logical transition request
human prompt reference
topology
source authority
```

It emits expert territories:

```text
domain d1 ... dm
phase/sub-phase assignments
specific task branches
dependencies
parallel opportunities
knowledge edges
arbiter needs
```

The splitter is not allowed to invent branches simply because they commonly
follow from the request. Every emitted branch must be grounded in:

```text
transition A;
target B;
transformation X;
explicit constraint;
source context;
or necessary dependency.
```

---

## Prompt Simplifier

Each expert branch receives a compact prompt.

The compact prompt is not a summary for a human. It is a focused logical source
for an ILogicalUnit.

### Input

```text
branch territory
source_state_A
target_state_B
transformation_intent_X
constraints relevant to the branch
```

### Output

```text
shortened_branch_source_Ai
target_branch_result_Bi
required_transformation_Xi
```

### Example

For the security branch:

```yaml
A1:
  "API authentication must enforce tenant and token isolation, but the current
  request has not yet established the security rule set."
B1:
  "Security model defines tenant isolation, token validation, duplicate identity
  rejection, and evidence required for release blocking."
X1:
  "Translate source authority from Wave 1 and XIIGen rules into a security model
  branch for sub-phase 3.4."
```

This is what a logical unit should work on.

---

## ILogicalUnit Boundary

Every unit receives a specific A and produces B plus optional X evidence.

```text
unit_input = Ai
unit_output = Bi + Xi evidence
```

The unit must not solve the whole user prompt. It solves only its assigned
functional principle.

This keeps each unit small, trainable, inspectable, and replaceable.

---

## Scheme Planner

The scheme planner receives the set of expert branch transitions:

```text
A1 -> B1 + X1
A2 -> B2 + X2
...
An -> Bn + Xn
```

It creates:

```text
ordered plan;
parallel groups;
dependencies;
wait states;
reprocess points;
arbiter checkpoints;
```

The planner is responsible for saying which transformations are independent and
which must wait.

---

## Arbiters

Arbiters review logical results before final human formatting.

They see:

```text
all branch results R1 ... Rn;
the original A -> B transition;
each branch Ai -> Bi + Xi;
source authority;
constraints;
rejected intents.
```

Arbiters can:

```text
pass a result;
raise a concern;
block a result;
send a branch back for reprocessing with a corrected Xi;
force a knowledge edge when source authority is missing.
```

The reprocess loop goes back to the relevant branch, not to the whole prompt,
unless the arbiter finding proves the original split was wrong.

---

## Result Translation

Result translation happens after logical work.

It converts:

```text
R1 ... Rn
arbiter verdicts
blocked branches
knowledge edges
next decisions
```

into human language.

The formatter must preserve:

```text
can proceed;
blocked;
rejected;
unknown;
next human decisions;
resource measurements when this is a teaching run.
```

It must not soften a rejection into a recommendation.

---

## Teaching Implication

The first learning set must teach transition normalization before broad expert
splitting.

The correct first teaching chain is:

```text
human prompt -> A/B/X transition object
A/B/X transition object -> expert branches
expert branch -> compact Ai/Bi/Xi logical source
Ai -> Bi + Xi result
logical results -> reviewed result
reviewed result -> human answer
```

This means the first trained unit becomes:

```text
logical-transition-normalizer
```

The second trained unit becomes:

```text
orchestration-prompt-splitter
```

The previous plan that started directly at the prompt splitter is close, but it
misses the explicit A/B/X transition object.

---

## Last Updated

2026-05-05. This model locks the A -> B, A1 -> B1 + X1 principle as the learning
shape for each ILogicalUnit.
