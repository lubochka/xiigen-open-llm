# Held-Out and Corruption Pairs (020)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-020-001 — HO: "move session data" (data) → feature_addition, 4.4

**Request:** "Move session data from the in-memory store to the new persistent session_store table."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4

**COR-020-001 (v6-02):** TransitionType: refactoring ("move" is a refactoring verb).
- Fix: "move" + data target (session data, records) = feature_addition + 4.4. "move" + code target (class, method) = refactoring + 4.1. Disambiguation: target vocabulary.

---

## B18-HO-020-002 — HO: "session_id tokens encrypted" fires SECURITY via "encrypted"

**Request:** "Migrate session data to the database; session_id tokens must be encrypted before storage."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security

**COR-020-002:** SECURITY absent. "session_id tokens" not recognized as security vocabulary.
- Fix: "encrypted" is the absolute SECURITY trigger here. "session_id tokens" alone does not fire SECURITY; "encrypted" fires it absolutely.

---

## B18-HO-020-003 — HO: "session storage structure" fires ARCHITECTURE

**Request:** "Write the SessionDataMigration and define the session storage structure for distributed systems."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1

**COR-020-003 (v6-01):** ARCHITECTURE absent.
- Fix: "session storage structure" contains "structure" (noun) → ARCHITECTURE fires absolutely.

---

## B18-HO-020-004 — HO: "refactor SessionManager" → refactoring, 4.1 (not 4.4)

**Request:** "Refactor the SessionManager class to extract session persistence into ISessionStore."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase: 4.1 (SessionManager class, ISessionStore interface)

**COR-020-004 (v6-03):** Sub-phase 4.4 because "session" is in the domain.
- Fix: "SessionManager class" and "ISessionStore interface" are code vocabulary → 4.1. "session" as data (session records, session_store table) → 4.4. Code class target → 4.1.

---

## B18-HO-020-005 — HO: combined code + data → both sub-phases

**Request:** "Refactor the SessionManager and migrate all active session records to the PostgreSQL session_store."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring (Rule 9 — "refactor" present)
- Sub-phases: 4.1 (SessionManager code) + 4.4 (session records data)
- Flows: BackendServiceFlow41 + DataMigrationFlow44

**COR-020-005:** Only sub-phase 4.1. DataMigrationFlow44 not activated.
- Fix: Both 4.1 vocabulary (SessionManager) and 4.4 vocabulary (session records) present → both sub-phases fire; both flows activate.
