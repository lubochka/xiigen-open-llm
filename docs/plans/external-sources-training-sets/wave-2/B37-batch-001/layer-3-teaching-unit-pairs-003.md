# Layer 3 — Teaching-Unit Pairs (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-003-001 — Teaching: "stakeholder" false positive — managing intent vs documentation routing

**Core concept:** "stakeholder" → DOCUMENT fires ALWAYS. This creates a systematic false positive when the request intent is stakeholder communication/notification in a managing context.

**The false positive is intentional:** The lexicon treats "stakeholder" as a documentation signal. This is correct for most software development contexts (requirements, specifications, acceptance criteria). It is a false positive specifically in B37 project managing sentences.

**The correct response to the false positive:**
1. DOCUMENT fires (correct)
2. DocumentationFlow selected (correct)
3. DomainId "software-development" (correct)
4. CapabilityGapWarning gap-project-managing-stakeholder-update-001 (correct — captures managing intent)

---

## B37-L3-003-002 — Teaching: what to do with the false positive

**Wrong response (v6-02):** Suppress DOCUMENT because "the intent is managing, not documentation."
**Correct response:** Fire DOCUMENT + emit gap warning for the unmet managing intent.

The false positive is acknowledged via the gap warning, not corrected via suppression. Suppression would break the lexical consistency rule. The gap warning correctly records that the managing intent (stakeholder notification, status update) was not addressed.

---

## B37-L3-003-003 — Teaching: "stakeholder" vs other B37 managing vocabulary

| Token | Fires predicate? | Notes |
|-------|-----------------|-------|
| stakeholder | YES (DOCUMENT) | False positive in managing context |
| track | NO | Gap |
| coordinate | NO | Gap |
| escalate | NO | Gap |
| blockers | NO | Gap |
| follow-up | NO | Gap |
| action items | NO | Gap |
| manage | NO | Gap |
| progress | NO | Gap |
| RAG-status | NO | Gap |

"stakeholder" is the ONLY B37 managing vocabulary token that fires a predicate (DOCUMENT). All others are gap.

---

## B37-L3-003-004 — Teaching: managing intent capture via gap warnings

**The gap warning system is the mechanism for managing intent capture in B37.**

When DOCUMENT fires on "stakeholder" but the intent is managing notification:
- The DocumentationFlow processes whatever documentation work is associated
- The gap warning gap-project-managing-stakeholder-update-001 records the notification intent
- The learning pipeline accumulates these gap warnings to build evidence for a future NOTIFY predicate

**The gap warning IS the intent capture.** It is not a failure message. It is a structured signal that says "the system handled this as DOCUMENT but the user wanted STAKEHOLDER_UPDATE."

---

## B37-L3-003-005 — Teaching: five "stakeholder" scenarios with gap warning analysis

1. "Gather stakeholder requirements." → DOCUMENT (correct, no gap warning needed — genuine documentation)
2. "Update stakeholders on status." → DOCUMENT (false positive) + gap-project-managing-stakeholder-update-001
3. "Escalate to stakeholders." → DOCUMENT (false positive) + gap-project-managing-escalation-001 + gap-project-managing-stakeholder-update-001
4. "Notify stakeholders of blockers." → DOCUMENT (false positive) + gap-project-managing-blockers-001 + gap-project-managing-stakeholder-update-001
5. "Define acceptance criteria for stakeholders." → DOCUMENT (correct — documentation for stakeholder review) + TEST (acceptance). No stakeholder-update gap warning (intent is documentation).
