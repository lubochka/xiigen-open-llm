# Layer 0-B — Router Pairs (023)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-023-001 — [QA] alone: canonical sign-off collection

PredicateSet: ["QA"]
Scenario: ReleaseSignOffCollection
Input: "Collect all release sign-offs from team leads and confirm the release is authorized."

Routing decision:
1. QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution
OutputFiles: [sign_off_register.md, release_authorization.md]

---

## B38-0B-023-002 — [QA, IMPLEMENT]: sign-off with service

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: AnalyticsServiceSignOff
Input: "Get sign-off on the analytics service before the release date."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition

---

## B38-0B-023-003 — [QA] with gap warning: sign-off checklist

PredicateSet: ["QA"]
Scenario: SignOffChecklist
Input: "Complete the sign-off checklist and get approval from the release manager."

Routing decision:
1. QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution
CapabilityGapWarnings: ["checklist"] — warning only; checklist is gap but does not suppress QA

---

## B38-0B-023-004 — [QA, DOCUMENT]: sign-off documentation

PredicateSet: ["QA", "DOCUMENT"]
Scenario: SignOffDocumentation
Input: "Document the sign-off process and verify all approval criteria are met before release."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution

---

## B38-0B-023-005 — [QA] with three gap warnings: go-live sign-off

PredicateSet: ["QA"]
Scenario: GoLiveSignOff
Input: "Sign off on the go-live checklist and confirm launch readiness."

Routing decision:
1. QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution
CapabilityGapWarnings: ["go-live", "checklist", "launch"]

This is the canonical v6-01 anti-corruption demonstration in routing:
QA fires from "sign off" → routes to 7.1.
Three gap vocabulary items produce three capability gap warnings.
None of the gap warnings become routing decisions.
Gap warnings do not suppress the QA routing decision.
