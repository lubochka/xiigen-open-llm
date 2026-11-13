# Layer 0-A — Compiler Pairs (007)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-007-001 — "feedback" on code → gap

A: "Give the team feedback on their code quality."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "give feedback" = mentoring vocabulary; "code quality" = quality concept, not SECURITY/QA predicate trigger.

---

## B34-compiler-007-002 — "write tests" inside guidance frame → TEST

A: "Coach the team and write integration tests for the payment module."
B: Predicates: ["TEST"] | TransitionType: documentation (no IMPLEMENT)
Evidence: "coach" → gap; "write integration tests" → TEST (test-writing verb embedded in guidance request). TEST fires.
Key rule: Embedded action verbs inside guidance frames fire their predicates.

---

## B34-compiler-007-003 — "explain security" → gap (not SECURITY)

A: "Explain how our authentication system works to the new developers."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "explain" = teaching vocabulary; "authentication system" does not fire SECURITY without security-implementing action.
Key rule: Explaining a security concept ≠ SECURITY predicate. SECURITY requires: audit/harden/secure/protect action verbs.

---

## B34-compiler-007-004 — "review code" mentor → gap

A: "Review the junior developer's code and give guidance."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "review code" = mentoring activity; "give guidance" = guidance vocabulary → gap.

---

## B34-compiler-007-005 — "document" + "mentor" → DOCUMENT fires, gap for mentor

A: "Mentor the team on testing and document the test strategy."
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "mentor" → gap; "document the test strategy" → DOCUMENT. One predicate fires; GapHandler not invoked.
