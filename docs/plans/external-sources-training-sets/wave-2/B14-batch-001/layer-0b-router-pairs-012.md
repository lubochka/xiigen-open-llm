# Layer 0-B — Router Pairs (012)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-012-001 — ARCHITECTURE+SECURITY+IMPLEMENT+TEST: full security hardening

A: ["ARCHITECTURE", "SECURITY", "IMPLEMENT", "TEST"]
B: → 3.5 + 6.3 + 4.1 + 5.1 | Four flows | TransitionType: feature_addition
  Teaching: Full security hardening cycle: design security architecture, audit existing exposure, implement controls, validate with tests. Four independent flows.

---

## B14-router-012-002 — ARCHITECTURE+DOCUMENT: ADR writing

A: ["ARCHITECTURE", "DOCUMENT"] | context: writing architecture decision records
B: → 3.5 + 6.2 | TransitionType: documentation
  Teaching: ADR creation is a documentation activity. ARCHITECTURE fires on "architecture decision" vocabulary; DOCUMENT fires on "write", "document", "record".

---

## B14-router-012-003 — ARCHITECTURE+QA+IMPLEMENT+DOCUMENT: release gate

A: ["ARCHITECTURE", "QA", "IMPLEMENT", "DOCUMENT"]
B: → 3.5 + 7.3 + 4.1 + 6.2 | TransitionType: feature_addition
  Teaching: Platform release workflow: design (ARCHITECTURE), implement (IMPLEMENT), document (DOCUMENT), sign off (QA). Four flows.

---

## B14-router-012-004 — ARCHITECTURE: "design" alone without architecture framing → gap

A: "Design the user interface for the dashboard."
  "design" → ARCHITECTURE? → No: "user interface/dashboard" vocabulary → UI predicate, not ARCHITECTURE
  CompiledPredicateIds: ["UI"]
  Teaching: "design" fires ARCHITECTURE only with architecture vocabulary co-occurrence (infrastructure, topology, architecture, blueprint, component, layer, boundary). "design" + UI vocabulary → UI predicate.

---

## B14-router-012-005 — ARCHITECTURE: TransitionType summary

A: ARCHITECTURE-only requests
B: "design/architect" verb → TransitionType: documentation
  "refactor the architecture" → TransitionType: refactoring
  "implement + design" → TransitionType: feature_addition
  Teaching: TransitionType for ARCHITECTURE depends on verbs: design/diagram alone → documentation; refactor → refactoring; implement present → feature_addition.

