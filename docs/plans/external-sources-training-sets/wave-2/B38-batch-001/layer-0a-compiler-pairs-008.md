# Layer 0-A — Compiler Pairs (008)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-008-001 — "security audit" + compliance fires QA+SECURITY; go-live is gap

Input:
"Audit the security posture of the release and validate compliance before go-live approval."

Predicate analysis:
- "Audit" → QA (audit in release context) AND contributes to SECURITY (audit in security context per rule 19)
- "security" → SECURITY (security in security-domain context fires SECURITY)
- "validate" → QA
- "compliance" → QA (compliance is in QA lexicon)
- "go-live" → GAP
- "approval" → QA (approve variant)

Predicates fired: ["QA", "SECURITY"]
CapabilityGapWarnings: ["go-live"]
TransitionType: no_contribution (no IMPLEMENT present)
DecisionCount: 2
Phase routing: 7.1 (QA) + 3.4 (SECURITY)
DomainId: software-development

Reasoning: Rule 19 is key here: "security" + "audit" fires SECURITY because audit is in security context. "audit" alone in release context fires QA. When both "security" and "audit" appear together, both QA and SECURITY fire. "go-live" is gap vocabulary per rule 5.

---

## B38-0A-008-002 — "security" alone fires SECURITY; "compliance" fires QA

Input:
"Review the security controls and ensure compliance with the release standards."

Predicate analysis:
- "Review" → QA
- "security" → SECURITY (security in any meaningful context fires SECURITY)
- "compliance" → QA
- "standards" → context word

Predicates fired: ["QA", "SECURITY"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 3.4
DomainId: software-development

Reasoning: "security" fires SECURITY in a standalone sense (not just in security + audit combination). "compliance" fires QA per the QA lexicon. Two predicates, QA last.

---

## B38-0A-008-003 — "audit" in pure release context (without "security") fires QA only

Input:
"Audit the release process and approve the sign-off register."

Predicate analysis:
- "Audit" → QA (audit in release context, no security-domain terms present, so SECURITY does not fire)
- "release" → context word
- "approve" → QA
- "sign-off" → QA

Predicates fired: ["QA"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 1
Phase routing: 7.1
DomainId: software-development

Reasoning: Rule 19 qualification: "audit" in release context fires QA; SECURITY requires "security" domain context to co-fire. Without "security" keyword or security-domain terms, "audit" maps to QA only.

---

## B38-0A-008-004 — "compliance" in security context escalates to SECURITY

Input:
"Validate security compliance posture and inspect the vulnerability scan results before approval."

Predicate analysis:
- "Validate" → QA
- "security" → SECURITY
- "compliance" → QA (compliance also contributes to SECURITY context)
- "inspect" → QA
- "vulnerability" → SECURITY (vulnerability is security-domain vocabulary)
- "approval" → QA

Predicates fired: ["QA", "SECURITY"]
CapabilityGapWarnings: []
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 3.4
DomainId: software-development

Reasoning: "security" and "vulnerability" firmly establish SECURITY predicate. "compliance" in this context also supports SECURITY. Multiple QA signals consolidate to single QA predicate. QA ordered last.

---

## B38-0A-008-005 — Four signals, two predicates: QA+SECURITY with go-live gap

Input:
"Sign off on the security assessment, verify compliance requirements, and approve before the go-live window."

Predicate analysis:
- "Sign off" → QA
- "security" → SECURITY
- "assessment" → context word
- "verify" → QA
- "compliance" → QA + SECURITY context
- "approve" → QA
- "go-live" → GAP

Predicates fired: ["QA", "SECURITY"]
CapabilityGapWarnings: ["go-live"]
TransitionType: no_contribution
DecisionCount: 2
Phase routing: 7.1 + 3.4
DomainId: software-development

Reasoning: Rich QA signal set (sign off, verify, compliance, approve) and SECURITY context (security, compliance in security context). "go-live" is gap. No IMPLEMENT or DOCUMENT terms present.
