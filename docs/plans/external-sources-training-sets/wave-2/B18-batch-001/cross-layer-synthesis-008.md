# Cross-Layer Synthesis (008)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

## B18-CLS-008 — Full Seven-Layer Trace: SubscriptionBillingMigration (IMPLEMENT + ARCHITECTURE + SECURITY + TEST + DOCUMENT → five-predicate)

**Request:** "Migrate all subscription billing records to the new billing_ledger table, design the billing ledger schema structure, ensure all payment credentials and card tokens are encrypted at rest, write integration tests to verify all subscription records migrated correctly, and document the migration runbook."

---

### Layer 0-A: Compiler

**Input tokens:** migrate, subscription billing records, new billing_ledger table, design, billing ledger schema structure, payment credentials, card tokens, encrypted at rest, write integration tests, verify, subscription records migrated, document, migration runbook

**Predicate firing:**
- "migrate" → IMPLEMENT fires (Rule 2; data vocabulary)
- "subscription billing records" → 4.4 sub-phase signal (database data vocabulary)
- "design" → ARCHITECTURE fires (Rule 11)
- "billing ledger schema structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "credentials" → SECURITY fires (Rule 14, absolute)
- "encrypted at rest" → "encrypted" → SECURITY fires (Rule 14, absolute; reinforcement)
- "card tokens" → SECURITY reinforcement (payment tokens = credentials domain)
- "write integration tests" → TEST fires (Rule 5)
- "document" → DOCUMENT fires (Rule 4)
- "migration runbook" → DOCUMENT reinforcement
- No refactoring verbs → TransitionType: feature_addition
- ARCHITECTURE fires twice (design + structure) → single ARCHITECTURE predicate in output
- SECURITY fires three times (credentials, encrypted, card tokens) → single SECURITY predicate

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- DomainId: software-development
- GapHandler: none

---

### Layer 0-B: Router

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"], TransitionType: feature_addition, sub-phase signals: 4.4 + 3.1 + security + 5.x + 2.1

**Routing decisions:**
- IMPLEMENT + data vocabulary (subscription billing records) → DataMigrationFlow44 (sub-phase 4.4)
- ARCHITECTURE → ArchitectureFlow31 (sub-phase 3.1)
- SECURITY → SecurityReviewFlow (security sub-phase)
- TEST → UnitTestingFlow51 (sub-phase 5.x)
- DOCUMENT + runbook → DocumentationFlow21 (sub-phase 2.1, parallel-eligible)
- TransitionType: feature_addition → confirms DataMigrationFlow44 (no refactoring redirect)

**Flow dependency analysis:**
- ArchitectureFlow31 must precede DataMigrationFlow44 (schema design before migration)
- DataMigrationFlow44 must precede SecurityReviewFlow (encrypt migrated credentials — requires migration data to exist)
- DataMigrationFlow44 must precede UnitTestingFlow51 (tests verify migration output)
- SecurityReviewFlow and UnitTestingFlow51 are parallel-eligible (neither blocks the other)
- DocumentationFlow21 (runbook): parallel-eligible from DataMigrationFlow44 onward

**Output:**
- IMPLEMENT → DataMigrationFlow44 (sub-phase 4.4)
- ARCHITECTURE → ArchitectureFlow31 (sub-phase 3.1)
- SECURITY → SecurityReviewFlow
- TEST → UnitTestingFlow51 (sub-phase 5.x)
- DOCUMENT → DocumentationFlow21 (sub-phase 2.1, parallel-eligible)

---

### Layer 0-C: Knowledge Edge

**Active edges:**
- "migrate" → IMPLEMENT edge; feature_addition TransitionType edge; 4.4 routing edge
- "subscription billing records" → 4.4 data vocabulary edge
- "design" (verb) → ARCHITECTURE edge (Rule 11)
- "billing ledger schema structure" → "structure" → ARCHITECTURE absolute edge (Rule 3)
- "credentials" → SECURITY absolute edge (Rule 14)
- "encrypted" → SECURITY absolute edge (Rule 14)
- "card tokens" → SECURITY reinforcement edge (payment token = credentials vocabulary)
- "write integration tests" → TEST predicate edge (Rule 5)
- "document" + "runbook" → DOCUMENT edge; parallel-eligible flag on DocumentationFlow21
- "subscription records" → 4.4 data vocabulary (subscription = subscription billing domain)

