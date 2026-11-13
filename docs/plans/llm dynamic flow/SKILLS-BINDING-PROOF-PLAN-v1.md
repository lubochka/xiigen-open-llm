# Skills Binding Proof Plan v1

Date: 2026-05-04

---

## Decision

This experiment tests the skills factory as an orchestration sub-system.

The question is:

Can the dynamic flow correctly identify which skills from the registry apply to a given task, validate that each selected skill is genuinely bound to its task component, and reject bindings that violate applicability rules — producing a human-readable skill plan that a stakeholder can act on?

This is not a repetition of E53. E53 proves that a complex problem can be orchestrated through the full node chain without losing edges. This experiment proves that the skill selection and binding validation layer — which decides WHAT skills to use at each node — works correctly, including when a skill is wrongly proposed, when a required skill is missing from the registry, and when a constraint (such as the injection budget) creates a conflict between selected skills.

---

## Why The Design-For-AI Skill Set Is The Right Domain

The design-for-ai cluster has five documented skills with explicit activation rules, input contracts, output contracts, and exclusion conditions:

- `design-for-ai-SKILL.md` — CHECKER and APPLIER modes, three explicit checks (intent unambiguous, constraints explicit, prior art provided)
- `ai-pipeline-design-SKILL.md` — 4-role structure, DPO triple format
- `ai-pipeline-topology-SKILL.md` — 9 node handler types, 6 wiring patterns (A through F)
- `skill-graph-sync-SKILL.md` — GovernsSeedEdge binding, 3-path drift protocol (>0.8, 0.5–0.8, <0.5)
- `skill-injection-SKILL.md` — 5 injectable blocks, max-3 constraint, priority ordering (DNA > FAB > PLAN > TEST > DOCS)

These five skills have known applicability rules, known exclusion conditions, and a documented constraint (the max-3 injection budget) that creates a real conflict when all five are selected simultaneously. That conflict is a first-class test case.

The domain is also stack-independent. None of the five skills require C#, .NET, SQL, cloud providers, or framework-specific steps. They describe how to design AI systems at the structural level. A binding plan produced by this experiment is readable by an architect or a product decision maker regardless of the implementation stack.

---

## What This Experiment Must Prove

1. The skill selector correctly identifies applicable skills for a given task from the registry.
2. Each binding is produced with an explicit contract: skill → task component, activation trigger, input shape, output shape, exclusion conditions.
3. A wrongly proposed binding is rejected by the binding arbiter with a named reason, not silently dropped.
4. A binding that violates the injection budget constraint is flagged as a dependency conflict, not accepted.
5. A skill needed by the task but absent from the registry produces a named skill coverage gap packet, not an invented local plan.
6. Binding arbiters operate on the formal binding records, not on the final human-readable plan.
7. The final output is a skill binding plan readable by a human decision maker, containing accepted bindings, rejected bindings, skill coverage gaps, and conflicts.
8. Evidence is readable without re-running the experiment.

---

## What This Experiment Must Not Prove

It does not need to prove that the skills themselves produce correct code.

It does not need to prove live skill injection into a generation call.

It does not need to prove that the DomainModelRegistry edges are correctly weighted.

It does not need a large task corpus. One hard scenario is sufficient for the first proof.

---

## The Binding Specification Format

Every binding produced by the skill selector must conform to this record shape.

This is the ground truth format for the experiment. Any binding that cannot be expressed in this format is not a valid binding.

```
Binding [ID]
Skill:               [skill file name without extension]
Task component:      [which part of the task this skill governs]
Activation trigger:  [the specific condition in the task that activates this skill]
Input contract:      [what the skill receives — predicate shape]
Output contract:     [what the skill produces — predicate shape]
Exclusion when:      [conditions under which this skill must NOT fire]
Binding arbiter:     [which arbiter validates this binding]
Status:              [ACCEPTED | REJECTED | CONFLICT | COVERAGE-GAP]
```

A binding arbiter verdict for each binding must also conform to a fixed shape:

