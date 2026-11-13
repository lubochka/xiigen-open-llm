# Layer 0-A — Compiler Pairs (024)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-024-001 — Complete predicate rule reference: IMPLEMENT triggers

IMPLEMENT fires on: build, implement, create, scaffold, bootstrap, add (code artifact), write (code), extend, develop, code, generate, refactor.
All produce TransitionType: feature_addition EXCEPT refactor → TransitionType: refactoring.
Deduplication: multiple IMPLEMENT triggers in one request → single IMPLEMENT predicate.

---

## B15-compiler-024-002 — Complete predicate rule reference: cross-predicate triggers

| Term | Predicate Fired |
|------|----------------|
| contract / data contracts / service contracts / API contracts | API_CONTRACT |
| integrate / wire up / connect | API_CONTRACT |
| domain model / bounded context / architecture / architect | ARCHITECTURE |
| secure / harden / protect / encrypt / authenticate | SECURITY |
| write tests / create tests / unit tests / integration tests | TEST |
| document / spec out / define spec | DOCUMENT |
| QA (abbreviation) | QA |

---

## B15-compiler-024-003 — v6 corruption type reference

| Type | Description | Example |
|------|-------------|---------|
| v6-01 | IMPLEMENT suppressed — domain noun treated as gap | "chargeback" confuses compiler into returning [] |
| v6-02 | Secondary predicate suppressed — "domain model" or "contract" not caught | ARCHITECTURE or API_CONTRACT missing from output |
| v6-03 | Multi-predicate collapsed — only dominant predicate returned | Four predicates → DecisionCount: 1 returned |

---

## B15-compiler-024-004 — Deduplication rule confirmed

Same predicate, multiple triggers: "build" + "implement" + "create" in one request → still ["IMPLEMENT"] (single predicate, not three).
Different predicates, one trigger each: "build" + "integrate" → ["IMPLEMENT", "API_CONTRACT"] (two predicates, two decisions).
Deduplication is per-predicate, not per-trigger.

---

## B15-compiler-024-005 — TransitionType override rule

refactor verb present → TransitionType: refactoring (overrides feature_addition even if other IMPLEMENT verbs also present).
extend/add/build only → TransitionType: feature_addition.
No IMPLEMENT verb at all → TransitionType: null (no IMPLEMENT predicate fired).
