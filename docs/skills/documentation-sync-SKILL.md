# Documentation Sync

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

## When to Sync

Sync documentation **before** committing, not after. The commit message lists changed files,
and doc files are changed files.

For the Smart Skills semantic-quality uplift work, Luba's current final-ready
condition is stricter than ordinary sync. Documentation fixes required by Luba
before a final ready, complete, done, or promoted report are part of the four
execution-review/documentation-fix cycles. They are not optional, not a
post-commit cleanup, and not satisfied by a promise to update docs later. Each
cycle must record the execution review, the documentation drift found or
explicitly ruled out, the documentation fix applied when drift exists, and the
recheck result before the final ready packet can pass.

---

## File → Canonical Docs Maintenance Map

| What Changed | Update These Docs |
|-------------|------------------|
| New `.cs` class or interface in `Src/` | `docs/SUBPROJECT-MAP.md` (status column) |
| New public interface (`I*.cs`) | `docs/COMPONENT-INTERFACES.md` (interface signature block) |
| New subproject (`.csproj` added) | `docs/SUBPROJECT-MAP.md` (add row), `docs/ARCHITECTURE.md` (add node), `CLAUDE.md` (Quick Reference if canonical) |
| Interface signature changed | `docs/COMPONENT-INTERFACES.md` (update signature), `docs/ARCHITECTURE.md` (if in diagram) |
| New algorithm or technique introduced | `docs/ARCHITECTURE.md` (relevant section) |
| Project-local wiki/documentation flow changed | `docs/SUBPROJECT-MAP.md`, `docs/ARCHITECTURE.md`, `CLAUDE.md`, `docs/HOW-TO-USE-SKILLS.md` |
| New dashboard, desktop UI, or API lab project added | `docs/projects/[project-doc].md`, `docs/projects/PROJECT-DOCUMENTATION-INDEX.md`, active plan/state evidence boundary |
| API method catalog or dashboard-exposed API contract changed | Project/API doc with human names, examples, parameters, outputs, and evidence boundaries for every exposed method |
| New xUnit test class or test file | `docs/SUBPROJECT-MAP.md` (test count column if tracked) |
| DNA rule applied for first time in new context | `docs/skills/dna-compliance-guard-SKILL.md` (examples section) |
| New experiment result recorded | `docs/EXPERIMENT-RESULTS-MAP.md` (E01..E50 entry) |
| Locked decision made | `docs/DECISIONS-LOCKED.md` (D-NN entry) |
| New skill added to `docs/skills/` | `docs/HOW-TO-USE-SKILLS.md` (index + activation trigger) |
| Phase plan completed | `docs/plans/[PLAN].md` (status update), `docs/STATE-P[N].json` (COMPLETE) |

---

## What "Synced" Means

A doc is synced when:
- The information in it matches what is currently true in the code
- There is no section describing a thing that no longer exists
- There is no missing section for a thing that now exists
- Version numbers / counts / lists are accurate

A doc is NOT synced by:
- Adding a placeholder "TODO: update this"
- Updating a different doc and assuming cross-references are fine
- Updating the doc in the next commit
- Adding tests or a QA report while leaving the human project/API document stale

For dashboard/API work, documentation must say exactly what the evidence proves:
view-model automation, WPF build, dashboard-to-core invocation, host runtime,
semantic quality, and external AI are separate evidence scopes.

For architecture, trainable, code-review, documentation, or audit artifacts,
sync also means the human-readable explanation is current. The documentation
must pass the Human-Readable Architecture Artifact Gate: operator-language summary,
PlantUML or Mermaid UML importable to draw.io, concrete simulations, no-code
algorithms, exact code quotes with adjacent concrete solutions, and Concrete
Repair Blueprint Gate for suspicious Dictionary/counter/KNN/scorer/selector/
private method repairs. A schema-only, table-only, or prompt/spec section is
not a solution.

---

## COMPONENT-INTERFACES.md Sync

When a new public interface is added or changed, add or update its block:

```markdown
### INewInterface

**Assembly:** `ProjectName` (Src/NN.ProjectName/)
**Purpose:** One sentence.

```csharp
public interface INewInterface
{
    Task<OperationResult<ReturnType>> MethodAsync(ParamType param);
}
```

**Implementations:**
- `ConcreteImpl` — Src/NN.ProjectName/ConcreteImpl.cs

**Used by:**
- `CallerClass` in `OtherProject`
```

---

## SUBPROJECT-MAP.md Sync

When a subproject status changes, update its row:

| # | Name | Status | Tests |
|---|------|--------|-------|
| NN | ProjectName | Active / Planned / Complete | X tests |

---

## HOW-TO-USE-SKILLS.md Sync

When a new skill file is added to `docs/skills/`, add it to the index:

```markdown
| skill-name-SKILL.md | When to use it | Activation trigger |
```

And add its activation trigger to the "When to Load Which Skill" table.

---

## Sync Checklist (Session End)

```
□ COMPONENT-INTERFACES.md: reflects all public interfaces added/changed this session
□ SUBPROJECT-MAP.md: reflects all projects and status changes
□ ARCHITECTURE.md: reflects structural changes (new subproject, new dependency edge)
□ CLAUDE.md: test baseline count updated if it changed
□ HOW-TO-USE-SKILLS.md: new skills added to index
□ Human-readable architecture audits: operator-language summary, draw.io UML, simulations,
  no-code algorithms, code quotes, concrete repair blueprint, tests/e2e/qa,
  sets/checkpoints/manifests, and lifecycle are synced
□ 42.ProjectWikiDomain: docs state whether the current flow has trained weights or only runner-missing graph readiness
□ STATE-Pn.json: phase state saved with deliverables and test count
□ Active STATE.json: after every state edit, JSON parse was rerun and final diff was checked
□ Dashboard/API docs: exposed API rows have human names, examples, parameters, outputs, and evidence boundaries
□ DECISIONS-LOCKED.md: any locked decisions recorded
□ EXPERIMENT-RESULTS-MAP.md: any experiment results recorded
```

Not every session triggers every item. Skip inapplicable rows — but confirm they are inapplicable, don't assume.

---

When a change affects learned, trainable, semantic, adaptive, DPO, selector,
router, classifier, scorer, retrieval, KNN, dictionary, counter, or pattern
matching logic, documentation must also state:

- which allowed hierarchy level owns the capability;
- whether deterministic helpers are runtime authority, candidate generators, or
  internal baseline details;
- whether the semantic claim is full model, baseline, scaffold, eval-only, or
  too strong;
- what datasets/checkpoints/manifests are affected;
- what tests prove the claim scope.

## Anti-Patterns

1. **Post-commit doc update** — docs are part of the commit, not a follow-up
2. **Updating CLAUDE.md test count without running dotnet test** — the number must come from a fresh run
3. **"I'll sync docs next session"** — there is no next session for this doc; it rots
4. **Updating the interface in COMPONENT-INTERFACES.md without updating the implementation list** — partial sync is worse than no sync
