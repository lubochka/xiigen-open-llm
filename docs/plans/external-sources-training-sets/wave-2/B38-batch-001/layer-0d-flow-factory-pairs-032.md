# Layer 0-D — Flow Factory Pairs (032)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B38-0D-032-001 — Factory invariants re-assertion for 025-036 and 037-060 range

Key factory invariants confirmed active for all new scenarios:

1. FlowCount == len(PredicateSet) always (knowledge_edge: FlowCount == 0)
2. One predicate → one flow; IMPLEMENT consolidates from multiple triggers
3. Gap warnings never create flows
4. Gate B not required for deterministic_generic flows
5. TransitionType == feature_addition iff IMPLEMENT in PredicateSet
6. QA flow always assigned the highest order number (last execution)

---

## B38-0D-032-002 — FlowCount by predicate count for new scenarios

| PredicateSet size | FlowCount | Example |
|---|---|---|
| 0 (knowledge_edge) | 0 | "Generate the SBOM." |
| 1 | 1 | ["QA"] / ["DOCUMENT"] / ["IMPLEMENT"] / ["SECURITY"] |
| 2 | 2 | ["QA","IMPLEMENT"] / ["DOCUMENT","SECURITY"] / ["QA","SECURITY"] |
| 3 | 3 | ["QA","IMPLEMENT","DOCUMENT"] / ["QA","DOCUMENT","SECURITY"] |
| 4 | 4 | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 5 | 5 | ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] |

---

## B38-0D-032-003 — New flow patterns in 037-060 not seen in 002-024

Two-flow without QA:
- ["DOCUMENT","SECURITY"]: DocumentationFlow(order 1) + SecurityAuditFlow(order 2)
- ["IMPLEMENT","DOCUMENT"]: BackendServiceFlow41(order 1) + DocumentationFlow(order 2)

These are valid two-flow factory outputs. QA-last rule does not apply when QA is absent.

Three-flow without QA:
- Not seen in this batch (all three-flow scenarios include QA from compliance/review)

---

## B38-0D-032-004 — SECURITY alone: single-flow factory

Scenario: CVESecurityAlone
Input: "Register the CVE with the national vulnerability database."
PredicateSet: ["SECURITY"]

Flows created:
1. SecurityAuditFlow (Phase 3.4, SECURITY, order 1)

TransitionType: no_contribution
FlowCount: 1
