# Layer 0-C — Knowledge-Edge Pairs (057)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-057-001 — Regulatory and compliance: full disambiguation table

| Input | PredicateSet | Rule |
|---|---|---|
| "Submit the regulatory package." | [] | submit gap |
| "File the regulatory dossier." | [] | file gap |
| "Apply for regulatory clearance." | [] | apply gap |
| "Ensure compliance requirements are met." | [] | ensure gap |
| "Configure the compliance controls." | [] | configure gap |
| "Verify compliance with release standards." | ["QA"] | verify fires QA |
| "Review the regulatory submission." | ["QA"] | review fires QA |
| "Inspect the compliance controls." | ["QA"] | inspect fires QA |
| "Compliance gate approval." | ["QA"] | compliance fires QA |
| "Document the regulatory criteria." | ["DOCUMENT"] | document + criteria |
| "Document the compliance requirements." | ["DOCUMENT"] | document + requirements |
| "Build the compliance service." | ["IMPLEMENT"] | build + service |
| "Build the regulatory submission service." | ["IMPLEMENT"] | build + service |
| "Audit the security compliance." | ["QA","SECURITY"] | audit rule 19 |
| "Audit the regulatory security controls." | ["QA","SECURITY"] | audit rule 19 |

---

## B38-0C-057-002 — Gated release and auditable release: full disambiguation table

| Input | PredicateSet | Rule |
|---|---|---|
| "Configure the gated release pipeline." | [] | configure gap |
| "Set the gate threshold." | [] | set gap |
| "Open the release gate." | [] | open gap |
| "Configure the audit logging." | [] | configure gap |
| "Enable tamper-proof logging." | [] | enable gap |
| "Review the gated release." | ["QA"] | review fires QA |
| "Audit the release process." | ["QA","SECURITY"] | audit rule 19 |
| "Sign off the release gate." | ["QA"] | sign-off fires QA |
| "Document the gate criteria." | ["DOCUMENT"] | document + criteria |
| "Build the release gate service." | ["IMPLEMENT"] | build + service |
| "Build the audit trail service." | ["IMPLEMENT"] | build + service |

---

## B38-0C-057-003 — Final report and closure: full disambiguation table

| Input | PredicateSet | Rule |
|---|---|---|
| "Archive the final release report." | [] | archive gap |
| "File the project closure document." | [] | file gap |
| "Store the lessons learned document." | [] | store gap |
| "Report the release status." | [] | report (verb) gap |
| "Review the final release report." | ["QA"] | review fires QA |
| "Document the final release report." | ["DOCUMENT"] | document fires |
| "Document the final release criteria." | ["DOCUMENT"] | document + criteria |
| "Build the release reporting service." | ["IMPLEMENT"] | build + service |
| "Audit the release records." | ["QA","SECURITY"] | audit rule 19 |
