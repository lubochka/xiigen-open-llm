# Capability Routing Gate

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

Implementing a capability as a regular C# class when it should be routed through
`ContinuousLearningPipeline` or `ISemanticCompiler29` accumulates technical debt:
the capability never improves with data, configurations are hardcoded, and it
cannot be traced. This skill defines the four-question test that classifies every
new capability before implementation begins.

---

## When to Invoke

- Before writing ANY new .cs file for a non-trivial capability
- When a plan classifies a capability as "manual C# class"
- Before any new service or algorithm class is designed
- When the question "should this use AI/learning pipeline or plain C#?" arises

---

## Section 1 — The Four-Question Test

Apply these 4 questions in order. The first YES answer determines the routing.

### Q1: Will this capability's output quality improve with more data?

**YES → route through `ContinuousLearningPipeline`**

If the capability produces recommendations, rankings, classifications, or
quality scores that should get better as more examples are observed:
this capability belongs in the learning pipeline.

Examples that say YES:
- Code quality classification
- Document similarity ranking
- Capability promotion decisions

Examples that say NO:
- String formatting utility
- File path computation
- OperationResult<T> wrapper

### Q2: Does this capability branch based on conditions that could change per context or over time?

**YES → route through `ContinuousLearningPipeline` with `IConfiguration` thresholds**

If the branching logic has thresholds that operators might tune (percentages,
counts, score cutoffs), those thresholds are FREEDOM config. The capability that
reads them should be in the learning/config tier, not hardcoded.

Examples that say YES:
- "Escalate if score < X" (X is configurable)
- "Promote if observationCount >= N" (N is configurable)
- "Use advanced model if complexity > threshold" (threshold is configurable)

### Q3: Should this capability appear in `DomainModelRegistry` as a traceable decision unit?

**YES → route through `DomainModelRegistry`**

If the capability makes decisions that should be auditable, reversible,
and improvable over time: it needs registry-level traceability.

### Q4: Is this capability on the bootstrap list?

The bootstrap list contains capabilities that cannot be in the learning pipeline
because they are prerequisites for the learning pipeline itself:

| Capability | Why manual | Notes |
|---|---|---|
| `ContinuousLearningPipeline` core execution | Cannot be a learning task itself | Infrastructure |
| `ISessionStorage` implementation | Must exist before any pipeline run | Infrastructure |
| `DomainModelRegistry` core read/write | Must exist before registry entries can be created | Infrastructure |
| ES index / local file store creation | Must exist before data can be stored | Infrastructure |
| DNA Rule enforcement | Architectural invariant — never learned | Immutable |
| OperationResult<T> | Core type — never learned | Immutable |

**This list is exhaustive. It does not grow without a new locked decision.**

**Q4 YES → plain C# class is acceptable**

If all 4 questions return NO: plain C# utility class (no pipeline involvement required).

---

## Section 2 — Capability Routing Record

For every capability in a phase plan, record the routing decision before Phase 1 begins:

```json
{
  "capabilityRouting": [
    {
      "capability": "SemanticSimilarityScorer",
      "decision": "ContinuousLearningPipeline",
      "reasoning": "Q1=YES: score quality improves with more comparison examples"
    },
    {
      "capability": "PromotionThresholdEvaluator",
      "decision": "IConfiguration",
      "reasoning": "Q2=YES: threshold configurable per deployment"
    },
    {
      "capability": "OperationResultBuilder",
      "decision": "PLAIN_CS",
      "reasoning": "Q4=YES: bootstrap list — OperationResult is the core return type"
    }
  ]
}
```

If a capability is classified as PLAIN_CS but does not match a bootstrap list entry:
**STOP — challenge the plan before proceeding.**

---

## Section 3 — Challenging a Routing Claim

When a plan claims PLAIN_CS for a capability not on the bootstrap list:

| Challenge | Correct response |
|---|---|
| "This queries existing data, so it must be PLAIN_CS" | Querying data is an ISessionStorage read — the query logic that interprets results may still be LEARNABLE |
| "This is simpler as a C# class" | Simplicity is not on the bootstrap list. Apply Q1-Q4 |
| "The threshold might change, so I'll make it a constant I can edit" | Configurable thresholds belong in `IConfiguration`. Editing constants requires redeployment |
| "This calls ISemanticCompiler29, so it must be manual" | Components that USE ISemanticCompiler29 can still be routing decisions (Q2=YES candidates) |

---

## Anti-Patterns

1. **"All business logic is plain C# — learning pipeline is only for ML features"** — any capability that branches on configurable thresholds belongs in the FREEDOM tier. Any capability whose quality improves with more data belongs in the LEARNABLE tier.

2. **"Bootstrap list can grow to include anything inconvenient"** — the bootstrap list is exhaustive by design. Adding items to it to avoid the routing gate defeats the purpose.

3. **"Hardcode the threshold now, make it configurable later"** — later never comes. FREEDOM config must be in `appsettings.json` before the capability is first called.

4. **"This is the same as a utility, so Q1-Q4 don't apply"** — Q1-Q4 apply to every new capability. The only exception is capabilities already on the bootstrap list.

---

## Integration

- four-tier-decision-classification-SKILL.md provides the tier model (BOUNDARY CODE / FREEDOM CONFIG / LEARNABLE / AI PIPELINE) that this skill's Q1-Q4 maps to
- confidence-lifecycle-SKILL.md governs Phase 2–5 for LEARNABLE capabilities
- solution-scope-gate-SKILL.md handles Level 3–5 scope decisions after routing is classified
- algorithm-as-service-SKILL.md provides specific guidance for Class 1–5 algorithm capabilities (always LEARNABLE tier)
