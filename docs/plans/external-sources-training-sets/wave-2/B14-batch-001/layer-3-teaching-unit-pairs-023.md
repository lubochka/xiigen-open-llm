# Layer 3 — Teaching Unit Pairs (023)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T023-001 — "scaffold the Terraform modules" fires IMPLEMENT

A: "Scaffold the Terraform modules for the VPC." Predicates?
B: ["IMPLEMENT", "ARCHITECTURE"]. "scaffold" → IMPLEMENT (IaC scaffolding action). "Terraform modules" → IMPLEMENT (code artifact). "VPC" → gap. "infrastructure" implied → ARCHITECTURE? Not fired here — only "scaffold" + "modules" without explicit architecture vocabulary. Result: ["IMPLEMENT"] only.
  Correction: If the sentence includes "infrastructure layer" or explicit ARCHITECTURE noun: both fire. Without it: only IMPLEMENT.

---

## T023-002 — "write the Dockerfile" fires IMPLEMENT

A: "Write the multi-stage Dockerfile for the API service." Predicates?
B: ["IMPLEMENT"]. "write" → IMPLEMENT. "Dockerfile" → IMPLEMENT (container code artifact). "API service" → gap. Route to 4.1.

---

## T023-003 — "configure" in Helm chart context fires IMPLEMENT

A: "Configure the RBAC roles in the Helm chart." Predicates?
B: ["IMPLEMENT", "SECURITY"]. "configure" in Helm chart (code artifact) context → IMPLEMENT. "RBAC roles" → SECURITY ("credential", access control binding). Two flows: 4.1 + 6.3.

---

## T023-004 — IMPLEMENT+ARCHITECTURE for IaC with topology design

A: "Design the infrastructure module architecture and implement the Terraform modules for the networking layer." Predicates?
B: ["ARCHITECTURE", "IMPLEMENT"]. "design" + "infrastructure module architecture" → ARCHITECTURE. "implement" + "Terraform modules" → IMPLEMENT. "networking layer" → ARCHITECTURE via "layer". TransitionType: feature_addition. Two flows: 3.5 + 4.1.

---

## T023-005 — IMPLEMENT+ARCHITECTURE+SECURITY for secure IaC

A: "Implement the Terraform module, design the infrastructure topology, and configure the TLS certificate chain." Predicates?
B: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]. "implement" + "Terraform module" → IMPLEMENT. "design" + "infrastructure topology" → ARCHITECTURE. "TLS certificate chain" → SECURITY ("tls", "certificate"). Three flows: 4.1 + 3.5 + 6.3. TransitionType: feature_addition.
