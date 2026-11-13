# Layer 3 — Teaching Unit Pairs (026)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: SendGrid Email connector

---

## B17-TU-026-001 — SendGrid: "integrate" never fires IMPLEMENT

**A:** "Integrate SendGrid to send transactional emails." What fires?

**B:** API_CONTRACT only. Rule 1 is absolute: "integrate" → ["API_CONTRACT"] → 3.3 → documentation. "transactional emails" is a domain phrase — no predicate. IMPLEMENT absent.

---

## B17-TU-026-002 — SendGrid: API key domain knowledge

**A:** "Build the SendGrid email connector using the SendGrid API key." Does SECURITY fire?

**B:** No. "API key" without the word "token" does not fire SECURITY (Rule 6 requires "token"). CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]. SecurityAuditFlow34 is NOT selected.

---

## B17-TU-026-003 — SendGrid: webhook validation

**A:** "Build the SendGrid connector to handle delivery webhooks and validate the HMAC-SHA256 event signature." What fires?

**B:** IMPLEMENT (Build), API_CONTRACT (connector + webhook), SECURITY (validate HMAC-SHA256 signature — Rule 8). CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]. DecisionCount: 3.

---

## B17-TU-026-004 — SendGrid: "email" domain noun

**A:** Does "email" fire any predicate?

**B:** No. "Email" is a domain noun. It fires no predicate. Neither "email" nor "transactional email" nor "bulk email" supplies integration vocabulary. Only "connector", "webhook", "API", "SDK" in integration context fire integration vocabulary.
