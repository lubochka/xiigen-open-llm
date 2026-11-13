# Layer 0-C — Knowledge Edge Pairs (053)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-053-001 — "consult, seek advice, get feedback" combined = single gap

"Consult with the security team, seek advice on the encryption approach, and get feedback on the implementation plan."
→ Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: three advisory vocabulary items in one request = single gap. SCHEDULE_CAPABILITY_LEARNING emitted once.

---

## KE-053-002 — "sanity check, second opinion, rubber duck" combined = single gap

"Sanity check the approach, get a second opinion from the architect, and rubber duck the edge cases."
→ Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: three advisory vocabulary items = single gap. No multiplication of gaps or SCHEDULE_CAPABILITY_LEARNING.

---

## KE-053-003 — "sounding board, consult, seek advice" + single code action

"Use the team as a sounding board, consult the architect, seek advice from the guild — then implement the recommended solution."
→ Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: three advisory vocabulary items = single gap warning; one code action = IMPLEMENT fires once. Gap warning emitted once regardless of advisory verb count.

---

## KE-053-004 — Round 1+2+3 vocabulary all in one request = single gap

"Mentor, coach, explain, workshop, show how to, demonstrate, familiarize with, consult on, rubber duck — all of these describe ways to support the team."
→ Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: listing all guidance vocabulary = single gap. SCHEDULE_CAPABILITY_LEARNING emitted once. No predicate fires for any guidance vocabulary item.

---

## KE-053-005 — "DomainId: software-development" is not derived from topic

DomainId is NOT derived from the specific technical topic mentioned (security, architecture, testing, etc.).
DomainId is fixed at software-development for ALL B34 guidance/skills gap events.
Correct: "Explain the OWASP Top 10 to the team" → DomainId: software-development (not: DomainId: security)
Correct: "Mentor on Kubernetes" → DomainId: software-development (not: DomainId: DevOps/infrastructure)
Correct: "Train on TDD" → DomainId: software-development (not: DomainId: testing)
All B34 gaps: DomainId = software-development. This is a hardcoded invariant, not derived.
