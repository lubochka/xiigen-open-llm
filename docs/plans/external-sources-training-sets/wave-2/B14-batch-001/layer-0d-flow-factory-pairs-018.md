# Layer 0-D — Flow Factory Pairs (018)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-018-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (DR topology)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the disaster recovery topology — active-passive failover, RTO < 15 minutes, RPO < 5 minutes."

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  SelectionReason: "ARCHITECTURE at 3.5 — DR topology = infra design"
  NodeCount: 9

---

## B14-factory-018-002 — ARCHITECTURE+DOCUMENT / 3.5+6.2 → two flows (DR runbook)

A:
  sub_phase: [3.5, 6.2]
  predicate: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Architect the disaster recovery failover procedure and document the runbook as an operational guide."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
  SelectionReason: "ARCHITECTURE + DOCUMENT → documentation TransitionType"

---

## B14-factory-018-003 — ARCHITECTURE+IMPLEMENT / 3.5+4.1 → two flows (DR IaC)

A:
  sub_phase: [3.5, 4.1]
  predicate: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Design the multi-region DR architecture and implement the Terraform failover configuration."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "ARCHITECTURE + IMPLEMENT → feature_addition"

---

## B14-factory-018-004 — TEST / 5.1 → UnitTestingFlow51 (test DR failover)

A:
  sub_phase: 5.1
  predicate: TEST
  compiled_text: "Test the disaster recovery failover by promoting the replica."

B:
  TrustState: deterministic_generic
  FlowId: "UnitTestingFlow51"
  SelectionReason: "TEST predicate at 5.1 — 'test' fires TEST; no ARCHITECTURE without design vocabulary"
  NodeCount: 9
