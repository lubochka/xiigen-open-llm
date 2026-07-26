# Confidence Seeding

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

Seeding all graph edges at 0.5 (neutral prior) throws away prior knowledge.
A capability class validated by 10 prior phases should seed at 0.8, not 0.5.
A capability that has never been built should seed at 0.1, not 0.5. Uniform
seeding means the learning loop starts over for every edge regardless of evidence.
This skill defines evidence-based seeding formulas, decay protection windows, and
the graduation thresholds per risk tier.

---

## When to Invoke

- After graph-entity-schema-SKILL.md designs the edges for a new capability
- Before writing any DomainModelRegistry seed entry
- When confidence-lifecycle-SKILL.md Section 2 says "see confidence-seeding-SKILL.md for seed values"
- When a capability class is first added to DomainModelRegistry

---

## Section 1 — Evidence-Based Seeding Formula

Initial confidence comes from evidence, not neutral assumption:

| Source | Initial confidence | Formula |
|---|---|---|
| No prior work (first time this capability class is built) | 0.1 | baseline |
| Prior validated phases of this capability class | min(0.8, 0.1 + N × 0.04) | N = count of prior phases with Gate B = PASS |
| Architectural invariant (DNA rule boundary) | 1.0, immutable=true | no learning |

**Examples with current project capabilities:**
- `HybridRetriever` with 3 prior validated phases: `0.1 + 3 × 0.04 = 0.22` → seed at 0.22
- `WeightedScoring` with 12 prior validated phases: `0.1 + 12 × 0.04 = 0.58` → seed at 0.58
- `BM25Scorer` with 24 prior validated phases: `min(0.8, 0.1 + 24 × 0.04) = 0.8` → seed at 0.80
- `OperationResult<T> boundary` (DNA Rule 1): seed at 1.0, immutable=true

Never seed at 1.0 unless immutable. Always room to learn.

---

## Section 2 — Confidence Update Delta

Per-observation updates (stored in FREEDOM config — never hardcoded):

| Edge type | Default delta | Rationale |
|---|---|---|
| Standard learnable edge | ±0.05 | Balanced — 20 observations to move 1.0 |
| AVOID edge (anti-pattern) | +0.06 per violation | Stronger — violations are clear signals |
| PREFERRED pattern edge | ±0.03 | Slower — pattern changes are higher risk |
| Immutable edge (DNA rule) | 0.0 (no update) | Not learnable |

Update rule:
```
outcomeWasPositive = true  → confidence += delta
outcomeWasPositive = false → confidence -= delta
confidence = Math.Clamp(confidence, 0.0, 1.0)
```

What constitutes "positive" and "negative" is decision-specific. Define
outcome attribution per edge type when creating the graph entity schema.

---

## Section 3 — Decay Protection Window

Decay protection prevents a small burst of outlier observations from overriding
accumulated evidence:

```
decayWindow = Math.Max(5, (int)Math.Floor(observationCount / 4))
```

**The window grows with evidence:**
- Edge with 3 observations: window = 5 (minimum — still protected)
- Edge with 20 observations: window = 5 (minimum still)
- Edge with 50 observations: window = 12 (harder to override)
- Edge with 100 observations: window = 25 (substantial stability required to change)

**Snapshot constraint:**
```
|currentConfidence - max(historicalSnapshots.confidence)| ≤ 0.20
UNLESS evidenceSinceSnapshot > snapshot.observationCount
```

Effect: 5 outlier runs cannot move confidence more than ±0.20 from the
highest historical snapshot. Only proportional counter-evidence — more
observations than the snapshot accumulated — can override.

