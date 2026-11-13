# Layer 0-C — Knowledge Edge Pairs (058)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-058-001 — "check with" / "check in with" as advisory vocabulary

"Check with the security team on the encryption approach" → gap (check with = advisory → mentor-001)
"Check in with the architect on the design decisions" → gap (check in with = advisory → mentor-001)
"Check with the team and implement the recommended approach" → IMPLEMENT fires; check with = gap warning
"Check in with the tech lead and write the tests" → TEST fires; check in with = gap warning
Rule: "check with" / "check in with [person] on [topic]" = advisory vocabulary → gap.

---

## KE-058-002 — "get a review from" / "have [person] review" as guidance vocabulary

"Get a review from the senior engineer on the implementation" → gap (request for review = advisory → mentor-001)
"Have the tech lead review the architecture" → gap (have [person] review = requesting mentoring act)
"Get a review from the security team and patch the vulnerabilities" → SECURITY fires; get a review = gap warning
"Have the team review and update the documentation" → DOCUMENT fires; have team review = gap warning
Rule: "get a review from" / "have [person] review" = guidance vocabulary → gap.

---

## KE-058-003 — "loop in" / "involve" as guidance vocabulary edge

"Loop in the security team on the authentication design" → gap (loop in = collaborative advisory vocabulary → mentor-001)
"Involve the QA (Quality Assurance) team in reviewing the test strategy" → gap (involve in reviewing = guidance framing → mentor-001)
"Loop in the team and implement the changes" → IMPLEMENT fires; loop in = gap warning
"Involve the architect and architect the solution" → ARCHITECTURE fires; involve = gap warning
Rule: "loop in [person] on" / "involve [person] in reviewing" = guidance vocabulary → gap.

---

## KE-058-004 — "walk [person] through how to [verb]" = gap (learning target)

"Walk the team through how to configure the Istio (Istio service mesh) service mesh" → gap (walk through how to = learning frame → gap)
"Walk the engineers through how to write the tests" → gap (walk through how to write = learning target → gap)
"Walk through how to integrate the OAuth2 provider" → gap (walk through how to integrate = infinitive exception → gap)
Rule: "walk [person] through how to [code verb]" = learning frame → gap. Same as "show how to."

---

## KE-058-005 — complete B34 knowledge edge master rule list (final)

1. Guidance vocabulary (all rounds) + standalone topic = gap always.
2. Guidance vocabulary + [how to + infinitive code verb] = gap always (infinitive exception).
3. Guidance vocabulary + [participial code verb, requester as agent] = predicate fires + gap warning.
4. Guidance vocabulary + advisory + [direct requester code action] = predicate fires + gap warning.
5. "Integrate" fires API_CONTRACT EXCEPT in: show/demonstrate/teach/train/explain how to integrate.
6. "Integrate" fires API_CONTRACT even for: participate in/walk through integrating, facilitate integration of.
7. Third-party code actions = gap EXCEPT "integrate" which fires API_CONTRACT even when third party performs it.
8. DomainId = software-development for ALL B34 gap events (invariant, not derived).
9. SCHEDULE_CAPABILITY_LEARNING emitted once per full-gap request only.
10. GapHandler suppressed when any code predicate fires (gap warning emitted instead).
