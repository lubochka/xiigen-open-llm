# Held-Out and Corruption Pairs (015)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H015-001 — Held-out: "vault" fires SECURITY as tool name

A: "Integrate HashiCorp Vault for secret management."
  Expected: ["SECURITY"] (vault fires SECURITY; "integrate" — gap unless IMPLEMENT context)
  Note: "integrate" → IMPLEMENT? Not reliably; "integrate" is not in IMPLEMENT primary lexicon. "vault" fires SECURITY.
  Expected: ["SECURITY"]
  Held-out for evaluation.

---

## H015-002 — Held-out: "key rotation" fires SECURITY

A: "Define the key rotation schedule for the production environment."
  Expected: ["ARCHITECTURE", "SECURITY"] — "define" → ARCHITECTURE; "key rotation" → SECURITY.
  Held-out for evaluation.

---

## H015-003 — Held-out: "rotate the secrets" is gap

A: "Rotate the API credentials for the payment gateway."
  Expected: [] — "rotate" alone is operational gap.
  Held-out for evaluation.

---

## C015-001 — Corruption v6-01: SECRETS invented as predicate

A: "Design the secrets management topology."
  Correct: ["ARCHITECTURE", "SECURITY"]
  Corrupt: ["ARCHITECTURE", "SECRETS"] ← v6-01 (SECRETS is not a valid predicate)
  Teaching: "secrets management topology" → ARCHITECTURE (topology) + SECURITY (secret). SECRETS is not a valid predicate.

---

## C015-002 — Corruption v6-02: "encrypt" fires IMPLEMENT instead of SECURITY

A: "Encrypt the configuration secrets at rest."
  Correct: ["SECURITY"]
  Corrupt: ["IMPLEMENT"] ← v6-02 (encrypt treated as an implementation action)
  Teaching: "encrypt" → SECURITY, not IMPLEMENT. Encryption is in the SECURITY predicate lexicon; it does not fire IMPLEMENT.
