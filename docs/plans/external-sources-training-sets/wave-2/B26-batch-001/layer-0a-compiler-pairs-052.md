# Layer 0-A — Compiler Pairs (052) — ThreatIntelReport
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C052-001 — Threat intelligence review → SECURITY

A: "Review the threat intelligence report for IOCs (Indicators of Compromise) matching our authentication service logs."
  "review" → SECURITY | "IOCs" → SECURITY (attack-vector vocabulary) | "authentication service" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C052-002 — IOC (Indicator of Compromise) scan → SECURITY

A: "Scan application logs for indicators of compromise from the latest CISA (Cybersecurity and Infrastructure Security Agency) advisory."
  "scan" → SECURITY | "indicators of compromise" → SECURITY (attack-vector vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C052-003 — "read the threat intel report" → gap (no security action)

A: "Read through the weekly threat intelligence briefing for the security team."
  "read" → gap | "threat intelligence briefing" → gap (passive information consumption, no action verb)
  CompiledPredicateIds: [] → knowledge_edge

---

## C052-004 — Threat intel + credential exposure → SECURITY

A: "Audit exposed credentials listed in the threat intelligence feed against our active service accounts."
  "audit" → SECURITY | "exposed credentials" → SECURITY (credential vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C052-005 — Threat intel + DOCUMENT → SECURITY+DOCUMENT

A: "Review threat intelligence IOCs against our perimeter logs and document findings in the security incident register."
  "review" → SECURITY | "IOCs" → SECURITY | "document findings" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation
