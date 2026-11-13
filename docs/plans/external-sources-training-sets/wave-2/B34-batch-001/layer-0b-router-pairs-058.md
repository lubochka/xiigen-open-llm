# Layer 0-B — Router Pairs (058)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-058-001 — all three rounds combined: single routing invariant

The single routing invariant for B34:
1. Scan full request for code action verbs (requester-performed: implement, build, deploy, integrate [direct], write [code/tests/docs], architect, harden, audit, secure, document, QA [abbrev]).
2. If none found: predicate set = [] → GapHandler → most specific gap ID → SCHEDULE_CAPABILITY_LEARNING once.
3. If any found: code predicate fires → route to code flow → GapWarning appended for each guidance verb.
4. "show/demonstrate/teach/explain how to [code verb]" = learning target → NOT a code action verb.
5. "walk through integrating [X]" = integrate exception → API_CONTRACT fires.

---

## B34-router-058-002 — "how to [code verb]" exception to code predicate rule

"How to implement X" → GapHandler (learning target)
"How to test X" → GapHandler (learning target)
"How to integrate X" → GapHandler (learning target; integrate exception blocked here)
"How to architect X" → GapHandler (learning target)
Exception blocked ONLY for participial form: "walk through integrating" → API_CONTRACT.

---

## B34-router-058-003 — complete "integrate" routing reference for B34

"Integrate [X] with [Y]" (requester direct) → API_CONTRACT → APIContractFlow33
"Walk through integrating [X]" (participial in guidance frame) → API_CONTRACT → APIContractFlow33 + gap warning
"Show how to integrate [X]" (infinitive) → GapHandler
"Demonstrate how to integrate [X]" (infinitive) → GapHandler
"Teach to integrate [X]" (infinitive) → GapHandler
"Facilitate [session where team] integrates [X]" → special: API_CONTRACT fires (integrate exception)
"Discuss integrating [X]" → borderline: if "integrating" is participial planning → GapHandler preferred

---

## B34-router-058-004 — CapabilityGapWarning payload routing

When CapabilityGapWarning emitted:
- It is appended to the flow execution result, NOT a separate flow invocation.
- Payload: {gap_id: "[most specific]", domainId: "software-development", request: "[original]"}
- Suppressed only when: requester's action is the same topic as the guidance vocabulary (no double-warning)
- Never causes SCHEDULE_CAPABILITY_LEARNING to emit.

---

## B34-router-058-005 — "train [model]" vs "train [team]" routing

"Train the ML (Machine Learning) model on the dataset" → outside B34 scope (ML training operation)
"Train the team on [technology]" → GapHandler → gap-skills-guidance-train-001
"Retrain the recommendation engine" → outside B34 scope
Rule: B34 scope = human skill/guidance only. All model training operations bypass B34 routing.
