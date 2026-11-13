# Layer 3 — Teaching Unit Pairs (012)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-012 — Full verb priority hierarchy for B33

**Teaching goal**: Complete ordered priority list of how action verbs map to predicates in AI context.

**Verb priority hierarchy** (highest fires first; lower verbs in same request also fire if present):

| Priority | Verb(s) | Predicate | Notes |
|----------|---------|-----------|-------|
| 1 | design, architect, plan, spec (+ structural noun) | ARCHITECTURE | requires structural noun in object |
| 2 | integrate, connect, wire, interface with | API_CONTRACT | always fires; no object restriction |
| 3 | build, implement, create, add (code artifact), write (code), refactor | IMPLEMENT | code-action verb; overrides AI objects |
| 4 | secure, protect, harden, guard, prevent (security context) | SECURITY | security vocabulary in intent |
| 5 | authorize, restrict access, add RBAC, add permissions | AUTHORIZATION | access-control vocabulary |
| 6 | test (write context), write tests, create tests, assert | TEST | test-writing verbs; not run/execute |
| 7 | document, write docs, spec | DOCUMENT | documentation verbs |
| 8 | QA (abbreviation only) | QA | "quality assurance" spelled out → gap |
| gap | adapt, fine-tune, orchestrate, train (ML), teach, enable (AI behavior), ground, embed (capability), evaluate (AI quality) | [] | AI vocabulary; no predicate |

**Exception**: "prompt injection" (contains "injection") → SECURITY regardless of surrounding verbs.
