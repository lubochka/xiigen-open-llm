# Layer 0-A — Compiler Pairs (049)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-049-001 — CrossDatabaseMigration with SECURITY

**Request:** "Migrate financial transaction records from the legacy Oracle database to PostgreSQL and ensure all sensitive payment data is encrypted at rest."

**Token scan:**
- "migrate" → IMPLEMENT fires (Rule 5: data migration)
- "sensitive payment data" → SECURITY fires (Rule 14: sensitive data)
- "encrypted at rest" → SECURITY fires (Rule 14: encrypted, absolute; reinforcement)
- "financial transaction records" → 4.4 vocabulary
- No ARCHITECTURE or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development

---

## B18-0A-049-002 — NoSQLToSQL with TEST

**Request:** "Migrate all product reviews from DynamoDB to the PostgreSQL reviews table and write tests to verify row counts match."

**Token scan:**
- "migrate" → IMPLEMENT fires (Rule 5)
- "write tests" → TEST fires (Rule 12: verb form "write tests")
- "row counts match" → TEST context (count assertion)
- "product reviews from DynamoDB" → 4.4 vocabulary
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-049-003 — EventSourcingSchema with SECURITY

**Request:** "Write a migration to create the event_store with encrypted payload fields for GDPR compliance."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "encrypted payload fields" → SECURITY fires (Rule 14: encrypted, absolute)
- "GDPR compliance" → SECURITY fires (Rule 14: GDPR, absolute; reinforcement)
- "event_store" → 4.4 vocabulary
- No ARCHITECTURE or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development

---

## B18-0A-049-004 — CQRSProjection with TEST

**Request:** "Implement the CQRS projection tables migration and write tests to verify all events are correctly projected."

**Token scan:**
- "implement the CQRS projection tables migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form)
- "correctly projected" → TEST context (projection verification)
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-049-005 — AuditTableSetup with ARCHITECTURE + SECURITY

**Request:** "Design the audit table structure for GDPR-compliant change tracking and write the migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "audit table structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "GDPR-compliant" → SECURITY fires (Rule 14: GDPR, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2)
- No TEST tokens; no refactoring verbs

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security
- DomainId: software-development
