# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B14-transition-007-001 — ARCHITECTURE+DOCUMENT: ADR sequential

A: ["ARCHITECTURE", "DOCUMENT"] | "design the topology → document ADRs"
B: ARCHITECTURE → DOCUMENT (sequential)
  TransitionType: documentation
  Teaching: ADR workflow: architectural decisions made first, documented second. Always sequential.

---

## B14-transition-007-002 — ARCHITECTURE+TEST+DOCUMENT: validate then document

A: ["ARCHITECTURE", "TEST", "DOCUMENT"] | "architect, validate connectivity, document findings"
B: ARCHITECTURE → TEST → DOCUMENT
  TransitionType: documentation
  Teaching: TEST can precede DOCUMENT in the sequence. Validation findings become documentation content.

---

## B14-transition-007-003 — ARCHITECTURE alone: single flow, no sequential

A: ["ARCHITECTURE"]
B: InfrastructureArchitectureFlow35 only | No sequential orchestration needed
  TransitionType: documentation

---

## B14-transition-007-004 — ARCHITECTURE+SECURITY+IMPLEMENT: audit then implement

A: ["ARCHITECTURE", "SECURITY", "IMPLEMENT"] | "audit existing + design zero-trust + implement controls"
B: SECURITY (audit) → ARCHITECTURE (design) → IMPLEMENT (controls)
  TransitionType: feature_addition
  Teaching: Security audits can precede both architecture redesign and implementation.

---

## B14-transition-007-005 — ARCHITECTURE+API_CONTRACT: design then contract

A: ["ARCHITECTURE", "API_CONTRACT"] | "design the gateway architecture → define the OpenAPI contracts"
B: ARCHITECTURE → API_CONTRACT (sequential)
  TransitionType: documentation
  Teaching: Architecture design logically precedes API contract definition. Sequential ordering follows domain logic.

