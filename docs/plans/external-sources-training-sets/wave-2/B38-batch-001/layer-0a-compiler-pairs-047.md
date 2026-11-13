# Layer 0-A — Compiler Pairs (047)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-047-001 — CVENotification: document vs. knowledge_edge

Input A: "Document the CVE (Common Vulnerabilities and Exposures) notification for the patched component."
- "Document" → DOCUMENT
- "CVE notification" → DOCUMENT (notification is a document artifact) + SECURITY (CVE is a security artifact)
- PredicateSet: ["DOCUMENT","SECURITY"]
- TransitionType: no_contribution

Input B: "Register the CVE with the national vulnerability database."
- "Register" → not in lexicon
- "CVE" → SECURITY (CVE is security-domain vocabulary; but no QA from "register")
- "national vulnerability database" → context
- PredicateSet: ["SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-047-002 — CVENotification: review and approve

Input: "Review the CVE notification and approve the security patch release."
- "Review" → QA
- "approve" → QA (consolidates)
- "CVE notification" → SECURITY (security domain)
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-047-003 — CVENotification: audit and sign-off

Input: "Audit the CVE and security controls, document the vulnerability criteria, and sign off the patch."
- "Audit" → QA + SECURITY (rule 19)
- "CVE" → SECURITY (consolidates)
- "security controls" → SECURITY (consolidates)
- "document" → DOCUMENT
- "vulnerability criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "sign off" → QA (consolidates)
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-047-004 — CVENotification: build patch service

Input: "Build the patch service, audit the CVE, document the security criteria, and approve the fix."
- "Build" → IMPLEMENT (rule 6)
- "patch service" → IMPLEMENT (service — rule 2; consolidates)
- "audit" → QA + SECURITY (rule 19)
- "CVE" → SECURITY (consolidates)
- "document" → DOCUMENT
- "security criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
