# Layer 0-C — Knowledge Edge Pairs (028) — CISBenchmark Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E028-001 — CIS Benchmark labels that are always gap

"CIS Benchmark Level 1 / Level 2" — profile label → gap alone
"CIS Controls v8" — framework reference → gap alone
"CIS assessment" — assessment event noun → gap
"CIS Benchmark compliance" — compliance noun → gap

---

## E028-002 — "harden" verb is a standalone SECURITY trigger

"harden the server" → SECURITY fires (harden = security-action verb)
"harden the API gateway" → SECURITY fires
"harden authentication middleware" → SECURITY fires (harden + authentication)
Note: "harden" fires independently even without a co-occurring vulnerability noun.

---

## E028-003 — CIS Benchmark areas that fire SECURITY

Server hardening: OS (Operating System) configuration, filesystem permissions, logging → SECURITY when "audit" or "scan" verb present
Network controls: SSH (Secure Shell), TLS protocols → SECURITY (ssh/tls fire alone)
Vulnerability scanning: "open vulnerabilities" → SECURITY (vulnerability fires alone)
Access controls: "privilege escalation", "RBAC" → SECURITY + AUTHORIZATION

---

## E028-004 — Configuration tasks without audit verb remain gap

"Configure firewall rules per CIS Benchmark" → gap ("configure" without audit/vulnerability)
"Install CIS-compliant OS image" → gap (install/setup action)
Teaching: Even CIS security-related config tasks are gap without audit/review/scan/harden verb.

---

## E028-005 — "vulnerability scan" trust state

"Scan infrastructure for CIS violations and open vulnerabilities" → deterministic_generic
"Check CIS controls" → probationary_accepted (weak verb "check")
"Achieve CIS Level 2 certification" → knowledge_edge (goal/planning, no action)
