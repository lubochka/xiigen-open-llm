# Cross-Layer Synthesis (004)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

---

## B37-CLS-004-001 — IMPLEMENT mixed with managing: full trace

**Input:** "Manage delivery of the auth service — build the access control module."

**Full cross-layer trace: IMPLEMENT fires; managing vocabulary → warnings; DomainId: software-development**

---

### Layer 0-A (Compiler)

Token scan:
- "Manage" → not in any predicate lexicon (Rule 10) → gap
- "delivery" → gap vocabulary
- "auth" → not directly triggering
- "service" → IMPLEMENT fires (Rule 9: service is IMPLEMENT trigger)
- "build" → IMPLEMENT fires (Rule 9)
- "access control" → not in any predicate lexicon
- "module" → IMPLEMENT fires (Rule 9)

Three IMPLEMENT triggers: service, build, module → ONE IMPLEMENT predicate
Predicate list: ["IMPLEMENT"]

---

### Layer 0-B (Router)

IMPLEMENT → route to 4.1 (BackendServiceFlow41)
DomainId: "software-development" (IMPLEMENT fired — any predicate → "software-development")

---

### Layer 0-C (Knowledge Edge)

Managing vocabulary alongside predicate:
- "Manage delivery" → gap-project-managing-manage-delivery-001 (mixed-signal warning)
- Note: This gap ID is flagged as mixed-signal because "manage" + "delivery" + IMPLEMENT creates ambiguity about whether the delivery management intent or the build intent was primary

---

### Layer 0-D (Flow Factory)

IMPLEMENT → BackendServiceFlow41 (sub-phase 4.1)
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (IMPLEMENT predicate fired)

---

### Layer 1 (Transition Orchestrator)

TransitionType: feature_addition (IMPLEMENT predicate)
ContextCarryForward: false
Next state: BackendServiceFlow41 (4.1)
Gap IDs forwarded: [gap-project-managing-manage-delivery-001]

---

### Layer 2 (Capability Gap Extended)

Mixed-signal trace recorded:
- Primary routing: IMPLEMENT → BackendServiceFlow41
- Managing intent: delivery management (unaddressed)
- Gap ID: gap-project-managing-manage-delivery-001 (mixed-signal warning)
- No SCHEDULE_CAPABILITY_LEARNING (predicate fired)

---

## B37-CLS-004-002 — Cross-layer synthesis output

**Input:** "Manage delivery of the auth service — build the access control module."

**Full output:**
```
Predicates: ["IMPLEMENT"]
DomainId: "software-development"
Route: BackendServiceFlow41 (4.1)
TransitionType: feature_addition
ContextCarryForward: false
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: [gap-project-managing-manage-delivery-001]
```

---

## B37-CLS-004-003 — Why DomainId is "software-development" despite managing framing

The sentence framing is "Manage delivery of the [service]". The word "manage" is gap. The word "delivery" is gap. Yet the sentence produces DomainId "software-development".

**Reason:** "service" fires IMPLEMENT. Any predicate → "software-development". Rule 14.

The managing framing ("manage delivery of the") does NOT:
- Prevent IMPLEMENT from firing
- Override DomainId to "project-management"
- Add a managing predicate
- Suppress the predicate

It only contributes: gap-project-managing-manage-delivery-001 (a warning, not a route).

---

## B37-CLS-004-004 — Contrast: same managing framing without service/build/module

**Variant A (with IMPLEMENT tokens):**
"Manage delivery of the auth service." → ["IMPLEMENT"], DomainId "software-development"

**Variant B (without IMPLEMENT tokens):**
"Manage delivery of the auth programme." → [] (no IMPLEMENT triggers), DomainId "project-management"

**Variant C (replacing service with IMPLEMENT-inert noun):**
"Manage delivery of the authentication workflow." → [] (workflow is not IMPLEMENT trigger), DomainId "project-management"

The presence or absence of IMPLEMENT trigger tokens (service, module, build, create, implement, write) is the sole determinant.

---

## B37-CLS-004-005 — v6 failure modes for this trace

| v6 corruption | What goes wrong | Correct value |
|--------------|----------------|--------------|
| v6-03 | DomainId "project-management" set because framing is "Manage delivery" | DomainId "software-development" — IMPLEMENT fired |
| v6-01 | MANAGEMENT predicate added alongside IMPLEMENT | Predicates: ["IMPLEMENT"] only — no MANAGEMENT |
| v6-02 | Not applicable (no stakeholder) | N/A |

The primary corruption risk for this trace is v6-03: the model sees "manage delivery" framing and incorrectly sets DomainId "project-management" despite IMPLEMENT having fired. The DomainId rule is binary — ANY predicate → "software-development", regardless of framing.
