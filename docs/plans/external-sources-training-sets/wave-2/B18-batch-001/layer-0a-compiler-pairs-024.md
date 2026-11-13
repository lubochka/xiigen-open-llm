# Layer 0-A — Compiler Pairs (024)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-024-001 — Complete predicate rule reference: IMPLEMENT triggers (extended)

**All IMPLEMENT trigger words and their TransitionType + sub-phase signals (B18 complete set):**

### Refactoring-vocabulary IMPLEMENT triggers (TransitionType: refactoring)

| Trigger word/phrase | Sub-phase signal | Notes |
|---|---|---|
| refactor | 4.1 | Rule 9: absolute; always sets refactoring |
| extract (code target) | 4.1 | Code class/method target; data extract → 4.4 |
| rename | 4.1 or 4.4 | Columns or code → sub-phase by vocabulary |
| consolidate | 4.1 or 4.4 | Tables or classes → sub-phase by vocabulary |
| restructure | 4.1 or 4.4 | Code or data tables → sub-phase by vocabulary |
| reorganize | 4.1 | Service/module vocabulary |
| move (code) | 4.1 | Code class/file target |
| clean up | 4.1 | Code cleanup |
| merge (code) | 4.1 | Service/class merge |

### Feature_addition-vocabulary IMPLEMENT triggers (TransitionType: feature_addition)

| Trigger word/phrase | Sub-phase signal | Notes |
|---|---|---|
| write migration | 4.4 | Rule 2: new migration artifact; absolute |
| create migration | 4.4 | Rule 2 |
| build migration | 4.4 | Rule 2 |
| seed | 4.4 | Rule 4 |
| fixture | 4.4 | Rule 4 |
| migrate (data) | 4.4 | Rule 5; data vocabulary present |
| transform (data) | 4.4 | Rule 5; ETL context |
| ETL | 4.4 | Rule 5 |
| load (data) | 4.4 | Rule 5 |
| populate | 4.4 | Seeding-equivalent |
| rebuild (data/index) | 4.4 | Seed/reload equivalent |

---

## B18-0A-024-002 — Complete predicate rule reference: ARCHITECTURE triggers

**ARCHITECTURE trigger words (always additive — fires as second predicate):**

| Trigger word/phrase | Absolute or contextual | Rule |
|---|---|---|
| structure | Absolute (no exceptions) | Rule 3 |
| architecture | Absolute | Rule 7 |
| pattern | Absolute | Rule 7 |
| design (structural noun) | Contextual (must be structural/schema use) | Rule 7/11 |
| redesign | Absolute (implies architectural intent) | Rule 7 |
| schema (+ structural noun) | Contextual only | Rule 16 |

**ARCHITECTURE never fires on:**
- "normalize" / "normalization" (action verbs describing data shape)
- "structured" (adjective)
- "data model" (unless "structure" or "architecture" also present)
- "class design" (code design; only fires ARCHITECTURE if "architecture" or "pattern" or "structure" also present)

---

## B18-0A-024-003 — Complete predicate rule reference: SECURITY triggers

**SECURITY trigger words (all absolute in B18):**

| Trigger word/phrase | Notes |
|---|---|
| PII | Personally Identifiable Information |
| encrypted / encryption | Any encryption vocabulary |
| GDPR | General Data Protection Regulation |
| compliance | Regulatory compliance vocabulary |
| credentials | API keys, passwords, tokens |
| authentication | Auth flow vocabulary |
| authorization | Access control vocabulary |
| sensitive data | Explicit sensitivity label |
| access control | Permission/role vocabulary |
| hashed / hash | Cryptographic hashing |
| anonymize / anonymization | PII removal (fires only with PII or GDPR co-present) |

---

## B18-0A-024-004 — Complete predicate rule reference: TEST and DOCUMENT triggers

**TEST triggers (verb form required):**

| Trigger | Fires TEST? | Notes |
|---|---|---|
| write tests | YES | Rule 12 |
| add tests | YES | Rule 12 |
| create tests | YES | Rule 12 |
| write integration tests | YES | Rule 12 |
| write unit tests | YES | Rule 12 |
| tests (standalone noun) | NO | Noun without write/add/create verb |
| integration testing | NO | Noun form; does not fire TEST |
| verify | NO | Verification is part of Gate B, not TEST predicate |

**DOCUMENT triggers:**

| Trigger | Fires DOCUMENT? |
|---|---|
| document | YES |
| spec out | YES |
| write spec | YES |
| annotate | YES |
| describe | YES |
| write a runbook | YES |
| publish docs | YES |
| runbook | YES (with document/write co-present) |

---

## B18-0A-024-005 — Complete predicate rule reference: TransitionType hierarchy and conflict resolution

**TransitionType hierarchy (Rule 8):**
1. refactoring (highest priority)
2. feature_addition
3. documentation
4. no contribution
5. none (lowest priority)

**Conflict resolution cases:**

| Scenario | TransitionType | Rule |
|---|---|---|
| "refactor" present | refactoring | Rule 9, absolute |
| "write migration" + "consolidate" | feature_addition | Rule 2 wins: new artifact |
| "refactor" + "migrate data" | refactoring | Rule 9 wins over feature_addition |
| "seed" alone | feature_addition | No refactoring verb |
| "migrate" + "rename columns" | refactoring | "rename" is a refactoring verb; wins |
| "ETL" alone | feature_addition | Data pipeline vocabulary |
| "restructure tables" + "seed" | refactoring | "restructure" wins over seed |
