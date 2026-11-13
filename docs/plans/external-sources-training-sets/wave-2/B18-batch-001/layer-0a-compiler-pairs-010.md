# Layer 0-A — Compiler Pairs (010)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-010-001 — Full five-predicate: IMPLEMENT+ARCHITECTURE+SECURITY+TEST+DOCUMENT

**Request:** "Write the user data migration to the GDPR (General Data Protection Regulation)-compliant encrypted structure, design the target schema architecture, write migration tests, and document the data transformation rules."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2 — "write migration" = feature_addition; new artifact)
- Token: "user data" → 4.4 sub-phase signal (data vocabulary)
- Token: "encrypted" → SECURITY fires (encryption vocabulary)
- Token: "GDPR" (General Data Protection Regulation) → SECURITY fires (data protection regulation = security context)
- Token: "structure" → ARCHITECTURE fires (Rule 3 — always fires)
- Token: "design" → ARCHITECTURE fires (Rule 7)
- Token: "architecture" → ARCHITECTURE fires (Rule 7)
- Token: "write migration tests" → TEST (Rule 12)
- Token: "document" → DOCUMENT (Rule 11)
- Token: "transformation rules" → DOCUMENT content
- TransitionType: feature_addition ("write migration" = Rule 2; no refactoring verb dominates)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
TransitionType: feature_addition
Sub-phases: 4.4 + 3.1 + (security review) + 5.x + 2.1
ConnectorName: GdprUserDataMigration

---

## B18-0A-010-002 — SECURITY fires on "GDPR" alone

**Request:** "Migrate the user personal data table to be GDPR-compliant."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; feature_addition; 4.4
- Token: "user personal data" → 4.4 sub-phase signal
- Token: "GDPR" → SECURITY fires (data protection compliance = security context)
- Token: "compliant" → SECURITY reinforcement (compliance vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security review

---

## B18-0A-010-003 — SECURITY fires on "encrypted" alone

**Request:** "Write the LegacyPaymentMigration to move payment records to the new encrypted storage format."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2); feature_addition; 4.4
- Token: "payment records" → 4.4 sub-phase signal
- Token: "encrypted" → SECURITY fires (encryption vocabulary)
- Token: "storage format" → 4.4 sub-phase signal
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security review
ConnectorName: LegacyPaymentMigration

---

## B18-0A-010-004 — Five-predicate ordering constraints

**Request:** "Write the GDPR-compliant migration, design the target architecture structure, review security requirements, write integration tests, and document the encryption approach."

**Compiler trace (all five predicates):**
- IMPLEMENT: "write migration" (Rule 2)
- ARCHITECTURE: "design", "architecture", "structure" (Rules 3, 7)
- SECURITY: "GDPR", "security", "encryption" (security vocabulary)
- TEST: "write integration tests" (Rule 12)
- DOCUMENT: "document", "approach" in documentation context (Rule 11)
- TransitionType: feature_addition

**Orchestration order:**
1. ARCHITECTURE (schema design before migration build)
2. IMPLEMENT (write migration — primary artifact)
3. SECURITY (review encrypted implementation)
4. TEST (integration tests verify migration)
5. DOCUMENT (document after implementation complete)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"], TransitionType: feature_addition

---

## B18-0A-010-005 — "PII" (Personally Identifiable Information) fires SECURITY

**Request:** "Migrate all PII (Personally Identifiable Information) fields to the encrypted column format and document the field mapping."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; feature_addition; 4.4
- Token: "PII" (Personally Identifiable Information) → SECURITY fires (personal data = security context)
- Token: "encrypted" → SECURITY fires (encryption vocabulary)
- Token: "document" → DOCUMENT (Rule 11)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + security review + 2.1

---

## B18-0A-010-006 — Complete five-predicate rule summary for B18

**All predicate triggers in B18:**

| Predicate | Primary triggers | Notes |
|---|---|---|
| IMPLEMENT | refactor, extract, rename, consolidate, restructure, reorganize, move, clean up, migrate, write migration, seed, fixture, ETL, transform, load (data), build | Always present in B18 |
| ARCHITECTURE | structure, pattern, architecture, design (noun), redesign | Literal; "structure" always fires |
| SECURITY | GDPR, encrypted, PII, compliance, security | Data protection context |
| TEST | write tests, create tests, add tests, integration tests, unit tests | Test creation vocabulary |
| DOCUMENT | document, spec out, annotate, describe, write spec | Documentation creation |

**GapHandler:** not present in B18.
**DomainId:** "software-development" always.
**TransitionType hierarchy:** refactoring > feature_addition > documentation > no contribution > none
