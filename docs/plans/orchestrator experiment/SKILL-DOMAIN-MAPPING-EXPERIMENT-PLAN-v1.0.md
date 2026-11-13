# Skill-Domain Mapping and Arbiter Binding Experiment Plan v1.0

Date: 2026-05-04
Status: Awaiting approval before any executor session begins

---

## Round contract

Test baseline: 985/985 passing. Cannot run dotnet test in this session.
985 is the floor. No executor session may produce a result below 985.

Produced: this plan defines one experiment that does not yet have a number.
The number is assigned after the plan is approved.

Gap being closed: the skills factory contains 129 skills across 15 layers.
Each skill has a declared trigger condition — "when to load" — but no experiment
has ever verified that the trigger correctly maps the right skill to the right
task and domain. Skills are cited in flow documents as inert labels (behavioral
correction 8 in the behavioral corrections registry). The plan review FC battery
fires on plan structure but does not verify that the arbiters configured for a
plan match the domain the plan actually touches. The goal delivery arbiter runs
first and blocks on goal coverage, but nothing verifies that the skill loaded
to do the goal delivery check is the correct skill for this task type.

This experiment treats skill-to-task binding as a first-class testable claim
and produces evidence about whether the current trigger rules route the right
skills to the right domains.

---

## Purpose

The experiment must answer one question:

Given a task description and a domain context, does the skills factory route
the correct skills, and do the arbiters configured for that task correctly
bind to the decisions that matter in that domain — or does the system silently
load the wrong skills, configure arbiters that do not cover the real risk, and
pass plans that contain domain-specific violations?

The plan review skill set is the first domain to test because it is the most
self-referential: the skills that govern plan review are themselves subject to
the same routing and binding failures they are supposed to catch.

---

## Why the plan review skill set is the right first domain

The plan review skill set consists of skills that fire during any session
producing a plan: the plan review skill (twelve failure classes, three gate
approval), the goal delivery completeness skill (runs first, two inputs), the
arbiter panel design skill (eight roles plus scope isolation), the NDJSON
arbiter type requirements skill (nine minimum type conditions), the plan self-
validation skill (phase zero gate), and the skill advisor skill (which skills
to load for which session type).

These skills interact. The goal delivery skill runs before all others and blocks
if goals are unmapped. The arbiter panel skill governs which reviewers fire. The
plan self-validation skill checks for phantom skills and phantom arbiters before
execution begins. The FC battery detects count drift, path errors, stale numbers,
and cross-document propagation failures.

The failure modes already documented in the behavioral corrections registry show
what happens when this set misfires:

Behavioral correction 8 shows that a session can execute an entire flow-specific
protocol citing the plan review skill by name, without that skill being loaded or
active. Custom arbiters (named F1 through F4 in the corpus example) replace the
full FC battery, and plans pass with DNA violations that the real FC battery
would have caught.

Behavioral correction 10 shows that the FC battery has a scope gap: it was
designed for code generation plans and does not apply its DNA compliance check
to remediation plans, allowing solution patterns to pass without verification
against the coding rules.

These two documented failures define exactly what this experiment must detect
and prevent.

---

## Experiment scenario

Scenario identifier: skill-mapping-scenario-001-plan-review-domain

Task description given to the flow:

A session is asked to review a plan that proposes adding a new registration
flow with three phases: Phase A creates a new service file in the engine,
Phase B generates arbiter configuration for that service, and Phase C runs
the portability test. The plan references the plan review skill, the goal
delivery completeness skill, and the arbiter panel design skill by name in
its load order. The session must determine whether the correct skills are
loaded, whether the arbiters configured for the plan cover the real domain
risks, and whether the plan would pass or be correctly blocked.

Why this scenario is hard enough:

It tests whether skill references in a plan are active prerequisites or inert
citations. It tests whether the arbiter panel for a plan review domain matches
the actual risk categories of the plan being reviewed. It tests whether the
goal delivery check fires first and with the correct two-input isolation. It
tests whether the NDJSON arbiter type requirements are checked when arbiter
configuration is part of the plan output.

---

## What the experiment must prove

Claim 1 — Skill routing correctness: given a task description naming a domain,
the trigger rules in the skill activation table produce a skill load list that
contains every skill required for that domain and no phantom skill that does not
apply.

