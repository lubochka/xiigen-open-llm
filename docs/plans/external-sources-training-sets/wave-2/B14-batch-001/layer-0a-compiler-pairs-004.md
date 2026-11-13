# Layer 0-A — Compiler Pairs (004)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-004-001 — ARCHITECTURE+API_CONTRACT: API gateway

A: "Design the API gateway architecture — define the routing rules, rate limits, and OpenAPI contract for external consumers."
  "design" → ARCHITECTURE | "API gateway architecture" → ARCHITECTURE | "define" → ARCHITECTURE (design decision) | "OpenAPI contract" → API_CONTRACT | "routing rules" → ARCHITECTURE ("rules" as structural patterns)
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]

---

## B14-compiler-004-002 — ARCHITECTURE: observability

A: "Architect the observability infrastructure — define the tracing, metrics, and log aggregation layers."
  "architect" → ARCHITECTURE | "observability infrastructure" → ARCHITECTURE ("infrastructure") | "define" → ARCHITECTURE | "tracing, metrics, log aggregation layers" → ARCHITECTURE ("layers")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "tracing", "metrics", "log aggregation" → observability tooling vocabulary, gap

---

## B14-compiler-004-003 — ARCHITECTURE+TEST: infra validation

A: "Design the infrastructure topology and write integration tests to validate the service-to-service connectivity."
  "design" → ARCHITECTURE | "infrastructure topology" → ARCHITECTURE | "write integration tests" → TEST | "validate" → TEST
  CompiledPredicateIds: ["ARCHITECTURE", "TEST"]

---

## B14-compiler-004-004 — ARCHITECTURE+IMPLEMENT+DOCUMENT: complete infra design

A: "Design the multi-region infrastructure blueprint, implement the Terraform modules, and document the deployment runbook."
  "design" → ARCHITECTURE | "infrastructure blueprint" → ARCHITECTURE | "implement" → IMPLEMENT | "Terraform modules" → IMPLEMENT (concrete implementation) | "document" → DOCUMENT | "deployment runbook" → DOCUMENT
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  Gap: "Terraform", "multi-region" → platform vocabulary, gap

---

## B14-compiler-004-005 — gap: "check if the infrastructure is ready"

A: "Check if the infrastructure is ready for the production deployment."
  "check" → gap | "infrastructure" without architecture framing → gap | "production deployment" → gap
  CompiledPredicateIds: [] → gap
  Teaching: "check" is gap for ARCHITECTURE. Requires design/architect/blueprint/diagram/structure.

