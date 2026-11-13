# Layer 0-B — Router Pairs (060)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-060-001 — complete B34 routing summary (all three rounds)

B34 routing algorithm (final reference):
1. Tokenize request. Identify guidance vocabulary tokens (all rounds).
2. Identify requester code action tokens (implement, build, integrate [direct], write [code/tests/docs], architect, harden, audit, secure, document, QA [abbrev], design [structural]).
3. Check for "show how to / demonstrate how to / teach to / explain [topic]" → these are NOT code action tokens.
4. Check for "walk through integrating / facilitate [where integrate]" → these ARE API_CONTRACT tokens.
5. Predicate set = [code action tokens]. If [] → GapHandler. If non-[] → code flows + CapabilityGapWarning.

---

## B34-router-060-002 — gap ID selection routing

Gap ID selection (from predicate set = []):
- new hire / onboard / familiarize / introduce [person] context → onboard-001
- train / teach / explain / show how to / demonstrate / workshop / present context → train-001
- mentor / coach / advise / guide / consult / seek advice / get feedback / sanity check / second opinion / rubber duck / sounding board / discuss / review with / run through / brief context → mentor-001
- ambiguous/mixed → mentor-001 as default

---

## B34-router-060-003 — SCHEDULE_CAPABILITY_LEARNING routing rule

SCHEDULE_CAPABILITY_LEARNING routed to: gap learning system.
Emitted: when GapHandler invoked (predicate set = []).
Payload fields: gap_id, domainId (software-development), request (original text).
Never routed to code flows. Never emitted when code predicates fire.

---

## B34-router-060-004 — CapabilityGapWarning routing rule

CapabilityGapWarning routed to: execution result payload (appended, not a separate flow).
Emitted when: guidance vocabulary present + predicate set ≠ [].
Not emitted when: GapHandler invoked (full gap replaces warning in that case).
Content: {gap_id: "[most specific]", domainId: "software-development"}.

---

## B34-router-060-005 — B34 DomainId routing invariant

DomainId: software-development for ALL B34 gap events.
No DomainId: education, training, HR (Human Resources), learning-management, or other domain is valid.
This invariant holds for: GapHandler invocations, SCHEDULE_CAPABILITY_LEARNING payloads, CapabilityGapWarning payloads.
