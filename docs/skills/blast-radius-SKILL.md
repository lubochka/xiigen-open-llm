# Blast Radius Assessor

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

Before touching any file, assess how far the change propagates.
A change that seems isolated — "just update this parameter" — may break every
caller of a public interface. The blast radius assessment makes this visible
before any file is modified.

---

## Classification Levels

```
CRITICAL — change affects all callers in the entire solution
  Triggers: public interface signature change, shared OperationResult<T> return type change,
            DNA Rule change, CLAUDE.md fundamental change
  Impact: every class that calls the interface must be checked
  Protocol: full baseline before and after; read every caller; human review before commit

HIGH — change affects all callers in one subproject or one assembly
  Triggers: class constructor signature change, new required DI dependency,
            shared base class change, cross-assembly type change
  Impact: all callers in the affected assembly must be checked
  Protocol: grep all callers; run full subproject tests before and after

MEDIUM — change affects one class or one method
  Triggers: method signature change within a class, new private helper,
            implementation change (not interface change)
  Impact: only the changed class and its direct tests
  Protocol: run tests for the changed class; check the class's callers

LOW — change affects only future code, not existing callers
  Triggers: new file added, new method added to a class, new test added,
            documentation update, new skill file added
  Impact: no existing code affected
  Protocol: standard gate (build + tests)
```

---

## Assessment Procedure

### Step 1 — Identify Change Type and Location

| Location | Default Blast Radius |
|----------|---------------------|
| `docs/skills/` | LOW |
| `docs/COMPONENT-INTERFACES.md` | MEDIUM (doc change) |
| `Tests/` only | LOW |
| `Src/*/I*.cs` (interface) | CRITICAL |
| `Src/*/BaseService.cs` (base class) | HIGH |
| `Src/NN.Project/*.cs` (concrete class) | MEDIUM |
| `CLAUDE.md` | HIGH (governs all sessions) |
| `.csproj` files | HIGH (assembly changes) |

### Step 2 — Find All Dependents

```bash
# For CRITICAL/HIGH changes, find every caller of the changed identifier
grep -rn "IVectorIndex\|BruteForceIndex" Src/ Tests/ --include="*.cs" | sort

# For interface changes, find all implementations
grep -rn "class.*:.*IVectorIndex" Src/ --include="*.cs"

# For base class changes, find all subclasses
grep -rn "class.*:.*BaseService" Src/ --include="*.cs"
```

Never claim "this is isolated" without running the grep.

### Step 3 — Identify Protection Gates

```
CRITICAL:
  □ Run full test suite before any change (record count)
  □ Read every caller in full (Rule 5)
  □ Run full test suite after change
  □ Total count must be ≥ before count

HIGH:
  □ Record test count for affected subproject before change
  □ Read all callers in affected subproject
  □ Run affected subproject tests after change

MEDIUM:
  □ Run tests for the changed class before and after
  □ Check direct callers of the changed method

LOW:
  □ dotnet build: 0 errors
  □ dotnet test: Failed 0
```

### Step 4 — Write Impact Summary

Before touching any file, write the impact summary:

```
Change: Add SearchRange(float[] query, float radius) to IVectorIndex

Blast radius: CRITICAL
  Reason: public interface change — all implementations and all callers affected

Dependents found:
  Implementations: BruteForceIndex (Src/09.VectorSearch/BruteForceIndex.cs:12)
                   HnswIndex (Src/09.VectorSearch/HnswIndex.cs:8)
  Callers: HybridRetriever (Src/12.HybridRetriever/HybridRetriever.cs:45)

Protection gates:
  □ Test count before: [run dotnet test, record N]
  □ Read all 3 dependent files completely (Rule 5)
  □ Add SearchRange to both implementations
  □ Test count after: must be ≥ N (+ new tests for SearchRange)

Estimated impact: 2 implementations + 1 caller + their test files
```

---

## Special Cases

### Public Interface Change (CRITICAL)

```
Adding a new method to an existing interface:
  → All existing implementations must add the method
  → All existing callers may need to be updated
  → Never add an abstract method to an interface with 3+ implementations
    without listing all implementations first

Removing a method from an interface:
  → Even higher blast radius — ALL callers will not compile
  → Requires Luba approval before proceeding
```

### Constructor Signature Change (HIGH)

```
Adding a required parameter to a constructor:
  → All DI registrations must be updated
  → All "new ClassName()" calls in tests must be updated
  → grep for all instantiation sites before changing
```

### DNA Rule Change (CRITICAL)

```
Changing which DNA rules apply or how they are enforced:
  → Affects every code review and commit in all future sessions
  → Must update CLAUDE.md and dna-compliance-guard-SKILL.md simultaneously
  → Requires explicit Luba approval
```

---

## Anti-Patterns

1. **"This is just a small change"** — run the grep before assuming small
2. **Changing an interface without listing all implementations first** — always list before touching
3. **Claiming LOW blast radius on an interface change** — interface changes are always CRITICAL
4. **Not running the test suite before the change** — cannot measure delta without a baseline
