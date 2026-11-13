# Held-Out and Corruption Pairs
# Batch: B34-batch-001
# Scenario: B34 — Skills and guidance gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## HO-001 — "write" + tutorial context: DOCUMENT fires; tutorial is gap

A: "Write a tutorial covering the API authentication flow for new engineers."
Expected: DOCUMENT fires on "write". "tutorial", "authentication flow", "new engineers" → gap.
CompiledPredicateIds: ["DOCUMENT"]. Routing: 6.2 (documentation).
CapabilityGapWarnings: gap-skills-guidance-onboard-001 (tutorial), gap-skills-guidance-mentor-001 (new engineers).

---

## HO-002 — pure onboarding: knowledge_edge

A: "Onboard the new hire to the codebase and walk them through the CI/CD pipeline."
Expected: zero predicates → knowledge_edge. "onboard", "walk through", "new hire" → gap.
"CI/CD pipeline" — no predicate (CI/CD is gap, pipeline alone no predicate).
Gap ID: gap-skills-guidance-onboard-001. DomainId: "software-development".

---

## HO-003 — "build" + "training" program: IMPLEMENT fires; training is gap

A: "Build a developer training program and track skill progression across the team."
Expected: IMPLEMENT fires on "build". "training program", "skill progression", "track" → gap.
CompiledPredicateIds: ["IMPLEMENT"]. Routing: 4.1.
CapabilityGapWarnings: gap-skills-guidance-train-001.

---

## COR-001 — v6-01: DOCUMENT invented for "guide the team through documentation"

CORRUPT: "Guide the team through the documentation process." → CompiledPredicateIds: ["DOCUMENT"].
Why corrupt: "guide" is gap vocabulary. "documentation process" contains "documentation"
as a noun, but the action verb is "guide" which has no predicate. DOCUMENT requires an
action verb: document, write, write up, specify. Correct: CompiledPredicateIds: [] → knowledge_edge.

---

## COR-002 — v6-01: SECURITY invented for "teach security practices"

CORRUPT: "Teach the team about security best practices." → CompiledPredicateIds: ["SECURITY"].
Why corrupt: "teach" is gap vocabulary. SECURITY fires on security-action verbs
(implement/enforce/audit/configure security), not on security-topic teaching.
"About security" is the topic of teaching, not a security action. Correct: CompiledPredicateIds: [] → knowledge_edge.

---

## COR-003 — v6-03: DomainId "education" for skills/guidance gap

CORRUPT: Pure guidance zero-predicate request → DomainId: "education".
Why corrupt: There is no registered "education" domain in the gap taxonomy. The two
valid gap DomainIds are "software-development" and "project-management". Skills and
guidance vocabulary in a software engineering context emits DomainId: "software-development".
The activity being described (mentoring engineers, onboarding developers) is in the
software-development domain — the gap is only in the human-learning vocabulary, not in domain.
