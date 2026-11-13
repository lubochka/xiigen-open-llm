# Layer 0-B — Router Pairs (012)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-012-001 — Complete B18 routing decision table

**All predicate combinations with sub-phase selections:**

| Predicates | TransitionType | Primary Flow | Secondary Flows | Sub-phases |
|---|---|---|---|---|
| [IMPLEMENT] | refactoring | BackendServiceFlow41 | — | 4.1 |
| [IMPLEMENT] | feature_addition | DataMigrationFlow44 | — | 4.4 |
| [IMPLEMENT, ARCHITECTURE] | refactoring | BackendServiceFlow41 | ArchitectureFlow31 | 4.1 + 3.1 |
| [IMPLEMENT, ARCHITECTURE] | feature_addition | DataMigrationFlow44 | ArchitectureFlow31 | 4.4 + 3.1 |
| [IMPLEMENT, TEST] | refactoring | BackendServiceFlow41 | UnitTestingFlow51 | 4.1 + 5.x |
| [IMPLEMENT, TEST] | feature_addition | DataMigrationFlow44 | UnitTestingFlow51 | 4.4 + 5.x |
| [IMPLEMENT, DOCUMENT] | refactoring | BackendServiceFlow41 | DocumentationFlow21 | 4.1 + 2.1 |
| [IMPLEMENT, DOCUMENT] | feature_addition | DataMigrationFlow44 | DocumentationFlow21 | 4.4 + 2.1 |
| [IMPLEMENT, SECURITY] | feature_addition | DataMigrationFlow44 | SecurityReviewFlow | 4.4 + security |
| [IMPLEMENT, ARCHITECTURE, TEST] | feature_addition | DataMigrationFlow44 | ArchitectureFlow31 + UnitTestingFlow51 | 4.4 + 3.1 + 5.x |
| [IMPLEMENT, ARCHITECTURE, TEST] | refactoring | BackendServiceFlow41 | ArchitectureFlow31 + UnitTestingFlow51 | 4.1 + 3.1 + 5.x |
| [IMPLEMENT, ARCHITECTURE, DOCUMENT] | feature_addition | DataMigrationFlow44 | ArchitectureFlow31 + DocumentationFlow21 | 4.4 + 3.1 + 2.1 |
| [IMPLEMENT, DOCUMENT] | feature_addition | DataMigrationFlow44 | DocumentationFlow21 | 4.4 + 2.1 |
| [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] | feature_addition | DataMigrationFlow44 | ArchitectureFlow31 + SecurityReviewFlow + UnitTestingFlow51 + DocumentationFlow21 | 4.4 + 3.1 + security + 5.x + 2.1 |

---

## B18-0B-012-002 — Flow selection rules

**DataMigrationFlow44 is selected when:**
- IMPLEMENT predicate present
- TransitionType: feature_addition
- Sub-phase vocabulary: migrate, migration, seed, fixture, ETL, transform (data), load (data), data, records, database, table

**BackendServiceFlow41 is selected when:**
- IMPLEMENT predicate present
- TransitionType: refactoring
- Sub-phase vocabulary: service, class, function, module, repository, domain, business, rule, handler

**TransitionType determines the IMPLEMENT flow:**
- refactoring → BackendServiceFlow41
- feature_addition → DataMigrationFlow44

---

## B18-0B-012-003 — B18 flow inventory

**Available flows in B18:**

| Flow | Sub-phase | Triggered by |
|---|---|---|
| DataMigrationFlow44 | 4.4 | IMPLEMENT (feature_addition) |
| BackendServiceFlow41 | 4.1 | IMPLEMENT (refactoring) |
| ArchitectureFlow31 | 3.1 | ARCHITECTURE |
| UnitTestingFlow51 | 5.x | TEST |
| DocumentationFlow21 | 2.1 | DOCUMENT |
| SecurityReviewFlow | security | SECURITY |

**Not available in B18:**
- GapHandler (no gap vocabulary in B18)
- ConnectorFlow43 (4.3 — rare, only when explicit external API integration vocabulary present)
- QAFlow (only fires on "QA" abbreviation)

---

## B18-0B-012-004 — Execution ordering rules for B18

**General ordering rules:**

1. **ARCHITECTURE before IMPLEMENT:** When both present, schema/design comes before implementation
2. **TEST after IMPLEMENT:** Tests verify implementation; cannot precede it
3. **DOCUMENT:** Flexible — can be parallel to IMPLEMENT (pre/spec documentation) or sequential (as-built documentation)
4. **SECURITY:** Can be concurrent with IMPLEMENT (review during development) or after (review complete implementation)

**Orchestrator receives these ordering constraints from the router.**

---

## B18-0B-012-005 — DomainId and routing context

**B18 routing context:**
- DomainId: "software-development" (always)
- Scenario: "B18 — Development — refactoring and data migration"
- No external service integrations (4.3) unless explicit API vocabulary present
- No gap-handling (no GapHandler)
- Primary sub-phases: 4.4 (data migration) and 4.1 (code refactoring)
- Secondary sub-phases: 3.1 (architecture), 5.x (testing), 2.1 (documentation), security

---

## B18-0B-012-006 — Router output schema template

**B18 router always produces this structure:**

```
RouterOutput {
  connector_name: string,        // Name of the flow/migration/service
  domain_id: "software-development",
  transition_type: "refactoring" | "feature_addition",
  predicate_routing: [
    {
      predicate: string,         // IMPLEMENT, ARCHITECTURE, TEST, etc.
      flow: string,              // Flow name
      sub_phase: string          // 4.4, 4.1, 3.1, 5.x, 2.1, etc.
    }
  ],
  execution_order: string[],     // Ordered list of flows
  parallel_eligible: string[]    // Flows that can run concurrently
}
```
