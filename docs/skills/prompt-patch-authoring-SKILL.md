# Prompt Patch Authoring

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

A context package that produces a score in the 0.65–0.89 range has a
fixable gap — but applying the wrong patch type, patching the wrong
bottleneck, or introducing a conflict with an existing named check will
produce a regression in a different capability class while appearing to
fix the current one. This skill defines the six rules that govern when
and how to patch a context package, what the patch must contain, and how
to prevent it from breaking what was already working.

---

## When to Invoke

- When a capability class scores in DETAIL_GAP (0.65–0.84) and a targeted
  patch is the right intervention (not a full rewrite)
- When score-interpretation-SKILL.md routes to "apply PATCH"
- When meta-arbiter-SKILL.md emits a PromptPatchProposal
- When the escalation orchestrator selects PATCH cycle before escalating

---

## Section 1 — The Six Rules

### Rule 0 — One Bottleneck Per Patch

A patch targets exactly one bottleneck per cycle. Never combine two
independent gap fixes in the same patch. If two gaps exist:

```
Identify the higher-impact bottleneck.
Patch it. Re-score.
If score still insufficient: identify next bottleneck. Patch separately.
```

Combining two fixes makes it impossible to determine which fix resolved
the gap — or which introduced a regression. One patch = one learning signal.

### Rule 1 — NAMED CHECK + WRONG/CORRECT Pair

Every patch must contain:

```
NAMED_CHECK: CF-[N] — [descriptive name]
WRONG:   [exact pattern that fails — not a generalization, the literal failure mode]
CORRECT: [exact pattern that passes — the replacement that satisfies the iron rule]
```

Example:

```
NAMED_CHECK: CF-12 — OperationResult return on public method
WRONG:   public void ProcessBatch(List<string> ids) { ... }
CORRECT: public OperationResult<BatchResult> ProcessBatch(List<string> ids) { ... }
```

The WRONG pattern must match the literal failure mode observed. A generalized
description ("missing return type") is insufficient — the validator needs to
match the pattern against generated code.

### Rule 2 — Target Zone 0.80–0.90

The patch target zone is **0.80–0.90**:

```
< 0.80:  Patch is attempting too much — narrow the scope further
  0.80–0.90: Target zone — single focused fix
> 0.90:  Over-specified — the patch is adding detail the iron rule doesn't require
```

Over-specification at > 0.90 is a signal to loosen, not to celebrate. A
patch that constrains generated code beyond what the iron rule requires
will block valid implementations that satisfy the rule through different
but correct means.

### Rule 3 — Regression Prevention Checklist

Before submitting any patch:

```
□ Load all existing NAMED CHECKs for this capability class
□ Verify: does the new WRONG pattern conflict with any existing CORRECT pattern?
□ Verify: does the new CORRECT pattern match any existing WRONG pattern?
□ If either conflict exists: the bottleneck is not a patch target — escalate to
  score-zero-investigation-SKILL.md Fix Class C (contract revision)
```

A patch that passes score for the current capability class while conflicting
with an existing check is worse than no patch — it introduces a silent
failure in another capability class.

### Rule 4 — Patch vs Rewrite Thresholds

```
Score 0.65–0.84 (DETAIL_GAP):        Targeted patch (this skill)
Score 0.50–0.64 (PATTERN_MISSING):   Section rewrite (all checks in one sub-section)
Score < 0.50 (STRUCTURAL):           Full genesis rewrite (reconstruct from iron rules)
```

A DETAIL_GAP score means the model understands the pattern — it missed
a specific detail. A PATTERN_MISSING score means the model has the wrong
mental model of one sub-section. A STRUCTURAL score means the context
package itself needs to be rebuilt.

Never apply a targeted patch to a STRUCTURAL-class failure. The patch will
appear to help locally while the structural problem compounds.

### Rule 5 — Multi-Generate Verification

When the target topology uses multi-generate (Pattern F from
ai-pipeline-topology-SKILL.md), both generators must avoid the WRONG
pattern:

```
After writing the patch:
  Run generator A with patch applied
  Run generator B with patch applied
  Verify: neither produces the WRONG pattern
  Verify: both produce a CORRECT pattern OR an acceptable variant
```

A patch that fixes Generator A and regresses Generator B produces no net
learning signal — the DPO (Direct Preference Optimization) arbiter will
see disagreement, not improvement.

### Rule 6 — Patch History Record

Every applied patch is recorded:

```csharp
public record PatchHistoryEntry
{
    public string CapabilityClass { get; init; }
    public string SessionId { get; init; }
    public string NamedCheck { get; init; }    // CF-N identifier
    public string WrongPattern { get; init; }
    public string CorrectPattern { get; init; }
    public double ScoreBefore { get; init; }
    public double ScoreAfter { get; init; }
    public string PatchOutcome { get; init; }  // RESOLVED / PARTIAL / REGRESSED
}
```

A REGRESSED outcome is high-value training data. Do not delete it.
It feeds score-zero-investigation-SKILL.md as evidence of a Class B or
Class C problem (check over-fires or contract wrong).

---

## Section 2 — Patch Format Template

```
CONTEXT_PATCH:
  CapabilityClass: [capability class name]
  IronRuleTarget:  CF-[N]
  ScoreBefore:     [current score]
  TargetScore:     0.85 (PASS minimum)

  NAMED_CHECK: CF-[N] — [check name]
  WRONG:   [exact failure pattern — literal code or description]
  CORRECT: [exact passing pattern — literal code or description]

  RegressionCheck:
    - Conflicts with existing WRONG patterns: NONE / [list]
    - Conflicts with existing CORRECT patterns: NONE / [list]

  MultiGenerateVerified: YES / NO / N/A (single-generate topology)
```

---

## Anti-Patterns

1. **"Patching two bottlenecks in one cycle"** — produces two learning
   signals simultaneously, making attribution impossible. One bottleneck per patch.

2. **"WRONG pattern is a generalization"** — "missing OperationResult" is not
   matchable. The validator needs a literal pattern. Write the actual code
   that fails, not a description of why it fails.

3. **"Target score set at 1.00"** — 1.00 means the model can only produce
   one valid implementation. 0.85–0.90 is the target; variation above that
   produces valid code through different routes.

4. **"Skipping Rule 3 regression check"** — a patch that introduces a
   conflict with an existing check creates a silent failure in a different
   capability class. The regression check is not optional.

5. **"Applying a patch to a STRUCTURAL failure"** — a context package
   scoring < 0.50 needs reconstruction, not a targeted addition. Patches on
   structural failures produce cosmetic score changes while the root cause remains.

---

## Integration

- score-interpretation-SKILL.md routes to this skill when bracket = DETAIL_GAP (0.65–0.84)
- score-zero-investigation-SKILL.md handles cases where named checks cannot be added
- meta-arbiter-SKILL.md produces PromptPatchProposal with this skill's format
- ai-pipeline-topology-SKILL.md Pattern F requires Rule 5 multi-generate verification
- planning-dpo-authoring-SKILL.md captures the patch outcome as a CALIBRATION_COMPARISON signal
