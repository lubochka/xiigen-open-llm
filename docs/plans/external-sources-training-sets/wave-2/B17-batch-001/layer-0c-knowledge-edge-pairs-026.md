# Layer 0-C — Knowledge-Edge Pairs (026)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: SendGrid email connector edge cases

---

## B17-0C-026-001 — SendGrid: "email" is a domain noun, not integration vocabulary

| Input | IMPLEMENT routes to | Reason |
|---|---|---|
| "Build SendGrid email connector" | 4.3 | "connector" is integration vocabulary |
| "Build SendGrid email service" | 4.1 | "email" and "service" are not integration vocabulary |
| "Build SendGrid email endpoint client" | 4.3 | "endpoint" is integration vocabulary (external context) |
| "Build SendGrid transactional sender" | 4.1 | no integration vocabulary |

"email", "transactional", "delivery", "bounce" are domain nouns — no routing effect.

---

## B17-0C-026-002 — SendGrid: "API key token" — token fires SECURITY

Input: "Build the SendGrid connector using an API key token for authentication."
- "API key" is a compound noun → does "API" count as integration vocabulary?
- Yes: "API" when used in the context of an external service is integration vocabulary
- "token" fires SECURITY (Rule 6; unconditional — the phrase "API key token" does not suppress it)
- "authentication" also fires SECURITY (reinforces; fires once)

SECURITY fires on "token" regardless of adjacent modifiers ("API key", "bearer", "access", "project").

---

## B17-0C-026-003 — SendGrid: "refactor" TransitionType rule

Input: "Refactor the SendGrid connector to use the v3 Mail Send API."
- "Refactor" → IMPLEMENT (Rule 14)
- TransitionType = refactoring (overrides feature_addition)
- TransitionType hierarchy: refactoring > feature_addition > documentation

Even when refactoring introduces new endpoint logic, TransitionType remains "refactoring" because "refactor" is the primary verb.
