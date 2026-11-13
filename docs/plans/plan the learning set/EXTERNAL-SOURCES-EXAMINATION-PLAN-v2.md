# External Knowledge Sources — Claude Code Execution Plan v2.0

**Replaces:** v1.0 (proposal/debate archives only)
**Adds:** architecture challenge sources, reliability, distributed data,
security standards, stack-specific advanced patterns, arbiter catalog,
richer row schema with accepted/rejected outputs.

**Purpose:** Complete instructions for a Claude Code session to fetch,
examine, and convert open-source knowledge sources into training-ready
rows using the same eight-domain structure and a→f(b)+c functional form
already established in the internal corpus.

---

## Prerequisite reading (agent must read before starting)

```text
MODEL-EXAMINATION-COVERAGE-SUMMARY-v1.md   — ledger rules and status vocabulary
domain claude research/INDEX.md             — eight domains and a→f(b)+c form
domain claude research/batches/batch-001-025.md  — batch format to produce
DOMAIN-CODEX-RESEARCH-ROWIZED-DRAFT-v1.json     — row schema baseline
DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md    — architecture principles
domain-A-topology-orchestration.md
domain-B-routing-selection.md
domain-C-semantic-translation.md
domain-D-execution-applier.md
domain-E-arbiter-quality.md
domain-F-registry-storage.md
domain-G-learning-training.md
domain-H-governance-contracts.md
```

The agent must treat the existing eight-domain map as binding.
New public sources map into A through H. No new taxonomy is introduced.

The agent must explicitly confirm at startup that the current rowized
dataset is not final and still requires exact source spans and
accepted/rejected example pairs before any row is promoted.

---

## The eight dataset families (binding reference)

```
1. userPrompt_to_machineCommands
2. machineCommand_to_machineActionItems
3. machineActionItem_to_domainResult
4. domainResultsArray_to_resultI
5. arbitrateResultI_with_resultsContext
6. resultI_to_finalMachineResults
7. finalMachineResults_to_userFormat
8. historyAndVersions_to_reprocessingDecision
```

Families 5 and 8 are the most under-covered in the existing 21 draft rows.
Sources that produce family-5 and family-8 rows have the highest priority.

---

## Output file structure

The agent creates this folder structure. All paths are relative to the
project root where the existing domain files live.

```
external-sources/
  00-CONTEXT-LOAD-REPORT.md
  01-SOURCE-REGISTRY-ARCHITECTURE-CHALLENGES-v1.json
  02-source-examinations/
    reliability-distributed-systems.md
    distributed-data-consistency.md
    cloud-design-patterns.md
    security-governance.md
    migration-versioning.md
    stack-advanced-patterns.md
    proposal-debate-archives.md
    test-ci-observability.md
  03-OPEN-SOURCE-KNOWLEDGE-DOMAIN-MAP-v1.md
  04-ROWIZED-ARCHITECTURE-CHALLENGES-v1.json
  05-ARCHITECTURE-ARBITER-CATALOG-v1.json
  06-FINAL-DOCUMENT-STRUCTURE-v1.md
  batches/
    ext-batch-001-025.md
    ext-batch-026-050.md
    ...
  domain-files/
    ext-domain-A-topology.md
    ext-domain-B-routing.md
    ext-domain-C-translation.md
    ext-domain-D-execution.md
    ext-domain-E-arbiter.md
    ext-domain-F-registry.md
    ext-domain-G-learning.md
    ext-domain-H-governance.md
  COVERAGE-LEDGER.jsonl
```

---

## Phase 0 — Context load

Produce `00-CONTEXT-LOAD-REPORT.md` containing:

- Files read and confirmed present
- Existing dataset families found (list all 8)
- Existing row count per family from ROWIZED-DRAFT
- Families with zero coverage (must be flagged)
- Current domain structure confirmed
- Source categories to add (from this plan)
- Explicit statement: current rowized dataset is not final;
  exact source spans and accepted/rejected pairs are still missing

**Acceptance gate:** Agent must not proceed to Phase 1 until
this report is written and the gap statement is present.

---

## Phase 1 — Source registry

Produce `01-SOURCE-REGISTRY-ARCHITECTURE-CHALLENGES-v1.json`.

