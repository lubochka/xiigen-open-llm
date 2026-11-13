# Learning Path Audit

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

capability-simulation-SKILL.md L3 asks: "If this capability runs, what learning
signals will it produce?" — a forward-looking simulation of a designed scenario.
This skill asks the backward-looking question: "Given today's code, what learning
signals does it ACTUALLY produce?" A topology that compiles and runs but has a
broken signal path is generating output without improving. The engine runs; the
engine does not learn. This skill finds what is broken in the signal path before
any run is performed.

---

## When to Invoke

- At the start of any capability audit session
- After any fix session that touches feedback.handler or signal emission
- When capability confidence never improves despite many run cycles
- When DPO triple count is 0 or far below expected after multiple promotion cycles

---

## Section 1 — Five Signal Types to Audit

For each signal type, three questions:

```
FIRES?      Does the handler actually emit this signal? (yes / no / never)
COMPLETE?   Are all required fields present at the call site?
CORRUPTION? What class of corruption exists if the signal fires with wrong data?
```

---

### Signal 1 — DPO_TRIPLE

**Expected:** feedback.handler emits after each generation cycle that crosses
the 0.85 threshold from below.

Required fields: `ChosenOutput`, `RejectedOutput`, `ChosenModel`, `RejectedModel`,
`Prompt.System`, `CurriculumTier`, `CapabilityId`, `CapabilityClass`.

**Audit commands:**

```bash
# 1. Does feedback.handler emit DPO_TRIPLE?
grep -n "DPO_TRIPLE\|StoreDpoTriple\|TrainingDataIndex" \
  src/**/*FeedbackHandler*.cs | head -10

# 2. Is Prompt.System populated at the call site?
grep -n "Prompt\.System\|GenesisContext\|ContextPackage" \
  src/**/*FeedbackHandler*.cs
# If not found: Prompt.System = null on all triples

# 3. Is ChosenModel != RejectedModel checked?
grep -n "DpoValidityResult\|SameModel\|PENDING_COMPARISON" \
  src/**/*DpoValidat*.cs
```

**Corruption classes:**

| Corruption | Symptom | Impact |
|---|---|---|
| `Prompt.System = null` | Context package not attached | Triple unusable for fine-tuning |
| `ChosenModel == RejectedModel` | Same-model DPO | Teaches style drift, not quality |
| `CurriculumTier = null` | Tier not assigned | Training pipeline cannot sequence |
| All three simultaneously | Triple looks valid, is not | Learning loop appears to run; produces 0 usable data |

---

### Signal 2 — OUTCOME

**Expected:** feedback.handler emits OUTCOME after every topology execution.

Required fields: `CapabilityId`, `Score`, `PatternsRetrieved[]`, `CyclesUsed`,
`TopologyId`.

**Audit commands:**

```bash
# 1. Does feedback.handler emit OUTCOME?
grep -n "OUTCOME\|OutcomeSignal\|LearningOutcomes" \
  src/**/*FeedbackHandler*.cs | head -5

# 2. Is PatternsRetrieved populated?
grep -n "PatternsRetrieved\|RetrievedPatterns" \
  src/**/*FeedbackHandler*.cs
# If absent: DomainModelRegistry cannot update edge quality scores
```

**Corruption class:** OUTCOME without `PatternsRetrieved` = edge quality scores
never update. After 50 runs, the DomainModelRegistry has the same confidence
values as after run 1.

---

### Signal 3 — ARBITER_VERDICT

**Expected:** score.handler emits one ARBITER_VERDICT per arbiter role after
each evaluation.

Required fields: `Role`, `Verdict` (APPROVED / BLOCK / CONCERN), `Score`,
`Reasoning`, `CapabilityId`.

**Audit commands:**

```bash
# 1. Does score.handler emit ARBITER_VERDICT?
grep -n "ARBITER_VERDICT\|ArbiterVerdict\|EmitVerdict" \
  src/**/*ScoreHandler*.cs

# 2. If not: does it compute only aggregate score?
grep -n "WeightedScore\|AverageScore\|return.*Score" \
  src/**/*ScoreHandler*.cs | head -5
# If only aggregate: no per-role learning signal produced
```

**Corruption class:** No ARBITER_VERDICT = per-role accuracy never improves.
Only aggregate score feedback exists; individual arbiter calibration is frozen.

---

### Signal 4 — MODEL_COMPARISON

**Expected:** When multi-generate node is used, MODEL_COMPARISON records the
score gap between model tiers.

Required fields: `CapabilityClass`, `GapScore`, `PrimaryModelScore`,
`SecondaryModelScore`, `ArchetypeTier`.

