# Layer 2 — Nine-Node Flow Pairs (sub-phase 4.4)
# Batch: B18-batch-001
# Scenario: B18 — Development — refactoring and data migration
# Flow: DataMigrationFlow44
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: nine_node

---

## N01 — DomainSplitterUnit

```
source_id: xiigen-b18-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node: 01
```

A:
  TransitionId: "transition-B18-t1-002-4.4-feature_addition"
  UserRequest: "Write the database migration to move customer data from the legacy schema to the new normalized structure."
  PredicateIds: ["IMPLEMENT", "ARCHITECTURE"]

B:
  DomainPackets:
    - PacketId: "pkt-B18-001-impl"
      Domain: "IMPLEMENT"
      Focus: "database migration authoring and execution"
      InputSummary: "Customer data migration: legacy schema → normalized structure"
      PredicateOwner: "IMPLEMENT"
  PacketCount: 1

---

## N02 — DomainTranslatorUnit

```
source_id: xiigen-b18-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node: 02
```

A: [IMPLEMENT DomainPacket]

B:
  StatementId: "stmt-B18-001-impl"
  DomainStatement: "Migrate customer data from LegacyDb.Customers (flat, denormalized) to
    NormalizedDb: Customers + CustomerAddresses + CustomerContacts. Migration steps: (1)
    read legacy records in batches of 500; (2) split flat address fields into CustomerAddresses
    rows; (3) split flat contact fields into CustomerContacts rows; (4) write Customer root
    with foreign key references; (5) verify row counts match. Non-functional: idempotent
    (re-runnable via migration status table), transactional per batch, rollback on batch
    failure, progress logging per 500 records."
  TranslationConfidence: 0.92

---

## N03 — SchemePlannerUnit

```
source_id: xiigen-b18-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node: 03
```

A: [TranslatedDomainStatement from N02]

B:
  WorkBranches:
    - BranchId: "branch-B18-schema-creation"
      BranchName: "Target Schema Creation"
      Description: "DDL for Customers, CustomerAddresses, CustomerContacts tables"
      Priority: 1

    - BranchId: "branch-B18-data-transform"
      BranchName: "Data Transformation Logic"
      Description: "Batch reader, field splitter, denormalized→normalized row mapper"
      Priority: 2

    - BranchId: "branch-B18-migration-runner"
      BranchName: "Migration Runner"
      Description: "Idempotent runner: status table check, batch execution, rollback, progress logging"
      Priority: 3

    - BranchId: "branch-B18-verification"
      BranchName: "Post-Migration Verification"
      Description: "Row count assertions, FK integrity checks, sample record spot checks"
      Priority: 4

  BranchCount: 4

---

## N04 — DomainProcessorUnit

```
source_id: xiigen-b18-n04
contract_signature: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
node: 04
```

A: [DATA-TRANSFORM WorkBranch]
  BranchId: "branch-B18-data-transform"

B:
  BranchId: "branch-B18-data-transform"
  ProcessedItems:
    - ItemId: "mig-B18-T01"
      Name: "LegacyCustomerReader"
      Specification: "IAsyncEnumerable<LegacyCustomer> ReadBatchAsync(int batchSize, int offset)"

    - ItemId: "mig-B18-T02"
      Name: "CustomerNormalizer"
      Specification: "NormalizedCustomerSet Normalize(LegacyCustomer legacy): splits AddressLine1/2/City/PostCode into CustomerAddress; splits Phone/Email into CustomerContact"
      BusinessRule: "BR-MIG-001: Empty address fields produce null CustomerAddress (not empty row)"

    - ItemId: "mig-B18-T03"
      Name: "NormalizedCustomerWriter"
      Specification: "OperationResult WriteAsync(NormalizedCustomerSet record) — transactional: Customer + Address + Contact in single transaction"
      BusinessRule: "BR-MIG-002: If CustomerId already exists in target, skip (idempotency); log as SKIPPED"

  OverallBranchStatus: "PROCESSED"

---

## N05 — ApplierUnit

```
source_id: xiigen-b18-n05
contract_signature: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
node: 05
```

A: [ProcessedBranchResult from N04]

B:
  BranchId: "branch-B18-data-transform"
  AppliedItems:
    - ActionId: "apply-B18-A01"
      Action: "Generate LegacyCustomerReader with batch-aware async enumerable"
      Result: "Generated: Migrations/CustomerMigration/LegacyCustomerReader.cs"

    - ActionId: "apply-B18-A02"
      Action: "Generate CustomerNormalizer with address/contact field splitting"
      Result: "Generated: Migrations/CustomerMigration/CustomerNormalizer.cs"

    - ActionId: "apply-B18-A03"
      Action: "Generate NormalizedCustomerWriter with transactional write and idempotency"
      Result: "Generated: Migrations/CustomerMigration/NormalizedCustomerWriter.cs"

  AppliedCount: 3
  BranchStatus: "APPLIED"

