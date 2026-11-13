# Layer 2 — Nine-Node Flow Pairs (021)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-021 — GitOpsTopology — ARCHITECTURE+IMPLEMENT

Request: "Design the GitOps deployment topology and implement the ArgoCD (continuous delivery tool) application set configuration for the multi-environment pipeline."

### Flow: InfrastructureArchitectureFlow35 + BackendServiceFlow41

**Node 1 — DomainSplitter**
  SelectedLogicalTransition: ARCHITECTURE → 3.5 + IMPLEMENT → 4.1
  Domains: git_repository_topology, reconciliation_topology, environment_promotion, rollback_strategy

**Node 2 — DomainTranslator**
  git_repository_topology: Monorepo; app-of-apps pattern; environment overlays via Kustomize; branch per environment
  reconciliation_topology: ArgoCD syncs every 3 minutes; auto-sync for dev; manual sync gate for production
  environment_promotion: dev → staging → pre-prod → production; each promotion requires passing CI (Continuous Integration) + test gates
  rollback_strategy: ArgoCD history retention 10 syncs; one-click rollback to previous revision; automated rollback on health degradation

**Node 3 — SchemePlanner**
  Pattern: App-of-apps + environment overlays; declarative Git as source of truth; production manual sync gate
  Decision: Auto-sync for dev/staging; manual approval gate for production — prevents accidental production changes
  Decision: Kustomize overlays per environment — single base config, per-environment patch

**Node 4 — DomainProcessor**
  Decision: App-of-apps pattern in ArgoCD
  Rationale: Single root application manages all child applications; enables environment-level rollout control
  Trade-off: App-of-apps adds ArgoCD complexity; parent app health depends on all children
  Decision: Manual sync gate for production
  Rationale: Prevents unintended changes reaching production; requires explicit operator approval
  Trade-off: Slower deployment velocity; accepted for production stability

**Node 5 — Applier**
  Constraints: All manifests must be in Git before deployment; no kubectl apply in production; ArgoCD self-managed (app manages itself)

**Node 6 — ResultTranslator**
  Output: gitops-topology-adr.md + ArgoCD Application manifests + ApplicationSet configuration

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Config: required: GitOps pattern decision + trade-off; promotion gate strategy; rollback mechanism

**Node 8 — Arbiter**
  git_repository_topology: App-of-apps + Kustomize overlays ✓ | PASS
  reconciliation_topology: Auto-sync dev + manual gate production ✓ | trade_off: deployment velocity ✓ | PASS
  environment_promotion: 4-environment promotion gates ✓ | PASS
  rollback_strategy: 10-revision history + health-based rollback ✓ | PASS
  OverallVerdict: PASS

**Node 9 — Formatter**
  Delivery: gitops-topology-adr.md + argocd-appset.yaml | Status: ready | Arbiter: PASS
