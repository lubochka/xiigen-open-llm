# Cross-Layer Synthesis (004)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

## B18-CLS-004 — Full Seven-Layer Trace: GdprUserDataMigration (Five Predicates)

**Request:** "Write the GdprUserDataMigration to anonymize and migrate 4 million user records to the normalized privacy schema structure, design the privacy-by-design architecture pattern for the target schema, ensure all PII fields are encrypted at rest, write integration tests to verify all user records transferred and anonymized correctly, and document the migration runbook."

---

### Layer 0-A: Compiler

**Input tokens:** write migration, anonymize, migrate, user records, schema structure, design, architecture pattern, PII, encrypted, write integration tests, document, migration runbook

**Predicate firing:**
- "write migration" → IMPLEMENT fires (Rule 2)
- "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "design" → ARCHITECTURE fires (Rule 11)
- "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "PII" → SECURITY fires (Rule 14, absolute)
- "encrypted" → SECURITY fires (Rule 14, absolute)
- "write integration tests" → TEST fires (Rule 5)
- "document" → DOCUMENT fires (Rule 4)
- No refactoring verbs → feature_addition
- ARCHITECTURE fires four independent times (structure, design, architecture, pattern) → single ARCHITECTURE predicate in output

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- DomainId: software-development
- GapHandler: none

---

### Layer 0-B: Router

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"], vocabulary: user records, migration, PII, integration tests, runbook

**Routing decisions:**
- IMPLEMENT + data vocabulary → DataMigrationFlow44 (4.4)
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- SECURITY → SecurityReviewFlow (security sub-phase)
- TEST → UnitTestingFlow51 (5.x)
- DOCUMENT → DocumentationFlow21 (2.1)
- TransitionType: feature_addition → confirms DataMigrationFlow44

**Flow dependency analysis:**
- ArchitectureFlow31 must precede DataMigrationFlow44 (schema design first)
- DataMigrationFlow44 must precede SecurityReviewFlow (encrypt migrated data)
- DataMigrationFlow44 must precede UnitTestingFlow51 (tests verify migration)
- DocumentationFlow21: eligible for parallel with SecurityReviewFlow or UnitTestingFlow51

**Output:**
- IMPLEMENT → DataMigrationFlow44 (sub-phase 4.4)
- ARCHITECTURE → ArchitectureFlow31 (sub-phase 3.1)
- SECURITY → SecurityReviewFlow
- TEST → UnitTestingFlow51 (sub-phase 5.x)
- DOCUMENT → DocumentationFlow21 (sub-phase 2.1)
- No QA predicate → QA last rule not triggered

---

### Layer 0-C: Knowledge Edge

**Active edges for this request:**
- "write migration" → IMPLEMENT + feature_addition + 4.4 (Rule 2 + Rule 6)
- "structure" → ARCHITECTURE (Rule 3, absolute)
- "design" (verb) → ARCHITECTURE (Rule 11)
- "architecture" (noun) → ARCHITECTURE (Rule 7, absolute)
- "pattern" → ARCHITECTURE (Rule 7, absolute)
- "PII" → SECURITY (Rule 14, absolute)
- "encrypted" → SECURITY (Rule 14, absolute)
- "write integration tests" → TEST (Rule 5)
- "document" + "runbook" → DOCUMENT (Rule 4)
- "user records" → 4.4 data vocabulary
- "anonymize" → SECURITY reinforcement (privacy action)
- No "refactor" → Rule 9 not active

**Key knowledge edges specific to GDPR scenario:**
- "anonymize" alone does NOT fire SECURITY (it fires data vocabulary → 4.4)
- "anonymize PII" fires SECURITY (via "PII", not "anonymize")
- "GDPR" fires SECURITY if present (not in this request, but "PII" covers it)

---

### Layer 0-D: Flow Factory

**Factory mode determination:**
- GdprUserDataMigration — five-predicate scenario; registered after repeated classification
- Factory mode: trusted_learned (five-predicate GDPR migration is a known scenario)
- Gate B types: five evaluations required

**Factory output:**
```
FlowFactoryEntry {
  ScenarioId: "GdprUserDataMigration",
  Flows: [ArchitectureFlow31, DataMigrationFlow44, SecurityReviewFlow, UnitTestingFlow51, DocumentationFlow21],
  TransitionType: feature_addition,
  FactoryMode: trusted_learned,
  GateBSet: [
    {Flow: ArchitectureFlow31, Type: schema_review_complete, Condition: "privacy_schema_spec_approved"},
    {Flow: DataMigrationFlow44, Type: row_count_assertion, Condition: "migrated_user_count == source_user_count"},
    {Flow: SecurityReviewFlow, Type: security_scan_pass, Condition: "no_unencrypted_PII_fields"},
    {Flow: UnitTestingFlow51, Type: test_pass_assertion, Condition: "all_integration_tests_pass"},
    {Flow: DocumentationFlow21, Type: doc_complete, Condition: "migration_runbook_published"}
  ]
}
```

---

### Layer 1: Transition Orchestrator

