# Layer 0-B — Router Pairs (012)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-012-001 — Complete B38 routing decision table

| PredicateSet | DecisionCount | Phase Routing | TransitionType | ResultState |
|---|---|---|---|---|
| [] | 0 | none | N/A | knowledge_edge |
| ["QA"] | 1 | 7.1 | no_contribution | routed |
| ["IMPLEMENT"] | 1 | 4.1 | feature_addition | routed |
| ["QA","DOCUMENT"] | 2 | 2.1 + 7.1 | no_contribution | routed |
| ["QA","IMPLEMENT"] | 2 | 4.1 + 7.1 | feature_addition | routed |
| ["QA","SECURITY"] | 2 | 3.4 + 7.1 | no_contribution | routed |
| ["QA","IMPLEMENT","DOCUMENT"] | 3 | 4.1 + 2.1 + 7.1 | feature_addition | routed |
| ["QA","IMPLEMENT","SECURITY"] | 3 | 4.1 + 3.4 + 7.1 | feature_addition | routed |
| ["QA","DOCUMENT","SECURITY"] | 3 | 2.1 + 3.4 + 7.1 | no_contribution | routed |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4 | 4.1 + 2.1 + 3.4 + 7.1 | feature_addition | routed |

---

## B38-0B-012-002 — Phase ownership table

| Phase | Predicate | Flow | Sub-scenario |
|---|---|---|---|
| 7.1 | QA | ReleaseGateFlow71 | Release/Deploy Automation |
| 7.2 | QA + IMPLEMENT | (both) | CI/CD (sub-phase 7.2 covers both predicates) |
| 7.3 | TEST + QA | (both) | QA test automation |
| 7.4 | ARCHITECTURE + IMPLEMENT | (both) | Infrastructure Automation |
| 4.1 | IMPLEMENT | BackendServiceFlow41 | Backend service implementation |
| 2.1 | DOCUMENT | DocumentationFlow | Technical documentation |
| 3.4 | SECURITY | SecurityAuditFlow | Security audit and compliance |

Note: B38 sub-phase is 7.1. The predicates QA+IMPLEMENT together could also route to 7.2, but in this batch the canonical routing for IMPLEMENT is 4.1. The 7.2 routing applies when both predicates are CI/CD-scoped together.

---

## B38-0B-012-003 — QA ordering rule in routing table

QA ordering rule is absolute (Rule 12). In all multi-predicate routings:
- QA is always the last entry in the routing table
- QA always executes after all other predicate flows complete
- This applies regardless of input order of terms

Routing order for common combinations:
- [QA, IMPLEMENT]: IMPLEMENT (4.1) → QA (7.1)
- [QA, DOCUMENT]: DOCUMENT (2.1) → QA (7.1)
- [QA, SECURITY]: SECURITY (3.4) → QA (7.1)
- [QA, IMPLEMENT, DOCUMENT, SECURITY]: IMPLEMENT (4.1) → DOCUMENT (2.1) → SECURITY (3.4) → QA (7.1)

---

## B38-0B-012-004 — Routing correctness assertions (Gate B for router)

Gate B checks for B38 routing:
1. DecisionCount == len(PredicateSet) — one decision per predicate
2. QA always last in routing table when present
3. knowledge_edge iff PredicateSet == []
4. feature_addition iff "IMPLEMENT" in PredicateSet
5. DOCUMENT always routes to Phase 2.x (not 7.x)
6. SECURITY always routes to Phase 3.x (not 7.x)
7. CapabilityGapWarnings count >= 0; gap warnings do not reduce DecisionCount
