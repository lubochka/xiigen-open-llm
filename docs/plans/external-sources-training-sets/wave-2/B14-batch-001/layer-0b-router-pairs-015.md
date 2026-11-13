# Layer 0-B — Router Pairs (015)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-015-001 — ARCHITECTURE+SECURITY → 3.5 + 6.3 (secrets topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "SECURITY"]
  compiled_text: "Design the secrets management topology — Vault cluster, dynamic secret rotation, and certificate lifecycle."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'secrets management topology' → ARCHITECTURE → 3.5"
    - predicate: SECURITY
      sub_phase: 6.3
      routing_note: "'secret', 'certificate' → SECURITY → 6.3"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.3]

---

## B14-router-015-002 — IMPLEMENT+SECURITY → 4.1 + 6.3 (secret rotation policy)

A:
  compiled_predicate_ids: ["IMPLEMENT", "SECURITY"]
  compiled_text: "Implement the secret rotation policy and key rotation schedule for the production credential store."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' → IMPLEMENT → 4.1"
    - predicate: SECURITY
      sub_phase: 6.3
      routing_note: "'secret rotation', 'key rotation', 'credential' → SECURITY → 6.3"
  TransitionType: feature_addition
  PrimarySubPhaseId: 4.1
  AllSubPhaseIds: [4.1, 6.3]

---

## B14-router-015-003 — ARCHITECTURE+SECURITY → 3.5 + 6.3 (AWS Secrets Manager)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "SECURITY"]
  compiled_text: "Architect the AWS Secrets Manager integration topology — secret injection, IAM role binding, and rotation triggers."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'architect' + 'integration topology' → ARCHITECTURE → 3.5"
    - predicate: SECURITY
      sub_phase: 6.3
      routing_note: "'secret injection', 'IAM role binding' (credential binding) → SECURITY → 6.3"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.3]

---

## B14-router-015-004 — gap → knowledge_edge (rotate the secrets)

A:
  compiled_predicate_ids: []
  compiled_text: "Rotate the database credentials in AWS Secrets Manager."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings:
    - "gap-devops-secrets-001: 'rotate' alone, 'database credentials', 'AWS Secrets Manager' are gap vocabulary"