**Audit commands:**

```bash
# 1. Are MODEL_COMPARISON signals emitted after multi-generate?
grep -n "MODEL_COMPARISON\|ModelComparison\|GapScore" \
  src/**/*FeedbackHandler*.cs

# 2. Are SHADOW_RUN placeholders seeded?
grep -n "SHADOW_RUN\|ShadowStatus\|PENDING_ALTERNATIVE" \
  src/**/*FeedbackHandler*.cs
# Expected: placeholders emitted even when alternative tier not active
```

**Corruption class:** No MODEL_COMPARISON = model tier decision data absent.
ai-model-selection-SKILL.md three-consecutive replacement signal cannot fire
because the comparison data never arrives.

---

### Signal 5 — DESIGN_REASONING

**Expected:** At architectural decision time (during planning sessions),
DESIGN_REASONING signal is stored as a RAG pattern.

Required fields: `DecisionType`, `Context`, `ProposedRepresentation`,
`FinalRepresentation`, `TeachingPoint`, `PrincipleApplied`, `QualityScore`.

**Audit commands:**

```bash
# 1. Are DESIGN_REASONING patterns stored?
grep -rn "DESIGN_REASONING\|ArchitectureDecision\|DecisionCapture" \
  src/ | head -5

# 2. Are they stored with TeachingPoint populated?
grep -n "TeachingPoint" src/**/*DecisionCapture*.cs
```

**Corruption class:** Zero DESIGN_REASONING patterns = planning decisions not
in training data. After many capability runs, code generation improves; planning
judgment does not.

---

## Section 2 — Audit Table

Produce after running all five signal audits:

```
## LEARNING PATH AUDIT — [date]

| Signal | Fires? | Fields complete? | Corruption class | Impact |
|--------|--------|-----------------|-----------------|--------|
| DPO_TRIPLE | YES | Prompt.System: ❌ CurriculumTier: ✅ | null prompt on all triples | 0 valid triples for fine-tuning |
| OUTCOME | YES | PatternsRetrieved: ❌ | Edge quality never updates | DomainModelRegistry frozen at seed values |
| ARBITER_VERDICT | NO | — | No per-role signal | Arbiter accuracy cannot improve |
| MODEL_COMPARISON | PARTIAL | GapScore: ❌ null | Shadow placeholders exist but gap never computed | Model tier switching never triggers |
| DESIGN_REASONING | NO | — | 0 architectural decisions in RAG | Planning judgment unchanged after N runs |

VERDICT: GENERATING BUT NOT LEARNING
ROOT CAUSE: Three disconnections (Prompt.System path, PatternsRetrieved path, ARBITER_VERDICT emission)
FIX PATH: Class D bug for each disconnection; three-level-verification-SKILL.md after each fix
```

---

## Section 3 — Verdict Classification

```
All signals fire + all fields complete              → LEARNING (proceed to next phase)
All signals fire + 1-2 field gaps                  → PARTIAL LEARNING (fix gaps before promotion)
1-2 signals not firing                             → GENERATING BUT NOT LEARNING (fix before execution)
3+ signals not firing OR Prompt.System null on all → STRUCTURAL LEARNING FAILURE (fix before Phase B)
```

**STRUCTURAL LEARNING FAILURE** means: the engine is running, producing output,
and accumulating no improvement data. Continuing to run accumulates runs that
must later be classified as CONTAMINATED or RECOVERABLE by
training-data-gap-audit-SKILL.md. Stop and fix the signal path first.

---

## Anti-Patterns

1. **"I'll audit the signals after the capability is done"** — signals broken
   during development produce runs that must be audited and potentially
   purged. Audit before the first real promotion run.

2. **"PARTIAL LEARNING is good enough for now"** — a 1-2 field gap that
   persists across 20 runs means those runs produced 20 partial records.
   Fix gaps before promotion; training on partial records degrades model quality.

3. **"Score looks good, so learning must be working"** — score reflects
   output quality. Learning signals are separate from the score. A score
   of 0.90 with STRUCTURAL LEARNING FAILURE means the capability passes
   but the engine learns nothing from it.

---

## Integration

- capability-simulation-SKILL.md L3 (forward-looking) is paired with this skill (backward-looking)
- learning-signal-capture-SKILL.md defines what each signal type requires; this skill checks the live code against those requirements
- pipeline-bug-taxonomy-SKILL.md: each signal disconnection is a Class D bug (pipeline stage using wrong data)
- training-data-gap-audit-SKILL.md: STRUCTURAL LEARNING FAILURE triggers a gap window audit
- three-level-verification-SKILL.md: each signal fix must be verified at unit + integration + suite levels
