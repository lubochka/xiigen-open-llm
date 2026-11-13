# Layer 0-B — Router Pairs (018)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-018-001 — [QA] alone: production handoff sign-off

PredicateSet: ["QA"]
Scenario: ProductionHandoff
Input: "Collect sign-off from the operations team for the production handoff."

Routing decision:
1. QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution
OutputFiles: [sign_off_register.md, handoff_confirmation.md]

---

## B38-0B-018-002 — [QA, IMPLEMENT]: handoff with service

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: PaymentServiceProductionHandoff
Input: "Verify the payment service handoff to the operations team and approve the go-live confirmation."

Routing decisions:
1. IMPLEMENT → Phase 4.1
2. QA → Phase 7.1 — QA last

DecisionCount: 2
TransitionType: feature_addition
CapabilityGapWarnings: ["go-live"]

---

## B38-0B-018-003 — [QA, DOCUMENT]: handoff documentation

PredicateSet: ["QA", "DOCUMENT"]
Scenario: ProductionHandoffDocumentation
Input: "Document the production handoff criteria and validate that all acceptance criteria are satisfied."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution

---

## B38-0B-018-004 — Knowledge_edge: handoff runbook gap

PredicateSet: []
Scenario: HandoffRunbookGap
Input: "Execute the handoff runbook and monitor the production environment after cutover."

Routing: knowledge_edge
CapabilityGapWarnings: ["execute", "runbook", "monitor", "cutover"]
DecisionCount: 1 (knowledge_edge sentinel)
TransitionType: N/A