**Key disambiguation applied:**
- "billing" alone does NOT fire ARCHITECTURE. "billing ledger schema structure" fires via "structure" only.
- "payment credentials" fires SECURITY via "credentials" (absolute). "payment" is contextual — it reinforces the security concern but "credentials" carries the absolute edge.
- "card tokens" are payment tokens → treated as credentials vocabulary → SECURITY reinforcement.
- "encrypted at rest" → "encrypted" fires SECURITY. "at rest" is a qualifier, not a separate trigger.
- "verify" in "verify all subscription records migrated correctly" does NOT independently fire TEST. TEST fires from "write integration tests" — that phrase is the TEST trigger. "verify" in data context is a migration action, not a test predicate.
- Multiple SECURITY triggers (credentials, encrypted, card tokens) → single SECURITY predicate.

---

### Layer 0-D: Flow Factory

**Factory mode determination:**
- SubscriptionBillingMigration — five-predicate migration + architecture + security + test + document
- Known five-predicate pattern (matches GdprUserDataMigration template, new domain context)
- Factory mode: trusted_learned (five-predicate full-stack migration is a recognized scenario)

**Factory output:**
```
FlowFactoryEntry {
  ScenarioId: "SubscriptionBillingMigration",
  Flows: [
    "ArchitectureFlow31",
    "DataMigrationFlow44",
    "SecurityReviewFlow",
    "UnitTestingFlow51",
    "DocumentationFlow21"
  ],
  TransitionType: "feature_addition",
  FactoryMode: "trusted_learned",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete", condition: "billing_ledger_schema_spec_approved"},
    {flow: "DataMigrationFlow44", type: "row_count_assertion", condition: "count(billing_ledger) == count(subscription_billing)"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass", condition: "no_plaintext_credentials AND all_card_tokens_encrypted"},
    {flow: "UnitTestingFlow51", type: "test_pass_assertion", condition: "all_integration_tests_pass"},
    {flow: "DocumentationFlow21", type: "doc_complete", condition: "migration_runbook_published"}
  ]
}
```

---

### Layer 1: Transition Orchestrator

**Ordering input:** [ArchitectureFlow31, DataMigrationFlow44, SecurityReviewFlow, UnitTestingFlow51, DocumentationFlow21]

**Orchestration rules applied:**
- Rule: ARCHITECTURE first (billing ledger schema spec required before migration)
- Rule: IMPLEMENT after ARCHITECTURE (migration consumes schema spec)
- Rule: SECURITY after IMPLEMENT (encryption verification requires migrated data)
- Rule: TEST after IMPLEMENT (integration tests verify migrated data)
- Rule: SECURITY and TEST are parallel-eligible after IMPLEMENT
- Rule: DOCUMENT (runbook) parallel-eligible from IMPLEMENT onward
- Rule: No QA predicate → QA last rule does not apply

**Execution schedule:**
1. ArchitectureFlow31 — design billing ledger schema structure; output: billing_ledger schema spec
2. DataMigrationFlow44 — migrate subscription billing records to billing_ledger per schema spec
3. SecurityReviewFlow ‖ UnitTestingFlow51 — parallel after step 2
   - SecurityReviewFlow: scan for plaintext credentials; verify card tokens encrypted
   - UnitTestingFlow51: run integration tests verifying record count and data integrity
4. DocumentationFlow21 — publish migration runbook (parallel-eligible from step 2; may complete alongside step 3)

**Metadata output:**
```
{
  TransitionType: "feature_addition",
  DomainId: "software-development",
  FlowCount: 5,
  ParallelPairs: [
    ["SecurityReviewFlow", "UnitTestingFlow51"],
    ["DataMigrationFlow44", "DocumentationFlow21"]
  ],
  GateBCount: 5
}
```

---

### Layer 2: Nine-Node Trace (DataMigrationFlow44 — primary flow)

**N01 — Input:** "migrate subscription billing records to billing_ledger; design schema structure; encrypt credentials; write integration tests; document runbook"

**N02 — Scope:** SubscriptionBillingMigration; source: subscription_billing (920K rows); target: billing_ledger; TransitionType: feature_addition

**N03 — Analysis:**
- subscription_billing: 920K rows; columns: subscription_id, customer_id, amount_cents, billing_date, status, card_token, payment_credentials_json
- Sensitive fields: card_token (requires encryption); payment_credentials_json (requires encryption at rest)
- Target schema: billing_ledger with encrypted credential columns per ArchitectureFlow31 spec
- Batch strategy: 50K rows per batch (18–19 batches); estimated duration: 12 minutes

