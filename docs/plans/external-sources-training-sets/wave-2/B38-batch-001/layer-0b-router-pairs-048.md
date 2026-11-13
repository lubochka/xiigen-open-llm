# Layer 0-B — Router Pairs (048)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-048-001 — Complete routing reference: 037-048 changelog and communication batch

This file is a complete routing reference for files 037-048.

### Routing summary for 037-048
| Scenario | PredicateSet | Phases | DecisionCount | TransitionType |
|---|---|---|---|---|
| ChangelogAutomation — knowledge_edge | [] | none | 0 | N/A |
| ChangelogAutomation — QA | ["QA"] | 7.1 | 1 | no_contribution |
| ChangelogAutomation — IMPLEMENT | ["IMPLEMENT"] | 4.1 | 1 | feature_addition |
| ChangelogAutomation — QA+DOCUMENT | ["QA","DOCUMENT"] | 2.1, 7.1 | 2 | no_contribution |
| ChangelogAutomation — QA+IMPLEMENT+SECURITY | ["QA","IMPLEMENT","SECURITY"] | 4.1, 3.4, 7.1 | 3 | feature_addition |
| ReleaseNoteTemplate — DOCUMENT | ["DOCUMENT"] | 2.1 | 1 | no_contribution |
| ReleaseNoteTemplate — QA+IMPLEMENT+DOCUMENT | ["QA","IMPLEMENT","DOCUMENT"] | 4.1, 2.1, 7.1 | 3 | feature_addition |
| MigrationGuide — IMPLEMENT+DOCUMENT | ["IMPLEMENT","DOCUMENT"] | 4.1, 2.1 | 2 | feature_addition |
| MigrationGuide — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| SecurityAdvisory — DOCUMENT+SECURITY | ["DOCUMENT","SECURITY"] | 2.1, 3.4 | 2 | no_contribution |
| SecurityAdvisory — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| CVENotification — DOCUMENT+SECURITY | ["DOCUMENT","SECURITY"] | 2.1, 3.4 | 2 | no_contribution |
| CVENotification — QA+IMPLEMENT+DOCUMENT+SECURITY | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |

---

## B38-0B-048-002 — New routing patterns established in 037-048

1. DOCUMENT+SECURITY (no QA): two-phase routing to 2.1 and 3.4 only — no 7.1 unless QA fires.
2. SECURITY alone (no QA): single-phase routing to 3.4 only.
3. IMPLEMENT+DOCUMENT (no QA): two-phase routing to 4.1 and 2.1 only.
4. All patterns without QA are valid — QA-last rule only applies when QA is present.

---

## B38-0B-048-003 — Anti-corruption for 037-048

Corrupt: SecurityAdvisory ["DOCUMENT","SECURITY"] → DecisionCount: 1 (only 7.1)
Correct: DecisionCount: 2 (2.1 for DOCUMENT, 3.4 for SECURITY) — QA not in predicate set, no 7.1.

Corrupt: CVENotification ["SECURITY"] → DecisionCount: 0 (knowledge_edge)
Correct: DecisionCount: 1 (3.4 for SECURITY) — SECURITY alone is a valid single-predicate set.
