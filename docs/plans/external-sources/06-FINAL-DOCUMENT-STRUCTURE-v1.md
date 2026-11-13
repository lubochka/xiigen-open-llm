# Architecture Challenge Learning Set v1
## Final Document Structure
## Date: 2026-05-04

---

## 1. Purpose

This learning set produces training rows for the XIIGEN dynamic flow pipeline.
The rows are derived exclusively from open-source, openly licensed technical sources.
Every row captures an architectural reasoning step — not a fact, not a tutorial.

The primary gap this set fills: families 5 (arbitration) and 8 (reprocessing decisions)
are under-covered in the existing internal corpus. External sources have been selected
to maximize the density of accepted/rejected decision pairs in these families.

---

## 2. Source Policy

**Governing document:** EXTERNAL-SOURCE-REGISTRY-v1.0.md

### License classes accepted

| Class | Examples | Allowed use |
|-------|----------|-------------|
| TRAINING_OK | Apache 2.0, MIT, BSD, Public domain | Raw text ingestion; derived rows; accepted/rejected pairs |
| ATTRIBUTION_REQUIRED | CC-BY 3.0, CC-BY 4.0 | Same as TRAINING_OK; attribution must appear in row licenseConstraint field |
| ATTRIBUTION_SHAREALIKE | CC-BY-SA 4.0 | Same as ATTRIBUTION_REQUIRED; derivative works must use compatible license |
| FREE_COMMERCIAL_USE_WITH_NOTICE | MITRE CWE Terms | Derived rows; required attribution notice in row metadata |
| IETF_TRUST_PROVISIONS | IETF RFCs | Document text; code component reuse requires per-RFC verification |
| PUBLIC_DOMAIN_US | NIST publications | Raw text; no attribution required but recommended |

### License classes rejected

| Class | Examples | Why rejected |
|-------|----------|-------------|
| CC-BY-NC-ND | Google SRE Book, Cosmic Python | No derivatives; no commercial use |
| CITE_ONLY_NO_COPY | Martin Fowler articles, AWS Builders Library | No reuse license; public access only |
| VERIFY_BEFORE_USE | DEFECTS4J, BugsInPy, QuixBugs, Semgrep rules, CodeQL examples | License not confirmed; not usable until promoted |

### What is never included

- Tutorial-level content (how to configure a framework, step-by-step setup guides)
- Circular rows (input and output restate the same concept)
- Fact-only rows ("OAuth requires PKCE" without reasoning context)
- Company engineering blog posts (no training license)
- Any source whose license cannot be confirmed from its LICENSE file

---

## 3. Mapping to XIIGEN Domains

| Domain | Primary sources | Row types |
|--------|----------------|-----------|
| A — Topology | KUBERNETES-KEPS, MICROSOFT-LEARN-ARCH, KAFKA-KIPS | Circuit breaker, bulkhead, strangler fig, rollout strategy |
| B — Routing | OPENAPI-SPEC, GOOGLE-AIP, NESTJS-ADVANCED | API versioning, schema routing, pagination selection |
| C — Semantic Translation | PYTHON-PEPS, REACT-ADVANCED, EFFECTIVE-GO | Format translation, language pattern expression |
| D — Execution Applier | SWE-BENCH, DOCKER-BEST-PRACTICES, GITHUB-ACTIONS-ADVANCED | Patch application, build execution, CI/CD pipeline |
| E — Arbiter Quality | OWASP-ASVS, CWE-TOP25, IETF-RFCS, OPENTELEMETRY-SPEC | Security controls, vulnerability detection, observability conformance |
| F — Registry Storage | MICROSOFT-LEARN-ARCH / CQRS+ES, DOTNET-MICROSERVICES | CQRS fit, event sourcing fit, transactional outbox |
| G — Learning Training | NIST-SSDF, SWE-BENCH, RUST-RFCS, PYTHON-PEPS | Lifecycle governance, proposal-outcome pairs |
| H — Governance Contracts | NIST-SSDF, GOOGLE-AIP, KUBERNETES-KEPS, OWASP-ASVS | Supply chain, API versioning, deprecation, release gates |

---

## 4. Dataset Families

