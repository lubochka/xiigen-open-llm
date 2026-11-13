# Algorithm-as-Service Pattern

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

Algorithms that hardcode scoring weights as constants are not configurable by operators.
Algorithms that bypass the fabric (directly writing to storage, directly calling
external ML libraries) violate Rule 2 and Rule 10. This skill defines the 5 algorithm
classes that appear in this project and specifies the required iron rules, fabric
interfaces, and test strategy for each.

---

## When to Invoke

- When designing any capability that computes a composite score, ranking, or recommendation
- When a capability has `if/else` logic over numeric thresholds
- Before writing any class with weights, decay functions, similarity scores, or point values
- When four-tier-decision-classification-SKILL classifies a decision as FREEDOM CONFIG or LEARNABLE

One classification before implementation = no hardcoded weights, no Rule 10 violations.

---

## Section 1 — The 5 Algorithm Classes

### Class 1 — Weighted Scoring (most common)

**Pattern:** Takes N input signals, applies configurable weights, produces a composite score
in [0.0, 1.0].

**Examples in this project:** BM25 + vector hybrid search score merger, capability quality
scoring (weighted rule pass rates), session quality scoring.

**Fabric interface:** Not applicable — pure computation; no external I/O.
**Configuration:** All weights are FREEDOM config via `IConfiguration.GetValue<double>(key, default)`.

**Required iron rules (violations score 0):**
1. `weights_from_config`: every weight reads from `IConfiguration`, never a literal decimal
2. `weights_sum_to_one`: verify sum ≈ 1.0 at construction (allow ±0.05 tolerance)
3. `score_range_zero_to_one`: output is clamped to [0.0, 1.0]
4. `score_denominator_not_zero`: guard against division by zero when all signals are 0

**C# IConfiguration pattern:**
```csharp
// ✅ CORRECT: weight from FREEDOM config
var matchWeight = _config.GetValue<double>("scoring.search.matchWeight", 0.25);

// ❌ WRONG: hardcoded weight (score-0 violation)
const double matchWeight = 0.25;
```

**Test strategy (MACHINE constant test):**
```csharp
[Fact]
public void WeightedScorer_UsesConfigWeight_NotHardcodedValue()
{
    // Set a non-default weight that differs from any default constant
    _config["scoring.search.matchWeight"] = "0.40"; // default would be 0.25
    var score = _scorer.Compute(signals);
    // If hardcoded: score uses 0.25 regardless; if config-driven: score uses 0.40
    Assert.NotEqual(expectedWithDefault025, score);
}
```

---

### Class 2 — Temporal Decay

**Pattern:** Score decreases as time since the event increases. Used in feed ranking,
relevance decay, content freshness.

**Fabric interface:** No external I/O — pure computation on timestamps.
**Configuration:** Decay rate, floor, and boost factors are FREEDOM config.

**Required iron rules:**
1. `decay_rate_from_config`: half-life or decay rate reads from `IConfiguration`
2. `decay_never_below_floor`: floor is configurable, minimum value enforced
3. `decay_idempotent`: computing decay twice on same inputs = same result
4. `timestamp_not_datetime_now`: uses event timestamp from input, never `DateTime.UtcNow` directly in decay formula

**IConfiguration pattern:**
```csharp
var halfLifeHours = _config.GetValue<double>("scoring.decay.search.halfLifeHours", 24.0);
var floor = _config.GetValue<double>("scoring.decay.search.floor", 0.05);
```

---

### Class 3 — Recommendation (adaptive ML)

**Pattern:** Learns from feedback (user interactions, test results) to improve future outputs.
The model weights UPDATE based on observed behavior.

**Key distinction from Class 1:** Weights in Class 1 are static (but configurable). In Class 3,
weights change based on training data through `ContinuousLearningPipeline`.

**Fabric interface required:** `ContinuousLearningPipeline` for model updates. Never call
external ML libraries directly (Rule 10).

**Required iron rules:**
1. `training_via_fabric`: model updates go through `ContinuousLearningPipeline`, not direct
2. `recommendation_per_context`: output is context-specific (per session / per domain), not global
3. `feedback_loop_wired`: `IncrementalTrainer` receives feedback after each output