Claim 2 — Arbiter binding correctness: the arbiters configured for the plan
review domain cover the actual risk categories in that domain. For the plan
review skill set, the minimum required arbiters are: goal delivery (runs first,
two-input isolation), scope isolation (runs alongside goal delivery), business
logic (plan structure), principles compliance (eight gate questions), and the
NDJSON type coverage check (when arbiter configuration is in scope).

Claim 3 — Active prerequisite enforcement: a skill cited by name in a flow
document is loaded and active before the document is processed, not treated as
an inert label. The experiment produces evidence of whether this holds.

Claim 4 — Scope gap detection: the FC battery correctly identifies when it is
being applied to a task type outside its designed scope (remediation plans vs
generation plans) and routes to the appropriate check variant.

Claim 5 — Arbiter authority preservation: a plan that passes all custom arbiters
but contains a domain-specific violation is correctly rejected when the real FC
battery runs. The experiment verifies the authority separation.

---

## Required documents before any experiment code

### Document 1 — Task-domain mapping table

File: SKILL-MAPPING-TASK-DOMAIN-TABLE-v1.md

Content: a structured table with one row per task type recognized by the
skills factory. Columns: task type name, domain label, required skills from
the activation trigger table, minimum arbiter set for that domain, scope
constraints (generation vs remediation vs review vs materialization), and
the known gap from the behavioral corrections registry if one exists.

The plan review domain row is the primary row for this experiment. It must
name all six skills in the plan review set, the five required arbiters, and
the two documented scope gaps from behavioral corrections eight and ten.

Gate B:
```bash
grep -c "plan review\|goal delivery\|arbiter panel\|NDJSON\|plan self-validation\|skill advisor" \
  SKILL-MAPPING-TASK-DOMAIN-TABLE-v1.md
# Expected: 6 or more — one hit per plan review domain skill
```

### Document 2 — Expected binding trace

File: SKILL-MAPPING-EXPECTED-TRACE-v1.md

Content: the expected output of the skill routing and arbiter binding process
for the experiment scenario, written at the same level of concreteness as the
E51B expected trace. For every step in the scenario:

What the skill advisor should produce when given the task description — the
exact skill load list in load order.

What the goal delivery arbiter should receive — verbatim goal statement and
turn list only, nothing else (the two-input isolation rule).

What the arbiter panel should look like for this domain — eight roles named
with their scope, plus scope isolation, plus goal delivery running first.

What a plan that contains a phantom arbiter (one cited by name but not loaded)
should produce — the plan self-validation skill blocks at phase zero.

What a plan that replaces the FC battery with custom arbiters should produce —
behavioral correction eight detection fires, the general skill is loaded, the
plan is re-evaluated under the real FC battery.

Gate B:
```bash
grep -c "skill advisor\|goal delivery.*two.input\|phantom arbiter\|behavioral correction 8\|FC battery" \
  SKILL-MAPPING-EXPECTED-TRACE-v1.md
# Expected: 5 or more
```

### Document 3 — Arbiter binding checklist

File: SKILL-MAPPING-ARBITER-BINDING-CHECKLIST-v1.md

Content: a per-domain checklist that defines what correct arbiter binding looks
like for the plan review domain. Derived from the arbiter panel design skill
minimum panel table and the NDJSON type requirements skill minimum type matrix.

For the plan review domain, the checklist must verify:

Goal delivery arbiter: fires first, receives exactly two inputs (verbatim goal
statement and turn list), blocks all other arbiters if it returns a blocking
verdict, cannot receive plan details or implementation context.

Scope isolation arbiter: runs alongside goal delivery, not after it, not before
it — both are structural pre-checks.

Business logic arbiter: fires after goal delivery passes, receives the plan
structure, verifies the phase map matches the deliverable block.

Principles compliance arbiter: fires on the eight gate questions from the
principles gate, cannot be satisfied by a partial answer.

NDJSON type requirements arbiter: fires when arbiter configuration is part of
the plan output, checks the nine minimum type conditions, blocks if scope
isolation count is zero.

Gate B:
```bash
grep -c "fires first\|two inputs\|alongside goal delivery\|eight gate\|scope isolation count" \
  SKILL-MAPPING-ARBITER-BINDING-CHECKLIST-v1.md
# Expected: 5 or more
```

### Document 4 — Evidence schema

File: SKILL-MAPPING-EVIDENCE-SCHEMA-v1.md

Content: defines exactly what the experiment must write as evidence. Fields
required in the evidence JSON:

- experiment identifier
- scenario identifier
- task description verbatim
- skill advisor output: the load list produced for this task and domain
- expected load list from the task-domain mapping table
- match result: which skills are correctly routed, which are missing, which
  are phantom