| Family | Name | Row count (batches 1–3) | Gap status |
|--------|------|------------------------|-----------|
| 1 | userPrompt_to_machineCommands | 7 rows (b2: EXT-037,039,048,049; b3: EXT-056,057) | Moderate — new KEP/AIP sources active |
| 2 | machineCommand_to_machineActionItems | 3 rows (b2: EXT-050; b3: EXT-061,065) | Moderate — Gherkin, EF, data-access sources active |
| 3 | machineActionItem_to_domainResult | 1 row (b3: EXT-069 SWE-bench) | Gap — SWE-bench source now confirmed usable |
| 4 | domainResultsArray_to_resultI | 0 | Gap — batch 4 target |
| 5 | arbitrateResultI_with_resultsContext | 38 rows (b1: 17; b2: 15; b3: 13) | **CRITICAL GAP — substantially addressed** |
| 6 | resultI_to_finalMachineResults | 0 | Gap — batch 4 target |
| 7 | finalMachineResults_to_userFormat | 4 rows (b2: EXT-034*; b3: EXT-068,070) | Gap — Mermaid/Diátaxis sources now active |
| 8 | historyAndVersions_to_reprocessingDecision | 8 rows (b1: 4; b2: 4) | **CRITICAL GAP — partially addressed** |

**Running total: 61 candidate rows (EXT-SEC-001 through EXT-070)**
**Source groups active: 1–7 (original) + Groups 8–16 (added 2026-05-04 via expanded plan)**

---

## 5. Row Classes

| Class | Purpose | Count (batches 1–3) |
|-------|---------|---------------------|
| anti-pattern-detection | Recognizing a failing design and stating why it fails | 22 rows |
| arbiter-construction | Definition of a quality gate with positive and negative signals | 16 rows |
| tradeoff-decision | Two approaches and the conditions that select each | 15 rows |
| migration-decision | How to move from an old design to a new one safely | 7 rows |
| negative-example | A rejected design as a standalone example | 1 row |

---

## 6. Arbiter Catalog Summary

| Arbiter | Source | What it rejects |
|---------|--------|----------------|
| jwtValidationArbiter | OWASP-ASVS, IETF-RFC-JWT | alg:none; signature skipped; claims not validated |
| oauthGrantArbiter | IETF-RFC-OAUTH2, OWASP-ASVS | Implicit grant for public clients; no PKCE |
| passwordStorageArbiter | OWASP-ASVS | MD5/SHA-1 hashing; fast hashes without memory cost |
| sqlInjectionArbiter | CWE-TOP25, OWASP-ASVS | String interpolation in queries; sanitization-only defense |
| secretPresenceArbiter | NIST-SSDF, CWE-TOP25 | Literal credentials in source; 'private repo' as defense |
| tenantIsolationArbiter | OWASP-ASVS | Tenant ID from request params; post-query access check |
| tlsConfigArbiter | IETF-RFC-TLS13, OWASP-ASVS | TLS 1.0/1.1; RC4; cert validation disabled |
| supplyChainArbiter | NIST-SSDF | Unverifiable dep hash; version ranges; no SBOM |
| circuitBreakerArbiter | MICROSOFT-LEARN-ARCH | No circuit breaker; direct retry under sustained failure |
| bulkheadIsolationArbiter | MICROSOFT-LEARN-ARCH | Shared pool; rate limiting as sole isolation |
| tracePropagationArbiter | OPENTELEMETRY-SPEC | Custom headers; no SDK propagator; disconnected spans |
| apiVersioningArbiter | GOOGLE-AIP, OPENAPI-SPEC | Breaking changes in existing version path |
| longRunningOpArbiter | GOOGLE-AIP | Blocking HTTP connection for long operations |
| idempotentCreateArbiter | GOOGLE-AIP | Duplicate creation; 409 on retry; no idempotency key |
| stranglerMigrationArbiter | MICROSOFT-LEARN-ARCH | Big-bang cutover; sunk cost continues stalled rewrite |
| cqrsFitArbiter | MICROSOFT-LEARN-ARCH | CQRS on symmetric CRUD; pattern familiarity as justification |
| transactionalOutboxArbiter | DOTNET-MICROSERVICES | Direct broker publish without atomic outbox |
| proposalCompletenessArbiter | RUST-RFCS | Empty alternatives; TODO unresolved questions |
| protocolMigrationArbiter | KAFKA-KIPS | Single cutover; no dual-mode; fixed removal date |
| outOfBoundsWriteArbiter | CWE-TOP25 (CWE-787) | memcpy with user-controlled size; no destination-capacity check |
| outOfBoundsReadArbiter | CWE-TOP25 (CWE-125) | Array subscript from unsigned subtraction without ordering check |
| useAfterFreeArbiter | CWE-TOP25 (CWE-416) | Double-free in exception handler chain; raw pointer instead of RAII |
| logProcessingArbiter | OWASP-ASVS V7.2 | Logging failure silently swallowed; no fallback alerting path |
| logProtectionArbiter | OWASP-ASVS V7.3 | Logs writable by application service account; no append-only destination |
| controllerResponsibilityArbiter | NESTJS-CORE | Fat controller orchestrating multi-step business logic |
| nestjsPipelinePhaseArbiter | NESTJS-CORE | Authorization logic in Interceptor instead of Guard |
| graphRoutingCompletenessArbiter | LANGGRAPH | Conditional edge ignores soft errors in HTTP 200 body |
| conventionalCommitArbiter | CONVENTIONAL-COMMITS | Missing type prefix; multi-concern bundling |
| mcpToolSelectionArbiter | MCP-DOCS | Tool selected by syntactic similarity, not information need |
| legacyDotNetModernizationArbiter | ASPNET-WEBSTACK | Direct SqlConnection in controller; synchronous SMTP on request thread |
| accessibilityErrorStateArbiter | GOVUK-DESIGN-SYSTEM | Color-only error signaling; no aria-describedby; no live region |
| gherkinScenarioArbiter | GHERKIN | Conflated scenarios; Then describes internal state not observable behavior |
| documentationTypeArbiter | DIATAXIS | Tutorial produced when reference is the correct documentation type |
| dataAccessSelectionArbiter | EFCORE-SOURCE, DAPPER | EF Core Include chain used when projection query is required for performance |
| legacyFrontendMigrationArbiter | ANGULARJS-SOURCE | jQuery DOM manipulation; $.ajax bypassing Zone.js; $scope state mixing |

