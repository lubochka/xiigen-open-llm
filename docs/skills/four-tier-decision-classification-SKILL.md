# Four-Tier Decision Classification

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

## When to Invoke

Before designing any class, service, or component that makes a decision:
any `if/else`, `switch`, lookup table, threshold comparison, enum-to-value
mapping, or conditional branch. Supersedes freedom-machine-SKILL.md for
NEW components by adding LEARNABLE and AI PIPELINE tiers.

---

## What This Skill Prevents

- Hardcoding a value that should learn from training data (e.g., baking
  k1/b BM25 defaults as constants when they could tune per corpus)
- Putting a LEARNABLE decision in `IConfiguration` (it gets a value but
  never improves with more data)
- Building a full IAIDecisionPipeline for a decision that is genuinely
  static and well-understood

---

## The Four-Tier Test

For every decision point, run these four questions IN ORDER. Stop at first YES.

```
Q1: Does changing this value break a system guarantee AND require a code change?
    YES → BOUNDARY CODE (Layer 1)
    Hardcode as a const or DNA invariant. Abstract interface + DI wiring.
    .NET examples:
      - OperationResult<T> contract (always .Ok / .Fail — never throw)
      - IBm25Scorer.Score() return type (always double — never null)
      - IVectorIndex.Search() result ordering contract (invariant by DNA)
    → STOP. See Rule 1 (OperationResult everywhere) + Rule 2 (Fabric First).

Q2: Might different deployments or users reasonably want different values?
    YES → FREEDOM CONFIG (Layer 2)
    IConfiguration key with a safe default. Changeable without code change.
    .NET examples:
      - BM25 k1 and b parameters (default 1.5 / 0.75 in appsettings.json)
      - HNSW M and efConstruction (default 16 / 200)
      - Vector embedding dimensions (default 768)
      - Model temperature or max_tokens for generation calls
    → STOP. Use IConfiguration + FREEDOM annotation in code comment.

Q3: Could the correct answer improve if the system observed more outcomes?
    YES → LEARNABLE (Layer 3)
    Backed by training data + ContinuousLearningPipeline.
    .NET examples:
      - DPO (Direct Preference Optimization) triple selection weights
      - Ranking function score weights learned from user feedback
      - Domain model confidence thresholds tuned by DomainModelRegistry
      - IncrementalTrainer LoRA parameters improved over fine-tuning rounds
    → Continue to ContinuousLearningPipeline + DomainModelRegistry design.

Q4: Does answering this require contextual reasoning beyond pattern matching?
    YES → AI PIPELINE (Layer 4)
    Inference call through the model + optional arbiter challenge.
    .NET examples:
      - Novel code structure not covered by any existing training example
      - Ambiguous user intent requiring multi-step clarification
      - Cross-subproject refactoring decisions that depend on context
    → Route through ISemanticCompiler29 or equivalent inference pipeline.
```

Every Q3 or Q4 decision must also produce a learning signal captured by
ContinuousLearningPipeline for future training rounds.

---

## The Five Learnable Indicators (for Q3)

When Q1 and Q2 are both NO, apply these to determine if the decision is
LEARNABLE or simply an unclassified FREEDOM value:

1. The decision has been wrong before (corrections in git history)
2. The answer differs by corpus or domain (e.g., code vs prose vs schema)
3. The right answer needs evidence from multiple runs, not one observation
4. Domain experts disagree on the default (uncertainty = learning opportunity)
5. A human has had to override the default more than once

If 2+ indicators are YES → LEARNABLE → design for ContinuousLearningPipeline.
If 0-1 indicators → likely FREEDOM CONFIG that hasn't been classified yet.

---

## Classification Table

Produce this table before writing any new service or component:

```
| Decision point                    | Q1  | Q2  | Q3  | Q4  | Tier            |
|-----------------------------------|-----|-----|-----|-----|-----------------|
| OperationResult return convention | YES | —   | —   | —   | BOUNDARY CODE   |
| BM25 k1 and b parameters          | NO  | YES | —   | —   | FREEDOM CONFIG  |
| HNSW M and efConstruction         | NO  | YES | —   | —   | FREEDOM CONFIG  |
| DPO triple selection weight       | NO  | NO  | YES | —   | LEARNABLE       |
| LoRA alpha and rank in fine-tune  | NO  | NO  | YES | —   | LEARNABLE       |
| Novel code intent classification  | NO  | NO  | NO  | YES | AI PIPELINE     |
```

---

## Anti-Patterns

1. **"Every tunable should be config"** — a value that improves with
   observed outcomes belongs in the LEARNABLE tier, not IConfiguration.
   IConfiguration gives a value but never improves it.

2. **"I'll make it learnable later"** — retrofitting DPO signal capture is
   5x harder than designing it in. Classify before implementing.

3. **"Simple enough to hardcode"** — simplicity is not the test. A simple
   threshold that should improve with corpus size IS a learning target.

4. **Classifying Q3 without a measurable outcome** — the LEARNABLE tier
   requires ContinuousLearningPipeline to attribute outcomes. If you
   cannot define what "better" means for this decision, it cannot be Q3.

---

## Connection to Downstream Skills

```
Q1 → BOUNDARY CODE   → Rule 1 + Rule 2 (CLAUDE.md DNA)
Q2 → FREEDOM CONFIG  → freedom-machine-SKILL.md annotation + IConfiguration
Q3 → LEARNABLE       → ContinuousLearningPipeline + DomainModelRegistry design
Q4 → AI PIPELINE     → ISemanticCompiler29 or inference pipeline design
All Q3/Q4            → plan for learning signal capture before implementing
```
