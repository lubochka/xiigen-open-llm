# Layer 3 — Teaching Unit Pairs (028)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: DocuSign connector

---

## B17-TU-028-001 — DocuSign: OAuth + webhook + token combination

**A:** "Build the DocuSign connector with OAuth token refresh and Connect webhook HMAC validation." What fires?

**B:** IMPLEMENT (Build), API_CONTRACT (connector + webhook), AUTHORIZATION (OAuth), SECURITY (token + HMAC validation). CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]. DecisionCount: 4, UniqueSubPhases: 3.

---

## B17-TU-028-002 — DocuSign: "envelope" does not fire

**A:** "Process DocuSign envelopes and track signing status." Does any predicate fire?

**B:** No. "Envelope" and "signing status" are DocuSign domain nouns. "Process" and "track" are not IMPLEMENT triggers. CompiledPredicateIds: [].

---

## B17-TU-028-003 — DocuSign: "Connect" as product name vs IMPLEMENT trigger

**A:** "DocuSign Connect webhook service sends events." Does IMPLEMENT fire from "Connect"?

**B:** No. "DocuSign Connect" here is a product/service name, not the IMPLEMENT trigger. In isolation it is a proper noun. "Build a handler for DocuSign Connect webhook events" → IMPLEMENT fires from "Build"; "webhook" fires API_CONTRACT.

---

## B17-TU-028-004 — DocuSign: OAuth 2.0 auth code vs JWT auth

**A:** "Build the DocuSign connector using JWT (JSON Web Token) service integration authentication." Does AUTHORIZATION fire?

**B:** AUTHORIZATION does NOT fire from JWT alone. AUTHORIZATION fires only from "OAuth". However, SECURITY fires from "Token" in "JSON Web Token" expansion. CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]. To fire AUTHORIZATION, the input must contain "OAuth".
