# Systematic Debugging

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
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

## Core Principle

Random fixes waste time and create new bugs. Quick patches mask underlying issues.

**ALWAYS find root cause before attempting fixes. Symptom fixes are failure.**

## The Iron Law

```
NO FIXES WITHOUT ROOT CAUSE INVESTIGATION FIRST
```

If you have not completed Phase 1, you cannot propose fixes.

---

## When to Use

Use for ANY technical issue: test failures, build errors, unexpected behavior,
performance problems, integration issues.

Use this **especially** when:
- Under time pressure (emergencies make guessing tempting)
- "Just one quick fix" seems obvious
- You've already tried multiple fixes that didn't work
- You don't fully understand the root cause

**Do not skip when:**
- Issue seems simple (simple bugs have root causes too)
- You're in a hurry (systematic is faster than thrashing)

---

## The Four Phases

Complete each phase before proceeding to the next.

### Phase 1: Root Cause Investigation

**BEFORE attempting any fix:**

1. **Read error messages carefully**
   - Read `dotnet build` and `dotnet test` output completely
   - Note file paths, line numbers, error codes
   - Stack traces show the exact call chain

2. **Reproduce consistently**
   ```bash
   dotnet test --filter "FullyQualifiedName~FailingTestName" --no-build
   # Can you trigger it reliably? Does it fail every time?
   ```

3. **Check recent changes**
   ```bash
   git diff HEAD~1 -- Src/
   git log --oneline -5
   # What changed that could cause this?
   ```

4. **Gather evidence in multi-component systems**

   When the system has multiple components (interface → implementation → DI → test):

   Before proposing fixes, add diagnostic instrumentation:
   ```bash
   # Check where the chain breaks
   # Layer 1: Does the interface method exist?
   grep -n "public.*MethodName" Src/*/I*.cs --include="*.cs"

   # Layer 2: Does the implementation match the interface?
   grep -n "public.*MethodName" Src/*/ClassName.cs

   # Layer 3: Is DI registration present?
   grep -rn "IInterface\|ClassName" Src/*/Program.cs --include="*.cs" | grep "AddSingleton\|AddTransient"

   # Layer 4: Does the test inject the right type?
   grep -n "IInterface\|ClassName" Tests/ClassNameTests.cs | head -10
   ```

   Run once to find WHERE it breaks → THEN analyze → THEN investigate that layer.

5. **Trace data flow** — see root-cause-tracing-SKILL.md for backward tracing.

### Phase 2: Pattern Analysis

**Find the pattern before fixing:**

1. **Find working examples** — locate similar working code in the same solution
2. **Compare against references** — read the working example COMPLETELY; do not skim
3. **Identify differences** — list every difference between working and broken, however small

### Phase 3: Hypothesis and Testing

**Scientific method:**

1. **Form single hypothesis** — "I think X is the root cause because Y" — write it down
2. **Test minimally** — the SMALLEST possible change to test the hypothesis; one variable at a time
3. **Verify before continuing** — did it work? Yes → Phase 4. No → form NEW hypothesis.
   Do NOT add more fixes on top.
4. **When you don't know** — say "I don't understand X" — do not pretend to know.

### Phase 4: Implementation

**Fix the root cause, not the symptom:**

1. **Create failing test case FIRST** (bug-to-tests-SKILL.md — 3 failing tests before fix)
2. **Implement single fix** — address the root cause identified; ONE change at a time;
   no "while I'm here" improvements; no bundled refactoring
3. **Verify fix** — test passes? No other tests broken? Issue resolved?
4. **If fix doesn't work** — STOP. Count: how many fixes have you tried?
   - If < 3: return to Phase 1 with new information
   - **If ≥ 3: STOP and question the architecture** — do not attempt Fix #4 without
     discussing the architectural problem

5. **If 3+ fixes failed: question architecture**

   Pattern indicating architectural problem:
   - Each fix reveals new coupling or shared state problem in a different place
   - Fixes require restructuring to implement
   - Each fix creates new symptoms elsewhere

   STOP and ask: Is this pattern fundamentally sound?
   Discuss with Luba before attempting more fixes.

---

## Red Flags — STOP and Follow Process

If you catch yourself thinking any of these, return to Phase 1:
- "Quick fix for now, investigate later"
- "Just try changing X and see if it works"
- "I don't fully understand but this might work"
- "One more fix attempt" (when already tried 2+)
- Each fix reveals a new problem in a different place

---

## Common Rationalizations

| Excuse | Reality |
|--------|---------|
| "Issue is simple, don't need process" | Simple issues have root causes too. Process is fast for simple bugs. |
| "Emergency, no time for process" | Systematic debugging is FASTER than guess-and-check thrashing. |
| "Just try this first, then investigate" | First fix sets the pattern. Do it right from the start. |
| "I'll write test after confirming fix works" | Untested fixes don't stick. Test first proves it. |
| "Multiple fixes at once saves time" | Can't isolate what worked. Causes new bugs. |
| "I see the problem, let me fix it" | Seeing symptoms ≠ understanding root cause. |
| "One more fix attempt" (after 2+ failures) | 3+ failures = architectural problem. Question design, don't fix again. |

---

## Integration With Other Skills

- root-cause-tracing-SKILL.md — trace backward through call chain (Phase 1 step 5)
- defense-in-depth-SKILL.md — add validation at multiple layers after finding root cause
- bug-to-tests-SKILL.md — write 3 failing tests before fix (Phase 4 step 1)
- verification-before-completion-SKILL.md — verify fix worked before claiming success
