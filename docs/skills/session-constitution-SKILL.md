# Session Constitution — Supreme Law of Every Session

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

## What This Skill Does

Governs session start, session end, and the critique-response protocol.
Every session follows this protocol without exception.

---

## Session Start Protocol (Run Every Time)

Project-local override: this protocol is subordinate to AGENTS.md, CLAUDE.md,
Luba's current instruction, and the active plan/state. It cannot force a branch
switch, full build, full dotnet test, or user-facing stop when the active plan
names a different allowed verification and a known `next_action`.

```
Step 1 — Branch check
  git branch --show-current
  -> Record the current branch. Do not switch branches unless Luba explicitly asks for that branch action in the current task.

Step 2 — Clean build baseline
  dotnet build LlmFromScratch.sln
  -> Run only when the active plan Gate B or Luba's current instruction requires it.

Step 3 — Test baseline
  dotnet test --no-build 2>&1 | tail -10
  -> Run only when the active plan Gate B or Luba's current instruction requires it.

Step 4 — Read session plan
  Read docs/plans/[ACTIVE-PLAN].md completely
  Record one specific fact from the end of the file (Rule 5 proof of reading)

Step 5 — Read STATE file (if resuming)
  Read docs/STATE-P[N].json for current phase context
```

**Do not skip steps.** A build baseline from a previous conversation is not a baseline.

---

## Session End Protocol (Before Every Commit)

```
Step 1 — DNA check
  grep -rn "throw new" Src/ --include="*.cs"
  → Zero matches in business logic (Rule 1 — OperationResult<T> everywhere)
  grep -rn "new [A-Z][a-zA-Z]*Reader\|new [A-Z][a-zA-Z]*Writer\|new [A-Z][a-zA-Z]*Storage" Src/ --include="*.cs"
  → Zero direct instantiation of concrete fabric types (Rule 2 — Fabric First)

Step 2 — Rule 3 comment audit
  Review every changed file: every non-blank production line has a // comment
  Spot-check: open each changed file, scroll top to bottom

Step 3 — Test gate
  dotnet test --no-build 2>&1 | tail -10
  -> Run only when the active plan Gate B or Luba's current instruction requires it. Must show Failed: 0 when required.

Step 4 — Documentation sync
  See documentation-sync-SKILL.md for which docs update on which change types
  Update before committing, not after

Step 5 — STATE update
  Save docs/STATE-P[N].json with session deliverables, test count, next step

Step 6 — Commit
  git add [specific files — not "git add -A"]
  git commit -m "phase N: [description] — [list every changed file]"
  git push origin [current-branch]
```

For bounded sub-agents, Step 6 is forbidden unless the work order is
commit/push-only or explicitly grants commit/push for the exact file set.
`STATE.json next_action`, a passed gate, or a known next phase is not commit/push
authority. If a sub-agent returns changed files without commit/push authority,
the sub-agent stops after the evidence report and parent ARCHITECT_INTERNAL
owns review plus the next commit-boundary work order.

Artifact-producing, code-producing, documentation-producing, data-producing,
report-producing, plan-producing, and governance-producing sub-agent work
orders must not run Step 6 in the same work order. They return artifacts and
evidence only. Parent review performs the commit/push boundary after accepting
the packet, or launches a separate commit-only sub-agent work order with exact
file list and commit message.

When `commit_push_authorized=false`, git write/ref commands are forbidden as
commands, not only as outcomes. A bounded sub-agent must not run `git add`,
`git commit`, `git push`, `git checkout`, `git reset`, `git branch`, `git tag`,
`git merge`, `git rebase`, or any staging/ref-writing/history/branch/publish
command unless the work order explicitly grants that authority for the exact
file set. Read-only `git status`, `git diff`, `git log`, `git show`, and
`git rev-parse` are allowed only when listed in the work order.

If a sub-agent report claims it committed, pushed, staged, changed refs, or
emitted git directives without exact authority, parent review treats the packet
as a process violation even when the files are useful. The artifacts go through
quarantine validation; the agent is not delegated more write work in that phase
family until the parent records and repairs the loophole.

---

## Critique-Response Protocol

When a plan, approach, or implementation is in doubt:

```
Rule: Ask ONE precise question only when the active plan, state, local files, and document RAG cannot derive a safe next action. If the next action is known, continue instead of asking.
```

Wrong:
```
"Should I use approach A? Or B? Or maybe C? What about D? Which do you prefer?
And also should I change the interface? And one more thing..."
```

Right:
```
"The ICodeReader interface currently returns List<string> for ListFilesAsync.
Should it return IEnumerable<string> for lazy enumeration, or keep List<string>?"
```

A precise question has:
- One specific decision point
- The current state described
- The two concrete options
- Nothing else

**Wait for the answer before writing code only when this is a real blocker under AGENTS.md.**

---

## Forbidden Decisions (Escalate to Luba)

Never decide these unilaterally:

| Forbidden | Why |
|-----------|-----|
| Add a new NuGet dependency | Dependency policy is product scope |
| Change an existing public interface signature | Breaks all callers |
| Skip Rule 4 (zero failures gate) for any reason | No exceptions |
| Delete a file that was part of a prior phase | May be referenced |
| Change the test baseline count down | Regressions require approval |
| Create a new subproject (*.csproj) | Architecture decision |
| Change assembly references | Cross-project impact |

---

## Session Complexity Scaling

| Session Type | Required Start Steps | Required End Steps |
|-------------|---------------------|-------------------|
| Bug fix (single file) | Steps 1-3 | Steps 1, 3, 6 |
| Feature (single subproject) | Steps 1-4 | Steps 1-6 |
| Phase (multiple subprojects) | Steps 1-5 | Steps 1-6 + doc sync |
| Architecture (interface changes) | Steps 1-5 + read all affected files | Steps 1-6 + full doc sync |

Scale up, never down. When uncertain, use the more thorough protocol.

---

## Self-Verification Loop (Session End)

Before reporting session complete, run this checklist mentally:

```
□ Build baseline: recorded at session start?
□ Test count: same or higher than start?
□ Failed: 0 — verified with fresh dotnet test run?
□ Rule 1: no throw in business logic?
□ Rule 2: no direct fabric instantiation?
□ Rule 3: every production line commented?
□ Rule 5: read proof recorded for every touched file?
□ Gate B: every placement's Gate B explicitly met?
□ Docs synced: relevant canonical docs updated?
□ STATE saved: phase state written to STATE-Pn.json?
□ Commit message: lists every changed file?
```

**"I think I covered everything" is not a checklist result.**
Run through each checkbox with evidence.

---

## Skill Registry

Skills are loaded on demand. Load a skill when the session task activates it:

| Situation | Load Skill |
|-----------|-----------|
| Bug reported | bug-to-tests-SKILL.md |
| Before writing code | phase-preflight-SKILL.md |
| Before any phase | phase-planning-SKILL.md |
| Claiming completion | verification-before-completion-SKILL.md |
| Verifying own output | self-verification-SKILL.md |
| Session end docs | documentation-sync-SKILL.md |
| Root cause unknown | root-cause-tracing-SKILL.md |
| Generated code review | generated-code-review-SKILL.md |
| Context running low | context-overflow-SKILL.md |
| Tracker update | tracker-SKILL.md |
