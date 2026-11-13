# Layer 0-A — Compiler Pairs (030)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Algolia search connector

---

## B17-0A-030-001 — Algolia: build connector

**Input:** "Build the Algolia search connector with index synchronization webhook."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-030-002 — Algolia: integrate

**Input:** "Integrate the Algolia search API for product lookup."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
"product lookup" is a domain phrase — no predicate.

---

## B17-0A-030-003 — Algolia: build with token

**Input:** "Build Algolia connector using the admin API token for index writes."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6; unconditional) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-030-004 — Algolia: implement (no connector vocab)

**Input:** "Implement Algolia faceted search for the product catalogue."

Triggers: "Implement" → IMPLEMENT
No integration vocabulary → IMPLEMENT routes to 4.1
CompiledPredicateIds: ["IMPLEMENT"]
DecisionCount: 1 | UniqueSubPhases: 1 (4.1)
"Algolia" proper noun: no predicate. "catalogue": no predicate.
