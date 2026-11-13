# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-013-001 — "risk-log" knowledge edge: RAID-R vocabulary

**Knowledge edge: risk log is RAID-R gap vocabulary**

"Risk log" is the Risks component (R) of the RAID log (Risks, Assumptions, Issues, Dependencies). It is project managing vocabulary with no registered predicate.

**Gap ID:** gap-project-managing-raid-log-001 (risk log = RAID-R subset)

**Zero-predicate scenarios for risk log:**
- "Update the risk log" → []
- "Maintain the risk log" → []
- "Populate the risk log" → []
- "Review entries in the risk log" → [QA] (exception: "review" fires QA)
- "Track the risk log" → [] (track ≠ TEST — Rule 2)

---

## B37-0C-013-002 — "risk-log" vs "test the risks": boundary

**Knowledge edge: risk assessment ≠ testing**

| Input | Predicates | Reason |
|-------|-----------|--------|
| "Track all risks in the risk log" | [] | track ≠ TEST; risk log ≠ predicate |
| "Test all risks in the risk log" | [TEST] | "test" fires TEST (Rule 21) |
| "Verify risk mitigations" | [TEST] | "verify" fires TEST (Rule 21) |
| "Assess risks in the log" | [] | "assess" not in any predicate lexicon |
| "Review the risk log" | [QA] | "review" fires QA (Rule 19) |

"Track" and "assess" and "manage" → zero predicates.
"Test" and "verify" → TEST fires.
"Review" → QA fires.

---

## B37-0C-013-003 — "risk-log" gap ID registration

**Complete risk log gap ID mapping:**

Primary gap ID: gap-project-managing-raid-log-001
- Covers: risk log, risk register, RAID-R, risk tracking, risk management

This gap ID is the same one used for the full RAID log (gap-project-managing-raid-log-001). A separate risk-log-specific gap ID is NOT registered — the RAID log gap ID covers the entire R+A+I+D vocabulary family.

**Related gap IDs that may co-fire:**
- gap-project-managing-track-001 (if "track" appears in the risk log sentence)
- gap-project-managing-escalation-001 (if escalation of risks is mentioned)

---

## B37-0C-013-004 — "risk-log" with "document" false-positive analysis

**Input:** "Document all project risks in the risk log."

**Knowledge edge analysis:**
- DOCUMENT fires on "document" (Rule 8)
- Risk log → gap warning forwarded: gap-project-managing-raid-log-001
- Intent: risk management (project managing gap)
- Routing: DocumentationFlow (2.1) — addresses documentation aspect only
- Risk management intent: NOT addressed by DocumentationFlow

**Teaching point:** When "document" fires, the routing addresses documentation. The risk management intent is captured only as a gap warning. This is the same pattern as all B37 false positives.

---

## B37-0C-013-005 — Five zero-predicate risk log test cases

1. "Set up the risk log." → []
2. "Track risks in the risk log." → []
3. "Escalate high-risk items from the risk log." → []
4. "Maintain and monitor the project risk log." → []
5. "Populate the risk log before the delivery milestone." → []

All five: DomainId "project-management", knowledge_edge, SCL emitted, gap-project-managing-raid-log-001 forwarded.
