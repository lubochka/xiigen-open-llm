# Layer 0-C — Knowledge Edge Pairs (054) — ForensicsAnalysis Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E054-001 — Forensic analysis vocabulary

"forensic artifact" → SECURITY (forensic analysis = security-action context)
"audit authentication logs for attack patterns" → SECURITY (audit + authentication + attack)
"collect and archive logs" → gap (data collection without analysis vocabulary)
"review compromised server image" → SECURITY (review + compromised = breach context)

---

## E054-002 — Backdoor vocabulary

"backdoor" → SECURITY (attack-vector vocabulary)
"backdoor binary" → SECURITY
"unauthorized SSH key addition" → SECURITY (unauthorized + credential vocabulary)
"add SSH key for deployment" → gap (operational task, no "unauthorized" qualifier)
Teaching: "unauthorized" qualifier transforms neutral vocabulary into SECURITY vocabulary.

---

## E054-003 — Breach context vocabulary

"breach" → SECURITY (attack outcome vocabulary)
"attack timeline" in breach context → SECURITY
"incident log" → gap (logging infrastructure without attack-vocabulary)
"audit breach artifacts" → SECURITY (audit + breach)

---

## E054-004 — Credential scope in forensics

"affected credentials" in breach context → SECURITY (credential vocabulary + breach context)
"credential scope" in forensic audit → SECURITY (audit + credential)
"list all active credentials" → gap (credential inventory without audit/breach vocabulary)

---

## E054-005 — Trust state calibration

Forensic audit + backdoor + credential → deterministic_generic (high)
"collect server logs" alone → gap (zero predicates)
Breach audit + SQL injection artifacts + credentials → deterministic_generic (three triggers)
