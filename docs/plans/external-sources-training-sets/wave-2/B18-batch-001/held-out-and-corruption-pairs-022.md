# Held-Out and Corruption Pairs (022)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-022-001 — HO: "credentials" fires SECURITY

**Request:** "Write the ApiKeyRotationMigration to rotate all stored credentials and re-hash with BCRYPT."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security

**COR-022-001:** SECURITY absent. "credentials" treated as domain vocabulary (the thing being migrated, not a security trigger).
- Fix: "credentials" is an absolute SECURITY trigger in B18. SecurityReviewFlow fires when credentials vocabulary present.

---

## B18-HO-022-002 — HO: "API keys" alone ≠ SECURITY; "API keys hashed" → SECURITY

**Request A:** "Migrate API keys from the api_credentials table to api_keys." → predicates?
**Answer A:** ["IMPLEMENT"], feature_addition, 4.4. (No SECURITY — no explicit security vocabulary.)

**Request B:** "Migrate API keys and ensure all keys are re-hashed with SHA-256." → predicates?
**Answer B:** ["IMPLEMENT", "SECURITY"], feature_addition, 4.4 + security. ("hashed" = cryptographic → SECURITY)

**COR-022-002:** Request A incorrectly fires SECURITY because "API keys" implies sensitive data.
- Fix: "API keys" alone does not fire SECURITY. "credentials"/"hashed"/"encrypted"/"authentication" fire SECURITY.

---

## B18-HO-022-003 — HO: "key rotation schema structure" fires ARCHITECTURE

**Request:** "Migrate API keys and design the key rotation schema structure for versioned key management."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1

**COR-022-003 (v6-01):** ARCHITECTURE absent.
- Fix: "key rotation schema structure" contains "structure" → ARCHITECTURE fires absolutely.

---

## B18-HO-022-004 — HO: "authentication" → SECURITY

**Request:** "Migrate API keys to new table and verify authentication tokens remain valid after migration."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security

**COR-022-004:** SECURITY absent. "verify authentication tokens" treated as a post-migration validation step, not a SECURITY trigger.
- Fix: "authentication" fires SECURITY absolutely. "tokens" reinforces it.

---

## B18-HO-022-005 — HO: "refactor ApiKeyManager" → refactoring, 4.1 (no SECURITY)

**Request:** "Refactor the ApiKeyManager class to extract key rotation logic into a dedicated KeyRotationService."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1 (class/service vocabulary)
- Note: No SECURITY trigger in this request (no PII/encrypted/credentials/authentication)

**COR-022-005:** SECURITY fires because the domain is API key management (inherently security-sensitive).
- Fix: SECURITY fires only on explicit vocabulary. The domain being security-related does NOT fire SECURITY. "ApiKeyManager class" and "KeyRotationService" are code vocabulary → 4.1, refactoring only.