```
Binding arbiter verdict [ID]
Binding:    [binding ID]
Arbiter:    [arbiter name]
Check:      [which applicability rule was evaluated]
Verdict:    [ACCEPTED | REJECTED]
Reason:     [named reason — references the skill's declared exclusion condition or applicability rule]
Reprocess:  [what must change if REJECTED — exact fragment, not general advice]
```

---

## Source Scenario

Scenario id: `SKILLS-SCENARIO-001-AI-CODE-REVIEW-PIPELINE`

Human task:

Design an AI-assisted code review pipeline. The pipeline takes code changes as input, identifies potential issues, explains each issue to the developer in terms they can understand and act on, learns from the reviewer's acceptance or rejection of each explanation, and injects the right skill context into each generation call so that explanations stay compliant with project rules. Identify which skills from the registry apply to each component of this pipeline. Define the binding between each skill and its task component. Flag any skill combinations that violate applicability rules or injection constraints.

Why this scenario is hard enough:

- It needs all five design-for-AI skills simultaneously.
- It produces an injection budget conflict: five skills are selected but the max-3 constraint limits injection to three at a time — the conflict must be surfaced, not silently resolved.
- It contains a deliberate wrong-binding challenge: applying `design-for-ai` to validate topology structure rather than to govern output readability (wrong use of the skill).
- It contains a skill coverage gap: the retrieval stage needs a skill for finding similar code patterns that does not exist in this skill cluster.
- The output must be stack-independent. The pipeline described must not reference a programming language, cloud provider, database, or framework.

---

## Node-By-Node Expected Behavior

### Node 00 — Logical Structure Compiler

A: The full task description.

B: Logical structure containing:
- source role: task decomposition request
- detected subject: AI pipeline design with skill binding
- domain cluster: design-for-AI skills
- predicate vector
- dependency edges between task components
- constraint markers (injection budget, exclusion conditions)
- coverage gap marker for the code pattern retrieval stage

Pass gate: The logical structure must contain all five task components (pipeline architecture, node handler selection, explanation output, learning loop, context injection) plus the budget constraint marker and the coverage gap marker. Any missing component fails the gate.

---

### Node 01 — Skill Selector (Domain Splitter role applied to skill registry)

A: Logical structure from Node 00.

B: Candidate binding set — one proposed binding per skill per task component where the activation trigger is satisfied.

The selector must propose:
- `ai-pipeline-design` → pipeline architecture component
- `ai-pipeline-topology` → node handler selection component
- `design-for-ai` → explanation output component
- `skill-graph-sync` → learning loop component
- `skill-injection` → context injection component

The selector must also propose (as the wrong-binding challenge):
- `design-for-ai` → topology validation component [WRONG — design-for-ai is for output readability, not structural validation; must be caught by the binding arbiter]

Pass gate: Five correct bindings proposed plus the wrong binding. Neither the wrong binding nor the coverage gap must be silently dropped at this stage. Both must appear as candidates for the arbiter to evaluate.

---

### Node 02 — Binding Translator (Domain Translator role applied to binding records)

A: One candidate binding at a time.

B: Formal binding record in the binding specification format, including activation trigger, input contract, output contract, and exclusion conditions drawn from the skill's declared rules.

Array contract: six candidates in, six formal binding records out. The wrong binding receives a complete formal record that the arbiter can evaluate — it is not filtered here.

Pass gate: Six binding records produced. Each record names the exclusion condition applicable to this binding, even if the binding will later be accepted. A record that omits the exclusion condition field fails.

---

### Node 03 — Binding Planner (Scheme Planner role applied to skill selection)

A: All six formal binding records.

B: The binding plan — an ordered set of binding groups with dependency edges, conflict markers, and coverage gap markers.

