# Work Scope Inventory

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
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

## Core Problem

A session that improves 3 subprojects without knowing there are 12 has not made
pipeline-level progress. It has made local progress on an unknown fraction of the work.
This skill installs the denominator before any work begins.

---

## When to Invoke

- Before any session that touches multiple subprojects
- Before any pipeline-level improvement claim
- Before any STOP that reports a percentage reduction in errors or failures
- At session start for any session that spans more than one subproject

One run of this skill before work begins = no overclaimed pipeline-level
improvement claims.

---

## Section 1 — The Scope Tracking Record

Before any subproject is touched, the session produces a scope tracking record:

```json
{
  "scope": {
    "totalSubprojects": 12,
    "completeCount": 0,
    "partialCount": 0,
    "blockedCount": 0,
    "notStartedCount": 0,
    "thisSessionCovers": [],
    "denominator": "N of 12 subprojects (M blocked → N of (12-M) eligible)",
    "capturedAt": "session-start"
  }
}
```

**Rule:** A session without a scope tracking record may only claim improvement
at the individual subproject level for subprojects it fully tested in that session.

---

## Section 2 — The Count Protocol

Before the first file is touched, establish all counts.

### Count 1 — Total subproject count

From docs/SUBPROJECT-MAP.md:
```bash
grep -c "^| [0-9][0-9]\." docs/SUBPROJECT-MAP.md
# → total count of subprojects
```

Or for a specific pipeline (e.g., search pipeline):
"This session covers the search pipeline: IBm25Scorer, IVectorIndex, HybridRetriever (3 subprojects)."
The full project total remains the denominator for any project-level claim.

### Count 2 — Complete count

A subproject is complete when all its tests pass AND it has DNA compliance evidence.
```bash
# Count subprojects with all tests passing
dotnet test --no-build 2>&1 | grep "Passed:" | tail -1
# → not per-subproject, but total passes. Use filter for per-subproject:
dotnet test --filter "FullyQualifiedName~SubprojectName" --no-build 2>&1 | tail -3
```

### Count 3 — Partial count

A subproject is partial when it has been started but has failing tests or missing DNA checks.

### Count 4 — Blocked count

A subproject is blocked when a prerequisite (interface, dependency, or decision)
is not yet in place. Check prerequisite-chain-SKILL.md for the blocking chain.

### Count 5 — Not-started count

```
not_started = total - complete - partial - blocked
```

### Count 6 — The denominator

State explicitly what fraction of the project this session covers:
```
Denominator: [sessionSubprojectCount] of [total - blockedCount] eligible subprojects
 ([sessionSubprojectCount / (total - blockedCount) × 100]% of eligible)
```

---

## Section 3 — Scope Tracking Record Template

```markdown
## Work Scope Inventory (work-scope-inventory-SKILL)

Project total: [N] subprojects
Complete (all tests pass, DNA checked): [N] subprojects
Partial: [N] subprojects
Blocked (missing prerequisites): [N] subprojects — [list names]
Not started: [N] subprojects

This session covers: [list subprojects or pipeline name]
Session count: [N] of [total - blocked] eligible subprojects
Denominator: [N / (total - blocked) × 100]% of eligible

Claim authority:
  Project-level: NOT ESTABLISHED (session covers < 20% of eligible)
  OR
  Project-level: ESTABLISHED (session covers ≥ 20% of eligible)
  Subproject-level: ESTABLISHED for [list subprojects fully tested this session]
```

---

## Section 4 — Claim Authority Rules

| Session covers | Permitted claim |
|---|---|
| 1 subproject, fully tested | "SubprojectName improved" |
| N subprojects, all fully tested | "These N subprojects improved" |
| ≥ 20% of eligible, all fully tested | "Pipeline is improving" (with coverage-completeness-gate check) |
| < 20% of eligible | Subproject-level claims only — no pipeline-level claims |
| Any session without scope tracking record | No improvement claims at any level |

**"Fully tested" means:** all tests pass for that subproject + DNA compliance verified.

---

## Anti-Patterns

1. **"I'll establish scope after I see what there is"** — scope is established
   before work begins, not discovered during work.

2. **"I only need the denominator if I'm making a pipeline claim"** — establish
   it regardless. Sessions that don't plan to make pipeline claims often end up
   making them when work goes well.

3. **"I improved 3 of 12 subprojects — that's meaningful progress"** — it is
   meaningful at the subproject level. At the pipeline level: 3/12 = 25%,
   which meets the 20% threshold. State the denominator explicitly.

4. **"Partial progress counts as complete"** — complete means all tests pass
   with DNA compliance. Partial is partial.

---

## Integration

- coverage-completeness-gate-SKILL.md reads the scope tracking record denominator
  to verify claim scope before every STOP
- improvement-measurement-SKILL.md uses the scope tracking record to determine
  whether behavioral delta is required
- output-skepticism-SKILL.md Q2 (scope validity) checks the same thing: "does
  my evidence cover my scope?" If scope tracking record is absent, the Q2
  answer is "unknown"
