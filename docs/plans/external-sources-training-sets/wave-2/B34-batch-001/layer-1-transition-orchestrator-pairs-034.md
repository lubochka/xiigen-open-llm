# Layer 1 — Transition Orchestrator Pairs (034)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-034-001 — GapHandler invocation conditions (extended Round 3)

GapHandler INVOKED when predicate set = [] after scanning for:
- Round 1 guidance vocabulary (mentor, coach, teach, train [human], guide, explain, onboard, shadow, upskill, advise, facilitate, pair-with)
- Round 2 guidance vocabulary (walk-through, workshop, review-with, ramp-up, show, introduce [person], brief, familiarize, run through, present, discuss)
- Round 3 guidance vocabulary (show how to, demonstrate, consult, seek advice, get feedback, sanity check, second opinion, rubber duck, sounding board, describe, illustrate, check with, loop in, involve, have [person] review, knowledge transfer, lunch and learn, brown bag, hand over)
- AND no code predicates found

---

## TO-034-002 — GapHandler NOT invoked when (extended)

GapHandler NOT invoked when any of these code actions present:
- implement, build, create, develop, deploy, configure, set up, introduce [feature into]
- integrate [direct action] or walk through integrating or facilitate integration of
- architect, design [structural], define service boundaries
- harden, secure, audit, patch vulnerabilities, perform security review
- write [tests/code], create tests, add assertions
- write [docs/guide/runbook/brief noun], update documentation
- QA [abbreviation as verb]
Even ONE of these = GapHandler suppressed → CapabilityGapWarning emitted instead.

---

## TO-034-003 — SCHEDULE_CAPABILITY_LEARNING emission timing

SCHEDULE_CAPABILITY_LEARNING emitted: immediately upon GapHandler invocation, before any other response.
Payload constructed from: gap_id (most specific), domainId (always software-development), request (original text).
Never emitted when: GapHandler suppressed, or when code predicate fires.
Emitted: exactly once per request, regardless of guidance verb count.

---

## TO-034-004 — CapabilityGapWarning emission timing and position

CapabilityGapWarning emitted: after first code flow result is returned.
Appended to: first code flow execution result payload.
Content: {gap_id: "[most specific]", domainId: "software-development"}.
Not a separate flow invocation. Not blocking. Appended as supplemental metadata.

---

## TO-034-005 — orchestration: TransitionType determination with guidance

TransitionType determined by code predicates ONLY. Guidance vocabulary = no contribution to TransitionType.
IMPLEMENT present → feature_addition (always overrides documentation).
No IMPLEMENT, only DOCUMENT/TEST/ARCHITECTURE/API_CONTRACT/SECURITY → documentation.
QA only → no contribution.
Gap only (no code predicates) → none.