The plan must include:
- Group 1 (pipeline structure): `ai-pipeline-design` and `ai-pipeline-topology` — can run in parallel, both needed before explanation or learning components begin
- Group 2 (output and learning): `design-for-ai` (explanation output) and `skill-graph-sync` (learning loop) — can run in parallel, both depend on Group 1 being resolved
- Group 3 (injection): `skill-injection` — depends on all other groups because it governs what gets injected into each generation call; cannot be resolved until all other skills are selected
- Conflict marker: five skills selected exceeds the max-3 injection budget; a prioritization decision is required before Group 3 can resolve
- Wrong-binding marker: `design-for-ai` → topology validation is flagged as a candidate binding that requires arbiter review before inclusion
- Coverage gap: retrieval stage skill not found in registry — context pack required

Pass gate: The plan must show the dependency between groups, the conflict marker, and the coverage gap. A flat list with no dependency edges fails.

---

### Node 04 — Binding Processor (Domain Processor role applied to binding groups)

A: One binding group at a time.

B: Processed outcomes for each binding in the group — success outcome, conflict outcome, coverage-gap outcome, or review signal.

Group 1 outcomes:
- `ai-pipeline-design` → pipeline architecture: ACCEPTED, input contract satisfied, output contract produces pipeline topology description
- `ai-pipeline-topology` → node handler selection: ACCEPTED, input contract satisfied, output contract produces wiring pattern selection

Group 2 outcomes:
- `design-for-ai` → explanation output: ACCEPTED, APPLIER mode activated (new context package is being written, not audited), output produces context package conforming to INTENT/CONSTRAINTS/PRIOR-ART/SCOPE/GATE-B format
- `design-for-ai` → topology validation: REVIEW SIGNAL — activation trigger does not match; design-for-ai activates on "AI context package output readability" not on "topology structure correctness"; send to binding arbiter
- `skill-graph-sync` → learning loop: ACCEPTED, GovernsSeedEdge record required for each edge seeded from this binding, drift detection threshold 0.3 applies

Group 3 outcomes:
- `skill-injection` → context injection: CONFLICT — five skills selected, budget is three blocks; priority ordering yields SK-DNA, SK-FAB, SK-PLAN as the top three; remaining two (SK-TEST, SK-DOCS or equivalent skills in this domain) require explicit prioritization decision; cannot resolve without input

Coverage gap:
- Retrieval stage skill: COVERAGE-GAP — no matching skill in design-for-AI cluster; context pack created

Pass gate: Each binding produces an explicit outcome. No binding produces a null or silent result. The wrong binding produces a REVIEW SIGNAL that routes to the binding arbiter.

---

### Node 05 — Binding Applier

A: One processed binding plus its binding record.

B: The applied binding result — what the skill actually does for this task component, at the level of specificity the task requires.

For `design-for-ai` APPLIER mode, the applied result is the context package template filled in for the explanation output component:

```
INTENT: Produce per-issue explanations that a developer can understand and act on without additional context
CONSTRAINTS:
  Check 1: intent must be unambiguous — "explain why this line is unsafe" not "explain the issue"
  Check 2: constraints must be explicit — "must reference the rule violated and its source"
  Check 3: prior art must be provided — reference accepted explanations from the learning loop as examples
SCOPE OUT: style formatting, severity ranking, fix suggestion (governed by separate skills)
GATE B: explanation reviewed and accepted by developer without follow-up clarification questions
```

Pass gate: Applied results preserve rejection and conflict status from Node 04. The wrong `design-for-ai` → topology validation binding does not produce an applied result here — it was sent to the binding arbiter and has not been accepted.

---

### Convergence Gate

A: All applied binding results.

B: Convergence report naming:
- accepted bindings with their applied results
- the wrong binding pending arbiter verdict
- the injection budget conflict pending prioritization decision
- the retrieval stage coverage gap
- the dependency chain: Group 3 cannot close until the conflict is resolved

Pass gate: The convergence report must explicitly name all four states (accepted, pending-arbiter, conflict, coverage-gap). A report that lists only accepted bindings fails.

---

### Node 07 — Binding Arbiter Selector

A: Formal applied results, convergence report, pending arbiter routing signal.

B: Required binding arbiters and their scope assignments.