This file contains every source the agent will process.
Each entry uses this schema:

```json
{
  "sourceId": "AWS-BUILDERS-RETRY-BACKOFF-JITTER",
  "title": "Timeouts, retries, and backoff with jitter",
  "publisher": "AWS",
  "sourceKind": "production-engineering-article",
  "trustTier": 1,
  "fetchUrl": "https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/",
  "license": "AWS Builders Library — public access, no copy-wholesale",
  "targetDomains": ["A_TOPOLOGY", "E_ARBITER_QUALITY", "H_GOVERNANCE"],
  "targetSubdomains": [
    "distributed-failure-handling",
    "retry-policy",
    "overload-protection"
  ],
  "targetFamilies": [2, 5, 8],
  "allowedUse": [
    "architecture challenge rows",
    "arbiter rules",
    "accepted/rejected examples"
  ],
  "notAllowedUse": [
    "copy long prose",
    "treat as framework tutorial"
  ],
  "extractionStatus": "pending",
  "unitBoundary": "section"
}
```

### Trust tiers

**Tier 1** — official vendor architecture centers, security standards,
production-engineering libraries from major infrastructure providers.
Examples: AWS Builders' Library, Google SRE Book, OWASP ASVS, IETF RFCs,
NIST publications, Azure Architecture Center.

**Tier 2** — recognized architecture pattern catalogs, official reference
architectures. Examples: microservices.io, AOSA, System Design Primer,
.NET Architecture Guides.

**Tier 3** — scenario repositories, vetted engineering blogs.
Examples: Martin Fowler articles, Cosmic Python.

### Full source list by group

#### Group A — Proposal and debate archives (tier 2, richest for families 5 and 8)

| Source ID | Title | Fetch location | License | Domains | Families |
|-----------|-------|---------------|---------|---------|---------|
| RUST-RFCS | Rust RFCs | `github.com/rust-lang/rfcs` — clone `text/` | MIT | E, H | 1, 5, 8 |
| PYTHON-PEPS | Python PEPs | `github.com/python/peps` — clone `peps/` | Public domain | H, C | 1, 5, 8 |
| GO-PROPOSALS | Go Proposals | `github.com/golang/proposal` — clone `design/` | BSD | H | 1, 5, 8 |
| KAFKA-KIPS | Kafka KIPs | `cwiki.apache.org/confluence/display/KAFKA/Kafka+Improvement+Proposals` | Apache 2.0 | A, F | 1, 5, 8 |
| TS-DESIGN-NOTES | TypeScript design meeting notes | `github.com/microsoft/TypeScript/issues` label "Design Meeting Notes" | MIT | B, C | 5, 8 |
| CLOUDEVENTS-ISSUES | CloudEvents spec closed issues | `github.com/cloudevents/spec/issues?q=is:closed` | Apache 2.0 | F, A | 5, 8 |

#### Group B — Reliability and distributed systems (tier 1)

| Source ID | Title | Fetch location | License | Domains | Families |
|-----------|-------|---------------|---------|---------|---------|
| AWS-BUILDERS-RETRY | Timeouts, retries, backoff with jitter | `aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/` | Public access | A, E, H | 2, 5, 8 |
| AWS-BUILDERS-OVERLOAD | Avoiding overload in distributed systems | `aws.amazon.com/builders-library/avoiding-overload-in-distributed-systems/` | Public access | A, E | 5, 8 |
| AWS-BUILDERS-CACHING | Caching challenges and strategies | `aws.amazon.com/builders-library/caching-challenges-and-strategies/` | Public access | B, F | 3, 5 |
| AWS-BUILDERS-IDEMPOTENCY | Making retries safe with idempotent APIs | `aws.amazon.com/builders-library/making-retries-safe-with-idempotent-apis/` | Public access | D, E | 3, 5 |
| GOOGLE-SRE-BOOK | Google SRE Book | `sre.google/sre-book/` — CC-BY-NC-ND (read-only reference, no row text copy) | CC-BY-NC-ND | A, E, H | 5, 8 |
| GOOGLE-SRE-WORKBOOK | Google SRE Workbook | `sre.google/workbook/` | CC-BY-NC-ND | A, E | 5 |

