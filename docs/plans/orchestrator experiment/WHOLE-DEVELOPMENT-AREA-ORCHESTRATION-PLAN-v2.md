# Whole Development Area Orchestration Plan
# Version: 2.0 - 2026-05-05
# Supersedes: WHOLE-DEVELOPMENT-AREA-ORCHESTRATION-PLAN-v1.md

---

## Purpose

This plan defines how to teach and test the dynamic flow as a real
development-cycle orchestrator.

The orchestrator is not being tested as a code explainer. The test is whether it
can take a real user prompt and manage the full development problem:

```text
understand the prompt;
split it into the right domains;
select the right phases and sub-phases;
sequence the work;
preserve decisions and constraints across transitions;
select arbiters;
reject contradictions;
name knowledge edges;
produce proper sub-tasks;
produce a human-readable orchestration result.
```

Implementation code is secondary in this stage. The primary product is the
logical orchestration result.

---

## Current Reality

The build-ready corpus exists. The system has enough structured examples to
start real teaching cycles.

What is not complete yet is actual learning inside the orchestration units.
Several existing units still behave as deterministic C# logic. Those units are
useful as baselines, but they are not the final learning system.

This plan starts the learning process at the correct level: the prompt-level
orchestration units.

---

## Main Hypothesis

The dynamic flow can learn to manage the whole development lifecycle if the
first learned unit is trained to split a real prompt into a lifecycle graph.

The lifecycle graph contains:

```text
domains;
phases;
sub-phases;
parallel groups;
sequential dependencies;
transition context;
conflict points;
knowledge edges;
arbiter responsibilities;
sub-task outputs.
```

If the first split is wrong, every downstream unit may appear locally correct
while the full orchestration is wrong. Therefore prompt splitting and phase
sequencing come before node-08 arbiter learning.

---

## Development Lifecycle Under Test

The canonical lifecycle remains seven phases and thirty-one sub-phases.

| Phase | Sub-phases |
|---|---|
| 1 UI/UX definition | 1.1 design system and tokens; 1.2 screen and layout structure; 1.3 interaction model and state transitions; 1.4 accessibility and compliance; 1.5 motion and animation |
| 2 Functional specification | 2.1 user stories and acceptance criteria; 2.2 business rules; 2.3 integration point specification; 2.4 data model boundaries |
| 3 Technical design | 3.1 architecture and component topology; 3.2 data schema and migration design; 3.3 API contract definition; 3.4 security and authorization model; 3.5 infrastructure and deployment topology |
| 4 Implementation | 4.1 backend services; 4.2 frontend and client rendering; 4.3 integration and provider connectors; 4.4 data migration execution; 4.5 documentation |
| 5 Testing | 5.1 unit and contract tests; 5.2 integration tests; 5.3 end-to-end scenario tests; 5.4 performance and load tests |
| 6 QA | 6.1 functional review; 6.2 regression coverage verification; 6.3 security and compliance review; 6.4 accessibility audit |
| 7 QA automation | 7.1 test scenario automation; 7.2 regression suite maintenance; 7.3 CI/CD gate configuration; 7.4 coverage and reporting |

The lifecycle is not hardcoded as one company methodology. It is the starting
topology. A methodology variant becomes a topology mutation, not an orchestrator
code change.

---

## Required Orchestration Units

These units are trained and evaluated in order.

### 1. Orchestration prompt splitter

Input:

```text
user prompt
available source context
current lifecycle topology
default logical flags
```

Output:

```text
selected domains
selected phases
selected sub-phases
initial dependency graph
conflicts
knowledge edges
required arbiters
confidence per selected branch
```

This is the first real learning target.

### 2. Phase sequencing unit

Input:

```text
selected phase/sub-phase graph
known topology transitions
conflict and knowledge-edge markers
```

Output:

```text
ordered phase plan
parallel groups
wait states
transition points
```

### 3. Context carry-forward unit

Input:

```text
completed upstream phase result
downstream phase request
predicate vectors from both sides
```

Output:

```text
constraints to carry forward
decisions to carry forward
knowledge edges to preserve
rejected items to preserve
boundary markers to preserve
```

### 4. Transition verifier unit

Input:

```text
upstream carry-forward packet
downstream planned work
```

Output:

```text
PASS
PASS_WITH_GAPS
BLOCK
named findings
required repair action
```

### 5. Arbiter selection unit

Input:

```text
planned work item
domain tags
risk markers
source authority tags
```

Output:

```text
required arbiters
why each arbiter applies
which arbiter has final authority
```

### 6. Sub-task planner unit

Input:

```text
accepted lifecycle graph
phase sequence
arbiter requirements
known gaps
```

Output:

```text
actionable sub-tasks
dependencies between sub-tasks
blocked sub-tasks
human decisions needed
```

### 7. Human-result formatter unit

Input:

```text
full orchestration trace
sub-task graph
arbiter verdicts
knowledge edges
resource metrics
```

