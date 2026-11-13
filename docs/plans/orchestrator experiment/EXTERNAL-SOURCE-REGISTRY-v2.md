# External Source Registry — v2.0
# Orchestration-priority ordering
# Version: 2.0 — 2026-05-05
# Replaces: EXTERNAL-SOURCE-REGISTRY-v1.0.md
# Key change: Sources reordered and annotated by their value for
#             orchestration-focused pair production (Nodes 1, 2, 3, 7)
#             Code-level sources remain listed but are deprioritized until Wave 4

---

## The governing rule (unchanged from v1.0)

No source enters the training set unless its LICENSE file or official terms
explicitly allow reuse, modification, redistribution, or commercial research
and development use. A public GitHub repository is not automatically open-source.
Public access is not a license class. A page being readable does not make it
reusable.

This rule is a hard gate. It applies to every row, every draft, every batch.
A row whose source does not appear in a TRAINING_OK section below is not
promotable regardless of content quality.

---

## Priority Tier 1 — First-party sources (highest orchestration value)

These sources require no license check — they are produced by the project itself.
They are the highest-priority source for Node 1 A sides because they contain real
prompts in the actual language used when asking for development help.

| Source ID | Description | What it provides | Primary nodes |
|---|---|---|---|
| `XIIGEN-SESSION-FILES` | XIIGen Claude Code session transcripts | Real developer prompts across all sub-phases | 1, 2 |
| `XIIGEN-PLAN-DOCUMENTS` | XIIGen architecture plans and session state documents | Multi-phase development request language, architecture decision vocabulary | 2, 3 |
| `XIIGEN-HISTORY-RAG` | XIIGen history RAG batch documents | Cross-phase context examples, development lifecycle transitions | 3, 7 |

**Extraction method for XIIGEN-SESSION-FILES:**

Read each session transcript and identify every message from Luba that:
- Describes a development need ("I need to add...")
- Poses a development question ("Can we...? How should...?")
- Identifies a problem ("The tokens aren't expiring...")
- Requests a design decision ("What is the best way to...?")

Each such message is a candidate Node 1 A side. Copy verbatim. Do not paraphrase.
Assign the sub-phase using the two-question classification method. Write the B
showing the territory array the orchestrator should produce for that prompt.

These pairs are the ground truth for orchestration correctness because they
reflect what the actual user of the system actually types.

---

## Priority Tier 2 — Real engineer request language (high orchestration value)

These sources contain language written by real engineers describing real
development situations. This language is the second-best source for Node 1
A sides after first-party session files.

### Group A — Engineering proposals and enhancement documents

| Source ID | Title | License | Node 1 value | Primary nodes |
|---|---|---|---|---|
| `KUBERNETES-KEPS` | Kubernetes Enhancement Proposals | Apache 2.0 | HIGH — Motivation and User Stories sections contain real engineer request language across sub-phases 3.5, 4.1, 5.4, 7.3 | 1, 2, 3, 7 |
| `GOOGLE-AIP` | Google API Improvement Proposals | CC-BY 4.0 (content) | HIGH — each AIP describes an API design situation from the API user's perspective, maps to sub-phase 3.3 | 1, 2, 3 |
| `RUST-RFCS` | Rust RFC repository | MIT | MEDIUM — motivation sections describe real language design needs; maps to design decision sub-phases | 1, 2 |
| `PYTHON-PEPS` | Python Enhancement Proposals | Public domain | MEDIUM — abstract and motivation sections; maps to design and architecture sub-phases | 1, 2 |
| `KAFKA-KIPS` | Apache Kafka Improvement Proposals | Apache 2.0 | MEDIUM — motivation sections describe distributed system needs; maps to sub-phases 3.1, 4.3 | 1, 2 |

**Fetch locations:**
- KUBERNETES-KEPS: `https://github.com/kubernetes/enhancements` — clone `keps/` directory
- GOOGLE-AIP: `https://github.com/aip-dev/google.aip.dev` — clone `aip/` directory
- RUST-RFCS: `https://github.com/rust-lang/rfcs` — clone `text/` directory
- PYTHON-PEPS: `https://github.com/python/peps` — clone repository
- KAFKA-KIPS: `https://github.com/apache/kafka` — find KIP documents in `docs/`

**Node 1 extraction method for engineering proposals:**

Read the Motivation section (sometimes called Background, Context, or Goals).
Find sentences that describe what an engineer wants to achieve or a problem they
are facing. These are the A sides.

Do not use the technical specification sections as A sides. Those belong to Node 2
and Node 3 vocabulary, not to Node 1 routing inputs.

---

### Group B — Real developer questions (CC-BY-SA 4.0 — attribution required)

Stack Overflow is the largest collection of real developer questions in existence.
Every question represents a developer describing a development situation in their
own words — exactly the language Node 1 must learn to handle.