- arbiter panel produced: the named arbiters with their scopes
- expected arbiter panel from the binding checklist
- binding result: which arbiters are correctly bound, which are missing, which
  are configured with wrong scope or wrong input isolation
- goal delivery input audit: what the goal delivery arbiter actually received
  (must be exactly two items)
- FC battery application: was the real FC battery applied or a custom subset
- scope gap detection: did the system correctly identify generation vs
  remediation vs review scope
- behavioral correction coverage: which documented failure modes were tested
  and whether they were correctly caught
- overall verdict: correctly mapped, partially mapped (with named gaps), or
  incorrectly mapped (with named violations)

Gate B:
```bash
grep -c "skill advisor output\|arbiter panel produced\|goal delivery input audit\|behavioral correction coverage\|overall verdict" \
  SKILL-MAPPING-EVIDENCE-SCHEMA-v1.md
# Expected: 5 or more
```

### Document 5 — Human result checklist

File: SKILL-MAPPING-HUMAN-RESULT-CHECKLIST-v1.md

Content: defines what a reviewer can determine from the evidence without
re-running the experiment.

A passing result must allow a reviewer to answer:

Which skills were correctly routed to this task and domain?
Which skills were missing from the load list?
Which arbiters were correctly bound to the domain risks?
Which arbiters had wrong scope or violated the two-input isolation rule?
Which documented failure modes from the behavioral corrections registry were
reproduced and correctly caught?
What is the minimum change required to close any gap found?

A failing result must name the specific gap, the specific arbiter or skill
involved, and the specific behavioral correction or FC check that would have
caught it if correctly applied.

Gate B:
```bash
grep -c "correctly routed\|missing from\|correctly bound\|behavioral corrections\|minimum change" \
  SKILL-MAPPING-HUMAN-RESULT-CHECKLIST-v1.md
# Expected: 5 or more
```

---

## Orchestration abilities under test

### Ability 1 — Trigger rule accuracy

The skill activation trigger table maps conditions to skills. For the plan
review domain, the correct trigger conditions are: any plan review session,
any session producing a plan, any ARCHITECT or REVIEW session. The experiment
verifies that these conditions route to the correct six-skill set and not to
a subset or a different domain's skills.

Metric: missing skill count equals zero. Phantom skill count equals zero.

### Ability 2 — Arbiter panel composition

The arbiter panel for the plan review domain must contain the correct roles
in the correct order with the correct input isolation. The goal delivery
arbiter must run first with two inputs only. The scope isolation arbiter must
run alongside goal delivery. Business logic, principles compliance, and NDJSON
type coverage must fire in the correct sequence after.

Metric: arbiter role count matches expected. Input isolation violations equal
zero. Order violations equal zero.

### Ability 3 — Active prerequisite enforcement

A skill cited by name in a document is loaded before the document is processed.
The experiment injects a scenario where the plan review skill is cited but not
loaded, and verifies whether the system detects and corrects this before
proceeding.

Metric: inert citation detection fires. Correct behavior: load the skill, then
re-process the document.

### Ability 4 — Scope gap identification

The FC battery has a documented scope gap for remediation plans (behavioral
correction ten). The experiment presents a remediation plan and verifies
whether the system correctly identifies that the standard FC battery scope
does not cover solution pattern DNA compliance, and routes to the extended
check.

Metric: scope gap detection fires on remediation plan input. Standard FC
battery is not applied as the sole gate.

### Ability 5 — Authority preservation under custom arbiter substitution

The experiment presents a scenario where custom arbiters (named F1 through F4)
have been substituted for the FC battery. The system must detect behavioral
correction eight, load the general skill, and re-evaluate the plan under the
real FC battery before accepting any verdict from the custom arbiters.

Metric: custom arbiter substitution detected. General skill loaded. FC battery
re-applied. Plan that passed F1 through F4 but contains a DNA violation is
correctly blocked.

---

## Pass gates

| Gate | Required result |
|---|---|
| Skill routing accuracy | Zero missing skills and zero phantom skills for plan review domain |
| Arbiter panel composition | All five required arbiters present in correct order with correct input isolation |
| Goal delivery isolation | Goal delivery arbiter receives exactly two inputs: verbatim goal and turn list |
| Active prerequisite enforcement | Inert citation detected and corrected before document processing continues |
| Scope gap detection | Remediation plan correctly identified as outside standard FC battery scope |
| Authority preservation | Custom arbiter substitution detected and FC battery re-applied |
| Evidence completeness | JSON and markdown evidence written with per-ability traces |
| Existing tests | Full suite remains at or above 985 passing |

