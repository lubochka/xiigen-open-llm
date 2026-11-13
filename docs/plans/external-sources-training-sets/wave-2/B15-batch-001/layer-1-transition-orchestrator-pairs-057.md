# Layer 1 — Transition Orchestrator Pairs (057)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-057-001 — Edge case: "write" fires IMPLEMENT (not DOCUMENT) for code artifacts

A: "Write the CacheService eviction policy handler."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true
"write" + code artifact → IMPLEMENT (not DOCUMENT).
DOCUMENT fires from: "write documentation", "write docs", "write runbook", "write spec" — not bare "write code".

---

## B15-t1-057-002 — Edge case: "write tests" fires TEST (not IMPLEMENT)

A: "Write integration tests for the DeadLetterService message replay."
B: TransitionType: feature_addition | SubPhaseIds: ["5.1"] | ContextCarryForward: true
Wait — "write tests" alone → TEST only. But in B15, a request always implies the service exists (IMPLEMENT is assumed in context).
Standalone "write tests" request with no build/implement verb → TEST only | SubPhaseIds: ["5.1"].

---

## B15-t1-057-003 — "write tests" in a build+test request fires both IMPLEMENT and TEST

A: "Build CircuitBreakerService and write tests for state transitions."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1", "5.1"] | ContextCarryForward: true
"build" → IMPLEMENT. "write tests" → TEST. Both predicates present.

---

## B15-t1-057-004 — "QA" triggers QA predicate (not TEST)

A: "QA the MonitoringService alert threshold configuration."
B: TransitionType: feature_addition | SubPhaseIds: ["7.1"] | ContextCarryForward: true
"QA" → QA predicate → "7.1" in SubPhaseIds. TEST is different from QA.

---

## B15-t1-057-005 — "validate" fires IMPLEMENT (not TEST or QA)

A: "Validate the address format in AddressValidationService."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true
"validate" as code artifact creation verb → IMPLEMENT. Not TEST or QA.
