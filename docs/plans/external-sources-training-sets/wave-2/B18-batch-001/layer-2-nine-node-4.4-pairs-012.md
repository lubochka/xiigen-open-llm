# Layer 2 — Nine-Node 4.4 Pairs (012)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-012-001 — Full five-predicate: GdprUserDataMigration

**Scenario:** GDPR-compliant user data migration with schema design, encryption, tests, and documentation.

**N01 input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- ConnectorName: GdprUserDataMigration
- DomainId: "software-development"
- Flows: DataMigrationFlow44 + ArchitectureFlow31 + SecurityReviewFlow + UnitTestingFlow51 + DocumentationFlow21

---

## B18-N9-012-002 — ArchitectureFlow31 phase: GDPR schema design

**ArchitectureFlow31 (3.1) output:**

```
Target schema: encrypted_users table
  - user_id UUID PK
  - email_encrypted BYTEA (AES-256)
  - phone_encrypted BYTEA (AES-256)
  - first_name VARCHAR (not encrypted; non-PII)
  - last_name VARCHAR (not encrypted; non-PII)
  - created_at TIMESTAMP
  - is_active BOOLEAN
  - gdpr_consent_date DATE
  - data_retention_expiry DATE (= consent_date + 7 years)

Encryption key management:
  - Key stored in encrypted_keys table (separate table, separate access control)
  - key_id FK in encrypted_users
  - Key rotation capability: update key_id + re-encrypt when key rotates
```

---

## B18-N9-012-003 — DataMigrationFlow44 phase: GDPR migration

**DataMigrationFlow44 (4.4) nine-node:**

N02: Scope — 100,000 user records; encrypt PII fields; add GDPR metadata
N05 — Script Generation:
1. GdprUserDataMigration_create_schema.sql (encrypted_users + encrypted_keys tables)
2. GdprUserDataMigration_up.sql (INSERT with AES-256 encryption calls)
3. GdprUserDataMigration_down.sql
4. GdprUserDataMigration_idempotency.sql

**N08 Gate B:**
- count(encrypted_users) == count(legacy_users) == 100,000 ✓
- all email_encrypted / phone_encrypted fields NOT NULL ✓
- all data_retention_expiry computed correctly ✓

---

## B18-N9-012-004 — Security, Test, Document phases

**SecurityReviewFlow:**
- Verifies AES-256 encryption on email_encrypted and phone_encrypted
- Verifies encrypted_keys table access controls (separate role required)
- Verifies audit_log records all access to PII fields
- Verifies GDPR consent recorded; data_retention_expiry computed
- Gate B: "All GDPR requirements met"

**UnitTestingFlow51:**
- Test: count(encrypted_users) == 100,000
- Test: email_encrypted decrypts to valid email format
- Test: data_retention_expiry == consent_date + 7 years for all users
- Test: PII fields NOT accessible without encryption key
- Gate B: all 4 tests pass

**DocumentationFlow21:**
- GDPR field mapping document
- Encryption approach document (AES-256, key management)
- Compliance checklist
- Data retention policy
- Gate B: all documents complete

---

## B18-N9-012-005 — N08 and N09: GdprUserDataMigration

**N08 Gate B (all five flows):**
- ArchitectureFlow31: schema complete ✓
- DataMigrationFlow44: count match + encryption applied ✓
- SecurityReviewFlow: GDPR requirements met ✓
- UnitTestingFlow51: all tests pass ✓
- DocumentationFlow21: all documents complete ✓

**N08 verdict: PASS**

**N09 output:**
```
{
  "connector_name": "GdprUserDataMigration",
  "flows_completed": [
    "ArchitectureFlow31", "DataMigrationFlow44",
    "SecurityReviewFlow", "UnitTestingFlow51", "DocumentationFlow21"
  ],
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "files_generated": 10,
  "artifacts": [
    "gdpr_schema_design.json",
    "GdprUserDataMigration_create_schema.sql",
    "GdprUserDataMigration_up.sql",
    "GdprUserDataMigration_down.sql",
    "security_review_report.md",
    "GdprUserDataMigration_tests.cs",
    "gdpr_field_mapping.md",
    "encryption_approach.md",
    "compliance_checklist.md",
    "data_retention_policy.md"
  ],
  "n08_verdict": "PASS",
  "completion_message": "GdprUserDataMigration complete; GDPR compliance verified; 10 files generated"
}
```

---

## B18-N9-012-006 — Five-predicate nine-node: all flows PASS Gate B

**Gate B verdict table for GdprUserDataMigration:**

| Flow | Gate B type | Gate B condition | Verdict |
|---|---|---|---|
| ArchitectureFlow31 | Schema completion | Schema specification complete | PASS |
| DataMigrationFlow44 | Row count + encryption | count(encrypted_users) == 100,000 AND all PII encrypted | PASS |
| SecurityReviewFlow | Security assertion | GDPR requirements met; audit trail complete | PASS |
| UnitTestingFlow51 | Test pass | All 4 migration tests pass | PASS |
| DocumentationFlow21 | Doc completeness | All 4 documents complete | PASS |

**Overall N08 verdict: PASS**

**This is the maximum predicate scenario for B18.** Five predicates → five flows → five Gate B evaluations → one overall verdict.
