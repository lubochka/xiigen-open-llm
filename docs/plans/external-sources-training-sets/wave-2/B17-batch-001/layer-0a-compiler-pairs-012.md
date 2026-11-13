# Layer 0-A — Compiler Pairs (012)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler | focus: complete rule table

---

## B17-0A-012-001 — Complete B17 predicate rule table

| Vocabulary trigger | Predicate fired | Notes |
|---|---|---|
| build, implement, write, create, code, develop | IMPLEMENT | Build verbs |
| refactor, restructure, rewrite | IMPLEMENT | Also sets TransitionType: refactoring |
| integrate | API_CONTRACT | NEVER IMPLEMENT (Rule 1 — top rule) |
| connector | API_CONTRACT | Any position (Rule 4) |
| webhook | API_CONTRACT | (Rule 5) |
| wire up | API_CONTRACT | (Rule 22) |
| connect + external service | API_CONTRACT | (Rule 22) |
| API, APIs, API endpoint | API_CONTRACT | Reinforcer |
| external service, third-party | API_CONTRACT context | Shifts IMPLEMENT to 4.3 |
| token | SECURITY | (Rule 6) |
| authentication | SECURITY | (Rule 8) |
| secure, validate signature | SECURITY | (Rule 8) |
| oauth (any case) | AUTHORIZATION | (Rule 7) |
| document, spec out, spec | DOCUMENT | (Rule 12) |
| write tests, create tests, add tests | TEST | (Rule 13) |
| QA (abbreviation only) | QA | NOT "quality assurance" (Rule 21) |

---

## B17-0A-012-002 — IMPLEMENT sub-phase routing table

| IMPLEMENT context vocabulary | Sub-phase |
|---|---|
| connector, integration, webhook, API, external service, third-party | 4.3 |
| service, logic, class, function, method (no integration vocabulary) | 4.1 |
| frontend, UI, component, page, view | 4.2 |
| migration, migrate, schema | 4.4 |
| config, flag, feature flag, environment | 4.5 |

**Rule:** The FIRST integration-context vocabulary item found shifts IMPLEMENT to 4.3, overriding any service/logic vocabulary also present.

---

## B17-0A-012-003 — TransitionType hierarchy for B17

| Highest-priority verb present | TransitionType |
|---|---|
| refactor, restructure, rewrite | refactoring |
| build, implement, write, create, code, develop (no refactor) | feature_addition |
| document, spec (no IMPLEMENT) | documentation |
| API_CONTRACT alone, no build or document | documentation |
| TEST or DOCUMENT alone (no IMPLEMENT) | no contribution or documentation |
| QA alone | no contribution |
| No predicates fire | none |

---

## B17-0A-012-004 — Predicate canonical ordering (Rule 20)

**Fixed canonical sequence:**
1. IMPLEMENT
2. API_CONTRACT
3. SECURITY
4. AUTHORIZATION
5. TEST
6. DOCUMENT
7. QA (always last — Rule 21)

**Example:** Input fires TEST, AUTHORIZATION, IMPLEMENT in that order.
**Correct output:** ["IMPLEMENT", "AUTHORIZATION", "TEST"] (canonical order, not input order)

**v6 error note:** Emitting predicates in input-encounter order is a v6-01 variant. Always reorder to canonical sequence before output.
