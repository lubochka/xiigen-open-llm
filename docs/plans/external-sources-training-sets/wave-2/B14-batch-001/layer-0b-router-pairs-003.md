# Layer 0-B — Router Pairs (003)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-003-001 — ARCHITECTURE+SECURITY → 3.5, 6.3

A: ["ARCHITECTURE", "SECURITY"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) + SubPhase 6.3 (SecurityAuditFlow63)
  Teaching: Security architecture reviews (zero-trust design, network segmentation, TLS termination placement) fire both ARCHITECTURE→3.5 and SECURITY→6.3.

---

## B14-router-003-002 — ARCHITECTURE+QA → 3.5, 7.3

A: ["ARCHITECTURE", "QA"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) + SubPhase 7.3 (QAGatingFlow73)
  Teaching: Architecture designs requiring QA sign-off route ARCHITECTURE→3.5 and QA→7.3 independently.

---

## B14-router-003-003 — ARCHITECTURE+API_CONTRACT → 3.5, 3.3

A: ["ARCHITECTURE", "API_CONTRACT"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) + SubPhase 3.3 (APIContractFlow33)
  Teaching: API gateway architecture designs fire ARCHITECTURE→3.5; OpenAPI contract definitions fire API_CONTRACT→3.3. Independent predicates, independent routing.

---

## B14-router-003-004 — ARCHITECTURE+AUTHORIZATION → 3.5, (AUTHZ sub-phase)

A: ["ARCHITECTURE", "AUTHORIZATION"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) + AUTHORIZATION sub-phase
  Teaching: RBAC architecture designs fire both ARCHITECTURE→3.5 (structural layer) and AUTHORIZATION (access control design). Independent.

---

## B14-router-003-005 — ARCHITECTURE: sub-phase disambiguation 3.5 vs 3.1

A: ["ARCHITECTURE"] — with infra vocabulary (infrastructure, topology, cloud, networking, container, cluster)
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) — not 3.1
  vs ["ARCHITECTURE"] — with app vocabulary (domain, service, module, pattern) → SubPhase 3.1
  Teaching: Infra vocabulary context → 3.5. Application/domain vocabulary context → 3.1. Same predicate, different sub-phase based on vocabulary co-occurrence.

