# Layer 0-D — Flow Factory Pairs (015)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-015-001 — ARCHITECTURE+SECURITY / 3.5+6.3 → two flows (secrets topology)

A:
  sub_phase: [3.5, 6.3]
  predicate: ["ARCHITECTURE", "SECURITY"]
  compiled_text: "Design the secrets management topology — Vault cluster, dynamic secret rotation, and certificate lifecycle."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "SecurityAuditFlow63"
      predicate: SECURITY
      sub_phase: 6.3
  SelectionReason: "ARCHITECTURE + SECURITY (secret, certificate) → two concurrent flows; both deterministic_generic"

---

## B14-factory-015-002 — IMPLEMENT+SECURITY / 4.1+6.3 → two flows (secret rotation)

A:
  sub_phase: [4.1, 6.3]
  predicate: ["IMPLEMENT", "SECURITY"]
  compiled_text: "Implement the secret rotation policy and key rotation schedule for the production credential store."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
    - FlowId: "SecurityAuditFlow63"
      predicate: SECURITY
      sub_phase: 6.3
  SelectionReason: "IMPLEMENT + SECURITY → feature_addition; both flows deterministic_generic"

---

## B14-factory-015-003 — ARCHITECTURE+SECURITY (Secrets Manager) / 3.5+6.3 → two flows

A:
  sub_phase: [3.5, 6.3]
  predicate: ["ARCHITECTURE", "SECURITY"]
  compiled_text: "Architect the AWS Secrets Manager integration topology — secret injection, IAM role binding, and rotation triggers."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "SecurityAuditFlow63"
      predicate: SECURITY
      sub_phase: 6.3
  SelectionReason: "ARCHITECTURE (topology) + SECURITY (secret, credential) → two concurrent flows"
