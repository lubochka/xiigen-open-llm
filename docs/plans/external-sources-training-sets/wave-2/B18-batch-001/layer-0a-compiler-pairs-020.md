# Layer 0-A — Compiler Pairs (020)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-020-001 — SessionData migration: "migrate session data" → IMPLEMENT, feature_addition, 4.4

**Request:** "Migrate all active session data from the in-memory session store to the new persistent session_store table."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "session data" → 4.4 sub-phase signal (data vocabulary)
- Token: "session store" → 4.4 sub-phase signal (data storage vocabulary)
- Token: "persistent session_store table" → 4.4 sub-phase signal (database vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-020-002 — SessionData: "session_id tokens encrypted" → SECURITY fires

**Request:** "Migrate session data to the database and ensure session_id tokens are encrypted before storage."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "session data" → 4.4 sub-phase signal
- Token: "encrypted" → SECURITY fires (Rule 14, absolute)
- Token: "session_id tokens" → security reinforcement (credentials vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phases: 4.4 + security

---

## B18-0A-020-003 — SessionData: "session storage structure" → ARCHITECTURE fires

**Request:** "Migrate session data and define the session storage structure to support distributed session management."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "session data" → 4.4 sub-phase signal
- Token: "session storage structure" → "structure" fires ARCHITECTURE (Rule 3, absolute)
- Token: "define" → ARCHITECTURE reinforcement
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-020-004 — SessionData: "refactor session management" → refactoring, 4.1

**Request:** "Refactor the SessionManager class to extract session persistence into a separate ISessionStore interface and implementation."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring vocabulary)
- Token: "SessionManager class" → 4.1 sub-phase signal (service/class vocabulary)
- Token: "extract" → IMPLEMENT (refactoring vocabulary)
- Token: "ISessionStore interface" → 4.1 sub-phase signal (class vocabulary)
- TransitionType: refactoring (Rule 9)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

---

## B18-0A-020-005 — SessionData: "write the session migration" = feature_addition (Rule 2)

**Request:** "Write the SessionDataMigration to transfer all user session records from Redis to the PostgreSQL session_store table."

**Compiler trace:**
- Token: "write" → IMPLEMENT
- Token: "migration" → IMPLEMENT; feature_addition signal (Rule 2)
- Token: "user session records" → 4.4 sub-phase signal
- Token: "session_store table" → 4.4 sub-phase signal (database vocabulary)
- TransitionType: feature_addition (Rule 2 — new migration artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
