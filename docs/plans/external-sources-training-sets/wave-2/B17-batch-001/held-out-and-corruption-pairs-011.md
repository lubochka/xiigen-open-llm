# Held-Out and Corruption Pairs (011)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-011-001 — CORRUPTION: v6-01 (INTEGRATION predicate invented)

**Input:**
"Build the PayPal connector and integrate the payment capture API."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "INTEGRATION"]
(Error: "INTEGRATION" predicate invented — this predicate does not exist in B17)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**Error type:** v6-01 — predicate invention. "integrate" fires API_CONTRACT (already fired from "connector"). No new predicate "INTEGRATION" is generated. The valid B17 predicates are: IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA. "INTEGRATION" is not valid.

**Detection:** Any predicate name not in the valid set {IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA} is a v6-01 invented predicate.

---

## B17-HO-011-002 — CORRUPTION: v6-02 (SECURITY suppressed for "token")

**Input:**
"Build the Auth0 connector with OAuth token refresh and secure token storage."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
(Error: "token" and "secure" present but SECURITY not fired; suppressed)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**Error type:** v6-02 — gap suppression. "token" → SECURITY (Rule 6) and "secure" → SECURITY (Rule 8). Both are SECURITY triggers. SECURITY must fire. AUTHORIZATION also fires from "OAuth".

**DecisionCount:** 3 (corrupted) vs 4 (correct). SecurityAuditFlow34 omitted in corrupted version.

---

## B17-HO-011-003 — CORRUPTION: v6-03 (IMPLEMENT → 4.1 instead of 4.3)

**Input:**
"Implement the Salesforce CRM connector for lead management."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.1 (IMPLEMENT), 3.3 (API_CONTRACT)
(Error: IMPLEMENT routes to 4.1 instead of 4.3)

**CORRECT output:**
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT)

**Error type:** v6-03. "connector" is integration vocabulary. IMPLEMENT + "connector" → 4.3, always. The "CRM connector" phrasing contains "connector" which is sufficient to override the 4.1 default.

---

## B17-HO-011-004 — All three v6 corruptions in summary

| Error | Input trigger | Wrong output | Correct output |
|---|---|---|---|
| v6-01 (predicate invention) | "integrate" in input | IMPLEMENT invented OR INTEGRATION invented | API_CONTRACT only |
| v6-02 (gap suppression) | "token" in input | SECURITY absent | SECURITY must be present |
| v6-03 (wrong sub-phase) | "connector" + IMPLEMENT | 4.1 for IMPLEMENT | 4.3 for IMPLEMENT |

**All three can co-occur in a single corrupted output.** For example, an output might invent IMPLEMENT (v6-01), suppress SECURITY (v6-02), and route the invented IMPLEMENT to 4.1 (v6-03) — three corruptions in one response.
