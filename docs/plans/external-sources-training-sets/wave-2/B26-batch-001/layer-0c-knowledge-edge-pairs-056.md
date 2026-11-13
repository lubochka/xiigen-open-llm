# Layer 0-C — Knowledge Edge Pairs (056) — APTHunting Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E056-001 — APT vocabulary fires SECURITY

"APT" (Advanced Persistent Threat) → SECURITY (attack-actor vocabulary)
"lateral movement" → SECURITY (attack technique vocabulary)
"credential harvesting" → SECURITY (attack technique vocabulary)
"exfiltration" → SECURITY (attack outcome vocabulary)
"monitor network traffic" → gap (observability context)

---

## E056-002 — Persistence mechanism vocabulary in APT context

"scheduled task injection" → SECURITY (injection in persistence context)
"registry key modification" in APT hunting → SECURITY (attack persistence vocabulary)
"create scheduled task for cleanup" → gap (operational task without attack vocabulary)
Teaching: Registry/scheduled task operations are gap unless in APT hunting/audit context with attack vocabulary.

---

## E056-003 — Service account compromise vocabulary

"compromised service account credentials" → SECURITY (credential vocabulary + compromised qualifier)
"service account for database access" → gap (credential configuration)
"audit compromised service account scope" → SECURITY (audit + compromised + credential)

---

## E056-004 — RBAC injection in APT context

"RBAC role injection" → SECURITY + AUTHORIZATION (dual-predicate: role injection = authorization violation + security attack)
"inject role claim" → SECURITY + AUTHORIZATION
"configure RBAC roles" → AUTHORIZATION only (no attack vocabulary present)

---

## E056-005 — Trust state calibration

APT audit + lateral movement + credential harvesting → deterministic_generic (high)
"monitor traffic" alone → gap (zero predicates)
APT + privilege escalation + RBAC injection → deterministic_generic (dual predicates, high)
