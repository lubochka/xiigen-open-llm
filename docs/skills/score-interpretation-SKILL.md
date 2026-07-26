# Score Interpretation

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

## Core Problem

Project-local override: this skill cannot create a user-facing stop gate. Any
`STOP`, `hard stop`, or score threshold in this file means an internal
investigation route for the scored artifact only. It does not override
AGENTS.md, the active plan, the active state, or Luba's current instruction to
continue when `next_action` is known.

A score number without an interpretation protocol produces two failure
modes: acting on a score that signals infrastructure failure (testability
problem) as if it were a content failure, and treating a 0.71 the same
as a 0.48 when they require completely different interventions. This skill
defines the five sub-scores, the five brackets, and the decision tree that
maps any score to a specific next action — including the hard stops that
must fire before any patching begins.

---

## When to Invoke

- Immediately after any AI pipeline cycle produces a score
- Before deciding whether to patch, rewrite, or escalate
- When the meta-arbiter asks what the score means for the current capability class
- When score-zero-investigation-SKILL.md needs a trigger classification

---

## Section 1 — Five Sub-Scores

Every composite score is composed of five sub-scores. Read each independently
before reading the composite:

```
iron_rules:        Fraction of CF-N iron rule checks that passed.
                   Low: the model is violating structural invariants.

dna_compliance:    Fraction of CLAUDE.md Rule 1-10 checks that passed.
                   Low: the model is generating code that violates DNA rules.

testability:       Fraction of generated code paths that are testable
                   (no static singletons, no untestable constructors, etc.)
                   Low: may be an infrastructure problem, not a model problem.

machine_constant:  Sub-score for hardcoded values that should be configurable.
                   Low (< 0.85): SILENT FAILURE RISK — load score-zero-investigation
                   immediately. The model produces code that compiles and passes
                   dotnet test but teaches wrong configuration patterns.

fail_open_behavior: Sub-score for whether the model generates safe failure modes.
                   Low: the model produces code that silently swallows errors.
```

**The machine_constant check is the only sub-score with a mandatory internal investigation route.**
Any machine_constant < 0.85 requires score-zero-investigation-SKILL.md before
any other action. Silent failures from wrong machine constants corrupt training
data.

---

## Section 2 — Five Score Brackets

```
PRESCRIPTIVE    ≥ 0.90   Over-specified or target reached.
                          Check: is the context package constraining valid
                          implementations that satisfy the iron rule through
                          a different (correct) route? If yes: loosen.

PASS            ≥ 0.85   Gate B achieved. Record. Move to next capability class.
                          No patch needed. Produce DPO triple.

DETAIL_GAP      ≥ 0.65   Model understands the pattern but missed a specific detail.
                          Intervention: targeted patch (prompt-patch-authoring-SKILL.md).

PATTERN_MISSING ≥ 0.50   Model has the wrong mental model of a sub-section.
                          Intervention: section rewrite — replace all checks in
                          the sub-section where iron_rules or dna_compliance is lowest.

STRUCTURAL       < 0.50  Model cannot produce a passing implementation from the
                          current context package.
                          Intervention: full genesis rewrite from iron rules.
```

---

## Section 3 — Decision Tree

Apply in this exact order. Do not skip steps.

```
Step 1: Check machine_constant sub-score
  machine_constant < 0.85 → load score-zero-investigation-SKILL.md before
  patching that scored artifact. Record the route in state and continue the
  plan-derived next action after the investigation path is known.

Step 2: Check testability sub-score
  testability < 0.50 → Verify mocks and fabric interfaces before patching.
  A low testability score may indicate an infrastructure problem (missing
  interface, wrong DI registration) rather than a model generation problem.
  Patching content will not fix an infrastructure failure.

Step 3: Check composite score bracket
  ≥ 0.90 → PRESCRIPTIVE. Check for over-specification. Loosen if needed.
  ≥ 0.85 → PASS. Record Gate B. Produce DPO triple. Done.
  ≥ 0.65 → DETAIL_GAP. Load prompt-patch-authoring-SKILL.md.
  ≥ 0.50 → PATTERN_MISSING. Section rewrite.
   < 0.50 → STRUCTURAL. Full genesis rewrite.

Step 4: Record interpretation
  Record the interpretation in STATE.json before acting:
  { "score": 0.78, "bracket": "DETAIL_GAP", "machineConstant": 0.91,
    "testability": 0.88, "intervention": "PATCH", "bottleneck": "CF-12" }
```

Recording before acting prevents a second cycle from starting on wrong
premises if the session is interrupted.

---

## Section 4 — Sub-Score Action Table

| Sub-score         | Threshold | Action                                                     |
|-------------------|-----------|------------------------------------------------------------|
| machine_constant  | < 0.85    | Internal investigation → score-zero-investigation-SKILL.md |
| testability       | < 0.50    | Verify infrastructure before patching content              |
| testability       | 0.50-0.74 | Note infrastructure risk; patch content with caution       |
| iron_rules        | < 0.70    | Section rewrite for iron rule sub-section                  |
| dna_compliance    | < 0.70    | Section rewrite for DNA rule sub-section                   |
| fail_open         | < 0.70    | Add explicit fail_open named check to context package      |

---

## Section 5 — Interpretation Record Format

```csharp
public record ScoreInterpretation
{
    public string CapabilityClass { get; init; }
    public string SessionId { get; init; }
    public double CompositeScore { get; init; }
    public string Bracket { get; init; }        // PRESCRIPTIVE / PASS / DETAIL_GAP / PATTERN_MISSING / STRUCTURAL
    public SubScores SubScores { get; init; }
    public string Intervention { get; init; }   // NONE / PATCH / SECTION_REWRITE / GENESIS_REWRITE / INVESTIGATION
    public string BottleneckCheck { get; init; } // CF-N of the highest-impact gap
    public bool MachineConstantHardStop { get; init; }
}

public record SubScores
{
    public double IronRules { get; init; }
    public double DnaCompliance { get; init; }
    public double Testability { get; init; }
    public double MachineConstant { get; init; }
    public double FailOpenBehavior { get; init; }
}
```

---

## Anti-Patterns

1. **"Reading composite score without checking machine_constant first"** —
   a 0.82 composite with machine_constant 0.71 is an internal investigation
   route, not a DETAIL_GAP.
   Sub-scores must be checked before the composite bracket determines the action.

2. **"Patching content when testability is low"** — testability < 0.50 means
   the test infrastructure cannot verify whether the generated code is correct.
   A patch will appear to change the score without validating anything real.

3. **"Treating PRESCRIPTIVE as success"** — a score of 0.95 may mean the context
   package is blocking valid correct implementations. Check for over-specification
   before filing it as done.

4. **"Acting before recording the interpretation"** — if the session is interrupted
   after a patch starts but before it completes, an unrecorded interpretation means
   the next cycle has no baseline. Record first.

5. **"Using the composite score to decide patch scope"** — the composite aggregates
   five independent signals. A low composite from a single low sub-score needs a
   targeted sub-score intervention, not a full composite patch.

---

## Integration

- prompt-patch-authoring-SKILL.md receives DETAIL_GAP routing from this skill
- score-zero-investigation-SKILL.md receives machine_constant < 0.85 internal investigation routing
- meta-arbiter-SKILL.md uses this skill's bracket classification to produce proposals
- planning-dpo-authoring-SKILL.md records interpretation as part of CALIBRATION_COMPARISON signal
- escalation-orchestrator uses the bracket to determine PATCH / REWRITE / ESCALATE routing
