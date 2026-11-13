# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-005-001 — "stakeholder" fires DOCUMENT: false positive in managing context explained

**Knowledge edge: "stakeholder" → DOCUMENT fires ALWAYS**

Rule 6 states: "stakeholder" → DOCUMENT fires ALWAYS (even in status report sentence — false positive in intent but correct in lexicon).

**Why this is a false positive in managing context:**
- The intent of "notify stakeholders" is communication, not documentation
- The intent of "status update to stakeholders" is status reporting, not creating a document
- The intent of "escalation to stakeholders" is managing escalation, not documentation
- Yet in all these cases, DOCUMENT fires because "stakeholder" is in the DOCUMENT lexicon

**This is by design:** The lexicon treats "stakeholder" as a documentation signal because stakeholder management most commonly involves requirements gathering, specification writing, and acceptance criteria documentation in software development.

---

## B37-0C-005-002 — How "stakeholder" got into DOCUMENT lexicon

**DOCUMENT fires on:** document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case

"stakeholder" appears in this list because:
1. "Stakeholder requirements" is a common phrase in documentation contexts
2. "Gather stakeholder input for specifications" is documentation work
3. "Define acceptance criteria with stakeholders" is documentation + testing work

In project managing context:
- "Notify stakeholders" → DOCUMENT fires (false positive)
- "Update stakeholders on status" → DOCUMENT fires (false positive)
- "Escalate to stakeholders" → DOCUMENT fires (false positive)

In all three cases, DomainId = "software-development" (predicate fired) and the managing intent is captured only in CapabilityGapWarnings.

---

## B37-0C-005-003 — "stakeholder" false positive: five examples with gap warning handling

1. "Send weekly status report to stakeholders." → DOCUMENT fires; gap-project-managing-stakeholder-update-001
2. "Notify stakeholders of the escalation." → DOCUMENT fires; gap-project-managing-escalation-001, gap-project-managing-stakeholder-update-001
3. "Coordinate with stakeholders on delivery progress." → DOCUMENT fires; gap-project-managing-coordinate-001, gap-project-managing-progress-001, gap-project-managing-stakeholder-update-001
4. "Keep stakeholders informed of blockers." → DOCUMENT fires; gap-project-managing-blockers-001, gap-project-managing-stakeholder-update-001
5. "Escalate to stakeholders and the steering board." → DOCUMENT fires; gap-project-managing-escalation-001, gap-project-managing-stakeholder-update-001

---

## B37-0C-005-004 — CapabilityGapWarning gap-project-managing-stakeholder-update-001

**When to emit gap-project-managing-stakeholder-update-001:**

This gap ID is emitted when:
1. The sentence contains "stakeholder(s)"
2. The intent of the sentence is communication/notification/update (not documentation)
3. DOCUMENT fires as a false positive

**Purpose:** Captures the unmet managing intent even though DOCUMENT routing has occurred.

**NOT emitted when:**
- The stakeholder reference is genuinely documentation-related (e.g., "gather stakeholder requirements")
- The context is clearly specification or requirements work

---

## B37-0C-005-005 — "stakeholder" suppression prohibition

**What MUST NOT happen (v6-02 corruption):**
Suppressing DOCUMENT when "stakeholder" appears in a managing context.

**Correct behavior:**
- "stakeholder" in any sentence → DOCUMENT fires
- No contextual override permitted
- Intent analysis does not suppress lexical firing
- DomainId = "software-development" when DOCUMENT fires (even if intent is managing)
- CapabilityGapWarning captures the managing intent gap

**Anti-pattern example:**
v6 response: "Because this sentence is about notifying stakeholders of status (a managing activity), DOCUMENT should not fire."
This is WRONG. DOCUMENT fires. No exceptions.
