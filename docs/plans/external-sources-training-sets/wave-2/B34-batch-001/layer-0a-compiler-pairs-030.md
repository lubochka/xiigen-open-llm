# Layer 0-A — Compiler Pairs (030)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-030-001 — "review with" + gap vs "review code for security" → SECURITY

A: "Review the payment service code for security vulnerabilities."
B: Predicates: ["SECURITY"] | TransitionType: documentation
Evidence: "review code for security vulnerabilities" = security audit action by requester → SECURITY fires. No guidance vocabulary framing present.

---

## B34-compiler-030-002 — "review with team" vs "review code for security" contrast

A: "Review the payment service security posture with the team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "review with [person]" = collaborative guidance vocabulary → gap. "security posture" = knowledge topic for discussion, not a direct security action predicate.

---

## B34-compiler-030-003 — "review" verb disambiguation rules

Review = gap when: "review with [person]", "review [topic] together", "review and explain", learning/teaching intent.
Review = SECURITY when: "review for vulnerabilities", "review security", "review the code [for issues to fix]".
Review = TEST when: "review code for test coverage", "code review targeting test completeness".
Review = DOCUMENT when: "review and document", "review the documentation".

---

## B34-compiler-030-004 — "present" + guidance boundary complete reference

"present to [audience]" → always gap (teaching/communication → gap-skills-guidance-train-001).
"present and then [code verb]" → gap warning + code predicate fires.
"present the architecture" → gap (present = teaching frame; architecture = topic).
"design and present the architecture" → ARCHITECTURE fires (design = code verb); "present" = gap warning.

---

## B34-compiler-030-005 — complete "discuss" boundary

"discuss [technical topic]" → gap (discussion = knowledge-sharing → gap-skills-guidance-mentor-001).
"discuss and implement [feature]" → IMPLEMENT fires; "discuss" = gap warning.
"discuss security practices" → gap (not SECURITY).
"discuss test strategy and write tests" → TEST fires; "discuss" = gap warning.
