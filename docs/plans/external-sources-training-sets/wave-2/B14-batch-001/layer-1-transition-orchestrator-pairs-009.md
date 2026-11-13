# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B14-transition-009-001 — ARCHITECTURE+IMPLEMENT: "build" verb

A: ["ARCHITECTURE", "IMPLEMENT"] | "design the platform + build the Terraform modules"
B: TransitionType: feature_addition ("build" = IMPLEMENT trigger)
  Teaching: "build" fires IMPLEMENT → feature_addition. "design + build" = architectural design + code = feature.

---

## B14-transition-009-002 — ARCHITECTURE+DOCUMENT: "document" verb only → documentation

A: ["ARCHITECTURE", "DOCUMENT"] | "architect the topology → document in confluence"
B: TransitionType: documentation ("document" = DOCUMENT, no IMPLEMENT)
  Teaching: "document" fires DOCUMENT, not IMPLEMENT. Writing documentation ≠ implementing code.

---

## B14-transition-009-003 — ARCHITECTURE+IMPLEMENT+QA → feature_addition

A: ["ARCHITECTURE", "IMPLEMENT", "QA"]
B: TransitionType: feature_addition | Sequential: ARCHITECTURE → IMPLEMENT → QA gate

---

## B14-transition-009-004 — ARCHITECTURE with "create" → feature_addition

A: ["ARCHITECTURE", "IMPLEMENT"] | "create the infrastructure architecture and Terraform code"
B: TransitionType: feature_addition ("create" fires IMPLEMENT trigger)
  Teaching: "create" fires IMPLEMENT → feature_addition just like "implement", "build", "write".

---

## B14-transition-009-005 — ARCHITECTURE+SECURITY (implement-security context)

A: ["ARCHITECTURE", "SECURITY"] | "design zero-trust + enforce TLS on all east-west traffic"
B: SECURITY → 5.2 (implement-security sub-phase, because "enforce" verb)
  TransitionType: documentation (no IMPLEMENT predicate)
  Teaching: "enforce" fires SECURITY, routes to 5.2 (not 6.3). But no IMPLEMENT predicate means no feature_addition. TransitionType: documentation.