**Note on Google SRE:** CC-BY-NC-ND means no derivatives and no commercial use.
Rows must be original formulations citing section headings, not copied text.
Mark all rows from this source with `"licenseConstraint": "no-copy"`.

#### Group C — Distributed data consistency (tier 2)

| Source ID | Title | Fetch location | License | Domains | Families |
|-----------|-------|---------------|---------|---------|---------|
| MSVC-SAGA | Saga pattern | `microservices.io/patterns/data/saga.html` | Public access | A, F | 2, 5, 8 |
| MSVC-OUTBOX | Transactional outbox | `microservices.io/patterns/data/transactional-outbox.html` | Public access | F, A | 2, 5 |
| MSVC-EVENT-SOURCING | Event sourcing | `microservices.io/patterns/data/event-sourcing.html` | Public access | F, G | 3, 5, 8 |
| MSVC-DB-PER-SVC | Database per service | `microservices.io/patterns/data/database-per-service.html` | Public access | F, A | 1, 5 |
| AZURE-CQRS | CQRS pattern | `learn.microsoft.com/azure/architecture/patterns/cqrs` | CC-BY 4.0 | F, A | 1, 5 |
| AZURE-EVENT-SOURCING | Event sourcing pattern | `learn.microsoft.com/azure/architecture/patterns/event-sourcing` | CC-BY 4.0 | F, G | 3, 5 |
| AZURE-COMPENSATING-TX | Compensating transaction pattern | `learn.microsoft.com/azure/architecture/patterns/compensating-transaction` | CC-BY 4.0 | A, F | 5, 8 |

#### Group D — Cloud design patterns (tier 1)

| Source ID | Title | Fetch location | License | Domains | Families |
|-----------|-------|---------------|---------|---------|---------|
| AZURE-CIRCUIT-BREAKER | Circuit breaker | `learn.microsoft.com/azure/architecture/patterns/circuit-breaker` | CC-BY 4.0 | A, E | 5, 8 |
| AZURE-BULKHEAD | Bulkhead isolation | `learn.microsoft.com/azure/architecture/patterns/bulkhead` | CC-BY 4.0 | A, E | 5 |
| AZURE-QUEUE-LOAD | Queue-based load leveling | `learn.microsoft.com/azure/architecture/patterns/queue-based-load-leveling` | CC-BY 4.0 | A, F | 2, 5 |
| AZURE-STRANGLER-FIG | Strangler fig application | `learn.microsoft.com/azure/architecture/patterns/strangler-fig` | CC-BY 4.0 | A, G | 2, 8 |
| AZURE-SIDECAR | Sidecar pattern | `learn.microsoft.com/azure/architecture/patterns/sidecar` | CC-BY 4.0 | A, B | 2, 5 |
| AWS-WELL-ARCH | AWS Well-Architected Framework | `docs.aws.amazon.com/wellarchitected/latest/framework/welcome.html` | Public access | H, E | 5 |
| GCP-ARCH-FRAMEWORK | Google Cloud Architecture Framework | `cloud.google.com/architecture/framework` | CC-BY 4.0 | H, E | 5 |

#### Group E — Security and governance (tier 1)

| Source ID | Title | Fetch location | License | Domains | Families |
|-----------|-------|---------------|---------|---------|---------|
| OWASP-ASVS-V5 | OWASP ASVS v5.0 | `github.com/OWASP/ASVS` — clone CSV export | CC-BY-SA 4.0 | E, H | 5 |
| OWASP-TOP10 | OWASP Top 10 | `owasp.org/www-project-top-ten/` | CC-BY-SA 4.0 | E, H | 5 |
| OWASP-CHEAT-SHEETS | OWASP Cheat Sheet Series | `github.com/OWASP/CheatSheetSeries` — clone | CC-BY-SA 4.0 | E, H | 3, 5 |
| IETF-RFC-JWT | RFC 7519 — JSON Web Tokens | `rfc-editor.org/rfc/rfc7519` | Public domain | E, H | 3, 5 |
| IETF-RFC-OAUTH2 | RFC 6749 — OAuth 2.0 | `rfc-editor.org/rfc/rfc6749` | Public domain | E, H | 3, 5 |
| IETF-RFC-TLS13 | RFC 8446 — TLS 1.3 | `rfc-editor.org/rfc/rfc8446` | Public domain | E, H | 5 |
| NIST-SSDF | NIST Secure Software Development Framework | `csrc.nist.gov/projects/ssdf` | Public domain | H, G | 5 |
| CWE-TOP25 | CWE Top 25 Most Dangerous Software Weaknesses | `cwe.mitre.org/top25/` | Public domain | E, H | 5 |

