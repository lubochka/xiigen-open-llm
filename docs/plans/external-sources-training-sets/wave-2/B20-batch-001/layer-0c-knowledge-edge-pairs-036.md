# Layer 0-C — Knowledge Edge Pairs (036) — Complete Round 2 gap reference for financial calculators
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E036-001 — Complete execution verb gap list (Round 2 confirmed)

Execution verbs that always produce zero predicates:
  run, execute, trigger, launch, invoke, start — core execution verbs
  calculate, compute, derive, determine, evaluate, process — domain computation verbs
  accrue, amortise, depreciate, capitalise, pay, disburse — financial domain verbs
  apply (standalone), distribute, award, grant — business operation verbs

---

## E036-002 — Complete informal QA gap list (Round 2 confirmed)

Informal QA phrases that always produce zero predicates:
  check, confirm, ensure, make sure — the four canonical informal QA verbs
  "verify" is NOT informal QA — "verify" is in the TEST predicate lexicon and always fires TEST.
  Critical boundary: "verify" (fires TEST) vs "confirm" (does not fire TEST).

---

## E036-003 — Complete "not IMPLEMENT" verb list (Round 2 confirmed)

Verbs confirmed as NOT in the IMPLEMENT predicate lexicon:
  update, modify, change, fix, patch, adjust, tweak, revise, apply, alter, refine
  IMPLEMENT lexicon (complete): write, create, build, develop, implement, add, extend, generate.
  No other verbs fire IMPLEMENT.

---

## E036-004 — "testable" vs "test" boundary reference (Round 2 reinforced)

"testable" (adjective) → never fires TEST. Example: "is testable", "testable class", "testable architecture"
"test" (noun/verb/gerund) → always fires TEST. Example: "write tests", "run tests" (note: "run" is zero but "tests" fires TEST separately)
Wait — correction: "run tests" produces zero predicates because "run" is the action verb parsed first.
"run tests" → action verb "run" → zero predicates. "tests" as object of execution verb does not independently fire TEST.
"write tests" → action verb "write" fires IMPLEMENT; "tests" fires TEST. Both parsed.
This is the fundamental parsing rule: the action verb governs; nouns can supplement but cannot override a zero-predicate verb context.
