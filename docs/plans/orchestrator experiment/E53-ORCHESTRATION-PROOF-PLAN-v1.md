# Orchestration Proof — Experiment Design Plan
# E53: Dynamic Flow Orchestrator Validation
# Status: DESIGN — awaiting approval before any execution begins

---

## What This Experiment Proves

The dynamic flow is an orchestration architecture. The LLM nodes are
decision-making units. The claim being tested is not whether nodes produce
correct domain-specific output — E51A and E51B already proved that for the
registration domain. The claim being tested is whether the nine-node pipeline
correctly manages a genuinely complex multi-domain problem: decomposing it,
planning it with real dependencies and constraints, detecting when a plan
violates a requirement, handling a knowledge edge without inventing an answer,
and producing a human-readable result that is honest about what was resolved
and what was not.

Stack independence is a design requirement, not an implementation detail. The
orchestrator must produce correct orchestration on problems that have nothing
to do with C# or the llm_mvp_core codebase. If the orchestration logic is
correct, the same pipeline handles a software architecture decision, a
business process design, a compliance review, and a technical migration plan
— because the decomposition, planning, and review logic operates on problem
structure, not domain vocabulary.

---

## The Three Scenarios

Three scenarios are selected. Each is chosen because it creates pressure on
a specific part of the orchestration that cannot be tested with a clean
single-domain problem.

### Scenario A — The Conflicting Requirements Scenario

**Problem:** A team is building a real-time collaborative document editor.
They need sub-100ms edit propagation to all active users. They also need full
edit history with the ability to replay any document state from the beginning.
They have three services: an editor frontend, a backend sync service, and a
storage service. The sync service currently uses REST polling every 2 seconds.
The storage service writes every edit as an immutable append-only event.
The team has six weeks.

**Why this scenario tests the orchestrator:**
It crosses at minimum four domain territories that have real dependencies and
a genuine tension. Reducing propagation latency requires WebSocket or
server-sent events — but the storage service's append-only event log must be
the source of truth for replay, which means the sync service cannot just
forward edits without persisting them first. A naive plan routes these in
parallel and misses the dependency. The correct plan establishes a sequencing
constraint: the sync service must write to the storage event log before
broadcasting to connected clients, or replays will diverge from the live
state.

The arbiter must catch any plan that proposes client-first propagation before
storage write. This is not a code review — it is a constraint enforcement
check on a proposed system design.

**Coverage types present:**
All four domain territories covered, two with real dependency constraints
between them, one wait state (storage write before broadcast), one review
point (the arbiter must catch client-first proposals), and a timeline
constraint that affects the plan ordering.

---

### Scenario B — The Knowledge Edge Scenario

**Problem:** A company is designing the authentication system for a new
financial services platform. Requirements: multi-factor authentication, session
management across web and mobile clients, regulatory compliance with PSD2
Strong Customer Authentication for European users, and a biometric fallback
for users who cannot use TOTP. The platform launches in four months.

**Why this scenario tests the orchestrator:**
PSD2 Strong Customer Authentication is a knowledge edge. The correct behavior
is for the orchestrator to identify the covered territories (MFA, session
management, TOTP), produce a plan for those territories, and explicitly name
the knowledge edge for PSD2 SCA rather than inventing compliance rules. The
biometric fallback is a second knowledge edge related to the registration flow
knowledge edge already established in E51B.

A failing orchestrator invents PSD2 compliance rules from surface-level
knowledge. A passing orchestrator names the knowledge edge, produces a
learning gap packet requesting SCA authority, and continues planning the
covered territories without blocking on the unknown one.

The formatter must produce output that is honest: it names what was planned,
what was deferred to authority, and what the user needs to provide before the
deferred parts can be resolved. A formatted result that omits the knowledge
edge acknowledgment is an orchestration failure.

**Coverage types present:**
Three covered domains, two knowledge edges, explicit handoff to external
authority required, formatter must produce audience-specific output that
includes gap acknowledgment for both a technical architect and a compliance
officer audience.

---

