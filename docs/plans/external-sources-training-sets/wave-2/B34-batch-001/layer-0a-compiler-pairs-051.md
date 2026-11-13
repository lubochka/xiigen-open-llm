# Layer 0-A — Compiler Pairs (051)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-051-001 — guidance + SECURITY topic disambiguation

A: "Brief the team on OWASP (Open Web Application Security Project) Top 10 vulnerabilities."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "brief on OWASP" = guidance vocabulary → gap. Security topic in briefing frame does not fire SECURITY.

---

## B34-compiler-051-002 — guidance + security action = SECURITY fires

A: "Brief the team on the SQL injection risks and patch the vulnerable query endpoints."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "brief on SQL injection risks" = gap warning; "patch the vulnerable query endpoints" = requester's SECURITY action → SECURITY fires.

---

## B34-compiler-051-003 — "explain and build" = IMPLEMENT fires (complete reference)

"Explain and build" compound rule:
- "Explain [X] and build [Y]" → Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001
- "Explain [X] and document [Y]" → Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-train-001
- "Explain [X] and test [Y]" → Predicates: ["TEST"] | GapWarning: gap-skills-guidance-train-001
- "Explain [X] and secure [Y]" → Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-train-001
In all cases: explain = gap warning, code action fires.

---

## B34-compiler-051-004 — purpose clause with guidance frame + code action

A: "To help the team understand the system, explain the service dependencies and write the dependency map documentation."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-train-001
Rule: "explain service dependencies" = gap warning; "write the dependency map documentation" = requester's DOCUMENT action → DOCUMENT fires.

---

## B34-compiler-051-005 — guidance purpose clause alone = gap

A: "To help the team grow in confidence, walk them through the deployment process from scratch."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "to help team grow" = purpose clause (gap warning context); "walk them through deployment" = guidance vocabulary → gap. No requester code action.
