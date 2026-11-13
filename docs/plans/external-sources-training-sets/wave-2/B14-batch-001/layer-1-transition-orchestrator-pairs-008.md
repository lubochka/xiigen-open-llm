# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B14-transition-008-001 — ARCHITECTURE: refactoring with documentation

A: ["ARCHITECTURE", "DOCUMENT"] | "refactor the architecture + document the new design"
B: TransitionType: refactoring (refactor verb overrides documentation)
  Sequential: refactor architecture → document updated ADRs
  Teaching: "refactor" overrides documentation TransitionType even when DOCUMENT is co-present.

---

## B14-transition-008-002 — ARCHITECTURE+IMPLEMENT: feature_addition despite documentation language

A: ["ARCHITECTURE", "IMPLEMENT"] | "design and build the caching layer"
B: TransitionType: feature_addition
  Teaching: "build" drives feature_addition. "design and build" = IMPLEMENT present → feature_addition.

---

## B14-transition-008-003 — ARCHITECTURE+TEST: infrastructure test first (TDD-infra pattern)

A: ["ARCHITECTURE", "TEST"] | "write connectivity tests, then architect the network to pass them"
B: TEST → ARCHITECTURE (inverted order — test-first)
  TransitionType: documentation
  Teaching: Ordering from request wording: "test first" puts TEST before ARCHITECTURE. TransitionType is still documentation (no IMPLEMENT).

---

## B14-transition-008-004 — ARCHITECTURE+IMPLEMENT+TEST+SECURITY: full hardening cycle

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY"]
B: ARCHITECTURE → SECURITY (audit existing) → IMPLEMENT (controls) → TEST (validate)
  TransitionType: feature_addition
  Teaching: Security hardening cycle: design the target architecture, audit current state, implement controls, validate coverage.

---

## B14-transition-008-005 — ARCHITECTURE+DOCUMENT+QA: architecture sign-off sequence

A: ["ARCHITECTURE", "DOCUMENT", "QA"]
B: ARCHITECTURE → DOCUMENT → QA gate
  TransitionType: documentation
  Teaching: Architecture review workflow: design → document → QA approves. QA always last.

