# Git publication contract — every task, not every slice

This contract applies to every S04-S10 task/unit and implements the project Golden Rule.

## Canonical branch source of truth

Do **not** invent per-slice branches. The current live `AGENTS.md` / `CLAUDE.md` define the canonical branch. For `llm_mvp_core` it is currently `codex/dpo-implementation` unless Luba explicitly changes it.

Every task must freshly verify the current branch instead of assuming a historical plan label.

## Pre-task branch reconciliation

Before each task:

- fetch/prune origin;
- record local HEAD, remote current-branch HEAD, and `origin/main`;
- require both `origin/main` and `origin/<current-branch>` to be ancestors of HEAD;
- if not, merge/reconcile before task work;
- require zero unmerged/conflicted paths.

No task begins on a diverged/conflicted branch.

## Per-task publication boundary

Every accomplished task—code, plan, evidence, verification, recovery—must have its own task-scoped durable commit. A no-code task commits its work/evidence/state delta.

After required validation and review, and **before the next task**:

1. fetch/prune again;
2. reconcile remote current branch and current main;
3. resolve conflicts completely;
4. prove both ancestry checks;
5. validate clean main merge/merge-tree compatibility where available;
6. commit task-owned reviewed changes;
7. push normally;
8. fetch/read back remote;
9. prove remote contains the task commit;
10. report task, tests/review, conflict result, local SHA, remote SHA, next task.

No later task starts while the current task is local-only, unpushed, not remotely read back, conflict-bearing, or missing its human report.

Reviewer PASS is not task completion. The manager owns the commit/push/reconciliation boundary after review.

Push/merge failure remains part of the current task and must be resolved before any subsequent task starts. Force push is not a main-compatibility repair.
