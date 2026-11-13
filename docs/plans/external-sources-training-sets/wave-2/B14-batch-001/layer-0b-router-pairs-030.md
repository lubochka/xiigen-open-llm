# Layer 0-B — Router Pairs (030)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-030-001 — PrometheusStack routing: ARCHITECTURE → 3.5

ARCHITECTURE → 3.5 for Prometheus observability topology:
  "Prometheus monitoring stack topology" → 3.5 (observability is listed infra vocabulary for 3.5)
  "federation" alone = gap; "observability infrastructure layer" → 3.5
  Never 3.1: observability stack is always infra vocabulary

---

## B14-router-030-002 — PrometheusStack routing: IMPLEMENT → 4.1

IMPLEMENT → 4.1 for Prometheus configuration artifacts:
  "scrape configuration file" → 4.1 | "alerting rule files" → 4.1 | "operator manifests" → 4.1
  All Prometheus configuration code artifacts route to 4.1 (BackendServiceFlow41)
  "write" + Prometheus config artifact → 4.1

---

## B14-router-030-003 — PrometheusStack routing: documentation only

ARCHITECTURE + DOCUMENT, no IMPLEMENT → documentation TransitionType:
  "document the scraping strategy and diagram the topology" → TransitionType: documentation
  Both ARCHITECTURE (3.5) and DOCUMENT (6.2) fire as concurrent flows
  Sequential: 3.5 → 6.2 | TransitionType: documentation (no IMPLEMENT present)

---

## B14-router-030-004 — PrometheusStack routing: zero predicates (gap)

"Start the Prometheus server and check the targets" → knowledge_edge_synthetic:
  No routing decisions emitted
  CapabilityGapWarnings: gap-devops-cloud-001 for "Prometheus server", "start", "check targets"
  GapHandler: receives capability gap signal; emits gap warning in response
