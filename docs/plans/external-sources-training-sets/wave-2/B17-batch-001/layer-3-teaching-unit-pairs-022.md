# Layer 3 — Teaching Unit Pairs (022)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Avalara tax connector

---

## B17-TU-022-001 — Tax domain vocabulary fires no predicates

**A:** "Build the Avalara tax connector and implement the sales tax exemption and nexus calculation." Do "tax", "exemption", or "nexus" fire predicates?

**B:** No. Tax-specific vocabulary (tax, sales tax, VAT, exemption, nexus, jurisdiction, rate, liability) fires no predicates. Predicates fire from:
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]

The tax domain context is handled by the nine-node flow as domain content. Predicates are structural routing signals, not domain labels.

---

## B17-TU-022-002 — "ERP" acronym is transparent to the predicate system

**A:** "Write the Avalara tax connector for the ERP system team." Does "ERP" fire any predicate?

**B:** No. "ERP" (Enterprise Resource Planning) is a product category acronym — transparent to the predicate system. No predicate fires from "ERP". The predicates fire from "Write" (IMPLEMENT) and "connector" (API_CONTRACT).

**Note:** Neither "team" nor "ERP" nor "system" supplies integration vocabulary. "connector" is the integration vocabulary token here.

---

## B17-TU-022-003 — "void transaction" is not an IMPLEMENT trigger

**A:** "Build the Avalara connector and void transaction." Does "void" fire IMPLEMENT?

**B:** No. "Void" in "void transaction" is an accounting operation term, not a build verb. It does not fire IMPLEMENT.

**IMPLEMENT triggers (complete list):** build, implement, write, create, code, develop, refactor, restructure, rewrite.
"Void" is not in this list.

**The predicates that fire:**
- "Build" → IMPLEMENT (from "Build the Avalara connector and void transaction")
- "connector" → API_CONTRACT

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]

"Void transaction" is domain content handled by the connector, not a predicate trigger.

---

## B17-TU-022-004 — Avalara basic auth vs OAuth token: which fires SECURITY?

**A:** Avalara supports both Basic auth and OAuth bearer token. If the request says "Implement the Avalara connector with bearer token authentication", does SECURITY fire?

**B:** Yes — from two triggers:
1. "token" → SECURITY (Rule 6: "bearer token" fires SECURITY)
2. "authentication" → SECURITY (Rule 8: reinforces; fires once)

If the request instead says "Implement the Avalara connector with Basic auth" (no "token", no "OAuth"):
- "Basic auth" — no predicate
- No SECURITY trigger
- Result: ["IMPLEMENT", "API_CONTRACT"] only

**Teaching point:** The type of authentication mechanism only affects predicates through the specific trigger words. "Bearer token" → SECURITY (from "token"). "Basic auth" → no SECURITY predicate (no trigger word). "OAuth" → AUTHORIZATION (from "OAuth"). The authentication mechanism itself (Basic, Bearer, OAuth) does not trigger predicates — only the specific words do.
