# Layer 0-A — Compiler Pairs (037)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-037-001 — DataAnonymization: predicate firing

**Request:** "Write a migration to anonymize all PII fields in the customers table before archiving."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "anonymize" → SECURITY fires (Rule 14: data anonymization = PII handling)
- "PII fields" → SECURITY fires (Rule 14, absolute: PII token)
- "customers table" → 4.4 vocabulary (database table)
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development

---

## B18-0A-037-002 — DataAnonymization with ARCHITECTURE

**Request:** "Design the data anonymization pattern for GDPR compliance and write the migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "anonymization pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "GDPR compliance" → SECURITY fires (Rule 14: GDPR token, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2)
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security
- DomainId: software-development

---

## B18-0A-037-003 — DataAnonymization: "anonymize" vs "anonymization" disambiguation

**Request:** "Review the data anonymization architecture for user records stored in legacy tables."

**Token scan:**
- "anonymization architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "anonymization" alone ≠ SECURITY; no explicit PII/GDPR/encrypted token present
- "stored in legacy tables" → 4.4 vocabulary (database tables)
- "review" alone does NOT fire SECURITY (no absolute security token present)
- No "write migration" or migration verb → IMPLEMENT does not fire

**Output:**
- predicates: ["ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 3.1
- DomainId: software-development

---

## B18-0A-037-004 — DataAnonymization with TEST

**Request:** "Write a migration to anonymize sensitive user data and write tests to verify all PII fields are masked."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "anonymize sensitive user data" → "sensitive" → SECURITY fires (Rule 14: sensitive data)
- "PII fields" → SECURITY fires (Rule 14, absolute: reinforcement)
- "write tests" → TEST fires (Rule 12: verb form)
- "masked" → SECURITY reinforcement (masking = PII handling)

**Output:**
- predicates: ["IMPLEMENT", "SECURITY", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security + 5.x
- DomainId: software-development

---

## B18-0A-037-005 — PIIMasking: predicate firing

**Request:** "Add a migration to mask credit card numbers and social security numbers in the payments table."

**Token scan:**
- "add a migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "mask" → SECURITY fires (Rule 14: masking = PII handling)
- "credit card numbers", "social security numbers" → SECURITY reinforcement (financial PII)
- "payments table" → 4.4 vocabulary (database table)
- No ARCHITECTURE or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development
