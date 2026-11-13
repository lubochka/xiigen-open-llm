# Layer 0-C — Knowledge Edge Pairs (027) — HIPAACompliance Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E027-001 — HIPAA labels that are always gap

"HIPAA compliant" — compliance adjective → gap
"HIPAA Privacy Rule" — regulatory label → gap
"HIPAA Security Rule" — even with "Security" in the name → gap alone
"PHI" (Protected Health Information) as a noun label → gap alone (domain term, not predicate trigger)
"Business Associate Agreement" — administrative term → gap; fires DOCUMENT if documentation action present

---

## E027-002 — Technical vocabulary within HIPAA context that fires SECURITY

"encrypt PHI at rest and in transit" → SECURITY (encryption fires)
"MFA for HIPAA-protected systems" → SECURITY (mfa fires)
"audit access logs for unauthorized PHI access" → SECURITY (audit verb + unauthorized access)
"session timeout for patient data" → SECURITY (session fires)
"certificate validation for HIPAA API endpoints" → SECURITY (certificate fires)

---

## E027-003 — HIPAA + AUTHORIZATION boundary

"Audit access controls to PHI and verify RBAC boundaries" → SECURITY + AUTHORIZATION
"Ensure minimum necessary access for HIPAA" → gap ("ensure minimum necessary" is an admin phrase)
Teaching: "RBAC" and "privilege" vocabulary needed to fire AUTHORIZATION; "minimum necessary" alone is gap.

---

## E027-004 — "PHI" in document vs security context

"Document the data retention policy for PHI under HIPAA" → DOCUMENT only
"Audit PHI encryption and document findings" → SECURITY + DOCUMENT

---

## E027-005 — Trust state for HIPAA technical audits

"Audit encryption and MFA for HIPAA" → deterministic_generic (2 SECURITY triggers + audit verb)
"Review session timeout for HIPAA" → probationary_accepted (single trigger)
"Ensure HIPAA compliance" → knowledge_edge (zero predicates)
