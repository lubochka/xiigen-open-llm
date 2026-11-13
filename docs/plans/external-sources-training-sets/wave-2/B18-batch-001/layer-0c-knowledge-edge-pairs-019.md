# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-019-001 — AuditLog: GDPR fires SECURITY edge absolutely

**GDPR knowledge edge:**
- Token: "GDPR" (General Data Protection Regulation) → SECURITY predicate edge (Rule 14, absolute)
- "GDPR data retention", "GDPR compliance", "GDPR requirements" — all fire SECURITY
- GDPR is an absolute SECURITY trigger; no contextual suppression possible
- Co-occurrence with migration vocabulary does not suppress GDPR→SECURITY edge

---

## B18-0C-019-002 — AuditLog: PII fires SECURITY edge absolutely

**PII knowledge edge:**
- Token: "PII" (Personally Identifiable Information) → SECURITY predicate edge (Rule 14, absolute)
- "PII fields", "contains PII", "PII anonymization" — all fire SECURITY
- PII is an absolute SECURITY trigger; always fires SecurityReviewFlow

**Together with GDPR:** if both PII and GDPR appear, SECURITY fires once (predicates are single-instance). SecurityReviewFlow is activated once.

---

## B18-0C-019-003 — AuditLog: "structured audit_log" — "structured" vs "structure"

**"structured" vs "structure" knowledge edges:**

| Token | Edge | Fires ARCHITECTURE? |
|---|---|---|
| "structure" (noun) | ARCHITECTURE absolute edge | YES |
| "structured" (adjective) | Descriptive adjective; no ARCHITECTURE edge | NO |
| "audit log structure" | "structure" noun → ARCHITECTURE absolute edge | YES |
| "structured audit log" | "structured" adjective; no "structure" noun present | NO |

**Critical distinction:** "structured" is an adjective describing the noun "audit log". It does NOT contain the noun "structure". Only the noun "structure" fires the ARCHITECTURE edge.

---

## B18-0C-019-004 — AuditLog: "rename columns" → refactoring edge; sub-phase by target

**"rename" knowledge edges:**
- Token: "rename" → IMPLEMENT predicate edge
- "rename" → refactoring TransitionType edge (Rule 19)
- Sub-phase edge: determined by target vocabulary
  - "rename columns" → columns is database vocabulary → 4.4 routing edge
  - "rename method" → method is code vocabulary → 4.1 routing edge

---

## B18-0C-019-005 — AuditLog: knowledge graph for IMPLEMENT+SECURITY+ARCHITECTURE

**Full edge trace for three-predicate AuditLog scenario:**

**Request:** "Migrate audit log data and redesign the audit log schema structure; ensure GDPR compliance."

| Token | Edge | Result |
|---|---|---|
| "migrate" | predicate edge | IMPLEMENT |
| "audit log data" | routing vocabulary edge | 4.4 sub-phase |
| "redesign" | predicate edge | ARCHITECTURE |
| "schema structure" | "structure" absolute edge | ARCHITECTURE confirmed |
| "GDPR" | predicate edge | SECURITY (absolute) |

**All three predicates fire from independent edges.**

**Result:** ["IMPLEMENT", "ARCHITECTURE", "SECURITY"], feature_addition, 4.4 + 3.1 + security
