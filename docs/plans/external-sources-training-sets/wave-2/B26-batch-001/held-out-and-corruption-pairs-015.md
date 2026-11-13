# Held-Out and Corruption Pairs (015)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H015-001 — Held-out: "tokenization" fires SECURITY (token trigger)
Request: "Audit the PaymentService tokenization pipeline — is PAN replaced by token before the database write?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "tokenization" → SECURITY (token trigger); "PAN" → context reinforcement

## H015-002 — Corruption v6-01: "PCI-DSS Level 1" fires SECURITY (invention)
Corrupt: "Achieve PCI-DSS Level 1 certification." → ["SECURITY"]
Correct: []
  Fix: "PCI-DSS" is a compliance framework label — gap. "achieve certification" is gap. Zero predicates.

## H015-003 — Held-out: "CVV retention" fires SECURITY
Request: "Audit the CardDataService — is CVV retained after payment authorization? PCI-DSS prohibits CVV storage."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "CVV retention" → SECURITY; "PCI-DSS" is present but gap — "CVV retention" is the trigger.

## H015-004 — Corruption v6-01: "card data environment" fires SECURITY (invention)
Corrupt: "Define the card data environment for PCI-DSS assessment." → ["SECURITY"]
Correct: []
  Fix: "define" is gap. "card data environment" is a PCI-DSS administrative term — gap. "assessment" alone is gap without vulnerability vocabulary.

## H015-005 — Held-out: "AES-GCM" fires SECURITY (cipher + encrypt triggers)
Request: "Review the CardDataService encryption — is AES-GCM (Galois/Counter Mode) used, or is vulnerable CBC mode still in place?"
  Correct: ["SECURITY"]
  Evidence: "review" (security context) → SECURITY; "AES-GCM" → SECURITY (cipher/encrypt); "CBC mode" → SECURITY; "vulnerable" → SECURITY
