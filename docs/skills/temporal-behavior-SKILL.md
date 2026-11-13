# Temporal Behavior Design

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

A service that computes different outputs at different times relative to an anchor
event is not a SCHEDULED archetype (which runs once at a set time). It is a single
service whose scoring formula is parameterized by time-delta. Without explicit design,
these two are conflated: one creates separate task types per phase (over-decomposition),
another polls `DateTime.UtcNow` on each request (non-reproducible, performance issue).
This skill defines the anchor → phase → weights → formula chain that lets one handler
produce correct output across all time phases.

---

## When to Invoke

- Before designing any scoring or ranking service whose behavior changes based on
  time distance from a reference event
- When a handler reads the current time but has no reference to an anchor event timestamp
- When separate task types are created for "early" vs "approaching" vs "active" phases
  of the same domain concept
- When phase threshold values are hardcoded instead of read from FREEDOM config

---

## Section 1 — Identify the Anchor Event and Phases

```
Anchor event: the fixed reference point that time is measured from
  Examples:
    ConfidencePromotionEvent.PromotionDate
    TrainingRunRecord.ScheduledAt
    CapabilityRecord.FirstActivationDate

Phases: distinct time ranges where behavior is qualitatively different
  Phase A: [creation] → [T minus N days]:       baseline behavior
  Phase B: [T minus N] → [T minus M]:           approach behavior
  Phase C: [T minus M] → [event date]:          imminent behavior
  Phase D: [event date] → [event end]:          active behavior
  Phase E: [event end] → [T plus K days]:       decay behavior
  Phase F: [T plus K] → ∞:                      archive behavior
```

Not all phases are required. Define only the phases where behavior is qualitatively
different — not just quantitatively different (that is a weight change, not a phase
change).

---

## Section 2 — Classify Phase Behavior

For each phase, three entries:

```
Phase A (baseline):
  MACHINE constants: formula structure (which factors combine)
  FREEDOM config:    weighting.phaseA.matchWeight = 0.25
                     weighting.phaseA.proximityWeight = 0.10
  Trigger: default when timeDelta > phase boundary threshold from config

Phase B (approach):
  MACHINE constants: formula structure (same factors as Phase A)
  FREEDOM config:    weighting.phaseB.capacityBoostMultiplier = 1.5
                     weighting.phaseBThresholdDays = 7
  Trigger: ISchedulerService.ScheduleAt at anchor - threshold days
           emits: ApproachPhaseActivated event

Phase C (imminent):
  MACHINE constants: urgency formula (NEW formula, not just different weights)
  FREEDOM config:    weighting.phaseC.proximityWeight = 0.40
  Trigger: ISchedulerService.ScheduleAt at anchor - 1 day
```

**Key design rule:** Phase transition thresholds are FREEDOM config (configurable
per tenant). The formula structure per phase is a MACHINE constant (cannot change
without a code deployment).

---

## Section 3 — State Machine Design

```csharp
// One handler for ALL phases — parameterized by phase
public enum TemporalPhase
{
    Baseline,   // default — no trigger needed
    Approach,   // ISchedulerService triggers transition
    Imminent,   // ISchedulerService triggers transition
    Active,     // ISchedulerService triggers transition
    Decay,      // ISchedulerService triggers transition
    Archive     // ISchedulerService triggers transition
}

public record TemporalContext
{
    public DateTimeOffset AnchorTimestamp { get; init; }  // from payload, not DateTime.UtcNow
    public DateTimeOffset EvaluatedAt { get; init; }      // current evaluation time
    public TemporalPhase Phase { get; init; }             // derived from timeDelta + config
    public TemporalWeights Weights { get; init; }         // from FREEDOM config for this phase
}
```

**Do NOT create a separate handler per phase.** One handler reads the anchor timestamp,
computes the delta, determines the phase from FREEDOM config thresholds, and applies
the phase-appropriate formula. Separate handlers per phase is over-decomposition — the
domain concept is the same entity at different times, not different entities.

---

## Section 4 — Three Iron Rules for Temporal Services

