# Pipeline Trace Reader

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

Every pipeline execution leaves evidence in `dotnet test` output, build logs,
and `OperationResult<T>` return values. Without a systematic reading protocol,
a below-threshold result leads to generic debugging rather than targeted fixes.
This skill defines the exact reading order and diagnosis decision tree for the
project's pipeline stages.

---

## When to Invoke

Auto-fire after every pipeline execution that does not pass its Gate B:
- After `dotnet test` that shows failures
- After `ContinuousLearningPipeline` run that scores below threshold
- After `IncrementalTrainer` run that produces wrong output
- After `SemanticCompilerPipeline` run that produces unexpected results

Required reading before ANY attempt to fix or iterate.

---

## Section 1 — The Pipeline Stages and What to Check in Each

The project's pipeline stages map to these verification points:

### Stage 1 — Input Retrieval (ISessionStorage / ICodeReader)

**What to check:**
- Did the stage receive the correct input? (not null, not empty)
- `OperationResult.IsSuccess` — did the retrieval succeed?
- If retrieval failed: what error code was returned?

**Failure signatures:**
- `OperationResult.IsSuccess = false`, `ErrorCode = "NOT_FOUND"` → source file or session key doesn't exist
- `OperationResult.IsSuccess = false`, `ErrorCode = "STORAGE_ERROR"` → ISessionStorage implementation issue
- Empty result with `IsSuccess = true` → input exists but is empty

**Fix:**
- NOT_FOUND: verify the input source path / session key before calling the stage
- STORAGE_ERROR: check the `ISessionStorage` concrete implementation; run its unit tests in isolation
- Empty result: check the ICodeReader implementation; add a test that reads the specific file

### Stage 2 — Context Building (IBm25Scorer / IVectorIndex / HybridRetriever)

**What to check:**
- Pattern count retrieved — did the search return results?
- Relevance of top result — does it match the query domain?
- Were the correct indices populated before this run?

**Failure signatures:**
- Zero results: search index empty or query terms don't match indexed content
- Low relevance top result (if scored): index populated with wrong data
- Wrong result type: `IBm25Scorer` and `IVectorIndex` returning different top results (expected for hybrid — diagnose the merger)

**Fix:**
- Zero results: populate the index before the pipeline run; verify with a direct search test
- Wrong content: check index population code; verify tags/keywords match query terms

### Stage 3 — Processing (ISemanticCompiler29 / SemanticCompilerPipeline)

**What to check:**
- `OperationResult.IsSuccess` for each processing step
- Intermediate output — does it structurally match the expected format?
- DNA rule violations in the processing output (Rule 1: OperationResult<T>, Rule 10: no external ML)

**Failure signatures:**
- `OperationResult.IsSuccess = false` in a processing step → check the specific ErrorCode
- Output structure wrong → the processing template/logic needs updating
- Rule 1 violated in output → a path inside the processor throws instead of returning OperationResult.Fail

**Fix:**
- Check the exact step that returned `IsSuccess = false` — start there, not at the final stage
- For structural wrong output: write a test that asserts the exact expected structure
- For Rule 1 violation: apply defense-in-depth-SKILL to the throwing path

### Stage 4 — Validation (DNA checks / Iron Rules / Quality Gates)

**What to check:**
- Which specific rule or assertion failed
- The failure message — what exactly was wrong
- Whether the failure is in the validation logic or in the input being validated

**Failure signatures:**
- DNA Rule 1 violation: `throw` found in production code path
- DNA Rule 2 violation: concrete type instantiated instead of interface
- DNA Rule 10 violation: external ML framework reference found

**Standard validation checklist:**
```bash
grep -rn "throw new" src/ --include="*.cs"
# Expected: 0 matches in production code (tests are OK)

grep -rn "new [A-Z][a-zA-Z]*Service()" src/ --include="*.cs"
# Expected: 0 matches (fabric-first: inject through interface)

dotnet build [SolutionFile] → 0 Error(s), 0 Warning(s)
```

### Stage 5 — Scoring / Evaluation

**What to check:**
- Exact score value
- Which component(s) produced the lowest sub-score
- Whether the score is deterministic (run twice, get same result)

