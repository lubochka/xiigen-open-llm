# Layer 3 — Teaching Unit Pairs (027)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Plaid Banking connector

---

## B17-TU-027-001 — Plaid: OAuth fires AUTHORIZATION; "link token" fires SECURITY

**A:** "Build the Plaid connector with OAuth token exchange for link token creation." What fires?

**B:** IMPLEMENT (Build), API_CONTRACT (connector), AUTHORIZATION (OAuth), SECURITY (token — from "OAuth token" and "link token"). CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]. DecisionCount: 4, UniqueSubPhases: 3 (4.3, 3.3, 3.4).

---

## B17-TU-027-002 — Plaid: "link" alone does not fire

**A:** "Connect the Plaid link to the banking application." Does IMPLEMENT fire?

**B:** No. "Connect" alone (without a co-occurring build verb) does not fire IMPLEMENT. "Link" is a Plaid domain noun. No predicates fire. CompiledPredicateIds: [].

---

## B17-TU-027-003 — Plaid: "integrate" rule

**A:** "Integrate the Plaid API to retrieve bank account balances." What fires?

**B:** API_CONTRACT only. Rule 1: "integrate" → ["API_CONTRACT"] → 3.3 → documentation. "retrieve bank account balances" is a domain phrase — no predicate. DecisionCount: 1.

---

## B17-TU-027-004 — Plaid: SECURITY + AUTHORIZATION at same sub-phase

**A:** Plaid fires SECURITY and AUTHORIZATION. Do they produce separate flows or one combined flow?

**B:** Separate flows at the same sub-phase. SecurityAuditFlow34(3.4) and AuthorizationFlow34(3.4) are two distinct flows. They share the sub-phase 3.4 destination but are separate orchestration entries. DecisionCount: 4 (counts predicates), UniqueSubPhases: 3 (counts distinct sub-phases).
