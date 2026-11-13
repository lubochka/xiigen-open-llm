# 10 - Simulation Check

## Simulation 1 - Full event promotion

Input prompt:
"I want organizers to create events and have the platform automatically match users, place events in feeds, send notifications, and report campaign metrics."

Expected path:

- Node-01: [event submission and validation, event storage and status, audience matching, feed placement, notification delivery, promotion metrics]
- Node-02: formal wording for event validation, storage, matching formula, feed placement, notification rules, metrics
- Node-03: validate/store, index, analyze, match, place feed, notify, aggregate metrics
- Node-04: process event validity, score, feed position, notification channel
- Node-05: R1 event stored, R2 indexed, R3 analyzed, R4 matched, R5 feed updated, R6 notifications sent
- Node-06: organizer-readable promotion state
- Node-07: select validation, event sequence, matching, feed, notification, metrics arbiters
- Node-08: pass only if formulas and thresholds are correct
- Node-09: one organizer-facing result

## Simulation 2 - Past event date

Input prompt:
"The organizer tries to publish an event dated yesterday."

Expected path:

- Node-01: [event submission and validation]
- Node-02: reject past date wording
- Node-03: reject promotion and archive stale draft
- Node-04: validation rejected
- Node-05: no EventCreated, no promotion
- Node-06: future-date instruction
- Node-07: validation arbiter
- Node-08: flag if event was promoted
- Node-09: organizer asked to choose a future date

## Simulation 3 - Final seat race

Input prompt:
"There is one seat left and two users register at the same time."

Expected path:

- Node-01: [payment, pricing, and capacity]
- Node-02: atomic capacity wording
- Node-03: protect capacity
- Node-04: one succeeds, one waitlisted
- Node-05: R7 no oversell
- Node-06: capacity stayed accurate
- Node-07: capacity arbiter
- Node-08: flag if capacity goes below zero
- Node-09: capacity summary

## Simulation 4 - Notification burst

Input prompt:
"A popular event creates more than ten thousand notifications."

Expected path:

- Node-01: [operations and resilience, notification delivery]
- Node-02: burst delivery wording
- Node-03: segment queue by priority
- Node-04: strong matches first, others within one hour
- Node-05: delivery queue preserved
- Node-06: eligible matched users notified as delivery completes
- Node-07: notification rules and operations arbiters
- Node-08: flag if delayed notifications are dropped
- Node-09: operations-facing burst summary

## Self-check Table

| Node | Runtime pairs | Required shape |
|---|---:|---|
| Node-01 Domain splitter | 10 | Human prompt -> domain labels |
| Node-02 Domain translator | 10 | Fragment plus domain -> formal wording |
| Node-03 Scheme planner | 8 | Domain wordings -> b1...bn and fb1...fbn |
| Node-04 Domain processor array | 8 | One planned piece -> processed result |
| Node-05 Applier array | 8 | Processed result -> concrete Rqi |
| Node-06 Result translator array | 9 | Formal result -> natural language or consistency flag |
| Node-07 Arbiter selector | 8 | Result group -> selected arbiters |
| Node-08 Arbiter units | 8 | Review -> pass or flag with fbx |
| Node-09 Formatter | 8 | Approved fragments -> one final answer |

Total runtime-shaped pairs: 77.