Required arbiters:
- `applicability-arbiter` → evaluate the wrong `design-for-ai` → topology validation binding
- `budget-constraint-arbiter` → evaluate the injection budget conflict (five skills, max-three limit)
- `coverage-gap-arbiter` → confirm the retrieval stage skill is genuinely absent, not mislabeled
- `stack-independence-arbiter` → confirm no binding in the accepted set requires a stack-specific term

Pass gate: The selector assigns arbiters from the formal binding records, not from the human-readable skill plan draft. The wrong binding must receive the `applicability-arbiter`, not be silently marked as rejected without review.

---

### Node 08 — Binding Arbiter Units

A: One formal binding record plus one arbiter role.

B: Verdict in the binding arbiter verdict format.

Expected verdicts:

`applicability-arbiter` on `design-for-ai` → topology validation:
```
Binding:  CB-WRONG-01
Arbiter:  applicability-arbiter
Check:    Activation trigger match — does the task component match the skill's declared activation scope?
Verdict:  REJECTED
Reason:   design-for-ai-SKILL declares two activation modes: CHECKER (audit existing AI context packages)
          and APPLIER (write new AI context packages). Topology structure validation is neither mode.
          The skill's declared scope is "AI readability and AI generation quality" — it governs
          how output is shaped for AI comprehension, not how pipeline topology is structurally correct.
          The applicable skill for topology validation is arbiter-panel-design or a topology-specific checker.
Reprocess: Replace this binding with a binding to the correct skill for topology structural validation.
           Do not apply design-for-ai to topology correctness in any future binding for this task.
```

`budget-constraint-arbiter` on five-skill injection conflict:
```
Binding:  CB-CONFLICT-01
Arbiter:  budget-constraint-arbiter
Check:    skill-injection-SKILL max-3 constraint satisfied?
Verdict:  REJECTED (as resolved — conflict surfaced, not silently discarded)
Reason:   Five skills selected. skill-injection-SKILL declares a hard limit of three blocks per
          generation call. Priority ordering (DNA > FAB > PLAN > TEST > DOCS) determines which
          three fire when all five are candidates. For this task, the three that must fire are:
          the pipeline design skill (highest structural impact), the topology skill (dependent on
          pipeline design resolving first), and the skill-graph-sync (governs the learning loop
          which is the novel component). design-for-ai and skill-injection itself are secondary
          and can be injected at later generation steps after the structural decisions are made.
Reprocess: Split injection into two phases. Phase 1: pipeline-design, topology, skill-graph-sync.
           Phase 2 (explanation generation calls): design-for-ai, skill-injection.
```

`coverage-gap-arbiter` on retrieval stage:
```
Binding:  CB-GAP-01
Arbiter:  coverage-gap-arbiter
Check:    Is the required skill genuinely absent from the design-for-AI cluster?
Verdict:  CONFIRMED ABSENT — skill coverage gap is real
Reason:   The design-for-AI skill cluster (five skills) does not contain a skill governing
          how to retrieve similar code patterns for comparison during review. This is a
          distinct competence from pipeline design, topology selection, output readability,
          learning loop management, or context injection.
          The system should not invent retrieval behavior by extending an existing skill.
          A named context pack must be created for this gap.
```

`stack-independence-arbiter` on full accepted binding set:
```
Binding:  CB-STACK-01 (all accepted bindings, scanned as a set)
Arbiter:  stack-independence-arbiter
Check:    Does any accepted binding require a stack-specific term?
Verdict:  ACCEPTED — no forbidden stack terms in accepted binding set
```

Pass gate:
- Wrong binding is REJECTED with a named replacement recommendation
- Budget conflict is surfaced and resolved into a two-phase injection plan
- Coverage gap is confirmed as real, not mislabeled
- Stack independence check passes on accepted bindings

---

### Node 06 — Result Translator

A: One accepted or rejected binding result at a time.

B: Human-readable binding description for that result.

Each description must preserve binding status. A rejected binding must not be softened into "consider whether this skill applies" — it must state "this binding was rejected because [named reason]."

