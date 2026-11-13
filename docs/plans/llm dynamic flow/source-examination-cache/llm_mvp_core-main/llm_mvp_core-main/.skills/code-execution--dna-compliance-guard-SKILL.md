# DNA Compliance Guard Skill v1.0

## When to Invoke

- Before committing any new or modified TypeScript file in `server/src/`
- After the genesis station Genesis or the RAG update station RAG Update generates a `.ts` file
- When `generated-service-audit.md` detects a potential violation
- During Phase 11 code modifications (mandatory pre-commit gate)

---

## The 9 DNA Guard Patterns

See `dna-guard-patterns.md` for full detection commands, violation examples, fix patterns, and test templates.

| DNA | Rule | Detection |
|-----|------|-----------|
| coding rule 1 (no typed model classes) | No entity-specific model classes | `grep "class [A-Z].*{" server/src/` excl. kernel/, interfaces/ |
| coding rule 2 (no hardcoded selectors) | No hardcoded field selectors | `grep "\.find({" server/src/` |
| coding rule 3 (DataProcessResult pattern) | No business logic in error handlers | inspect catch blocks — must return DataProcessResult.failure() only |
| coding rule 4 (extends MicroserviceBase) | All services extend MicroserviceBase | `grep "class.*Service" server/src/` — check for extends |
| coding rule 5 (tenantId from context) | tenantId from context, not parameter | `grep "tenantId" server/src/` — check if it's a param |
| coding rule 6 (DynamicController) | No entity-specific controllers | `grep "@Controller(" server/src/` — check for entity specificity |
| coding rule 7 (setIfAbsent idempotency) | Event subscriptions have dedup ID | `grep "@Subscribe(" server/src/` — check dedup field |
| coding rule 8 (store before enqueue) | Document stored before queued | enqueue() calls — check storeDocument() precedes them |
| coding rule 9 (CloudEvents wrapper) | CloudEvents wrapper on all events | `grep "enqueue(" server/src/` — check CloudEvents wrapper |

---

## Pre-Commit Gate (run on every modified file)

```bash
FILE="<path-to-modified-file>"

# DNA-1
grep "class [A-Z].*{" $FILE | grep -v "extends\|interface\|abstract"

# DNA-2
grep "\.find({" $FILE

# DNA-4
grep "class.*Service" $FILE | grep -v "extends MicroserviceBase"

# DNA-5
grep -n "tenantId" $FILE
# If tenantId appears as a function parameter → violation

# DNA-6
grep "@Controller(" $FILE
# If controller maps to entity-specific route → review

# DNA-9
grep "enqueue(" $FILE
# Check each call — must pass CloudEvents-wrapped object
```

Expected: all sections empty for a clean file.

---

## Quick Verdicts

| Observation | DNA Rule | Class |
|-------------|----------|-------|
| `class OrderModel {}` in new service | coding rule 1 (no typed model classes) | C (+ D if from generation) |
| `.find({ userId: input.userId })` | coding rule 2 (no hardcoded selectors) | C |
| `catch (e) { if (e.type) { ... } }` | coding rule 3 (DataProcessResult pattern) | C |
| `class OrderService {}` (no extends) | coding rule 4 (extends MicroserviceBase) | C |
| `async process(tenantId: string, ...)` | coding rule 5 (tenantId from context) | C |
| `@Controller('orders')` | coding rule 6 (DynamicController) | C |
| `@Subscribe('order.completed')` without dedup | coding rule 7 (setIfAbsent idempotency) | C |
| `enqueue(event)` before `storeDocument()` | coding rule 8 (store before enqueue) | C |
| `enqueue({ orderId, type })` (no CloudEvents) | coding rule 9 (CloudEvents wrapper) | C |

---

## When a Violation Is Found

1. Do NOT commit the violating file
2. Classify the bug (engine-qa-skill): CLASS C if violation in output; CLASS D if traceable to station choice
3. If generated code: fix the AF station (af1-genesis.ts or af7-rag-update.ts) — not the generated file
4. If hand-written code: fix directly
5. Write 3 failing tests before fix (bug-to-tests-skill)
6. Re-run pre-commit gate → must be clean

---

## Integration

```
dna-compliance-guard
  → pre-commit gate → blocks violating commits
  → generated-service-audit (mental-debug Rule 12) → same checklist, different trigger
  → engine-qa (CLASS C or D classification)
  → bug-to-tests → 3 failing tests before fix
```
