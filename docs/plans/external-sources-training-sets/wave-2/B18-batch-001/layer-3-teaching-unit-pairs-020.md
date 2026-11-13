# Layer 3 — Teaching Unit Pairs (020)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-020-001 — Teaching: "migrate session data" → feature_addition, 4.4

**Rule:** "migrate" + data vocabulary (session data, session_store) → IMPLEMENT + feature_addition + 4.4.

**Test:** "Migrate all active session data from in-memory store to database" → predicates?
**Answer:** ["IMPLEMENT"], feature_addition, 4.4.

No ARCHITECTURE (no "structure"), no SECURITY (no PII/encrypted/GDPR), no TEST (no "write tests").

---

## B18-TU-020-002 — Teaching: "encrypted" in session context fires SECURITY

**Rule:** "encrypted" is an absolute SECURITY trigger regardless of what is being encrypted.

**Test:** "Migrate session data ensuring session_id tokens are encrypted before storage" → predicates?
**Answer:** ["IMPLEMENT", "SECURITY"], feature_addition, 4.4 + security.

"encrypted" fires SECURITY from its absolute edge. Session context does not suppress it.

---

## B18-TU-020-003 — Teaching: "session storage structure" fires ARCHITECTURE

**Rule:** Compound noun "session storage structure" contains "structure" → ARCHITECTURE fires.

**Test:** "Migrate session data and define the session storage structure" → predicates?
**Answer:** ["IMPLEMENT", "ARCHITECTURE"], feature_addition, 4.4 + 3.1.

"storage structure" = storage + structure. "structure" retains its absolute ARCHITECTURE trigger edge.

---

## B18-TU-020-004 — Teaching: "move session data" → feature_addition, 4.4

**Rule:** "move" + data target → feature_addition + 4.4. "move" + code target → refactoring + 4.1.

**Test:** "Move session data from Redis to PostgreSQL" → TransitionType?
**Answer:** feature_addition. "move" targets data (session records), not code.

**Contrast:** "Move the SessionManager method to the StorageLayer class" → refactoring (code class target).

---

## B18-TU-020-005 — Teaching: refactoring + migration → two sub-phases fire

**Rule:** When both 4.1 vocabulary (code) and 4.4 vocabulary (data) are present, both sub-phases fire.

**Test:** "Refactor the SessionManager class and migrate session records to the database."
- "Refactor" + "SessionManager class" → IMPLEMENT, refactoring, 4.1
- "migrate session records" → IMPLEMENT, 4.4 added to sub-phases
- TransitionType: refactoring (Rule 9 — "refactor" verb dominates)
- Flows: BackendServiceFlow41 (4.1) + DataMigrationFlow44 (4.4)

**Both sub-phases fire independently. Neither suppresses the other.**