```
IR-TEMPORAL-1: temporal_threshold_from_config
  Phase boundary thresholds must be read from FREEDOM config.
  Hardcoding "7 days" or "24 hours" as a literal violates this rule.
  Named check: temporal_threshold_from_config
  Violation symptom: grep for integer literals adjacent to TimeSpan or
    timeDelta comparisons in non-test code

IR-TEMPORAL-2: anchor_event_timestamp_present
  Time-delta computation must use the anchor event's timestamp from the
  event payload, not DateTime.UtcNow alone. Using only current time makes
  the service non-reproducible — two calls at different times produce
  different results for the same event.
  Named check: anchor_event_timestamp_present
  Violation symptom: ComputeScore(string input) where input has no
    anchor timestamp field; method uses only DateTime.UtcNow

IR-TEMPORAL-3: phase_transition_uses_scheduler
  Phase transitions must be scheduled via ISchedulerService, not detected
  by polling DateTime.UtcNow on each request.
  Named check: phase_transition_uses_scheduler
  Violation symptom: if (DateTime.UtcNow - anchorDate > threshold) inside
    the main execution path instead of in a scheduler registration
```

---

## Section 5 — C# Pattern

```csharp
public class TemporalScoringHandler
{
    private readonly ISchedulerService _scheduler;
    private readonly IFreedomConfig _config;

    public OperationResult<TemporalScore> Execute(TemporalScoreRequest request)
    {
        // IR-TEMPORAL-2: anchor timestamp must come from payload
        var anchorTimestamp = request.AnchorEvent.Timestamp;
        var timeDelta = DateTimeOffset.UtcNow - anchorTimestamp;

        // Phase boundary from FREEDOM config — IR-TEMPORAL-1
        var approachThresholdDays = _config.Get<int>("weighting.phaseBThresholdDays");
        var imminentThresholdHours = _config.Get<int>("weighting.phaseCThresholdHours");

        var phase = DeterminePhase(timeDelta, approachThresholdDays, imminentThresholdHours);
        var weights = GetWeightsForPhase(phase);
        var score = ApplyFormula(request.Factors, weights, phase);

        return OperationResult<TemporalScore>.Ok(new TemporalScore
        {
            Phase = phase,
            Score = score,
            Weights = weights,
            EvaluatedAt = DateTimeOffset.UtcNow
        });
    }

    private TemporalPhase DeterminePhase(TimeSpan delta, int approachDays, int imminentHours)
    {
        // MACHINE constant: the phase determination logic
        if (delta.TotalDays > approachDays) return TemporalPhase.Baseline;
        if (delta.TotalHours > imminentHours) return TemporalPhase.Approach;
        if (delta.TotalHours > 0) return TemporalPhase.Imminent;
        if (delta.TotalDays > -1) return TemporalPhase.Active;
        return TemporalPhase.Decay;
    }
}

// Phase transitions registered at scheduler bootstrap — IR-TEMPORAL-3
public class TemporalPhaseScheduler
{
    private readonly ISchedulerService _scheduler;

    public OperationResult<Unit> ScheduleTransitions(DateTimeOffset anchorDate, string entityId)
    {
        var approachThreshold = _config.Get<int>("weighting.phaseBThresholdDays");
        _scheduler.ScheduleAt(
            anchorDate.AddDays(-approachThresholdDays),
            new ApproachPhaseActivated { EntityId = entityId }
        );
        return OperationResult<Unit>.Ok(Unit.Value);
    }
}
```

---

## Section 6 — Archetype Classification

```
ArchetypeClass:   TEMPORAL (sub-type of DATA_PIPELINE)
CurriculumTier:   3 (same as TRANSACTION — multi-step with time-based side effects)
GeneratorHint:    "This service's output changes based on time-delta from an anchor
                   event. Phase boundaries are FREEDOM config. Formula structure per
                   phase is MACHINE. Use ISchedulerService for phase transitions,
                   not polling. Anchor timestamp comes from the event payload,
                   not from DateTime.UtcNow alone."
```

---

## Anti-Patterns

1. **"I'll create Phase A handler, Phase B handler..."** — one handler parameterized
   by phase is the correct design. Separate handlers duplicate formula logic and make
   phase transitions a handler-routing problem instead of a configuration problem.

2. **"DateTime.UtcNow is the only time I need"** — without the anchor event's timestamp,
   the same event evaluated at different wall-clock times produces different scores.
   Temporal services are reproducible only when both timestamps are present.

3. **"7 days is a business constant, not a config"** — threshold days are FREEDOM config
   because different tenants may need different approach windows. A conference platform and
   a training platform have different urgency timelines for the same concept.

---

## Integration

- freedom-machine-SKILL.md: phase boundary thresholds are FREEDOM; formula structure is MACHINE
- ai-pipeline-topology-SKILL.md: TEMPORAL handlers wire into score.handler position in Pattern C
- arbiter-registration-SKILL.md: add named checks IR-TEMPORAL-1/2/3 to validate.handler registry
  before Phase B
- learning-signal-capture-SKILL.md: TEMPORAL triples carry phase as metadata field so curriculum
  sequencer can group by phase when building fine-tuning batches
