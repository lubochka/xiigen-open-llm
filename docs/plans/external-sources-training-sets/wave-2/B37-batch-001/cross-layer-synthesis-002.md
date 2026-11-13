# Cross-Layer Synthesis (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

---

## B37-CLS-002-001 — Mixed signal DOCUMENT: "Send progress report to stakeholders" → DOCUMENT fires

**Input:** "Send the weekly progress report to stakeholders and confirm the on-track delivery status."

**Full cross-layer trace showing false positive in managing context:**

---

### Layer 0-A (Compiler)

Token scan:
- "Send" → not in any predicate lexicon
- "weekly" → not in any predicate lexicon
- "progress" → not in any predicate lexicon (Rule 12) → gap
- "report" → DOCUMENT does NOT fire on "report" (Rule 3) → gap context
- "stakeholders" → DOCUMENT fires (Rule 6: ALWAYS — false positive in managing context)
- "confirm" → not in any predicate lexicon
- "on-track" → gap vocabulary
- "delivery" → gap vocabulary
- "status" → not in any predicate lexicon (Rule 4) → gap

Predicate list: ["DOCUMENT"]

---

### Layer 0-B (Router)

DOCUMENT predicate → route to 2.1 (DocumentationFlow)
DomainId: "software-development" (predicate fired — Rule 14)

---

### Layer 0-C (Knowledge Edge)

Gap vocabulary identified alongside DOCUMENT:
- "progress" → gap-project-managing-progress-001
- "on-track" → gap-project-managing-track-status-001
- "delivery status" → gap-project-managing-manage-delivery-001

False positive advisory:
- "stakeholders" fired DOCUMENT but intent was stakeholder notification → gap-project-managing-stakeholder-update-001
- "report" did NOT fire DOCUMENT (correctly — Rule 3)

---

### Layer 0-D (Flow Factory)

DOCUMENT → DocumentationFlow selected (sub-phase 2.1)
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicate fired)

---

### Layer 1 (Transition Orchestrator)

TransitionType: documentation (DOCUMENT predicate)
ContextCarryForward: false
Next state: DocumentationFlow (2.1)
Gap IDs forwarded as context metadata

---

### Layer 2 (Capability Gap Extended)

Mixed-signal trace recorded:
- Predicate routing: DOCUMENT → 2.1 (correct per lexicon)
- Managing intent: progress report to stakeholders (unaddressed by DocumentationFlow)
- Gap IDs: gap-project-managing-progress-001, gap-project-managing-track-status-001, gap-project-managing-stakeholder-update-001, gap-project-managing-manage-delivery-001

---

## B37-CLS-002-002 — Cross-layer synthesis output

**Input:** "Send the weekly progress report to stakeholders and confirm the on-track delivery status."

**Full output:**
```
Predicates: ["DOCUMENT"]
DomainId: "software-development"
Route: DocumentationFlow (2.1)
TransitionType: documentation
ContextCarryForward: false
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: [
  gap-project-managing-progress-001,
  gap-project-managing-track-status-001,
  gap-project-managing-stakeholder-update-001,
  gap-project-managing-manage-delivery-001
]
```

---

## B37-CLS-002-003 — False positive analysis: intent vs routing

**Intent:** Sending a progress report to stakeholders. This is a B37 project managing activity.
**Routing:** DocumentationFlow (2.1). This is a documentation flow for requirements/specifications.
**Mismatch:** The routing does not address the intent.
**Resolution:** CapabilityGapWarnings capture the intent mismatch. The managing intent is recorded for learning.

This is the expected B37 false positive pattern: "stakeholders" in any sentence fires DOCUMENT and routes to 2.1, regardless of whether documentation was the intent.

---

## B37-CLS-002-004 — v6 failure modes for this trace

| v6 corruption | What goes wrong | Correct value |
|--------------|----------------|--------------|
| v6-02 | DOCUMENT suppressed: "This is communication, not documentation" | DOCUMENT must fire — no suppression |
| v6-03 | DomainId "project-management" set because intent is managing | DomainId "software-development" — DOCUMENT fired |
| v6-01 | Not applicable (predicate fires correctly) | N/A |

v6-02 and v6-03 are the corruption risks for this trace. The model must resist the temptation to override lexical firing based on intent.
