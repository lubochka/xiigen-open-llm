# Held-Out and Corruption Pairs (005)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-005-001 — HELD-OUT: pure rollout/go-live → knowledge_edge

Input: "Kick off the production rollout after the go-live window opens."

Correct result:
- "Kick off" → not a predicate
- "production" → context word
- "rollout" → GAP (gap-project-delivery-rollout-golive-001)
- "go-live" → GAP (gap-project-delivery-go-live-001)
- "window" → context word
- PredicateSet: []
- CapabilityGapWarnings: ["rollout", "go-live"]
- ResultState: knowledge_edge
- KnownFailureMode: no_predicate_match_capability_gap

Gate B (held-out assertion): PredicateSet == [] AND ResultState == "knowledge_edge"

---

## B38-HO-005-002 — CORRUPTION: QA wrongly fires (no QA terms present)

Corrupt input processing:
- "Kick off" → interpreted as an action verb → QA context (WRONG)
- "rollout" → QA (WRONG — rollout is gap vocabulary)
- "go-live" → QA (WRONG — go-live is gap vocabulary)

Corrupted result:
- PredicateSet: ["QA"] (WRONG — no QA terms actually present)
- ResultState: routed to 7.1 (WRONG — should be knowledge_edge)
- TransitionType: no_contribution (WRONG in a different way — should be N/A)

Why this is wrong:
None of the QA lexicon terms (review, approve, verify, validate, sign-off, audit, quality, compliance, inspect, acceptance) appear in the input. QA must only fire from explicit QA lexicon terms. "Kick off", "rollout", and "go-live" are not QA terms.

The corruption here is pattern-matching on the B38 scenario label (it's a release scenario so assume QA) rather than on actual input vocabulary. This is a critical error: predicate firing must be vocabulary-driven, not scenario-driven.

---

## B38-HO-005-003 — HELD-OUT: near-miss inputs that SHOULD fire QA

"Review the rollout plan" → ["QA"] (review fires; rollout is gap warning)
"Approve the go-live window" → ["QA"] (approve fires; go-live is gap warning)
"Sign off before the rollout" → ["QA"] (sign-off fires; rollout is gap warning)

These fire QA because they contain explicit QA lexicon words ("review", "approve", "sign off").
The inputs in B38-HO-005-001 do NOT contain these words; hence knowledge_edge.
