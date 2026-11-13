# Cross-Layer Synthesis (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B37-CLS-008-001 — DOCUMENT+IMPLEMENT fires: "track, coordinate, document stakeholder needs, and build the feature"

**Input:** "Track delivery blockers, coordinate stakeholder needs, document the requirements, and build the notification service."

**Full cross-layer trace: DOCUMENT + IMPLEMENT both fire; track/coordinate/blockers → gap warnings**

---

### Layer 0-A (Compiler)

Token scan:
- "Track" → NOT in TEST lexicon (Rule 2) → gap
- "delivery blockers" → gap
- "coordinate" → not in any predicate lexicon (Rule 11) → gap
- "stakeholder needs" → "stakeholder" fires DOCUMENT (Rule 6)
- "document" → DOCUMENT fires (Rule 8)
- "requirements" → DOCUMENT fires (Rule 8)
- "build" → IMPLEMENT fires (Rule 9)
- "notification service" → "service" fires IMPLEMENT (Rule 9)

Multiple DOCUMENT triggers (stakeholder, document, requirements) → ONE DOCUMENT predicate
Multiple IMPLEMENT triggers (build, service) → ONE IMPLEMENT predicate

Predicate list: ["DOCUMENT", "IMPLEMENT"]

---

### Layer 0-B (Router)

DOCUMENT → DocumentationFlow (2.1)
IMPLEMENT → BackendServiceFlow41 (4.1)
DomainId: "software-development" (predicates fired)
Routes: 2.1 + 4.1 (parallel eligible)

---

### Layer 0-C (Knowledge Edge)

Gap vocabulary identified alongside predicates:
- "track" → gap-project-managing-track-001
- "delivery blockers" → gap-project-managing-blockers-001, gap-project-managing-manage-delivery-001
- "coordinate" → gap-project-managing-coordinate-001
- "stakeholder needs" intent → gap-project-managing-stakeholder-update-001

SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates fired)

---

### Layer 0-D (Flow Factory)

DOCUMENT → DocumentationFlow (2.1)
IMPLEMENT → BackendServiceFlow41 (4.1)
Both factories selected (parallel eligible)
DomainId: "software-development"

---

### Layer 1 (Transition Orchestrator)

TransitionType: documentation + feature_addition (two predicates)
ContextCarryForward: false
Next states: DocumentationFlow (2.1) + BackendServiceFlow41 (4.1)
Gap IDs forwarded: [gap-project-managing-track-001, gap-project-managing-blockers-001, gap-project-managing-coordinate-001, gap-project-managing-stakeholder-update-001, gap-project-managing-manage-delivery-001]

---

### Layer 2 (Capability Gap Extended)

Mixed-signal trace recorded:
- Primary routing: DOCUMENT → 2.1 (requirements documentation addressed)
- Secondary routing: IMPLEMENT → 4.1 (notification service build addressed)
- Managing intent: tracking blockers + coordination (NOT addressed by either flow)
- Gap IDs: track-001, blockers-001, coordinate-001, stakeholder-update-001, manage-delivery-001

---

## B37-CLS-008-002 — Full output summary

```
Predicates: ["DOCUMENT", "IMPLEMENT"]
DomainId: "software-development"
Routes: DocumentationFlow (2.1) + BackendServiceFlow41 (4.1)
TransitionType: documentation + feature_addition
ContextCarryForward: false
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: [
  gap-project-managing-track-001,
  gap-project-managing-blockers-001,
  gap-project-managing-coordinate-001,
  gap-project-managing-stakeholder-update-001,
  gap-project-managing-manage-delivery-001
]
```

---

## B37-CLS-008-003 — Why the sentence produces two separate routes

"track...coordinate...document stakeholder needs...build the notification service"

Technical vocabulary: "document", "requirements" (DOCUMENT), "build", "service" (IMPLEMENT) → two flows
Managing vocabulary: "track", "coordinate", "blockers", "stakeholder needs" → gap warnings + managing intent unaddressed

The sentence mixes project managing request (track/coordinate/blockers) with technical work (document requirements, build service). The technical part routes to flows. The managing part routes to gap warnings only.

---

## B37-CLS-008-004 — v6 failure modes

| v6 corruption | What goes wrong | Correct value |
|--------------|----------------|--------------|
| v6-01 | COORDINATION predicate added alongside DOCUMENT + IMPLEMENT | ["DOCUMENT", "IMPLEMENT"] only |
| v6-02 | DOCUMENT suppressed: "stakeholder needs is managing communication" | DOCUMENT fires on "stakeholder" — no suppression |
| v6-03 | DomainId "project-management" despite two predicates | DomainId "software-development" — predicates fired |

All three v6 types present as risks. v6-01 is the highest risk when managing vocabulary is prominently featured.

---

## B37-CLS-008-005 — Synthesis lesson: managing vocabulary never overrides technical predicates

The key lesson of CLS-008: managing vocabulary and technical vocabulary can coexist in one sentence. When they do:

1. Technical predicate tokens fire normally (DOCUMENT, IMPLEMENT, etc.)
2. Managing vocabulary produces gap warnings only
3. Routing is determined entirely by predicate firing
4. DomainId is "software-development" (any predicate)
5. SCL is NOT emitted (any predicate)
6. Managing intent is captured in gap warnings and forwarded
7. The technical flows do NOT address the managing intent — they address the technical predicate
