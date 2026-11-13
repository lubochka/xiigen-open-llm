# Layer 3 — Teaching Unit Pairs (020)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU020-001 — Network security vocabulary: TLS fires SECURITY; "network latency" does not
A: "Audit the network configuration for TLS 1.3 enforcement." vs "Review network latency between microservices."
B: "TLS" → SECURITY. "audit" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "network latency" → gap (performance term). "review" alone → gap. CompiledPredicateIds: [].

## TU020-002 — "MITM" fires SECURITY; "man-in-the-middle" (expanded form) also fires SECURITY
A: "Review the inter-service communication for MITM (man-in-the-middle) attack exposure."
B: Both abbreviation and expanded form fire SECURITY. "MITM" → SECURITY. "man-in-the-middle" → SECURITY.
   "mutual TLS" → SECURITY. "certificate validation" → SECURITY. Dense signal. CompiledPredicateIds: ["SECURITY"].

## TU020-003 — "open port" fires SECURITY in audit context; "configure port" is gap
A: "Scan all services for open ports that should not be externally accessible." vs "Configure the service to listen on port 8080."
B: "scan" + "open ports" + "externally accessible" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "configure... port 8080" → gap (configuration task). CompiledPredicateIds: [].

## TU020-004 — "replay attack" fires SECURITY; "duplicate request" is gap
A: "Audit the WebhookService for replay attack vulnerabilities." vs "Handle duplicate webhook requests by checking event ID."
B: "replay attack" → SECURITY. "vulnerabilities" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "duplicate webhook requests" → gap. CompiledPredicateIds: [].

## TU020-005 — Firewall rules fire SECURITY in audit context; firewall update alone is gap
A: "Audit the firewall rules for the production cluster — confirm all inbound ports except 443 blocked." vs "The firewall was updated during the maintenance window."
B: "audit" + "firewall rules" + "inbound ports" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "firewall was updated" → gap (maintenance event). CompiledPredicateIds: [].
