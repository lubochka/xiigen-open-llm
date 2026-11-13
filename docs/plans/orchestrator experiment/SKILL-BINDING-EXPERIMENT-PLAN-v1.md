# Skill Binding Validation Experiment
# Orchestration proof applied to a meta-domain: skill-to-task mapping correctness
# Status: DESIGN — awaiting approval before any documents are produced

---

## What This Experiment Proves

This experiment uses the dynamic flow orchestration architecture to address
a meta-level problem: given a planning task and a skill library, does the
orchestrator correctly identify which skills belong, which are misbound or
missing, and what the arbiters must catch when a skill is applied to the
wrong domain?

The subject matter is the XIIGen planning skill set. The orchestration
challenge is: a planning session has a declared task type, a set of skills
loaded for that session, and a set of arbiters evaluating the output. Are
the right skills loaded? Are the bindings between skill and task correct?
Are the arbiters capable of catching a skill that fires outside its domain?

This is not a test of whether individual skills work correctly. It is a test
of whether the orchestrator can reason about the mapping between skills,
tasks, and domains as a structured orchestration problem — the same way the onboarding orchestration experiment
reasoned about the mapping between registration requirements, domain
territories, and arbiters.

Stack independence applies here too. The experiment does not care whether
the underlying task is implemented in TypeScript, C#, or any other language.
The binding being tested is between a task type (planning, code-execution,
governance) and a skill domain (service boundary design, arbiter panel
design, goal delivery, reconnaissance gate). Those bindings hold regardless
of stack.

---

## The Core Problem Being Tested

The XIIGen skill library has 129 skills organized into 14 layers. Each skill
has a declared activation trigger — the condition under which it should fire.
The activation trigger table in the skill index defines when each skill loads.

Three failure modes exist in practice:

**Failure mode 1 — Wrong skill for the task (domain mismatch).** A session
loads a skill whose domain does not match the task type. For example, loading
the temporal behavior design skill (which governs time-delta state machines)
for a service boundary design task. The skill fires, produces output, and the
session uses it — but the skill was designed for a different problem class.
The output is not wrong in a detectable way; it is wrong in a structural way
that only becomes visible downstream when the plan fails a gate check.

**Failure mode 2 — Missing skill for the task (coverage gap).** A session
has a task that requires a skill that is not loaded. The session proceeds
without it. The missing skill's domain goes unchecked. The plan passes all
loaded arbiters but fails the gate check that the missing skill governs.
This is the equivalent of the knowledge-edge case in the onboarding orchestration experiment — the orchestrator
must name the missing competence rather than proceeding as if it is covered.

**Failure mode 3 — Correct skill, wrong binding (activation mismatch).** A
skill is present in the library and would be correct for the task, but its
activation trigger does not fire for this task's context. The skill is
available but not loaded. The session does not know to load it because the
trigger condition is ambiguous or the session's context does not match the
trigger's expected form.

This experiment tests whether the orchestrator can identify all three failure modes for
a given planning session, correctly map skills to task domains, and produce
arbiters that catch the binding violations.

---

## Why The Planning Skill Set Is The Right Domain For This Experiment

The planning skill set is self-referential in a useful way. These skills
govern how planning sessions run — which means they are the domain most
likely to be applied incorrectly in a planning session. A session that
incorrectly loads the service boundary design skill when it should load the
goal delivery skill is a real observed failure class in the behavioral
corrections registry.

The planning skill set also has rich documented structure. Each skill has a
named domain, a declared activation trigger, a layer assignment, an arbiter
integration note (where relevant), and a load order. This structure is
exactly the kind of formal input that the domain translator node (node-02)
and the scheme planner node (node-03) are designed to work with. The skill
index is effectively a domain vocabulary for the planning domain.

Finally, the meta-arbiter skill already exists in the planning corpus and
defines a four-step diagnosis protocol for skill binding failures. This experiment uses that protocol as the ground truth for what correct arbiter behavior looks
like when a binding violation is detected.

---

## The Three Scenarios

### Scenario P1 — The Domain Mismatch Scenario

**Planning task:** Design the service boundary for a new flow that handles
real-time bid processing in a multi-tenant auction platform. The flow
receives bid events, validates them against auction state, updates the
running maximum bid, and emits a winner-declared event when the auction
closes. The session has loaded the temporal behavior design skill and the
algorithm as service skill. It has not loaded the service boundary design
skill or the domain event design skill.

