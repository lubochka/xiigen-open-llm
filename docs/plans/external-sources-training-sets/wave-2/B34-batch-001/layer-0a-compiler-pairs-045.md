# Layer 0-A — Compiler Pairs (045)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-045-001 — guidance + SECURITY topic = gap (advisory)

A: "Seek advice on hardening the authentication layer from the security guild."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "seek advice on [security topic]" = advisory framing → gap. Hardening topic does not trigger SECURITY when the requester's action is "seek advice."

---

## B34-compiler-045-002 — "get feedback on" + security topic + audit action

A: "Get feedback on the security posture and then perform a full vulnerability audit."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "get feedback on security posture" = gap warning; "perform vulnerability audit" = requester's SECURITY action → SECURITY fires.

---

## B34-compiler-045-003 — guidance + QA disambiguation

A: "Consult on quality assurance practices for the release pipeline."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "consult on quality assurance practices" = advisory/guidance → gap. "Quality assurance" in this context = discussion topic, not QA abbreviation direct action verb.

---

## B34-compiler-045-004 — QA abbreviation direct action fires

A: "QA the release candidate before shipping."
B: Predicates: ["QA"] | QAGatingFlow73 | TransitionType: no contribution
Rule: "QA" as direct action verb = QA fires. This is the abbreviation-as-verb form, not guidance vocabulary.

---

## B34-compiler-045-005 — guidance + DOCUMENT topic = gap

A: "Walk the team through the documentation standards we follow."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "walk through documentation standards" = explanation vocabulary → gap. Documentation topic in this frame does not fire DOCUMENT predicate.
