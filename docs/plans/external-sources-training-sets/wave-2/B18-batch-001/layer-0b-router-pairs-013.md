# Layer 0-B — Router Pairs (013)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-013-001 — AddressBook: IMPLEMENT (4.4) → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (address records, contacts table vocabulary)
- ConnectorName: AddressBookMigration

**Routing decision:**
- IMPLEMENT + feature_addition + 4.4 vocabulary → DataMigrationFlow44
- No secondary routing (single predicate, single sub-phase)

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-013-002 — AddressBook + ARCHITECTURE → two flows

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (address data) + 3.1 (structure/redesign)
- ConnectorName: AddressBookMigration

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- Ordering: ArchitectureFlow31 first (schema design precedes migration)

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44], sub-phases: [3.1, 4.4]

---

## B18-0B-013-003 — AddressBook: "refactor" + "migrate" → refactoring; 4.1 + 4.4

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signals: 4.1 (AddressService vocabulary) + 4.4 (migrate, address records)
- ConnectorName: AddressServiceRefactoring+Migration

**Routing decision:**
- IMPLEMENT + refactoring + 4.1 vocabulary → BackendServiceFlow41
- IMPLEMENT + 4.4 vocabulary also present → DataMigrationFlow44 also routes
- Both sub-phases fire when both vocabulary sets present

**Output:** flows: [BackendServiceFlow41, DataMigrationFlow44], sub-phases: [4.1, 4.4]

---

## B18-0B-013-004 — AddressBook routing: "address data" always routes to 4.4, not 4.1

**Routing disambiguation:**

| Input vocabulary | Sub-phase | Flow |
|---|---|---|
| "AddressService", "class", "method" | 4.1 | BackendServiceFlow41 |
| "address records", "contacts table", "addresses" | 4.4 | DataMigrationFlow44 |
| Both present | 4.1 + 4.4 | BackendServiceFlow41 + DataMigrationFlow44 |

**Key rule:** Data vocabulary (records, tables, data) always activates 4.4 routing. Service vocabulary activates 4.1. Co-occurrence activates both.

---

## B18-0B-013-005 — AddressBook: IMPLEMENT+SECURITY → DataMigrationFlow44 + SecurityReviewFlow

**Input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (address records) + security (PII vocabulary)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow (security sub-phase)
- Ordering: DataMigrationFlow44 first; SecurityReviewFlow after (reviews migrated data)

**Output:** flows: [DataMigrationFlow44, SecurityReviewFlow], sub-phases: [4.4, security]
