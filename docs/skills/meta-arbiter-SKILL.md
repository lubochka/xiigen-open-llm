# Meta-Arbiter

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

An AI pipeline that produces a score below threshold will keep producing below-threshold
scores if the same inputs are fed back in. The Meta-Arbiter fires when a score fails
and diagnoses which layer caused the failure: retrieval (wrong skills loaded), assembly
(context package incomplete), prompt effectiveness (model did not follow instructions),
or skill content (skill is wrong or missing a case). It then produces a concrete proposal
for the affected layer — PROMPT_PATCH, SKILL_EDIT, or SKILL_ADD — and routes it to Luba
for approval before any change is applied.

---

## When to Invoke

```
SITUATION 1 — Single bad score:
  score < acceptanceThreshold (first occurrence)
  → Diagnosis runs; one proposal produced

SITUATION 2 — Repeated failure pattern:
  score < acceptanceThreshold AND same capability class failed N ≥ 3 times
  → Diagnosis runs across all N executions; up to 3 proposals ranked

SITUATION 3 — Regression:
  score < acceptanceThreshold AND score < prior score for this capability class
  → Regression diagnosis runs FIRST — something changed and made it worse
```

The Meta-Arbiter does NOT fire when:
- score ≥ acceptanceThreshold (even if score dropped slightly from prior run)
- score < threshold on the very first execution of a capability class
  (no prior art — Situation 1 applies but regression check skips)

---

## Section 1 — Execution Record

The Meta-Arbiter receives one execution record per failing cycle:

```csharp
public record MetaArbiterInput
{
    public string ExecutionId { get; init; }
    public string CapabilityClass { get; init; }      // e.g. "WeightedScoring"

    public double Score { get; init; }
    public double AcceptanceThreshold { get; init; }
    public double ScoreDelta { get; init; }            // negative = regression

    public RetrievalRecord Retrieval { get; init; }
    public PromptRecord Prompt { get; init; }
    public ArbiterVerdictSummary ArbiterVerdicts { get; init; }

    public List<double> PriorScores { get; init; }     // empty on first run
    public List<RecentChange> RecentChanges { get; init; }
}

public record RetrievalRecord
{
    public string Query { get; init; }
    public List<string> SkillsRetrieved { get; init; }
    public List<string> SkillsExpected { get; init; }
    public List<string> RetrievalGap { get; init; }    // expected − retrieved
}

public record PromptRecord
{
    public bool SelfCheckPresent { get; init; }
    public int IronRulesCount { get; init; }
    public bool AllContextSectionsPresent { get; init; }  // INTENT/DOMAIN/CONSTRAINTS/PRIOR_ART/SUCCESS
}

public record ArbiterVerdictSummary
{
    public string IronRules { get; init; }      // e.g. "BLOCK — Rule 1 violated at line 47"
    public string Principles { get; init; }     // e.g. "PASS"
    public string BusinessLogic { get; init; }  // e.g. "CONCERN — missing null check"
}

public record RecentChange
{
    public string ChangeType { get; init; }     // "SKILL_EDIT" | "PROMPT_PATCH" | "SKILL_ADD"
    public string Target { get; init; }
    public DateTime ChangedAt { get; init; }
}
```

---

## Section 2 — Four-Step Diagnosis Protocol

Run steps in order. Stop at the first step that identifies the failure layer.
Do not run all 4 steps and average — the first match is the root cause.

### Step 1 — Check retrieval

Were the right skills loaded for this capability class?

```
Compare:
  execution.Retrieval.SkillsRetrieved
  vs
  execution.Retrieval.SkillsExpected

If RetrievalGap is non-empty:
  → Failure layer: RETRIEVAL
  → Note the gap; continue to Step 2 (retrieval gap may compound a skill gap)

If RetrievalGap is empty:
  → Retrieval not the primary cause → Step 2
```

### Step 2 — Check prompt assembly

Was the context package complete?

```
Check:
  execution.Prompt.SelfCheckPresent
  execution.Prompt.AllContextSectionsPresent
  execution.Prompt.IronRulesCount vs expected minimum for this capability class

If SelfCheckPresent = false:
  → Failure layer: ASSEMBLY — model had no instruction to self-review output
  → Proposal type: PROMPT_PATCH (add QUESTION YOURSELF section)
  → STOP — this is the primary cause

If AllContextSectionsPresent = false:
  → Failure layer: ASSEMBLY — context package missing required section
  → Proposal type: PROMPT_PATCH (add missing section)
  → STOP

If IronRulesCount < expectedMinimum for this capability class:
  → Failure layer: ASSEMBLY — constraint layer incomplete
  → Proposal type: PROMPT_PATCH (add missing iron rules)
  → STOP

If all checks pass → Step 3
```