```csharp
public OperationResult<double> ApplyDecayProtection(
    double newConfidence,
    double[] historicalSnapshots,
    int currentObservationCount,
    int snapshotObservationCount)
{
    // Decay window grows with accumulated evidence
    int decayWindow = Math.Max(5, currentObservationCount / 4);
    double maxHistorical = historicalSnapshots.Max();

    // Check snapshot constraint
    bool sufficientCounterEvidence = currentObservationCount > snapshotObservationCount;
    bool withinBand = Math.Abs(newConfidence - maxHistorical) <= 0.20;

    if (!withinBand && !sufficientCounterEvidence)
    {
        // Clamp to ±0.20 band — not enough counter-evidence to override
        double clamped = newConfidence > maxHistorical
            ? maxHistorical + 0.20
            : maxHistorical - 0.20;
        return OperationResult<double>.Ok(Math.Clamp(clamped, 0.0, 1.0));
    }

    return OperationResult<double>.Ok(Math.Clamp(newConfidence, 0.0, 1.0));
}
```

**Do NOT apply decay protection to AVOID edges.** AVOID edges should strengthen
monotonically from violations. A violation is never an outlier — it is always
evidence that the anti-pattern is real.

---

## Section 4 — Graduation Thresholds by Risk Tier

When confidence reaches the risk-tier threshold, the AI pipeline is bypassed
entirely for that edge — the graph answers directly:

| Risk tier | Threshold | Applies to |
|---|---|---|
| LOW_RISK | 0.75 | Model selection, cycle budget, feature routing |
| MEDIUM | 0.85 | Arbiter panel selection, capability class routing |
| HIGH_RISK | 0.95 | Score routing, promotion decisions, escalation triggers |
| NEVER | 1.0, immutable | DNA rule boundaries — never bypassed by AI |

These thresholds are FREEDOM config keys (never hardcoded in C#):
- `engine.graph.lowRiskThreshold` = 0.75
- `engine.graph.mediumRiskThreshold` = 0.85
- `engine.graph.highRiskThreshold` = 0.95

---

## Section 5 — Seed Entry Format

Every DomainModelRegistry edge must use this seed format:

```csharp
new CapabilityEdge
{
    FromEntity = "CapabilityClass:WeightedScoring",
    Relationship = "REQUIRES",
    ToEntity = "ConfigKey:MatchWeight",

    // Evidence-based seeding — NOT neutral 0.5
    Confidence = 0.58,            // 12 prior validated phases × 0.04 + 0.1
    ObservationCount = 0,         // no runtime observations yet
    Immutable = false,            // can be learned away

    Source = "bootstrap",         // "bootstrap" | "discovered" | "manual"
    Reasoning = "Seeded from 12 prior WeightedScoring phases — confidence earned, not assumed"
}
```

---

## Anti-Patterns

1. **"Seed all edges at 0.5 to be neutral"** — 0.5 is not neutral, it is uninformed.
   Use the evidence-based formula. A capability with 24 validated phases deserves 0.8.

2. **"Seed confidence delta at 0.20 per observation"** — 3 outlier runs would shift
   confidence by 0.6, past any reasonable threshold. Use 0.05 (standard) or 0.03 (slow).

3. **"Set delta to 0.001 for safety"** — would take 500 observations to reach promotion
   threshold. Learning stalls. The learning loop becomes decorative.

4. **"Apply decay protection to AVOID edges"** — AVOID edges should always strengthen
   from violations. No protection, no decay window.

5. **"Hardcode promotion thresholds in C#"** — thresholds are FREEDOM config (IConfiguration).
   `if (confidence >= 0.85)` is a Rule 10 violation analogue. Use `_config.GetValue<double>("engine.graph.mediumRiskThreshold")`.

---

## Integration

- graph-entity-schema-SKILL.md Section 3 (seed values) cites this skill for seeding formulas
- confidence-lifecycle-SKILL.md Section 2 (observation-based update) uses the delta values from Section 2
- DomainModelRegistry is the persistence layer that stores confidence, observationCount, and snapshots
- learning-loop-closure-SKILL.md Event 3 (confidence updated) triggers the update formula from Section 2
- ai-pipeline-design-SKILL.md Section 1 (when AI activates) uses the risk-tier thresholds from Section 4
