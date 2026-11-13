# Capability Improvement Cycle

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

A capability that is implemented once, tested once, and never revisited does not
improve. Without a defined improvement cycle, a capability that starts at 70%
quality stays at 70% — because no mechanism exists to detect degradation, collect
better training signals, or promote proven patterns. This skill defines the
5-stage cycle that keeps capabilities improving over their lifetime.

---

## When to Invoke

- When designing a new capability that will handle real user data
- When reviewing a completed phase to determine if the cycle is wired
- When a capability's test score drops below its previous baseline
- When a capability has been running for 5+ invocations but quality has not improved

---

## Section 1 — The 5-Stage Cycle

Every capability moves through these five stages, then repeats from Stage 2:

```
Stage 1: GAP IDENTIFIED
  Source: - A phase plan identifies a missing capability
          - A behavioral test fails that covers a required scenario
          - ContinuousLearningPipeline emits a gap signal (low confidence + repeated failure)
          - Luba identifies a repeated failure pattern in session output
  Output: Capability name, failure class, proposed fix class (from solution-scope-gate-SKILL.md)

Stage 2: DESIGN (Phase A of the owning subproject)
  Who:    Claude Code writes the interface + concrete class + test class
  How:    design-artifact-completeness-SKILL.md + multi-reviewer-design-SKILL.md
  Output: I[Name].cs (interface)
          [Name].cs (concrete implementation)
          [Name]Tests.cs (test class with all e2e-test-matrix-SKILL.md categories)
          DI registration in Program.cs
  Gate:   All 5 design-artifact-completeness checks PASS before Stage 3

Stage 3: IMPLEMENTATION + SCORING
  Who:    Claude Code implements and runs tests
  Score 0–59%  → STRUCTURAL_FAILURE: redesign (back to Stage 2)
  Score 60–69% → PATCH: targeted fix, max 3 cycles
  Score 70–84% → NEAR-PASS: improve specific failing category
  Score 85%+   → PASS: capability promoted to ACTIVE in DomainModelRegistry
  Output: - DomainModelRegistry entry with score + status
          - Failing test evidence (if PATCH/STRUCTURAL_FAILURE)
          - ContinuousLearningPipeline feedback entries if wired

Stage 4: RUNS AND RECORDS OUTCOMES (ongoing)
  Every invocation: OperationResult feedback → ContinuousLearningPipeline
  After 5+ invocations: confidence updated per confidence-lifecycle-SKILL.md
  Multi-cycle failures: gap signal emitted (triggers Stage 1 for next cycle)
  Structural issues: design-flaw signal captured for next Stage 2

  Shadow scoring (for capabilities in DomainModelRegistry):
    Each invocation records: input + output + correctness observable
    When confidence ≥ risk-tier threshold (per confidence-lifecycle-SKILL.md):
      → capability eligible for graduation to bypass-eligible status
      → graduation is an explicit promotion decision, not automatic

Stage 5: PATTERNS PROMOTED (thresholds triggered)
  confidence ≥ 0.85 across 10+ invocations
    → Capability promoted to PROVEN status in DomainModelRegistry
    → Pattern added to project knowledge (ARCHITECTURE.md if architectural)
  Repeated failures of same type (3x same failing scenario)
    → new behavioral test added for that failure class
    → capability returns to Stage 2 (targeted redesign of the failing path)
  Design flaw × 3 with similar root cause
    → new iron rule derived (iron-rule-derivation-SKILL.md)
    → all new capabilities in same class receive the iron rule in Stage 2
```

---

## Section 2 — What Each Stage Produces

```
Stage 2 → I[Name].cs + [Name].cs + [Name]Tests.cs + DI registration
Stage 3 → DomainModelRegistry entry (status: OPTIONAL/ACTIVE/PROVEN)
           Failing test report (if PATCH or STRUCTURAL_FAILURE)
Stage 4 → ContinuousLearningPipeline feedback entries per invocation
           Gap signals for repeated failures
Stage 5 → Status promotion in DomainModelRegistry
           New iron rule (if 3x same design flaw)
           New behavioral test (if 3x same runtime failure)
```

