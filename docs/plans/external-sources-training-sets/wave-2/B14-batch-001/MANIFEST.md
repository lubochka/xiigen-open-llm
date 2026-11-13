# MANIFEST — B14-batch-001
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 (extended from 2026-05-05)
# Total files: 201

---

## File Count by Layer

| Layer | Files |
|-------|-------|
| Layer 0-A (compiler pairs) | 24 (001–024) |
| Layer 0-B (router pairs) | 24 (001–024) |
| Layer 0-C (knowledge edge pairs) | 24 (001–024) |
| Layer 0-D (flow factory pairs) | 24 (001–024) |
| Layer 1 (transition orchestrator pairs) | 24 (001–024) |
| Layer 2 (nine-node 3.5 pairs) | 24 (001–024) |
| Layer 3 (teaching unit pairs) | 24 (001–024) |
| Held-out and corruption pairs | 24 (001–024) |
| Cross-layer synthesis | 8 (001–008) |
| MANIFEST | 1 |
| **Total** | **201** |

---

## InfrastructureArchitectureFlow35 Nine-Node Flows (Layer 2)

| File | Architecture Target | Trade-off | Verdict |
|------|---------------------|-----------|---------|
| 001 | Platform (API gateway + service mesh + compute + data) | 5 ADRs with explicit trade-offs | PASS |
| 002 | OrderService DB replication topology | Asynchronous replication, RPO < 60s | PASS |
| 003 | ProductCatalog CDN edge caching | Hybrid TTL + event-driven invalidation | PASS |
| 004 | EventBus message broker topology | Per-event-type Kafka topics, DLQ policy | PASS |
| 005 | Container orchestration scheduling | Domain namespaces, soft anti-affinity | PASS |
| 006 | Service mesh sidecar (mTLS + circuit breaker) | STRICT mTLS, 5 consecutive 5xx ejection | PASS |
| 007 | API gateway routing + rate limiting | Path-based routing, Redis-backed rate limits | PASS |
| 008 | Observability infrastructure (OTel + Jaeger + Prometheus + Loki) | OTel vendor-neutral, Loki label-only | PASS |
| 009 | OrderService auto-scaling (HPA) | CPU-based HPA, 5-min scale-in cooldown | PASS |
| 010 | Zero-trust network (Istio STRICT mTLS) | STRICT mTLS with cert-manager, NetworkPolicy deny-all | PASS |
| 011 | Payments platform DR topology (active-active) | Synchronous replication, RTO < 5 min, RPO = 0 | PASS |
| 012 | Platform backup and restore | EBS snapshots + PITR + S3 CRR | PASS |
| 013 | EKS cluster topology — namespace isolation | Multi-AZ node groups, ResourceQuota per namespace | PASS |
| 014 | Service mesh (Istio) — ARCHITECTURE+IMPLEMENT | STRICT mTLS + deny-all AuthorizationPolicy | PASS |
| 015 | Secrets management topology — ARCHITECTURE+SECURITY | Vault 3-node Raft, dynamic creds, 1h TTL | PASS |
| 016 | CDN and edge topology — ARCHITECTURE alone | Anycast 8 PoPs, L1→L2→Shield cache hierarchy | PASS |
| 017 | Auto-scaling infrastructure — ARCHITECTURE+IMPLEMENT | HPA 70% CPU, cluster autoscaler 10-min scale-down | PASS |
| 018 | Disaster recovery topology — ARCHITECTURE+DOCUMENT | Async replication, Route53 30s TTL failover | PASS |
| 019 | Multi-region deployment — ARCHITECTURE+IMPLEMENT | Active-active 3 regions, CockroachDB consensus | PASS |
| 020 | Observability stack — ARCHITECTURE+IMPLEMENT | OTel collector, 5% trace sampling, Loki label-only | PASS |
| 021 | GitOps topology — ARCHITECTURE+IMPLEMENT | App-of-apps, manual production sync gate | PASS |
| 022 | Network security topology — ARCHITECTURE+SECURITY | Deny-all NetworkPolicy, STRICT mTLS, PrivateLink | PASS |
| 023 | Probationary rejected — ARCHITECTURE+TEST | TEST routing corrected to IMPLEMENT for infra validation | PASS with correction |
| 024 | Complete nine-node rule summary for sub-phase 3.5 | Node roles, arbiter rules, formatter rules | REFERENCE |