#### Group F — Architecture narrated by builders (tier 2)

| Source ID | Title | Fetch location | License | Domains | Families |
|-----------|-------|---------------|---------|---------|---------|
| AOSA-VOL1 | Architecture of Open Source Apps vol 1 | `aosabook.org/en/v1/` | CC-BY 3.0 | A, D | 1, 3, 5 |
| AOSA-VOL2 | Architecture of Open Source Apps vol 2 | `aosabook.org/en/v2/` | CC-BY 3.0 | A, D | 1, 3, 5 |
| AOSA-500L | 500 Lines or Less | `aosabook.org/en/500L/` | CC-BY 3.0 | D, C | 1, 3 |
| DOTNET-MICROSERVICES | .NET Microservices Architecture Guide | `learn.microsoft.com/en-us/dotnet/architecture/microservices/` | CC-BY 4.0 | A, F | 1, 2, 3 |
| SYSTEM-DESIGN-PRIMER | System Design Primer | `github.com/donnemartin/system-design-primer` | CC-BY 4.0 | A, B | 1, 5 |
| COSMIC-PYTHON | Architecture Patterns with Python | `cosmicpython.com/book/preface.html` | CC-BY-NC-ND (cite only) | A, F, G | 1, 3, 5 |
| MARTIN-FOWLER-STRANGLER | Strangler Fig Application (Fowler) | `martinfowler.com/bliki/StranglerFigApplication.html` | Public access | A, G | 2, 8 |

#### Group G — Stack-specific advanced architecture (tier 2)

These sources are processed only for architecture-level patterns,
anti-patterns, and decision conditions. Not for API syntax or tutorials.

| Source ID | Title | Fetch location | License | Domains | Families |
|-----------|-------|---------------|---------|---------|---------|
| NESTJS-ADVANCED | NestJS advanced topics: guards, interceptors, pipes, modules | `docs.nestjs.com` | MIT | B, D, E | 1, 3, 5 |
| REACT-ADVANCED | React.dev advanced: state structure, effects, reducers, context | `react.dev/learn` | CC-BY 4.0 | D, C | 3, 7 |
| ANGULAR-STYLE | Angular style guide, DI, signals, RxJS | `angular.dev/style-guide` | CC-BY 4.0 | B, D | 1, 3 |
| DOTNET-ASPNET-PERF | ASP.NET Core performance and security | `learn.microsoft.com/en-us/aspnet/core/performance/` | CC-BY 4.0 | D, E | 3, 5 |
| EFCORE-CONCURRENCY | EF Core concurrency and performance | `learn.microsoft.com/en-us/ef/core/querying/` | CC-BY 4.0 | F, D | 3, 5 |
| EFFECTIVE-GO | Effective Go concurrency patterns | `go.dev/doc/effective_go` | CC-BY 3.0 | D, A | 3 |
| RUST-ASYNC | Rust Async Book | `rust-lang.github.io/async-book/` | MIT | D, A | 3 |
| PHP-FIG-PSRS | PHP-FIG PSR standards (PSR-7, PSR-11, PSR-14) | `github.com/php-fig/fig-standards` | CC-BY 4.0 | B, F | 1, 5 |
| DOCKER-BEST-PRACTICES | Docker build best practices | `docs.docker.com/build/building/best-practices/` | Apache 2.0 | D, A | 2, 5 |
| GITHUB-ACTIONS-ADVANCED | GitHub Actions reusable workflows and matrix builds | `docs.github.com/en/actions` | CC-BY 4.0 | H, D | 2, 5 |
| OPENTELEMETRY-SPEC | OpenTelemetry specification | `opentelemetry.io/docs/specs/` | Apache 2.0 | H, A | 5 |
| OPENAPI-SPEC | OpenAPI Specification | `github.com/OAI/OpenAPI-Specification` | Apache 2.0 | B, F | 1, 3, 5 |

---