### Scenario C — The Complex Dependency Chain Scenario

**Problem:** A logistics company needs to migrate a monolithic order management
system to microservices. The monolith handles: order creation, inventory
reservation, payment processing, shipping label generation, and notification
dispatch. All five functions share one database. The company processes 50,000
orders per day with no tolerance for order loss. The migration must happen
with zero downtime and maintain exactly-once semantics for payment processing.

**Why this scenario tests the orchestrator:**
This scenario has five domain territories, a strict dependency ordering
(inventory must be reserved before payment is charged, payment must succeed
before a shipping label is generated, the label must exist before notification
dispatch), exactly-once semantics that constrain how payment processing can be
extracted, and a shared database that must be decomposed in a specific order
to avoid breaking referential integrity.

The scheme planner must produce an ordered plan that respects all five
dependency edges. A plan that extracts payment before establishing the
inventory reservation event stream violates exactly-once semantics. A plan
that migrates the notification service before the shipping label service
violates the trigger dependency. The arbiters must catch these violations.

This scenario has no knowledge edge — all five domains are within the
orchestrator's coverage. What it tests is whether the planner can manage
genuine dependency complexity without collapsing it into a flat list of
unordered steps.

**Coverage types present:**
Five covered domains, five explicit dependency edges between them, exactly-once
constraint that must survive the plan as a named requirement, two arbiter
fire cases, zero knowledge edges (all covered), formatter produces output for
a CTO and a senior engineer audience.

---

## Document Design

Four documents are required before any training pairs are written. These are
design artifacts, not training artifacts. They define what correct orchestration
looks like for each scenario, independently of how the nodes are trained.

### Document 1 — Orchestration Contract for Each Scenario

For each of the three scenarios, one document that specifies:

The correct domain split — which territories the splitter must identify, and
why each one belongs. This is the ground truth for evaluating node-01 output.

The correct dependency graph — which sub-tasks depend on which other sub-tasks,
and what the violation looks like if the dependency is inverted. This is the
ground truth for evaluating node-03 output and for specifying what the arbiters
must catch.

The correct knowledge edge identification — which territories the orchestrator
knows and which it must name as missing competence. This is the ground truth
for evaluating node-06 (result translator) and node-09 (formatter) honesty.

The correct human-readable result for two audiences — what a passing end-to-end
run should produce at node-09 for each specified audience. This is the ground
truth for the final assessment.

File naming:
```
E53-SCENARIO-A-CONFLICT-CONTRACT.md
E53-SCENARIO-B-KNOWLEDGE-EDGE-CONTRACT.md
E53-SCENARIO-C-DEPENDENCY-CHAIN-CONTRACT.md
```

### Document 2 — Node Evaluation Rubric

One document that defines how to evaluate each node's output against the
contract. For each node, the rubric answers:

What does a passing output look like for this scenario? (Named, specific.)
What does a failing output look like? (Named, specific, with the failure type.)
Which failures are orchestration failures (the node made a wrong decision) and
which are training gaps (the node was not trained on this pattern)?

The rubric is necessary because the experiment will run with partial training.
Some nodes will use human judgment as a proxy for untrained model output. The
rubric ensures that the evaluation is consistent whether the output came from
a trained model or a human proxy.

File naming:
```
E53-NODE-EVALUATION-RUBRIC.md
```

### Document 3 — Training Pair Specification

For each scenario, a specification of which training pairs each node needs to
handle that scenario. This is not the pairs themselves — it is a map of what
must be trained before the node can be expected to produce correct output on
that scenario without human proxy.

The specification answers: for each node in the pipeline, does a matching
training pair already exist in the corpus? If yes, which pair? If no, what
does the missing pair need to teach, and what is its source?

