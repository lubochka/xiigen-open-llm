# Layer 0-A — Compiler Pairs (044)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-044-001 — "rubber duck" = gap

A: "Rubber duck the logging strategy with a senior engineer."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "rubber duck [problem] with [person]" = thinking-aloud/advisory guidance → gap. No code action.

---

## B34-compiler-044-002 — "sounding board" = gap

A: "Use the architecture guild as a sounding board for the new service mesh design."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "sounding board for [design]" = advisory/review framing → gap. Architecture topic in this advisory frame does not fire ARCHITECTURE.

---

## B34-compiler-044-003 — "sounding board" + architecture action

A: "Use the team as a sounding board, then architect the final service mesh topology."
B: Predicates: ["ARCHITECTURE"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "use as sounding board" = gap warning; "architect the final topology" = requester's direct ARCHITECTURE action → ARCHITECTURE fires.

---

## B34-compiler-044-004 — "rubber duck" + implement action

A: "Rubber duck the retry logic design, then implement the exponential backoff in the HTTP (Hypertext Transfer Protocol) client."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "rubber duck design" = gap warning; "implement exponential backoff" = requester's IMPLEMENT action → IMPLEMENT fires.

---

## B34-compiler-044-005 — "consult on" + security action

A: "Consult on the threat model and harden the authorization middleware."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "consult on threat model" = gap warning; "harden the authorization middleware" = requester's SECURITY action → SECURITY fires.
