# Layer 0-A — Compiler Pairs (030)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-030-001 — TriggerMigration: predicate firing

**Request:** "Write a migration to add a database trigger that logs all changes to the accounts table."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2, absolute)
- "database trigger" → 4.4 DDL vocabulary
- "logs all changes" → 4.4 audit data vocabulary
- "accounts table" → 4.4 data vocabulary
- No "structure", "architecture" → ARCHITECTURE does not fire
- No security tokens, no TEST verb

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-030-002 — TriggerMigration with SECURITY

**Request:** "Create a database trigger to audit all updates to the customer PII fields, logging access for GDPR compliance."

**Token scan:**
- "create a database trigger" → IMPLEMENT fires (new artifact)
- "audit all updates" → 4.4 audit vocabulary
- "customer PII fields" → "PII" → SECURITY fires (Rule 14, absolute)
- "GDPR compliance" → "GDPR" → SECURITY fires (Rule 14, absolute; reinforcement); "compliance" → SECURITY fires (absolute; third fire)
- SECURITY fires three times → single SECURITY predicate

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development

---

## B18-0A-030-003 — TriggerMigration: "replace trigger" disambiguation

**Request:** "Replace the old update_timestamp trigger with a new migration that uses an improved timestamp function."

**Token scan:**
- "replace" (code/DB object target) → IMPLEMENT fires; "replace" is a feature_addition verb (new artifact replaces old)
- "old update_timestamp trigger" → 4.4 vocabulary
- "new migration" → IMPLEMENT reinforcement (Rule 2)
- "improved timestamp function" → 4.4 database function vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-030-004 — TriggerMigration with ARCHITECTURE

**Request:** "Design the trigger-based audit log structure for the financial tables and implement the migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "audit log structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "implement the migration" → IMPLEMENT fires (Rule 2)
- "financial tables" → 4.4 data vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-030-005 — TriggerMigration: "trigger" alone does not imply SECURITY

**Request:** "Write a migration to remove the outdated inventory_update trigger and replace it with application-level logic."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2, absolute)
- "remove the outdated inventory_update trigger" → 4.4 database object vocabulary
- "replace it with application-level logic" → could suggest 4.1; but "write a migration" anchor keeps sub-phase at 4.4
- "trigger" alone does NOT fire SECURITY (trigger is a database object, not a security vocabulary token)
- No ARCHITECTURE tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development
