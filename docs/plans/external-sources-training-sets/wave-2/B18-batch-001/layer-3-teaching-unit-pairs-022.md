# Layer 3 — Teaching Unit Pairs (022)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-022-001 — Teaching: "credentials" fires SECURITY absolutely

**Rule:** "credentials" is an absolute SECURITY trigger in B18. It fires SecurityReviewFlow.

**Test:**
- "Migrate API credentials to new table" → SECURITY fires? **YES.** "credentials" token present.
- "Rotate stored credentials" → SECURITY fires? **YES.** "credentials" present.
- "API key management" → SECURITY fires? **No.** "API key" alone is not a SECURITY trigger; needs "credentials"/"hashed"/"encrypted"/"authentication".

---

## B18-TU-022-002 — Teaching: "authentication" fires SECURITY

**Rule:** "authentication" fires SECURITY in B18.

**Test:** "Migrate API keys and verify authentication tokens remain valid" → predicates?
**Answer:** ["IMPLEMENT", "SECURITY"], feature_addition, 4.4 + security.

"authentication" fires SECURITY. "tokens" reinforces it but is not a standalone trigger.

---

## B18-TU-022-003 — Teaching: "key rotation schema structure" fires both ARCHITECTURE and SECURITY

**Test:** "Migrate API keys and design the key rotation schema structure"

**Predicate analysis:**
- "migrate" → IMPLEMENT, 4.4
- "design" → ARCHITECTURE
- "key rotation schema structure" → "structure" → ARCHITECTURE absolute edge
- (No credentials/encrypted/GDPR in this phrase; depends on request context)

**If request also includes "credentials" or "hashed":**
- "migrate API keys, design key rotation schema structure, ensure all credentials hashed"
- ["IMPLEMENT", "ARCHITECTURE", "SECURITY"], feature_addition, 4.4 + 3.1 + security

**Predicates are always additive.**

---

## B18-TU-022-004 — Teaching: "API" vocabulary — 4.4 vs 4.3 disambiguation

**Rule:**
- "API keys" (as data being migrated = rows in a table) → 4.4 vocabulary
- "API contract" (interface/endpoint being updated) → 4.3 vocabulary
- "API key management class/service" → 4.1 vocabulary (code)

**Test:**
- "Migrate API keys to new table" → 4.4 ✓ (keys are data rows)
- "Update the billing API contract" → 4.3 ✓ (contract is an interface)
- "Refactor the ApiKeyManager class" → 4.1 ✓ (service code)

---

## B18-TU-022-005 — Teaching: four-predicate scenario fires four flows

**Test:** "Migrate API keys, design the rotation schema structure, ensure credentials hashed, document key rotation runbook" → predicates?

**Analysis:**
- "migrate" + "API keys" → IMPLEMENT, 4.4
- "design" + "schema structure" → ARCHITECTURE (two triggers: design + structure)
- "credentials hashed" → SECURITY ("credentials" absolute; "hashed" = cryptographic)
- "document" + "runbook" → DOCUMENT

**Result:** ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "DOCUMENT"], feature_addition, 4.4 + 3.1 + security + 2.1

**Four predicates → four flows → four Gate B conditions.**
