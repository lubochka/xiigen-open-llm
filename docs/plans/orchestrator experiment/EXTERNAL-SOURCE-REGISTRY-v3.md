# External Source Registry
# Orchestration teaching edition
# Version: 3.0 - 2026-05-05
# Supersedes: EXTERNAL-SOURCE-REGISTRY-v2.md

---

## Why v3 Exists

Version 2 correctly moved source priority toward orchestration and away from
code-level pair production. It still treated orchestration mainly as Nodes 1, 2,
3, and 7.

Version 3 changes the registry target to the full prompt-level orchestration
learning cycle:

```text
user prompt -> lifecycle split graph -> phase sequence -> context carry-forward
-> transition verification -> arbiter selection -> sub-task plan -> human result
```

External sources are accepted only when they can teach at least one part of that
chain.

---

## License Rule

No source enters the learning set unless its license or official terms allow the
intended reuse.

Public access is not a license.

Every row derived from an external source carries:

```text
source_id;
source_url_or_local_path;
license_class;
attribution_required;
allowed_use_note;
source_state;
```

Rows without this information stay out of the training corpus.

---

## Source States

| State | Meaning | Can teach? |
|---|---|---|
| `first_party` | XIIGen-owned project documents, code, sessions, ledgers, plans | yes |
| `external_license_checked` | External source with acceptable license or terms | yes |
| `attribution_required` | External source allowed with attribution or share-alike constraints | yes, with attribution field |
| `source_gap` | Correct behavior is to ask for missing source authority | yes, as gap behavior |
| `corruption_case` | Deliberately wrong input used to teach rejection | yes, as negative evidence |
| `verify_before_use` | Candidate source not yet license-cleared | no |

---

## Orchestration Unit Roles

Sources are mapped to orchestration units, not only to node numbers.

| Unit role | What the source must teach |
|---|---|
| `orchestration-prompt-splitter` | How real prompts imply domains, phases, sub-phases, conflicts, and knowledge edges. |
| `phase-sequencing-unit` | Which development work must happen before other work and which can run in parallel. |
| `context-carry-forward-unit` | Which upstream decisions, constraints, gaps, and rejected items must survive into downstream phases. |
| `transition-verifier-unit` | How to detect a downstream phase ignoring, contradicting, or misusing upstream context. |
| `arbiter-selection-unit` | Which review authority applies to each planned branch or result. |
| `sub-task-planner-unit` | How to convert an accepted lifecycle graph into actionable sub-tasks. |
| `human-result-formatter-unit` | How to show can-proceed, blocked, rejected, unknown, and next-decision sections honestly. |

Node 1 through Node 9 pairs remain useful, but the registry now classifies
sources by the orchestration behavior they can teach.

---

## Priority Tier 1 - XIIGen First-Party Sources

These are the highest-value sources because they contain the real methodology,
real prompts, real corrections, and real artifacts of this project.

| Source ID | Material | Primary orchestration value |
|---|---|---|
| `XIIGEN-SESSION-FILES` | User/executor/reviewer session transcripts | Real user prompt phrasing and review corrections. |
| `XIIGEN-ORCHESTRATOR-PLANS` | Orchestrator experiment plans and WDA docs | Expected phase/sub-phase topology and transition behavior. |
| `XIIGEN-CORPUS-LEDGERS` | Corpus plans, manifests, completion ledgers | Teaching-cycle state, source states, batch evolution. |
| `XIIGEN-CODEBASE` | `C:\Projects\llm_mvp_core` | Runtime shape of logical units, flows, orchestrators, inspector, and tests. |
| `XIIGEN-INSPECTOR-DOCS` | Inspector design and performance correction docs | Resource constraints and trace inspection behavior. |

### Extraction Use

Use first-party prompts as A sides for prompt-splitter rows.

Use accepted reviews as B-side correctness evidence.

Use correction sessions as adaptation examples.

Use inspector performance history as resource-measurement teaching material.

---

## Priority Tier 2 - Wave 1 Security Authority Sources

Wave 1 is license-checked and deep. It is not a full lifecycle source. It is a
security authority source.

| Source ID | License class | Strongest lifecycle use |
|---|---|---|
| `OWASP-ASVS` | CC BY-SA 4.0 | 3.4 security model, 6.3 security review, 7.3 security release gates |
| `NIST-SSDF` | Public domain | secure development process, evidence type, review timing |
| `CWE-TOP25` | free commercial use with notice | weakness category, severity, review authority |
| `IETF-RFCS` | IETF Trust provisions | OAuth/OIDC/JWT/TLS normative rules |

