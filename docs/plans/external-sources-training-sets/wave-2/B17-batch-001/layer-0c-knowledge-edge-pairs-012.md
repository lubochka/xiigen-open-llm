# Layer 0-C — Knowledge-Edge Pairs (012)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-012-001 — B17 knowledge edge summary: all rules in one reference

**Predicate firing rules:**

| # | Rule | Trigger | Predicate | Exception |
|---|---|---|---|---|
| 1 | integrate → API_CONTRACT ONLY | integrate | API_CONTRACT | NEVER IMPLEMENT |
| 2 | IMPLEMENT routes to 4.3 when integration vocabulary present | connector, webhook, API, etc. | n/a (routing rule) | Without integration vocab → 4.1 |
| 3 | IMPLEMENT routes to 4.1 for service/logic vocabulary | service, logic, class, function | n/a | Overridden by Rule 2 |
| 4 | connector → API_CONTRACT | connector | API_CONTRACT | None |
| 5 | webhook → API_CONTRACT | webhook | API_CONTRACT | None |
| 6 | token → SECURITY | token | SECURITY | None |
| 7 | oauth → AUTHORIZATION (case-insensitive) | oauth, OAuth, OAUTH | AUTHORIZATION | None |
| 8 | authentication → SECURITY | authentication | SECURITY | Does not fire AUTHORIZATION |
| 9 | SECURITY and AUTHORIZATION both route to 3.4 | — | n/a (routing rule) | Two decisions, not one |
| 10 | API_CONTRACT alone → 3.3 (not 4.3) | — | n/a (routing rule) | 4.3 requires IMPLEMENT |
| 11 | build/implement/write/create/code/develop → IMPLEMENT | build, implement, etc. | IMPLEMENT | — |
| 12 | document/spec out/spec → DOCUMENT | document, spec | DOCUMENT | — |
| 13 | write/create/add tests → TEST | write tests | TEST | — |
| 14 | refactor → IMPLEMENT + refactoring TransitionType | refactor | IMPLEMENT | Overrides feature_addition |
| 15 | TransitionType hierarchy | refactoring > feature_addition > documentation > no contribution > none | — | — |
| 16 | IMPLEMENT present → feature_addition (unless refactor) | — | — | — |
| 17 | API_CONTRACT alone → documentation TransitionType | — | — | — |
| 18 | DomainId: software-development always | — | — | — |
| 19 | No GapHandler in B17 | — | — | — |
| 20 | Canonical predicate order: IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA | — | — | — |
| 21 | QA fires on "QA" abbreviation only; always last; QA alone → no contribution | QA | QA | "quality assurance" does not fire QA |
| 22 | wire up → API_CONTRACT; connect + external → API_CONTRACT | wire up, connect | API_CONTRACT | — |

---

## B17-0C-012-002 — Sub-phase routing summary

| Predicate | Sub-phase | Condition |
|---|---|---|
| IMPLEMENT | 4.3 | integration vocabulary co-present |
| IMPLEMENT | 4.1 | no integration vocabulary |
| API_CONTRACT | 3.3 | always |
| SECURITY | 3.4 | always |
| AUTHORIZATION | 3.4 | always |
| TEST | 5.1 | always |
| DOCUMENT | 2.1 | always |
| QA | 7.x | always |

---

## B17-0C-012-003 — Three v6 corruption summaries

**v6-01 (predicate invention):** "integrate" → IMPLEMENT (wrong); INTEGRATION predicate invented (wrong)
→ Correction: "integrate" → API_CONTRACT only; only valid predicates are IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA

**v6-02 (gap suppression):** "token" present, SECURITY not fired (wrong)
→ Correction: every trigger word must produce its predicate; check each trigger word against the output

**v6-03 (wrong sub-phase):** IMPLEMENT + connector → 4.1 (wrong)
→ Correction: IMPLEMENT + integration vocabulary → 4.3 always

---

## B17-0C-012-004 — B17 scenario invariants

1. DomainId is always "software-development"
2. No GapHandler activates in B17
3. probationary_accepted / probationary_rejected are factory-level metadata, not predicate or routing outcomes
4. ConnectorName is metadata; it does not affect predicate firing or routing
5. The canonical flow for B17 IMPLEMENT is always IntegrationConnectorFlow43 (4.3)
6. The canonical flow for B17 API_CONTRACT is always APIContractFlow33 (3.3)
