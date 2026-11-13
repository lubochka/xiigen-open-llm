# Held-Out and Corruption Pairs (019)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-019-001 — HO: GDPR fires SECURITY absolutely in AuditLog

**Request:** "Migrate the audit_log to the new table and apply GDPR data retention rules."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security

**COR-019-001:** SECURITY absent. "GDPR data retention" treated as a migration requirement, not a SECURITY trigger.
- Fix: "GDPR" is an absolute SECURITY trigger (Rule 14). SecurityReviewFlow always fires when GDPR present.

---

## B18-HO-019-002 — HO: "structured audit log" ≠ ARCHITECTURE

**Request:** "Migrate to the new structured audit log table for better query performance."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4

**COR-019-002 (v6-01):** ARCHITECTURE fires from "structured".
- Fix: "structured" is an adjective, not the noun "structure". Adjective form does NOT fire ARCHITECTURE. "audit log structure" (noun form) would fire; "structured audit log" (adjective form) does not.

---

## B18-HO-019-003 — HO: PII fires SECURITY; anonymize alone does not

**Request:** "Migrate audit log entries and anonymize all records containing PII before transfer."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security
- Reason: "PII" fires SECURITY absolutely; "anonymize" reinforces but does not independently trigger

**COR-019-003:** "anonymize" alone fires SECURITY.
- The knowledge graph note: "anonymize" alone does NOT fire SECURITY. "anonymize PII" fires SECURITY via "PII", not "anonymize".
- Correct: SECURITY fires from "PII" (absolute trigger).

---

## B18-HO-019-004 — HO: "rename audit columns" → refactoring, 4.4

**Request:** "Rename the audit_trail columns to align with the new audit_log naming convention."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring (Rule 19 — "rename" is refactoring verb)
- Sub-phase: 4.4 (columns = database vocabulary)

**COR-019-004 (v6-03):** Sub-phase 4.1, flow: BackendServiceFlow41 (refactoring always routes to 4.1).
- Fix: TransitionType is refactoring, but sub-phase is determined by vocabulary. "columns" = database vocabulary → 4.4. DataMigrationFlow44 handles database column renaming.

---

## B18-HO-019-005 — HO: IMPLEMENT+ARCHITECTURE+SECURITY three predicates from one AuditLog request

**Request:** "Migrate audit log data, redesign the audit log schema structure, and ensure GDPR compliance."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1 + security

**COR-019-005:** Only ["IMPLEMENT", "SECURITY"]. ARCHITECTURE absent.
- Fix: "schema structure" contains "structure" → ARCHITECTURE fires absolutely. Three predicates fire independently: IMPLEMENT (migrate), ARCHITECTURE (schema structure), SECURITY (GDPR).
