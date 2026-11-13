# UML Simulation To Test Case Skill

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## Purpose

Use this skill for Luba-readable implementation plans that include UML, data
flow, algorithm branches, or extension/core interaction diagrams.

The goal is to turn every diagram connection and algorithm branch into a
testable scenario.

## Required Per-Connection Row

For every UML arrow:

```text
uml_connection_case:
  connection_id:
  from_component:
  to_component:
  data_sent:
  example_input:
  expected_output:
  failure_case:
  responsible_class:
  responsible_method:
  test_case_id:
  test_layer: unit | integration | e2e_host | qa_automation | negative
  evidence_path:
```

## Required Per-Branch Row

For every algorithm branch:

```text
algorithm_branch_case:
  branch_id:
  human_condition:
  input_example:
  expected_result:
  not_expected_result:
  responsible_class:
  responsible_method:
  test_case_id:
```

## Gate

- A diagram with boxes and arrows but no simulation table is incomplete.
- A branch with no test row is incomplete.
- Part A and Part B must match: every Part A row maps to a Part B work order or
  test, and every Part B test maps back to a Part A branch.