**Architectural note:** `ContinuousLearningPipeline` is this project's ML layer.
It is separate from the ISemanticCompiler29 code generation pipeline. Do NOT wire
training data for code generation to domain recommendation training data.

---

### Class 4 — Matching (multi-criteria similarity)

**Pattern:** Takes two entities and computes similarity across N dimensions. Used in
K-NN (K-Nearest Neighbours) search, semantic similarity, document comparison.

**Fabric interface:** `IVectorIndex` for vector-based matching; `IBm25Scorer` for
term-frequency matching.

**Required iron rules:**
1. `match_score_symmetric`: `score(A,B) == score(B,A)` for all valid inputs
2. `match_score_bounded`: score ∈ [0.0, 1.0]
3. `match_weights_from_config`: dimension weights from `IConfiguration`
4. `match_never_self`: if `entityA.Id == entityB.Id`, return 0.0 (no self-match)

**Symmetry test (required):**
```csharp
[Fact]
public void MatchingAlgorithm_IsSymmetric()
{
    var scoreAB = _matcher.Compute(entityA, entityB);
    var scoreBA = _matcher.Compute(entityB, entityA);
    Assert.InRange(Math.Abs(scoreAB.Value - scoreBA.Value), 0.0, 0.001);
}
```

---

### Class 5 — Gamification (points, levels, achievements)

**Pattern:** Awards points for events, computes levels from totals, unlocks achievements
at milestones.

**Fabric interface:** Any gamification service must be behind an `I`-prefixed interface.
Never write directly to storage from the algorithm class.

**Required iron rules:**
1. `points_via_fabric`: points awarded through interface, not direct storage write (Rule 2)
2. `points_idempotent`: same event + idempotency key = same points, never doubled
3. `achievement_check_after_award`: achievement check triggered after every point award
4. `level_derived_not_stored`: level is computed from total points, never stored independently

**IConfiguration pattern for configurable point values:**
```csharp
var pointsForAction = _config.GetValue<int>($"gamification.points.{actionName}", 10);
```

---

## Section 2 — Design Checklist for Any Algorithm Capability

Before writing the first line of implementation:

```
1. Classify the algorithm (Class 1–5 above)
2. Register all weight/rate config keys in appsettings.json FREEDOM section
3. Add class-appropriate iron rules (see Section 1 per class)
4. Write the MACHINE constant test BEFORE writing the implementation
   (the test that proves config-driven weights differ from hardcoded defaults)
5. Design the feedback loop: what signals indicate a correct output?
6. Wire to ContinuousLearningPipeline if Class 3 (adaptive ML)
7. Verify weights sum to 1.0 for Class 1 at construction time
```

---

## Section 3 — FREEDOM Config Key Convention

All algorithm weights and rates follow this pattern in `appsettings.json`:

```json
{
  "Scoring": {
    "[Domain]": {
      "[Signal]Weight": 0.25,
      "[Dimension]Weight": 0.30
    }
  },
  "Decay": {
    "[Domain]": {
      "HalfLifeHours": 24,
      "Floor": 0.05
    }
  }
}
```

Keys are PascalCase in C# `IConfiguration` binding. Access with:
```csharp
_config.GetValue<double>("Scoring:Search:MatchWeight", 0.25)
```

---

## Anti-Patterns

1. **"Weight is a constant — it's the right value"** — today's right value is tomorrow's tuning requirement. All weights are FREEDOM config.

2. **"Recommendation doesn't need `ContinuousLearningPipeline`"** — any Class 3 algorithm that updates weights without going through the training pipeline has bypassed Rule 2 and Rule 10.

3. **"Matching is deterministic, no test needed"** — the symmetry and self-match tests are structural, not probabilistic. They are required.

4. **"The algorithm outputs the right value, DNA compliance is fine"** — Rule 1 (OperationResult<T>) applies to every method, including algorithm compute methods.

---

## Integration

- four-tier-decision-classification-SKILL.md classifies each decision point in an algorithm as BOUNDARY CODE, FREEDOM CONFIG, LEARNABLE, or AI PIPELINE
- freedom-machine-SKILL.md verifies FREEDOM/MACHINE classification per algorithm parameter
- training-data-gap-audit-SKILL.md identifies training data gaps when Class 3 capabilities graduate from STUB to ACTIVE
- capability-adaptation-map-SKILL.md provides expected score ranges per algorithm class