Output:

```text
human-readable orchestration result
```

The formatter must not hide rejected, blocked, or unknown work.

---

## Real Scenario Suite

The scenario suite uses XIIGen and license-checked external sources.

### O1 - Multi-tenant authentication addition

User prompt:

```text
Add multi-tenant authentication to the API, preserve internal-first execution,
avoid duplicate identity creation, enforce token and tenant isolation, test it,
review it for security, and add automation so an unsafe release cannot pass.
```

Expected selected areas:

```text
3.1 architecture and component topology
3.3 API contract definition
3.4 security and authorization model
4.1 backend services
4.3 integration and provider connectors
5.1 unit and contract tests
5.2 integration tests
6.3 security and compliance review
7.3 CI/CD gate configuration
7.4 coverage and reporting
```

Main risks:

```text
tenant isolation dropped after design;
security design not carried into implementation;
tests verify token validity but not tenant boundary;
QA accepts missing isolation evidence;
automation reports coverage without blocking release.
```

### O2 - Wave-gated flow registration

User prompt:

```text
Register a new dynamic-flow capability through the wave system, make sure the
flow is discoverable, traceable, versioned, tested, and cannot be promoted when
its conformance evidence is missing.
```

Expected selected areas:

```text
2.1 user stories and acceptance criteria
2.3 integration point specification
3.1 architecture and component topology
3.3 API contract definition
4.1 backend services
4.3 integration and provider connectors
5.1 unit and contract tests
5.3 end-to-end scenario tests
6.2 regression coverage verification
7.3 CI/CD gate configuration
```

### O3 - Performance regression investigation

User prompt:

```text
The inspector or orchestration run is consuming too much CPU and RAM. Find the
right lifecycle path to diagnose it, preserve user-visible behavior, avoid
heavy visualization by default, and add performance evidence to prevent
regression.
```

Expected selected areas:

```text
3.1 architecture and component topology
4.2 frontend and client rendering
5.4 performance and load tests
6.2 regression coverage verification
7.2 regression suite maintenance
7.4 coverage and reporting
```

### O4 - External source integration under license constraint

User prompt:

```text
Use the license-checked external sources to teach the local machine, preserve
source authority, avoid inventing missing facts, route security claims to the
right arbiters, and produce a traceable learning set.
```

Expected selected areas:

```text
2.2 business rules
2.3 integration point specification
3.4 security and authorization model
4.3 integration and provider connectors
5.2 integration tests
6.3 security and compliance review
7.4 coverage and reporting
```

### O5 - Skill advisor full lifecycle

User prompt:

```text
Build a skill advisor that can inspect project documents, identify which skills
or flows apply, explain missing context, route work through the development
lifecycle, learn from review corrections, and produce QA automation evidence.
```

Expected selected areas:

```text
all seven phases
all thirty-one sub-phases
all six adjacent transitions
```

O5 is the full lifecycle proof. It is not the first run.

---

## Teaching Data Per Scenario

Each scenario produces:

```text
one original prompt row;
one Phrasing B prompt row;
five held-out prompt rows;
three corruption rows;
one expected split graph;
one expected sequence graph;
one context carry-forward table;
one arbiter responsibility table;
one final human result;
one resource evidence template.
```

The corruption rows are deliberately wrong. They teach rejection and transition
repair. They are not negative user examples to be softened.

---

## Measurement

Every teaching cycle records:

```text
teaching time;
feature extraction time;
model build time;
model load time;
prompt processing time;
CPU time;
RAM peak;
managed allocation;
drive read/write allocation;
model file size;
training corpus size.
```

Every prompt run records:

```text
selected phases;
selected sub-phases;
lost context predicates;
wrong transition verdicts;
missing arbiter selections;
invented knowledge-edge solutions;
reopened rejected items;
sub-task count;
blocked sub-task count;
human result completeness.
```

The resource evidence is part of the learning record, not an optional profiling
note.

---

## What Success Means

The system is considered useful for this stage when:

```text
O1 through O4 can be split, sequenced, and explained by learned orchestration units;
O5 can run through the deterministic baseline and learned splitter without losing phase coverage;
at least one missed held-out prompt is corrected by adding a new teaching row and retraining;
resource use stays low enough to run on this PC;
all produced sub-tasks are traceable back to source facts, constraints, or gaps.
```

This is not a claim of production maturity. It is the point where the project
has real learning evidence at orchestration level.

---

## First Execution Path

The first execution path is O1.

O1 is narrow enough to train now and complex enough to prove real
orchestration:

```text
security design -> backend implementation -> integration tests -> security QA -> automation gate
```

The first trained unit is:

```text
orchestration-prompt-splitter
```

The first comparison is:

```text
deterministic split fixture vs trained splitter vs retrained splitter after one missed held-out prompt
```

The first final result must be a human-readable orchestration plan, not code.

---

## Last Updated

2026-05-05. This version moves the plan from structure proof to measured
orchestration teaching.
