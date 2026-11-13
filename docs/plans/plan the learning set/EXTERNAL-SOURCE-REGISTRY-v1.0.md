# External Source Registry — v1.0
## Governing policy for all external training set sources
## Supersedes: source list in EXTERNAL-SOURCES-EXAMINATION-PLAN-v2.md (Groups A–G)
## Source list authority: EXAMINATION-CONTINUATION-PLAN-v1.0.md Groups 1–16 (expanded 2026-05-04)
## Last updated: 2026-05-04

> **Registry scope update (2026-05-04):** EXAMINATION-CONTINUATION-PLAN-v1.0.md has been
> expanded from 7 groups to 16 groups. Groups 8–16 cover: legacy .NET/ASP.NET/WPF (8),
> SQL/EF/Dapper (9), legacy JavaScript/AngularJS/jQuery (10), modern frontend/Angular/React/
> NestJS/design systems (11), diagramming/AsyncAPI (12), Agile/BDD/test automation (13),
> documentation/release management (14), agent skills/LLM orchestration (15), and language
> corpora (16). This registry v1.0 documents the original Groups 1–7. The governing license
> rules in the Section 1 preamble apply unchanged to all 16 groups. For Groups 8–16 source
> IDs and fetch locations, refer to EXAMINATION-CONTINUATION-PLAN-v1.0.md §§ Group 8–16.

---

## The governing rule

**No source enters the training set unless its LICENSE file or official terms
explicitly allow reuse, modification, redistribution, or commercial research
and development use.**

A public GitHub repository is not automatically open-source. GitHub itself states
that a repository needs a license for others to be free to use, change, and
distribute it (GitHub Docs: Licensing a repository). "Public access" is not
a license class. A page being readable does not make it reusable.

This rule is a hard gate. It applies to every row, every draft, every batch.
A row whose source does not appear in the TRAINING_OK or ATTRIBUTION_REQUIRED
section below is not promotable, regardless of how architecturally useful it is.

---

## Section 1 — TRAINING_OK sources (confirmed open license)

These sources are cleared for derived row production, accepted/rejected examples,
and arbiter rule extraction. Raw text ingestion is allowed where noted.

### 1.1 Architecture and orchestration

| Source ID | Title | License | Use for | Raw text ingestion |
|-----------|-------|---------|---------|-------------------|
| `KUBERNETES-KEPS` | Kubernetes Enhancement Proposals | Apache 2.0 | Orchestration, migration, rollout, production-readiness gates | Yes |
| `OPENTELEMETRY-SPEC` | OpenTelemetry Specification | Apache 2.0 | Observability architecture, traces, metrics, semantic conventions | Yes |
| `OPENAPI-SPEC` | OpenAPI Specification | Apache 2.0 | API contracts, schema governance, request/response validation | Yes |
| `GOOGLE-AIP` | Google API Improvement Proposals | Content CC-BY 4.0; code Apache 2.0 | API design, pagination, resource naming, long-running operations | Yes |
| `MICROSOFT-LEARN-ARCH` | Microsoft Learn architecture docs | CC-BY 4.0 (where stated on page) | Azure architecture patterns, CQRS (Command Query Responsibility Segregation), circuit breaker, strangler, cloud design | Yes |

**MICROSOFT-LEARN-ARCH note:** This covers all `learn.microsoft.com/azure/architecture/` pages
that display a CC-BY 4.0 notice. Verify the license footer before ingesting any individual page.
Pages without a stated license are treated as CITE_ONLY_NO_COPY.

### 1.2 Security and governance

| Source ID | Title | License | Use for | Raw text ingestion |
|-----------|-------|---------|---------|-------------------|
| `OWASP-ASVS` | OWASP Application Security Verification Standard | CC-BY-SA 4.0 | Security verification arbiters, accepted/rejected security controls | Yes |
| `OWASP-CHEAT-SHEETS` | OWASP Cheat Sheet Series | CC-BY-SA 4.0 (verify repo file before ingestion) | Secure implementation rules, negative examples | Yes (after per-file check) |
| `NIST-SSDF` | NIST Secure Software Development Framework | Not subject to U.S. copyright; attribution recommended | Secure SDLC (Software Development Life Cycle), supply chain, governance rows | Yes |
| `CWE-TOP25` | MITRE CWE (Common Weakness Enumeration) Top 25 | Free for research, development, commercial use with required notice | Vulnerability-pattern arbiters, secure-code negative examples | Yes |
| `IETF-RFCS` | RFCs: OAuth2 (RFC 6749), JWT (RFC 7519), TLS 1.3 (RFC 8446), HTTP, etc. | IETF Trust Legal Provisions; code components have specific reuse rules | Protocol contracts, security, interoperability rows | Yes (document text); code reuse: verify per RFC |