| Source ID | Title | License | Node 1 value | Primary nodes |
|---|---|---|---|---|
| `STACKOVERFLOW-QUESTIONS` | Stack Overflow questions (selected tags) | CC-BY-SA 4.0 | VERY HIGH — real developer prompt language across all sub-phases | 1 |

**Attribution requirement:** Every pair derived from a Stack Overflow question
must carry the question URL and "CC-BY-SA 4.0 Stack Overflow" in its source
field. The ShareAlike clause applies to derivative works.

**Tag selection for each sub-phase:**

| Sub-phase | Stack Overflow tags |
|---|---|
| 1.1 Design system and tokens | [design-system] [css-variables] [theming] |
| 1.2 Screen and layout structure | [layout] [responsive-design] [grid] [flexbox] |
| 1.3 Interaction model and state transitions | [state-machine] [ui-state] [animation] [transitions] |
| 1.4 Accessibility and compliance | [accessibility] [wcag] [aria] [screen-reader] |
| 2.1 User stories and acceptance criteria | [user-stories] [acceptance-testing] [bdd] |
| 2.3 Integration point specification | [api-integration] [webhook] [event-driven] |
| 3.1 Architecture and component topology | [architecture] [microservices] [monolith] [dependency-injection] |
| 3.2 Data schema and migration design | [database-design] [migration] [schema] [normalization] |
| 3.3 API contract definition | [rest-api] [openapi] [swagger] [api-versioning] |
| 3.4 Security and authorization model | [oauth2] [jwt] [authorization] [authentication] |
| 3.5 Infrastructure and deployment topology | [kubernetes] [docker] [deployment] [infrastructure] |
| 4.1 Backend services | [asp.net-core] [nestjs] [backend] [microservice] |
| 4.2 Frontend and client rendering | [react] [angular] [vue] [rendering] |
| 4.3 Integration and provider connectors | [third-party-api] [sdk] [integration] [connector] |
| 5.1 Unit and contract tests | [unit-testing] [mocking] [tdd] [contract-testing] |
| 5.2 Integration tests | [integration-testing] [end-to-end] [test-environment] |
| 5.3 End-to-end scenario tests | [e2e-testing] [selenium] [playwright] [cypress] |
| 5.4 Performance and load tests | [performance-testing] [load-testing] [benchmarking] |
| 6.2 Regression coverage verification | [regression-testing] [code-coverage] [test-suite] |
| 6.3 Security and compliance review | [security-audit] [penetration-testing] [compliance] |
| 7.3 CI/CD gate configuration | [ci-cd] [github-actions] [pipeline] [deployment-gate] |

**Extraction method for Stack Overflow:**

Use the Stack Exchange Data Explorer (data.stackexchange.com) to query questions
by tag. Fetch the question title and question body. The title is the primary A
side candidate. The body provides context for sub-phase classification.

Filter to questions with a score above 5 — higher-scored questions tend to be
phrased more clearly and represent more common situations.

Do not use the answers — they belong to later-wave pair production.

---

## Priority Tier 3 — Domain vocabulary sources (high value for Nodes 2 and 3)

These sources define what correct work looks like in each domain. They provide
the formal vocabulary that Node 2 must learn to produce, and the structural
patterns that Node 3 must learn to plan around.

### Group C — Architecture and API design

| Source ID | Title | License | Value for | Primary nodes |
|---|---|---|---|---|
| `OPENAPI-SPEC` | OpenAPI Specification | Apache 2.0 | Node 2 sub-phase 3.3 formal vocabulary | 2, 3 |
| `OPENTELEMETRY-SPEC` | OpenTelemetry Specification | Apache 2.0 | Node 2 sub-phases 3.1, 4.3, 5.4 formal vocabulary | 2, 3 |
| `ASYNCAPI-SPEC` | AsyncAPI Specification | Apache 2.0 | Node 2 sub-phase 4.3 event contract vocabulary | 2, 3 |
| `MICROSOFT-LEARN-ARCH` | Microsoft Learn architecture docs | CC-BY 4.0 where stated | Node 3 plan structure patterns for sub-phases 3.1, 3.2, 3.5 | 3, 7 |
| `DOTNET-MICROSERVICES` | .NET Microservices Architecture Guide | CC-BY 4.0 | Node 3 multi-service plan patterns | 3 |
| `SYSTEM-DESIGN-PRIMER` | System Design Primer | CC-BY 4.0 | Node 3 topology plan patterns | 3 |

### Group D — Security domain vocabulary

