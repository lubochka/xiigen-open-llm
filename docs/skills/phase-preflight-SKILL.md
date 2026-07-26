# Phase Preflight

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the equivalent in the operator's language), challenges an answer, or says a failure came
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

## When to Run

Run this skill **before writing any code in a phase**. It catches wrong assumptions
before they are baked into implementation.

---

## Assumption Registry

Before writing the first line, record every assumption the plan makes about the codebase:

```
ASSUMPTION-1: BruteForceIndex implements IVectorIndex
  Verify: grep -rn "IVectorIndex" Src/09.VectorSearch/ --include="*.cs"
  Expected: "class BruteForceIndex : IVectorIndex"
  Status: [ ] PENDING

ASSUMPTION-2: HybridRetriever takes IVectorIndex and IBm25Scorer via constructor
  Verify: Read Src/12.HybridRetriever/HybridRetriever.cs constructor
  Expected: public HybridRetriever(IVectorIndex index, IBm25Scorer scorer)
  Status: [ ] PENDING

ASSUMPTION-3: SessionStateStore.SaveAsync returns OperationResult<bool>
  Verify: Read Src/21.AgentInfrastructure/ISessionStorage.cs
  Expected: Task<OperationResult<bool>> SaveAsync(...)
  Status: [ ] PENDING
```

Verify every assumption before writing code. Change the plan if any assumption is wrong.

---

## Fix Patterns by Type

### Pattern A — Handler Injection (adding a new dependency to an existing class)

```
Verify before touching the file:
1. Read the class constructor completely (Rule 5)
2. Record current constructor signature
3. Find all callers of the constructor:
   grep -rn "new ClassName\|services.AddSingleton<ClassName>\|services.AddTransient<ClassName>" --include="*.cs"
4. Confirm DI registration location
5. Plan: what changes to constructor + DI registration + all callers
```

Example:
```csharp
// Current: public HybridRetriever(IVectorIndex index)
// Plan: add IBm25Scorer
// → Must update DI registration in Startup.cs (or equivalent)
// → Must find any "new HybridRetriever(...)" calls and add the new parameter
```

### Pattern B — Interface Field Access (adding a new method to an interface)

```
Verify before changing the interface:
1. Read the interface completely (Rule 5)
2. Find all implementations:
   grep -rn "class.*: IInterfaceName" Src/ --include="*.cs"
3. Find all callers:
   grep -rn "IInterfaceName\." Src/ --include="*.cs"
4. Plan: new method signature + implementation in each concrete class + callers
```

If there are more than 3 implementations, escalate to Luba before proceeding.
Interface changes have wide blast radius.

### Pattern C — Module Registration (adding a new class to DI container)

```
Verify before writing the class:
1. Locate the DI registration file (Program.cs, Startup.cs, or Module*.cs)
   grep -rn "AddSingleton\|AddTransient\|AddScoped" Src/ --include="*.cs" -l
2. Confirm the interface is already defined
3. Confirm the concrete class name doesn't already exist:
   grep -rn "class NewClassName" Src/ --include="*.cs"
4. Plan: new class + DI registration entry + any factory if required
```

---

## Escalation Format

If preflight discovers a blocking issue:

```
PREFLIGHT BLOCKER: [one-line description]

Assumption: [what the plan assumed]
Reality: [what the code actually shows]
Evidence: [file:line where reality is visible]

Options:
A. [option A — impact]
B. [option B — impact]

Recommended: [A or B and brief why]

Awaiting direction before proceeding.
```

Do not choose an option unilaterally unless it is clearly non-architectural.
If in doubt, escalate.

---

## Issue Inventory Integration

As you run preflight, record any issues discovered that are out of scope for the current phase:

```
OUT-OF-SCOPE-1: HybridRetriever constructor uses "new Bm25Scorer()" directly
  — DNA Rule 2 violation (Fabric First), not in scope for this phase
  — Record for future fabric-first audit pass
  — File: Src/12.HybridRetriever/HybridRetriever.cs:23
```

These become inputs for the fabric-first audit skill. Do not fix them in this phase
unless fixing is required for the current task to compile and test.

---

## Preflight Completion Criteria

Preflight is complete when:
```
□ All assumptions verified against live code (not from memory)
□ All assumptions that failed have plan amendments
□ Fix pattern identified (Handler Injection / Interface Field / Module Registration)
□ All callers of any changed component identified
□ DI registration location confirmed
□ No undocumented blockers remain
□ Out-of-scope issues recorded for future
```

Only then: begin writing code.
