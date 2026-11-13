# Layer 0-C — Knowledge Edge Pairs (030)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-030-001 — PrometheusStack: "start" and "check" are always gap

Gap: "start the Prometheus server", "check the scrape targets", "view the metrics"
No predicate fires from operational monitoring server actions
Teaching: Running and observing a running server is operational. Designing its topology (ARCHITECTURE) or writing its config (IMPLEMENT) fire predicates.

---

## B14-edge-030-002 — PrometheusStack: "federation" alone is gap

Gap: "enable Prometheus federation" (operational + tool name)
ARCHITECTURE fires: "design the Prometheus federation topology" (design verb + topology noun)
Teaching: Technology terms like "federation" alone are gap. They must be paired with design/architect/structure verbs to fire ARCHITECTURE.

---

## B14-edge-030-003 — PrometheusStack: documentation TransitionType boundary

ARCHITECTURE + DOCUMENT with no IMPLEMENT → documentation:
  "document the scraping strategy" (DOCUMENT) + "diagram the metrics topology" (ARCHITECTURE) = documentation
  Only IMPLEMENT can convert documentation to feature_addition; ARCHITECTURE alone does not convert
Teaching: ARCHITECTURE alone with design verb → feature_addition; ARCHITECTURE + DOCUMENT with no IMPLEMENT → documentation; ARCHITECTURE + DOCUMENT + IMPLEMENT → feature_addition.

---

## B14-edge-030-004 — PrometheusStack: scrape configuration is a code artifact

IMPLEMENT fires: "implement the Prometheus scrape configuration" — "scrape configuration" = configuration file (code artifact)
IMPLEMENT fires: "write the alerting rule file" — "alerting rule file" = code artifact
Gap: "update the scrape interval" (operational parameter update — not creating an artifact)
Teaching: Creating a configuration file fires IMPLEMENT. Updating a runtime parameter is operational gap.