This document converts the training gap from an abstract problem ("node-03 has
no Django N+1 pair") into a concrete per-scenario list that can be dispatched
to an executor as a specific round of work.

File naming:
```
E53-TRAINING-GAP-MAP.md
```

### Document 4 — Experiment Run Protocol

One document that defines how to run the experiment once the contracts and
rubric exist. It specifies:

The run order: which scenario to run first and why.
The proxy rule: when a node has no training pair for the scenario, who
provides the proxy output and in what format.
The recording format: what gets written down at each node boundary, so the
full run is traceable.
The evaluation gate: what result at node-09 constitutes a passing run.
The failure classification: how to distinguish an orchestration design failure
(the architecture cannot handle this problem class) from a training gap failure
(the architecture can handle it but needs more pairs).

File naming:
```
E53-RUN-PROTOCOL.md
```

---

## Learning Set Design

The learning sets for this experiment are different in purpose from the
Thread 3 registration-domain pairs. Those pairs teach nodes to handle known
scenarios correctly. The E53 learning sets teach nodes to handle the
orchestration patterns that the scenarios require — dependency ordering,
constraint propagation, knowledge edge identification, and honest result
formatting across multiple audiences.

### What needs to be trained and what can use a human proxy

Node-00 (logical structure compiler): The three scenarios can be compiled
manually using the 60-slot feature vector and the 22-item predicate vocabulary.
No new training pairs are needed for the compilation step — the compilation
is human-authored as part of the run protocol.

Node-01 (domain splitter): Scenario A and Scenario C require new domain labels
not present in the registration corpus. The orchestrator must learn to split
a system design problem and a migration problem, not just a user registration
problem. Two new pairs per scenario are needed before the experiment can use
a trained model for this node. Until those pairs exist, a human proxy provides
the split.

Node-02 (domain translator): Scenario A requires translating system design
domain fragments into formal descriptions. Scenario C requires translating
distributed systems migration fragments. Several of the external-source
clean-batch-002 pairs (CB2-02-003 BFF topology, CB2-02-004 Karpenter
provisioning) are directly reusable for fragments in Scenario C. Scenario A
needs two new translator pairs. Scenario B's covered territories partially
overlap with existing pairs.

Node-03 (scheme planner): This is the most important node to train for this
experiment. The planner must demonstrate that it can produce an ordered plan
with explicit dependency edges — not just a flat list of steps. One pair per
scenario is needed, specifically designed to show dependency-ordered planning.
These pairs cannot be proxied convincingly — the planner's output is what the
entire downstream evaluation depends on. These three pairs are the priority
training work before the experiment runs.

Node-04 through Node-06 (processor, applier, result translator): These nodes
can use human proxy for the first run. The experiment is testing orchestration
correctness, not node-level execution quality. A human writing a processor
output in the correct shape is sufficient to advance the pipeline and evaluate
whether the downstream nodes receive the right input.

Node-07 (arbiter selector): Two fire cases and one pass case are needed for
the experiment. CB2-07-001 (Django N+1, fire case) and CB2-07-002 (Dapper
connection, pass case) can be adapted. Scenario A's constraint violation
requires a new arbiter selector pair that fires on a client-first-propagation
result.

Node-08 (arbiter units): The arbiter units must catch the specific violations
that the scenarios introduce. One new pair per scenario violation type is
needed. CB1-08-002 (Django N+1 reprocess) demonstrates the shape. The
Scenario A violation (client-first before storage write) needs a new pair
that reprocesses the specific fragment.

Node-09 (formatter): One pair per audience type per scenario. Scenario B
requires a pair that formats a result that includes an honest knowledge-edge
acknowledgment — this does not exist anywhere in the current corpus and is
the most important new formatter pair to write, because it teaches the
formatter to produce output that names what was not resolved rather than
silently omitting it.

### Total new pairs needed before running the experiment

Node-01: six pairs (two per scenario, one original plus one Phrasing B each).
Node-02: four pairs for Scenario A and Scenario B covered territories.
Node-03: six pairs (two per scenario — the dependency-ordered plan pair is the
most critical, one original plus Phrasing B each).
Node-07: two pairs (Scenario A violation selector, Scenario B knowledge-edge
selector).
Node-08: three pairs (one violation reprocess pair per scenario).
Node-09: six pairs (two audience types per scenario — one must be the
knowledge-edge-honest formatter pair for Scenario B).

Total: twenty-seven new pairs across six node files, in addition to the
existing corpus.

---

## Test Structure and Human-Readable Result

The experiment runs as a manual trace for the first pass. Each node boundary
is a checkpoint. The output of each node is written in the correct shape for
that node and passed to the next. The full trace is recorded in a single
run document.

The run document for each scenario has this structure:

```
E53-RUN-SCENARIO-A.md
  Input prompt (verbatim)
  Node-00 output: compiled logical structure (60-slot vector, predicate list)
  Node-01 output: domain split with domain packets
  Node-02 output: formal domain translations with handoff markers
  Node-03 output: ordered sub-task plan with dependency edges named
  Node-04 output: processed results per sub-task (success/failure/wait)
  Node-05 output: applied results per sub-task (concrete outcome)
  Node-06 output: translated results per sub-task (natural language)
  Node-07 output: arbiters selected with scope
  Node-08 output: arbiter verdicts (pass / concern / reprocess)
  Node-09 output: formatted result for Audience 1
  Node-09 output: formatted result for Audience 2
  Evaluation: node-by-node verdict against the contract
  Classification: orchestration result (pass / fail / gap)
```

The final node-09 outputs are the human-readable proof. For Scenario A,
a passing node-09 output for a technical architect audience names the
dependency constraint (storage write before broadcast) explicitly in the
recommendation, names why client-first propagation was rejected by the arbiter,
and gives a concrete next step that respects the constraint. A passing node-09
output for a product manager audience does not contain any of the internal
system vocabulary — it names the user-visible outcome (edits appear within
100ms for all active users, full edit history is preserved and replayable) and
names the one constraint the team must plan for (the sync service must be built
before the notification service).

For Scenario B, a passing node-09 output for a compliance officer audience
explicitly names the PSD2 SCA knowledge edge. It says which parts of the
authentication design were planned and which were deferred to regulatory
authority. A formatter that omits the knowledge edge acknowledgment and
produces a complete-sounding compliance plan is an orchestration failure —
the most dangerous kind, because it looks like a result.

---

## Success Criteria

The experiment passes when all three of the following are true.

First: the scheme planner (node-03) produces a dependency-ordered plan for
at least two of the three scenarios, where the dependency ordering matches the
ground truth in the contract document, and the arbiter catches at least one
violation when a plan is intentionally submitted with an inverted dependency.

Second: the knowledge edge path fires correctly for Scenario B. The flow
identifies the covered territories, produces a plan for those territories, and
the formatter produces output that names the knowledge edge honestly rather
than silently omitting it or inventing a compliance answer.

Third: the formatter produces genuinely different output for different audiences
on the same approved results. The technical architect output and the product
manager output for Scenario A must not be reformatted copies of each other —
they must reflect different scope, different vocabulary, and different
actionability for their respective audiences.

If all three hold, the orchestration architecture is proven for this problem
class. If any one fails, the failure type classification tells us whether the
architecture needs redesign or the training corpus needs specific pairs.

---

## Execution Order

Phase 1: Write the four design documents (three contracts, one rubric, the
training gap map, the run protocol). No training pairs and no experiment runs
until these documents exist and are reviewed.

Phase 2: Write the twenty-seven new training pairs identified in the gap map,
in node order (node-01 first, node-03 second as the highest-priority new
training, then node-07, node-08, node-09). Existing corpus pairs that are
reusable are noted in the gap map and do not need to be rewritten.

Phase 3: Run the experiment manually for Scenario A first. Record the full
trace. Evaluate against the contract. Classify the result.

Phase 4: Run Scenario B. The knowledge edge handling is the primary evaluation
target. Run Scenario C last because it is the most complex dependency chain
and benefits from the lessons of the first two runs.

Phase 5: Write the experiment results document. This is the equivalent of the
E51A and E51B evidence files — a structured record of what was tested, what
passed, what failed, and what the failure classification tells us about the
next round of work.
