# dev-safety-SKILL.md — Build gate, git discipline, DNA quick check

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
## v2.0: compile error blocking, assertion rule, sensitive files — translated from xiigen

## When to load
Load at the start of every EXECUTOR session.
Load before any commit.
Load when a new .cs service file is created (Rule 1 + Rule 2 quick check).

---

## SESSION GATE — runs at start AND end of every phase

```bash
dotnet build LlmFromScratch.sln                       # zero errors
dotnet test Tests/LlmFromScratch.Tests.csproj         # zero failures, count >= baseline
git diff --name-only                                   # only expected files changed
```

All three must pass. If any fails: fix before proceeding.

---

## Build errors = BLOCKED

A C# compile error is not a warning. It is not "the type is slightly wrong but the logic is fine."

Compile errors in this project cause real runtime failures because:
- `OperationResult<T>` generics break if the type argument is wrong — the wrong T silently changes which `.Value` is returned
- Interface mismatches cause the DI (Dependency Injection) container to resolve the wrong concrete type
- OperationResult namespace mixing (SC29 vs AgentInfrastructure) causes impossible casts at runtime

**Protocol when you encounter a compile error:**
1. STOP all other work
2. Fix the error completely
3. Re-run `dotnet build` — must show 0 errors
4. Then continue

**Anti-patterns to reject immediately:**
- "I'll fix the type error later, the logic is right" → REJECT
- "It's just a missing using, harmless" → REJECT — fix it now
- "The compiler is being overly strict" → REJECT — fix the code, not the compiler

---

## Test suite discipline

### Count must never decrease

Session-start count is the floor. If tests drop:
1. Do NOT proceed
2. Identify which tests disappeared and why
3. If a test was accidentally deleted: restore it
4. If a test helper was renamed: update all references
5. Re-run until count is ≥ baseline

### Never change an assertion to make a test pass

```csharp
// WRONG — changing the assertion to accept wrong output
Assert.Equal("CODE_Q", result.Value![0].Domain);  // ← changed from "BUG" because code returned "CODE_Q"

// CORRECT — fix the code so it returns "BUG"
```

Changing an assertion to make a test pass is fixing the test to accept wrong output.
Fix the engine. The only exception: Luba explicitly says "the old behavior was wrong, update the test."
That requires explicit written approval.

### Test baseline

| Suite | Minimum passing |
|-------|----------------|
| `dotnet test Tests/LlmFromScratch.Tests.csproj` | ≥ 916 (as of 2026-05-01) |

Always read the actual number from `dotnet test` output — never copy from memory.

---

## Git discipline

### Branch

This project works on `main`. In a worktree, the working branch is `claude/<worktree-name>`.
Before any code change, verify:

```bash
git branch --show-current
# Must NOT be: a branch other than claude/<worktree> or main directly
```

### Commit hygiene

- One commit per phase — never bundle multiple phases
- Commit message format: `"phase N: what changed — files changed"`
- Never `git commit --no-verify`
- Never commit with failing tests
- Push to main after the final phase: `git push origin <worktree-branch>:main`

### Sensitive files — never commit

- `.env` or `.env.*` files
- Any file containing API keys, tokens, or credentials
- `*.pem`, `*.key`, `*.cert` files
- `appsettings.Development.json` with real connection strings

---

## DNA quick check — run before every commit

For every new or modified `.cs` file in a subproject (not in Tests/):

```
☐ Rule 1: every public method returns OperationResult<T> — no void, no raw bool, no throw
☐ Rule 2: every external dependency injected through interface — no "new ConcreteClass()"
☐ Rule 3: every production code line has a plain-English comment
☐ Rule 10: no ML.NET, PyTorch, or external ML library references
☐ BaseService: every service class inherits BaseService and passes ISessionStorage to base()
☐ OperationResult<T>: SC29 namespace only inside 29.SemanticCompiler — everywhere else use AgentInfrastructure
```

If any check fails: fix before committing.

---

## Zero failures rule

"Zero failures" means zero. Not "zero new failures." Not "pre-existing failures excluded."

Pre-existing failures must be fixed in the current phase, not deferred.
There is no narrow reading where only new code must pass.
