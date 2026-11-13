# Layer 0-C — Knowledge Edge Pairs (044) — InsecureDeserializationAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E044-001 — Deserialization vocabulary

"insecure deserialization" → SECURITY (OWASP (Open Web Application Security Project) Top 10 attack category)
"gadget chain" → SECURITY (deserialization attack-vector vocabulary)
"TypeNameHandling" in security audit context → SECURITY (known .NET deserialization risk)
"deserialize the request body" → gap (functional task without vulnerability vocabulary)

---

## E044-002 — RCE (Remote Code Execution) context

"deserialization enabling RCE" → SECURITY (RCE = attack outcome)
"remote code execution vulnerability" → SECURITY (attack-vector vocabulary)
"execute remote procedures" → gap (RPC (Remote Procedure Call) context without vulnerability vocabulary)

---

## E044-003 — BinaryFormatter boundary

"switch from BinaryFormatter to System.Text.Json" → gap (migration task)
"audit BinaryFormatter usage for insecure deserialization exposure" → SECURITY (audit + vulnerability)
"remove BinaryFormatter" → gap (no audit/vulnerability vocabulary)
Teaching: Migration tasks are gap. Auditing the security risk is SECURITY.

---

## E044-004 — Type allowlist boundary

"implement a type allowlist for deserialization" → IMPLEMENT (implementation action)
"audit type allowlist for bypass vectors" → SECURITY (audit + bypass vectors)
"whitelist allowed types" → gap (configuration action without audit vocabulary)

---

## E044-005 — Trust state calibration

Insecure deserialization alone + audit → deterministic_generic
Gadget chain + RCE audit → deterministic_generic (high)
"deserialize request body" alone → gap (zero predicates)
