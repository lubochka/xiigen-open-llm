# Layer 0-C — Knowledge Edge Pairs (032)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-032-001 — "show how to integrate" = gap (parallel to "teach to integrate")

"show how to integrate third-party systems" → gap (show how to = teaching frame; infinitive "to integrate" = learning target).
"demonstrate how to integrate" → gap (same rule; demonstrate = teaching frame).
"teach to integrate" → gap (established rule).
"show how to integrate" → gap (same exception applies to "show how to" as to "teach to").

These patterns are parallel: [teaching verb] + "how/to" + infinitive = gap. The infinitive is always the learning target.

---

## KE-032-002 — "review" in "code review" vs audit context

"code review the PR (Pull Request)" → gap (code review as mentoring/guidance activity).
"code review and approve/merge" → still gap (approval is not a code-action predicate).
"code review and fix security bugs" → SECURITY fires; code review = gap warning.
"perform security code review" → SECURITY fires (review = audit action with security focus).
"audit the codebase for security issues" → SECURITY fires (audit = security action).

---

## KE-032-003 — "present" edge: round-trip to gap even for implementation topics

"present the implementation plan" → gap.
"present the test results" → gap.
"present the security audit findings" → gap.
"present the architecture diagram" → gap.
"present and [code verb]" → code fires; present = gap warning.
All present-only scenarios = gap regardless of topic.

---

## KE-032-004 — guidance vocabulary additions: Round 2 expanded list

Updated gap vocabulary (all → gap):
mentor, coach, teach, train (human), guide, explain, onboard, shadow, upskill, advise, facilitate, pair-with, walk-through, review-with, workshop, ramp-up, demonstrate, show (guidance sense), introduce [person], brief [team] on, familiarize with, run through (explanation sense), present to, discuss.

All share: DomainId: software-development; GapHandler invoked when predicate set = [].

---

## KE-032-005 — SCHEDULE_CAPABILITY_LEARNING emission rule (Round 2 restatement)

Emitted: exactly once, when predicate set = [] and GapHandler invoked.
NOT emitted when: any code predicate fires (even with gap warnings alongside).
NOT emitted when: GapHandler is suppressed.
NOT emitted: multiple times for multiple guidance verbs.
Payload: {gap_id: "[most specific]", domainId: "software-development", request: "[original]"}.