### Step 3 — Check prompt effectiveness

Did the model follow the instructions it was given?

```
Read ArbiterVerdicts:

If IronRules = "BLOCK":
  Distinguish two sub-cases:
    A) Iron rule stated vaguely in prompt:
       → Failure layer: PROMPT — guidance needs a concrete WRONG/CORRECT pair
       → Proposal type: PROMPT_PATCH (add paired example)
       → STOP

    B) Iron rule stated correctly but model ignored it:
       → Proceed to Step 4 (skill content problem, not prompt problem)

  How to distinguish A from B:
    Read the iron rule as stated in the context package
    □ Is it stated without .NET type names?
    □ Does it have a testable failure consequence?
    □ Does it derive from a specific failure mode?
    If ANY check fails → Sub-case A → PROMPT_PATCH
    If ALL pass → Sub-case B → Step 4

If BusinessLogic = "CONCERN":
  → Model partially satisfied the constraint — prompt guidance was too weak
  → Failure layer: PROMPT — specificity insufficient
  → Proposal type: PROMPT_PATCH (increase specificity with an example)
  → STOP

If model candidates were clustered (all scores within 0.05 of each other):
  → Prompt over-specified — all models produced the same output
  → No meaningful choice available for the arbiter
  → Failure layer: PROMPT — over-specified, reducing model contribution
  → Proposal type: PROMPT_PATCH (reduce specificity)
  → STOP

If no prompt effectiveness signal found → Step 4
```

### Step 4 — Check skill content

Was the governing skill wrong, incomplete, or missing a case?

```
Only runs when Steps 1-3 found no primary cause.

Check for regression first:
  If RecentChanges contains a SKILL_EDIT on a skill that governs this capability class:
    AND ScoreDelta is negative (score dropped since the edit):
    → The edit introduced a regression
    → Proposal type: SKILL_EDIT (revert or correct the recent change)
    → Priority: HIGHEST — regression proposals are ranked first

Check for missing case:
  The arbiter named a specific violation
  Find which skill governs this constraint for this capability class
  Read that skill's content for this constraint
  If the skill does not cover this case or covers it incorrectly:
    → Failure layer: SKILL_CONTENT
    → Proposal type: SKILL_EDIT (add missing case or correct wrong guidance)

Check for missing skill:
  If no skill in SkillsRetrieved governs the constraint that was violated:
    → Failure layer: SKILL_MISSING
    → Proposal type: SKILL_ADD (new skill needed for this constraint)
```

---

## Section 3 — Proposal Format

### PROMPT_PATCH

```csharp
public record PromptPatchProposal
{
    public string ProposalType { get; init; }      // "PROMPT_PATCH"
    public int Situation { get; init; }            // 1, 2, or 3
    public string DiagnosisLayer { get; init; }    // "ASSEMBLY" or "PROMPT"
    public int DiagnosisStep { get; init; }        // 2 or 3
    public double Confidence { get; init; }

    public string TargetSection { get; init; }     // e.g. "QUESTION_YOURSELF" | "iron_rules"
    public string TargetAction { get; init; }      // "add" | "strengthen" | "weaken" | "replace"

    public string WhatModelDid { get; init; }      // exact pattern from failing output
    public string WhatWentWrong { get; init; }     // one sentence

    public string WrongExample { get; init; }      // exact wrong pattern
    public string CorrectExample { get; init; }    // what model should produce instead
    public string ReasonWrong { get; init; }       // one sentence
    public string ReasonCorrect { get; init; }     // one sentence

    public bool RegressionRiskLow { get; init; }   // false = flag for Luba review
}
```

**One bottleneck per patch rule:** A PROMPT_PATCH may target exactly one section.
A patch that targets two sections simultaneously cannot be attributed — the grade
change cannot be isolated to either fix. Split into two separate proposals.

### SKILL_EDIT

```csharp
public record SkillEditProposal
{
    public string ProposalType { get; init; }      // "SKILL_EDIT"
    public int Situation { get; init; }
    public string DiagnosisLayer { get; init; }    // "SKILL_CONTENT"
    public double Confidence { get; init; }

    public string TargetSkill { get; init; }       // e.g. "docs/skills/iron-rule-derivation-SKILL.md"
    public string TargetSection { get; init; }     // section title
    public string TargetAction { get; init; }      // "add" | "edit" | "cut"

    public string WhatSkillSays { get; init; }     // current content verbatim
    public string WhatExecutionShowed { get; init; }
    public string GapOrError { get; init; }        // what skill should say but doesn't

    public string ProposedContent { get; init; }
    public string PositiveExample { get; init; }
    public string NegativeExample { get; init; }
    public string WhyThisFixesIt { get; init; }    // one sentence

    public bool EscalationRequired { get; init; }  // true for foundational skills
    public string EscalationReason { get; init; }
}
```

