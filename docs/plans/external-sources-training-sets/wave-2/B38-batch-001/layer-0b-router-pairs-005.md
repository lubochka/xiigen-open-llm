# Layer 0-B — Router Pairs (005)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-005-001 — [] predicates → knowledge_edge (no routing)

PredicateSet: []
Scenario: Pure operational rollout vocabulary
Input: "Execute the production rollout and monitor the go-live deployment for any issues."

Routing decisions: none

RoutingTable: empty

DecisionCount: 0
TransitionType: N/A
ResultState: knowledge_edge
KnownFailureMode: no_predicate_match_capability_gap

CapabilityGapWarnings: ["execute", "rollout", "monitor", "go-live", "deployment"]

What the router does with [] predicate set:
1. No flow is selected
2. No phase receives a routing decision
3. ResultState set to knowledge_edge
4. All gap vocabulary terms logged as CapabilityGapWarnings
5. knowledge_edge state returned to caller

What the router does NOT do:
- Does NOT invoke GapHandler (GapHandler is for AI/guidance capability gaps; knowledge_edge is for domain vocabulary the system does not handle)
- Does NOT select ReleaseGateFlow71 by default
- Does NOT attempt to infer QA from context

---

## B38-0B-005-002 — knowledge_edge vs GapHandler: routing distinction

knowledge_edge path (B38 zero-predicate):
- Triggered by: project delivery vocabulary with no predicate matches
- Handler: none (knowledge_edge is a terminal state)
- Output: gap warning metadata + knowledge_edge state label
- No flow execution
- Used for: rollout, go-live, cutover, runbook, on-call, monitor, execute (execution context)

GapHandler path (B33/B34 style):
- Triggered by: AI or guidance vocabulary where a learning gap is identified
- Handler: GapHandler → SCHEDULE_CAPABILITY_LEARNING
- Output: capability learning task scheduled
- Used for: B33 guidance scenarios, B34 AI-specific gaps

In B38, "I don't know how to do a rollout" → knowledge_edge
In B33, "I don't know how to guide users through X" → GapHandler

The distinction is: knowledge_edge = vocabulary outside the system's domain; GapHandler = capability the system should learn.

---

## B38-0B-005-003 — Zero-predicate: gap warnings inventory

When input is pure B38 gap vocabulary and predicates = [], the CapabilityGapWarnings list is populated from:

Priority gap IDs:
- gap-project-delivery-go-live-001 (go-live term)
- gap-project-delivery-rollout-golive-001 (rollout term)
- gap-project-delivery-checklist-001 (checklist term)
- gap-project-delivery-cutover-001 (cutover term)
- gap-project-delivery-runbook-001 (runbook term)
- gap-project-delivery-launch-001 (launch term)

Other operational gaps (no assigned gap ID, logged as string):
- monitor
- execute (execution context)
- on-call
- rollback
- threshold

The gap ID list is finite. Terms not on the ID list are logged by their string value.
