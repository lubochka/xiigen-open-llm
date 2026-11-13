# Layer 0-B — Router Pairs (036)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-036-001 — Complete routing reference: 025-036 extension summary

This file is a complete routing reference summary for all scenarios in the 025-036 extension batch.

### Routing summary by predicate count
| PredicateCount | Phases | Example scenario |
|---|---|---|
| 0 (knowledge_edge) | none | CalVer planning, nightly schedule, SemVer finalisation |
| 1 ["QA"] | 7.1 | SemVer approval, hotfix approval, RC sign-off |
| 1 ["DOCUMENT"] | 2.1 | SemVer criteria doc, nightly build criteria, weekly process doc |
| 1 ["SECURITY"] | 3.4 | Security-only audit (no co-predicate) |
| 2 ["QA","IMPLEMENT"] | 4.1, 7.1 | Nightly build validate, hotfix build, SemVer build |
| 2 ["QA","DOCUMENT"] | 2.1, 7.1 | Monthly release gate, patch notes sign-off, RC approval |
| 2 ["QA","SECURITY"] | 3.4, 7.1 | Pre-release tag signing audit, patch security audit |
| 2 ["QA","ARCHITECTURE"] | 5.x, 7.1 | Hotfix tagging arch, monthly release arch |
| 3 ["QA","IMPLEMENT","DOCUMENT"] | 4.1, 2.1, 7.1 | RC full validation, weekly release with criteria |
| 3 ["QA","IMPLEMENT","SECURITY"] | 4.1, 3.4, 7.1 | Patch build + security |
| 4 ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | Quarterly full gate, hotfix full review, RC four-predicate |
| 5 ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] | 4.1, 2.1, 3.4, 5.x, 7.1 | Patch five-predicate gate |

---

## B38-0B-036-002 — QA last rule verification for all 025-036 scenarios

For every multi-predicate scenario in the 025-036 extension:
- QA is always routed to Phase 7.1 at the last position
- No exception exists in this batch
- ARCHITECTURE does not contribute feature_addition regardless of position

QA ordering enforcement table:
| PredicateSet size | QA position |
|---|---|
| 1 | 1 (trivially last) |
| 2 | 2 |
| 3 | 3 |
| 4 | 4 |
| 5 | 5 |

---

## B38-0B-036-003 — Anti-corruption: v6-03 routing collapse examples for versioning

Corrupt case 1: SemanticVersioningBuildApproval ["QA","IMPLEMENT"] → DecisionCount: 1 (only 7.1)
Correct: DecisionCount: 2 (4.1 for IMPLEMENT, 7.1 for QA)

Corrupt case 2: QuarterlyReleaseFourPredicate ["QA","IMPLEMENT","DOCUMENT","SECURITY"] → DecisionCount: 1
Correct: DecisionCount: 4 (4.1, 2.1, 3.4, 7.1)

Corrupt case 3: PatchReleaseFivePredicateGate → DecisionCount: 3 (missing ARCHITECTURE and one other)
Correct: DecisionCount: 5 (all five phases)
