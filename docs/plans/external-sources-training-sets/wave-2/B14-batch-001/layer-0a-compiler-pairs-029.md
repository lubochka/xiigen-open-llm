# Layer 0-A — Compiler Pairs (029)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-029-001 — IstioMesh: ARCHITECTURE trigger

A: "Design the Istio service mesh topology with sidecar injection and traffic management policies."
  CompiledPredicateIds: ["ARCHITECTURE"]
  ARCHITECTURE: "design" + "service mesh" + "topology" + "traffic management" + "policies" (structural)
  Gap: "Istio" (tool name); "sidecar injection" (operational term); fired by architecture vocabulary

---

## B14-compiler-029-002 — IstioMesh: SECURITY trigger

A: "Enforce mTLS (mutual TLS) across all Istio namespaces and configure peer authentication."
  CompiledPredicateIds: ["SECURITY"]
  SECURITY: "mTLS" + "TLS" → SECURITY fires | "peer authentication" context
  Gap: "configure" (operational), "Istio" (tool name), "namespaces" (platform vocabulary)

---

## B14-compiler-029-003 — IstioMesh: IMPLEMENT trigger

A: "Implement the Istio VirtualService routing rules and write the DestinationRule configuration file."
  CompiledPredicateIds: ["IMPLEMENT"]
  IMPLEMENT: "implement" + "routing rules" (code artifact — VirtualService = IaC) | "write" + "configuration file"
  Gap: "Istio", "VirtualService", "DestinationRule" are tool/resource names; "implement" + artifact noun fires IMPLEMENT

---

## B14-compiler-029-004 — IstioMesh: ARCHITECTURE + SECURITY + IMPLEMENT

A: "Architect the service mesh topology, enforce zero-trust mTLS, and scaffold the VirtualService manifests."
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY", "IMPLEMENT"]
  ARCHITECTURE: "architect" + "topology" | SECURITY: "zero-trust" + "mTLS" | IMPLEMENT: "scaffold" + "manifests"
  TransitionType: feature_addition

---

## B14-compiler-029-005 — IstioMesh: gap vs ARCHITECTURE boundary

A: "Enable Istio sidecar injection in the production namespace."
  CompiledPredicateIds: []
  Gap: "enable" (operational), "sidecar injection" (operational action), "Istio" (tool), "namespace" (platform)
  Teaching: Enabling a feature is operational (gap). Designing the feature topology fires ARCHITECTURE.
