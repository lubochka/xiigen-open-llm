# Layer 0-C — Knowledge-Edge Pairs (048)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-048-001 — UI+SECURITY: SECURITY predicate lexicon

```
source_id: xiigen-b01-0c-048-001
source_license_state: first_party
pair_kind: knowledge_edge
```

SECURITY predicate firing terms (confirmed in file 048):
  Security-specific: "security", "secure", "XSS", "CSRF", "token", "authentication", "authorization"
  Security controls: "sanitization", "sanitize", "validation" (security context), "virus scanning", "malware scan"
  Security terms: "penetration", "vulnerability", "injection", "cipher", "encryption"

Gap terms that resemble SECURITY but do NOT fire:
  "private" alone → gap (context-dependent; not in SECURITY lexicon)
  "safe" alone → gap
  "protected" alone → gap
  "sensitive" alone → gap

Rule: SECURITY requires explicit security terminology, not just security-adjacent adjectives.

---

## B01-0C-048-002 — UI+SECURITY: routing when SECURITY fires vs gaps

```
source_id: xiigen-b01-0c-048-002
source_license_state: first_party
pair_kind: knowledge_edge
```

Routing decisions when SECURITY fires:
  SECURITY alone: routes to SEC.1
  UI + SECURITY: routes to 1.1 + SEC.1 (two decisions)
  UI + IMPLEMENT + SECURITY: routes to 1.1 + 4.1 + SEC.1 (three decisions)

Incorrect (v6-03 violation to avoid):
  Merging SECURITY with IMPLEMENT: routing to 4.1 only → WRONG.
  SECURITY always gets its own independent routing decision.
  SECURITY does NOT suppress UI, IMPLEMENT, or any other predicate.

Example:
  "Build a secure login form" → UI fires on "form"; IMPLEMENT fires on "build"; SECURITY fires on "secure".
  Three independent decisions: 1.1, 4.1, SEC.1.