### 1.3 Bug-fix and code-agent datasets

| Source ID | Title | License | Use for | Raw text ingestion |
|-----------|-------|---------|---------|-------------------|
| `SWE-BENCH` | SWE-bench | MIT | Issue-to-patch rows, test validation rows, agent edit-loop rows | Yes |

### 1.4 Proposal and debate archives

| Source ID | Title | License | Use for | Raw text ingestion |
|-----------|-------|---------|---------|-------------------|
| `RUST-RFCS` | Rust RFC (Request for Comments) repository | MIT | Language design decision rows, accepted/rejected feature rows, families 5 and 8 | Yes |
| `PYTHON-PEPS` | Python PEPs (Python Enhancement Proposals) | Public domain | Language governance rows, proposal-debate pairs | Yes |
| `GO-PROPOSALS` | Go Proposals (`design/` directory) | BSD 3-Clause | Language design decisions, families 5 and 8 | Yes |
| `KAFKA-KIPS` | Apache Kafka KIPs (Kafka Improvement Proposals) | Apache 2.0 | Distributed topology decisions, consumer group design | Yes |
| `CLOUDEVENTS-SPEC` | CloudEvents specification closed issues | Apache 2.0 | Event schema governance rows | Yes |

### 1.5 Open-source architecture documentation

| Source ID | Title | License | Use for | Raw text ingestion |
|-----------|-------|---------|---------|-------------------|
| `AOSA-VOL1` | Architecture of Open Source Applications vol. 1 | CC-BY 3.0 | OSS (Open Source Software) architecture patterns and decision narratives | Yes |
| `AOSA-VOL2` | Architecture of Open Source Applications vol. 2 | CC-BY 3.0 | OSS architecture patterns and decision narratives | Yes |
| `AOSA-500L` | 500 Lines or Less | CC-BY 3.0 | Minimal implementation examples | Yes |
| `DOTNET-MICROSERVICES` | .NET Microservices Architecture Guide (Microsoft Learn) | CC-BY 4.0 | Microservice topology, data patterns, families 1–3 | Yes |
| `SYSTEM-DESIGN-PRIMER` | System Design Primer | CC-BY 4.0 | Topology rows, scaling decisions | Yes |
| `EFFECTIVE-GO` | Effective Go | CC-BY 3.0 | Concurrency patterns, idiomatic design rows | Yes |
| `RUST-ASYNC` | Rust Async Book | MIT | Async execution patterns | Yes |

### 1.6 Stack-specific documentation (CC-BY 4.0 or MIT)

| Source ID | Title | License | Use for | Raw text ingestion |
|-----------|-------|---------|---------|-------------------|
| `REACT-ADVANCED` | React.dev advanced topics | CC-BY 4.0 | State structure, effects, reducers, context — Domain C and D rows | Yes |
| `ANGULAR-STYLE` | Angular style guide and DI (Dependency Injection) | CC-BY 4.0 | Routing and selection rows, DI pattern rows | Yes |
| `DOTNET-ASPNET-PERF` | ASP.NET Core performance and security docs | CC-BY 4.0 | Execution and security rows | Yes |
| `EFCORE-CONCURRENCY` | EF Core (Entity Framework Core) concurrency docs | CC-BY 4.0 | Registry and storage rows | Yes |
| `NESTJS-ADVANCED` | NestJS advanced docs: guards, interceptors, pipes, modules | MIT | Routing and execution rows — architecture level only, not tutorials | Yes |
| `PHP-FIG-PSRS` | PHP-FIG PSR (PHP Standard Recommendation) standards | CC-BY 4.0 | API contract and registry rows | Yes |
| `DOCKER-BEST-PRACTICES` | Docker build best practices | Apache 2.0 | Execution rows, deployment topology | Yes |
| `GITHUB-ACTIONS-ADVANCED` | GitHub Actions reusable workflows | CC-BY 4.0 | Governance and execution rows | Yes |
| `GCP-ARCH-FRAMEWORK` | Google Cloud Architecture Framework | CC-BY 4.0 | Governance and arbiter rows | Yes |

