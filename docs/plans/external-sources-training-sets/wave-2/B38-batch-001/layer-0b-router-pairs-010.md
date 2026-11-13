# Layer 0-B — Router Pairs (010)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-010-001 — [] → knowledge_edge (pure cutover/runbook routing)

PredicateSet: []
Scenario: CutoverRunbookExecution
Input: "Execute the cutover runbook and initiate rollback if the error rate exceeds threshold."

Routing decisions: none

DecisionCount: 0
TransitionType: N/A
ResultState: knowledge_edge
CapabilityGapWarnings: ["execute", "cutover", "runbook", "rollback"]
Gap IDs fired: gap-project-delivery-cutover-001, gap-project-delivery-runbook-001

What the router does:
1. Receives empty predicate set from compiler
2. Sets ResultState = knowledge_edge
3. Populates CapabilityGapWarnings with all gap terms
4. Returns knowledge_edge state to orchestrator
5. No flow execution; no N01-N09 traversal

---

## B38-0B-010-002 — knowledge_edge: no phase receives routing

When knowledge_edge is the result:
- Phase 7.1 does NOT receive a routing decision (no ReleaseGateFlow71 selected)
- Phase 4.1 does NOT receive a routing decision
- Phase 2.1 does NOT receive a routing decision
- Phase 3.4 does NOT receive a routing decision

All phases are bypassed. The result is a terminal metadata state, not a routed flow.

---

## B38-0B-010-003 — knowledge_edge gap ID inventory for cutover+runbook scenario

For the specific input "Execute the cutover runbook...":

gap-project-delivery-cutover-001:
- Triggered by: "cutover"
- Meaning: the system has no flow capability for production cutover operations

gap-project-delivery-runbook-001:
- Triggered by: "runbook"
- Meaning: the system has no flow capability for runbook execution

Additional non-ID gaps:
- "execute" → operational execution; no predicate match
- "rollback" → operational recovery; no predicate match

---

## B38-0B-010-004 — Distinguishing zero-predicate with review word vs pure operational

Zero predicates, knowledge_edge:
Input: "Execute the cutover runbook and initiate rollback."
No QA terms → [] → knowledge_edge

One predicate (QA), 7.1 routing:
Input: "Review the cutover runbook."
"Review" → QA → ["QA"] → 7.1 routing

The presence of a single QA word ("review") prevents knowledge_edge and produces a 7.1 routing. Even reviewing a runbook (a gap artifact) fires QA and routes to 7.1 with gap warnings.

This boundary test appears in Layer 0-A file 010, pair 004 for confirmation.