| Source ID | Title | License | Value for | Primary nodes |
|---|---|---|---|---|
| `OWASP-ASVS` | OWASP Application Security Verification Standard 5.0.0 | CC-BY-SA 4.0 | Node 2 sub-phase 3.4 formal security vocabulary | 2, 7, 8 |
| `OWASP-CHEAT-SHEETS` | OWASP Cheat Sheet Series | CC-BY-SA 4.0 | Node 2 sub-phase 3.4 and 6.3 formal security vocabulary | 2, 7, 8 |
| `NIST-SSDF` | NIST Secure Software Development Framework | Public domain | Node 2 sub-phase 3.4 and 6.3 process vocabulary | 2, 7, 8 |
| `CWE-TOP25` | MITRE CWE Top 25 | Free commercial use with notice | Node 2 sub-phase 6.3 vulnerability vocabulary | 2, 8 |
| `IETF-RFCS` | Selected IETF RFCs | IETF Trust provisions | Node 2 sub-phase 3.4 protocol vocabulary | 2, 8 |

**Note on security sources for Nodes 2 and 7:**

These sources have deep CB4 coverage for Node 8 (arbiter verdict production).
The gap is in Nodes 2 and 7. For Wave 2, use these sources specifically to
write Node 2 formal vocabulary pairs (translating a security-related territory
into the correct ASVS/RFC/CWE vocabulary) and Node 7 arbiter selection pairs
(which review units fire for which security-related result signals).

Do not write more Node 8 pairs from these sources until Nodes 1, 2, 3, and 7
have achieved 17/20 routing accuracy across all sub-phases. Node 8 is already
the deepest-covered node in the corpus.

### Group E — Testing domain vocabulary

| Source ID | Title | License | Value for | Primary nodes |
|---|---|---|---|---|
| `SWE-BENCH` | SWE-bench | MIT | Node 2 sub-phase 5.1 and 5.2 formal test vocabulary | 2, 3 |
| `NIST-NVD` | NIST National Vulnerability Database | Public domain | Node 2 sub-phase 6.3 formal compliance vocabulary | 2 |

### Group F — CI/CD and deployment vocabulary

| Source ID | Title | License | Value for | Primary nodes |
|---|---|---|---|---|
| `GITHUB-ACTIONS-ADVANCED` | GitHub Actions reusable workflows | CC-BY 4.0 | Node 2 sub-phase 7.3 CI/CD gate vocabulary | 2, 3 |
| `DOCKER-BEST-PRACTICES` | Docker build best practices | Apache 2.0 | Node 2 sub-phase 3.5 deployment topology vocabulary | 2, 3 |

---

## Priority Tier 4 — Code-level sources (deferred to Wave 4)

These sources are valuable for producing implementation-level pairs (Nodes 4, 5,
8, and 9) but are not needed until the orchestration-routing nodes (1, 2, 3, 7)
are working correctly. Rushing to produce code-level pairs before routing is
correct means the system produces excellent implementation guidance routed to
the wrong sub-phase.

These sources remain in the registry and retain their TRAINING_OK status. They
are not removed — they are deferred.

**Deferred until Wave 4:**

- NESTJS-CORE, NESTJS-DOCS — implementation patterns for NestJS
- ASPNET-WEBSTACK, DOTNET-REFERENCE-SOURCE — legacy .NET implementation
- EFCORE-SOURCE, EFCORE-CONCURRENCY — database access implementation
- REACT-ADVANCED, ANGULAR-STYLE, ANGULAR-DOCS, REACT-DOCS — frontend implementation
- SQL-SERVER-SAMPLES, ADVENTUREWORKS, WIDEWORLDIMPORTERS — data access
- All sources in Groups 8, 9, 10, 11, 12 from the original registry v1.0

---

## Priority Tier 5 — Routing and agent trace sources (specialized)

These sources are useful for expanding routing variety but are not ground truth.
Use them to add vocabulary diversity to Node 1, not as the primary source of
routing patterns.

| Source ID | Title | License | Note |
|---|---|---|---|
| `SEMANTIC-ROUTER-DATASET` | semantic-router-dataset | MIT | Synthetic — good for shape, not ground truth |
| `AGENTIC-CODING-TESSA` | Agentic-Coding-Tessa | Apache 2.0 | Real agent traces — verify constituent attributions |

---

## Sub-phase to source mapping (quick reference)

For each sub-phase, the sources listed below have confirmed content relevant to
that sub-phase's Node 1 and Node 2 pair production.

