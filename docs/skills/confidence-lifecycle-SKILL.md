# Confidence Lifecycle

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

A capability that stays in AI-pipeline mode forever accumulates cost without
reducing it. A capability that graduates to direct lookup too early makes decisions
based on insufficient evidence. This skill defines the 5-phase lifecycle that
every capability in `ContinuousLearningPipeline` follows, from initial seeding
through graduation to autonomous decision-making.

---

## When to Invoke

- When designing a new capability for `ContinuousLearningPipeline`
- When a capability is being promoted from STUB to ACTIVE
- When `DomainModelRegistry` entries are being seeded
- Before writing any promotion threshold in code
- When a capability has accumulated N observations and graduation is being considered

---

## Section 1 — The 5 Phases

### Phase 1 — Seeding

Initial confidence comes from evidence, not assumption.

| Evidence source | Initial confidence |
|---|---|
| Designed but no validation runs | 0.1 |
| N validated test runs | min(0.8, 0.1 + N × 0.04) |
| Architectural invariant (DNA rule) | 1.0, immutable: true |

Examples:
- New capability, no test history: confidence = 0.1
- Capability with 10 passing runs: confidence = min(0.8, 0.1 + 0.40) = 0.5
- Capability with 20+ passing runs: confidence = 0.8 (capped)
- DNA Rule 1 (OperationResult<T>): confidence = 1.0, immutable

**Rule:** Never seed at 1.0 unless immutable. There is always room to learn.

### Phase 2 — Learning (per observation)

```
observation correct   → confidence += delta
observation incorrect → confidence -= delta

Default deltas (FREEDOM config, never hardcoded):
  ContinuousLearning:ConfidenceDelta            default: 0.05  (standard)
  ContinuousLearning:AvoidPatternDelta          default: 0.06  (violations — stronger signal)
  ContinuousLearning:ModelSelectionDelta        default: 0.03  (slower — higher risk)
```

**What constitutes "correct"** is decision-specific. For every capability:
- Define the positive outcome explicitly in the capability spec
- Define the negative outcome explicitly
- Do not infer outcomes from absence of failure — explicit observation required

### Phase 3 — Promotion

```
OPTIONAL   → ACTIVE        when observationCount ≥ optionalToActiveThreshold
ACTIVE     → AUTHORITATIVE when observationCount ≥ activeToAuthoritativeThreshold

FREEDOM config keys:
  ContinuousLearning:OptionalToActiveThreshold        default: 3
  ContinuousLearning:ActiveToAuthoritativeThreshold   default: 5
```

Promotion fires after each relevant observation.
Returns `PROMOTED | AUTHORITATIVE | UNCHANGED` — logged in `DomainModelRegistry`.

### Phase 4 — Decay Protection

Accumulated evidence protects against outlier observations:

```
Decay window = max(5, floor(currentObservationCount / 4))
  — grows with accumulated evidence, not fixed at bootstrap time
  — capability with 50 observations: window = 12 (harder to override)
  — capability with 3 observations:  window = 5  (minimum, still protected)

Snapshot constraint:
  |currentConfidence - max(snapshots[].confidence)| ≤ 0.2
  UNLESS evidenceSinceSnapshot > snapshot.observationCount

Effect: 5 outlier observations cannot move confidence more than ±0.2 from the
highest historical snapshot. Only proportional counter-evidence can override.
```

### Phase 5 — Graduation

When confidence ≥ graduation threshold:
- Bypass `ISemanticCompiler29` pipeline entirely
- Read `DomainModelRegistry` directly (no AI call)
- No DPO pair produced — the model is confident, nothing to learn
- Cost drops to zero for this decision

**Risk-tier graduation thresholds (FREEDOM config):**

| Risk tier | Decision examples | Threshold |
|---|---|---|
| LOW RISK | Model selection, cycle budget | 0.75 |
| MEDIUM | Algorithm weights, quality gate selection | 0.85 |
| HIGH RISK | Capability routing, escalation decisions | 0.95 |
| NEVER | DNA rules, architectural invariants | 1.0, immutable |

---

## Section 2 — Immutable Registry Entries

Some entries must never change regardless of observations:

```csharp
// These are registered with Confidence = 1.0, Immutable = true
// No learning, no decay, no promotion evaluation
var invariants = new[]
{
    // DNA Rule 1: OperationResult<T> required — architectural invariant
    new DomainModelEntry("DNA.Rule1.OperationResult", confidence: 1.0, immutable: true),
    // DNA Rule 2: Fabric-first — never override
    new DomainModelEntry("DNA.Rule2.FabricFirst", confidence: 1.0, immutable: true),
    // DNA Rule 10: No external ML frameworks
    new DomainModelEntry("DNA.Rule10.NoExternalML", confidence: 1.0, immutable: true),
};
```

---

## Section 3 — Capability Lifecycle Record

Track each capability's lifecycle in `DomainModelRegistry`:

```json
{
  "capabilityId": "[CapabilityName]",
  "currentPhase": "ACTIVE",
  "confidence": 0.72,
  "observationCount": 14,
  "promotionHistory": [
    { "from": "OPTIONAL", "to": "ACTIVE", "at": "2026-05-01", "observationCount": 3 }
  ],
  "snapshotHistory": [
    { "confidence": 0.5, "observationCount": 5, "date": "2026-04-15" },
    { "confidence": 0.72, "observationCount": 14, "date": "2026-05-01" }
  ],
  "graduated": false,
  "graduationThreshold": 0.85
}
```

---

## Anti-Patterns

1. **"Seed all capabilities at 0.1"** — a capability with 10 validated runs deserves 0.5, not 0.1. Under-seeding slows learning.

2. **"Hardcode threshold at 5 observations"** — `if (observationCount >= 5)` is a FREEDOM violation. Always read from `IConfiguration`.

3. **"Confidence delta = 0.2"** — 5 observations would shift confidence by 1.0 in one direction. Delta should be small (0.03–0.06).

4. **"Graduate capability after 2 correct runs"** — below minimum threshold, the evidence base is too thin. Trust the phase table.

5. **"Apply decay protection to violation patterns"** — violations should strengthen monotonically. A violation is evidence, not an outlier.

---

## Integration

- `ContinuousLearningPipeline` implements the observe/promote/graduate cycle
- `DomainModelRegistry` stores the lifecycle record per capability
- `IncrementalTrainer` triggers Phase 2 (learning) after each training run
- four-tier-decision-classification-SKILL.md classifies whether a decision is LEARNABLE (Phase 2–5 apply) or BOUNDARY CODE (immutable)
- training-data-gap-audit-SKILL.md ensures training pairs are RECOVERABLE before Phase 2 can fire
