# Layer 0-A — Compiler Pairs (033)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-033-001 — JSONBColumn: predicate firing

**Request:** "Write a migration to add a JSONB (JSON Binary) column to the products table to store flexible attributes."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2, absolute)
- "JSONB column" → 4.4 DDL vocabulary (column addition is a database migration)
- "products table" → 4.4 data vocabulary
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No security tokens, no TEST verb

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-033-002 — JSONBColumn with ARCHITECTURE

**Request:** "Design the JSONB column structure for storing configurable product attributes before creating the migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "JSONB column structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "creating the migration" → IMPLEMENT fires (Rule 2)
- "configurable product attributes" → 4.4 data vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-033-003 — JSONBColumn: migrate JSONB data

**Request:** "Migrate all existing product metadata from separate columns into a single JSONB column."

**Token scan:**
- "migrate all existing product metadata" → IMPLEMENT fires (Rule 5: data vocabulary)
- "product metadata" → 4.4 data vocabulary
- "JSONB column" → 4.4 DDL vocabulary
- "separate columns" → 4.4 column vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-033-004 — JSONBColumn with SECURITY

**Request:** "Add a JSONB column for customer preference data and ensure that any PII fields within the JSON are encrypted."

**Token scan:**
- "add a JSONB column" → IMPLEMENT fires (new artifact)
- "customer preference data" → 4.4 data vocabulary
- "PII fields" → "PII" → SECURITY fires (Rule 14, absolute)
- "encrypted" → SECURITY fires (Rule 14, absolute; reinforcement)
- SECURITY fires twice → single SECURITY predicate

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development

---

## B18-0A-033-005 — JSONBColumn: "restructure" with JSONB

**Request:** "Restructure the user settings table to consolidate all preference fields into a JSONB column."

**Token scan:**
- "restructure" → IMPLEMENT fires; restructure is a refactoring verb → TransitionType: refactoring (Rule 1)
- "user settings table" → 4.4 vocabulary
- "consolidate all preference fields" → "consolidate" reinforces refactoring (Rule 20)
- "JSONB column" → 4.4 DDL vocabulary
- No ARCHITECTURE tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development