**What the orchestrator must find:**
The temporal behavior design skill governs time-delta state machines and
phase boundaries. Bid processing is time-sensitive but is not a state machine
with phase transitions — it is a stream-processing problem with a convergence
condition. Loading the temporal skill for this task is a domain mismatch.
The algorithm as service skill governs ranking, matching, and scoring
algorithms. Bid processing includes a comparison (is this bid higher than
current maximum?) but the dominant pattern is event sourcing, not algorithmic
ranking. The algorithm skill is a partial match at best.

The missing skills are the service boundary design skill (which governs how
to define the boundary between the bid validation service and the auction
state service) and the domain event design skill (which governs the naming,
payload, and consumer limit for winner-declared events).

**What the arbiters must catch:**
A plan produced with the temporal skill loaded but the boundary design skill
missing will define phase boundaries where it should define service contracts.
The arbiter must detect that the plan's output is in the vocabulary of the
wrong skill domain and reject it as a binding mismatch — not as a logic error.

**Why this is hard:**
The temporal behavior skill and the service boundary skill are both in the
planning layer and both produce plan documents. Their outputs look similar
in structure. The domain mismatch is not obvious from the plan's surface
form — it is visible only when the plan's concepts are checked against the
activation trigger of the skill that produced them.

---

### Scenario P2 — The Coverage Gap Scenario

**Planning task:** Review a plan for a user onboarding flow that has been
submitted for plan review. The plan covers: the flow contract, the task
types, the arbiter configuration, the quality gates, and the milestone
definition. The session has loaded the plan review skill (version 2.1). It
has not loaded the goal delivery completeness skill.

**What the orchestrator must find:**
The plan review skill version 2.1 runs 12 failure class checks. One of those
checks — the definition-of-done check — references the goal delivery
completeness skill as a required arbiter. The plan review skill's check
requires the goal delivery skill to be loaded and active before the
definition-of-done check can run. Without the goal delivery skill, the
definition-of-done check fires incompletely — it checks structural
completeness (is the definition of done present?) but not semantic
completeness (does the definition of done actually close the stated goal?).

This is the coverage gap pattern: the primary skill is loaded and fires, but
a dependency of one of its checks is missing, causing that check to produce
a partial result without signaling that it is partial.

**What the arbiters must catch:**
The arbiter must detect that the plan review's definition-of-done check
produced a structural pass without a semantic pass, and name the missing
dependency (the goal delivery completeness skill) as the reason. It must not
accept the structural pass as a complete pass.

**Why this is hard:**
The plan review skill fires and produces a verdict. The verdict looks
complete. The coverage gap is inside one of the 12 checks — a check that
partially fired rather than not firing at all. Detecting a partial fire
requires the arbiter to examine not just whether the check was run but
whether all the check's dependencies were active when it ran.

---

### Scenario P3 — The Activation Mismatch Scenario

**Planning task:** A new planning session opens for a MATERIALIZATION session
type. The session's opening message contains a session state document, a
handoff note, and a list of remaining items. The session loads the
reconnaissance gate skill (v2.5.0), the session mode declaration, and the
goal context persistence check. It does not load the materialization session
type skill.

**What the orchestrator must find:**
The materialization session type skill has an activation trigger: "Any
MATERIALIZATION session." The session is a MATERIALIZATION session — the
trigger condition is met. But the trigger did not fire because the session's
boot protocol loaded skills from the PLANNING skill set (which includes the
reconnaissance gate skill, session mode declaration, and goal context
persistence check) without recognizing that the MATERIALIZATION session type
requires an additional specialized skill.

The activation mismatch is not that the wrong skills were loaded. The loaded
skills are correct for the governance layer. The mismatch is that the
specialized session-type skill was not loaded on top of the governance skills,
because the trigger condition (session type = MATERIALIZATION) was in the
opening message but was not processed as a trigger event.

**What the arbiters must catch:**
The arbiter must detect that a MATERIALIZATION session proceeded without the
materialization session type skill, identify the trigger condition that should
have fired (session type declaration in opening message), and classify this
as an activation mismatch rather than a domain mismatch or coverage gap. The
distinction matters because the fix is different: activation mismatch requires
fixing the trigger detection, not the skill content.