---

## N06 — ResultTranslatorUnit

```
source_id: xiigen-b18-n06
contract_signature: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
node: 06
```

A: [All 4 AppliedBranchResults]

B:
  ResultFragments:
    - FragmentId: "frag-B18-migration-summary"
      FragmentType: "migration_summary"
      Content:
        migration_name: "CustomerNormalizationMigration"
        files_generated: 7
        tables_created: 3
        business_rules: 4
        idempotent: true
        batch_size: 500
        rollback_supported: true

    - FragmentId: "frag-B18-verification-plan"
      FragmentType: "verification_plan"
      Content:
        checks:
          - "legacy_count == customers_count — row count parity"
          - "all customers have at least one CustomerAddress or null — no orphan addresses"
          - "FK integrity: CustomerAddresses.CustomerId references Customers.Id"
          - "spot check: 10 random records manually verified pre/post"

  FragmentCount: 2

---

## N07 — ArbiterSelectorUnit

```
source_id: xiigen-b18-n07
contract_signature: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
node: 07
```

A: [2 ResultFragments]

B:
  ArbiterRequests:
    - RequestId: "arb-B18-001"
      ArbiterType: "migration-completeness-arbiter"
      FragmentId: "frag-B18-migration-summary"
      ReviewFocus: "Verify all 4 branches implemented, idempotency present, rollback supported"

    - RequestId: "arb-B18-002"
      ArbiterType: "data-integrity-arbiter"
      FragmentId: "frag-B18-verification-plan"
      ReviewFocus: "Verify row count parity check and FK integrity check are both present"

  RequestCount: 2

---

## N08 — ArbiterUnit

```
source_id: xiigen-b18-n08
contract_signature: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
node: 08
```

A: [migration-completeness-arbiter]
  RequestId: "arb-B18-001"

B:
  RequestId: "arb-B18-001"
  Verdict: "PASS"
  Confidence: 0.95
  Findings:
    - "Schema creation branch: implemented — PASS"
    - "Data transform branch: implemented — PASS"
    - "Migration runner: idempotent via status table — PASS"
    - "Verification: row count + FK checks present — PASS"
    - "Rollback: per-batch transaction with rollback — PASS"
  VerdictReason: "Migration complete: all 4 branches implemented with idempotency, rollback, and verification."

---

## N09 — FormatterUnit

```
source_id: xiigen-b18-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node: 09
```

A:
  ReviewedResultSet:
    OverallVerdict: "PASS"

B:
  HumanResult:
    Title: "CustomerNormalizationMigration — data migration complete"
    OverallVerdict: "PASS"
    Summary: "Customer data migration from legacy flat schema to normalized Customers/CustomerAddresses/CustomerContacts structure. Idempotent batch migration (500 records/batch) with rollback and post-migration verification."
    FilesGenerated:
      - "Migrations/CustomerMigration/LegacyCustomerReader.cs"
      - "Migrations/CustomerMigration/CustomerNormalizer.cs"
      - "Migrations/CustomerMigration/NormalizedCustomerWriter.cs"
      - "Migrations/CustomerMigration/MigrationRunner.cs"
      - "Migrations/CustomerMigration/MigrationVerifier.cs"
      - "Database/Schema/CustomerAddresses.sql, CustomerContacts.sql"
    BusinessRules: ["BR-MIG-001 (null vs empty address)", "BR-MIG-002 (idempotency skip)", "BR-MIG-003 (batch rollback)", "BR-MIG-004 (row count parity gate)"]
  Metrics:
    node_01_unit_id: "DomainSplitterUnit"
    node_02_unit_id: "DomainTranslatorUnit"
    node_03_unit_id: "SchemePlannerUnit"
    node_04_unit_id: "DomainProcessorUnit"
    node_05_unit_id: "ApplierUnit"
    node_06_unit_id: "ResultTranslatorUnit"
    node_07_unit_id: "ArbiterSelectorUnit"
    node_08_unit_id: "ArbiterUnit"
    node_09_unit_id: "FormatterUnit"
    files_generated: 7
    tables_created: 3
    sub_phase: "4.4"
    flow_id: "DataMigrationFlow44"
