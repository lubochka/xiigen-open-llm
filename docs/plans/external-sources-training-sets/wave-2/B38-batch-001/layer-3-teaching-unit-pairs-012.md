# Layer 3 — Teaching Unit Pairs (012)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-012-001 — Complete B38 teaching summary

This file summarizes the complete B38 teaching curriculum for AdaptiveLogicalOrchestrator training.

Scenario: B38 — Project delivery — release gates and go-live checklists
Sub-phase: 7.1 (Release and Deployment Automation)
Flow: ReleaseGateFlow71
Domain: software-development

---

## B38-TU-012-002 — The 10 most important B38 rules

1. QA fires from: review, approve, verify, validate, sign-off, audit, quality, compliance, inspect, acceptance
2. IMPLEMENT fires from: service (ALWAYS), build (ALWAYS including artifacts), pipeline (code artifact)
3. DOCUMENT fires from: criteria (ALWAYS — all qualifiers), document/documentation
4. SECURITY fires from: security, vulnerability; audit + security → SECURITY co-fires (rule 19)
5. Gap vocabulary: go-live, checklist, rollout, cutover, runbook, launch, on-call, monitor, execute, deployment
6. Gap vocab + other predicates → CapabilityGapWarning (does not suppress predicates)
7. Zero predicates → knowledge_edge (not GapHandler; not SCHEDULE_CAPABILITY_LEARNING)
8. TransitionType = feature_addition iff IMPLEMENT in predicate set
9. QA is always last in multi-predicate pipeline (absolute rule 12)
10. v6 corruptions: QA suppression (v6-01), gate bypass (v6-02), predicate collapse (v6-03)

---

## B38-TU-012-003 — Quick predicate lookup for exam/test scenarios

"sign off" → QA
"launch" → GAP
"service" → IMPLEMENT
"criteria" → DOCUMENT
"security audit" → QA + SECURITY
"build artifacts" → IMPLEMENT (from "build")
"deployment" → GAP
"rollout" → GAP
"monitor" → GAP
"pipeline" → IMPLEMENT
"review" → QA
"execute" → GAP

---

## B38-TU-012-004 — Phase routing quick reference for B38

QA → 7.1 (ReleaseGateFlow71)
IMPLEMENT → 4.1 (BackendServiceFlow41)
DOCUMENT → 2.1 (DocumentationFlow)
SECURITY → 3.4 (SecurityAuditFlow)
[] → knowledge_edge (no routing)
QA always last when present.
feature_addition when IMPLEMENT present.
no_contribution when IMPLEMENT absent (and predicates not empty).
