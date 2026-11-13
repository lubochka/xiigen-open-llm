# Layer 0-B — Router Pairs (022)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-022-001 — ARCHITECTURE → 3.5 (VPC topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the VPC network topology — public and private subnet boundaries, NAT gateway placement, and inter-VPC peering."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'VPC network topology' + 'subnet boundaries' + 'NAT gateway placement' → ARCHITECTURE → 3.5"
  TransitionType: feature_addition
  SubPhaseId: 3.5

---

## B14-router-022-002 — ARCHITECTURE+SECURITY → 3.5 + 6.3 (zero-trust network)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "SECURITY"]
  compiled_text: "Architect the zero-trust network topology — deny-all NetworkPolicy baseline, PrivateLink service boundaries, and mTLS enforcement."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'architect' + 'zero-trust network topology' + 'service boundaries' → ARCHITECTURE → 3.5"
    - predicate: SECURITY
      sub_phase: 6.3
      routing_note: "'zero-trust', 'mTLS' ('tls') → SECURITY → 6.3"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.3]

---

## B14-router-022-003 — ARCHITECTURE+IMPLEMENT+SECURITY → 3.5 + 4.1 + 6.3 (VPC security config)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  compiled_text: "Design the VPC security topology and implement the security group rules and TLS certificate configuration."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'VPC security topology' → ARCHITECTURE → 3.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' + 'security group rules' → IMPLEMENT → 4.1"
    - predicate: SECURITY
      sub_phase: 6.3
      routing_note: "'TLS certificate configuration' ('tls', 'certificate') → SECURITY → 6.3"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1, 6.3]

---

## B14-router-022-004 — gap → knowledge_edge (open port 443)

A:
  compiled_predicate_ids: []
  compiled_text: "Open port 443 on the production security group."

B:
  RoutingDecisions: []
  TransitionType: unknown
  TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings:
    - "gap-devops-network-001: 'open port', 'security group', 'port 443' are operational gap vocabulary"
