# Layer 0-B — Router Pairs (008)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-008-001 — [QA, SECURITY] → 7.1 + 3.4

PredicateSet: ["QA", "SECURITY"]
Scenario: SecurityAuditRelease (pre-release security audit)
Input: "Audit the security posture of the release and validate compliance before go-live approval."

Routing decisions:
1. SECURITY → Phase 3.4 (SecurityAuditFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Target Phase | Flow | Order |
|---|---|---|---|
| SECURITY | 3.4 | SecurityAuditFlow | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 (last) |

DecisionCount: 2
TransitionType: no_contribution (no IMPLEMENT)
CapabilityGapWarnings: ["go-live"]
OutputFiles: [security_posture_report.md, compliance_check.md, release_gate_decision.md]
DomainId: software-development

---

## B38-0B-008-002 — SECURITY routes to 3.4, not 7.1

Phase 3.4 = Security audit and compliance sub-phase.
SECURITY predicate always routes to Phase 3 in B38.
Phase 7.1 = Release gate (QA only).

The security audit (3.4) and release gate (7.1) run as independent routing decisions.
The security audit result may be consumed by the release gate as evidence, but they are separate flows.

---

## B38-0B-008-003 — [QA, SECURITY]: no_contribution despite two predicates

TransitionType = no_contribution for [QA, SECURITY]:
- SECURITY = security audit (not code authoring)
- QA = quality gate (not code authoring)
- IMPLEMENT is absent → no feature_addition

This is a review-and-audit scenario where no new code is written. The output is reports and decisions, not code artifacts.

Contrast with [QA, IMPLEMENT, SECURITY] which would be feature_addition because IMPLEMENT adds code.

---

## B38-0B-008-004 — Ordering [QA, SECURITY]: SECURITY before QA

For [QA, SECURITY], execution order:
1. SecurityAuditFlow (3.4) executes first — produces security posture report
2. ReleaseGateFlow71 (7.1) executes second (QA last rule)

The release gate (QA) uses the security audit output (SECURITY) as input evidence.
This ordering ensures the gate decision has complete security data.

QA ordering rule (absolute rule 12) applies equally when SECURITY is the only other predicate.
