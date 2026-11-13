# Layer 0-D — Flow Factory Pairs (003)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-003-001 — TEST: UnitTestingFlow51 factory (embedded in guidance)

Predicates: ["TEST"] | Request: "Coach the team and write unit tests for the payment processor."
Factory selection: UnitTestingFlow51 (unit tests + pure function/module → 5.1)
TransitionType: documentation (no IMPLEMENT)
Gap for "coach" = suppressed (TEST fires)

---

## FF-003-002 — IMPLEMENT+TEST: two-flow factory

Predicates: ["IMPLEMENT", "TEST"] | Request: "Build the quiz module and write integration tests."
Factory selection: ImplementationFlow41 + E2ETestingFlow53 (integration → 5.3)
TransitionType: feature_addition
Ordering: ImplementationFlow41 first (test the built artifact)

---

## FF-003-003 — ARCHITECTURE: ArchitectureFlow31 factory (embedded)

Predicates: ["ARCHITECTURE"] | Request: "Walk the team through architecting the training platform."
Factory selection: ArchitectureFlow31 (sub-phase 3.1 — application-level platform)
TransitionType: documentation

---

## FF-003-004 — QA: QAGatingFlow73 factory

Predicates: ["QA"] | Request: "QA the developer onboarding workflow."
Factory selection: QAGatingFlow73 (7.3 — feature acceptance gate; not staging/release context)
TransitionType: no contribution

---

## FF-003-005 — v6 corruption: MENTORING flow invented

v6 corrupt: Predicates: ["MENTORING"] → MentoringFlow selected
Correct: Predicates: [] → GapHandler (no flow factory invoked)
Error type: v6-01 — MENTORING predicate does not exist; MentoringFlow does not exist.
Fix: Guidance vocabulary → GapHandler. No predicate factory selection for guidance verbs.
