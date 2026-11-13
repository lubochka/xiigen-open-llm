# Layer 0-A — Compiler Pairs (044)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-044-001 — EventSourcingSchema: predicate firing

**Request:** "Write a migration to create the event_store table with append-only constraints for an event sourcing system."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "create the event_store table" → 4.4 vocabulary (DDL table creation)
- "event sourcing system" → 4.4 architecture vocabulary
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No security tokens → SECURITY does not fire

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-044-002 — EventSourcingSchema with ARCHITECTURE

**Request:** "Design the event sourcing schema structure for the order domain and write the migration to create the event tables."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "event sourcing schema structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2)
- "order domain" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-044-003 — CQRSProjection: predicate firing

**Request:** "Write a migration to create the read-model projection tables for the CQRS (Command Query Responsibility Segregation) order query side."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "create the read-model projection tables" → 4.4 vocabulary (DDL creation)
- "CQRS" (Command Query Responsibility Segregation) → 4.4 architecture vocabulary
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire independently
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-044-004 — CQRSProjection with ARCHITECTURE

**Request:** "Design the CQRS read-model projection pattern and implement the migration to create the query-side tables."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "CQRS read-model projection pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "implement the migration" → IMPLEMENT fires (Rule 2)
- "query-side tables" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-044-005 — AuditTableSetup: predicate firing

**Request:** "Write a migration to create audit tables for tracking changes to the orders and customers tables."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "create audit tables" → 4.4 vocabulary (DDL table creation)
- "tracking changes" → 4.4 vocabulary (audit = data tracking)
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No PII/GDPR/encrypted tokens present → SECURITY does not fire from "audit" alone

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development