**N04 — Specification:**
- SubscriptionBillingMigration_up.sql (INSERT billing_ledger SELECT from subscription_billing with encryption transform)
- SubscriptionBillingMigration_down.sql (rollback)
- SubscriptionBillingMigration_verify.sql (row count + credential field encryption validation)
- BillingLedgerSchemaSpec.md (ArchitectureFlow31 output)
- BillingMigrationIntegrationTests.cs (UnitTestingFlow51 output)
- SecurityScanReport.md (SecurityReviewFlow output)
- MigrationRunbook.md (DocumentationFlow21 output)

**N05 — Generation:** INSERT ... SELECT with credential encryption applied at migration time; card_token encrypted via application-level key before insert

**N06 — Execution:** run 19 batches; validate encrypted field format per batch; capture row counts

**N07 — Pre-Gate:** regression check; confirm subscription_id and customer_id FK integrity; confirm no plaintext credential fields; TransitionType: feature_addition confirmed

**N08 — Gate B (five evaluations):**
1. ArchitectureFlow31: `billing_ledger_schema_spec_approved == true` → PASS
2. DataMigrationFlow44: `count(billing_ledger) == 920,000` → PASS
3. SecurityReviewFlow: `no_plaintext_credentials AND all_card_tokens_encrypted == true` → PASS
4. UnitTestingFlow51: `all_integration_tests_pass == true` → PASS
5. DocumentationFlow21: `migration_runbook_published == true` → PASS

**N09 — Output:** SubscriptionBillingMigration delivered; all five Gate B evaluations PASS; 7 files generated

---

### Layer 3: Teaching Points

**Core lessons from SubscriptionBillingMigration:**

**Lesson 1:** "credentials" is an absolute SECURITY trigger. It fires SECURITY regardless of what surrounds it. "payment credentials", "card credentials", "API credentials" — all fire SECURITY via the "credentials" token.

**Lesson 2:** Multiple SECURITY triggers in one request collapse to one SECURITY predicate. "credentials" + "encrypted" + "card tokens" all fire SECURITY — the output contains exactly one SECURITY predicate. Predicates do not duplicate.

**Lesson 3:** Five predicates = five flows = five Gate B evaluations. The SECURITY Gate B condition is specific: it names the sensitive fields and the required state (encrypted, no plaintext). "security review complete" is not a valid Gate B — a condition must be stated as a comparison or assertion.

**Lesson 4:** SECURITY and TEST are parallel-eligible after IMPLEMENT. Neither requires the other to complete first. The orchestrator can schedule them in parallel to reduce total execution time.

**Lesson 5:** "verify" in data migration context is NOT a TEST trigger. "verify all subscription records migrated correctly" uses "verify" as a migration validation action. TEST fires only from "write integration tests" — the specific phrase pattern. "verify" alone is neutral in migration vocabulary.

**Lesson 6:** DocumentationFlow21 for runbook documentation has TWO parallel-eligibility windows: (a) parallel with DataMigrationFlow44 during migration, and (b) parallel with SecurityReviewFlow + UnitTestingFlow51 post-migration. It is never forced to wait for all flows to complete.

---

### Cross-Layer Invariants Verified

| Layer | Input | Output | Consistent? |
|---|---|---|---|
| 0-A Compiler | "migrate" + "design" + "structure" + "credentials" + "encrypted" + "write tests" + "document" | 5 predicates; feature_addition | YES |
| 0-B Router | 5 predicates; 4.4 + 3.1 + security + 5.x + 2.1 | 5 flows; SEC+TEST parallel; DOC parallel-eligible | YES |
| 0-C Knowledge | "credentials" + "encrypted" absolute edges; "structure" absolute; "verify" neutral | SECURITY fires 3×→1 predicate; ARCH fires 2×→1 predicate | YES |
| 0-D Factory | SubscriptionBillingMigration; 5 predicates | trusted_learned; 5 Gate B entries with specific conditions | YES |
| L1 Orchestrator | 5 flows | ARCH→IMPL→SEC‖TEST; DOC parallel eligible | YES |
| L2 Nine-Node | 920K rows; 5 Gate B evaluations | all PASS; 7 files; credentials encrypted | YES |
| L3 Teaching | "credentials" absolute; multi-trigger collapse; "verify"≠TEST; DOC dual-window | 6 lessons covering all five-predicate edge cases | YES |
