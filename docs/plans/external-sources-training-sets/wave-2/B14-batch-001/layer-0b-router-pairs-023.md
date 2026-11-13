# Layer 0-B — Router Pairs (023)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-023-001 — ARCHITECTURE+IMPLEMENT → 3.5 + 4.1 (Terraform module scaffold)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Design the infrastructure module architecture and scaffold the Terraform modules for the VPC and compute layers."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'infrastructure module architecture' + 'compute layers' → ARCHITECTURE → 3.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'scaffold' + 'Terraform modules' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1]

---

## B14-router-023-002 — IMPLEMENT+ARCHITECTURE → 4.1 + 3.5 (Helm chart creation)

A:
  compiled_predicate_ids: ["IMPLEMENT", "ARCHITECTURE"]
  compiled_text: "Create the Helm chart structure for the microservices deployment and configure the values hierarchy."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'create' + 'Helm chart structure' + 'configure' → IMPLEMENT → 4.1"
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'values hierarchy' ('layer') → ARCHITECTURE → 3.5"
  TransitionType: feature_addition
  PrimarySubPhaseId: 4.1
  AllSubPhaseIds: [4.1, 3.5]

---

## B14-router-023-003 — IMPLEMENT+ARCHITECTURE+SECURITY → 4.1 + 3.5 + 6.3 (secure IaC)

A:
  compiled_predicate_ids: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
  compiled_text: "Implement the Terraform network module and ensure TLS certificate configuration and secrets injection are baked into the infrastructure topology."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' + 'Terraform network module' → IMPLEMENT → 4.1"
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'infrastructure topology' → ARCHITECTURE → 3.5"
    - predicate: SECURITY
      sub_phase: 6.3
      routing_note: "'TLS certificate configuration' + 'secrets injection' → SECURITY → 6.3"
  TransitionType: feature_addition
  PrimarySubPhaseId: 4.1
  AllSubPhaseIds: [4.1, 3.5, 6.3]

---

## B14-router-023-004 — IMPLEMENT+ARCHITECTURE → 4.1 + 3.5 (Ansible playbook)

A:
  compiled_predicate_ids: ["IMPLEMENT", "ARCHITECTURE"]
  compiled_text: "Write the Ansible playbook to provision the Kubernetes worker nodes and define the node group topology."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'write' + 'Ansible playbook' + 'provision' → IMPLEMENT → 4.1"
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'define' + 'node group topology' → ARCHITECTURE → 3.5"
  TransitionType: feature_addition
  PrimarySubPhaseId: 4.1
  AllSubPhaseIds: [4.1, 3.5]
