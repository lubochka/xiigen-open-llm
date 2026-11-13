# Layer 0-C — Knowledge Edge Pairs (055) — MalwareReview Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E055-001 — Malware vocabulary fires SECURITY

"malware signatures" → SECURITY (attack-vector vocabulary)
"malicious code" → SECURITY (attack vocabulary)
"backdoor binaries" → SECURITY (attack vocabulary)
"antivirus software update" → gap (maintenance task without audit vocabulary)

---

## E055-002 — Supply chain injection vocabulary

"supply chain injection of malicious code" → SECURITY (injection + malicious = two security triggers)
"dependency injection" pattern → gap (design pattern, not attack vector)
"audit supply chain for malicious code injection" → SECURITY (audit + malicious + injection)
Teaching: "supply chain injection" of code is an attack vector. Software "dependency injection" is a design pattern.

---

## E055-003 — Artifact integrity vocabulary

"artifact integrity" → SECURITY (integrity = security-action vocabulary in audit context)
"verify artifact signatures" → SECURITY (verify + signature = cryptographic integrity check)
"download artifact from repository" → gap (operational task)

---

## E055-004 — Unauthorized outbound connection vocabulary

"unauthorized outbound connection patterns" → SECURITY (unauthorized + connection pattern in audit)
"monitor network connections" → gap (observability without audit/attack vocabulary)
"audit for unauthorized outbound connections" → SECURITY (audit + unauthorized)

---

## E055-005 — Trust state calibration

Malware scan + artifact signatures → deterministic_generic
"update antivirus" alone → gap (zero predicates)
Malware + backdoor + unauthorized outbound + audit → deterministic_generic (high)
