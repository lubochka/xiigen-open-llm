# Escalation Orchestrator

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

An evaluation pipeline without a defined escalation protocol makes ad-hoc decisions
when arbiters disagree. A BLOCK verdict from one arbiter gets diluted by PASS verdicts
from others. The pipeline proceeds with a violated invariant. Human escalation never
happens because no Rule 5 exists. This skill defines 6 decision rules applied in order
that route every panel verdict to exactly one outcome.

---

## When to Invoke

- When an arbiter panel evaluation completes and verdicts must be aggregated
- After multi-reviewer-design-SKILL.md produces per-reviewer verdicts
- When diagnostic-escalation-SKILL.md triggers an escalation path
- Before declaring any capability evaluation complete

---

## Section 1 — The 6 Decision Rules

Apply in order. Stop at the first rule that matches.

### Rule 1: ANY BLOCK → Remove Output from Pool

If any arbiter returns BLOCK severity on any output:
- That output is removed from the candidate pool entirely
- If all outputs are removed: proceed to Rule 6 (UNDECIDED)
- If some outputs remain: re-evaluate remaining outputs against Rules 2–6

**BLOCK-class arbiters:** Business Logic, DNA Compliance, Iron Rules, Goal Delivery (Role 8).
A single BLOCK from any of these rejects the output. No averaging. No discussion.

The violating arbiter returns the violation text. This becomes the targeted fix
in Rule 2 (CYCLE_WITH_PATCH) — not a generic re-run.

---

### Rule 2: One Output Survives All BLOCK Checks → ACCEPTED (with advisory review)

After Rule 1 removes BLOCK-failed outputs:
- If exactly one output has no BLOCK verdicts:
  - Check its CHALLENGE_OR_PASS arbiter verdicts (Roles 3, 6, 7)
  - If 0–2 CHALLENGE advisories: ACCEPTED — store advisory violations in evaluation notes
  - If 3+ CHALLENGE advisories: CYCLE_WITH_PATCH — targeting those specific challenges

---

### Rule 3: Multiple Outputs Survive → Winner by Score

- Upper evaluator (manager role from ai-pipeline-design-SKILL.md) scores surviving outputs
- Highest score = chosen, lowest score = rejected
- Tie-break: alphabetical label order (A beats B beats C) — deterministic, unbiased
- chosen + rejected = DPO triple populated in ContinuousLearningPipeline

---

### Rule 4: Max Cycles Reached Without Consensus → ESCALATE_TO_UPPER_JUDGE

If Rule 2 sends to CYCLE_WITH_PATCH and the cycle budget (from cycle-budget-prediction-SKILL.md)
is exhausted:
- The upper judge receives: all arbiter verdicts + unresolved conflict
- Upper judge may spawn a specialized arbiter for this run only
- If specialized arbiter resolves: continue from Rule 1 with its verdict included
- If still unresolved after upper judge: proceed to Rule 5

**Also triggers Rule 4 when:**
- Two BLOCK-class arbiters both CHALLENGE the same capability section with conflicting reasons
- DNA Compliance arbiter returns BLOCK on all candidate outputs (capability may need redesign)

---

### Rule 5: ESCALATE_TO_HUMAN

Human escalation produces:

```csharp
public record HumanEscalationRecord
{
    public string EscalationType { get; init; }   // "HUMAN_JUDGMENT_REQUIRED"
    public string CapabilityClass { get; init; }
    public string SubprojectId { get; init; }
    public Dictionary<string, List<ArbiterVerdict>> ArbiterVerdicts { get; init; }
    public string UnresolvedConflict { get; init; } // specific conflict description
    public bool UpperJudgeAttempted { get; init; }
    public string UpperJudgeResult { get; init; }
    public string HumanQuestion { get; init; }  // exactly what Luba must decide
}
```

**Effect:** Capability evaluation is SUSPENDED. No implementation proceeds until
Luba resolves the conflict. After resolution: record as DESIGN_REASONING entry
in `docs/DECISIONS-LOCKED.md`. Seed to DomainModelRegistry as an edge.

