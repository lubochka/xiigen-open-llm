# Dev Safety — LlmFromScratch
## stack: .NET 9 · C# · xUnit

> BLOCKING constraint. Build errors = session blocked. No exceptions.

---

## What This Skill Does

Enforces the hard technical safety floor for every session:
- `dotnet build` must be green before and after every code change
- Test suite must pass at or above the session-start baseline
- Git discipline prevents accidental contamination of main
- No ML frameworks — TensorCore is the only tensor primitive

---

## SESSION GATE (Run at Start AND End of Every Phase)

```bash
# Build gate
dotnet build LlmFromScratch.sln
# Expected: Build succeeded. 0 Error(s)

# Test gate — record the count at session start, it is the floor
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: Passed! - Failed: 0, Errors: 0, Passed: N

# No client project — single-user, no web UI, no frontend build
```

**If either fails: session is BLOCKED. Do not proceed.**

---

## Build Discipline

### Compile errors = BLOCKED

A C# compile error is not a warning. Type errors in LlmFromScratch often
hide real runtime failures because:

- `OperationResult<T>` generics depend on correct type parameters — wrong type
  means wrong error handling flows silently
- Fabric interfaces mismatched at construction time cause wrong provider resolution
- xUnit test compilation errors mean the test that should catch the regression
  never runs

**Protocol when you encounter a compile error:**
1. STOP all other work
2. Fix the compile error completely
3. Re-run `dotnet build LlmFromScratch.sln` — must show 0 Error(s)
4. Then continue

### Never Skip the Build Check

Anti-patterns to reject immediately:
- "I'll fix the types later, the logic is right" → **REJECT**
- "It's just a cast, harmless" → **REJECT** — unsafe casts mask DNA violations
- "The compiler is being too strict here" → **REJECT** — fix the code

---

## Test Suite Discipline

### Count Must Never Decrease

Session-start count is the floor. If tests drop:
1. DO NOT proceed
2. Identify which tests disappeared and why
3. If a test was accidentally deleted: restore it
4. If a test file was renamed: update the project reference
5. Re-run until count is ≥ baseline

### Never Change an Assertion to Make a Test Pass

Changing `Assert.Equal(expectedValue, result.Value)` to `Assert.True(true)`
because the algorithm returns wrong output — that fixes the test to accept wrong
output. Fix the algorithm.

Exception: if Luba explicitly says "the old expected value was wrong, update
the assertion." That requires explicit approval.

### Test Baseline

There is no fixed minimum number. The baseline is whatever `dotnet test` reports
at the START of this session. Read it. Record it. That is the floor.

```bash
# At session start — record this number
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | grep "Passed\|Failed"
# Example output: Passed! - Failed: 0, Errors: 0, Passed: 877
# Your baseline this session: 877
```

---

## Git Discipline

### Never commit directly to main

Every session works on a designated branch.

```bash
# Always verify branch before any code change
git branch --show-current
# Must NOT be: main, master
```

### Commit Hygiene

- Commit message must reference the phase and what changed:
  `phase 2: AstTokenizer + RoleAssigner — 7 tests added`
- Never commit `/bin/`, `/obj/`, or build artifacts
- One logical change per commit — do not batch unrelated subprojects
- One commit per phase (Rule 6 in CLAUDE.md)

### Sensitive Files

Never commit:
- `.env` or any file containing API keys or credentials
- Any file containing model weights or training data
- `*.pem`, `*.key`, `*.cert` files

---

## No External ML Frameworks (Rule 10)

Every algorithm is implemented from scratch in C#.

**Forbidden dependencies:**
- `TensorFlow.*`
- `Microsoft.ML` (ML.NET training components)
- `PyTorch` (via any C# binding)
- Any GPU training library

**Allowed:**
- `Microsoft.CodeAnalysis.CSharp` (Roslyn — for code analysis subprojects)
- `Microsoft.Data.Sqlite` (session state persistence)
- `YamlDotNet` (skill file parsing)
- NuGet packages for non-ML utilities

If an algorithm requires a capability from one of the forbidden libraries,
implement it from scratch. That is the point of this codebase.

---

## Reference Files

| File | Read When |
|------|-----------|
| `CLAUDE.md` | The 10 Rules — full reference |
| `AGENTS.md` | Operational rules + Round Contract |
| `Tests/LlmFromScratch.Tests.csproj` | Before adding new test files |