### 1.7 CNCF (Cloud Native Computing Foundation) Apache 2.0 repos

Sources from CNCF projects whose repository LICENSE file is Apache 2.0.
Check the LICENSE file of each individual repo before adding a source.
Do not use CNCF repos whose license is not confirmed Apache 2.0.

Examples of confirmed Apache 2.0 CNCF repos (verify before each ingestion session):
- Kubernetes enhancements (covered by KUBERNETES-KEPS above)
- OpenTelemetry specification (covered by OPENTELEMETRY-SPEC above)
- Argo Workflows, Flux, Helm documentation repos — verify per repo

---

## Section 2 — VERIFY_BEFORE_USE sources

These sources are potentially open but license has not been confirmed
for every version, branch, or subdirectory. Do not ingest any text
or produce any rows until the repository LICENSE file is read and
the `licenseUseClass` is updated to TRAINING_OK.

```json
[
  {
    "sourceId": "DEFECTS4J",
    "title": "Defects4J",
    "licenseUseClass": "VERIFY_BEFORE_USE",
    "rawTextIngestionAllowed": false,
    "derivedRowAllowed": false,
    "verificationTarget": "check LICENSE file in the Defects4J GitHub repository",
    "useFor": "Java bug-fix examples"
  },
  {
    "sourceId": "BUGSINPY",
    "title": "BugsInPy",
    "licenseUseClass": "VERIFY_BEFORE_USE",
    "rawTextIngestionAllowed": false,
    "derivedRowAllowed": false,
    "verificationTarget": "check LICENSE file in the BugsInPy GitHub repository",
    "useFor": "Python bug-fix examples"
  },
  {
    "sourceId": "QUIXBUGS",
    "title": "QuixBugs",
    "licenseUseClass": "VERIFY_BEFORE_USE",
    "rawTextIngestionAllowed": false,
    "derivedRowAllowed": false,
    "verificationTarget": "check LICENSE file in the QuixBugs GitHub repository",
    "useFor": "Small bug-repair tasks"
  },
  {
    "sourceId": "SEMGREP-RULES",
    "title": "Semgrep rules registry",
    "licenseUseClass": "VERIFY_BEFORE_USE",
    "rawTextIngestionAllowed": false,
    "derivedRowAllowed": false,
    "verificationTarget": "check LICENSE file in the semgrep-rules GitHub repository",
    "useFor": "Static analysis and security pattern rows"
  },
  {
    "sourceId": "CODEQL-EXAMPLES",
    "title": "CodeQL query examples",
    "licenseUseClass": "VERIFY_BEFORE_USE",
    "rawTextIngestionAllowed": false,
    "derivedRowAllowed": false,
    "verificationTarget": "check LICENSE file in the codeql GitHub repository",
    "useFor": "Vulnerability-detection rows"
  }
]
```

To promote a VERIFY_BEFORE_USE source:
1. Read the repository LICENSE file at its canonical URL.
2. Confirm the license is MIT, Apache 2.0, BSD, CC-BY, CC-BY-SA, public domain, or equivalent.
3. Update `licenseUseClass` to TRAINING_OK and set `rawTextIngestionAllowed: true`.
4. Record the license text location (URL + commit SHA or date) in the source entry.

---

## Section 3 — REMOVED sources (from v2 plan)

These sources appeared in EXTERNAL-SOURCES-EXAMINATION-PLAN-v2.md but do not
meet the governing rule. They are removed from the training source list.
They may be cited in plan documents but must not produce training rows.

