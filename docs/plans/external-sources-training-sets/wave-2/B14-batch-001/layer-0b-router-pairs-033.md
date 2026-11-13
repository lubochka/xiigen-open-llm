# Layer 0-B — Router Pairs (033)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-033-001 — LogAggregation routing: ARCHITECTURE → 3.5

ARCHITECTURE → 3.5 for log aggregation topology:
  "centralized logging pipeline topology" → 3.5 (pipeline + topology + centralized = infra vocabulary)
  "retention layers" = structural infra vocabulary → 3.5
  Log aggregation is always infra-domain → never routes to 3.1

---

## B14-router-033-002 — LogAggregation routing: IMPLEMENT → 4.1

IMPLEMENT → 4.1 for log pipeline configuration artifacts:
  "Fluent Bit parser configuration" → 4.1 | "log routing pipeline manifest" → 4.1 | "Loki stack configuration" → 4.1
  All log pipeline code artifacts (parsers, manifests, configs) route to 4.1

---

## B14-router-033-003 — LogAggregation routing: SECURITY → 6.3

SECURITY → 6.3 for log stream encryption:
  "encrypt log streams in transit" → 6.3 ("encrypt" + "in transit" = SECURITY vocabulary)
  "TLS on log ingestion endpoint" → 6.3
  Concurrent with ARCHITECTURE (3.5) and IMPLEMENT (4.1) when all fire

---

## B14-router-033-004 — LogAggregation routing: documentation TransitionType

ARCHITECTURE (3.5) + DOCUMENT (6.2), no IMPLEMENT → documentation:
  "document the log aggregation architecture and diagram the ingestion flow" → documentation
  Both flows active (3.5 + 6.2); recording verbs dominate; TransitionType = documentation
  Feature_addition requires IMPLEMENT or ARCHITECTURE alone with design verb
