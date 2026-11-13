# code-execution-SKILL.md — Test gate, tier system, Gate B format
## v2.0: BaseService template + subproject completion gate + OperationResult rule added.

## When to load
Load for any session that adds, modifies, or reviews code or tests.

---

## Zero failures is a hard gate — Rule 4

`dotnet test` must report zero failures before every commit.
Not "only new tests fail." Not "pre-existing failures excluded." Zero.
No exceptions. Pre-existing failures must be fixed in the same commit,
not deferred.

---

## One test = one metric

Every test verifies one specific number, count, or comparison.
"Works correctly" is not a test assertion.

```csharp
// ✅ Correct
Assert.Equal(0.895, result.BoundaryAccuracy, precision: 2);
Assert.True(enhanced > baseline, $"expected enhanced > {baseline}, got {enhanced}");

// ❌ Too vague
Assert.True(result.IsSuccess);
```

---

## Test-before-after rule — Rule 7

Every placement in a plan has a metric measured before and after.
The test captures both numbers and asserts enhanced >= baseline.

---

## Tier system for experiments

    Tier 1: 1–10 files    (unit validation)
    Tier 2: 11–100 files  (integration signal)
    Tier 3: 100+ files    (production signal)

A result of FAILED at Tier 2 must escalate to Tier 3 before a final verdict.
A result of SCALE_UP means the algorithm is correct but needs more data.

---

## Subproject completion gate — before any subproject phase is marked done

```bash
dotnet build LlmFromScratch.sln                        # zero errors
dotnet test Tests/LlmFromScratch.Tests.csproj          # zero failures, count >= baseline
git diff --name-only                                    # only expected files changed
```

All three must pass. If any fails: fix before marking the phase complete.

---

## BaseService rule — every new service class

Every new service class must extend BaseService and pass ISessionStorage to base.

```csharp
// ✅ Correct
public sealed class MyService : BaseService, IMyInterface
{
    // Every subclass needs this constructor — no exceptions (Rule 2, Fabric First)
    public MyService(ISessionStorage storage) : base(storage) { }
}

// ❌ Missing BaseService
public sealed class MyService : IMyInterface { }

// ❌ Missing constructor
public sealed class MyService : BaseService { }
```

---

## OperationResult rule — every public method

Every public method returns OperationResult<T>. No throw. No null return.

```csharp
// ✅ Correct
public OperationResult<string> GetName() { ... }
public Task<OperationResult<bool>> SaveAsync(...) { ... }

// ❌ Missing wrapper
public string GetName() { ... }
public bool Save() { ... }
```

---

## Live baseline rule — never use a remembered count

Always run `dotnet test` at the start of a session and before producing
any plan that references a test count. A number from memory or from a
JSON state file is stale. The gate requires the live number.
