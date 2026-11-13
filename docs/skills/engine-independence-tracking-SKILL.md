# Engine Independence Tracking

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

The primary economic goal of the AI pipeline is to replace expensive external model
calls with a locally-hosted model. Progress toward this goal is measurable: the gap
score between expensive model and local model scores per capability per archetype.
Without gap scores, the independence timeline is unknown. An unknown independence
timeline makes it impossible to decide when to invest in local model infrastructure.
This skill defines the gap score formula, shadow run records, action thresholds, and
the ENGINE PROGRESS section format that makes independence progress visible.

---

## When to Invoke

- When writing any PHASE-COMPLETE-Pn.md (ENGINE PROGRESS section is FIRST)
- When a new capability archetype completes Phase B for the first time
- When deciding whether a capability class is ready for local model routing
- When DPO triple count approaches 80 (graduation test threshold)
- When gap scores are flat or rising after 5 or more capability runs

---

## Section 1 — Gap Score Formula

```
gap_score = expensive_model_score - local_model_score
```

Measured per capability class per archetype. Both scores are `CapabilityScore`
values from score.handler output in the same evaluation context.

```csharp
public record GapScoreRecord
{
    public string CapabilityId { get; init; }
    public string ArchetypeClass { get; init; }   // ROUTING, VALIDATION, TRANSACTION, etc.
    public int CurriculumTier { get; init; }       // 1-5, from dpo-triple-format-SKILL.md
    public double ExpensiveModelScore { get; init; }
    public double LocalModelScore { get; init; }   // null if local model not deployed
    public double? GapPercent { get; init; }       // null if local model not deployed
    public ShadowStatus ShadowStatus { get; init; }
    public DateTimeOffset EvaluatedAt { get; init; }
}

public enum ShadowStatus
{
    PendingLocalModel,   // local model not deployed; ExpensiveModelScore recorded for later
    Active,              // both scores present; GapPercent computed
    Graduated,           // gap < 5% for 3 consecutive runs; capability routed to local model
}
```

**When local model is not deployed:** create a shadow record with `ShadowStatus =
PendingLocalModel` and `LocalModelScore = null`. Record the expensive model score now.
When a local model becomes available, fill in `LocalModelScore` and compute `GapPercent`.
Do not omit shadow records — omitting them means the independence timeline cannot be
computed retroactively.

---

## Section 2 — Action Thresholds

| Condition | Action |
|-----------|--------|
| Gap score < 5% for a capability class | Capability class is a candidate for local model routing |
| Gap score < 5% for 3 consecutive runs | Route to local model; mark GapScoreRecord.ShadowStatus = Graduated |
| Valid DPO triples ≥ 80 across curriculum tiers | Graduation test can begin |
| Gap score trending DOWN over 3+ runs | Local model is learning; continue current approach |
| Gap score flat or rising after 5+ runs | Check DPO triple validity; potential curriculum quality issue |

**Graduation test:** when valid DPO triple count reaches 80 across all 5 curriculum
tiers, run a parallel evaluation: local model vs expensive model on the same 10
capability inputs across all archetype classes. If local model achieves ≥ 0.85 score
on all archetypes: graduation declared.

---

## Section 3 — ENGINE PROGRESS Section

Every PHASE-COMPLETE-Pn.md begins with this section as the FIRST content.
Never bury it after file lists or test counts.

```markdown
## ENGINE PROGRESS — Phase [N] Complete

| Metric | This Phase | Cumulative | Target | Gap |
|--------|-----------|------------|--------|-----|
| Valid DPO triples | +N | TOTAL | 80 | REMAINING |
| Pending DPO triples | +N | TOTAL | 0 | — |
| Gap score ROUTING (Tier 1) | N% / PENDING | N% | <5% | N% |
| Gap score VALIDATION (Tier 2) | N% / PENDING | N% | <5% | N% |
| Gap score TRANSACTION (Tier 3) | N% / PENDING | N% | <5% | N% |
| Gap score ORCHESTRATION (Tier 4) | N% / PENDING | N% | <5% | N% |
| Gap score SCHEDULED (Tier 5) | N% / PENDING | N% | <5% | N% |
| Prompt versions improved | N | N total | continuous | — |
| Capabilities to graduation test | — | — | 80 triples | N remaining |
| Estimated cost this phase | $N | $N total | → $0/run | $N/run |

### What improved this phase (required)
- [ ] At least one gap score row updated (or new PENDING record added if first run)
- [ ] DPO triple count increased
- [ ] At least one capability prompt version advanced

### What is blocked from improving (required — state explicitly if nothing blocked)
- [Dimension]: [specific blocker and what would unblock it]

### Decision needed
[One specific question with 2-3 options and timeline impact, OR "No decision needed: [reason]"]
```

Rules:
- Metrics are queried — never estimated
- PENDING is valid: "PENDING — local model not deployed" is more useful than omitting the row
- A row showing "PENDING" for 3+ consecutive phases with no explanation is a blocker that
  must be named in "What is blocked"
- Gap scores are cumulative: a capability that graduated stays in the table at < 5%

---

## Section 4 — Graduation Criteria

A capability class graduates from expensive model to local model when:

```
Criterion 1: Gap score < 5% for 3 consecutive evaluation runs for this archetype class
Criterion 2: Valid DPO triples ≥ minimum for this class (see dpo-triple-format-SKILL.md Section 4)
Criterion 3: Graduation test passes (local model score ≥ 0.85 on 10 held-out inputs)
```

Graduation is per-archetype, not per-project. ROUTING graduating does not affect
SCHEDULED. Each archetype class earns graduation independently.

**After graduation:** route.handler directs generation requests for this archetype class
to the local model endpoint (from FREEDOM config). The expensive model becomes the
fallback when local model score drops below 0.80.

---

## Anti-Patterns

1. **"Gap scores PENDING for 3+ phases without explanation"** — PENDING with no blocker
   named means the independence timeline is unknown by design. Name the blocker. "Local
   model not deployed" is a valid blocker — but it must be stated, not silently absent.

2. **"We don't track gap scores until FLOW-39 is active"** — FLOW-39 (or its .NET
   equivalent) refines the curriculum. The gap score tracking begins with the first
   expensive model evaluation. Shadow records with PendingLocalModel status preserve
   the expensive model score for later comparison.

3. **"80 DPO triples means graduation"** — 80 triples across all curriculum tiers is the
   threshold to BEGIN the graduation test. The graduation test still needs to pass. Triples
   without CurriculumTier null do not count toward the 80.

4. **"Cost per run is zero overhead to track"** — cost trajectory is a first-class metric
   because the goal is to drive expensive model cost toward zero. Without cost tracking per
   phase, the graduation test has no economic context.

5. **"ENGINE PROGRESS after gate results"** — it is the FIRST section. Luba reads
   PHASE-COMPLETE to understand mission progress. Test counts are secondary. "Are we closer
   to independence?" must be answerable from the first paragraph, not after scrolling.

---

## Integration

- mission-progress-SKILL.md: gap score rows extend the 5 mandatory questions with
  independence-specific metrics; load both skills when writing PHASE-COMPLETE
- dpo-triple-format-SKILL.md: Section 4 (minimum triple count per archetype class) defines
  the per-archetype DPO threshold that feeds Criterion 2 of graduation
- ai-model-selection-SKILL.md: three-consecutive replacement signal maps to gap score
  staying < 5% for 3 consecutive runs (Criterion 1 of graduation)
- learning-signal-capture-SKILL.md: SHADOW_RUN signal type creates the shadow records that
  store expensive model scores for later gap computation
