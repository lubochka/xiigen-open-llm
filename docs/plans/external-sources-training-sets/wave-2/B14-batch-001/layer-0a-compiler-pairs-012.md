# Layer 0-A — Compiler Pairs (012)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-012-001 — ARCHITECTURE: service registry

A: "Design the service registry and discovery architecture — define the health-check topology and DNS-based routing layer."
  "design" → ARCHITECTURE | "service registry and discovery architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "health-check topology" → ARCHITECTURE | "DNS-based routing layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-012-002 — ARCHITECTURE+IMPLEMENT+TEST: network policy

A: "Architect the Kubernetes network policy layer, implement the NetworkPolicy manifests, and validate connectivity isolation between namespaces."
  "architect" → ARCHITECTURE | "network policy layer" → ARCHITECTURE ("layer") | "implement" → IMPLEMENT | "NetworkPolicy manifests" → IMPLEMENT | "validate" → TEST | "connectivity isolation" → ARCHITECTURE + SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY"]

---

## B14-compiler-012-003 — ARCHITECTURE: backup and restore

A: "Design the backup and restore architecture — define the backup frequency, retention policy, cross-region replication topology, and restoration boundary."
  "design" → ARCHITECTURE | "backup and restore architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "retention policy" → ARCHITECTURE ("policy") | "cross-region replication topology" → ARCHITECTURE | "restoration boundary" → ARCHITECTURE ("boundary")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-012-004 — ARCHITECTURE+DOCUMENT+QA: final design review

A: "Finalize the infrastructure architecture blueprint, document all design decisions, and obtain QA sign-off before production rollout."
  "finalize" → ARCHITECTURE (in context of completing an architectural design) | "infrastructure architecture blueprint" → ARCHITECTURE | "document all design decisions" → DOCUMENT | "QA sign-off" → QA
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "QA"]

---

## B14-compiler-012-005 — gap: "reboot the services"

A: "Reboot all the microservices after the configuration change."
  "reboot" → gap (operational action) | "microservices" → gap (platform vocabulary) | "configuration change" → gap
  CompiledPredicateIds: [] → gap
  Teaching: Operational commands ("reboot", "restart", "deploy", "rollback") are gap. They do not fire ARCHITECTURE, IMPLEMENT, or any predicate without explicit design/test/implement framing.

