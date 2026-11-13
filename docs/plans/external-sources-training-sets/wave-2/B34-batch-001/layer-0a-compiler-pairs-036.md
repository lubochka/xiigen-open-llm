# Layer 0-A — Compiler Pairs (036)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-036-001 — complete guidance boundary reference: Round 2 summary

All Round 2 guidance vocabulary (025–036) routes via same rules as Round 1:
1. Pure guidance verbs → gap (GapHandler + SCHEDULE_CAPABILITY_LEARNING).
2. Guidance frame + requester code action → gap warning + code predicate fires.
3. "introduce [person] to" = gap; "introduce [feature] into" = IMPLEMENT.
4. Multiple guidance verbs with no code actions → single gap, single SCHEDULE_CAPABILITY_LEARNING.

---

## B34-compiler-036-002 — "demonstrate" vocabulary: no exceptions

A: "Demonstrate the observability toolchain to the platform team."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: demonstrate = always gap. Demonstrating to others = teaching act regardless of topic.

---

## B34-compiler-036-003 — "review with" disambiguation complete reference

"review [X] with [person]" → gap (collaborative teaching → gap-skills-guidance-mentor-001).
"review [X] for bugs/vulnerabilities" → SECURITY or TEST (depending on focus).
"review and document [X]" → DOCUMENT fires; "review" = gap warning.
"review [X] with [person] and fix" → code predicate fires; "review with" = gap warning.

---

## B34-compiler-036-004 — "run through" + "review with" compound

A: "Run through the security checklist with the ops team and patch the critical vulnerabilities."
B: Predicates: ["SECURITY"] | TransitionType: documentation
Evidence: "run through with the ops team" = gap warning (gap-skills-guidance-mentor-001); "patch the critical vulnerabilities" = requester's direct security action → SECURITY fires.

---

## B34-compiler-036-005 — v6 corruption: "brief" → invented BRIEFING predicate

v6 corrupt: "Brief the team on the new architecture" → Predicates: ["BRIEFING"] or ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-01 (invented predicate) or v6-02 (gap suppression). Brief = guidance vocabulary → gap only. No "BRIEFING" predicate exists; architecture is the topic not the action.
