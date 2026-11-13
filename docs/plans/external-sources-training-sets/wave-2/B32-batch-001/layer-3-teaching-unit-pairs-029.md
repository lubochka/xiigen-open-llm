# Layer 3 — Teaching Unit Pairs (029)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-029-001 — Teaching: gap + API_CONTRACT

**Teaching unit:** gap vocabulary with API_CONTRACT predicate

API_CONTRACT fires from action verbs: "define the contract", "specify the API contract", "document the contract".

API_CONTRACT does NOT fire from the noun "API contract" alone.

**Gap co-occurrence rule:** Any gap qualifier alongside API_CONTRACT trigger verb → API_CONTRACT fires + gap warning.

---

## B32-L3-029-002 — Teaching: "document the contract" fires two predicates

**Teaching unit:** verb + noun combination triggering multiple predicates

"document the API contract" → two predicates:
1. DOCUMENT fires from the verb "document"
2. API_CONTRACT fires from the noun "contract" (as object of a relevant action)

Both predicates are independent. The gap qualifier adds a warning to both flows.

---

## B32-L3-029-003 — Teaching: IMPLEMENT overrides documentation TransitionType

**Teaching unit:** TransitionType precedence

When [API_CONTRACT, IMPLEMENT] are both present:
- API_CONTRACT alone → documentation
- IMPLEMENT alone → feature_addition
- Both together → feature_addition (IMPLEMENT/UI takes precedence)

This precedence rule applies to all predicate combinations where IMPLEMENT or UI is present.

---

## B32-L3-029-004 — Teaching: API_CONTRACT gap noun-only boundary

**Teaching unit:** avoiding false API_CONTRACT fires

| Expression | API_CONTRACT fires? | Reason |
|---|---|---|
| "define the contract" | YES | Action verb present |
| "specify the API" | YES | "specify" = action verb for contracts |
| "document the contract" | YES | "document" verb |
| "API contract" (noun) | NO | No action verb |
| "we need a contract" | NO | "need" not in API_CONTRACT lexicon |

---
