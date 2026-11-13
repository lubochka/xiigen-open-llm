# Layer 3 — Teaching Unit Pairs (012)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-012-001 — Complete B17 teaching summary: all rules

**The 12 most important B17 teaching points:**

1. "integrate" → API_CONTRACT ONLY. Never IMPLEMENT. (Rule 1 — top priority)
2. IMPLEMENT routes to 4.3 when integration vocabulary is co-present; 4.1 without integration vocabulary (Rule 2/3)
3. "connector" always fires API_CONTRACT (Rule 4)
4. "webhook" always fires API_CONTRACT (Rule 5)
5. "token" always fires SECURITY (Rule 6)
6. "oauth" (any case) always fires AUTHORIZATION (Rule 7)
7. "authentication" fires SECURITY, not AUTHORIZATION (Rule 8)
8. SECURITY and AUTHORIZATION both route to 3.4; DecisionCount counts predicates, not sub-phases (Rule 9)
9. API_CONTRACT alone → 3.3 (not 4.3); 4.3 requires IMPLEMENT (Rule 10)
10. "wire up" = API_CONTRACT (Rule 22)
11. "refactor" → IMPLEMENT + TransitionType: refactoring (Rule 14)
12. QA fires only on "QA" abbreviation; always last (Rule 21)

---

## B17-TU-012-002 — Complete predicate → sub-phase routing map

| Predicate | Default sub-phase | Override condition |
|---|---|---|
| IMPLEMENT | 4.1 | Integration vocabulary → 4.3 |
| API_CONTRACT | 3.3 | None |
| SECURITY | 3.4 | None |
| AUTHORIZATION | 3.4 | None |
| TEST | 5.1 | None |
| DOCUMENT | 2.1 | None |
| QA | 7.x | None (always last) |

---

## B17-TU-012-003 — Three v6 corruptions and their corrections

| v6 code | What goes wrong | How to detect | Correction |
|---|---|---|---|
| v6-01 | "integrate" → IMPLEMENT invented | IMPLEMENT in output with no build verb in input | "integrate" → API_CONTRACT only |
| v6-02 | "token" or "oauth" predicate suppressed | SECURITY or AUTHORIZATION absent despite trigger word present | Each trigger word must produce its predicate |
| v6-03 | IMPLEMENT → 4.1 despite integration vocabulary | 4.1 in output when connector/webhook/API in input | Integration vocabulary → 4.3, overrides 4.1 |

---

## B17-TU-012-004 — B17 at a glance: one-sentence summaries

- **Scenario:** Building integration connectors for third-party payment and communication services
- **Primary flow:** IntegrationConnectorFlow43 (nine-node, sub-phase 4.3)
- **Top rule:** "integrate" → API_CONTRACT only, never IMPLEMENT
- **Top error:** v6-01 (integrate → IMPLEMENT) or v6-03 (IMPLEMENT → 4.1 instead of 4.3)
- **Core predicate set:** IMPLEMENT + API_CONTRACT (the minimal B17 connector build)
- **Full predicate set:** IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION + TEST + DOCUMENT + QA
- **DomainId:** software-development (always)
- **No GapHandler** in B17
