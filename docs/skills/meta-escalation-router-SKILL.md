# Meta-Escalation Router

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

## Core Problem

Not every AI pipeline failure needs Luba's judgment. Many failures are
mechanical — wrong model in pool, missing prompt section, localized constraint
violation. Escalating every failure to Luba produces noise that obscures the
real escalation signals and consumes session budget on mechanical fixes.
This skill defines the decision hierarchy: try algorithmic options first,
escalate only when no algorithmic option remains.

---

## When to Invoke

- When an AI pipeline cycle produces score < acceptanceThreshold
- When meta-arbiter-SKILL.md produces a proposal that requires routing
- When a cycle has failed 3+ times with no improvement
- When deciding whether to retry, modify the panel, or escalate to Luba
- Before writing an escalation briefing to Luba

---

## Section 1 — Decision Hierarchy

Try options in order. Stop at the first option that applies.
Do NOT skip ahead to escalate when an algorithmic option is available.

```
Step 1: Is there an accepted winner?
  → ACCEPT. No escalation needed.

Step 2: Is this a principles HARD_STOP?
  (All models produced Rule 1-10 violations — principles-arbiter-SKILL.md BLOCK)
  → ESCALATE immediately. No algorithmic fix applies.

Step 3: Is this a budget halt?
  (Cycle budget exhausted — escalation-orchestrator-SKILL.md Rule 4)
  → ESCALATE with best_available candidate.

Step 4: Is the score trajectory STUCK?
  (Score never exceeded 60% after 3+ cycles on same capability class)
  → ESCALATE with gap analysis. The capability design may need fundamental revision.

Step 5: Is the score trajectory REGRESSION?
  (Current score < prior score for this capability class, and a recent change exists)
  → ESCALATE with best historical result + change log.
  (Something changed that made it worse — requires investigation, not retry.)

Step 6: Everything else — try algorithmic options first:
  Option A: REDUCE — drop low-value model from panel, retry with remaining
  Option B: RETRY — inject targeted prompt fix, full new cycle
  Option C: JUDGE_ONLY — re-run only failing arbiters (not full cycle)
  Only after all algorithmic options exhausted → ESCALATE
```

---

## Section 2 — Score Trajectory Classification

Before routing, classify the score trajectory:

| Pattern | Score sequence example | Routing |
|---|---|---|
| CONVERGING | [45, 67, 81] | RETRY or ACCEPT |
| PLATEAU | [67, 71, 71] | RETRY with injection |
| STUCK | [45, 50, 48] | ESCALATE with gap analysis |
| REGRESSION | [85, 88, 72] with recent change | ESCALATE with change log |
| HARD_STOP | any round with principles BLOCK | ESCALATE immediately |

**CONVERGING** (score improving each cycle, even if below threshold): do not escalate.
Continue cycling or accept the best candidate with a targeted manual fix.

---

## Section 3 — Escalation Briefing Format

When escalation is required, the briefing must answer all five questions.
An escalation briefing that does not answer all five forces Luba to reconstruct
history from session files — which defeats the purpose of the briefing.

```markdown
## Escalation Briefing — [CapabilityClass] [sessionId]

### 1. WHY is this escalating?
[One sentence, specific — not "it failed"]
Example: "Score plateau at 71% after 4 cycles. Principles arbiter blocks on
         Rule 1 (throw statement) in every candidate."

### 2. WHAT happened across all cycles?
Score trajectory: [45, 67, 71, 71]
Best result: Cycle 3, score = 71%
Failing arbiters: [principles-arbiter: BLOCK — Rule 1 line 47] [iron-rules: PASS]

### 3. WHAT is the best candidate?
[Code snippet or file reference for the cycle 3 candidate]
Passing: [arbiter list]
Failing: [arbiter list with specific violation]

### 4. WHAT specifically is failing?
[Gap analysis per failing arbiter — see Section 4]

### 5. WHAT can be done?
[3–4 concrete options — see Section 5]
```

---

## Section 4 — Gap Analysis (Required for ESCALATE)

For each failing arbiter in the best candidate:

```csharp
public record EscalationGap
{
    public string ArbiterId { get; init; }          // e.g. "principles-arbiter"
    public double Score { get; init; }              // 0 = BLOCK
    public int CyclesFailed { get; init; }
    public string Violation { get; init; }          // exact violation text
    public string RootCause { get; init; }          // WHY the model keeps violating
    public string LikelyFix { get; init; }          // specific text or code change
    public string FixType { get; init; }            // "prompt_injection" | "manual_fix" | "design_change"
    public string EstimatedCost { get; init; }      // "$0.35 (1 more cycle)"
}
```

