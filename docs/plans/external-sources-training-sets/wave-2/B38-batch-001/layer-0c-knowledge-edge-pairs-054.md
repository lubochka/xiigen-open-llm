# Layer 0-C — Knowledge-Edge Pairs (054)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-054-001 — Gated release and regulatory submission: predicate disambiguation

| Input | PredicateSet |
|---|---|
| "Configure the gated release." | [] |
| "Review the gated release conditions." | ["QA"] |
| "Build the release gate service." | ["IMPLEMENT"] |
| "Document the gate exit criteria." | ["DOCUMENT"] |
| "Audit the security gate controls." | ["QA","SECURITY"] |
| "Submit the regulatory submission." | [] |
| "Review the regulatory submission." | ["QA"] |
| "Build the regulatory submission service." | ["IMPLEMENT"] |
| "Document the regulatory criteria." | ["DOCUMENT"] |
| "Audit the security compliance of the regulatory submission." | ["QA","SECURITY"] |

---

## B38-0C-054-002 — Auditable release and final report: predicate disambiguation

| Input | PredicateSet |
|---|---|
| "Configure the release for auditability." | [] |
| "Audit the release process." | ["QA","SECURITY"] (audit fires QA+SECURITY rule 19) |
| "Build the audit trail service." | ["IMPLEMENT"] |
| "Document the auditability criteria." | ["DOCUMENT"] |
| "Archive the final release report." | [] |
| "Review the final release report." | ["QA"] |
| "Document the final release report." | ["DOCUMENT"] |
| "Build the release reporting service." | ["IMPLEMENT"] |

---

## B38-0C-054-003 — "audit" alone vs. "audit" + "security"

Critical distinction: "audit" alone fires QA AND SECURITY (rule 19) — always co-fires both.

| Sentence | PredicateSet |
|---|---|
| "Audit the release process." | ["QA","SECURITY"] (audit fires QA+SECURITY) |
| "Audit the release criteria." | ["QA","SECURITY"] (audit fires QA+SECURITY) |
| "Audit the migration guide." | ["QA","SECURITY"] (audit fires QA+SECURITY) |
| "Audit the changelog entries." | ["QA","SECURITY"] (audit fires QA+SECURITY) |

Note: Rule 19 states audit co-fires SECURITY in all contexts — not just when "security" is co-present.
The specification in files 039, 046, etc. says "audit" + "security" co-fires both — but audit alone also fires both per rule 19.
This is the authoritative rule; earlier files showing "audit alone = QA only" should yield to rule 19.
