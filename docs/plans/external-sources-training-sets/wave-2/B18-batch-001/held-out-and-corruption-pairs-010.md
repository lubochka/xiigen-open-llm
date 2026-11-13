# Held-Out and Corruption Pairs (010)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-010-001 — HO: GDPR migration → IMPLEMENT+ARCHITECTURE+SECURITY, feature_addition, 4.4

**Request:** "Write the GdprUserDataMigration to anonymize 4 million user records, design the target schema structure using the privacy-by-design architecture pattern, and ensure all PII fields are encrypted at rest."

**Expected classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1 + security
- Flows: DataMigrationFlow44 + ArchitectureFlow31 + SecurityReviewFlow

**Reasoning:**
- "write migration" → IMPLEMENT, feature_addition, 4.4 (Rule 2)
- "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "PII" → SECURITY fires (Rule 14 — PII is security vocabulary)
- "encrypted" → SECURITY fires (Rule 14, absolute)
- "anonymize" → SECURITY reinforcement (data anonymization = privacy concern)
- TransitionType: feature_addition (migration is the artifact; no refactoring verbs)

---

## B18-HO-010-002 — COR: SECURITY suppressed in migration context (v6-01 variant)

**Request:** "Write the GdprUserDataMigration to anonymize 4 million user records and ensure all PII fields are encrypted at rest."

**Corrupted classification:**
- predicates: ["IMPLEMENT"] ← WRONG (ARCHITECTURE and SECURITY both missing)
- TransitionType: feature_addition (correct)
- Sub-phase: 4.4 only
- Flow: DataMigrationFlow44 only

**Corruption type:** v6-01 variant — SECURITY suppressed because "PII" and "encrypted" treated as migration description
**Corrupted reasoning:** "encrypted at rest is a standard infrastructure requirement, not a predicate trigger; PII is just describing the data type being migrated, not triggering a security review"

**Why wrong:**
1. "encrypted" fires SECURITY (Rule 14, absolute — no context suppression)
2. "PII" (Personally Identifiable Information) fires SECURITY (absolute trigger)
3. Migration context does NOT suppress SECURITY triggers
4. Every absolute trigger fires regardless of surrounding context

---

## B18-HO-010-003 — COR: ARCHITECTURE suppressed in GDPR context

**Request:** "Write the GdprUserDataMigration to anonymize all user records, design the privacy-by-design architecture pattern for the target schema structure."

**Corrupted classification:**
- predicates: ["IMPLEMENT", "SECURITY"] ← WRONG (ARCHITECTURE missing)
- TransitionType: feature_addition
- Sub-phase: 4.4 + security

**Corruption type:** v6-01 — ARCHITECTURE suppressed despite "architecture", "pattern", and "structure" all present
**Corrupted reasoning:** "the architecture reference here is just naming the privacy-by-design approach, not requesting architectural design work"

**Why wrong:**
- "architecture" fires ARCHITECTURE (Rule 7, absolute)
- "pattern" fires ARCHITECTURE (Rule 7, absolute)
- "structure" fires ARCHITECTURE (Rule 3, absolute)
- Three independent ARCHITECTURE triggers → ARCHITECTURE fires three times
- Context (naming a privacy approach) cannot suppress absolute rules

---

## B18-HO-010-004 — HO: "anonymize" alone does NOT fire SECURITY

**Request:** "Anonymize the test data in the staging environment by replacing real names with generated placeholders."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44
- SECURITY: does NOT fire

**Reasoning:**
- "anonymize" → data manipulation vocabulary → 4.4
- "anonymize" alone without SECURITY vocabulary (encrypted, PII, GDPR, credential, token, auth) → SECURITY does NOT fire
- "test data" + "staging environment" → 4.4 data context
- No encryption, no PII label, no GDPR → SECURITY absent
- SECURITY fires on: encrypted, PII, GDPR, credentials, token, authentication, authorization, sensitive data, access control

**Contrast:** "anonymize PII" → SECURITY fires (PII is the trigger, not anonymize)

---

## B18-HO-010-005 — HO: "GDPR" alone fires SECURITY

**Request:** "Write the migration to ensure all user data complies with GDPR requirements."

**Expected classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase: 4.4 + security
- Flows: DataMigrationFlow44 + SecurityReviewFlow

**Reasoning:**
- "write migration" → IMPLEMENT, feature_addition, 4.4
- "GDPR" → SECURITY fires (absolute trigger — GDPR is always a security/compliance concern)
- No "structure", "architecture", "pattern" → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

---

## B18-HO-010-006 — Summary: SECURITY trigger vocabulary in B18

| Term | SECURITY fires? | Notes |
|---|---|---|
| "encrypted" | YES | Rule 14, absolute |
| "PII" | YES | Personally Identifiable Information = security |
| "GDPR" | YES | Compliance = security trigger |
| "credentials" | YES | Always SECURITY |
| "access control" | YES | Authorization = SECURITY |
| "authentication" | YES | Auth = SECURITY |
| "sensitive data" | YES | SECURITY |
| "anonymize" (alone) | NO | Data manipulation only |
| "anonymize PII" | YES | PII fires it, not anonymize |
| "privacy" (alone) | CONTEXT | "privacy-by-design" without PII/encrypt may not fire |
| "private" (field) | NO | OOP visibility modifier, not security |