---

## Key Teaching Points

1. **"design" fires ARCHITECTURE universally** — canonical trigger verb
2. **"architect" (verb) fires ARCHITECTURE** — equivalent to "design"
3. **Sub-phase disambiguation**: infra vocabulary (infrastructure, topology, cluster, service mesh) → 3.5; app/domain vocabulary → 3.1
4. **Cloud platform names are gap**: AWS, Azure, GCP, Kubernetes, EKS, S3 do not fire ARCHITECTURE
5. **"scale" as operational verb is gap**: "design the scaling architecture" → ARCHITECTURE; "scale up" → gap
6. **Operational verbs are gap**: configure (UI/ops), deploy (standalone), install, set up, restart, rollback, enable, disable, purge, run → gap
7. **"plan" is gap**: ARCHITECTURE requires design/architect/structure/blueprint/diagram
8. **ARCHITECTURE+IMPLEMENT → feature_addition**: IMPLEMENT drives TransitionType
9. **ARCHITECTURE alone → feature_addition** (design verb = creating new artifact); ARCHITECTURE+DOCUMENT → documentation
10. **"refactor" → refactoring TransitionType**: overrides documentation for ARCHITECTURE flows
11. **Architectural patterns fire ARCHITECTURE**: circuit breaker, sidecar, bulkhead, canary, active-active, GitOps, pub/sub, event sourcing
12. **mTLS, TLS, zero-trust fire SECURITY independently** in any context
13. **"vault" (tool name) fires SECURITY** directly in SECURITY predicate lexicon
14. **"credential", "certificate", "secret", "key rotation" fire SECURITY** independently
15. **IMPLEMENT in IaC context**: scaffold, write (IaC), build (container), configure (code artifact) → IMPLEMENT
16. **No predicate suppression**: gap vocabulary never suppresses fired predicates
17. **No predicate count cap**: all valid vocabulary fires all applicable predicates
18. **TEST at 5.1 requires test code artifacts**: infrastructure operational validation → IMPLEMENT (script), not TEST
19. **"validate" fires TEST**: validate is a TEST action verb for connectivity/correctness checking
20. **"API contracts" fires API_CONTRACT**: combined with "define" fires both ARCHITECTURE and API_CONTRACT

---

## Cross-Layer Synthesis Traces

| File | Scenario | Predicate Set |
|------|----------|---------------|
| 001 | Architect platform + document ADRs | ARCHITECTURE+DOCUMENT |
| 002 | Architect IaC pipeline + implement Terraform | ARCHITECTURE+IMPLEMENT |
| 003 | Gap: "Set up Kubernetes for production" | [] → GapHandler |
| 004 | Six-predicate: design + zero-trust + implement + validate + document + QA | ARCHITECTURE+SECURITY+IMPLEMENT+TEST+DOCUMENT+QA |
| 005 | EKS deployment IaC — design topology + implement Terraform | ARCHITECTURE+IMPLEMENT |
| 006 | Secrets management topology — design + enforce TLS/secrets | ARCHITECTURE+SECURITY |
| 007 | Four-predicate infra trace — design + implement + security + document | ARCHITECTURE+IMPLEMENT+SECURITY+DOCUMENT |
| 008 | Six-predicate payment platform — full stack trace | ARCHITECTURE+API_CONTRACT+IMPLEMENT+SECURITY+TEST+DOCUMENT |

---

## Extension Summary (013–024 per layer, synthesis 005–008)

Files 013–201 added on 2026-05-06. Coverage topics:
- **013**: EKS/Kubernetes cluster topology
- **014**: Service mesh (Istio/Linkerd) architecture
- **015**: Secrets management (Vault/AWS Secrets Manager)
- **016**: CDN and edge architecture
- **017**: Auto-scaling and capacity planning
- **018**: Disaster recovery topology
- **019**: Multi-region deployment
- **020**: Observability stack (metrics/logs/traces)
- **021**: GitOps and deployment pipeline topology
- **022**: Network security topology (VPC, PrivateLink, zero-trust)
- **023**: IMPLEMENT+ARCHITECTURE IaC cases (Terraform, Helm, Ansible, Dockerfile)
- **024**: Complete predicate rule reference per layer
