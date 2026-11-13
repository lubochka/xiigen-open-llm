# Improvement Measurement Protocol

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

An internal metric that improved is not an improvement until a user-facing or
behavioral change can be described. Test counts can rise while the capability
regresses. Build passes while the algorithm produces wrong results.

**Observable behavioral delta is required before any improvement claim is permitted.**

---

## Origin

Extracted from sessions where "+N tests" was reported as a capability improvement.
The tests counted what they counted — but whether the algorithm produced better
results for any input was never established. Internal metric improved. Whether
the system became more capable was never verified.

---

## When to Invoke

- Any session claiming a capability improvement
- Any STOP that includes an improvement percentage or before/after metric
- Before any claim of "X is now complete" or "X improved by N%"
- Before writing ENGINE PROGRESS in mission-progress-SKILL.md

---

## Section 1 — The Two-Layer Protocol

For any capability change, two layers must be present before an improvement claim is valid.

### Unit Test Layer — Internal metrics (necessary but not sufficient)

Internal metrics confirm structural correctness. Required. Not sufficient alone.

| Metric type | What it confirms | What it does NOT confirm |
|---|---|---|
| Test count increased | More scenarios covered | Algorithm produces better results |
| Build errors = 0 | Code compiles | Correct behavior |
| Code coverage % | Tests reach the code | Tests test the right thing |
| New interface method added | Contract extended | Callers benefit from the extension |

Unit test layer format (required in every STOP including capability claims):
```
UNIT TEST LAYER — Internal metrics:
  Test count: [before] → [after] ([delta])
  Build errors: 0
  New tests: [list test names]
```

### Behavioral Layer — Observable delta (required for improvement claim)

For every changed component, one concrete statement describing what the system
does differently for a specific input.

Behavioral layer format (required for any improvement claim):
```
BEHAVIORAL LAYER — Observable delta:
  [Component]: [what input now produces what different output]
  BM25Scorer: Score("cat sat", "the cat sat on the mat") now returns 0.73 (was untested)
  HybridRetriever: query "neural network" now returns ranked hybrid results
                   combining BM25 + cosine similarity (previously vector-only)
```

**"No sentence = no improvement claim"** — if the session cannot state what the
system does differently for a concrete input, the improvement has not been measured.

---

## Section 2 — Claim Validity Rules

### Component-level claim: "IBm25Scorer capability improved"

Requires:
- Unit test layer: test count delta with passing tests listed
- Behavioral layer: one sentence per changed method describing input → output change
- Gate B evidence from verification-before-completion-SKILL.md

Example of a valid component-level claim:
```
IBm25Scorer capability improved.
UNIT TEST LAYER: 0 → 8 tests (+8); all 8 pass; Failed: 0
BEHAVIORAL LAYER: BM25Scorer.Score("query", "document containing query") returns
  positive score > 0; Score("query", "completely unrelated text") returns lower score.
  Relative ranking verified in BM25ScorerTests.RelativeRankingTest.
```

### Pipeline-level claim: "Search pipeline capability improved"

Requires:
- Unit test layer: test count delta across all pipeline components
- Behavioral layer: one sentence per pipeline stage describing integration behavior
- Coverage completeness: all N pipeline components have test evidence
  (coverage-completeness-gate-SKILL.md)

---

## Section 3 — The STOP Gate

Before any STOP that includes an improvement percentage or before/after metric:

```
IMPROVEMENT CLAIM GATE (improvement-measurement-SKILL):

Claim: "[stated improvement]"

Unit test layer check:
  □ Internal metrics present (test counts, build status)
  □ Unit test layer format block present in this response

Behavioral layer check:
  □ Observable delta present for each changed component
  □ At least one input/output example per changed component
  □ "Behavioral delta not yet verified" stated explicitly if not established

Coverage check:
  □ Component-level: all affected tests listed
  □ Pipeline-level: all pipeline stages have unit test evidence
  □ coverage-completeness-gate-SKILL.md check run

Verdict: VALID | INVALID — [reason if invalid]

If INVALID: revise the claim before STOP fires.
  Option A: Provide the behavioral observable (run a specific test that
            demonstrates the behavior, quote the assertion)
  Option B: Downgrade the claim to what unit test layer actually establishes:
            "Test count improved — behavioral improvement not yet verified"
```

---

## Section 4 — "No Behavioral Test" Declaration

If the session added tests but none verify a behavioral observable:

```
BEHAVIORAL LAYER STATUS: UNIT TESTS ONLY.
Test count improved. Behavioral improvement not yet verified.
Claim revised to: "[test count] tests added.
  Behavioral delta: not established — [what specific test would verify it]."
```

This is not a failure. It is an honest claim. A session that correctly states
"tests added, behavior not yet verified" produces more useful output than
one that claims "+15 tests = capability improved" without establishing observable delta.

---

## Section 5 — Anti-Patterns

1. **"Test count rose — capability improved"** — tests measure coverage, not
   correctness of results. Establish behavioral delta.

2. **"Build passes — feature is complete"** — build confirms compilation, not
   correct behavior. State what the component does for a specific input.

3. **"It's obvious the search got better — BM25 was added"** — "obvious" is
   prediction. State the concrete delta: "HybridRetriever.SearchAsync('neural network')
   returns results ranked by combined BM25 + cosine score."

4. **"The test asserts the behavior"** — quote the assertion. Do not reference the
   test by name. The claim must stand without reading the test file.

---

## Integration

- output-skepticism-SKILL.md Q3 (proxy check) addresses the same question:
  "could this metric improve without the underlying capability improving?"
- coverage-completeness-gate-SKILL.md verifies that the claimed scope
  matches the evidence scope.
- capability-measurement-SKILL.md provides the 6-class scoring framework —
  the behavioral layer evidence here feeds into capability-measurement scores.
