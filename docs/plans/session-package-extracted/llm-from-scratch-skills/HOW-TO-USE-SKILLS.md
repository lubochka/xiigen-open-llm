# HOW TO USE SKILLS — LlmFromScratch
## stack: .NET 9 · C# · xUnit · no external ML frameworks

---

## What these skills are

These skills govern how Claude operates on the LlmFromScratch codebase.
They are rewritten from scratch for this project's specific structure:
subprojects, xUnit tests, OperationResult pattern, Fabric-first interfaces,
and experiment-driven development.

Every skill is written in terms of actual LlmFromScratch patterns:
- `dotnet build LlmFromScratch.sln` (not `cd server && npm run build`)
- `dotnet test --filter "ClassName" Tests/LlmFromScratch.Tests.csproj` (not `npx jest`)
- `OperationResult<T>` (not `DataProcessResult<T>`)
- `BaseService : BackgroundService` (not `MicroserviceBase`)
- `new MyService(new NullSessionStorage())` (not NestJS TestingModule)
- `*Tests.cs` (not `*.spec.ts`)
- `NN.SubprojectName/` (not `server/src/engine/flows/`)
- Baseline: read live from `dotnet test` (not hardcoded "≥ 2,342")

---

## Skill Index

| Skill | When to load |
|-------|-------------|
| `agent-constitution-SKILL.md` | Every session — load first |
| `no-product-decisions-SKILL.md` | Every session — load second |
| `dev-safety-SKILL.md` | Every session — build gate and git discipline |
| `infrastructure-discovery-SKILL.md` | Before writing any plan or touching code |
| `experiment-design-SKILL.md` | Before designing any E-N experiment |
| `mental-debug-SKILL.md` | When a test fails or output is wrong |
| `systematic-debugging-SKILL.md` | Complex multi-layer debug — four-phase protocol |
| `dna-compliance-guard-SKILL.md` | Before every commit — 10-rule check |
| `bug-to-tests-SKILL.md` | Every bug fix — 3 failing tests before fix |
| `three-level-verification-SKILL.md` | Every fix — unit + integration + full suite |
| `test-integrity-SKILL.md` | When test count changes or assertions are questioned |
| `self-verification-SKILL.md` | Before declaring any fix "done" |
| `retroactive-development-SKILL.md` | Bug in algorithm → fix the source |
| `plan-review-SKILL.md` | Before handing any plan to code execution |

---

## Human Override

```
1. Luba's direct instruction ← ALWAYS WINS
2. SESSION-START-PROMPT.md
3. These skill files
4. CLAUDE.md and AGENTS.md
5. Claude's training defaults
```

---

## Quick Reference: Command Translations

| Action | Command |
|--------|---------|
| Build the solution | `dotnet build LlmFromScratch.sln` |
| Run all tests | `dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 \| tail -5` |
| Run specific test class | `dotnet test --filter "ClassName" Tests/LlmFromScratch.Tests.csproj` |
| Run by name fragment | `dotnet test --filter "FullyQualifiedName~Fragment" Tests/...` |
| List tests matching filter | `dotnet test --filter "ClassName" Tests/... --list-tests` |
| Check current branch | `git branch --show-current` |
| See what changed | `git diff --name-only` |

## Quick Reference: LlmFromScratch DNA Patterns

| Pattern | C# implementation |
|---------|-----------------|
| `OperationResult<T>` | `OperationResult<T>.Ok(value)` / `.Fail(msg, code)` |
| `BaseService` | `public abstract class BaseService { protected readonly IXxxStorage Storage; }` |
| Fabric-first | `public MyService(ISessionStorage storage)` — inject, never `new Storage()` |
| Test stub | `new MyService(new NullSessionStorage())` |
| Test assertion | `Assert.True(result.IsSuccess); Assert.Equal(expected, result.Value!.Field)` |
| xUnit test | `[Fact] public void ClassName_Condition_ExpectedResult()` |

---

## Subproject Structure Reference

Every subproject follows this layout:
```
NN.SubprojectName/
├── Core/
│   └── Core.cs              ← OperationResult<T>, StandardEvent, IdempotencyStore, BaseService
├── Fabric/
│   └── Interfaces.cs        ← IXxxStorage, IXxxEventLogger, IXxxAnnotator
├── Algorithms/
│   ├── AlgorithmA.cs        ← : BaseService, IXxxAnnotator
│   └── AlgorithmB.cs
└── NN.SubprojectName.csproj
```

Tests live in:
```
Tests/LlmFromScratch.Tests/
├── SubprojectNameTests.cs   ← main test class
└── LlmFromScratch.Tests.csproj
```