| Source ID (v2) | Reason removed | Allowed use |
|----------------|----------------|-------------|
| `AWS-BUILDERS-RETRY` | "Public access" is not a license. AWS Builders' Library terms do not grant training or redistribution rights. | CITE_ONLY_NO_COPY |
| `AWS-BUILDERS-OVERLOAD` | Same as above | CITE_ONLY_NO_COPY |
| `AWS-BUILDERS-CACHING` | Same as above | CITE_ONLY_NO_COPY |
| `AWS-BUILDERS-IDEMPOTENCY` | Same as above | CITE_ONLY_NO_COPY |
| `AWS-WELL-ARCH` | "Public access" not a license; AWS documentation terms restrict redistribution | CITE_ONLY_NO_COPY |
| `GOOGLE-SRE-BOOK` | CC-BY-NC-ND: no derivatives, no commercial use — training rows are derivatives | CITE_ONLY_NO_COPY |
| `GOOGLE-SRE-WORKBOOK` | CC-BY-NC-ND: same reason | CITE_ONLY_NO_COPY |
| `COSMIC-PYTHON` | CC-BY-NC-ND: same reason | CITE_ONLY_NO_COPY |
| `MARTIN-FOWLER-STRANGLER` | martinfowler.com has no open license; "public access" only | CITE_ONLY_NO_COPY |
| `MSVC-SAGA` | microservices.io does not carry a clearly stated open license | VERIFY_BEFORE_USE |
| `MSVC-OUTBOX` | Same as above | VERIFY_BEFORE_USE |
| `MSVC-EVENT-SOURCING` | Same as above | VERIFY_BEFORE_USE |
| `MSVC-DB-PER-SVC` | Same as above | VERIFY_BEFORE_USE |
| `TS-DESIGN-NOTES` | GitHub issues do not carry an explicit reuse license | CITE_ONLY_NO_COPY |

**Note on microservices.io:** If the site owner publishes an explicit open license
(CC-BY or Apache 2.0), it may be promoted to TRAINING_OK. Check the site's
terms and footer before any future ingestion. Do not ingest until confirmed.

### Permanently excluded categories

The following categories are excluded by policy, not just by current status.
No individual source from these categories may enter without Luba's explicit approval:

```
Company engineering blogs (Stripe, Netflix, Uber, GitHub, Cloudflare, etc.)
  — No training license; blog posts carry site copyright by default.

Closed-license books
  — Including Google SRE Book, Designing Data-Intensive Applications, etc.

Public-but-not-open articles
  — A page being publicly readable is not a training license.

GitHub issues and pull request discussions
  — Unless the repository has an explicit contribution license covering issue text.

Any repository with no LICENSE file
  — GitHub's own guidance: no license = no rights to use, change, or distribute.

GPL or AGPL repositories
  — Copyleft propagates to derivative works. Requires explicit decision before inclusion.

Incident reports and postmortems from private companies
  — No license; internal documents published informally.

Jepsen analyses
  — Copyright Kyle Kingsbury; no open license.
```

---

## Section 4 — Source registry (JSON)

This is the machine-readable registry for all TRAINING_OK sources.
This is the input to Phase 1 of EXTERNAL-SOURCES-EXAMINATION-PLAN-v2.md,
replacing the source list in that plan's Groups A through G.

