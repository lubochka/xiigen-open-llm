# Layer 0-A — Compiler Pairs (017)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-017-001 — "advise" on best practices → gap

A: "Advise the team on best practices for RESTful (Representational State Transfer) API design."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "advise" = advisory/guidance vocabulary → gap. "RESTful API design" = advisory topic, not design action.

---

## B34-compiler-017-002 — "advise" + embedded ARCHITECTURE

A: "Advise and then design the high-availability architecture for the new platform."
Analysis: "advise" = gap frame; "design the architecture" = direct requester action → ARCHITECTURE fires.
B: Predicates: ["ARCHITECTURE"] | TransitionType: documentation
Evidence: "design" + "architecture" → ARCHITECTURE; "advise" = gap warning; embedded verb rule: ARCHITECTURE fires.

---

## B34-compiler-017-003 — "advise" on security → gap (not SECURITY)

A: "Advise the junior engineers on how to write secure code."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "advise on how to write secure code" = advisory vocabulary → gap. Teaching about security ≠ securing a system. SECURITY fires on "secure/harden/protect [system]", not on advisory about security.

---

## B34-compiler-017-004 — "advise" + embedded SECURITY verb

A: "Advise on approach, then harden the API gateway against injection attacks."
Analysis: "advise" = gap frame; "harden the API gateway" = direct action → SECURITY fires.
B: Predicates: ["SECURITY"] | TransitionType: documentation
Evidence: "harden" + "API gateway" → SECURITY; "advise" = gap warning alongside SECURITY.

---

## B34-compiler-017-005 — v6 corruption: "advise on QA" incorrectly fires QA

v6 corrupt: "Advise the team on quality assurance processes" → Predicates: ["QA"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "advise on quality assurance" = advisory vocabulary → gap. "quality assurance" spelled out → gap; only "QA" abbreviation fires. Double gap suppression: advise = gap, quality assurance = gap.
Fix: Predicate = [].
