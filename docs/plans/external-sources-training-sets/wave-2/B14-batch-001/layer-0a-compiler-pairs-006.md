# Layer 0-A — Compiler Pairs (006)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-006-001 — ARCHITECTURE: secrets management layer

A: "Design the secrets management layer — architect the key vault integration, rotation policy, and access control boundaries."
  "design" → ARCHITECTURE | "secrets management layer" → ARCHITECTURE ("layer") + SECURITY ("secrets") | "architect" → ARCHITECTURE | "key vault integration" → ARCHITECTURE + SECURITY | "rotation policy" → SECURITY ("key rotation") | "access control boundaries" → ARCHITECTURE + AUTHORIZATION
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY", "AUTHORIZATION"]

---

## B14-compiler-006-002 — ARCHITECTURE: caching layer

A: "Architect the distributed caching layer — define Redis (Remote Dictionary Server) cluster topology and eviction strategy."
  "architect" → ARCHITECTURE | "distributed caching layer" → ARCHITECTURE ("layer") | "define" → ARCHITECTURE | "cluster topology" → ARCHITECTURE | "eviction strategy" → ARCHITECTURE ("strategy")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "Redis", "distributed caching" → platform/operational vocabulary, gap

---

## B14-compiler-006-003 — ARCHITECTURE: event-driven backbone

A: "Design the event-driven architecture backbone — define event sourcing patterns, consumer group topology, and schema registry structure."
  "design" → ARCHITECTURE | "event-driven architecture backbone" → ARCHITECTURE | "define" → ARCHITECTURE | "event sourcing patterns" → ARCHITECTURE ("patterns") | "consumer group topology" → ARCHITECTURE | "schema registry structure" → ARCHITECTURE ("structure")
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-006-004 — ARCHITECTURE+IMPLEMENT: VPN gateway

A: "Architect the VPN (Virtual Private Network) gateway layer and implement the firewall rules for inter-service communication."
  "architect" → ARCHITECTURE | "VPN gateway layer" → ARCHITECTURE ("layer") + SECURITY ("VPN") | "implement" → IMPLEMENT | "firewall rules" → SECURITY + IMPLEMENT
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]

---

## B14-compiler-006-005 — gap: "use microservices"

A: "We should use microservices for the new platform."
  "use" → gap | "microservices" → gap (architectural style name, not an architecture verb) | "new platform" → gap
  CompiledPredicateIds: [] → gap
  Teaching: Naming an architectural style ("microservices", "monolith", "serverless") is gap. ARCHITECTURE requires a design verb: design, architect, structure, blueprint.

