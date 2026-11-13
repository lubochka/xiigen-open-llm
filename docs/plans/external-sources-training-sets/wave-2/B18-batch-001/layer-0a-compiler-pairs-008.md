# Layer 0-A — Compiler Pairs (008)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-008-001 — IMPLEMENT+DOCUMENT: migration + documentation

**Request:** "Write the database migration for the payment table and document the before/after schema changes."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2 — "write migration" = feature_addition)
- Token: "database migration" → 4.4 sub-phase signal
- Token: "payment table" → 4.4 sub-phase signal (database vocabulary)
- Token: "document" → DOCUMENT fires (Rule 11 — "document" fires DOCUMENT)
- Token: "before/after schema changes" → DOCUMENT reinforcement (describes documentation content)
- TransitionType: feature_addition (migration creates new artifact)

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 2.1

**Ordering note:** IMPLEMENT and DOCUMENT can run in parallel (documentation does not depend on migration completion; schema changes can be documented before and after).

---

## B18-0A-008-002 — "annotate" → DOCUMENT

**Request:** "Migrate the legacy customer data and annotate each migration step with inline comments explaining the transformation logic."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; feature_addition; 4.4
- Token: "customer data" → 4.4 sub-phase signal
- Token: "annotate" → DOCUMENT fires (Rule 11)
- Token: "comments" → DOCUMENT reinforcement (documentation vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 2.1

---

## B18-0A-008-003 — "spec out" → DOCUMENT

**Request:** "Write the order archive migration and spec out the data mapping rules for each legacy field."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2); feature_addition; 4.4
- Token: "spec out" → DOCUMENT (Rule 11 — "spec out" fires DOCUMENT)
- Token: "data mapping rules" → DOCUMENT content (documentation of rules)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 2.1

---

## B18-0A-008-004 — "describe" → DOCUMENT

**Request:** "Build the ETL pipeline for the product catalog and describe the transformation rules for each data field."

**Compiler trace:**
- Token: "build" + "ETL" → IMPLEMENT; feature_addition; 4.4
- Token: "describe" → DOCUMENT (Rule 11 — "describe" fires DOCUMENT)
- Token: "transformation rules" → DOCUMENT content
- Token: "data field" → 4.4 sub-phase reinforcement
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 2.1

---

## B18-0A-008-005 — OrderArchiveMigration: migration + documentation

**Request:** "Write the OrderArchiveMigration that moves orders older than 7 years to the archive table, and document the archival policy and data retention rules."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2); feature_addition; 4.4
- Token: "orders" → 4.4 sub-phase signal
- Token: "archive table" → 4.4 sub-phase signal (database vocabulary)
- Token: "document" → DOCUMENT (Rule 11)
- Token: "policy", "rules" → DOCUMENT content (policy documentation)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phases: 4.4 + 2.1
ConnectorName: OrderArchiveMigration

---

## B18-0A-008-006 — IMPLEMENT+DOCUMENT for refactoring scenario

**Request:** "Refactor the PaymentService to consolidate validation methods and document the new method signatures."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring vocabulary)
- Token: "consolidate" → IMPLEMENT (refactoring vocabulary, Rule 20)
- Token: "service" → 4.1 sub-phase signal
- Token: "methods" → 4.1 sub-phase signal
- Token: "document" → DOCUMENT (Rule 11)
- Token: "method signatures" → DOCUMENT content (API documentation)
- TransitionType: refactoring (refactor + consolidate; refactoring > feature_addition)

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: refactoring, sub-phases: 4.1 + 2.1
