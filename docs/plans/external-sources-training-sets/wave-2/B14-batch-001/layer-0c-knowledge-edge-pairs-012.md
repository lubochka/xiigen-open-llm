# Layer 0-C — Knowledge Edge Pairs (012)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E012-001 — "throttling" fires ARCHITECTURE in design context

A: "Design the throttling architecture for the external API consumers — define exponential backoff rules and queue depth thresholds."
  "design" → ARCHITECTURE | "throttling architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "exponential backoff rules" → ARCHITECTURE ("rules") | "queue depth thresholds" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "throttling" in architecture design context fires ARCHITECTURE. Throttling policies are structural rate-control constraints.

---

## E012-002 — "proxy" fires ARCHITECTURE in structural role

A: "Define the reverse proxy layer for the ProductCatalog API — define TLS termination, header rewriting, and upstream routing rules."
  "define" → ARCHITECTURE | "reverse proxy layer" → ARCHITECTURE ("proxy", "layer") | "upstream routing rules" → ARCHITECTURE ("rules")
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"] — TLS termination fires SECURITY
  Teaching: "proxy" in structural design context fires ARCHITECTURE. "reverse proxy" is a recognized architectural component.

---

## E012-003 — "EDA" (Event-Driven Architecture) fires ARCHITECTURE

A: "Design the EDA (Event-Driven Architecture) backbone for the platform."
  "design" → ARCHITECTURE | "EDA backbone" → ARCHITECTURE (Event-Driven Architecture abbreviation)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "EDA" fires ARCHITECTURE via abbreviation expansion.

---

## E012-004 — "serverless" fires ARCHITECTURE in design context

A: "Design the serverless architecture for the image processing pipeline — define function boundaries, trigger topology, and cold-start mitigation strategy."
  "design" → ARCHITECTURE | "serverless architecture" → ARCHITECTURE | "function boundaries" → ARCHITECTURE ("boundaries") | "trigger topology" → ARCHITECTURE | "cold-start mitigation strategy" → ARCHITECTURE ("strategy")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "serverless" in architectural design context fires ARCHITECTURE.

---

## E012-005 — "plan" without architecture vocabulary is gap (final summary)

A: "Plan the infrastructure migration."
  "plan" → gap | "infrastructure migration" → gap (without design verb framing)
  CompiledPredicateIds: [] → gap
  vs "Architect the infrastructure migration topology."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "plan" is the canonical gap verb for ARCHITECTURE. Required verbs: design, architect, structure, blueprint, diagram, define (in structural context).