**Why this is hard:**
The session appears to have correct governance skills loaded. The
reconnaissance gate, mode declaration, and goal context persistence check
are all present and firing correctly. The missing skill is not in the same
layer as the others — it is a specialized session-type skill that fires on
top of the governance layer. Detecting its absence requires knowing that
MATERIALIZATION sessions have a two-layer skill requirement: governance layer
plus session-type layer.

---

## Document Design

Five documents are required before any training pairs are written. These are
the oracle documents for this experiment, equivalent to the scenario contracts and rubric
in the onboarding orchestration experiment.

### Document 1 — Skill Domain Map

File: `SKILL-BINDING-DOMAIN-MAP.md`

This document maps every planning skill in the active skill index to its
primary domain, its activation trigger condition, its layer assignment, and
its arbiter integration point (if any). It is the reference that the domain
translator node uses to produce formal domain statements for skill-related
packets.

The map has this structure for each skill:
- Skill name (human-readable)
- Layer and load order
- Primary domain (the problem class this skill addresses)
- Activation trigger (the condition under which it should fire)
- Dependencies (other skills that must be active for this skill's checks
  to complete)
- Arbiter integration (which arbiters reference this skill, and how)

This document is the ground truth for evaluating whether the domain splitter
correctly identifies which skill domains are relevant to a planning task, and
whether the domain translator produces formal domain statements that correctly
represent the skill's scope.

### Document 2 — Binding Failure Taxonomy

File: `SKILL-BINDING-FAILURE-TAXONOMY.md`

This document defines the three binding failure types (domain mismatch,
coverage gap, activation mismatch) with concrete examples from the
behavioral corrections registry and the skill index gap table. For each
failure type, it defines:
- The detectable signal (what the arbiter can observe)
- The failure vocabulary (the terms the result translator uses to describe it)
- The resolution path (what must change to fix the binding)
- The distinction from the other two failure types

This document is the ground truth for evaluating whether the arbiters
correctly classify a binding violation and whether the result translator
uses the right vocabulary when describing it.

### Document 3 — Expected Orchestration Traces

File: `SKILL-BINDING-SCENARIO-EXPECTED-TRACES.md`

One section per scenario. For each scenario, the expected output at every
node boundary — the oracle that all gate checks run against. The node-03
scheme planner section is the most critical: it must show the correct
dependency graph between skill domains, the correct identification of missing
skills, and the correct parallel work (which domain checks can run
simultaneously and which must sequence).

For Scenario P2 specifically, the node-03 plan must show that the
definition-of-done check is a dependent step that requires the goal delivery
skill to be active — it must not show it as an independent step.

### Document 4 — Human Result Standards

File: `SKILL-BINDING-HUMAN-RESULT-STANDARDS.md`

Defines what a passing human-readable result looks like for each scenario.
For a skill binding experiment, the final result must:

- Name which skills were correctly bound (the covered territory)
- Name which skills were incorrectly bound and what domain they belong to
  instead (the mismatch territory)
- Name which skills were missing and which check they are required for
  (the gap territory)
- Name which trigger conditions were not detected (the activation territory)
- Not use any stack-specific vocabulary
- Not hide a binding violation by calling it a style preference
- Specify a concrete resolution for each identified violation

### Document 5 — Executor Contract

File: `SKILL-BINDING-EXECUTOR-ROUND-CONTRACT.md`

Same structure as the onboarding orchestration experiment executor contract: files to read first, files to
create, test names to add, evidence files to write, commands to run, pass
gates, what not to change, zip requirement.

---

## Learning Set Design

The learning sets for this experiment teach the orchestrator to reason about
skill-to-task bindings as a structured problem. The key difference from the onboarding orchestration experiment
and the Flow-01 training sets is the domain: the A inputs are planning
session contexts (task type, loaded skills, session state), and the B
outputs are binding assessments (which skills match, which are misbound,
which are missing).

### New pairs needed

**Node-01 (domain splitter):** Three pairs. One per scenario. Each A input
is a planning session context (task type, loaded skills, session mode). Each
B output is the set of domain packets: the skill domains present, the skill
domains missing, the trigger conditions that should have fired.

**Node-02 (domain translator):** Six pairs. Two per scenario. Each A input
is one domain packet from the splitter output. Each B output is the formal
domain statement for that skill domain, using the vocabulary of the skill
domain map. A mismatch packet translates differently from a gap packet — the
mismatch packet names the domain the loaded skill actually belongs to; the
gap packet names the domain that is uncovered.

