# Layer 0-C — Knowledge Edge Pairs (033)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-033-001 — LogAggregation: "encryption in transit" fires SECURITY

SECURITY fires: "encrypt log streams in transit" — "encrypt" + "in transit" = listed compound phrase for SECURITY
SECURITY fires: "TLS on log ingestion endpoint" — "TLS" fires SECURITY directly
Gap: "log streams" alone (operational term), "ingestion endpoint" alone (runtime term)
Teaching: "encrypt" or "TLS" in any infra sentence fires SECURITY regardless of surrounding operational vocabulary.

---

## B14-edge-033-002 — LogAggregation: parser configuration is a code artifact

IMPLEMENT fires: "implement the Fluent Bit parser configuration" — parser configuration = code artifact (configuration file)
IMPLEMENT fires: "write the log routing pipeline manifest" — manifest = code artifact
Gap: "view the parsed logs" (operational), "check the log pipeline" (operational health check)
Teaching: Writing/implementing parser configuration files fires IMPLEMENT. Viewing/checking logs is operational gap.

---

## B14-edge-033-003 — LogAggregation: "centralized" fires ARCHITECTURE context

ARCHITECTURE fires: "design the centralized logging infrastructure layer" — "centralized" + "infrastructure layer" = structural infra vocabulary
"centralized" alone does not fire ARCHITECTURE; requires design verb + structural noun
Gap: "centralize the logs" (operational migration action without structural design verb + noun)
Teaching: Vocabulary density matters — design verb + structural noun combination fires ARCHITECTURE.

---

## B14-edge-033-004 — LogAggregation: Loki is a tool name (gap) but scaffold fires IMPLEMENT

Gap contribution: "Loki" alone = tool name → gap
IMPLEMENT fires: "scaffold the Loki stack configuration" — "scaffold" + "configuration" (code artifact) → IMPLEMENT fires
Teaching: The tool name "Loki" is gap but does not suppress IMPLEMENT firing from "scaffold" + code artifact noun. Gap vocabulary never suppresses fired predicates.
