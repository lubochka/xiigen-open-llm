# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: transition-orchestrator

## B18-L1-009-001 — QA last rule in migration context

**QA (Quality Assurance) flow in B18:**

QA fires ONLY on "QA" abbreviation in the request. It does NOT fire on:
- "quality" alone
- "quality assurance" spelled out (borderline — depends on context)
- "testing" (that is TEST, not QA)
- "verify" (that is TEST context, not QA)

**In B18 migration scenarios:** QA is almost never present. The standard verification is TEST (integration tests) not QA (quality assurance process).

---

## B18-L1-009-002 — If QA fires: it is ALWAYS last in B18 orchestration

**"QA last" rule:**
If QA fires (on "QA" abbreviation in request), the QAFlow is always the LAST flow to execute in the orchestration sequence.

**Reason:** QA validates the completed implementation. It cannot precede implementation or testing. QA is a final gate, not a parallel operation.

**B18 orchestration order when QA is present:**
1. ARCHITECTURE (schema/design)
2. IMPLEMENT (migration/refactoring)
3. SECURITY (if present)
4. TEST (unit/integration tests)
5. DOCUMENT (if present; may be parallel)
6. **QA (always last)**

---

## B18-L1-009-003 — Example: migration with QA

**Request (hypothetical):** "Write the user data migration, design the schema, write tests, and have the QA team verify the data integrity."

**Compiler trace:**
- "write migration" → IMPLEMENT, feature_addition
- "design schema" → ARCHITECTURE
- "write tests" → TEST
- "QA team verify" → QA fires (on "QA")
- TransitionType: feature_addition

**Orchestration sequence:**
1. ArchitectureFlow31 (schema design)
2. DataMigrationFlow44 (migration)
3. UnitTestingFlow51 (tests)
4. QAFlow (QA verification — LAST)

---

## B18-L1-009-004 — B18 migration context: Gate B vs TEST vs QA

**Three different verification mechanisms:**

**Gate B (N08 assertion):**
- Built into every flow
- Automatic comparison: count(target) == count(source)
- Not a separate flow; embedded in DataMigrationFlow44

**TEST (UnitTestingFlow51):**
- Fires on "write tests", "add tests", "integration tests"
- Generates test code
- Runs tests to verify migration behavior
- Separate flow from DataMigrationFlow44

**QA (QAFlow):**
- Fires on "QA" abbreviation
- External quality assurance process
- Manual or automated QA review
- Always last in orchestration

**These three are NOT synonymous.** Each has a distinct trigger and role.

---

## B18-L1-009-005 — "verify" in migration context: TEST vs Gate B

**"verify" can signal different things:**

1. **"verify row counts"** → this is Gate B language. IMPLEMENT fires; Gate B condition is row count. TEST does NOT fire unless "write tests" is also present.

2. **"write tests to verify"** → TEST fires. Integration tests verify the migration.

3. **"QA to verify"** → QA fires if "QA" abbreviation present.

4. **"verify the migration completed"** → Gate B language; no additional predicate fires.

**Teaching:** "verify" alone does NOT fire TEST. "write tests to verify" or "add tests to verify" fires TEST. The verb "write/add/create tests" is the TEST trigger.

---

## B18-L1-009-006 — Orchestration ordering summary for B18

**Complete B18 orchestration ordering rules:**

| Flow | Position | Dependency | Notes |
|---|---|---|---|
| ArchitectureFlow31 | First | None | Always first when present |
| DataMigrationFlow44 | After ARCHITECTURE | Depends on ArchitectureFlow31 | Primary migration flow |
| BackendServiceFlow41 | After ARCHITECTURE | Depends on ArchitectureFlow31 | Primary refactoring flow |
| SecurityReviewFlow | After IMPLEMENT | Depends on IMPLEMENT | Reviews completed implementation |
| UnitTestingFlow51 | After IMPLEMENT | Depends on IMPLEMENT | Tests after implementation |
| DocumentationFlow21 | Parallel/After | Optional dependency | Parallel-eligible for most docs |
| QAFlow | LAST | Depends on all others | Always last if present |