## Phase 2 — Source examination files

For each source group, produce one examination file in `02-source-examinations/`.

Each file uses this exact template:

```markdown
# Source Examination: <Group Name>

## 1. Sources examined

| Source ID | Title | Trust tier | Why included |
|-----------|-------|-----------|-------------|

## 2. Architecture challenges found

| Challenge ID | Challenge statement | Source anchor | XIIGEN domain |
|-------------|--------------------|--------------|--------------| 

## 3. Patterns found

| Pattern ID | Pattern name | When to use | When NOT to use | Source anchor |
|-----------|-------------|------------|----------------|--------------|

## 4. Anti-patterns found

| Anti-pattern ID | Bad design | Failure mode | Better design | Source anchor |
|----------------|-----------|-------------|--------------|--------------|

## 5. Arbiter rules extracted

| Arbiter ID | What it checks | Positive signal | Negative signal | Source anchor |
|-----------|--------------|----------------|----------------|--------------|

## 6. Candidate training rows

| Row ID | Dataset family | Domain | Status |
|--------|--------------|--------|-------|

## 7. Gaps — requires deeper source or additional fetch

<free text: what this group does not cover that was expected>
```

**Every extracted claim must have a source anchor.**
No "general knowledge" rows. If the anchor cannot be cited, the row is skipped.

---

## Phase 3 — Domain mapping document

Produce `03-OPEN-SOURCE-KNOWLEDGE-DOMAIN-MAP-v1.md`.

This document maps every source ID from the registry to the eight domains.

Template:

```markdown
# Open Source Technology Knowledge Domain Map v1

## Domain A — Topology and Orchestration

Sources:
- AWS-BUILDERS-RETRY: retry topology, bounded retry loops, fallback chains
- AWS-BUILDERS-OVERLOAD: load shedding, overload detection
- AZURE-CIRCUIT-BREAKER: circuit breaker state machine
- AZURE-QUEUE-LOAD: queue-based load leveling topology
- MSVC-SAGA: saga orchestration vs choreography
- KAFKA-KIPS: distributed topology decisions (ZooKeeper removal, consumer groups)
- AOSA-VOL1: Nginx event loop vs thread-per-connection architecture decision

Dataset row types from this domain:
- topology-decision rows
- retry-topology rows
- saga-orchestration rows
- circuit-breaker-state-machine rows
- queue-worker-topology rows

Arbiters sourced here:
- retryAmplificationArbiter
- circuitBreakerArbiter
- bulkheadIsolationArbiter
- orchestrationVsChoreographyArbiter
- queueLoadLevelingArbiter

## Domain B — Routing and Selection
...

## Domain C — Semantic Translation
...

## Domain D — Execution and Applier
...

## Domain E — Arbiter and Quality
...

## Domain F — Registry and Storage
...

## Domain G — Learning and Training
...

## Domain H — Governance and Contracts
...
```

**Acceptance gate:** Every source ID from the registry must appear
in at least one domain section. No source may be listed only as
"general architecture" without a specific subdomain assignment.

---

## Phase 4 — Rowized architecture challenge dataset

Produce `04-ROWIZED-ARCHITECTURE-CHALLENGES-v1.json`.

This is the primary deliverable. Every row uses this schema:

```json
{
  "rowId": "ARCH-<SUBDOMAIN>-<PATTERN>-<SEQUENCE>",
  "sourceIds": ["<source ID from registry>"],
  "sourceTrustTier": 1,
  "datasetFamily": "<one of the 8 family names>",
  "domain": "<A through H>",
  "subdomain": "<specific subdomain>",
  "architectureChallenge": "<statement of the problem the pattern addresses>",
  "input": {
    "a": "<trigger — what the user or system is trying to do>",
    "b": "<content — the design or code being evaluated>",
    "c": "<context — constraints, system state, failure conditions>"
  },
  "function": "<what reasoning or evaluation is applied>",
  "acceptedOutput": {
    "decision": "<what the correct design or verdict looks like>"
  },
  "rejectedOutput": {
    "decision": "<what the wrong design looks like — the anti-pattern>"
  },
  "arbiters": ["<arbiter IDs that would catch the rejected case>"],
  "whyAccepted": "<reasoning — why the accepted design is correct under these constraints>",
  "whyRejected": "<reasoning — why the rejected design fails under these constraints>",
  "sourceAnchors": [
    {
      "sourceId": "<source ID>",
      "anchor": "<section heading, URL fragment, or paragraph description>"
    }
  ],
  "licenseConstraint": "<none | no-copy | cite-only>",
  "trainingStatus": "candidate"
}
```

