# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-019-001 — "stakeholder" → DOCUMENT fires: complete rule statement

**Knowledge edge: "stakeholder" fires DOCUMENT in EVERY context**

Rule 6 (absolute): Any occurrence of the token "stakeholder" or "stakeholders" fires DOCUMENT, regardless of:
- Sentence framing (sending, notifying, escalating, reporting)
- Intent (notification, communication, update, consultation)
- Surrounding vocabulary (managing, technical, mixed)

This is a documented false positive in B37 context. The lexical rule has no exceptions.

**Gap warning:** When DOCUMENT fires on "stakeholder" in a managing context, gap-project-managing-stakeholder-update-001 is raised alongside DOCUMENT routing.

---

## B37-0C-019-002 — "stakeholder" false positive: six example sentences

| Sentence | Predicates | Route |
|---------|-----------|-------|
| "Notify stakeholders of progress." | [DOCUMENT] | 2.1 |
| "Update stakeholders on status." | [DOCUMENT] | 2.1 |
| "Escalate blockers to stakeholders." | [DOCUMENT] | 2.1 |
| "Coordinate with stakeholders." | [DOCUMENT] | 2.1 |
| "Send the risk report to stakeholders." | [DOCUMENT] | 2.1 |
| "Track progress for stakeholders." | [DOCUMENT] | 2.1 |

All six sentences route to DocumentationFlow (2.1) because "stakeholders" fires DOCUMENT. None of them intend documentation — they intend project managing communication. All six raise gap-project-managing-stakeholder-update-001.

---

## B37-0C-019-003 — "stakeholder" vs equivalent non-firing tokens

**Knowledge edge: tokens that do NOT fire DOCUMENT in place of "stakeholder"**

| Token | Predicate | Notes |
|-------|----------|-------|
| stakeholder | DOCUMENT | Always fires |
| stakeholders | DOCUMENT | Always fires |
| programme board | None | Not in DOCUMENT lexicon |
| steering committee | None | Not in DOCUMENT lexicon |
| senior leadership | None | Not in DOCUMENT lexicon |
| delivery team | None | Not in DOCUMENT lexicon |
| project sponsor | None | Not in DOCUMENT lexicon |

"Notify the programme board" → zero predicates.
"Notify stakeholders" → DOCUMENT fires.

---

## B37-0C-019-004 — v6-02 corruption: suppressing DOCUMENT for "stakeholder"

**v6-02 corruption pattern:**
"Update stakeholders on delivery status" → DOCUMENT suppressed with reasoning: "This is a status update notification, not documentation activity."

**Why this is wrong:**
Rule 6 is absolute. DOCUMENT fires on "stakeholder" regardless of intent. v6-02 is the error of suppressing DOCUMENT because the sentence "looks like" a notification rather than documentation.

**Correct output:** ["DOCUMENT"], DomainId "software-development". Gap warning: gap-project-managing-stakeholder-update-001. The intent mismatch is captured in the gap warning, not by suppressing the predicate.

---

## B37-0C-019-005 — Stakeholder false positive summary

**Complete stakeholder false positive rule for B37:**

1. ANY sentence containing "stakeholder" fires DOCUMENT
2. The intent of the sentence is irrelevant to the firing decision
3. DomainId becomes "software-development" (predicate fired)
4. SCHEDULE_CAPABILITY_LEARNING is NOT emitted (predicate fired)
5. CapabilityGapWarning: gap-project-managing-stakeholder-update-001 raised
6. DocumentationFlow (2.1) is selected — it does NOT address managing intent
7. This is by design: the lexical firing rule is authoritative; intent analysis is not performed

This pattern is the canonical B37 false positive. It applies equally to: notify, update, send, escalate, communicate, report to "stakeholders".
