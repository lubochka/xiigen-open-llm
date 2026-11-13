# Layer 0-A — Compiler Pairs (032)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-032-001 — AlertManager: ARCHITECTURE trigger

A: "Design the AlertManager routing topology with receiver hierarchy and inhibition rules."
  CompiledPredicateIds: ["ARCHITECTURE"]
  ARCHITECTURE: "design" + "topology" + "hierarchy" + "routing" (infra context) + "rules" (structural)
  Gap: "AlertManager" (tool name); routing topology and hierarchy fire ARCHITECTURE

---

## B14-compiler-032-002 — AlertManager: IMPLEMENT trigger

A: "Implement the AlertManager configuration file and write the routing tree YAML (YAML Ain't Markup Language)."
  CompiledPredicateIds: ["IMPLEMENT"]
  IMPLEMENT: "implement" + "configuration file" (code artifact) | "write" + "routing tree YAML" (code artifact)
  Gap: "AlertManager" (tool name); code artifact nouns fire IMPLEMENT

---

## B14-compiler-032-003 — AlertManager: SECURITY trigger

A: "Configure TLS for AlertManager webhook endpoints and enforce API key authentication."
  CompiledPredicateIds: ["SECURITY"]
  SECURITY: "TLS" + "API key" (authentication context)
  Gap: "configure" (operational), "AlertManager" (tool), "webhook endpoints" (runtime term)

---

## B14-compiler-032-004 — AlertManager: ARCHITECTURE + IMPLEMENT + DOCUMENT

A: "Architect the alerting topology, scaffold the receiver configuration, and document the escalation policy."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  ARCHITECTURE: "architect" + "topology" | IMPLEMENT: "scaffold" + "receiver configuration" (code artifact) | DOCUMENT: "document" + "policy"
  TransitionType: feature_addition (IMPLEMENT overrides DOCUMENT)

---

## B14-compiler-032-005 — AlertManager: gap vocabulary

A: "Silence the firing alerts and delete the suppression rules."
  CompiledPredicateIds: []
  Gap: "silence" (operational), "delete" (operational), "firing" (runtime state), "suppression rules" (operational config)
  TrustState: knowledge_edge_synthetic
