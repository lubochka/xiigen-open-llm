# Layer 0-C — Knowledge Edge Pairs (054)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-054-001 — complete boundary: "guidance vocabulary + how to [code verb]" → gap in all cases

All guidance vocabulary + "how to [code verb]" = gap (infinitive exception applies to ALL guidance vocabulary):
- "show how to build" → gap
- "demonstrate how to implement" → gap
- "explain how to integrate" → gap
- "walk through how to test" → gap
- "coach on how to architect" → gap
- "mentor on how to document" → gap
- "guide through how to secure" → gap
- "consult on how to implement" → gap
- "get feedback on how to test" → gap
Rule: guidance vocabulary + "how to" + infinitive = always gap. No exceptions across all vocabulary.

---

## KE-054-002 — "guidance vocabulary + [participial code verb phrase]" → predicate fires

Participial code verb phrase (requester performing action in progress) in guidance frame:
- "walk through integrating [X]" → API_CONTRACT + gap warning
- "coach through building [X]" → IMPLEMENT + gap warning
- "guide through writing tests" → TEST + gap warning
- "walk through hardening [X]" → SECURITY + gap warning
- "walk through documenting [X]" → DOCUMENT + gap warning
- "walk through architecting [X]" → ARCHITECTURE + gap warning
Rule: participial code verb in guidance frame = predicate fires + gap warning. Contrast with infinitive = gap only.

---

## KE-054-003 — "guidance vocabulary + [noun phrase describing code artifact]" → gap

Guidance vocabulary + noun phrase (no participial verb) describing code artifact = gap:
- "walk through the service architecture" → gap (architecture = noun; no architect verb)
- "explain the test suite" → gap (test suite = noun; no write-tests verb)
- "brief on the API contract" → gap (API contract = noun; no integrate verb)
- "discuss the security model" → gap (security model = noun; no harden verb)
- "present the implementation design" → gap (implementation design = noun; no implement verb)
Rule: guidance vocabulary + code artifact noun (no participial) = gap. The noun names a topic, not an action.

---

## KE-054-004 — "facilitate" extended edge cases

"Facilitate the sprint planning session" → gap (facilitate [event] = guidance vocabulary)
"Facilitate a security workshop" → gap (facilitate workshop = guidance event)
"Facilitate the integration of the payment provider" → API_CONTRACT fires (facilitate + direct integration noun phrase)
"Facilitate a session where the team integrates [X]" → gap (team integrates = third-party; session facilitation = guidance)
"Facilitate and integrate [X]" → API_CONTRACT fires; facilitate = gap warning
Rule: "facilitate integration of [X]" = API_CONTRACT (direct integration); "facilitate [event about integration]" = gap.

---

## KE-054-005 — "review" in code review context

"Do a code review with the junior developer" → gap (code review with person = mentoring act)
"Code review the PR (Pull Request)" → gap (code review = peer review act = guidance)
"Code review and merge the PR" → gap (merge = not a code predicate; merge ≠ implement/build)
"Code review and fix the security bug" → SECURITY fires; code review = gap warning
"Code review and refactor the duplicated logic" → IMPLEMENT fires; code review = gap warning
Rule: code review (with or without person) = gap when standalone; code action following = predicate fires.