After the first 5 capabilities through the full cycle, the DomainModelRegistry
accumulates PROVEN patterns that new capabilities can learn from — new Stage 2
designs that encounter similar structure can reference PROVEN examples.

---

## Section 3 — Cycle Health Metrics

Track these per capability to verify the cycle is working:

```
□ Stage 2 → design-artifact-completeness all 5 checks PASS
□ Stage 3 → dotnet test → ≥85% pass within 3 cycles
□ Stage 3 → DomainModelRegistry entry created (grep for capability name)
□ Stage 4 → ContinuousLearningPipeline wired (observationCount incrementing)
□ Stage 5 → confidence updates visible in DomainModelRegistry after 5 runs
```

If Stage 3 requires 3+ cycles on the same capability consistently:
→ The Stage 2 design is inadequate (interface mismatch, wrong abstraction level)
→ Escalate to multi-reviewer-design-SKILL.md before another Stage 2 attempt

---

## Section 4 — Cycle Record Format

After each Stage 3 cycle, record in the session log:

```markdown
## Capability Improvement Cycle — [CapabilityName] — Cycle N

Stage: 3 (Implementation + Scoring)
Score this cycle: [N]%
Prior score: [N]% (Cycle N-1)

Failing categories (from e2e-test-matrix-SKILL.md):
- Category [N]: [specific failing test name]
- Category [N]: [specific failing test name]

Action taken: PASS (promote) | PATCH (targeted fix) | STRUCTURAL_FAILURE (redesign)

If PATCH:
  Cycles used: [N] of 3 maximum
  If cycle 3 without PASS: escalate to Luba with redesign proposal

DomainModelRegistry status after this cycle: OPTIONAL | ACTIVE | PROVEN
```

---

## Section 5 — Escalation

If Stage 3 reaches cycle 3 without achieving 85%:

```markdown
## Capability Cycle Escalation

Capability: [name]
Cycles attempted: 3
Best score: [N]%
Persistent failing category: [category + test name]
Root cause assessment: [interface mismatch | algorithm error | test environment issue]
Redesign proposal: [specific structural change proposed]
Question for Luba: [one specific decision needed to unblock]
```

---

## Anti-Patterns

1. **"It passes the happy path, ship it"** — happy path at Stage 3 is not Stage 5 readiness.
   Stage 4 (ongoing invocations) is required before Stage 5 promotion. Shipping at Stage 3
   without wiring Stage 4 means the capability never improves beyond its initial score.

2. **"Skip ContinuousLearningPipeline wiring, add it later"** — Stage 4 data is only
   valuable when collected from the start. A capability that runs 100 times unobserved
   has lost 100 learning opportunities. Wire Stage 4 in Stage 2.

3. **"DomainModelRegistry entry is optional"** — without a registry entry, Stage 5 promotion
   has nowhere to land. The registry entry is required by Stage 3.

4. **"3 cycles without improvement, try a 4th"** — after 3 cycles, the problem is architectural.
   Escalate. A 4th cycle without structural change produces the same result.

---

## Integration

- confidence-lifecycle-SKILL.md governs Stage 4 confidence updates and Stage 5 graduation thresholds
- quality-improvement-cycle-SKILL.md provides the 9-step fix cycle for Stage 3 PATCH
- capability-routing-gate-SKILL.md determines whether a capability enters this cycle via ContinuousLearningPipeline
- iron-rule-derivation-SKILL.md is triggered by Stage 5 design-flaw accumulation
- multi-reviewer-design-SKILL.md is invoked when Stage 3 cycle 3 fails (escalation path)
- DomainModelRegistry is the persistence layer for Stage 3 entries and Stage 5 promotions
