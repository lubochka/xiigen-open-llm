# Coverage Completeness Gate

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

## Pass06 B-1..B-22 / NOT_READY Continuation Guard

For ILearningDynamicGraph/DPO plan-readiness claims, `B-1..B-22 unresolved blocker inventory is mandatory`. Missing blocker numbers are trace defects, not permission to drop them. Older B1-B21 or B5-absent evidence must be labelled historical/trace-repair evidence and kept as backlog, including `MISSING_NUMBER_NEEDS_TRACE_REPAIR` where the source is grouped, absent, renamed, or ambiguous.

`PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary`. `PLAN_NOT_READY` is a bounded repair queue, not a stop, while safe plan/governance repair remains. Future Gate B may contain tests/builds/runtime/proof collection after approval, but it may not hide unresolved design/dependency decisions, missing source-to-plan rows, missing blocker numbers, provider/repo/contract/provenance/threshold/model lifecycle choices, or active-vs-Claude drift. Current-looking `READY`, `NOT_READY`, `BLOCKED`, `DONE`, or similar retained evidence must be labelled historical/evidence-only with the current verdict nearby.

A session that improves 3 subprojects and claims "the search pipeline is
complete" has not lied — 3 subprojects did improve. It has overclaimed — the
pipeline result does not follow from 3 data points if the pipeline has 6
subprojects. This skill gates improvement claims before every STOP so that
the evidence scope never falls short of the claimed scope.

---

## When to Invoke

Fires before every STOP that contains a claim of improvement, completion,
or progress.

Trigger phrases (any of these in the response activates the gate):
- "improved", "fixed", "resolved", "complete", "done"
- percentage expressions: "-X%", "X% reduction", "X% improvement"
- completion claims: "all tests pass", "all subprojects updated", "pipeline complete"
- progress claims: "significantly improved", "major progress"

One gate check before STOP = no overclaimed scope in the session output.

---

## Section 1 — The Coverage Gate Table

| Claim type | Required coverage before claim is valid |
|------------|----------------------------------------|
| "This subproject improved" | All affected classes in that subproject tested |
| "These N subprojects improved" | All N subprojects have passing test deltas |
| "Pipeline is improving" | ≥ 2 of N pipeline components have test evidence |
| "Pipeline is complete" | All N pipeline components pass; dotnet test → 0 failed |
| "This interface is complete" | Implementation class + all integration tests pass |
| "X% improvement in [metric]" | Metric is from `dotnet test` output, not estimated |

---

## Section 2 — Coverage Check Before STOP

For every claim trigger phrase detected in the response, run this check
before STOP fires:

```
COVERAGE CHECK (coverage-completeness-gate):

Claim detected: "[exact quote]"
Claim type: [single subproject | N subprojects | pipeline | interface | metric]

Coverage required: [per table in Section 1]
Coverage actual:
  This session touched: [list subprojects or classes]
  Each fully tested (dotnet test evidence): [YES/NO per item]
  Test count delta: [+N from baseline]

Coverage verdict:
  SUFFICIENT — claim matches evidence scope → STOP may fire
  INSUFFICIENT — [actual coverage] < [required coverage] → revise claim
```

If INSUFFICIENT: do not suppress the response. Downgrade the claim per Section 3.

---

## Section 3 — Claim Downgrade Protocol

When coverage is insufficient, the claim is downgraded to match actual evidence.
The work is preserved. The claim is revised.

| Original claim | Coverage gap | Downgraded claim |
|---|---|---|
| "Search pipeline is complete" | 3 of 6 components tested | "IBm25Scorer, IVectorIndex, HybridRetriever components pass. ISemanticCompiler29, PermissionGate, ToolRegistry not yet verified." |
| "All integration tests pass" | 2 of 5 integration scenarios tested | "BM25 + VectorSearch integration passes. HybridRetriever + SessionStorage + PermissionGate integration: not yet tested." |
| "50% improvement in search quality" | Counted grep matches, not test assertions | "50% reduction in failing assertion count in BM25ScorerTests. End-to-end quality unverified." |
| "Phase 6 complete" | DNA check not run | "Phase 6 code complete. DNA compliance not verified — run dna-compliance-guard-SKILL.md before claiming done." |

**Rule:** A downgraded claim is not a failure. It is an honest report. A session
that correctly identifies its own coverage limit is more trustworthy than one
that ships an unfalsifiable result.

---

## Section 4 — Threshold Reasoning

### Why 100% for "this subproject improved"

A subproject is a small scope (one directory, one namespace). If the
`SessionStateStore` was not tested, the session state behavior is unknown.
No partial "improved" for a specific class.

### Why dotnet test output for "X% improvement"

Internal code change counts (lines changed, methods renamed) do not
measure behavioral improvement. Only `dotnet test` output showing delta
from baseline counts as behavioral improvement evidence.

### Why the pipeline claim requires component evidence

Claiming the pipeline improved without testing each component is equivalent
to claiming a train arrived on time because the departure station looked
busy. Each component must show individual test evidence.

---

## Section 5 — Integration With Other Skills

**output-skepticism-SKILL.md** — skeptic question Q2 (scope validity) is the
upstream version of this gate. output-skepticism asks "does my evidence cover
my scope?" before drafting; coverage-completeness-gate verifies the answer at STOP.
If output-skepticism ran correctly, this gate should have little to catch.

**verification-before-completion-SKILL.md** — the iron law gate. This skill
gates SCOPE of claims; verification-before-completion gates FRESHNESS of evidence.
Both must pass before STOP fires.

**capability-measurement-SKILL.md** — provides the scored evidence this gate
reads to verify improvement claims. If a capability measurement was done, this
gate reads its evidence rather than requiring new verification commands.

---

## Section 6 — Anti-Patterns

1. **"I improved what I worked on — that's valid"** — improving what you
   worked on is valid AT THE SCOPE of what you worked on. The gate catches
   scope inflation: claiming pipeline improvement from component-level work.

2. **"Downgrading makes the session look bad"** — the opposite. A session that
   correctly identifies its coverage limit and states it explicitly is more
   trustworthy than one that overclaims.

3. **"The gate is too strict for short sessions"** — short sessions should
   make short-scope claims. A one-class session claiming one-class improvement
   needs only that one class fully tested. The gate is proportional to the claim.

4. **"I'll note the caveat in the notes section"** — a caveat alongside an
   overclaimed headline is still an overclaimed headline. The claim itself
   must be downgraded, not caveated.