```json
[
  {
    "sourceId": "KUBERNETES-KEPS",
    "title": "Kubernetes Enhancement Proposals",
    "licenseUseClass": "TRAINING_OK",
    "license": "Apache-2.0",
    "targetDomains": ["A_TOPOLOGY", "E_ARBITER_QUALITY", "G_LEARNING", "H_GOVERNANCE"],
    "targetFamilies": [1, 2, 5, 8],
    "allowedUse": ["derived rows", "accepted/rejected examples", "arbiter rules"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "OPENTELEMETRY-SPEC",
    "title": "OpenTelemetry Specification",
    "licenseUseClass": "TRAINING_OK",
    "license": "Apache-2.0",
    "targetDomains": ["A_TOPOLOGY", "E_ARBITER_QUALITY", "H_GOVERNANCE"],
    "targetFamilies": [3, 5, 8],
    "allowedUse": ["observability contracts", "trace propagation rows", "semantic convention arbiters"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "OPENAPI-SPEC",
    "title": "OpenAPI Specification",
    "licenseUseClass": "TRAINING_OK",
    "license": "Apache-2.0",
    "targetDomains": ["B_ROUTING_SELECTION", "F_REGISTRY_STORAGE", "H_GOVERNANCE"],
    "targetFamilies": [1, 3, 5, 8],
    "allowedUse": ["API contract rows", "schema validation arbiters", "compatibility examples"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "GOOGLE-AIP",
    "title": "Google API Improvement Proposals",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0 for content; Apache-2.0 for code samples",
    "targetDomains": ["B_ROUTING_SELECTION", "H_GOVERNANCE", "E_ARBITER_QUALITY"],
    "targetFamilies": [1, 3, 5, 8],
    "allowedUse": ["API design rows", "naming arbiters", "pagination and LRO (Long-Running Operation) decision rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "MICROSOFT-LEARN-ARCH",
    "title": "Microsoft Learn architecture docs (CC-BY 4.0 pages only)",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["A_TOPOLOGY", "F_REGISTRY_STORAGE", "E_ARBITER_QUALITY", "H_GOVERNANCE"],
    "targetFamilies": [1, 2, 3, 5, 8],
    "allowedUse": ["CQRS rows", "circuit breaker rows", "strangler fig rows", "cloud design pattern rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true,
    "perPageVerificationRequired": true
  },
  {
    "sourceId": "NIST-SSDF",
    "title": "NIST Secure Software Development Framework",
    "licenseUseClass": "PUBLIC_DOMAIN_US",
    "license": "Not subject to U.S. copyright",
    "targetDomains": ["G_LEARNING", "H_GOVERNANCE", "E_ARBITER_QUALITY"],
    "targetFamilies": [5, 8],
    "allowedUse": ["secure SDLC rows", "supply-chain governance rows", "security quality gates"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": false,
    "attributionRecommended": true
  },
  {
    "sourceId": "OWASP-ASVS",
    "title": "OWASP Application Security Verification Standard",
    "licenseUseClass": "ATTRIBUTION_SHAREALIKE",
    "license": "CC-BY-SA-4.0",
    "targetDomains": ["E_ARBITER_QUALITY", "H_GOVERNANCE"],
    "targetFamilies": [5],
    "allowedUse": ["security verification rows", "accepted/rejected security controls"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true,
    "shareAlikeApplies": true
  },
  {
    "sourceId": "OWASP-CHEAT-SHEETS",
    "title": "OWASP Cheat Sheet Series",
    "licenseUseClass": "ATTRIBUTION_SHAREALIKE",
    "license": "CC-BY-SA-4.0 (verify repo file before ingestion)",
    "targetDomains": ["E_ARBITER_QUALITY", "H_GOVERNANCE"],
    "targetFamilies": [3, 5],
    "allowedUse": ["secure implementation rows", "negative security examples"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true,
    "shareAlikeApplies": true,
    "perFileVerificationRequired": true
  },
  {
    "sourceId": "CWE-TOP25",
    "title": "CWE Top 25 Most Dangerous Software Weaknesses",
    "licenseUseClass": "FREE_COMMERCIAL_USE_WITH_NOTICE",
    "license": "MITRE CWE Terms of Use",
    "targetDomains": ["E_ARBITER_QUALITY", "H_GOVERNANCE"],
    "targetFamilies": [5],
    "allowedUse": ["weakness detection rows", "secure-code negative examples"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "IETF-RFCS",
    "title": "IETF RFCs — OAuth2, JWT, TLS 1.3, HTTP, etc.",
    "licenseUseClass": "IETF_TRUST_PROVISIONS",
    "license": "IETF Trust Legal Provisions",
    "targetDomains": ["E_ARBITER_QUALITY", "H_GOVERNANCE", "B_ROUTING_SELECTION"],
    "targetFamilies": [3, 5],
    "allowedUse": ["protocol contract rows", "security interoperability rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true,
    "noteOnCodeReuse": "Code components in RFCs have specific reuse rules; verify per RFC before code extraction"
  },
  {
    "sourceId": "SWE-BENCH",
    "title": "SWE-bench",
    "licenseUseClass": "TRAINING_OK",
    "license": "MIT",
    "targetDomains": ["A_TOPOLOGY", "D_EXECUTION_APPLIER", "E_ARBITER_QUALITY", "G_LEARNING"],
    "targetFamilies": [2, 3, 4, 6, 8],
    "allowedUse": ["issue-to-patch rows", "test validation rows", "agent edit-loop rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "RUST-RFCS",
    "title": "Rust RFCs",
    "licenseUseClass": "TRAINING_OK",
    "license": "MIT",
    "targetDomains": ["E_ARBITER_QUALITY", "H_GOVERNANCE"],
    "targetFamilies": [1, 5, 8],
    "allowedUse": ["language design decision rows", "accepted/rejected feature rows", "proposal-debate pairs"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "PYTHON-PEPS",
    "title": "Python PEPs",
    "licenseUseClass": "TRAINING_OK",
    "license": "Public domain",
    "targetDomains": ["H_GOVERNANCE", "C_SEMANTIC_TRANSLATION"],
    "targetFamilies": [1, 5, 8],
    "allowedUse": ["language governance rows", "proposal-debate pairs"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": false,
    "attributionRecommended": true
  },
  {
    "sourceId": "GO-PROPOSALS",
    "title": "Go Proposals",
    "licenseUseClass": "TRAINING_OK",
    "license": "BSD-3-Clause",
    "targetDomains": ["H_GOVERNANCE"],
    "targetFamilies": [1, 5, 8],
    "allowedUse": ["language design decision rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "KAFKA-KIPS",
    "title": "Apache Kafka KIPs",
    "licenseUseClass": "TRAINING_OK",
    "license": "Apache-2.0",
    "targetDomains": ["A_TOPOLOGY", "F_REGISTRY_STORAGE"],
    "targetFamilies": [1, 5, 8],
    "allowedUse": ["distributed topology decisions", "consumer group design rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "CLOUDEVENTS-SPEC",
    "title": "CloudEvents specification",
    "licenseUseClass": "TRAINING_OK",
    "license": "Apache-2.0",
    "targetDomains": ["F_REGISTRY_STORAGE", "A_TOPOLOGY"],
    "targetFamilies": [5, 8],
    "allowedUse": ["event schema governance rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "AOSA-VOL1",
    "title": "Architecture of Open Source Applications vol. 1",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-3.0",
    "targetDomains": ["A_TOPOLOGY", "D_EXECUTION_APPLIER"],
    "targetFamilies": [1, 3, 5],
    "allowedUse": ["OSS architecture pattern rows", "decision narrative rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "AOSA-VOL2",
    "title": "Architecture of Open Source Applications vol. 2",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-3.0",
    "targetDomains": ["A_TOPOLOGY", "D_EXECUTION_APPLIER"],
    "targetFamilies": [1, 3, 5],
    "allowedUse": ["OSS architecture pattern rows", "decision narrative rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "AOSA-500L",
    "title": "500 Lines or Less",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-3.0",
    "targetDomains": ["D_EXECUTION_APPLIER", "C_SEMANTIC_TRANSLATION"],
    "targetFamilies": [1, 3],
    "allowedUse": ["minimal implementation example rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "DOTNET-MICROSERVICES",
    "title": ".NET Microservices Architecture Guide",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["A_TOPOLOGY", "F_REGISTRY_STORAGE"],
    "targetFamilies": [1, 2, 3],
    "allowedUse": ["microservice topology rows", "data pattern rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "SYSTEM-DESIGN-PRIMER",
    "title": "System Design Primer",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["A_TOPOLOGY", "B_ROUTING_SELECTION"],
    "targetFamilies": [1, 5],
    "allowedUse": ["topology rows", "scaling decision rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "EFFECTIVE-GO",
    "title": "Effective Go",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-3.0",
    "targetDomains": ["D_EXECUTION_APPLIER", "A_TOPOLOGY"],
    "targetFamilies": [3],
    "allowedUse": ["concurrency pattern rows", "idiomatic design rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "RUST-ASYNC",
    "title": "Rust Async Book",
    "licenseUseClass": "TRAINING_OK",
    "license": "MIT",
    "targetDomains": ["D_EXECUTION_APPLIER", "A_TOPOLOGY"],
    "targetFamilies": [3],
    "allowedUse": ["async execution pattern rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "REACT-ADVANCED",
    "title": "React.dev advanced topics",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["D_EXECUTION_APPLIER", "C_SEMANTIC_TRANSLATION"],
    "targetFamilies": [3, 7],
    "allowedUse": ["state structure rows", "effect pattern rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "ANGULAR-STYLE",
    "title": "Angular style guide and DI",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["B_ROUTING_SELECTION", "D_EXECUTION_APPLIER"],
    "targetFamilies": [1, 3],
    "allowedUse": ["DI pattern rows", "routing rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "DOTNET-ASPNET-PERF",
    "title": "ASP.NET Core performance and security docs",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["D_EXECUTION_APPLIER", "E_ARBITER_QUALITY"],
    "targetFamilies": [3, 5],
    "allowedUse": ["execution rows", "security rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "EFCORE-CONCURRENCY",
    "title": "EF Core concurrency and performance docs",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["F_REGISTRY_STORAGE", "D_EXECUTION_APPLIER"],
    "targetFamilies": [3, 5],
    "allowedUse": ["registry and storage rows", "concurrency pattern rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "NESTJS-ADVANCED",
    "title": "NestJS advanced documentation",
    "licenseUseClass": "TRAINING_OK",
    "license": "MIT",
    "targetDomains": ["B_ROUTING_SELECTION", "D_EXECUTION_APPLIER", "E_ARBITER_QUALITY"],
    "targetFamilies": [1, 3, 5],
    "allowedUse": ["routing rows", "execution rows — architecture level only, not tutorials"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true,
    "note": "Architecture-level patterns only. Tutorial rows ('how to configure guards') are excluded."
  },
  {
    "sourceId": "PHP-FIG-PSRS",
    "title": "PHP-FIG PSR standards",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["B_ROUTING_SELECTION", "F_REGISTRY_STORAGE"],
    "targetFamilies": [1, 5],
    "allowedUse": ["API contract rows", "registry rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "DOCKER-BEST-PRACTICES",
    "title": "Docker build best practices",
    "licenseUseClass": "TRAINING_OK",
    "license": "Apache-2.0",
    "targetDomains": ["D_EXECUTION_APPLIER", "A_TOPOLOGY"],
    "targetFamilies": [2, 5],
    "allowedUse": ["execution rows", "deployment topology rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "GITHUB-ACTIONS-ADVANCED",
    "title": "GitHub Actions reusable workflows and matrix builds",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["H_GOVERNANCE", "D_EXECUTION_APPLIER"],
    "targetFamilies": [2, 5],
    "allowedUse": ["governance rows", "execution rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  },
  {
    "sourceId": "GCP-ARCH-FRAMEWORK",
    "title": "Google Cloud Architecture Framework",
    "licenseUseClass": "ATTRIBUTION_REQUIRED",
    "license": "CC-BY-4.0",
    "targetDomains": ["H_GOVERNANCE", "E_ARBITER_QUALITY"],
    "targetFamilies": [5],
    "allowedUse": ["governance rows", "arbiter rows"],
    "rawTextIngestionAllowed": true,
    "requiresAttribution": true
  }
]
```

