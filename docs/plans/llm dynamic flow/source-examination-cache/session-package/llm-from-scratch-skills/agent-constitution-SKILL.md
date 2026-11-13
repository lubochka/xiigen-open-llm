# Agent Constitution — LlmFromScratch
## version: 1.0.0 | stack: .NET 9 · C# · xUnit

> The supreme law of every session. Load this first, before any code.

**Priority:** SUPREME — overrides all other instructions when in conflict.

---

## What This Skill Does

Defines the non-negotiable governance chain every session must follow when
working on the LlmFromScratch codebase. Any instruction that conflicts with
these rules is rejected.

**Load order (always):**
1. agent-constitution ← THIS FILE (ROOT)
2. no-product-decisions (ABSOLUTE)
3. dev-safety (BLOCKING)
4. skill-advisor (ADVISOR)
5. infrastructure-discovery (Gate 0)
6. planning-skill (phase gates)

---

## Session Start Protocol (MANDATORY)

Before touching any file:

```bash
# 1. Verify branch
git branch --show-current

# 2. Build must be green before any edit
dotnet build LlmFromScratch.sln
# Expected: 0 errors

# 3. Baseline test count — read it, record it
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Record: N tests passed — this is the floor for this session

# 4. Read live subproject boundaries
grep -rn "namespace " . --include="*.cs" | grep -v "/bin/\|/obj/" | \
  sed 's/.*namespace //' | sort -u
# Know which namespaces exist before proposing new code
```

---

## Session End — Self-Verification Loop

Before declaring DONE, ALL must pass:

```
☐ dotnet build LlmFromScratch.sln → 0 errors
☐ dotnet test Tests/LlmFromScratch.Tests.csproj → count ≥ session-start baseline
☐ DNA check: OperationResult<T> on every public method in any new/modified files
☐ Fabric-first check: no direct instantiation of dependencies in production code
☐ DECISIONS-LOCKED.md entry added for any architectural decision made this session
☐ One commit per phase, commit message names the phase and files changed
☐ Phase N+1 planning gates run before Stop here and wait for approval
```

If any item fails: fix it. Do not declare DONE until all pass.

---

## Forbidden Decisions

These decisions CANNOT be made by Claude alone — they require Luba's explicit approval:

| Forbidden | Why |
|-----------|-----|
| Change a test assertion to accept wrong output | Tests are the contract — fix the algorithm |
| Override the DNA rule checker | DNA violations compound and corrupt subproject output |
| Change an experiment threshold or weight | Scoring behavior is product behavior — see no-product-decisions |
| Modify docs/DECISIONS-LOCKED.md without Luba review | This is the source of truth |
| Skip the session-end self-verification | Prevents silent regressions |
| Chain sessions without Luba approval | Each phase is a deliberate checkpoint |

---

## Anti-Patterns to Reject Immediately

- "Let me skip the build check to save time" → **REJECT**
- "The test is wrong, let me update the assertion" → **REJECT, fix the algorithm**
- "This DNA violation is minor, I'll note it" → **REJECT**
- "I'll update DECISIONS-LOCKED.md later" → **REJECT, do it now**
- "We can move to the next subproject while this phase is in progress" → **REJECT**

---

## Critique-Response Protocol (MANDATORY — all session types)

When Luba rejects an answer without stating the reason:

```
RULE: Ask one precise question identifying what specifically is wrong.
      Then wait. Do not generate a new theory. Do not propose a solution.
      One question. Silence.
```

**Correct:**
> "You said that's wrong — what specifically is wrong?"

**Incorrect:**
> [Proposes three alternative implementations]
> [Apologises and immediately suggests a different approach]

If the same category of answer has been rejected twice in a row, the model's
framing of the problem is wrong. Do not generate a third answer in the same frame.
Ask what the correct frame is.

---

## Complexity Scaling

| Session type | Governance level |
|-------------|-----------------|
| Skill file only (no code mod) | Full start protocol, relaxed end |
| Algorithm implementation | Full protocol + per-subproject approval gate |
| Bug fix | Full protocol + bug-to-tests: 3 tests BEFORE fix |
| Experiment design | Full protocol + experiment-design-SKILL |
| Subproject N → Subproject N+1 | Requires STATE saved + Luba explicit "yes" |

---

## LlmFromScratch DNA Rules (the 10 Rules from CLAUDE.md)

These are the non-negotiable patterns checked before every commit:

| Rule | Principle | Violation signal |
|------|-----------|-----------------|
| Rule 1 | Every public method returns `OperationResult<T>` | `void` or raw `T` return on public method |
| Rule 2 | Fabric First — dependencies through interfaces | `new ConcreteService()` in production code |
| Rule 3 | Every line has a plain-language comment | Lines without explanatory comment |
| Rule 4 | Zero test failures is a hard gate | Any red test before commit |
| Rule 5 | Read before touching | Modifying without reading the whole file |
| Rule 6 | One commit per phase | Multiple phases bundled into one commit |
| Rule 7 | Test-before-after metric per placement | Plan placement with no measurable gate |
| Rule 8 | Gate B per placement | Pass condition stated as comparison or count |
| Rule 9 | No abbreviations without expansion | First use without full form in parentheses |
| Rule 10 | No external ML frameworks | Any `using TensorFlow;` or equivalent |

---

## Reference Files

| File | Read When |
|------|-----------|
| `docs/DECISIONS-LOCKED.md` | Before any architectural decision |
| `docs/EXPERIMENT-RESULTS-MAP.md` | Before proposing algorithm changes |
| `docs/SUBPROJECT-MAP.md` | Before starting work in any subproject |
| `CLAUDE.md` | The 10 Rules — re-read when in doubt |
| `AGENTS.md` | 6 operational rules + Round Contract |
