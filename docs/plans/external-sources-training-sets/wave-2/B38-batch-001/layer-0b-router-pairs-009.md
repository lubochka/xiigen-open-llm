# Layer 0-B — Router Pairs (009)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-009-001 — [QA, IMPLEMENT, DOCUMENT, SECURITY] → 4 routing decisions

PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
Scenario: FullReleaseGatePipeline
Input: "Review the payment service deployment, validate the release criteria, audit the security controls, and approve the go-live."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. DOCUMENT → Phase 2.1 (DocumentationFlow)
3. SECURITY → Phase 3.4 (SecurityAuditFlow)
4. QA → Phase 7.1 (ReleaseGateFlow71) — QA last (absolute)

RoutingTable:
| Predicate | Target Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| DOCUMENT | 2.1 | DocumentationFlow | 2 |
| SECURITY | 3.4 | SecurityAuditFlow | 3 |
| QA | 7.1 | ReleaseGateFlow71 | 4 (last) |

DecisionCount: 4
TransitionType: feature_addition
CapabilityGapWarnings: ["deployment", "go-live"]
OutputFiles: [service_review.md, release_criteria_doc.md, security_audit_report.md, release_gate_decision.md, build_provenance.md, compliance_summary.md]
DomainId: software-development

---

## B38-0B-009-002 — Ordering rule for four predicates

The canonical B38 four-predicate execution order:
IMPLEMENT → DOCUMENT → SECURITY → QA

Rationale:
- IMPLEMENT: code artifacts produced first (code review output, service analysis)
- DOCUMENT: criteria documents produced second (may reference IMPLEMENT output)
- SECURITY: security audit third (uses code and documentation as input)
- QA: release gate last (uses all prior outputs to make gate decision)

This ordering means the release gate (QA, Phase 7.1) has access to all prior phase outputs when making the approval/rejection decision.

---

## B38-0B-009-003 — DecisionCount = 4 in four-predicate scenario

Each predicate produces exactly one routing decision.
Four predicates → DecisionCount = 4.

The DecisionCount does not include:
- CapabilityGapWarnings (these are metadata, not decisions)
- Individual QA lexicon terms (multiple QA-firing terms count as one predicate → one decision)
- Routing steps within a flow (N01 through N09 are flow steps, not routing decisions)

---

## B38-0B-009-004 — Feature_addition in full four-predicate scenario

TransitionType = feature_addition because IMPLEMENT is in the predicate set.
Even though QA, DOCUMENT, and SECURITY would individually produce no_contribution, the presence of IMPLEMENT elevates the whole result to feature_addition.

This reflects the reality: when a service is being reviewed (IMPLEMENT fires), code work is happening. The release gate is verifying a code contribution.
