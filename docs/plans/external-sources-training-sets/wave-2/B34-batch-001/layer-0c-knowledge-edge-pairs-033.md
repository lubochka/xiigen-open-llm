# Layer 0-C — Knowledge Edge Pairs (033)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-033-001 — "show how to" infinitive = gap (all code verbs)

"Show how to build" → gap (not IMPLEMENT)
"Show how to test" → gap (not TEST)
"Show how to integrate" → gap (not API_CONTRACT)
"Show how to architect" → gap (not ARCHITECTURE)
"Show how to secure" → gap (not SECURITY)
"Show how to document" → gap (not DOCUMENT)
The "show how to" + infinitive pattern = full exception. Code verb blocked in ALL cases.

---

## KE-033-002 — "demonstrate how to" = same exception as "show how to"

"Demonstrate how to build" → gap
"Demonstrate how to integrate" → gap (not API_CONTRACT; integrate exception blocked)
"Demonstrate how the system works" → gap
"Demonstrate [feature/behavior] to [person]" → gap
Exception: "demonstrate and then [code verb]" → code verb fires, demonstrate = gap warning.

---

## KE-033-003 — "introduce [person]" vs "introduce [feature]": knowledge edge boundary

Knowledge edge: the subject's type determines the predicate outcome.
"Introduce [person/team/role] to [X]" → gap
"Introduce [feature/module/component] into [Y]" → IMPLEMENT
"Introduce [concept/pattern/approach] to [person]" → gap
"Introduce [library/dependency] into [codebase]" → IMPLEMENT
Edge case: "Introduce the team to the new module and introduce the module into the codebase" → gap warning + IMPLEMENT.

---

## KE-033-004 — "brief" knowledge edge: verb vs adjective vs noun

"Brief the team on [topic]" (verb: give briefing) → gap
"Write a brief for [stakeholder]" (noun: short document) → DOCUMENT
"A brief technical overview" (adjective: short) → no vocabulary trigger
"Brief as in short" (adjective usage) → no gap vocabulary
Edge: "Write a brief to introduce the team to the architecture" → DOCUMENT fires; "introduce to architecture" = gap warning.

---

## KE-033-005 — "familiarize" knowledge edge: scope boundary

"Familiarize [person] with [tool/process/system]" → gap-skills-guidance-onboard-001
"Familiarize yourself with [X]" (self-instruction) → outside B34 scope (requester is the learner, not the teacher)
"Familiarize [person] with [X] and configure [X]" → IMPLEMENT or appropriate code predicate fires; familiarize = gap warning
"Familiarize with [X] and then integrate [X]" → API_CONTRACT fires; familiarize = gap warning
