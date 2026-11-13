# Layer 0-D — Flow Factory Pairs (012)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-012-001 — Complete B38 factory map table

| PredicateSet | Flows Created | Phases | TransitionType | result_state |
|---|---|---|---|---|
| [] | [] | none | null | knowledge_edge |
| ["QA"] | [ReleaseGateFlow71] | [7.1] | no_contribution | routed |
| ["IMPLEMENT"] | [BackendServiceFlow41] | [4.1] | feature_addition | routed |
| ["DOCUMENT"] | [DocumentationFlow] | [2.1] | no_contribution | routed |
| ["SECURITY"] | [SecurityAuditFlow] | [3.4] | no_contribution | routed |
| ["QA","DOCUMENT"] | [DocumentationFlow, ReleaseGateFlow71] | [2.1, 7.1] | no_contribution | routed |
| ["QA","IMPLEMENT"] | [BackendServiceFlow41, ReleaseGateFlow71] | [4.1, 7.1] | feature_addition | routed |
| ["QA","SECURITY"] | [SecurityAuditFlow, ReleaseGateFlow71] | [3.4, 7.1] | no_contribution | routed |
| ["QA","IMPLEMENT","DOCUMENT"] | [BackendServiceFlow41, DocumentationFlow, ReleaseGateFlow71] | [4.1, 2.1, 7.1] | feature_addition | routed |
| ["QA","IMPLEMENT","SECURITY"] | [BackendServiceFlow41, SecurityAuditFlow, ReleaseGateFlow71] | [4.1, 3.4, 7.1] | feature_addition | routed |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | [BackendServiceFlow41, DocumentationFlow, SecurityAuditFlow, ReleaseGateFlow71] | [4.1, 2.1, 3.4, 7.1] | feature_addition | routed |

---

## B38-0D-012-002 — Factory invariants

1. len(flows) == len(predicate_set)
2. ReleaseGateFlow71 always last in flows list when QA present
3. BackendServiceFlow41 always in flows list when IMPLEMENT present
4. DocumentationFlow always in flows list when DOCUMENT present
5. SecurityAuditFlow always in flows list when SECURITY present
6. knowledge_edge iff predicate_set == []
7. feature_addition iff IMPLEMENT in predicate_set
8. Probationary flow requires Gate B pass before execution

---

## B38-0D-012-003 — Factory registration table for B38

| FlowId | Phase | Predicate | DefaultType |
|---|---|---|---|
| ReleaseGateFlow71 | 7.1 | QA | deterministic_generic |
| BackendServiceFlow41 | 4.1 | IMPLEMENT | deterministic_generic |
| DocumentationFlow | 2.1 | DOCUMENT | deterministic_generic |
| SecurityAuditFlow | 3.4 | SECURITY | deterministic_generic |

All flows registered as deterministic_generic in batch-001. Probationary variants exist as _v2 candidates in the registry but are not the default for this batch.
