# Blast Radius Tagger

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

Categorizes every planned deliverable file before writing it. Files with higher
blast radius get additional validation gates. Prevents treating a test file and
a public interface as equivalent deliverables.

---

## When to Invoke

- At the start of any phase — tag every planned deliverable before writing
- At session end — verify PRODUCTION files have been properly validated
- In any session that mixes file types (tests + production code + docs + skills)

---

## Blast Radius Categories

| Tag | Definition | Examples | Additional Gates Required |
|-----|-----------|---------|--------------------------|
| TEST-ONLY | Only affects test suite | `Tests/**/*.cs`, test helper classes | Standard gates only (Rule 4) |
| GOVERNANCE | Affects session behavior | `docs/skills/*.md`, `CLAUDE.md`, `AGENTS.md` | Commit BEFORE code files in mixed sessions |
| PRODUCTION | Affects runtime algorithms | `Src/**/*.cs`, `*.csproj` | dotnet build + dotnet test after every change |
| INTERFACE | Changes a public contract | `Src/*/I*.cs` (interface files) | Blast radius assessment + all caller check |
| DOCS | Documentation update | `docs/*.md`, `docs/plans/*.md` | Verify accuracy against code state |

---

## Rules

1. **Tag first** — assign blast radius to every deliverable before writing any file
2. **INTERFACE** files require a blast radius assessment (blast-radius-SKILL.md) before any edit
3. **GOVERNANCE** files in mixed sessions MUST be committed separately BEFORE code files
4. **PRODUCTION** files must pass `dotnet build → 0 errors` before session is declared done
5. **Never mix** TEST-ONLY and INTERFACE in the same commit (different blast radii)
6. **DOCS** files go in the SAME commit as the code they describe (Rule 6)

---

## Tagging Example

Session: "Add SearchRange method to IVectorIndex and implement in BruteForceIndex"

| File | Tag | Extra Gates |
|------|-----|-------------|
| `Src/09.VectorSearch/IVectorIndex.cs` | INTERFACE | Blast radius assessment first — list all implementations and callers |
| `Src/09.VectorSearch/BruteForceIndex.cs` | PRODUCTION | dotnet build after edit |
| `Tests/VectorSearch/BruteForceIndexTests.cs` | TEST-ONLY | dotnet test after edit |
| `docs/COMPONENT-INTERFACES.md` | DOCS | Must match final interface signature |
| `docs/skills/blast-radius-SKILL.md` | GOVERNANCE | Can be in same commit as docs |

Commit sequence in this session:
1. GOVERNANCE changes (if any separate governance files) — separate commit if mixed
2. INTERFACE + PRODUCTION + TEST-ONLY + DOCS — one commit (Rule 6: one commit per phase)

---

## INTERFACE Validation Template

When writing to or changing a `I*.cs` interface file:

```bash
# 1. List all implementations before changing
grep -rn "class.*:.*IVectorIndex" Src/ --include="*.cs"
# → Must add the new method to EVERY implementation before committing

# 2. List all callers before changing
grep -rn "IVectorIndex\." Src/ Tests/ --include="*.cs"
# → Must verify callers still compile after the change

# 3. Build verification after change
dotnet build LlmFromScratch.sln
# → Must show 0 Error(s)

# 4. Rollback documentation (before editing, note how to revert)
git stash  # if needed to revert
```

---

## PRODUCTION Validation Template

When writing to `Src/**/*.cs`:

```bash
# After editing, before committing:
dotnet build LlmFromScratch.sln
# → 0 Error(s)

dotnet test --no-build 2>&1 | tail -5
# → Failed: 0

# Rule 3 check: every production line has a comment
# Manual review of changed file
```

---

## Anti-Patterns

1. **Session edits a public interface alongside 5 test files without tagging** — the interface edit gets no blast radius check
2. **Committing GOVERNANCE files together with code during a blocked code session** — skill files and code should be separable
3. **Tagging TEST-ONLY and then asserting on call counts** — test fragility not caught by blast radius tagging; also run test-failure-triage-SKILL.md
4. **Treating DOCS as a follow-up commit** — docs go in the same commit as the code they describe
