# Layer 0-A — Compiler Pairs (032)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-032-001 — "show how to" vs "show [what exists]" disambiguation

A: "Show the team how to set up the observability stack."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "show how to [infinitive]" = "teach to X" parallel pattern → gap. The infinitive "to set up" is the learning content for others.

---

## B34-compiler-032-002 — "show how to" + later code action

A: "Show how to configure the service mesh and then configure it for our three primary clusters."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "show how to configure" = gap warning (teaching frame); "configure it for our three primary clusters" = requester's direct configuration action → IMPLEMENT fires.

---

## B34-compiler-032-003 — "demonstrate" + SECURITY embedded

A: "Demonstrate the threat modeling process and secure the API endpoints against injection attacks."
B: Predicates: ["SECURITY"] | TransitionType: documentation
Evidence: "demonstrate the threat modeling process" = gap warning (gap-skills-guidance-train-001); "secure the API endpoints against injection attacks" = requester's direct security action → SECURITY fires.

---

## B34-compiler-032-004 — "demonstrate" + TEST embedded

A: "Demonstrate TDD (Test-Driven Development) methodology and write the first three test cycles for the order service."
B: Predicates: ["TEST"] | TransitionType: documentation
Evidence: "demonstrate TDD methodology" = gap warning (gap-skills-guidance-train-001); "write the first three test cycles" = requester's direct test-writing action → TEST fires.

---

## B34-compiler-032-005 — guidance boundary complete: show/demonstrate/present/introduce routing

| Guidance verb + context | Predicate result | Gap ID |
|------------------------|-----------------|--------|
| "show how to [verb]" (learning frame) | [] | gap-skills-guidance-train-001 |
| "show and then [code verb]" | [code predicate] + gap warning | gap-skills-guidance-train-001 |
| "demonstrate [topic]" | [] | gap-skills-guidance-train-001 |
| "demonstrate and [code verb]" | [code predicate] + gap warning | gap-skills-guidance-train-001 |
| "present [topic] to [audience]" | [] | gap-skills-guidance-train-001 |
| "introduce [person] to [topic]" | [] | gap-skills-guidance-train-001 or onboard-001 |
| "introduce [feature] into [system]" | ["IMPLEMENT"] | — (no gap) |