The `RootCause` field must explain WHY the model keeps making the mistake —
not just repeat the violation. "Genesis prompt says no throws without defining
what counts as a throw" is a root cause. "Rule 1 violated" is a violation restatement.

---

## Section 5 — Options for Luba (Always 3–4)

Every escalation briefing includes 3–4 concrete options:

```
[A] RETRY with prompt injection  ($cost, probability of resolution)
    Specific: which section of context package to modify and how

[B] JUDGE_ONLY with targeted arbiter  ($cost, lower probability)
    Specific: which arbiter to re-run after which manual fix

[C] Manual fix  (free, low risk)
    Specific: which line(s) to change in the best candidate

[D] Design revision  (free, higher risk)
    Use when: the capability design itself may be causing the violation
    Specific: which design artifact to revise and what to change
```

**Always include at least one manual fix option.** Manual fixes are always available
and often faster than an additional AI cycle.

**What Luba always decides (never algorithmic):**
- Capability design rewrites (context package fundamentally wrong)
- DECISIONS-LOCKED.md entries when the fix requires a locked decision
- Budget reallocations (new phase or cycle budget)
- Accepting a candidate with known violations (judgment call)
- Adding new capabilities to the AI pipeline

---

## Section 6 — Escalation Quality Checklist

Before submitting an escalation briefing:

```
□ Score trajectory included (not just current cycle)
□ Gap analysis has root_cause, not just violation restatement
□ At least 3 options provided
□ At least 1 option is a manual fix (always available)
□ Cost estimate for each retry option
□ No session-private data or personally identifiable information in briefing
□ Briefing references files by path, not by "in the previous session" type reference
```

---

## Section 7 — Positive and Negative Examples

**Correct escalation:**
```
WHY: Score plateau at 71% after 4 cycles.
     principles-arbiter BLOCK on Rule 1 (throw statement) in every candidate.

TRAJECTORY: [45, 67, 71, 71] — plateau detected

BEST CANDIDATE: Cycle 3, score 71%
  Failing: principles-arbiter (BLOCK — Rule 1 line 47)

GAP:
  Violation: "throw new ArgumentException" at line 47
  Root cause: Context package says "no exceptions" but does not show an example
              of OperationResult.Fail() usage. Models interpret "no exceptions"
              as "no unhandled exceptions" rather than "no throw statements."
  Likely fix: Add to context package CONSTRAINTS: "never use throw — use
              OperationResult<T>.Fail('reason', 'CODE') instead"
  Fix type:   prompt_injection
  Cost:       ~1 AI cycle

OPTIONS:
  [A] RETRY with Rule 1 example injection (~$0.30, HIGH probability)
  [B] JUDGE_ONLY after manually removing throw at line 47 (~$0.10)
  [C] Manual fix — replace throw at line 47 with OperationResult.Fail() (free)
  [D] Review context package CONSTRAINTS section for all Rule 1-10 examples
```

**Wrong escalation:**
```
WRONG: "Cycle 4 failed. principles-arbiter is blocking."
  Missing: trajectory, root cause, options
  → Luba cannot act without reading 4 cycles of history

WRONG: Escalating a CONVERGING trajectory [45, 67, 81]:
  Score is improving. This should RETRY, not escalate.
  Escalating a CONVERGING trajectory wastes Luba's attention.
```

---

## Anti-Patterns

1. **"Score < threshold → immediately escalate"** — check the trajectory first.
   CONVERGING trajectories should retry. Only STUCK or REGRESSION escalate.

2. **"Gap analysis says 'violation: Rule 1'"** — a violation restatement is not
   a root cause. State WHY the model keeps making the mistake.

3. **"Escalation briefing doesn't include options"** — without options, Luba
   must figure out what to do from scratch. The briefing's job is to make
   the decision fast, not to hand the whole problem back.

4. **"Manual fix option omitted because AI should handle it"** — manual fixes
   are always available and are often the fastest path. Always include one.

---

## Integration

- meta-arbiter-SKILL.md produces PROMPT_PATCH/SKILL_EDIT/SKILL_ADD proposals → this skill routes them
- escalation-orchestrator-SKILL.md Rule 4 (ESCALATE_TO_UPPER_JUDGE) feeds this skill
- escalation-orchestrator-SKILL.md Rule 5 (ESCALATE_TO_HUMAN) is what this skill gates before Luba
- ai-pipeline-design-SKILL.md: the pipeline whose cycles this skill monitors
- cycle-budget-prediction-SKILL.md: the budget values that trigger HALT escalation
