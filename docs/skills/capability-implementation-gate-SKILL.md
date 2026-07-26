# Capability Implementation Gate

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

Manual C# code that should be a ContinuousLearningPipeline topology bypasses
the quality scoring and accumulates no DPO triples — it cannot be improved by
learning. A plan that claims "this is simpler as manual C#" without applying the
4-question test is guessing. This skill defines the exhaustive list of what may
be written as manual C# and the 4-question test for everything else.

---

## When to Invoke

- At the start of every phase that introduces new code
- Before writing any `.cs` file in a capability plan
- When a plan claims "manual C# is acceptable" for a capability
- When capability-routing-gate-SKILL.md Q3 needs bootstrap boundary verification

---

## Section 1 — The Exhaustive Bootstrap List

Manual C# is acceptable ONLY for these components:

| Component | Why manual | Created in |
|---|---|---|
| `GenericNodeExecutor` equivalent — the component that executes pipeline nodes | Cannot be a pipeline node itself (circular) | Core infrastructure |
| Pipeline handler implementations (genesis, scoring, feedback, storage) | The pipeline cannot build itself | Core infrastructure subproject |
| Index/storage creation scripts | Must exist before pipeline can write to them | Phase A of any subproject |
| Topology JSON parser | Must exist before topology files can be read | Core infrastructure |
| DomainModelRegistry bootstrap seed | Registry must exist before capabilities can register | Phase A of new subproject |
| This skill itself | The skill that explains the rule — circular otherwise | Skills document |

**This list is exhaustive. It does not grow without a new locked decision in
`docs/DECISIONS-LOCKED.md`.**

---

## Section 2 — The 4-Question Test

Apply to every capability before classifying it as manual C# or pipeline topology:

```
Q1: Will this capability's output quality improve with more data?
    YES → PIPELINE (capability-improvement-cycle-SKILL.md + DomainModelRegistry)
    NO  → continue to Q2

Q2: Does this capability branch based on conditions that could change
    over time or per session?
    YES → PIPELINE (route.handler nodes, FREEDOM config thresholds in IConfiguration)
    NO  → continue to Q3

Q3: Should this capability appear in the plan as a traceable execution unit
    with Gate B measurability?
    YES → PIPELINE
    NO  → continue to Q4

Q4: Is this capability on the exhaustive bootstrap list above?
    YES → manual C# is acceptable for Phase A only
    NO  → PIPELINE (default for everything not on the bootstrap list)
```

If Q1, Q2, or Q3 is YES: the capability is a pipeline topology.
If only Q4 is YES: manual C# is acceptable for Phase A bootstrap only.
If all four are NO: this is genuinely a utility — manual C# is fine.

Genuine utilities are rare. When uncertain, default to PIPELINE.

---

## Section 3 — Capability Routing Declaration

Every phase plan that classifies capabilities as manual vs pipeline must declare
this before Phase A begins:

```markdown
## Capability Routing — [SubprojectName]

| Capability | Decision | Reasoning |
|---|---|---|
| WeightedScoring score normalization | PIPELINE | Q1=YES: normalization quality improves with per-session feedback |
| OperationResult<T> error mapping | MANUAL | Q4=YES: infrastructure utility, no learning signal |
| DomainModelRegistry seed data | MANUAL | Q4=YES: bootstrap list item |
| HybridRetriever BM25+vector merge | PIPELINE | Q1=YES: merge weight improves with retrieval outcome data |
```

If a capability is classified MANUAL but does not match a bootstrap list entry:
**STOP — challenge the plan before proceeding.**

---

## Section 4 — Challenging a Manual Claim

When a plan claims manual C# for a capability not on the bootstrap list:

```
Challenge 1: "This modifies an existing interface, so it must be manual."
Fix:    Components that USE an interface are not bootstrap components.
        A capability that calls ISessionStorage is a pipeline node.

Challenge 2: "This just checks a condition, so it can't be a pipeline."
Fix:    Condition checking is a validate handler.
        "Does this capability have sufficient observations?" = validate(observationCount >= N)

Challenge 3: "This is simpler as C#."
Fix:    Simplicity is not on the bootstrap list. Apply Q1-Q4.

Challenge 4: "The threshold might change, so I need to configure it in C#."
Fix:    Configurable thresholds belong in IConfiguration (FREEDOM config).
        C# hardcodes the change-deployment cycle. IConfiguration does not.
```

---

## Section 5 — Test Scenarios

Apply the 4-question test to these examples:

| Capability | Test result | Reasoning |
|---|---|---|
| `CapabilityScoreNormalizer` that normalizes scores 0–1 | PIPELINE | Q1=YES: normalization formula improves with outcome data |
| `ObservationCountIncrementer` that adds 1 to observationCount | PIPELINE | Q2=YES: atomic increment logic depends on threshold config |
| `DomainModelRegistrySeeder` that creates initial edges | MANUAL (Phase A only) | Q4=YES: bootstrap list — registry must exist before pipeline can register |
| `BM25TokenizerFactory` that creates tokenizer instances | MANUAL utility | All Q=NO: factory pattern, no learning, no conditions, not traceable |

---

## Anti-Patterns

1. **"Capability gap scanning checks files — must be manual"** — file checking is a
   validate handler operation. File-existence checks are pipeline-compatible.

2. **"This aggregates results — needs manual TypeScript-style C#"** — aggregation is a
   decompose/aggregate handler pair. Orchestration is pipeline.

3. **"CalibrationMemory just stores records — C# is fine"** — Q1=YES (guidance quality
   improves with data) AND Q2=YES (count threshold is configurable). PIPELINE.

4. **"We need a runner script to execute the scanner"** — a runner script for a capability
   = unregistered manual code. The capability is a pipeline; a pipeline has a registered
   capability entry in DomainModelRegistry.

---

## Integration

- capability-routing-gate-SKILL.md Q3 (should it appear in DomainModelRegistry?) gates this skill
- four-tier-decision-classification-SKILL.md Q2 (is this FREEDOM config or LEARNABLE?) is the prior question
- DomainModelRegistry is where PIPELINE capabilities register — MANUAL capabilities do not appear there
- ContinuousLearningPipeline is the execution layer for PIPELINE capabilities
- docs/DECISIONS-LOCKED.md is where bootstrap list extensions are recorded
