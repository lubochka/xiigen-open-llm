# Subproject Examination

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

## Core Problem

A subproject that starts implementation before its infrastructure gaps are known
will stall mid-phase when a missing interface or missing dependency is discovered.
This skill installs a pre-implementation examination gate that produces three
documents before any code is written. Nothing starts until those documents exist
and are approved.

---

## When to Invoke

- When a new subproject is added to SUBPROJECT-MAP.md
- When an existing subproject advances from STUB to ACTIVE
- When a new capability (interface + implementation + tests) is planned
- Before any Phase that creates more than one new .cs file

One examination before implementation = no mid-phase infrastructure discoveries.

---

## Section 1 — The 3-Document Output

The examination produces exactly 3 documents before any .cs file is written.
Do not produce implementation until all 3 are reviewed and approved by Luba.

### Document 1: `docs/SESSION-[SUBPROJECT]-MASTER.md`

**Header:**
```
# SESSION-[SUBPROJECT]-MASTER.md
## Subproject: [number] — [name]
## Date: [date]
## Status: DRAFT — review before executing
```

**Section 1: Infrastructure Decision Tree**

Write a decision tree — NOT a checklist. Every FAIL condition has a BUILD instruction.

```markdown
Run each check in order. If it fails, follow the BUILD instruction.
Only begin Phase 1 after ALL checks pass.

CHECK 1: Build baseline
  Command: dotnet build [SolutionFile] → must output "0 Error(s)"
  If PASS: proceed to CHECK 2
  If FAIL: Fix build errors first. Record failing files. Do not proceed.

CHECK 2: Test baseline
  Command: dotnet test [SolutionFile] → record exact count (N passed, 0 failed)
  Record in STATE-P[n].json as test_baseline.
  Not pass/fail — measurement only. Proceed to CHECK 3.

CHECK 3: Required interfaces
  For each interface this subproject depends on:
  Command: grep -rn "interface [InterfaceName]" src/
  If PASS (found): proceed to next interface
  If FAIL (not found): record as BLOCKED in MISSING-INFRASTRUCTURE document.

CHECK 4: Required project references
  Command: grep -n "[ProjectName]" [SubprojectFile].csproj
  If PASS: proceed to CHECK 5
  If FAIL: Add ProjectReference. dotnet build to verify. Then proceed.

CHECK 5: Naming convention
  All new files must follow: [ClassName].cs, [ClassName]Tests.cs
  Verify no existing files in the subproject conflict.
  Proceed to implementation.

All checks passed → produce Document 2 and Document 3 → await Luba approval → begin Phase 1.
```

**Section 2: Capability Contract**

For each class/interface to be created, produce the complete specification:

```markdown
### [ClassName]

Interface: I[ClassName] (if public-facing)
Concrete: [ClassName]
Test class: [ClassName]Tests

Public methods:
  [MethodName]([params]) → OperationResult<[T]>
  [description of what this method does]

DNA requirements:
  Rule 1: returns OperationResult<T> — never throws, never returns null
  Rule 2: depends on [IInterfaceName] — not the concrete class
  Rule 3: every production line has a comment

Constructor dependencies (fabric-first):
  [IInterfaceName] [_fieldName]

Gate B:
  dotnet test --filter "FullyQualifiedName~[ClassName]Tests" → [N] passed, 0 failed
```

**Section 3: Phase Plan**

```markdown
Phase 1: [ClassName] implementation
  Files: [ClassName].cs, [ClassName]Tests.cs
  Gate B: dotnet test --filter "[ClassName]Tests" → [N] passed, 0 failed

Phase 2: Integration (if applicable)
  Files: [integration file]
  Gate B: dotnet test --filter "[filter]" → [N] passed, 0 failed

Phase [LAST]: HOW-TO-USE-SKILLS.md update (if skill files added)
  Gate B: Skill file exists at docs/skills/[name]-SKILL.md
```

---

### Document 2: `docs/[SUBPROJECT]-MISSING-INFRASTRUCTURE.md`

One entry per gap found in CHECK 3 (missing interfaces, missing project references,
missing base classes). Format:

```markdown
### GAP-[N]: [Feature Name]

**What it is:** one paragraph — what is missing and what it provides

**What fails without it:**
- [which Phase gate fails]
- [which tests cannot be written]

**Build spec:**
  File to create: [exact path]
  Interface/class: [full C# signature with namespace]
  Verification: dotnet build → 0 Error(s)
               dotnet test --filter "[filter]" → [N] passed, 0 failed

**Priority:** BLOCKER | CRITICAL | IMPORTANT
  BLOCKER   = Phase 1 cannot start without this
  CRITICAL  = Phase completes with wrong behavior
  IMPORTANT = quality or test coverage degraded
```

End with:
1. Summary table: Gap | Name | Priority | Blocks Phase | Also needed by
2. Implementation order (dependency chain)
3. "What is NOT missing" section — explicit list of what was verified present

---

### Document 3: `docs/[SUBPROJECT]-LEARNING-SPEC.md`

```markdown
## Domain input
[Plain-English description of what this subproject does — no C# class names]

## New capabilities introduced
| Capability | What it adds to the system | First consumer |
|---|---|---|
| [capability] | [description] | [who calls it] |

## Behavioral tests required
For each capability:
  Input: [specific input value or state]
  Expected output: [specific OperationResult with value]
  Verified by: dotnet test --filter "[TestName]"

## Training signal
  Capabilities that will feed ContinuousLearningPipeline: [list or "none"]
  DpoTrainer involvement: YES — requires chosen/rejected pairs | NO

## Completion criteria
  All tests pass: dotnet test → [N] passed, 0 failed
  Build: dotnet build → 0 Error(s)
  DNA compliance: Rule 1 (OperationResult<T>), Rule 2 (fabric-first), Rule 3 (comments)
```

---

## Section 2 — Examination Rules

1. **No placeholder values** — every field in Document 1 Section 2 must have a real value. If a value is unknown, state it as a gap in Document 2.

2. **Gaps have BUILD specs** — a gap entry that says "add this interface" without showing the full C# signature is incomplete. Include the complete interface definition.

3. **Decision tree, not checklist** — every CHECK failure must have a BUILD instruction, not "STOP and investigate."

4. **BLOCKER means stop** — a GAP-N marked BLOCKER means Phase 1 cannot start. Resolve it first.

5. **Approval required** — do not begin writing .cs files until Luba has reviewed all 3 documents.

6. **Gate B per phase** — every phase in Document 1 Section 3 has one `dotnet test` command with an exact expected count.

---

## Anti-Patterns

1. **"I know what's needed, skip the examination"** — the examination takes one session. Discovering a missing interface mid-Phase 2 takes two sessions.

2. **"Infrastructure gap can be fixed later"** — BLOCKER gaps block Phase 1. They cannot be fixed in Phase 2 — Phase 2 depends on Phase 1 completing.

3. **"Document 3 (learning spec) is optional"** — it is required for subprojects that feed `ContinuousLearningPipeline`. Omitting it means training data cannot be designed.

4. **"Gap has no build spec"** — a gap without a build spec is not actionable. An EXECUTOR session cannot fix an unspecified gap.

---

## Integration

- prerequisite-chain-SKILL.md verifies the BLOCKER items in Document 2 before execution begins
- plan-self-validation-SKILL.md requires Document 1 Section 3 (phase plan) to have Gate B per phase
- solution-scope-gate-SKILL.md determines the scope level (Level 3/4/5) for each gap in Document 2
- phase-completion-packager-SKILL.md uses Document 3 completion criteria as the packager's evidence baseline
