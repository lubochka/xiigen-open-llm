# Layer 0-B — Router Pairs (048)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-048-001 — "seek advice on security" does not route to SecurityAuditFlow

A: "Seek advice on the security architecture from the security guild."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "seek advice on" = advisory vocabulary → GapHandler. Security topic in advisory frame does not route to SecurityAuditFlow.

---

## B34-router-048-002 — "get feedback on QA process" does not fire QA predicate

A: "Get feedback on the quality assurance process from the QA (Quality Assurance) lead."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "get feedback on" = advisory vocabulary → GapHandler. Quality assurance as topic does not trigger QA predicate (only QA abbreviation as direct action verb fires).

---

## B34-router-048-003 — "sanity check" + "QA abbreviation" = QA fires (direct action)

A: "Sanity check the approach with the team and QA the release candidate."
Router decision: Predicates = ["QA"] → QAGatingFlow73 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "sanity check with team" = gap warning; "QA the release candidate" = QA abbreviation direct action → QA fires.

---

## B34-router-048-004 — "rubber duck" + "then QA" routing

A: "Rubber duck the release checklist and QA the platform before shipping."
Router decision: Predicates = ["QA"] → QAGatingFlow73 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "rubber duck checklist" = gap warning; "QA the platform" = QA direct action → QAGatingFlow73.

---

## B34-router-048-005 — complete Round 3 routing reference

Round 3 vocabulary routing:
- show how to → GapHandler
- demonstrate (to person) → GapHandler
- introduce [person] to → GapHandler
- introduce [feature] into → ImplementationFlow41
- brief [person] on → GapHandler
- familiarize with → GapHandler
- consult on/with → GapHandler
- seek advice on → GapHandler
- get feedback on → GapHandler
- sanity check [X] with → GapHandler
- second opinion → GapHandler
- rubber duck → GapHandler
- sounding board → GapHandler
All GapHandler invocations: SCHEDULE_CAPABILITY_LEARNING emitted once when predicate set = [].