**Ordering input:** [ArchitectureFlow31, DataMigrationFlow44, SecurityReviewFlow, UnitTestingFlow51, DocumentationFlow21]

**Orchestration rules applied:**
- Rule: ARCHITECTURE first (schema design prerequisite)
- Rule: IMPLEMENT after ARCHITECTURE (migration uses schema spec)
- Rule: SECURITY after IMPLEMENT (encrypt migrated data — requires migration completion)
- Rule: TEST after IMPLEMENT (verify migration — hard dependency)
- Rule: DOCUMENT parallel-eligible (runbook can be drafted alongside execution)
- Rule: No QA → QA last rule not triggered

**Execution schedule:**
1. ArchitectureFlow31 — design privacy schema; output: privacy-by-design schema spec
2. DataMigrationFlow44 — migrate 4M user records; anonymize PII fields per schema spec
3. SecurityReviewFlow — verify encryption; scan for unencrypted PII fields
4. UnitTestingFlow51 — run integration tests verifying record counts and anonymization
5. DocumentationFlow21 — publish migration runbook (eligible to run parallel with steps 3-4)

**No QA flow** — "QA" abbreviation not present in request. QA last rule does not apply.

---

### Layer 2: Nine-Node Trace (DataMigrationFlow44 — primary flow)

**N01 — Input:** "anonymize and migrate 4M user records; PII encrypted; integration tests; document runbook"

**N02 — Scope:** GdprUserDataMigration; source: legacy users table; target: users_privacy_v2 (normalized, encrypted PII); 4M records

**N03 — Analysis:** PII field identification; encryption key provisioning; anonymization strategy (pseudonymization vs full anonymization); batch size planning for 4M records

**N04 — Specification:**
- GdprUserDataMigration.cs (primary migration)
- UserPiiAnonymizer.cs (anonymization logic)
- UserEncryptionHandler.cs (field encryption at rest)
- UserMigrationBatcher.cs (batch processing — 4M records)
- UserMigrationValidator.cs (PII field validation post-migration)
- UserMigrationRollback.cs (idempotency)
- GdprMigrationIntegrationTests.cs (TEST flow)
- PrivacySchemaSpec.md (ArchitectureFlow31 output)
- MigrationRunbook.md (DocumentationFlow21 output)
- SecurityScanReport.md (SecurityReviewFlow output)

**N05 — Generation:** batch migration with anonymization + encryption; checkpoint per batch

**N06 — Execution:** run in batches; validate PII fields encrypted after each batch; capture counts

**N07 — Pre-Gate:** regression check; confirm no user queries broken; TransitionType: feature_addition confirmed

**N08 — Gate B (five evaluations):**
1. ArchitectureFlow31: `privacy_schema_spec_approved == true` → PASS
2. DataMigrationFlow44: `migrated_user_count == 4_000_000` → PASS
3. SecurityReviewFlow: `no_unencrypted_PII_fields == true` → PASS
4. UnitTestingFlow51: `all_integration_tests_pass == true` → PASS
5. DocumentationFlow21: `migration_runbook_published == true` → PASS

**N09 — Output:** GdprUserDataMigration delivered; all five Gate B evaluations PASS; 10 files generated.

---

### Layer 3: Teaching Points

**Core lessons from GdprUserDataMigration:**

**Lesson 1:** Multiple ARCHITECTURE triggers do not add multiple ARCHITECTURE predicates. "structure" + "design" + "architecture" + "pattern" all fire ARCHITECTURE — the output predicate appears exactly once.

**Lesson 2:** SECURITY fires on absolute vocabulary (PII, encrypted) regardless of context. Even in a migration scenario, SECURITY is not suppressed.

**Lesson 3:** Five predicates = five flows = five Gate B evaluations. Each flow has exactly one Gate B. No flow is exempt.

**Lesson 4:** DocumentationFlow21 is parallel-eligible. It does not depend on DataMigrationFlow44 completing — the runbook can be drafted while migration runs.

**Lesson 5:** No QA predicate fires unless "QA" abbreviation appears. QA is not implied by "integration tests" or "verify" or any other term.

---

### Cross-Layer Invariants Verified

| Layer | Input | Output | Consistent? |
|---|---|---|---|
| 0-A Compiler | 4 ARCH triggers + PII/encrypt + tests + document | 5 predicates; feature_addition | YES |
| 0-B Router | 5 predicates | 5 flows; sequential + 1 parallel eligible | YES |
| 0-C Knowledge | GDPR/PII/encrypt absolute edges | SECURITY fires; ARCH fires 4 times → 1 predicate | YES |
| 0-D Factory | GdprUserDataMigration | trusted_learned; 5 Gate B entries | YES |
| L1 Orchestrator | 5 flows | ARCH→IMPL→SEC+TEST; DOC parallel eligible | YES |
| L2 Nine-Node | 5 Gate B evaluations | all PASS; 10 files | YES |
| L3 Teaching | 5 lessons | multi-trigger collapse; SECURITY absolute; DOC parallel | YES |
