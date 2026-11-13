# Held-Out and Corruption Pairs (024) — Comprehensive Corruption Pattern Reference
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H024-001 — v6-01 corruption pattern: regulatory label fires SECURITY (invention)
Pattern: Model fires SECURITY on GDPR, SOC2, ISO 27001, PCI-DSS, HIPAA alone.
Corrupt example: "Ensure SOC2 Type 2 compliance." → ["SECURITY"]
Correct: []
Fix: Regulatory framework labels are ALWAYS gap. SECURITY requires technical action vocabulary.

## H024-002 — v6-01 corruption pattern: setup/configuration actions fire SECURITY (invention)
Pattern: Model fires SECURITY on "configure", "setup", "deploy" actions on security-adjacent infrastructure.
Corrupt example: "Configure the WAF (Web Application Firewall) rules." → ["SECURITY"]
Correct: [] (configuration task; "configure" without audit/vulnerability vocabulary is gap)
Fix: Setup and configuration tasks without vulnerability/audit vocabulary are gap.
Exception: "Audit the WAF configuration for security gaps." → SECURITY fires on "audit" + "security gaps".

## H024-003 — v6-03 corruption pattern: multi-predicate collapse
Pattern: Request with 3+ predicates → model collapses to 1–2 predicates.
Corrupt example: "Audit the access controls, document all findings, and QA the permission matrix." → ["SECURITY"]
Correct: ["SECURITY", "DOCUMENT", "QA"]
Fix: Each predicate fires independently. Never collapse. Three predicates → three routing decisions.

## H024-004 — v6-03 corruption pattern: FAIL suppressed to PASS
Pattern: Audit finds CRITICAL vulnerability → model outputs PASS instead of FAIL.
Corrupt example: Nine-node audit finds AES keys in source control → ArbiterVerdict: PASS
Correct: ArbiterVerdict: FAIL (any CRITICAL finding produces FAIL)
Fix: Arbiter must output FAIL when CRITICAL findings exist. FAIL is not an error state.

## H024-005 — v6-03 corruption pattern: SECURITY routing flipped to wrong sub-phase
Pattern: Audit context request → SECURITY → 5.2 (wrong). Test-writing context → SECURITY → 6.3 (wrong).
Corrupt example A: "Audit the AuthService for session fixation." → SECURITY → 5.2
Correct A: SECURITY → 6.3 (audit verb = review context = 6.3)
Corrupt example B: "Write security tests for the AuthService." → SECURITY → 6.3
Correct B: SECURITY → 5.2 (write tests = test-code-writing context = 5.2)
Fix: Action verb determines sub-phase. "audit/review/scan" → 6.3. "write/implement/assert tests" → 5.2.
