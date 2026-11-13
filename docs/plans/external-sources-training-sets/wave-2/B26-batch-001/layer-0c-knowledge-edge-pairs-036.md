# Layer 0-C — Knowledge Edge Pairs (036) — Complete SECURITY Knowledge Edge Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E036-001 — Comprehensive gap vocabulary master list

Regulatory labels (always gap): GDPR, SOC2, ISO 27001, PCI-DSS, HIPAA, NIST CSF, CIS Benchmark (alone)
Compliance terms (always gap): compliance, compliant, compliance-ready, certification, readiness
Risk family (always gap): risk assessment, risk management, risk register, risk posture, security risk
Vague security (always gap): "make more secure", "more secure", "ensure it's secure", "security posture conceptually"
Event nouns (always gap): "the upcoming audit", "the scheduled audit", "the certification audit"
Admin actions (always gap): "configure", "set", "deploy", "install" (without vulnerability vocabulary)

---

## E036-002 — Solo-noun SECURITY triggers (no verb required)

The following fire SECURITY as standalone nouns regardless of action verb:
  vulnerability / vulnerabilities | jwt | tls | mfa | rbac
  cipher | encryption | certificate (cryptographic context) | hash (crypto context)
  secret | credential | injection (attack context) | xss | csrf | ssrf | xxe | idor
  sanitization | signing | hmac | asvs

---

## E036-003 — "quality of security" vs "QA" disambiguation

"ensure quality of security" → SECURITY (security action vocabulary fires)
"QA the security controls" → SECURITY + QA (literal "QA" abbreviation fires QA separately)
"quality assurance on security" → SECURITY only (spelled-out "quality assurance" → gap for QA)
Teaching: "quality" modifier does not fire QA. Only the literal 2-letter abbreviation "QA" fires QA predicate.

---

## E036-004 — "code review" disambiguation

"code review" alone → gap (no security co-occurrence)
"code review for SQL injection vulnerabilities" → SECURITY (security co-occurrence present)
"security code review" → SECURITY (explicit security-action adjective)
Teaching: "code review" fires SECURITY only with explicit security-action vocabulary.

---

## E036-005 — Trust state master reference

Zero triggers (all-gap vocabulary) → knowledge_edge
1 solo-noun trigger (no audit verb) → probationary_accepted
1 solo-noun trigger + audit verb OR 2+ solo-noun triggers → deterministic_generic (moderate)
3+ triggers OR audit verb + 2+ security nouns → deterministic_generic (high confidence)