| Sub-phase | Primary Node 1 sources | Primary Node 2 vocabulary sources |
|---|---|---|
| 1.1 Design system and tokens | XIIGEN-SESSION-FILES, STACKOVERFLOW-QUESTIONS | GOVUK-DESIGN-SYSTEM, FIGMA-SDS |
| 1.2 Screen and layout structure | XIIGEN-SESSION-FILES, STACKOVERFLOW-QUESTIONS | GOVUK-DESIGN-SYSTEM, PATTERNFLY |
| 1.3 Interaction model | XIIGEN-SESSION-FILES, STACKOVERFLOW-QUESTIONS | GOVUK-DESIGN-SYSTEM |
| 1.4 Accessibility | XIIGEN-SESSION-FILES, STACKOVERFLOW-QUESTIONS | GOVUK-DESIGN-SYSTEM, WCAG (verify license) |
| 2.1 User stories | KUBERNETES-KEPS (user story sections), XIIGEN-SESSION-FILES | EXOTHERMIC-STORY-MAPPING |
| 2.3 Integration point spec | XIIGEN-SESSION-FILES, KAFKA-KIPS | ASYNCAPI-SPEC, OPENAPI-SPEC |
| 3.1 Architecture topology | KUBERNETES-KEPS, XIIGEN-PLAN-DOCUMENTS, STACKOVERFLOW-QUESTIONS | MICROSOFT-LEARN-ARCH, DOTNET-MICROSERVICES |
| 3.2 Data schema | STACKOVERFLOW-QUESTIONS, XIIGEN-SESSION-FILES | ADVENTUREWORKS (deferred), EFCORE-SOURCE (deferred) |
| 3.3 API contract | GOOGLE-AIP, STACKOVERFLOW-QUESTIONS, XIIGEN-SESSION-FILES | OPENAPI-SPEC, GOOGLE-AIP |
| 3.4 Security model | XIIGEN-SESSION-FILES, STACKOVERFLOW-QUESTIONS | OWASP-ASVS, IETF-RFCS, NIST-SSDF |
| 3.5 Infrastructure topology | KUBERNETES-KEPS, XIIGEN-SESSION-FILES | DOCKER-BEST-PRACTICES, OPENTELEMETRY-SPEC |
| 4.1 Backend services | XIIGEN-SESSION-FILES, STACKOVERFLOW-QUESTIONS | NESTJS-DOCS (deferred Wave 4) |
| 4.3 Integration connectors | KAFKA-KIPS, STACKOVERFLOW-QUESTIONS | ASYNCAPI-SPEC, OPENTELEMETRY-SPEC |
| 5.1 Unit and contract tests | STACKOVERFLOW-QUESTIONS, XIIGEN-SESSION-FILES | SWE-BENCH |
| 5.2 Integration tests | STACKOVERFLOW-QUESTIONS, XIIGEN-SESSION-FILES | SWE-BENCH |
| 5.4 Performance tests | KUBERNETES-KEPS, STACKOVERFLOW-QUESTIONS | OPENTELEMETRY-SPEC |
| 6.2 Regression coverage | STACKOVERFLOW-QUESTIONS, XIIGEN-SESSION-FILES | — |
| 6.3 Security compliance review | XIIGEN-SESSION-FILES, STACKOVERFLOW-QUESTIONS | OWASP-CHEAT-SHEETS, NIST-SSDF, NIST-NVD |
| 7.3 CI/CD gate configuration | KUBERNETES-KEPS, XIIGEN-SESSION-FILES | GITHUB-ACTIONS-ADVANCED |

Sub-phases not listed above (1.5, 2.2, 2.4, 3.2, 4.2, 4.4, 4.5, 5.3, 6.1, 6.4,
7.1, 7.2, 7.4) rely primarily on XIIGEN-SESSION-FILES and STACKOVERFLOW-QUESTIONS
for Node 1 A sides, with domain vocabulary sourced from the closest related entry
in the table above. These sub-phases are lower priority because they appear less
frequently in real cross-phase development requests.

---

## Wave assignment for source batches

| Wave | Target nodes | Primary sources | Goal |
|---|---|---|---|
| Wave 1 (complete) | 1-9 sub-phase 3.4 only | OWASP-ASVS, NIST-SSDF, CWE-TOP25, IETF-RFCS | Deep security domain coverage |
| Wave 2 (next) | 1, 2, 3, 7 — all 31 sub-phases | XIIGEN-SESSION-FILES, KUBERNETES-KEPS, GOOGLE-AIP, STACKOVERFLOW-QUESTIONS | Wide orchestration routing coverage |
| Wave 3 | 1, 2, 3, 7 — hard adjacent sub-phase pairs | STACKOVERFLOW-QUESTIONS, OWASP-CHEAT-SHEETS, OPENAPI-SPEC | Disambiguation of adjacent sub-phases |
| Wave 4 | 4, 5, 6, 8, 9 — all sub-phases | NESTJS-DOCS, EFCORE-SOURCE, and code-level sources | Implementation-level pair production |
| Wave 5 | Cross-phase context carry-forward | XIIGEN-HISTORY-RAG, XIIGEN-PLAN-DOCUMENTS | Phase transition pair production |