### Mandatory row classes

Every important source must produce rows across multiple classes:

| Row class | What it captures |
|-----------|----------------|
| `pattern-selection` | When to apply a pattern and when not to |
| `anti-pattern-detection` | Recognizing a failing design and stating why it fails |
| `tradeoff-decision` | Two valid approaches and the conditions that select each |
| `migration-decision` | How to move from an old design to a new one safely |
| `arbiter-construction` | Definition of a new quality gate with positive and negative signals |
| `negative-example` | A rejected design or declined proposal as a standalone example |

At least 70% of rows must include both `acceptedOutput` and `rejectedOutput`.
Every row must include `sourceIds`, `domain`, `datasetFamily`, and `arbiters`.
No row may be a "how to use framework X" tutorial row.

### Batch 1 target (first Claude Code session)

Minimum 60 candidate rows:
- 15 reliability rows (retry, backoff, timeout, idempotency, circuit breaker)
- 15 distributed-data rows (saga, outbox, event sourcing, CQRS, database ownership)
- 15 security/governance rows (ASVS requirements, OWASP top 10, JWT, OAuth2)
- 10 migration/versioning rows (strangler fig, canary, shadow mode, rollback)
- 5 meta-arbiter rows (arbiter definitions from architecture patterns)

---

## Phase 5 — Arbiter catalog

Produce `05-ARCHITECTURE-ARBITER-CATALOG-v1.json`.

Each arbiter entry uses this schema:

```json
{
  "arbiterId": "transactionalOutboxArbiter",
  "domain": "F_REGISTRY_STORAGE",
  "sourceIds": ["MSVC-OUTBOX", "AZURE-EVENT-SOURCING"],
  "checks": [
    "Does the design update business state and publish an event?",
    "Is the message stored atomically with the business update in one transaction?",
    "Is there a separate relay or publisher process that reads the outbox?",
    "Are duplicate messages handled idempotently by consumers?"
  ],
  "positiveSignals": [
    "business update and outbox insert in one database transaction",
    "event relay publishes only after the business transaction commits",
    "consumer idempotency key present"
  ],
  "negativeSignals": [
    "database update followed by direct broker publish without atomicity guarantee",
    "no duplicate message handling",
    "event published before state is persisted"
  ],
  "datasetFamilies": [
    "arbitrateResultI_with_resultsContext",
    "machineCommand_to_machineActionItems"
  ]
}
```

### Initial arbiter list to produce

```
retryAmplificationArbiter         — A: catches unbounded retry loops
timeoutPolicyArbiter              — A: catches missing or misconfigured timeouts
idempotencyArbiter                — D: catches non-idempotent retry-unsafe operations
transactionalOutboxArbiter        — F: catches publish-before-commit patterns
sagaCompensationArbiter           — A: catches saga designs without compensation paths
databaseOwnershipArbiter          — F: catches cross-service database access
eventSourcingFitArbiter           — F: catches event sourcing applied where it adds no value
cqrsFitArbiter                    — F: catches CQRS applied to simple CRUD scenarios
circuitBreakerArbiter             — A: catches missing circuit breaker on external dependencies
bulkheadIsolationArbiter          — A: catches shared thread pools across independent workloads
queueLoadLevelingArbiter          — A: catches synchronous calls that should be queued
stranglerMigrationArbiter         — G: catches big-bang migrations without incremental path
rollbackAvailabilityArbiter       — G: catches irreversible deployments with no rollback plan
tenantIsolationArbiter            — E: catches cross-tenant data leakage paths
accessControlArbiter              — E: catches missing authorization on service endpoints
secretManagementArbiter           — H: catches secrets in source code or environment variables
supplyChainProvenanceArbiter      — H: catches unverified dependencies and build artifacts
observabilityCoverageArbiter      — H: catches services with no structured logging or tracing
```

---

## Phase 6 — Final document structure