**Score component breakdown:**
| Component | What determines it |
|---|---|
| Test pass rate | dotnet test pass count / total count |
| DNA compliance | Rule violations found / total rules checked |
| Behavioral correctness | Specific assertions that test output values |
| Coverage | Code paths exercised by tests |

**Lowest component = first fix target.**
Do not fix high-scoring components. Fix the lowest.

### Stage 6 — Feedback / Training Data Capture

**What to check:**
- Was a training pair captured? (if ContinuousLearningPipeline is active)
- `DpoTrainingPair.ChosenScore > DpoTrainingPair.RejectedScore` — always required
- Was the pair written to the training store?

**Failure signatures:**
- Training pair not captured: `IncrementalTrainer` not triggered or threshold not crossed
- Chosen score ≤ rejected score: invalid pair — do not index
- Pair written but context missing: `DpoTrainingPair.Context` is empty → training signal is incomplete

---

## Section 2 — Score Band → Action Mapping

After reading all stages, classify the overall result:

| Score | Status | Action |
|-------|--------|--------|
| ≥ promotion threshold | PASS | Promote to ACTIVE. Record training pair if applicable. |
| 70–84% | NEAR-PASS | Run quality-improvement-cycle-SKILL targeting lowest component. |
| 60–69% | PATCH | Run quality-improvement-cycle-SKILL. Max 2 cycles before escalate. |
| < 60% | ESCALATE | Do not run improvement cycles. Root cause is architectural. |

---

## Section 3 — Trace Reading Template

After every pipeline run, fill in this template before taking any action:

```
PIPELINE TRACE — [Capability] — [Date]

Stage 1 (Input):
  Result: IsSuccess=[T/F] ErrorCode=[code]
  Diagnosis: [what this means]

Stage 2 (Context):
  Result: [N] patterns retrieved, top relevance=[score or "not measured"]
  Diagnosis: [what this means]

Stage 3 (Processing):
  Result: IsSuccess=[T/F] StructureCorrect=[Y/N]
  Diagnosis: [what this means]

Stage 4 (Validation):
  DNA: [N]/[N] rules pass
  Iron Rules: [list PASS/FAIL per rule]
  Diagnosis: [what this means]

Stage 5 (Score):
  Total: [value]
  Lowest component: [name] at [value]
  Diagnosis: [what this means]

Stage 6 (Feedback):
  Training pair captured: YES/NO
  Pair valid (chosen > rejected): YES/NO

First failing stage: [N] — [stage name]
Root cause: [one sentence]
Fix direction: [what to change]
```

**The first failing stage is the fix target.** Do not fix stage 5 when stage 3 is failing.

---

## Section 4 — Escalation Triggers

Escalate (do not run more cycles) when:
1. Score < 60% on any run
2. Same stage fails after 2 fix attempts (plateau)
3. Stage 2 returns zero results despite index being populated
4. Stage 3 produces structural output that is wrong at every run (template/logic issue)
5. Stage 4 finds DNA Rule 2 violations (fabric-first) — architectural fix required

Escalation report format:
```
ESCALATION REQUIRED
Capability:       [name]
Final score:      [value]
First failing stage: Stage [N] — [stage name]
Failure message:  [exact message from trace]
Root cause:       [assessment — training data / interface design / architecture]
Attempts made:    [list of changes tried]
Next step:        [specific action — redesign / rebuild / decision-reopening-SKILL]
```

---

## Anti-Patterns

1. **"Fix the highest-impact stage"** — fix the first failing stage in the pipeline. Earlier failures contaminate later stage results.

2. **"Score looks reasonable, skip the trace"** — a below-threshold score that "looks reasonable" hides specific failures. Read the trace.

3. **"Same stage failing twice — run a third cycle"** — two failures at the same stage signals that the fix direction is wrong. Escalate.

4. **"Stage 6 (training capture) is optional"** — if `ContinuousLearningPipeline` is active, uncaptured training pairs mean the model doesn't learn from this run. Stage 6 is required.

---

## Integration

- quality-improvement-cycle-SKILL.md uses this skill's trace template as its Step 2 reading
- systematic-debugging-SKILL.md handles the escalation path identified by this skill
- improvement-measurement-SKILL.md requires the score values from this skill's template
- session-execution-log-SKILL.md records the trace summary in EXECUTION-LOG-Pn.json
