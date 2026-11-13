# Layer 0-C — Knowledge Edge Pairs (050) — RedTeamExercise Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E050-001 — Red team event noun vs findings audit

"run a red team exercise" → gap (event planning/execution)
"schedule red team engagement" → gap (scheduling is always gap)
"review red team findings" → SECURITY (review verb + findings context)
"audit red team attack paths" → SECURITY (audit verb present)

---

## E050-002 — Lateral movement vocabulary

"lateral movement" → SECURITY (attack vector vocabulary)
"network segmentation review for lateral movement" → SECURITY (review + attack vector)
"monitor network for anomalies" → gap (observability context, no attack-vector vocabulary)

---

## E050-003 — Credential harvesting vocabulary

"credential harvesting" → SECURITY (attack vector vocabulary)
"harvest credentials" in attacker-path context → SECURITY (attack vocabulary)
"collect user credentials" in onboarding context → gap (no attack-vector indicator)
Teaching: "credential harvesting" is an attack-vector term. "collect credentials" in feature context is gap.

---

## E050-004 — Persistence mechanism vocabulary

"persistence mechanisms" in red team context → SECURITY (attack vocabulary)
"scheduled task injection" → SECURITY (injection in persistence context)
"create scheduled tasks" → gap (operational task without attack vocabulary)

---

## E050-005 — Trust state calibration

Red team findings + auth bypass + credential harvesting → deterministic_generic (high)
"run red team exercise" alone → gap (zero predicates)
Lateral movement + privilege escalation audit → deterministic_generic (dual predicates)
