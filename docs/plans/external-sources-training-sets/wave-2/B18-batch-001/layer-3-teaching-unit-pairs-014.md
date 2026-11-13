# Layer 3 — Teaching Unit Pairs (014)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-014-001 — Teaching: ETL routes to 4.4, not 4.3

**Question:** Does ETL vocabulary route to 4.3 (connector/API) or 4.4 (data migration)?

**Answer:** Always 4.4. ETL (Extract, Transform, Load) is a data pipeline vocabulary word. Sub-phase 4.3 is for connector/webhook/API integration vocabulary.

**4.4 signals:** ETL, extract (data), transform (data), load (data), CSV, data pipeline, warehouse
**4.3 signals:** API endpoint, webhook, connector, external service integration, HTTP call

**Anti-example (v6 ETL misrouting):** "Build ETL pipeline" → routed to ApiContractFlow43 (4.3). Wrong: ETL is data migration vocabulary → DataMigrationFlow44 (4.4).

---

## B18-TU-014-002 — Teaching: "extract" disambiguation (ETL vs code)

**Rule:** "extract" has two meanings in B18. Context determines the edge.

**"extract" (code target):**
- "extract the PaymentProcessor class" → refactoring, 4.1
- Context signals: class, method, service, interface present

**"extract" (data target):**
- "extract historical events from CSV" → feature_addition, 4.4
- Context signals: file, CSV, data, records, database present

**Test:** "Build ETL to extract, transform, load analytics events" → sub-phase 4.4, feature_addition (data context).
**Test:** "Extract the EventProcessor into a separate class" → sub-phase 4.1, refactoring (code context).

---

## B18-TU-014-003 — Teaching: "encrypted" fires SECURITY in any context

**Rule:** "encrypted" is an absolute SECURITY trigger (Rule 14). Context does not suppress it.

**All fire SECURITY:**
- "store events encrypted at rest" ✓
- "ensure user_id fields are encrypted" ✓
- "encrypted columns" ✓
- "encrypt the session tokens" ✓

**Does not fire SECURITY:**
- "protect the data" (no explicit security vocabulary)
- "secure the transfer" (without "encrypted"/"PII"/"GDPR")

---

## B18-TU-014-004 — Teaching: "architecture" token fires ARCHITECTURE absolutely

**Rule:** "architecture" as a noun is an absolute ARCHITECTURE trigger (Rule 7).

**Test questions:**
1. "Design the event log architecture pattern" → ARCHITECTURE fires? **YES** (architecture + pattern — both fire).
2. "Implement an event-driven architecture" → ARCHITECTURE fires? **YES** ("architecture" token present).
3. "Design the domain architecture" → ARCHITECTURE fires? **YES**.
4. "Build a microservices event pipeline" → ARCHITECTURE fires? **NO** ("microservices" is not an absolute trigger; no "structure"/"architecture"/"pattern" token).

---

## B18-TU-014-005 — Teaching: ETL "transform" is not a TransitionType: refactoring signal

**Rule:** "transform" in ETL context fires feature_addition, NOT refactoring.

**"transform" = feature_addition when:**
- Context is data pipeline (ETL, CSV, records, warehouse)
- "transform data/records/fields" → feature_addition + 4.4

**"transform" as refactoring when:**
- (This is not a B18 case. In B18, "transform" is exclusively data vocabulary → 4.4.)

**Teaching point:** "transform" in B18 always fires feature_addition and routes to 4.4. No refactoring edge for "transform" in B18.
