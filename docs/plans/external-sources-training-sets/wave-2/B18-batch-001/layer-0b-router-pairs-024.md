# Layer 0-B — Router Pairs (024)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-024-001 — Complete B18 router: all flow activations by predicate

**Full predicate-to-flow routing table:**

| Predicate | Sub-phase | Flow activated |
|---|---|---|
| IMPLEMENT (feature_addition, 4.4) | 4.4 | DataMigrationFlow44 |
| IMPLEMENT (refactoring, 4.1) | 4.1 | BackendServiceFlow41 |
| IMPLEMENT (refactoring, 4.4 tables) | 4.4 | DataMigrationFlow44 |
| IMPLEMENT (feature_addition, 4.3) | 4.3 | ApiContractFlow43 |
| ARCHITECTURE | 3.1 | ArchitectureFlow31 |
| SECURITY | security | SecurityReviewFlow |
| TEST | 5.x | UnitTestingFlow51 |
| DOCUMENT | 2.1 | DocumentationFlow21 |

---

## B18-0B-024-002 — Complete B18 router: execution ordering rules

**Flow execution ordering (from Orchestrator):**

1. **ARCHITECTURE (ArchitectureFlow31)** — always first; schema design prerequisite
2. **IMPLEMENT (DataMigrationFlow44 or BackendServiceFlow41)** — second; primary artifact
3. **SECURITY (SecurityReviewFlow)** — third; reviews implementation output
4. **TEST (UnitTestingFlow51)** — third or fourth; hard dependency on IMPLEMENT
5. **DOCUMENT (DocumentationFlow21)** — parallel-eligible; can run with step 2-4

**QA:** always last if present; fires only on "QA" abbreviation; never implied.

---

## B18-0B-024-003 — Complete B18 router: parallel eligibility rules

**Parallel-eligible combinations:**
- DocumentationFlow21 with DataMigrationFlow44 (for spec/policy/runbook documentation)
- SecurityReviewFlow with UnitTestingFlow51 (after IMPLEMENT completes)
- DocumentationFlow21 with SecurityReviewFlow

**Never parallel:**
- ArchitectureFlow31 with DataMigrationFlow44 (schema must be complete before migration)
- UnitTestingFlow51 before DataMigrationFlow44 (tests verify migration; hard dependency)
- SECURITY before IMPLEMENT (cannot review what hasn't been built)

---

## B18-0B-024-004 — Complete B18 router: sub-phase disambiguation

**When both 4.1 and 4.4 vocabulary are present:**
- Both BackendServiceFlow41 (4.1) AND DataMigrationFlow44 (4.4) activate
- TransitionType applies to BackendServiceFlow41 if refactoring; DataMigrationFlow44 always feature_addition for data migrations

**When 4.4 and 4.3 vocabulary are present:**
- Both DataMigrationFlow44 (4.4) AND ApiContractFlow43 (4.3) activate

**Single flow is the default:**
- If only one sub-phase vocabulary is present → only that flow activates

---

## B18-0B-024-005 — Complete B18 router: five anti-patterns to avoid

**Five routing errors the v6 classifier makes:**

| Error | Incorrect routing | Correct routing |
|---|---|---|
| v6-03: data vocabulary routed to 4.1 | seed → BackendServiceFlow41 | seed → DataMigrationFlow44 |
| ETL to 4.3 | ETL → ApiContractFlow43 | ETL → DataMigrationFlow44 |
| fixture to 5.x | fixture → UnitTestingFlow51 | fixture → DataMigrationFlow44 |
| ARCHITECTURE suppression | "structure" present → no ArchitectureFlow31 | Always route ArchitectureFlow31 |
| SECURITY suppression | PII present → no SecurityReviewFlow | Always route SecurityReviewFlow |