Produce `06-FINAL-DOCUMENT-STRUCTURE-v1.md`.

This is the human-readable master document for the dataset.

```markdown
# Architecture Challenge Learning Set v1

## 1. Purpose

## 2. Source policy
   - Trust tiers 1, 2, 3
   - License constraints by tier
   - What is never included (tutorials, syntax examples, beginner material)

## 3. Mapping to XIIGEN domains
   | Domain | Source groups | Row types |

## 4. Dataset families
   | Family | Meaning | Row count in this set |

## 5. Row classes
   | Class | Purpose | Count |

## 6. Arbiter catalog summary
   | Arbiter | Source | What it rejects |

## 7. Gaps
   | Missing area | Reason | Proposed source |

## 8. Next extraction plan
   (what the next Claude Code session should pick up)
```

---

## Examination procedure (per source unit)

The agent runs this for every source unit (one RFC, one PEP, one AOSA chapter,
one ASVS requirement group, one KIP, one AWS Builders article, one Azure pattern).

### Step 1 — Fetch

```bash
# GitHub repos — sparse clone
git clone --depth=1 --filter=blob:none --sparse <repo-url> <local-dir>
cd <local-dir>
git sparse-checkout set <subdirectory>

# Single pages
curl -s "<url>" -o <local-file>

# AOSA chapter list
curl -s https://aosabook.org/en/v1/ | grep -oP 'href="[^"]+\.html"' | sort -u

# Kafka KIPs — fetch index then each page
curl -s "https://cwiki.apache.org/confluence/display/KAFKA/Kafka+Improvement+Proposals"

# Rust RFCs ranked by comment count (richest first)
gh api repos/rust-lang/rfcs/issues \
  --jq '.[] | {number: .number, title: .title, comments: .comments}' \
  --paginate | jq -s 'sort_by(-.comments) | .[:50]'
```

### Step 2 — Identify unit boundary

| Source | Unit boundary |
|--------|-------------|
| Rust RFCs | One `.md` file per RFC |
| Python PEPs | One `.rst` file per PEP |
| Go Proposals | One `.md` file in `design/` |
| Kafka KIPs | One wiki page per KIP |
| AOSA | One chapter |
| OWASP ASVS | One requirement group (e.g., section 2.1 Passwords) |
| IETF RFC | One complete RFC document |
| AWS Builders' Library | One article |
| Azure patterns | One pattern page |
| microservices.io | One pattern page |

### Step 3 — Examine each unit

For every source unit, produce a record in the batch file:

```markdown
### EXT-<GROUP>-<SEQUENCE> <source-name> | <unit-title>

**Source:** <canonical URL>
**Source ID:** <from registry>
**License:** <license>
**Trust tier:** <1 | 2 | 3>
**Fetched:** <date>
**Unit type:** <RFC | PEP | article | pattern | chapter | requirement-group>

**Structure:**
<brief description — sections, headings, key parts>

**Domain:** <A through H>
**Subdomains:** <specific subdomains>

**Reasoning chain present:** <YES | PARTIAL | NO>
<If YES: which sections contain live reasoning — motivation, alternatives,
community debate, revision, verdict>

**Architecture challenge:**
<what problem this unit addresses>

**a→f(b)+c:**
- a (trigger): <what initiates the reasoning>
- f (transform): <what evaluation or decision process is applied>
- b (content): <what is being evaluated>
- c (output): <what the conclusion or verdict is>

**Families this unit feeds:**
<family numbers and why>

**Positive examples in this unit:**
<1–3 spans that become accepted-output rows>

**Negative examples in this unit:**
<1–3 spans that become rejected-output rows — anti-patterns, declined proposals>

**Draft rows:**
<1–3 rows in the Phase 4 row schema>
```

### Step 4 — Batch file naming

Group 25 units per batch file: `ext-batch-001-025.md`, `ext-batch-026-050.md`, etc.

### Step 5 — Ledger update

After each unit, append one row to `COVERAGE-LEDGER.jsonl`:

```jsonl
{"sourceId": "EXT-G1-001", "registryId": "RUST-RFCS", "unitTitle": "RFC 2229 closures", "sourceUrl": "https://...", "license": "MIT", "trustTier": 2, "status": "domain_reviewed_draft_rowized", "domainsTagged": ["E", "H"], "familiesTagged": [5, 8], "rowsProduced": 2, "licenseConstraint": "none", "examinedDate": "2026-05-03"}
```

