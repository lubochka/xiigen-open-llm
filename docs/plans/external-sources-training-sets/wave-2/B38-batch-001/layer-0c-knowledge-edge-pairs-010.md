# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-010-001 — knowledge_edge state: what it means

knowledge_edge is the result state when PredicateSet = [] in B38.

It means:
- All input vocabulary is gap vocabulary (no predicates fired)
- The system has no predicate to route on
- No phase receives a routing decision
- No flow executes
- The result is a metadata state containing CapabilityGapWarnings

knowledge_edge does NOT mean:
- The system encountered an error
- The input was invalid
- A capability learning task should be scheduled

---

## B38-0C-010-002 — knowledge_edge vs GapHandler: the critical distinction

knowledge_edge (B38 zero-predicate):
- Path: compiler returns [] → router sets knowledge_edge → returns to caller
- No handler invoked
- CapabilityGapWarnings populated with gap term names and gap IDs
- Terminal state — no further processing

GapHandler (B33/B34 style):
- Path: compiler identifies a learning gap → GapHandler invoked → SCHEDULE_CAPABILITY_LEARNING
- Handler IS invoked
- Result: new capability learning task created
- Used for AI/guidance scenarios where the system should learn new capabilities

The distinction:
- B38 gap vocabulary (rollout, go-live, cutover, etc.) = operational vocabulary OUTSIDE system scope
  → knowledge_edge (system acknowledges it doesn't handle this)
- B33/B34 AI guidance gaps = capabilities the system SHOULD have but doesn't yet
  → GapHandler + SCHEDULE_CAPABILITY_LEARNING

---

## B38-0C-010-003 — When knowledge_edge is reached in B38

Input patterns that produce knowledge_edge in B38:

Pure operational vocabulary:
- "Execute the rollout" → knowledge_edge
- "Monitor the go-live" → knowledge_edge
- "Follow the runbook" → knowledge_edge
- "Initiate cutover" → knowledge_edge
- "On-call handover" → knowledge_edge

Mixed operational with no QA/IMPLEMENT/DOCUMENT/SECURITY:
- "Execute the production rollout and monitor deployment" → knowledge_edge
- "Complete the go-live checklist and initiate rollback" → knowledge_edge
- "Run the cutover runbook at the launch window" → knowledge_edge

NOT knowledge_edge (one QA term present):
- "Review the rollout plan" → ["QA"] (review fires QA)
- "Approve the go-live" → ["QA"] (approve fires QA)

---

## B38-0C-010-004 — B38 rule 18 clarification (Important)

Rule 18 states:
"When predicates = [] and it's project delivery vocabulary, the result is knowledge_edge (not SCHEDULE_CAPABILITY_LEARNING/GapHandler which is for AI/guidance gaps). B38 gap vocabulary produces knowledge_edge state."

This rule overrides any general system behavior that might invoke GapHandler for unknown vocabulary. In B38 specifically, unknown-to-predicates vocabulary = project delivery operations vocabulary. The system classifies this as out-of-scope (knowledge_edge), not as a learning opportunity (GapHandler).

The knowledge_edge state name reflects: "I know I'm at the edge of my knowledge for this vocabulary." The system doesn't claim a bug; it claims a scope boundary.
