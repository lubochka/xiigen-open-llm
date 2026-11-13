# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-013-001 — AddressBook: single-flow orchestration

**Scenario:** AddressBookMigration — IMPLEMENT only, feature_addition, 4.4

**Orchestrator sequence:**
```
1. [DataMigrationFlow44]
   → Execute AddressBookMigration
   → N08 Gate B: count(addresses + phone_numbers + emails) == count(legacy_contacts)
   → N09 output
```

No parallelism. No dependency management needed. Single flow.

---

## B18-L1-013-002 — AddressBook: IMPLEMENT+ARCHITECTURE ordering

**Scenario:** IMPLEMENT+ARCHITECTURE — schema design before migration

**Orchestration sequence:**
```
1. ArchitectureFlow31 — design normalized address/phone/email schema
2. DataMigrationFlow44 — migrate contact data per schema spec
3. N08 evaluates: schema complete? migration count matches?
4. N09 output
```

**Dependency:** DataMigrationFlow44 depends_on ArchitectureFlow31.
No parallel execution possible (sequential dependency).

---

## B18-L1-013-003 — AddressBook: IMPLEMENT+SECURITY ordering

**Orchestration sequence:**
```
1. DataMigrationFlow44 — migrate address records
2. SecurityReviewFlow — review PII handling in migrated data
3. N08 evaluates both Gate B conditions
4. N09 output
```

**Ordering rule:** SECURITY reviews after IMPLEMENT completes.

---

## B18-L1-013-004 — AddressBook: refactoring service + migration — parallel question

**Scenario:** BackendServiceFlow41 (4.1) + DataMigrationFlow44 (4.4)

**Can they run in parallel?**
- Depends on coupling: if the service refactoring changes how address data is inserted, migration must wait
- If they are independent (different concerns), parallel is possible
- **B18 conservative default:** sequential (service refactoring first, then migration)

**Orchestration sequence:**
```
1. BackendServiceFlow41 — refactor AddressService
2. DataMigrationFlow44 — migrate address records (uses refactored service)
3. N08: service tests pass AND migrated count matches
```

---

## B18-L1-013-005 — AddressBook: no QA unless "QA" abbreviation present

**QA rule reminder:**
- "QA" abbreviation in request → QA flow always last in orchestration
- "verify", "integration tests", "review" do NOT trigger QA
- AddressBookMigration scenarios: QA does not fire unless explicitly requested with "QA" keyword

**All AddressBook scenarios in B18-batch-001 have no QA predicate.**