**Node-03 (scheme planner):** Three pairs. One per scenario. Each A input
is the complete set of translated domain statements for the scenario. Each
B output is the binding assessment plan: which domains are checked first
(the independent domains), which depend on others (the dependent domains),
what the parallel groups are, and what the wait states are. For Scenario P2,
the plan must show the goal delivery skill as a required dependency of the
definition-of-done check.

**Node-08 (arbiter units):** Six pairs. Two per scenario. One fire case and
one pass case per scenario. The fire cases are the binding violations (domain
mismatch, coverage gap, activation mismatch). The pass cases are correct
bindings that the arbiter should not flag.

**Node-09 (formatter):** Three pairs. One per scenario, targeting a planning
session lead as the audience. The output must use the human result standards
defined in Document 4.

Total new pairs: twenty-one, across five node files.

Most of the existing corpus pairs do not transfer directly to this experiment because
the domain is different. However, the structural patterns transfer: the
knowledge-edge handling from the knowledge-edge scenario maps directly to the coverage
gap handling in Scenario P2, and the conflict handling from the
registration training set maps to the domain mismatch handling in Scenario P1.

---

## Pass Gates

| Gate | Required result |
|---|---|
| Stack independence | zero stack-specific terms in planning session inputs or results |
| Domain mismatch detection | temporal skill misbound to bid processing identified in Scenario P1 |
| Missing skill identification | goal delivery skill absence named in Scenario P2 definition-of-done check |
| Activation mismatch classification | materialization session type skill absence classified as trigger failure in Scenario P3 |
| Dependency preservation | node-03 plan shows goal delivery skill as a required dependency in Scenario P2 |
| Arbiter vocabulary | binding failure type correctly named (mismatch vs gap vs activation) in all three fire cases |
| Pass case correctness | arbiter does not flag correct bindings in the three pass cases |
| Human result completeness | final result names covered, misbound, missing, and trigger-gap territories |
| Evidence written | JSON, markdown, and Mermaid files produced for each scenario |
| Existing tests | full suite remains green |

---

## Failure Classification

If the experiment fails because the domain splitter collapses mismatch and gap packets:
The priority is node-01 training on the binding failure taxonomy before any
other node work.

If the experiment fails because the scheme planner treats all domain checks as
independent (no dependency structure):
The priority is node-03 dependency-ordering training — the same training
gap that already identified for the Django N+1 dependency chain.

If the experiment fails because the arbiter cannot distinguish mismatch from gap:
The binding failure taxonomy document needs more granular vocabulary and
the node-08 pairs need explicit negative examples showing why the distinction
matters.

If the experiment fails because the human result uses skill index vocabulary that a
planning session lead cannot understand:
The node-09 formatter pairs need audience calibration — the planning session
lead is a technical audience but not an LLM architecture audience.

---

## Execution Order

Phase 1: Produce the five design documents. Stop for review.

Phase 2: Produce the twenty-one new training pairs in node order (node-01,
node-02, node-03, node-08, node-09). Stop for review of node-03 before
writing node-08.

Phase 3: Run the experiment in deterministic mode for Scenario P1 (domain
mismatch — the most clearly defined failure type). Record the full trace.
Evaluate against the oracle.

Phase 4: Run Scenario P2 (coverage gap — the dependency detection test).
Run Scenario P3 (activation mismatch — the trigger detection test).

Phase 5: Write the evidence files and the experiment results document.

---

## What This Will Tell Us

If the experiment passes: the orchestrator can reason about skill-to-task binding
correctness as a structured domain. The skill library can be used as input
to the orchestrator — not just as a governance layer for the orchestrator's
sessions. This enables a new class of use: planning sessions that
self-audit their own skill bindings before producing output.

If the experiment fails because the skill domain vocabulary does not translate cleanly:
The skill domain map (Document 1) needs revision before the experiment can
run. The skill index activation triggers are not precise enough to serve as
formal domain inputs to node-02.

If the experiment fails because the orchestrator cannot distinguish the three failure
types: The binding failure taxonomy needs to be redesigned with clearer
discriminating criteria, and the node-08 pairs need more negative examples.

Either result advances the project. A passing a passing result proves that the
orchestrator generalizes to meta-domains. A failing result identifies exactly
which part of the skill binding problem the current architecture cannot
handle and why.
