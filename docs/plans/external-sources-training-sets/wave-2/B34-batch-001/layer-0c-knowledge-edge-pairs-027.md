# Layer 0-C — Knowledge Edge Pairs (027)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-027-001 — "review" disambiguation: code review gap vs security review

"code review" with teaching/mentoring intent → gap-skills-guidance-mentor-001.
"code review" for bugs/issues (action focus) → depends on embedded action: "review and fix security issues" → SECURITY.
"review code for test coverage gaps" → TEST.
"review and document findings" → DOCUMENT.

Key: "code review" alone without action intent → gap. "code review + [action verb]" → action predicate fires + gap warning.

---

## KE-027-002 — guidance + UI embedded: embedded verb rule applied

"walk the frontend team through the component library" → gap (walk-through = gap; library = existing artifact).
"walk the frontend team through implementing the component library" → IMPLEMENT fires (implementing = participial code action by requester).
"show the frontend team how to use the component library" → gap (show how to = teaching frame).
"show the frontend team and build the component library" → IMPLEMENT fires; "show" = gap warning.

---

## KE-027-003 — guidance + ARCHITECTURE embedded: embedded verb rule

"present the architecture" → gap (present = teaching; architecture = topic).
"design the architecture" → ARCHITECTURE fires (design = code action).
"present and design the architecture" → ARCHITECTURE fires; "present" = gap warning.
"discuss the architecture" → gap (discuss = dialogue).
"discuss and design the architecture" → ARCHITECTURE fires; "discuss" = gap warning.

---

## KE-027-004 — "brief on" complete edge cases

"brief on security" → gap (not SECURITY).
"brief on architecture" → gap (not ARCHITECTURE).
"brief on tests" → gap (not TEST).
"brief on API conventions" → gap (not API_CONTRACT).
"brief and harden" → SECURITY fires; "brief" = gap warning.
"brief and design" → ARCHITECTURE fires; "brief" = gap warning.

The topic noun after "brief on" NEVER activates a predicate. Only "and [code verb]" clauses fire predicates.

---

## KE-027-005 — "familiarize" + embedded code verb edge case

"familiarize with the codebase and write the setup documentation" → DOCUMENT fires; "familiarize" = gap warning (onboard-001).
"familiarize with the API and integrate the service" → API_CONTRACT fires (integrate always fires); "familiarize" = gap warning.
"familiarize with testing patterns and write tests" → TEST fires; "familiarize" = gap warning.

Rule: "familiarize" never blocks embedded code verbs. Embedded verb rule applies normally.
