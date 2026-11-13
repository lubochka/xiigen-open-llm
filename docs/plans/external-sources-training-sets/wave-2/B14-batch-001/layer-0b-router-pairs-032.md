# Layer 0-B — Router Pairs (032)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-032-001 — AlertManager routing: ARCHITECTURE → 3.5

ARCHITECTURE → 3.5 for AlertManager routing topology design:
  "AlertManager routing topology with receiver hierarchy" → 3.5 ("topology", "hierarchy", "routing" = infra vocabulary)
  "inhibition rules" as structural design vocabulary → 3.5 context
  Never 3.1: alerting infrastructure is always infra vocabulary

---

## B14-router-032-002 — AlertManager routing: IMPLEMENT → 4.1

IMPLEMENT → 4.1 for AlertManager configuration creation:
  "AlertManager configuration file" → 4.1 | "routing tree YAML" → 4.1
  YAML configuration files = code artifacts → BackendServiceFlow41
  "implement" + configuration file always routes to 4.1

---

## B14-router-032-003 — AlertManager routing: SECURITY → 6.3

SECURITY → 6.3 for TLS and authentication:
  "TLS for webhook endpoints" → 6.3 | "API key authentication" → 6.3
  Concurrent with ARCHITECTURE (3.5) and IMPLEMENT (4.1) when all three fire
  Sequential: 3.5 → 6.3 → 4.1

---

## B14-router-032-004 — AlertManager routing: zero predicates

"Silence the firing alerts and delete the suppression rules" → knowledge_edge_synthetic:
  "silence" = operational | "delete" = operational | "firing alerts" = runtime state
  No predicates fire from purely operational alert management vocabulary
  GapHandler: receives signal; CapabilityGapSignalForwarded: true
