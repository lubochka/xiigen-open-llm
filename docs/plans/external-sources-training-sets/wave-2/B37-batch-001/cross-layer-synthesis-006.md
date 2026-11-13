# Cross-Layer Synthesis (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B37-CLS-006-001 — "stakeholder status update" → DOCUMENT fires, DomainId: software-development

**Input:** "Produce the weekly stakeholder status update and confirm delivery is on track."

**Full cross-layer trace: DOCUMENT fires on "stakeholder"; DomainId: software-development**

---

### Layer 0-A (Compiler)

Token scan:
- "Produce" → not in any predicate lexicon
- "weekly" → not in any predicate lexicon
- "stakeholder" → DOCUMENT fires (Rule 6: stakeholder fires DOCUMENT always)
- "status update" → "status" fires nothing (Rule 4); "update" not in DOCUMENT lexicon
- "confirm" → not in any predicate lexicon
- "delivery" → gap
- "on track" → gap

Predicate list: ["DOCUMENT"]

---

### Layer 0-B (Router)

DOCUMENT predicate → route to 2.1 (DocumentationFlow)
DomainId: "software-development" (predicate fired — Rule 14)

---

### Layer 0-C (Knowledge Edge)

Gap vocabulary identified alongside DOCUMENT:
- "status update" → gap-project-managing-status-report-001 (status report intent)
- "on track" → gap-project-managing-track-status-001
- "delivery" → gap-project-managing-manage-delivery-001
- "stakeholder" intent → gap-project-managing-stakeholder-update-001 (intent gap: notification, not documentation)

False positive advisory: "stakeholder" fired DOCUMENT. Intent is stakeholder status communication, not documentation. Gap warning captures intent mismatch.

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
- Managing intent: stakeholder status notification (unaddressed by DocumentationFlow)
- Gap IDs: gap-project-managing-stakeholder-update-001, gap-project-managing-status-report-001, gap-project-managing-track-status-001, gap-project-managing-manage-delivery-001

---

## B37-CLS-006-002 — Full output summary

```
Predicates: ["DOCUMENT"]
DomainId: "software-development"
Route: DocumentationFlow (2.1)
TransitionType: documentation
ContextCarryForward: false
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: [
  gap-project-managing-stakeholder-update-001,
  gap-project-managing-status-report-001,
  gap-project-managing-track-status-001,
  gap-project-managing-manage-delivery-001
]
```

---

## B37-CLS-006-003 — False positive analysis

**Intent:** Producing a weekly status update for stakeholders. Project managing communication.
**Routing:** DocumentationFlow (2.1). Documentation/requirements flow.
**Mismatch:** DocumentationFlow does not address status communication intent.
**Resolution:** CapabilityGapWarnings capture intent. Managing intent recorded for learning.

---

## B37-CLS-006-004 — v6 failure modes

| v6 corruption | What goes wrong | Correct value |
|--------------|----------------|--------------|
| v6-02 | DOCUMENT suppressed: "This is communication" | DOCUMENT must fire — stakeholder token absolute |
| v6-03 | DomainId "project-management" — intent is managing | DomainId "software-development" — DOCUMENT fired |
| v6-01 | Not applicable | N/A |

v6-02 and v6-03 are the two primary corruption risks for this trace.
