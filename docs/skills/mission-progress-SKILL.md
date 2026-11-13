# Mission Progress

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

## Why This Skill Exists

After every session, Luba needs to know one thing: are we closer to a capable,
self-improving ML engine than we were before? The answer must arrive as the
FIRST section of every phase completion report — not buried after file lists
and test counts.

This skill answers that question with five mandatory questions backed by
verification commands, not estimates. If data does not exist: state PENDING
and why. Never omit, never estimate.

---

## When to Invoke

Load before every STOP in every session type:
- Before writing PHASE-COMPLETE-Pn.md
- Before writing any session handoff note
- Before the final commit of any phase

**The report is the FIRST section.** Gate results, file lists, and test counts
follow — never before.

---

## The Five Mandatory Questions

Answer all five. State PENDING with a reason if data is unavailable.

---

### Question 1: Did the engine LEARN this session? (Training Data)

```bash
# Count DPO triples in training pipeline
grep -rn "DpoTriple\|DpoTrainer\|TrainingRecord" Src/12.ContinuousLearning/ --include="*.cs" | wc -l

# Count domain model versions
grep -rn "DomainModelVersion\|ModelVersion" Src/12.ContinuousLearning/ --include="*.cs" | head -10

# Tests that verify training data quality
dotnet test --filter "DpoTrainer|TrainingData" --no-build 2>&1 | tail -5
```

Report format:
```
TRAINING
  DPO triples captured this session:  +N
  Domain model versions advanced:     N → N+1 (or PENDING if pipeline not running)
  Training tests passing:             N passed, 0 failed
  Blocking gap:                       [what prevents training, or NONE]
```

---

### Question 2: Did the engine IMPROVE this session? (Capability)

```bash
# Run capability tests to see current pass rate
dotnet test --filter "Category=Capability" --no-build 2>&1 | tail -5

# Check if any inference accuracy tests advanced
grep -rn "AccuracyTest\|CapabilityScore" Tests/ --include="*.cs" | wc -l
```

Report format:
```
CAPABILITY
  Capability tests this session:   +N new, N total
  Accuracy improvements:           [specific test names or PENDING — no accuracy tests yet]
  Inference quality delta:         PENDING | [measurable delta if tracked]
  Nothing improved:                [state explicitly if true]
```

---

### Question 3: How close to self-improvement? (Architecture Completeness)

```bash
# Check ContinuousLearningPipeline integration
grep -rn "ContinuousLearningPipeline\|IContinuousLearning" Src/ --include="*.cs" | head -10

# Check DomainModelRegistry status
grep -rn "DomainModelRegistry\|IDomainModelRegistry" Src/ --include="*.cs" | head -10

# Check IncrementalTrainer
grep -rn "IncrementalTrainer\|IIncrementalTrainer" Src/ --include="*.cs" | head -5
```

Report format:
```
ARCHITECTURE COMPLETENESS
  ContinuousLearningPipeline:  ACTIVE | PARTIAL | MISSING
  DomainModelRegistry:         ACTIVE | PARTIAL | MISSING
  IncrementalTrainer:          ACTIVE | PARTIAL | MISSING
  Training round-trip test:    PASS | FAIL | MISSING
  Estimated phases to full round-trip: N
```

---

### Question 4: What BLOCKED improvement this session?

State each blocked dimension explicitly. Acceptable blockers:
- "IncrementalTrainer built but no training corpus — LoRA training not running"
- "ContinuousLearningPipeline built but DPO triple ingestion not wired to user feedback"
- "Phase A infrastructure session — no training ran, no capability data produced"

**Not acceptable:** omitting this section entirely.

Report format:
```
BLOCKING
  [Dimension]: [specific blocker and what would unblock it]
  ...
  None: [state explicitly if nothing blocked]
```

---

### Question 5: What does LUBA DECIDE next?

One specific decision with 2-3 options and timeline impact. If no decision
is needed: state that explicitly.

Report format:
```
DECISION NEEDED
  Question: [one specific question]
  Option A: [description] → [impact on capability timeline]
  Option B: [description] → [impact on capability timeline]
  Recommendation: [if any, with reasoning]

  OR

  No decision needed this session: [reason]
```

---

## Progress Table Template

Copy this into every PHASE-COMPLETE-Pn.md as the FIRST section:

```markdown
## ENGINE PROGRESS — Phase [N] Complete
*(Run verification commands — never estimate. State PENDING if not yet built.)*

| Component | Status | Evidence |
|-----------|--------|---------|
| ContinuousLearningPipeline | ACTIVE / PARTIAL / MISSING | grep result |
| DomainModelRegistry | ACTIVE / PARTIAL / MISSING | grep result |
| IncrementalTrainer | ACTIVE / PARTIAL / MISSING | grep result |
| Training round-trip test | PASS / FAIL / MISSING | dotnet test result |
| DPO triples this session | +N | count from training data |
| Capability tests passing | N | dotnet test --filter Capability |

### What improved this phase (required)
- [ ] At least one new capability test added
- [ ] Training pipeline component integrated or extended
- [ ] Round-trip verification passing (if pipeline components present)

### What is blocked (required — state explicitly if nothing blocked)
- [Dimension]: [specific blocker]

### Decision needed
[Question, options, recommendation OR "No decision needed: [reason]"]
```

---

## Decision Thresholds

| Metric | Threshold | Action |
|--------|-----------|--------|
| Round-trip test missing for 3+ phases | Always | Design training integration before next feature work |
| Capability tests < 5% of total tests | 3+ phases | Add capability measurement as a phase gate |
| No DPO triples after IncrementalTrainer built | 2+ phases | Investigate DPO ingestion — blocked by design gap |

---

## Placement Rule

ENGINE PROGRESS is the **FIRST** section of every PHASE-COMPLETE-Pn.md.

Ordering:
1. ENGINE PROGRESS (this skill's output)
2. Phase gate results (build, test counts, DNA compliance)
3. Files changed
4. Session handoff

Never place ENGINE PROGRESS after gate results. Luba reads PHASE-COMPLETE
to understand mission progress, not test counts. Test counts are secondary.
