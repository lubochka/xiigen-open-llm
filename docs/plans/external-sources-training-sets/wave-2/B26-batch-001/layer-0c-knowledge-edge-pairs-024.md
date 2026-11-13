# Layer 0-C — Knowledge Edge Pairs (024) — Complete Knowledge Edge Reference
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E024-001 — Gap vocabulary master list for SECURITY predicate

SECURITY does NOT fire on:
  Regulatory labels: GDPR, SOC2, ISO 27001, PCI-DSS (as standalone acronyms)
  Compliance words: "compliance", "compliant", "compliance-ready", "certification"
  Risk vocabulary: "risk assessment", "risk management", "risk register", "security risk"
  Temporal audit nouns: "the upcoming audit", "next quarter's audit", "scheduled audit"
  Ambiguous verbs without security co-occurrence: "ensure", "review" (alone), "assess" (alone)
  Product/feature terms: "roles", "permissions" (in product-feature context), "settings"
  Performance terms: "network latency", "throughput", "load"

---

## E024-002 — Strong SECURITY trigger vocabulary (fires independently)

Single-word SECURITY triggers (fire regardless of action verb):
  "vulnerability" / "vulnerabilities" — standalone noun fires SECURITY
  "jwt" — any context involving JWT fires SECURITY
  "authentication" / "auth" — standalone fires SECURITY (may be weak if isolated)
  "encryption" / "encrypt" — standalone fires SECURITY
  "tls" — standalone fires SECURITY
  "certificate" — standalone fires SECURITY
  "secret" — standalone fires SECURITY
  "cipher" — standalone fires SECURITY
  "hash" (in password/cryptographic context) — fires SECURITY
  "mfa" — standalone fires SECURITY
  "rbac" — standalone fires SECURITY

---

## E024-003 — Abbreviation disambiguation cases

Cases where abbreviations require careful disambiguation:
  "QA" → QA predicate (abbreviation gate) | "quality assurance" → gap
  "MITM" → SECURITY | "man-in-the-middle" → SECURITY (expanded form also fires)
  "IDOR" → SECURITY + AUTHORIZATION | "direct object reference" alone → gap
  "MAC" (Message Authentication Code) → SECURITY | "MAC" (Media Access Control) → gap
  "IV" (Initialization Vector) → SECURITY (cipher context) | "IV" (Roman numeral 4) → gap
  "TLS" → SECURITY | "TLS" alone without context → weak SECURITY signal

---

## E024-004 — Verb form determines predicate for ambiguous terms

  "audit" (verb): "audit the service" → SECURITY fires
  "audit" (noun event): "the upcoming audit" → gap
  "review" + security co-occurrence → SECURITY fires
  "review" alone → gap
  "assess" + vulnerability co-occurrence → SECURITY fires
  "assess" + performance co-occurrence → gap
  "scan" + security vocabulary → SECURITY fires
  "scan" + performance/business context → gap

---

## E024-005 — Trust state determination for SECURITY signals

  Dense SECURITY vocabulary (3+ triggers + audit verb) → deterministic_generic
  Single security noun + security audit verb → deterministic_generic (moderate confidence)
  Single security noun + gap action verb → probationary_accepted (weak signal)
  Gap vocabulary only (regulatory labels, risk terms) → knowledge_edge (zero predicates)

Examples:
  "Audit authentication vulnerabilities" → deterministic_generic (audit + auth + vulnerability)
  "Look into the authentication flow" → probationary_accepted (auth noun + gap "look into")
  "Ensure GDPR compliance" → knowledge_edge (all gap vocabulary)
