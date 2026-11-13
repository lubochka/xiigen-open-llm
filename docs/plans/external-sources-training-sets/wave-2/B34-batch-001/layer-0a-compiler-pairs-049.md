# Layer 0-A — Compiler Pairs (049)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-049-001 — "show how to" + "demonstrate" stacked = single gap

A: "Show the interns how to write clean code and demonstrate the refactoring techniques."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: "show how to" + demonstrate = both guidance vocabulary → single gap. SCHEDULE_CAPABILITY_LEARNING emitted once only.

---

## B34-compiler-049-002 — "present" + "introduce" stacked = single gap

A: "Present the onboarding materials and introduce new hires to the engineering culture."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Rule: present + introduce [person] = both guidance vocabulary → gap-skills-guidance-onboard-001 (most specific for onboarding context).

---

## B34-compiler-049-003 — "brief" + "run through" stacked = single gap

A: "Brief the platform team on the SLA (Service Level Agreement) changes and run through the new monitoring setup."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: brief + run through = both guidance vocabulary → gap-skills-guidance-mentor-001. Single gap emitted.

---

## B34-compiler-049-004 — stacked advisory verbs = single gap

A: "Get a second opinion on the caching strategy and sanity check the eviction policy."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: second opinion + sanity check = both advisory vocabulary → gap-skills-guidance-mentor-001. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B34-compiler-049-005 — mixed Round 1+2+3 vocabulary = single gap

A: "Mentor the team, demonstrate the patterns, consult on edge cases, and explain the deployment checklist."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: mentor + demonstrate + consult on + explain = all guidance vocabulary → single gap. GapHandler invoked once. SCHEDULE_CAPABILITY_LEARNING emitted once.