---

## Execution order

Run phases in this order within the first Claude Code session:

**Priority 1 — Reliability and distributed data (batch 1)**

Sources: AWS-BUILDERS-RETRY, AWS-BUILDERS-OVERLOAD, AWS-BUILDERS-IDEMPOTENCY,
MSVC-SAGA, MSVC-OUTBOX, MSVC-EVENT-SOURCING, MSVC-DB-PER-SVC,
AZURE-CIRCUIT-BREAKER, AZURE-CQRS, AZURE-COMPENSATING-TX

Target: 30 rows (families 2, 5, 8)

**Priority 2 — Security and governance (batch 2)**

Sources: OWASP-ASVS-V5 sections 2 and 4, OWASP-TOP10,
IETF-RFC-JWT, IETF-RFC-OAUTH2, CWE-TOP25, NIST-SSDF

Target: 15 rows (family 5 primarily)

**Priority 3 — Proposal archives (batch 3)**

Sources: RUST-RFCS (top 20 by comment count), PYTHON-PEPS
(PEP 572, 634, rejected PEPs), KAFKA-KIPS (KIP-500, KIP-848, KIP-98)

Target: 15 rows (families 1, 5, 8)

**Priority 4 — Architecture narrated by builders (batch 4)**

Sources: AOSA-VOL1 (LLVM, Nginx, PostgreSQL chapters),
DOTNET-MICROSERVICES, SYSTEM-DESIGN-PRIMER, AZURE-STRANGLER-FIG,
MARTIN-FOWLER-STRANGLER

Target: 10 rows (families 1, 2, 3, 8)

**Priority 5 — Stack-specific advanced patterns (batch 5)**

Sources: NESTJS-ADVANCED, DOCKER-BEST-PRACTICES, GITHUB-ACTIONS-ADVANCED,
OPENTELEMETRY-SPEC, OPENAPI-SPEC

Target: 10 rows (families 1, 2, 3, 5)

---

## Quality gates before promoting any row

1. **License clean** — CC-BY-SA acceptable with attribution.
   CC-BY-NC-ND: cite section only, no text. Proprietary: skip.

2. **Reasoning present** — row must capture a reasoning step, not a fact.
   "OAuth requires PKCE" is a fact. "The implicit flow was deprecated
   because token interception in the redirect URI was demonstrated in
   CVE-2019-3779 and PKCE solves the same use case without the vulnerability"
   is reasoning. Only the second is usable.

3. **Context sufficient** — fields a, b, c must contain enough context
   that the output can be reproduced without the source document.
   If not, mark `requires_span_extraction` and note the exact section.

4. **Both outputs present** — any row where the pattern has a clear
   anti-pattern must include both `acceptedOutput` and `rejectedOutput`.
   A row with only `acceptedOutput` is incomplete unless the pattern
   has no meaningful anti-pattern (rare).

5. **Family assignment correct** — a requirement statement (ASVS) is family 5.
   A decomposition into steps (KIP implementation plan) is family 2.
   A user question plus accepted answer is family 1 or 7.
   A version revision triggered by an arbiter verdict is family 8.

6. **No duplication** — check existing internal rows in
   DOMAIN-CODEX-RESEARCH-ROWIZED-DRAFT-v1.json before adding a new row
   on the same concept.

---

## What the agent must never do

- Copy long prose from CC-BY-NC-ND sources (Google SRE, Cosmic Python)
  into row fields. Cite the section, formulate the row independently.

- Produce rows from tutorial content. "How to configure NestJS guards"
  is a tutorial. "NestJS global guards vs route-scoped guards: which
  to use when and what breaks if the wrong choice is made" is architecture.

- Produce circular rows where input and output restate the same concept.
  "What is the Strategy pattern? — It is a behavioral pattern..." is circular.

- Exceed 25 source units per batch file.

- Skip the ledger update. Every examined unit must appear in the ledger.
  An examined unit with no ledger row is invisible to the next session.

- Invent a new domain taxonomy. The eight domains A through H are binding.
  New sources are mapped into them, not around them.