### Orchestration Use

Wave 1 teaches the security branches of O1 and O4:

```text
3.4 -> 4.1 security implementation constraints;
4.1 -> 5.2 security integration-test obligations;
5.2 -> 6.3 security review evidence;
6.3 -> 7.3 release-blocking automation.
```

Wave 1 must not be used to claim UI/UX, functional spec, performance, or QA
automation maturity outside the security branch.

---

## Priority Tier 3 - Real Engineering Request Sources

These sources provide natural engineering language for prompt-splitter training.

| Source ID | License | Strong orchestration use |
|---|---|---|
| `KUBERNETES-KEPS` | Apache 2.0 | multi-phase design, implementation, testing, rollout, production-readiness prompts |
| `GOOGLE-AIP` | CC BY 4.0 | API contract and API lifecycle design prompts |
| `KAFKA-KIPS` | Apache 2.0 | distributed-system feature proposals and integration prompts |
| `RUST-RFCS` | MIT | language and architecture design decisions |
| `PYTHON-PEPS` | Public domain | design proposals with motivation and compatibility sections |

### Extraction Use

Use motivation, goals, non-goals, user stories, rollout, compatibility, testing,
and migration sections.

Do not use dense specification paragraphs as prompt-splitter A sides. Use them
as formal vocabulary and expected-plan evidence.

---

## Priority Tier 4 - Developer Question Sources

Stack Overflow and similar Q&A sources can teach real developer wording, but
their license and attribution burden must be honored.

| Source ID | License | Use |
|---|---|---|
| `STACKOVERFLOW-QUESTIONS` | CC BY-SA 4.0 | prompt wording and ambiguity examples |

Every derived row must preserve:

```text
question URL;
author attribution if required by export format;
CC BY-SA 4.0 marker;
share-alike note;
```

Use question titles and bodies for prompt wording. Do not use answers as
truth unless the answer license and attribution are carried forward and the
answer is explicitly reviewed.

---

## Priority Tier 5 - Domain Vocabulary Sources

These sources teach formal translations, sequencing, and review criteria.

| Source ID | License | Primary use |
|---|---|---|
| `OPENAPI-SPEC` | Apache 2.0 | API contract vocabulary and API transition constraints |
| `ASYNCAPI-SPEC` | Apache 2.0 | event-driven integration contract vocabulary |
| `OPENTELEMETRY-SPEC` | Apache 2.0 | observability, telemetry, performance evidence vocabulary |
| `MICROSOFT-LEARN-ARCH` | CC BY 4.0 where stated | architecture and deployment planning patterns |
| `DOTNET-MICROSERVICES` | CC BY 4.0 | component topology and service-boundary patterns |
| `SYSTEM-DESIGN-PRIMER` | CC BY 4.0 | architecture tradeoff language |
| `SWE-BENCH` | MIT | bug-fix and test-evidence task language |

These are not always prompt sources. Often they are B-side authority sources.

---

## Source Mapping To The Five Real Scenarios

| Scenario | Primary first-party sources | Primary external sources |
|---|---|---|
| O1 multi-tenant authentication addition | codebase, orchestrator plans, security flow evidence | Wave 1 security authority sources, OpenAPI |
| O2 wave-gated flow registration | corpus ledgers, flow registry plans, logical unit plans | Kubernetes KEPs, Google AIP |
| O3 performance regression investigation | inspector design history, performance correction sessions | OpenTelemetry, Kubernetes KEP rollout/perf sections |
| O4 external-source integration under license constraint | external source session, corpus plans, artifact reviewing protocol | Wave 1 sources, Stack Overflow only if attributed |
| O5 skill advisor full lifecycle | all XIIGen orchestration docs and codebase | mix of KEPs, AIP, OpenAPI, accessibility/test sources as license-cleared |

---

## Current Source Gaps

The registry still needs stronger license-cleared sources for:

```text
UI/UX design systems and accessibility;
functional specification and business rules;
data migration planning;
frontend rendering and state management;
QA functional review;
QA automation coverage/reporting;
phase-transition methodology evidence outside XIIGen.
```

Until sources are selected, those rows remain first-party or `source_gap`.

---

## Active Source Use

Immediate source use is O1:

```text
XIIGen internal architecture and codebase sources for topology and implementation context;
Wave 1 sources for security authority and security review context;
OpenAPI/API contract sources for 3.3 contract branch if license-cleared in local registry;
source_gap rows for any missing source authority.
```

---

## Last Updated

2026-05-05. Version 3 aligns the registry with complete orchestration learning
sets rather than isolated node-pair production.
