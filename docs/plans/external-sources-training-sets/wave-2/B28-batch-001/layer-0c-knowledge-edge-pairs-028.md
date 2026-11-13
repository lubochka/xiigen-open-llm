# Layer 0-C — Knowledge Edge Pairs (028) — PerformanceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E028-001 — PerformanceGate trigger edge

A: "pass PerformanceGate" → TEST | "run performance tests" → TEST | "validate PerformanceGate thresholds" → TEST
  Teaching: PerformanceGate and performance test nouns fire TEST. "validate" fires TEST always.

---

## E028-002 — "monitor performance" is gap

A: "monitor performance metrics after release" → [] → knowledge_edge
  Teaching: "monitor" is operational vocabulary; gap. Does not fire TEST even with performance noun.

---

## E028-003 — "design gate architecture" fires ARCHITECTURE

A: "design the performance gate architecture" → ARCHITECTURE (design verb + architecture noun — deduplicated)
  Teaching: "design" fires ARCHITECTURE; "architecture" fires ARCHITECTURE. Deduplicated to one.

---

## E028-004 — "going live" is gap

A: "the release going live after PerformanceGate passes" → [] → knowledge_edge
  Teaching: "going live" is release execution vocabulary; gap regardless of context.

---

## E028-005 — "implement threshold service" fires IMPLEMENT (twice, deduplicated)

A: "implement the performance threshold service" → IMPLEMENT (implement verb + service noun — deduplicated)
  Teaching: "implement" fires IMPLEMENT; "service" fires IMPLEMENT. Set deduplication → one IMPLEMENT.
