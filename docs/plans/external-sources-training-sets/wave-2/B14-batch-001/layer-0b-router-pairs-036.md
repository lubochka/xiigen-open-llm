# Layer 0-B — Router Pairs (036)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-036-001 — Complete routing table: all 6 B14 predicates → sub-phases

ARCHITECTURE → 3.5 (InfrastructureArchitectureFlow35) — infra vocabulary required for 3.5 vs 3.1
IMPLEMENT → 4.1 (BackendServiceFlow41) — any code artifact in IaC/DevOps context
SECURITY → 6.3 (SecurityAuditFlow63) — TLS, secret, certificate, key rotation, vault, zero-trust
AUTHORIZATION → authorization sub-phase — IAM roles, RBAC roles, service account binding, trust policies
API_CONTRACT → 2.1 (APIContractFlow21) — API contracts, endpoint definitions, OpenAPI spec
DOCUMENT → 6.2 (DocumentationFlow62) — ADRs, runbooks, documentation artifacts

---

## B14-router-036-002 — Complete ordering rule: all 6 predicates concurrent

When all 6 predicates fire concurrently:
  Sequential: 3.5 (ARCHITECTURE) → 2.1 (API_CONTRACT) → 6.3 (SECURITY) → AUTHORIZATION → 4.1 (IMPLEMENT) → 6.2 (DOCUMENT)
  Design before contract definition; security before implementation; documentation after all decisions
  PrimarySubPhaseId: 3.5 (ARCHITECTURE takes primacy)

---

## B14-router-036-003 — Routing rule: ARCHITECTURE 3.5 vs 3.1 final disambiguation

Infra vocabulary (→ 3.5): infrastructure, topology, cluster, node, mesh, VPC, network, CDN, edge, container, Kubernetes, GitOps, observability, pipeline (infra), DR (Disaster Recovery)
App vocabulary (→ 3.1): domain, service boundary, bounded context, aggregate, DDD (Domain-Driven Design), CQRS, SAGA, module (app-level)
Tie-breaker: infra vocabulary density > app vocabulary → 3.5; app vocabulary density > infra → 3.1

---

## B14-router-036-004 — Routing invariant: gap vocabulary never degrades routing

Gap vocabulary in a request does not downgrade existing predicate routing:
  "Design the AWS EKS topology" → ARCHITECTURE fires on "design" + "topology"; "AWS", "EKS" are gap but do not suppress 3.5 routing
  CapabilityGapWarnings emitted for gap vocabulary but routing proceeds normally for all fired predicates
  TrustState: deterministic_generic whenever at least one predicate fires