---

### Node 09 — Formatter

A: All human-readable binding descriptions.

B: The skill binding plan — one document a human decision maker can read and act on.

Required sections:
1. **Accepted bindings** — what can proceed, which skills apply to which components, what each skill produces
2. **Rejected bindings** — what was proposed and why it was rejected, what the correct skill is
3. **Conflicts resolved** — the injection budget conflict and its two-phase resolution
4. **Skill coverage gaps** — what is missing, what the context pack contains, what must not be invented
5. **Next actions** — what the team must decide before the pipeline design can proceed

---

## Abilities Under Test

### Ability 1 — Skill Selection Completeness
All applicable skills are identified. No applicable skill is silently omitted.
Metric: `missing_applicable_skill_count = 0`

### Ability 2 — Binding Record Completeness
Every proposed binding produces a formal record with all required fields, including exclusion conditions.
Metric: `incomplete_binding_record_count = 0`

### Ability 3 — Wrong Binding Detection
A wrongly proposed binding reaches the binding arbiter and receives a named rejection verdict, not a silent drop.
Metric: `wrong_binding_silently_dropped = false`

### Ability 4 — Constraint Conflict Surfacing
The injection budget conflict (five skills, max-three limit) is surfaced and resolved into a two-phase plan, not ignored.
Metric: `budget_conflict_silently_ignored = false`

### Ability 5 — Coverage Gap Handling
The retrieval stage skill gap produces a named context pack. No local behavior is invented.
Metric: `invented_retrieval_behavior = false`

### Ability 6 — Arbiter Authority Over Skill Selection
The binding arbiter verdict is binding. A rejected binding does not appear in the accepted binding set even if the skill selector would have included it.
Metric: `rejected_binding_accepted_downstream = false`

### Ability 7 — Stack Independence
No accepted binding requires a stack-specific term.
Metric: `forbidden_stack_term_count = 0`

### Ability 8 — Human Result Quality
The skill binding plan contains all five required sections.
Metric: `human_result_sections_present = 5/5`

---

## Evidence Shape

The experiment must write three evidence files.

```text
experiments-v2/SKILLS-BINDING-PROOF/results/run-YYYY-MM-DD-###.json
experiments-v2/SKILLS-BINDING-PROOF/results/run-YYYY-MM-DD-###.md
experiments-v2/SKILLS-BINDING-PROOF/results/run-YYYY-MM-DD-###-binding-map.mmd
```

The JSON holds the machine-readable binding records, arbiter verdicts, and metrics.

The markdown holds the human-readable skill binding plan — the output a stakeholder can act on.

The Mermaid file holds the actual binding graph: nodes are skills and task components, edges are accepted or rejected bindings, dashed edges are coverage gaps.

---

## Evidence JSON Shape

```json
{
  "experiment_id": "SKILLS-BINDING-PROOF",
  "scenario_id": "SKILLS-SCENARIO-001-AI-CODE-REVIEW-PIPELINE",
  "stack_independent": true,
  "skill_registry_subset": [
    "design-for-ai-SKILL",
    "ai-pipeline-design-SKILL",
    "ai-pipeline-topology-SKILL",
    "skill-graph-sync-SKILL",
    "skill-injection-SKILL"
  ],
  "candidate_bindings": [],
  "formal_binding_records": [],
  "binding_plan": {},
  "convergence_report": {},
  "arbiter_selections": [],
  "arbiter_verdicts": [],
  "accepted_bindings": [],
  "rejected_bindings": [],
  "conflicts_resolved": [],
  "skill_coverage_gaps": [],
  "human_binding_plan": "",
  "metrics": {
    "missing_applicable_skill_count": 0,
    "incomplete_binding_record_count": 0,
    "wrong_binding_silently_dropped": false,
    "budget_conflict_silently_ignored": false,
    "invented_retrieval_behavior": false,
    "rejected_binding_accepted_downstream": false,
    "forbidden_stack_term_count": 0,
    "human_result_sections_present": "5/5",
    "skills_binding_proof_passed": true
  }
}
```

