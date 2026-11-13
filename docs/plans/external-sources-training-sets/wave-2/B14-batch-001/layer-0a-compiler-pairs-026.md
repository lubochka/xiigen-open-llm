# Layer 0-A — Compiler Pairs (026)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-026-001 — HelmChart: IMPLEMENT trigger

A: "Implement the Helm chart for the payment service with configurable replicas and resource limits."
  CompiledPredicateIds: ["IMPLEMENT"]
  IMPLEMENT: "implement" + "Helm chart" (code artifact: chart = IaC artifact)
  Gap: "Helm" alone is gap; "Helm chart" as code artifact noun fires IMPLEMENT

---

## B14-compiler-026-002 — HelmChart: ARCHITECTURE trigger

A: "Design the Helm chart architecture for multi-environment deployment topology."
  CompiledPredicateIds: ["ARCHITECTURE"]
  ARCHITECTURE: "design" + "architecture" + "topology" + "deployment" (design context)
  Gap: "Helm" (tool name); "multi-environment" (operational term alone)

---

## B14-compiler-026-003 — HelmChart: IMPLEMENT + DOCUMENT

A: "Write the Helm chart values file and document the configuration parameters."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  IMPLEMENT: "write" + "Helm chart values file" (code artifact) | DOCUMENT: "document" + "configuration parameters"
  TransitionType: feature_addition (IMPLEMENT present overrides DOCUMENT)

---

## B14-compiler-026-004 — HelmChart: gap boundary

A: "Install the nginx Helm chart and upgrade the ingress controller."
  CompiledPredicateIds: []
  Gap: "install" (operational), "upgrade" (operational), "nginx" (platform name), "Helm chart" (tool artifact name alone without implement/write verb)
  TrustState: knowledge_edge_synthetic

---

## B14-compiler-026-005 — HelmChart: ARCHITECTURE + IMPLEMENT

A: "Architect the Helm chart dependency topology and scaffold the chart templates."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  ARCHITECTURE: "architect" + "topology" | IMPLEMENT: "scaffold" + "chart templates" (code artifact)
  TransitionType: feature_addition
