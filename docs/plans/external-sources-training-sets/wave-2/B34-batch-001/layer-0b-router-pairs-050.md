# Layer 0-B — Router Pairs (050)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-050-001 — "show how to" never fires the infinitive's predicate (routing rule)

Routing rule (extended):
"Show how to [build/implement/create]" → GapHandler (not IMPLEMENT)
"Show how to [test/write tests]" → GapHandler (not TEST)
"Show how to [integrate/connect]" → GapHandler (not API_CONTRACT)
"Show how to [architect/design]" → GapHandler (not ARCHITECTURE)
"Show how to [secure/harden]" → GapHandler (not SECURITY)
"Show how to [document/write docs]" → GapHandler (not DOCUMENT)
No exception exists for "show how to [code verb]" pattern.

---

## B34-router-050-002 — "demonstrate how to" mirrors "show how to" (routing rule)

Routing rule:
"Demonstrate how to [any code verb]" → GapHandler in all cases.
"Demonstrate [result/output] to [person]" → GapHandler (demonstrate = guidance vocabulary).
"Demonstrate and [code verb]" → code predicate fires + gap warning.
No exception exists for "demonstrate how to" pattern.

---

## B34-router-050-003 — "explain" routing extended reference

"Explain [topic] to [person]" → GapHandler → gap-skills-guidance-train-001 (always)
"Explain and [code verb]" → code predicate fires + gap warning
"Explain how to [code verb]" → GapHandler (same as "show how to")
"Explain [code verb]" (topic) → GapHandler (explain always gap)
No form of "explain" routes to a code flow without a separate direct code action by requester.

---

## B34-router-050-004 — "onboard" + "train" routing distinction

"Onboard [person] to [system/team]" → GapHandler → gap-skills-guidance-onboard-001
"Train [person] on [topic]" → GapHandler → gap-skills-guidance-train-001
"Onboard and train" → GapHandler → gap-skills-guidance-onboard-001 (most specific)
"Mentor" → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-050-005 — purpose-clause guidance routing

"To mentor the team, [code verb]..." → code predicate fires + gap warning (purpose clause = not requester's action)
"To help them learn, [code verb]..." → code predicate fires + gap warning
"To support onboarding, [code verb]..." → code predicate fires + gap warning
Rule: purpose clause containing guidance vocabulary = gap warning + code predicate fires for the actual action in the clause body.