---

## What this will tell us

If the experiment passes on all gates: the trigger rules correctly route the
plan review skill set, the arbiter bindings are correct for this domain, and
the documented failure modes from the behavioral corrections registry are
actively prevented.

If the trigger routing fails: the skill activation table has gaps in its
condition coverage for the plan review domain. The gap is named and the
activation trigger row that needs to be added or corrected is identified.

If arbiter binding fails: the arbiter panel design skill's minimum panel table
does not correctly specify the plan review domain. The specific arbiter with
wrong scope or wrong input isolation is named.

If active prerequisite enforcement fails: the infrastructure for loading skills
cited by name in flow documents does not exist and must be built before the
skill citations in any flow document can be trusted.

If scope gap detection fails: remediation plans are passing FC checks that
were not designed to evaluate them, and all prior remediation plan approvals
are potentially unreliable.

If authority preservation fails: the custom arbiter substitution pattern from
behavioral correction eight is still active in the current system and any plan
reviewed with custom arbiters may have passed with violations.

This experiment produces a failure diagnosis that is as targeted as the E53
failure diagnosis. It tells us which part of the skills factory needs repair,
not just that the factory has a problem.

---

## Execution sequence

### Round A — Documents

Produce all five documents named above. Stop for review. No experiment code
until all five documents are reviewed and accepted.

Source authorities the executor must read before writing any document:

- The behavioral corrections registry (behavioral corrections eight and ten)
- The plan review skill (twelve failure classes, three gate approval)
- The goal delivery completeness skill (two-input isolation, runs first rule)
- The arbiter panel design skill (minimum panel by archetype table)
- The NDJSON arbiter type requirements skill (nine minimum type conditions)
- The skill activation triggers table from the governance layer

### Round B — Deterministic experiment

Implement the routing and binding checks as deterministic functions that apply
the trigger rules, build the expected panel, compare against the produced panel,
and write the evidence files.

The experiment does not call live LLMs in round B. It applies the documented
rules deterministically and records where the rules produce the correct result
and where they produce a gap or violation.

Commands to run after round B:
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "SkillMapping" 2>&1 | tail -5
# Expected: all skill mapping tests pass

dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: 985 or more passing, 0 failing
```

Create round zip containing all five documents, experiment source file, test
file, evidence files, and round manifest.

Stop for review.

### Round C — Extend to a second domain

Only after round B passes, select a second domain from the skill activation
trigger table and run the same routing and binding verification. The second
domain is chosen based on the failure diagnosis from round B — whichever
domain is most likely to share the gaps found in the plan review domain.

Do not select the second domain before round B evidence is reviewed.

---

## What not to do

Do not treat this as a plan review of the plan review skill. This is an
experiment about whether the routing and binding machinery works correctly.
The plan review skill is the test subject, not the test tool.

Do not substitute custom arbiters for the FC battery during the experiment
itself. If the experiment detects that custom arbiters were substituted in the
scenario, it records that as a finding, not as a valid alternative review.

Do not accept a result where all five gates pass but the evidence does not name
what was tested and what the result was for each ability. Metric-only passes
without readable evidence are not accepted (same rule as E53).

Do not extend to the second domain in round C before round B evidence is
reviewed and the failure diagnosis is complete.

---

## Relationship to E53

This experiment and E53 run in parallel and are independent. E53 tests
orchestration ability on a complex multi-domain human problem. This experiment
tests whether the skills factory correctly routes and binds its own tools
before orchestration begins. Both are needed. E53 can fail because orchestration
breaks after correct skill loading. This experiment can fail because the skills
were never correctly loaded in the first place.

The dependency direction is: this experiment should complete before E53 round C
(replacing deterministic fixtures with learned units), because E53 round C
replaces node-00 or node-01 with an actual learned unit, and a learned unit's
routing decisions depend on the same skill activation machinery this experiment
is testing. If the machinery is broken, the round C replacement will produce
unreliable routing.

---

## Immediate next action

Round A begins with the executor reading the six source authorities listed
above, then producing the five documents in the order listed. The task-domain
mapping table is produced first because every other document references it.
The expected binding trace is produced second because the arbiter binding
checklist, evidence schema, and human result checklist all reference it.
