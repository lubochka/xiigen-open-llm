# Plugin Runtime Reality Gate Skill

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

Use this skill for Claude, Cursor, Codex, or any host extension/plugin plan,
review, or completion claim.

It prevents JSON, PowerShell, manifests, fixtures, reports, or static validators
from being accepted as plugin implementation.

## Real Implementation Requirements

Every host extension must name and implement:

- host repository;
- real host entrypoint;
- host manifest/package file required by that host;
- thin adapter that calls xiigen exe or approved `llm_mvp_core` API;
- request shape sent to XIIGen;
- response shape displayed to user;
- programmer debug setting;
- debug trace format:

```text
xiigen <method>:
input: <what was sent to xiigen>
output: <what was received from xiigen>
```

## Full Extension Definition Gate

A Claude, Cursor, or Codex artifact is not a full extension until that specific
host has evidence for every row below:

```text
host:
real_host_entrypoint:
settings_debug_surface:
target_locator_or_core_api_path_into_llm_mvp_core:
prompt_to_task_path:
task_to_orchestration_path:
task_or_plan_to_skills_path:
local_adaptive_learning_path:
export_import_backup_continue_training_evidence:
qa_live_host_or_explicitly_scoped_handler_evidence:
```

All core logic must remain owned by `llm_mvp_core`; the extension layer is a
thin host adapter that sends a concrete task and displays a concrete answer.
Static JSON, PowerShell, manifests, reports, validators, internal helper calls,
or dashboard-to-core calls are allowed only as narrower evidence. They do not
prove a full extension unless the real host entrypoint and the rows above are
also proven.

## Dual-Path Host Verification Rule

Двухконтурная проверка host-сценариев is mandatory for every
extension/runtime/product readiness scenario. Do not define verification as one
fragile `user in host -> extension -> core -> answer in host UI` chain. Require
both paths and join them honestly:

- Automated boundary path: adapter/API/core request-response, model logic,
  DPO/training, token measurement, quality scoring, backup/export/import/share/
  continue-training/rollback, checkpoint and locator gates.
- Local desktop path: an agent on the local computer runs the same scenario in
  the real installed Claude/Cursor/Codex or relevant host, shows a visible
  marker, captures screenshot/video/log evidence, and records the exact blocker
  if UI control is impossible.

JSON-only, adapter/core-only, test-only hook, process/window-list, or synthetic
helper UI evidence cannot replace the Local desktop path. Screenshot-only
evidence cannot replace Automated boundary proof. If Local desktop is blocked,
record the blocker, downgrade the claim, continue the Automated boundary path
and other hosts/scenarios, and do not create a stop gate. Reports must show
`phase.step`, Automated boundary status, Local desktop status, evidence,
remaining work, and downgraded claims. Local desktop work orders must say how to
launch/activate the real app, show the marker, capture screenshot/video/log, and
save evidence.

Downgrade, blocker, not proven, not ready, needs verification, no claim, or
status-only artifact is not completion. It automatically creates a next concrete
scenario work item unless all product requirements are already proven or Luba
explicitly stops. Product-readiness progress is measured by scenario counters,
not report existence:

```text
total_scenarios:
fully_joined_proven:
partial:
not_moved:
blocked_with_next_action:
next_concrete_work_items:
evidence_accounting_done:
product_requirement_proven:
```

Every host scenario should attempt dual-path proof where possible: automated/
core/API path plus local real-program path or closest honest local/test-only
adapter path. If direct host UI is impossible, the next action is a feasible
alternate proof path, not a terminal stop.

Russian wording examples:

```text
Сценарии: всего 12; полностью доказано 3; частично доказано 5; не сдвинуто 2; заблокировано с понятным следующим действием 2.
Сдвинулся сценарий Cursor prompt-to-task: real host entrypoint найден, automated boundary доказан, local desktop proof ещё не полный.
```

Parent Architect Subagent-Only Firewall runtime gate: host/runtime evidence work
does not let the parent become executor under architect-only or sub-agent-only
authority. Scoped governance/skill files must contain `parent local sidecar work
ban`, `read-only verification after a sub-agent packet returns`, `sub-agent
waiting is not a stop`, and `no local non-overlapping work escape`. Runtime
status that waits for a bounded sub-agent packet remains orchestration, not a
product execution stop.

Exact literals: `Parent Architect Subagent-Only Firewall`; `parent local sidecar work ban`; `read-only verification after a sub-agent packet returns`; `sub-agent waiting is not a stop`; `no local non-overlapping work escape`.

## Host-Specific Entry Evidence

| Host | Acceptable runtime path |
| --- | --- |
| Claude | real Claude plugin/host command or installed runtime entrypoint |
| Cursor | VS Code/Cursor extension host command, chat participant, panel, or command palette entry |
| Codex | Codex plugin/app runtime entrypoint, not only a helper script |

## Rejected Substitutes

- JSON report bundle instead of host code.
- PowerShell smoke script instead of plugin entrypoint.
- Static source scan instead of host invocation when runtime is claimed.
- Internal helper call without proving user-facing host entry.
- Manifest/package without adapter code.

## Gate

A phase may say:

- `NOT_IMPLEMENTED_YET`;
- `PASS_AS_STATIC_CONTRACT`;
- `PASS_AS_HOST_RUNTIME`;
- `PASS_AS_FULL_EXTENSION`;

It may not say `PASS_AS_HOST_RUNTIME` unless the host entrypoint was invoked and
the trace shows request -> XIIGen -> response.

It may not say `PASS_AS_FULL_EXTENSION` unless the Full Extension Definition
Gate passes for that host. For the three-extension target, Claude, Cursor, and
Codex must each pass separately; one host cannot substitute for another.
