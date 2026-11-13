# Layer 0-C — Knowledge Edge Pairs (052) — ThreatIntelReport Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E052-001 — IOC (Indicator of Compromise) vocabulary

"IOCs" (Indicators of Compromise) → SECURITY (attack-vector vocabulary abbreviation)
"indicators of compromise" (spelled out) → SECURITY
"indicators of performance" → gap (no security-action vocabulary)
"read the threat intel briefing" → gap (passive reading, no action verb)

---

## E052-002 — Threat intelligence action verb boundary

"review threat intelligence for IOCs" → SECURITY (review + IOCs)
"scan for IOCs from CISA advisory" → SECURITY (scan + IOCs)
"read the weekly threat brief" → gap (no action verb on security object)
Teaching: "review" and "scan" fire SECURITY when applied to IOCs or security feeds. "read" does not.

---

## E052-003 — Exposed credential vocabulary

"exposed credentials in threat feed" → SECURITY (credential vocabulary + threat context)
"audit exposed credentials against service accounts" → SECURITY (audit + credential)
"credential feed subscription" → gap (feed management, no audit/vulnerability vocabulary)

---

## E052-004 — CISA advisory context

"CISA advisory CVEs" → SECURITY (CVE = vulnerability vocabulary)
"CISA compliance reporting" → gap (compliance label without technical action vocabulary)
"scan for CISA-listed IOCs" → SECURITY (scan + IOCs)

---

## E052-005 — Trust state calibration

IOC review + credential exposure → deterministic_generic
"read threat brief" alone → gap (zero predicates)
IOC scan + CISA CVEs + exposed credentials → deterministic_generic (high, three triggers)
