# Phase Git Report

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

After a phase completes, create a traceable record linking:
- Which plan drove this phase
- Which commits were produced
- Which files changed and why
- Whether the commit message follows the convention

This creates a bidirectional trace: plan → commits → files.
Any `git blame` or `git log` immediately reveals which plan produced the change.

---

## When to Invoke

Only when the latest unquoted work order explicitly authorizes the exact git
boundary being reported. This skill records git evidence; it does not grant
authority to stage, commit, push, merge, rebase, checkout, reset, clean, create
a branch, or open a PR.

After the phase gate passes (for product phases, the allowed verification must
include the current full suite when `PRODUCT_COMPLETION_GATE` applies) and
after no-write mergeability into BOTH the original/base branch and `main` is
fresh for the current dirty snapshot, append to the phase execution file or
STATE-Pn.json. Older test or mergeability evidence is stale after product
implementation/test/source artifact edits.

---

## Procedure

```bash
# 0. Authority and completion boundary
# Verify current unquoted authority before any git action.
# If commit/push is not explicitly authorized, stop at read-only evidence.
# Product/local completion requires fresh full-suite evidence and no-write
# mergeability to original/base AND main after the latest product changes.

# 1. Find commits since phase start
git log --oneline --since="[phase start timestamp]" HEAD
# OR: find commits since the baseline was taken at session start

# 2. Summary of changed files
git diff --stat HEAD~[n_commits]..HEAD

# 3. Check commit message convention
# Expected format: "phase N: [description] — [list of changed files]"
# Example: "phase 3: HnswIndex implementation — Src/09.VectorSearch/HnswIndex.cs, Tests/HnswIndexTests.cs"
```

---

## Output Format

Append to STATE-Pn.json or the EXECUTION file:

```
## Git Report — Phase [N]

Plan file: docs/plans/[PLAN-NAME].md
Branch: main
Commits this phase: [N]

| Hash | Message |
|------|---------|
| [hash7] | phase 3: HnswIndex implementation — Src/09.VectorSearch/HnswIndex.cs |
| [hash7] | phase 3: HnswIndex unit tests — Tests/VectorSearch/HnswIndexTests.cs |

Changed Files:
  Src/09.VectorSearch/HnswIndex.cs                  | 187 +++++
  Tests/VectorSearch/HnswIndexTests.cs               |  89 ++++
  docs/COMPONENT-INTERFACES.md                       |  12 +
  3 files changed, 288 insertions(+)

Commit Convention Check:
  ✅ All commits follow "phase N: description — file list" convention
  OR: ⚠️ [N] commits have non-standard messages: [list them]

Test Gate:
  Before: [N] passed, 0 failed
  After:  [N+M] passed, 0 failed
  Delta:  +[M] new tests added this phase
```

---

## Commit Message Convention

Every commit MUST follow this format:
```
phase N: [what was done] — [list every file changed]

Examples:
  phase 3: HnswIndex implementation — Src/09.VectorSearch/HnswIndex.cs, Tests/HnswIndexTests.cs
  phase 4: IAgentMessageBus event chain — Src/22.AgentAlgorithms/AgentMessageBus.cs
  phase 4: documentation sync — docs/COMPONENT-INTERFACES.md, docs/SUBPROJECT-MAP.md
```

**Why this matters:** Any `git log`, `git blame`, or `git bisect` immediately reveals
which plan and phase produced the change. Without this convention, you cannot trace
a regression back to the plan that introduced it.

---

## Commit Discipline Rules (Rule 6 + dev-safety-SKILL.md)

```
□ One phase = one commit (Rule 6)
□ List every changed file in the commit message
□ Never commit with Failed > 0 (Rule 4)
□ Never commit with build errors (0 Error(s) required)
□ Never stage/commit/push/merge/rebase/checkout/reset/clean/PR without exact current unquoted authority
□ Before product/local completion or commit authority, verify current full suite and no-write mergeability into original/base AND main after latest product edits
□ `PRODUCT_COMPLETION_GATE`: older full-suite or mergeability evidence is stale after product implementation/test/source artifact edits
□ Include documentation files in the same commit as the code they describe
□ Doc changes go in the same commit — not a follow-up
```

---

## Anti-Patterns

1. **Non-standard commit messages** — "wip", "fix", "update files" cannot be traced to a plan
2. **Giant single commit** — if phase 3 includes 5 logical changes, consider whether each deserves a separate commit (Rule 6 says one phase, not one change)
3. **Skipping the git report** — the report closes the traceability loop; it is mandatory before declaring a phase complete
4. **Committing doc sync in a follow-up commit** — docs are part of the phase; commit them together