---

## Human Result Minimum Clarity Standard

The markdown evidence file must contain at least this level of answer:

```text
The AI-assisted code review pipeline maps to five skills from the design-for-AI cluster.

Accepted bindings:
- ai-pipeline-design governs the pipeline architecture decision. It receives the task requirements
  and produces a pipeline structure with the four roles (implementors, arbiters, context, manager).
- ai-pipeline-topology governs node handler selection and wiring. It selects from the six wiring
  patterns (A through F) and requires Pattern F when the pipeline must produce DPO training data
  from reviewer feedback.
- design-for-ai governs the explanation output component in APPLIER mode. It produces context
  packages in the INTENT/CONSTRAINTS/PRIOR-ART/SCOPE/GATE-B format for each review explanation
  generation call.
- skill-graph-sync governs the learning loop. It creates GovernsSeedEdge records for each
  feedback signal and uses the three-path drift protocol to resolve divergence.

Rejected binding:
- design-for-ai was proposed for topology validation but this binding was rejected. design-for-ai
  governs output readability for AI comprehension, not structural correctness of pipeline topology.
  The correct skill for topology structural validation is not present in the current cluster.

Conflicts resolved:
- Five skills were selected but the injection budget allows a maximum of three blocks per generation
  call. Resolved by two-phase injection: Phase 1 injects ai-pipeline-design, ai-pipeline-topology,
  and skill-graph-sync for architecture and learning decisions. Phase 2 injects design-for-ai and
  skill-injection for explanation generation calls.

Skill coverage gap:
- The retrieval stage — finding similar past code patterns to compare against the current change —
  has no covering skill in this cluster. A context pack has been created describing the required
  competence. No retrieval behavior should be invented locally.

Next actions:
- Confirm the wiring pattern (A through F) appropriate for this pipeline's DPO training requirement.
- Decide the Phase 1 and Phase 2 injection timing relative to the generation call sequence.
- Decide whether to request approved outside help for the retrieval stage skill gap.
```

---

## Pass Gates

| Gate | Required result |
|---|---|
| Skill selection completeness | All five applicable skills identified |
| Binding record completeness | All six candidate records (including wrong binding) fully formed |
| Wrong binding routing | Wrong binding reaches applicability-arbiter, not silently dropped |
| Arbiter rejection enforced | Rejected binding absent from accepted binding set |
| Budget conflict surfaced | Five-skill conflict resolved to two-phase plan, not ignored |
| Coverage gap handling | Retrieval stage produces context pack, not invented behavior |
| Stack independence | Zero forbidden stack terms in accepted bindings and human result |
| Human result quality | Five sections present: accepted, rejected, conflicts, gaps, next actions |
| Evidence completeness | JSON, markdown, and Mermaid binding map written |
| Existing tests | Full test suite remains green |

---

## Failure-Mode Diagnostics

If the wrong binding is silently dropped without an arbiter verdict:
The priority is the binding arbiter routing path, not the human result formatter.

If the budget conflict does not surface:
The priority is the constraint propagation layer in Node 04, which must emit CONFLICT not ACCEPTED when a constraint is violated.

If retrieval behavior is invented:
The priority is the skill coverage gap detector and the context pack output format — the system is generating locally rather than naming the gap.

If the human result has fewer than five sections:
The priority is Node 09 and Node 06. The formatter is receiving only accepted bindings and is not being given rejected, conflict, and gap material to format.

If the Mermaid binding map shows no rejected edges:
The evidence writer is not recording rejection verdicts into the graph. This is a provenance gap.

---

## Execution Sequence

### Round A — Documents and Oracle