---

## 7. Gaps (updated after batches 1–3)

| Missing area | Reason | Proposed source |
|-------------|--------|----------------|
| Family 4 rows (domain results array) | No batch has targeted this family yet | SWE-BENCH multi-result aggregation |
| Family 6 rows (final machine results) | No batch has targeted this family yet | BIGCODEBENCH, SWE-CARE |
| CWE entries 11–25 | Batches 1–3 covered ranks 1–10 except CWE-476 and CWE-119 | CWE-TOP25 entries 11–25 |
| OWASP ASVS V2 — additional password controls | V2.4.x partially covered; V2.2 (authentication) not covered | OWASP-ASVS chapter 2 |
| Family 8 additional rows | Only 8 rows; 20+ needed for critical gap | NIST-SSDF PO practices; KUBERNETES-KEPS deprecation policy |
| Legacy .NET rows — EF6 N+1 | Group 8/9 sources now active; EF6 lazy-loading not yet covered | EF6-SOURCE §Lazy loading |
| BDD family 2 rows — Robot Framework | ROBOT-FRAMEWORK now in plan; no rows produced yet | ROBOT-FRAMEWORK test suite decomposition |
| Agent graph rows — LANGGRAPH | EXT-060 covers routing completeness; no graph state management rows | LANGGRAPH state management and checkpointing |
| Formatter rows — Node.js docs | NODEJS-DOCS now in plan; no rows produced yet | NODEJS-DOCS event loop explanation |
| Language corpora rows | Group 16 sources (Tatoeba, Common Voice) now in plan; no rows yet | TATOEBA multilingual sentence pairs |

---

## 8. Next Extraction Plan (batch 4)

Batch 4 target: 20 additional candidate rows

Priority 1: Remaining CWE Top 25 entries
- CWE-476 NULL pointer dereference (rank 7)
- CWE-119 Buffer overflow (rank 8)
- CWE-416 additional pattern — use-after-free via iterator invalidation
- CWE entries 11–15 (OS command injection, format string, integer overflow, CSRF, directory traversal variants)

Priority 2: Family 4 and 6 rows — aggregation and result translation
- SWE-BENCH multi-result scenario (family 4: aggregate patch results)
- BIGCODEBENCH function result to natural-language translation (family 6)
- SWE-CARE code review result translation (family 6)

Priority 3: Group 8 legacy .NET rows
- EF6 lazy-loading N+1 detection (EF6-SOURCE)
- WPF Dispatcher thread violation (WPF-SAMPLES)
- WebForms ViewState dependency identification (ESHOP-MODERNIZING)

Priority 4: Group 13 BDD rows
- Robot Framework test suite decomposition — action items from acceptance criteria (family 2)
- SCRUM-GUIDE sprint planning decision — which stories meet Definition of Done (family 5)

Priority 5: Group 15 agent orchestration rows
- LANGGRAPH graph state checkpoint — when to checkpoint vs. replay (family 8)
- OPENAI-COOKBOOK few-shot prompt construction — prompt selection criteria (family 1)
- PROMPTFOO evaluation result — test failure classification (family 5)