---

### Rule 6: All Outputs BLOCKED or Three-Way Tie → UNDECIDED

If all outputs were removed in Rule 1 (every generator failed every arbiter), or if
three outputs score exactly equal in Rule 3:

- Do NOT store to the main training data index
- Store all outputs to review index with `status: UNDECIDED`
- Emit a review required signal
- Record DESIGN_FLAW signal: the context package produced no acceptable output

**Three-way tie handling:** Three equally-scored outputs means the arbiter panel
cannot differentiate quality. This is a genesis prompt problem — the context
package needs more domain-specific constraints. The DESIGN_FLAW signal triggers
a context-package-design-SKILL.md review.

---

## Section 2 — Verdict Collection Format

The orchestrator receives verdicts in this format (model attribution stripped):

```csharp
public record PanelVerdict
{
    public string OutputLabel { get; init; }  // "A", "B", or "C"
    public Dictionary<string, ArbiterVerdict> Arbiters { get; init; }
}

public record ArbiterVerdict
{
    public string ArbiterRole { get; init; }       // "business_logic", "dna_compliance", etc.
    public string Verdict { get; init; }           // "APPROVED", "BLOCK", "CHALLENGE"
    public List<ViolationRecord> Violations { get; init; }
}

public record ViolationRecord
{
    public string Rule { get; init; }        // e.g. "Rule 1 — OperationResult<T> everywhere"
    public string Evidence { get; init; }    // e.g. "method Score() returns float, not OperationResult<float>"
    public string Severity { get; init; }   // "BLOCK" or "ADVISORY"
}
```

Note: `key_principles` arbiter lists ALL passed rules, not just violations. Absence
of a passed entry is ambiguous — the full list makes the check auditable.

---

## Section 3 — Learning Signal from Escalation

Every arbiter verdict is stored as a learning signal for future cycles:

```csharp
public record ArbiterVerdictSignal
{
    public string RunId { get; init; }
    public string CapabilityClass { get; init; }
    public string SubprojectId { get; init; }
    public string ArbiterRole { get; init; }
    public string Verdict { get; init; }
    public List<ViolationRecord> Violations { get; init; }
    public string OutputLabel { get; init; }
    public string? ResolvedSource { get; init; }  // null until DPO triple finalized
}
```

Teaching value from accumulated signals: After N capabilities, the signal store shows
which arbiter role most frequently blocks which capability class. DNA Compliance
blocking WeightedScoring often → context package for WeightedScoring needs better
Rule 1-10 constraints. This data feeds ContinuousLearningPipeline confidence updates.

---

## Anti-Patterns

1. **"BLOCK verdict averaged with PASS verdicts from other arbiters"** — ANY BLOCK
   rejects the output (Rule 1). There is no averaging. BLOCK is not a score.

2. **"Human escalation skipped because it slows down the cycle"** — if Rule 4's triggers
   are met, Rule 5 is mandatory. No implementation of a conflicted capability proceeds
   without Luba's decision.

3. **"Three-way tie stored as valid training data"** — a tie means the panel cannot
   differentiate. Store as UNDECIDED and trigger a context package review.

4. **"Upper judge called on every disagreement"** — the upper judge activates only at
   Rule 4 (budget exhausted or two BLOCK-class arbiters conflict same section).
   Routine CHALLENGE advisories route through Rules 2-3, not Rule 4.

5. **"Cross-session arbiter disagreements are lost"** — when an arbiter in one session
   reaches a different verdict about the same invariant as in a prior session, record the
   disagreement and its resolution. This is the highest-value learning signal.

---

## Integration

- arbiter-panel-design-SKILL.md Section 1 defines the 8 arbiter roles whose verdicts this skill receives
- cycle-budget-prediction-SKILL.md provides the cycle budget that triggers Rule 4
- diagnostic-escalation-SKILL.md governs what happens after Rule 5 (human escalation)
- DomainModelRegistry Rule 5 resolutions become new DECISIONS-LOCKED.md entries
- ContinuousLearningPipeline stores the learning signals from Rule 3 (DPO triples)
