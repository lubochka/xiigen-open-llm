# Layer 0-C — Knowledge Edge Pairs (020)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E020-001 — "TLS" fires SECURITY; "HTTPS" co-occurring with audit context fires SECURITY

A-fires: "Audit the load balancer TLS termination — confirm TLS 1.3 is enforced and TLS 1.0/1.1 are disabled."
  "audit" → SECURITY; "TLS" → SECURITY (tls trigger); "TLS 1.3" → SECURITY; "disabled" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

A-borderline: "The API gateway uses HTTPS for all inbound traffic."
  "HTTPS" → SECURITY weak trigger; "uses HTTPS" without audit/vulnerability vocabulary → weak signal
  CompiledPredicateIds: ["SECURITY"] — probationary_accepted (TLS/HTTPS mention without audit verb)

---

## E020-002 — "MITM" fires SECURITY; "man-in-the-middle" (expanded form) also fires SECURITY

A: "Review the inter-service communication for man-in-the-middle attack exposure — is mutual TLS enforced between all pods?"
  "review" (security context) → SECURITY; "man-in-the-middle" → SECURITY (MITM expanded form); "mutual TLS" → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: Both the abbreviation "MITM" and the expanded form "man-in-the-middle" fire SECURITY.

---

## E020-003 — "firewall rules" fires SECURITY via security context; "firewall" alone is weak

A-fires: "Audit the firewall rules for the production cluster — confirm all inbound ports except 443 and 80 are blocked."
  "audit" → SECURITY; "firewall rules" → SECURITY (security context noun); "inbound ports blocked" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The firewall was updated during the maintenance window."
  "firewall" alone → weak; "updated during maintenance" → gap context
  CompiledPredicateIds: [] — no audit/vulnerability vocabulary → knowledge_edge

---

## E020-004 — "open port" fires SECURITY in audit context; "port number" alone is gap

A-fires: "Scan all services for open ports that should not be externally accessible — report any unexpected TCP port exposures."
  "scan" → SECURITY; "open ports" → SECURITY; "TCP port exposures" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Configure the service to listen on port 8080."
  "listen on port 8080" → gap (configuration, not security audit)
  CompiledPredicateIds: []

---

## E020-005 — "replay attack" fires SECURITY; "duplicate request" is gap

A-fires: "Audit the WebhookService for replay attack vulnerability — are request nonces validated and expired within 5 minutes?"
  "audit" → SECURITY; "replay attack" → SECURITY; "nonces" → SECURITY (cryptographic context)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Handle duplicate webhook requests gracefully by checking the event ID."
  "duplicate webhook requests" → gap; "event ID" → gap
  CompiledPredicateIds: []