---

## Section 5 — Integration with EXTERNAL-SOURCES-EXAMINATION-PLAN-v2.md

This registry replaces the source list in v2 Groups A through G.
The execution procedure (Phases 0–6 in v2), the output file structure,
the row schema, the arbiter catalog, and the batch examination protocol
all remain in force.

**When executing Phase 1 of v2:** use Section 4 of this document as the
complete source list. Do not add any source not in Section 1 without
first confirming its license and updating this registry.

**Priority order for batch production (unchanged from v2):**

1. Security and governance rows — families 5, 8 (highest gap)
   Sources: OWASP-ASVS, OWASP-CHEAT-SHEETS, NIST-SSDF, CWE-TOP25, IETF-RFCS

2. Architecture and orchestration rows — families 1, 2, 5, 8
   Sources: KUBERNETES-KEPS, OPENTELEMETRY-SPEC, OPENAPI-SPEC, GOOGLE-AIP, MICROSOFT-LEARN-ARCH

3. Bug-fix and code-agent rows — families 2, 3, 4, 6
   Sources: SWE-BENCH; then DEFECTS4J, BUGSINPY, QUIXBUGS after license verification

4. Proposal and debate archive rows — families 1, 5, 8
   Sources: RUST-RFCS, PYTHON-PEPS, GO-PROPOSALS, KAFKA-KIPS, CLOUDEVENTS-SPEC

5. Open-source architecture narrative rows — families 1, 3, 5
   Sources: AOSA-VOL1, AOSA-VOL2, DOTNET-MICROSERVICES, SYSTEM-DESIGN-PRIMER

6. Stack-specific pattern rows — families 1, 3, 5 (architecture level only)
   Sources: NESTJS-ADVANCED, REACT-ADVANCED, ANGULAR-STYLE, DOTNET-ASPNET-PERF, DOCKER-BEST-PRACTICES

---

## Section 6 — Change log

| Version | Date | Change |
|---------|------|--------|
| v1.0 | 2026-05-04 | Initial registry. Replaces v2 plan source groups. Removes AWS Builders Library, Google SRE, Cosmic Python, Martin Fowler, microservices.io, TypeScript GitHub issues. Adds KUBERNETES-KEPS, GOOGLE-AIP, SWE-BENCH per license review. All remaining sources confirmed Apache 2.0, CC-BY, MIT, BSD, or equivalent. |
