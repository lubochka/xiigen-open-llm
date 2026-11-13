# Layer 0-A — Compiler Pairs (022)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-022-001 — ApiKeyRotation migration: "migrate API keys" → IMPLEMENT, feature_addition, 4.4

**Request:** "Migrate all legacy API keys from the api_credentials table to the new api_keys table with SHA-256 hashed storage."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "API keys" → 4.4 sub-phase signal; also security-adjacent vocabulary
- Token: "api_credentials table" → 4.4 sub-phase signal (database vocabulary)
- Token: "SHA-256 hashed" → SECURITY fires (encryption/hashing vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security

---

## B18-0A-022-002 — ApiKeyRotation: "credentials" → SECURITY fires absolutely

**Request:** "Write the ApiKeyRotationMigration to rotate all stored credentials and re-hash them using BCRYPT."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT; feature_addition (Rule 2)
- Token: "ApiKeyRotation" → 4.4 sub-phase signal
- Token: "credentials" → SECURITY fires (credentials vocabulary is an absolute SECURITY trigger)
- Token: "re-hash" → SECURITY reinforcement (hashing = cryptographic operation)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security

---

## B18-0A-022-003 — ApiKeyRotation: "key rotation schema structure" → ARCHITECTURE fires

**Request:** "Migrate API keys and design the key rotation schema structure to support versioned key management."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "API keys" → 4.4 sub-phase signal
- Token: "design" → ARCHITECTURE (Rule 7)
- Token: "schema structure" → "structure" fires ARCHITECTURE (Rule 3, absolute)
- Token: "key rotation" → SECURITY reinforcement
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1 + security

**Teaching point:** Three predicates fire independently. "structure" fires ARCHITECTURE regardless of whether the subject is a database schema or a key management schema.

---

## B18-0A-022-004 — ApiKeyRotation: "refactor key management" → refactoring, 4.1

**Request:** "Refactor the ApiKeyManager class to extract key rotation logic into a dedicated KeyRotationService."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring vocabulary)
- Token: "ApiKeyManager class" → 4.1 sub-phase signal (class/service vocabulary)
- Token: "extract" → IMPLEMENT (refactoring vocabulary)
- Token: "KeyRotationService" → 4.1 sub-phase signal
- TransitionType: refactoring (Rule 9 — "refactor" verb present)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

---

## B18-0A-022-005 — ApiKeyRotation: "authentication" fires SECURITY

**Request:** "Migrate API keys to the new table and verify that all authentication tokens remain valid after the migration."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "API keys" → 4.4 sub-phase signal
- Token: "authentication" → SECURITY fires (authentication vocabulary is a B18 SECURITY trigger)
- Token: "tokens" → SECURITY reinforcement
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security