### SKILL_ADD

```csharp
public record SkillAddProposal
{
    public string ProposalType { get; init; }      // "SKILL_ADD"
    public int Situation { get; init; }            // always Situation 2 (pattern)
    public string DiagnosisLayer { get; init; }    // "SKILL_MISSING"

    public string ProposedSkillName { get; init; } // kebab-case filename
    public List<string> CapabilityClasses { get; init; }

    public string GapDescription { get; init; }
    public int ExecutionsAffected { get; init; }
    public string Pattern { get; init; }

    public string ProposedSections { get; init; }  // section titles
    public string MinimumContent { get; init; }    // what skill must contain to close the gap
}
```

---

## Section 4 — Two-Call Execution

The Meta-Arbiter runs as two sequential AI calls. Call 2 does NOT see Call 1's reasoning.

**Call 1 — Diagnosis and proposal generation**
- Receives: execution record + governing skills + this skill
- Task: run 4-step diagnosis, produce proposals, do NOT rank
- Output: unranked proposals + diagnosis trace

**Call 2 — Blind ranking**
- Receives: proposals (labeled A/B/C only, no proposal type visible) + execution record
- Does NOT receive: Call 1's diagnosis reasoning
- Task: rank proposals by likely impact on the score
- Output: ranked proposals with impact scores and regression risk per proposal

**Luba receives:**
1. Execution record summary (score, capability class, what failed)
2. Ranked proposals (highest impact first)
3. Diagnosis trace (which step found the failure layer)
4. Escalation flags (if any proposal requires architectural review)

**Luba's response options per proposal:**

| Response | Effect |
|---|---|
| APPROVED | System applies the change |
| REJECTED | Proposal stored with rejection reasoning (becomes training signal) |
| DEFERRED | Revisit after N more executions of same capability class |
| ESCALATE | Route to architectural review before any decision |

---

## Section 5 — Escalation Triggers

Escalate before ranking when:

```
□ Proposal targets a foundational skill:
  docs/skills/arbiter-panel-design-SKILL.md
  docs/skills/principles-arbiter-SKILL.md
  docs/skills/iron-rule-derivation-SKILL.md
  docs/skills/session-file-authoring-SKILL.md
  → These govern every capability. A wrong edit propagates everywhere.

□ Proposal affects > 3 other skills via dependency edges
  → Cascade risk — architectural review required before ranking

□ Proposal contradicts a CLAUDE.md rule (Rules 1-10) or locked decision (D-NN)
  → The Meta-Arbiter states which rule is contradicted and why
  → No proposal may be approved without architectural review if it contradicts Rules 1-10

□ Same proposal type has fired on the same skill 3+ times without approval
  → The pattern suggests the approval process has a gap, not the skill
  → Escalate the pattern
```

---

## Anti-Patterns

1. **"Call 1 and Call 2 use the same model"** — the ranker must be a different model.
   A model that generated the diagnosis will rank its own proposals highest.
   Call 2 must use a different model, and Call 2 must not see Call 1's reasoning.

2. **"Proposal applied without waiting for Luba's APPROVED response"** — every proposal
   starts as PROPOSED. Nothing changes until APPROVED. No self-approval.

3. **"Diagnosis stopped at Step 1 and produced a SKILL_EDIT"** — a retrieval gap means
   the skill may exist and be correct but is not being loaded. Fix the retrieval path,
   not the skill content. Note the gap in Step 1 and continue to Step 2.

4. **"5 proposals for a single below-threshold score"** — Situation 1 (single occurrence)
   produces one proposal. Situation 2 (N ≥ 3 pattern) produces up to 3. More proposals
   than the situation allows means diagnosis was not focused.

5. **"PROMPT_PATCH that targets two sections"** — one bottleneck per patch. Split.

---

## Integration

- ai-pipeline-design-SKILL.md defines the pipeline whose scores this skill monitors
- escalation-orchestrator-SKILL.md Rule 5 (ESCALATE_TO_HUMAN) handles proposals that need Luba
- learning-loop-closure-SKILL.md: rejected proposals with reasoning become training signal
- dna-compliance-guard-SKILL.md governs what counts as a Rule 1-10 violation in ArbiterVerdicts
- decision-reopening-SKILL.md handles proposals that contradict locked decisions (D-NN)
