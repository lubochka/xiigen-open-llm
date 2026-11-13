# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-006-001 — TransitionType summary for B34

| Predicates | TransitionType |
|-----------|----------------|
| [] (gap) | none |
| [DOCUMENT] | documentation |
| [ARCHITECTURE] | documentation |
| [TEST] | documentation |
| [QA] | no contribution |
| [IMPLEMENT] | feature_addition |
| [IMPLEMENT + any] | feature_addition |
| refactor verb + [IMPLEMENT] | refactoring |

No B34-specific override. Hierarchy unchanged from global rules.

---

## TO-006-002 — "explain architecture" vs "architect"

"Explain the architecture to the team" → gap (explain = teaching verb)
"Architect the new platform" → ARCHITECTURE (architect = design verb)
"Explain and architect the platform" → ["ARCHITECTURE"] (architect fires; explain → gap suppressed)

---

## TO-006-003 — Six-predicate orchestration

Request: "Design the LMS architecture, integrate with SSO, build the course module, secure auth, write tests, and document the API."
Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT"]
Orchestration: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestingFlow ‖ DocumentationFlow
TransitionType: feature_addition

---

## TO-006-004 — Ordering summary for B34

Canonical ordering rules:
1. ARCHITECTURE first
2. API_CONTRACT before IMPLEMENT
3. IMPLEMENT before TEST
4. SECURITY after IMPLEMENT
5. QA always last (absolute)
DOCUMENT: parallel-eligible with TEST after IMPLEMENT completes.

---

## TO-006-005 — Gap: educational institution vocabulary

Request: "Prepare the new engineering graduates for production-level work."
Predicates: [] | GapHandler → gap-skills-guidance-onboard-001
Evidence: "prepare graduates" = onboarding/mentoring vocabulary → gap.