Produce:
- `SKILLS-SCENARIO-001-SOURCE-PACKET.md` — the scenario with source constraints, explicit conflicts, unknown skill, forbidden assumptions, stack-independence rule
- `SKILLS-SCENARIO-001-EXPECTED-TRACE.md` — the oracle: expected output at every node, every binding record, every arbiter verdict, the conflict resolution, the coverage gap
- `SKILLS-BINDING-SPECIFICATION.md` — the canonical binding record format and the binding arbiter verdict format, with one completed example per field
- `SKILLS-EVIDENCE-SCHEMA.md` — the JSON, markdown, and Mermaid file shapes
- `SKILLS-HUMAN-RESULT-CHECKLIST.md` — the five required sections with examples of passing and failing content for each section
- `SKILLS-EXECUTOR-ROUND-CONTRACT.md` — files to read, files to create, test names, evidence files, commands, pass gates, what not to change

Stop for review after Round A.

### Round B — Deterministic Experiment

Implement with deterministic station functions. No live LLM calls. No live skill injection.

The deterministic fixtures represent the node outputs from the expected trace produced in Round A.

Rationale: the first proof is about binding logic and arbiter authority. Live LLM variance would obscure whether a failure is a binding logic problem or a model wording problem. After the deterministic proof passes, learned skill units replace fixtures one at a time.

Tests to add:
- `SKILLS_SourceScenario_IsStackIndependent`
- `SKILLS_SkillSelector_IdentifiesAllFiveApplicableSkills`
- `SKILLS_BindingRecords_AllFieldsPresent`
- `SKILLS_WrongBinding_RoutesToApplicabilityArbiter`
- `SKILLS_ApplicabilityArbiter_RejectsWrongBinding`
- `SKILLS_RejectedBinding_AbsentFromAcceptedSet`
- `SKILLS_BudgetConflict_SurfacesAsConflictNotAccepted`
- `SKILLS_BudgetConflict_ResolvesToTwoPhaseInjection`
- `SKILLS_CoverageGap_ProducesContextPackNotInventedBehavior`
- `SKILLS_HumanResult_ContainsFiveRequiredSections`
- `SKILLS_Evidence_WritesJsonMarkdownAndMermaid`

Command to run:
```powershell
dotnet test C:\Projects\llm_mvp_core\LlmFromScratch.sln --no-restore --filter "SKILLS_"
dotnet test C:\Projects\llm_mvp_core\LlmFromScratch.sln --no-restore
```

Create the round zip after both commands pass.

### Round C — Replace One Fixture With A Learned Unit

Only after Round B passes, replace one deterministic station with a learned unit.

Start with the skill selector (Node 01 equivalent) because the skill registry files already exist as the training material. Each skill file's "When to invoke" and activation trigger sections are the direct source for training pairs.

Do not replace multiple stations in one round. Run the same evidence comparison before and after.

---

## What Not To Do

Do not apply a skill to a task component it does not govern and accept the binding without arbiter review.

Do not silently drop the wrong binding before the applicability-arbiter has seen it.

Do not resolve the injection budget conflict by selecting three skills and discarding the others without surfacing the decision.

Do not invent retrieval behavior because no retrieval skill exists.

Do not let a high skill-to-task similarity score override an arbiter rejection.

Do not produce a human result that lists only accepted bindings and hides rejected bindings, budget conflicts, and coverage gaps.

Do not bind the scenario to a programming language, database, cloud provider, or framework.

Do not treat this experiment as a repeat of E53. E53 proves orchestration. This experiment proves skill mapping and binding validation. Both are required; neither replaces the other.

---

## Relationship To E53

E53 and the Skills Binding Proof run on the same architecture but probe different sub-systems.

E53 asks: when a complex problem enters the flow, does the orchestrator keep all edges alive, manage dependencies, handle knowledge edges, enforce arbiter authority, and produce a human-readable result?

The Skills Binding Proof asks: when skills are selected for a task, are the bindings correct, are wrong bindings rejected, are constraint conflicts surfaced, and does the output clearly communicate what was accepted, rejected, and missing?

The two experiments together cover the orchestration layer (E53) and the skill selection layer (this proof). After both pass, the next work is replacing deterministic fixtures with learned units — and at that point the training set work done in clean-batch-001 through clean-batch-003 becomes directly load-bearing.
