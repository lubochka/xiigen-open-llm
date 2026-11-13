# Held-Out and Corruption Pairs (010)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-010-001 — HELD-OUT: "runbook" + "cutover" → knowledge_edge

Input: "Use the runbook to manage the production cutover and rollback procedure."

Correct result:
- "runbook" → GAP (gap-project-delivery-runbook-001)
- "cutover" → GAP (gap-project-delivery-cutover-001)
- "rollback" → operational gap (no gap ID)
- "Use" → not a predicate
- "manage" → not a predicate
- "production" → context word
- "procedure" → context word
- PredicateSet: []
- CapabilityGapWarnings: ["runbook", "cutover", "rollback"]
- Gap IDs: gap-project-delivery-runbook-001, gap-project-delivery-cutover-001
- ResultState: knowledge_edge
- KnownFailureMode: no_predicate_match_capability_gap

Gate B (held-out assertion): ResultState == "knowledge_edge" AND PredicateSet == []

---

## B38-HO-010-002 — CORRUPTION: QA wrongly fires on "cutover"

Corrupt input processing:
- "runbook" → context word (correctly not a predicate) ✓
- "cutover" → QA (WRONG — cutover is classified as requiring quality assurance review)
- "rollback" → QA (WRONG — rollback interpreted as a review action)
- "manage" → QA (WRONG — manage interpreted as quality management)

Corrupted result:
- PredicateSet: ["QA"] (WRONG — none of these words are QA lexicon terms)
- ResultState: routed to 7.1 (WRONG — should be knowledge_edge)

Why this is wrong:
"cutover", "rollback", and "manage" are not in the QA lexicon.
QA fires from: review, approve, verify, validate, sign-off, audit, quality, compliance, inspect, acceptance.
None of those words appear in the input. No QA term → no QA predicate → knowledge_edge.

The corruption is pattern-matching on release-context semantics (cutover = something that needs approval) rather than on vocabulary. Predicate firing must be vocabulary-driven.

---

## B38-HO-010-003 — HELD-OUT: runbook review (QA fires from "review")

Input: "Review the cutover runbook with the operations team."
Correct:
- "Review" → QA (review fires QA regardless of what is being reviewed)
- "cutover" → GAP
- "runbook" → GAP
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["cutover", "runbook"]
- ResultState: routed to 7.1 (NOT knowledge_edge — QA fires)

This is the key boundary: reviewing a runbook fires QA; using/executing a runbook does not.
