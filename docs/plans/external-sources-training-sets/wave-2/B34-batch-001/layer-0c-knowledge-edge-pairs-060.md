# Layer 0-C — Knowledge Edge Pairs (060)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-060-001 — complete B34 guidance vocabulary list (final consolidated reference)

All vocabulary → gap when standalone (predicate set = []):

**Round 1**: mentor, coach, teach, train [human], guide, explain, onboard, shadow, upskill, advise, facilitate, pair-with

**Round 2**: walk-through/walkthrough, workshop, review-with, ramp-up, show [guidance/demo], introduce [person], brief [person] on, familiarize with, run through [with person], present to, discuss

**Round 3**: show how to, demonstrate, demonstrate how to, introduce [person], brief [person], familiarize with, run through, review with, present, discuss, consult on/with, seek advice on, get feedback on, sanity check, second opinion, rubber duck, sounding board, knowledge transfer, describe [to person], illustrate [to person], check with, check in with, get a review from, have [person] review, loop in, involve [person] in reviewing, help [person] understand, help [person] learn, hand over/off, show [person] the ropes, walk [person] through how to, lunch and learn, brown bag

**All**: DomainId: software-development | GapHandler invoked when predicate set = []

---

## KE-060-002 — complete B34 predicate trigger reference

Code predicates fire when requester performs DIRECT ACTION:
IMPLEMENT: build, implement, create, develop, deploy, configure, set up, introduce [feature into]
API_CONTRACT: integrate (requester direct), walk through integrating, facilitate integration of, help [person] integrate
ARCHITECTURE: architect, design [structural], define service boundaries
SECURITY: harden, secure, audit, patch vulnerabilities, implement security controls
TEST: write tests, create tests, add assertions
DOCUMENT: write docs/guide/runbook/brief [noun], update documentation
QA: QA [abbreviation as verb]

---

## KE-060-003 — final gap ID selection summary

gap-skills-guidance-onboard-001: new hire, familiarize, introduce [person], ramp-up, hand over, onboard, show [person] the ropes
gap-skills-guidance-train-001: teach, train, explain, show how to, demonstrate, workshop, present, describe, illustrate, lunch and learn, brown bag
gap-skills-guidance-mentor-001: mentor, coach, guide, advise, shadow, pair-with, walk-through, review with, brief, run through, discuss, consult, seek advice, get feedback, sanity check, second opinion, rubber duck, sounding board, knowledge transfer, check with, loop in, code review with

---

## KE-060-004 — final "integrate" exception summary

Integrate FIRES API_CONTRACT: requester direct action; walk through integrating; facilitate integration of [X]; help [person] integrate; pair with to integrate
Integrate DOES NOT fire: show how to integrate; demonstrate how to integrate; teach/train to integrate; explain how to/how to integrate; consult on integrating; discuss integrating (planning); rubber duck integration design; sanity check the integration; get feedback on integration; sounding board for integration design

---

## KE-060-005 — final SCHEDULE_CAPABILITY_LEARNING summary

When emitted: predicate set = [] after full scan; GapHandler invoked.
When NOT emitted: any code predicate fires; GapHandler suppressed.
Count: exactly once per full-gap request regardless of guidance verb count.
Payload: {gap_id: "[most specific]", domainId: "software-development", request: "[original text]"}.
DomainId invariant: software-development always. Never derived from topic.
