# Layer 0-C — Knowledge-Edge Pairs (038)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-038-001 — ComplianceRelease context boundary

### ComplianceRelease terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| compliance | Process noun | Fires QA in verification context; no predicate alone |
| regulatory | Adjective | No predicate alone |
| standard | Requirement noun | No predicate |
| framework | Process noun | No predicate |
| SOC2 | Standard name | No predicate |
| PCI-DSS (Payment Card Industry Data Security Standard) | Standard name | No predicate |
| ISO27001 | Standard name | No predicate |

### ComplianceRelease inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Ensure the release meets all compliance requirements." | "Ensure" not in lexicon → [] |
| "Apply SOC2 compliance controls." | "Apply" not in lexicon → [] |
| "Configure the PCI-DSS compliance framework." | "Configure" not in lexicon → [] |

---

## B38-0C-038-002 — AuditableRelease context boundary

### AuditableRelease terms: context only
| Term | Category | Notes |
|---|---|---|
| auditable | Adjective | No predicate alone |
| audit trail | Audit noun | No predicate alone |
| audit log | Log noun | No predicate |
| tamper-proof | Adjective | No predicate |
| immutable log | Log type | No predicate |
| traceability | Property noun | No predicate |

### AuditableRelease inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Configure the release pipeline for full auditability." | "Configure" not in lexicon → [] |
| "Enable tamper-proof audit logging." | "Enable" not in lexicon → [] |
| "Store the immutable audit trail." | "Store" not in lexicon → [] |

---

## B38-0C-038-003 — "compliance" context vs. predicate trigger

"Compliance" as a standalone noun is context — no predicate.
"Compliance" in verification context ("verify compliance") fires QA via the QA lexicon term "compliance" (rule: compliance fires QA).

| Sentence | PredicateSet |
|---|---|
| "Ensure SOC2 compliance." | [] → knowledge_edge |
| "Verify compliance with release standards." | ["QA"] ("verify" fires QA; "compliance" also fires QA; consolidate) |
| "Audit the security compliance controls." | ["QA","SECURITY"] (audit fires QA; security fires SECURITY) |
| "Document the compliance requirements." | ["DOCUMENT"] ("document" fires) |
