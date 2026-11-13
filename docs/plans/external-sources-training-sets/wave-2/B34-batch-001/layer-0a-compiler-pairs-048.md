# Layer 0-A — Compiler Pairs (048)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-048-001 — v6 corruption: "show how to" → invented TUTORIAL predicate

v6 corrupt: "Show the team how to deploy the service" → Predicates: ["TUTORIAL"] or ["DEPLOY"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-01 (invented predicate) or v6-02 (gap suppression). "Show how to" = teaching frame → gap only.

---

## B34-compiler-048-002 — v6 corruption: "consult on" → ARCHITECTURE fires incorrectly

v6 corrupt: "Consult with the team on the architecture" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02 (gap suppression). Consult = advisory vocabulary → gap. Architecture is discussion topic only.

---

## B34-compiler-048-003 — v6 corruption: "get feedback on security" → SECURITY fires incorrectly

v6 corrupt: "Get feedback on the security posture" → Predicates: ["SECURITY"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02 (gap suppression). "Get feedback on" = advisory framing → gap. Security topic without direct action = gap only.

---

## B34-compiler-048-004 — v6 corruption: "rubber duck" + "implement" loses gap warning

v6 corrupt: "Rubber duck the design, then implement the service" → Predicates: ["IMPLEMENT"] (no gap warning)
Correct: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Error type: v6-03 (gap warning lost). Advisory vocabulary alongside code action = gap warning must be emitted.

---

## B34-compiler-048-005 — v6 corruption: "introduce [person] to" → IMPLEMENT fires

v6 corrupt: "Introduce the team to the new framework" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001 or onboard-001
Error type: v6-02 (gap suppression). "Introduce [person] to [topic]" = onboarding/teaching → gap, not IMPLEMENT.
