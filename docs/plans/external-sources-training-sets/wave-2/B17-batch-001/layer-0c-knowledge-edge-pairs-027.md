# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Plaid banking connector edge cases

---

## B17-0C-027-001 — Plaid: "link token" — "token" always fires SECURITY

Input: "Implement Plaid link token creation and bank account authentication."
- "link token" contains "token" → SECURITY fires (Rule 6; unconditional)
- "authentication" → SECURITY (reinforces; fires once)
- No "connector", "webhook", "API" etc. → IMPLEMENT → 4.1
- "Plaid" proper noun: no integration vocabulary

CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
This is the key edge: "link token" does NOT become integration vocabulary just because Plaid is named.

---

## B17-0C-027-002 — Plaid: "OAuth" vs "authorization" English word

| Input fragment | AUTHORIZATION fires? | Reason |
|---|---|---|
| "OAuth token exchange" | Yes | "OAuth" fires AUTHORIZATION (Rule 7) |
| "Plaid authorization flow" | No | English word "authorization" ≠ AUTHORIZATION trigger |
| "OAuth 2.0 code exchange" | Yes | "OAuth" present |
| "authorized access to bank data" | No | "authorized" is not "OAuth" |

---

## B17-0C-027-003 — Plaid: DecisionCount 4 with UniqueSubPhases 3

When SECURITY and AUTHORIZATION both fire:
- SECURITY → 3.4 (SecurityAuditFlow34)
- AUTHORIZATION → 3.4 (AuthorizationFlow34)
Both route to sub-phase 3.4. DecisionCount counts each predicate (4 total). UniqueSubPhases counts distinct destinations (3: 4.3, 3.3, 3.4).
This is Rule 9. DecisionCount is NEVER reduced to match UniqueSubPhases.
