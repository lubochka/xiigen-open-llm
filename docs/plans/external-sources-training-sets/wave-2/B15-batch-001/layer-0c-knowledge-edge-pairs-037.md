# Layer 0-C — Knowledge-Edge Pairs (037)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-037-001 — CustomerProfileService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers for CustomerProfileService:
- "profile domain model" → ARCHITECTURE
- "customer profile model" → ARCHITECTURE
- "bounded context" (customer bounded context) → ARCHITECTURE
- "architect the customer profile structure" → ARCHITECTURE (architect verb)

---

## B15-kedge-037-002 — CustomerProfileService: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "integrate with the identity provider" → API_CONTRACT
- "connect to the SSO (Single Sign-On) service" → API_CONTRACT
- "wire up the profile enrichment API" → API_CONTRACT
- "profile service contracts" → API_CONTRACT

---

## B15-kedge-037-003 — CustomerProfileService: SECURITY triggers

Confirmed SECURITY triggers:
- "secure the PII endpoints" → SECURITY
- "encrypt the customer data store" → SECURITY
- "protect the profile update endpoint" → SECURITY
- "authenticate profile read access" → SECURITY

---

## B15-kedge-037-004 — "PII" and "personal data" are domain nouns — do NOT fire SECURITY alone

"PII", "personal data", "sensitive data" are domain nouns. They do not fire SECURITY on their own.
SECURITY fires only from: secure, harden, protect, encrypt, authenticate.
"Handle PII securely" → SECURITY (from "securely" = secure). "Store PII data" → IMPLEMENT only.

---

## B15-kedge-037-005 — "profile" does NOT fire ARCHITECTURE — modifier "domain model" required

"profile" alone does not fire ARCHITECTURE.
"profile domain model" → ARCHITECTURE. "profile data" → no predicate (domain noun).
The "[X] domain model" pattern is required — the domain noun alone is insufficient.
