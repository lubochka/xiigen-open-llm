# Source Registry — Batch ES-2026-05-08

Every source used in this batch with license state, attribution, supported
contracts, and which scenarios consume it.

2026-05-11 routing-flow correction: source entries that support
`PromptRoutingInputCompilerUnit` or `MultiSubPhaseRouterUnit` support station
pairs only. They do not independently support accepted routing promotion unless
the same scenario is connected to Layer 0-F flow-level evidence:
compiled document, routing decisions, knowledge-edge state, flow/factory
resolution, and final routing verdict. Without that evidence, routing status is
`PAIRSET_READY_ROUTING_FLOW_PENDING`.

---

## License gate

The license gate follows `EXTERNAL-SOURCE-REGISTRY-v7.md` lines 65–69
exactly:

```
MIT             — accepted (no attribution required, copyright notice retained)
Apache 2.0      — accepted (no attribution required, NOTICE retained where present)
CC BY 4.0       — accepted with attribution
CC BY-SA        — REJECTED (share-alike incompatible with closed corpus)
AGPL            — REJECTED
```

**On RFC, MITRE CWE, and NIST publications.** v7 does not list a separate
"public domain" top-level acceptance category. These sources are admitted
under the existing license states by the registry's Family 7 (line 496–499)
and Family 8 (line 569) entries:

```
license_state          = external_license_checked
attribution_required   = false
```

The registry treats RFC, MITRE, and NIST publications as
`external_license_checked` because they originate from a US-government or
inter-organisation publication path that lawyers have already cleared. The
parenthetical `(public domain)` next to each in the registry is an
attribution note, not a license-gate category. This batch follows the same
convention — every RFC / CWE / NIST citation carries
`source_license_state: external_license_checked` and
`attribution_required: false`. No source is admitted under a "public
domain" gate that is not in v7.

---

## Sources used in this batch

### Family 4 — Architecture Decision Records

| Source | License | Attribution | Used in |
|--------|---------|-------------|---------|
| `joelparkerhenderson/architecture-decision-record` | MIT | retained | ES-S1 |
| `arc42/arc42-template` | MIT | retained | ES-S1, ES-S7 |
| `npryce/adr-tools` | MIT | retained | ES-S1 |

Authority: ADR template structure (Context / Decision / Consequences) maps
directly to ARCH 3.1 boundary decisions. arc42 building-block view supplies
the dependency-direction vocabulary.

### Family 5 — Orchestration Pattern Documentation

| Source | License | Attribution | Used in |
|--------|---------|-------------|---------|
| `temporalio/documentation` (saga, retry, compensation) | MIT | retained | ES-S2, ES-S7 |
| `dapr/dapr` documentation (observability) | Apache 2.0 | NOTICE retained | ES-S2 |
| `apache/airflow` tutorial DAG docs | Apache 2.0 | NOTICE retained | ES-S7 |

Authority: Temporal saga documentation explicitly states that compensation
activities run only when forward activities have already taken effect — the
correct dependency direction for ES-S7 Type B chosen.

### Family 6 — API Contract Specifications

| Source | License | Attribution | Used in |
|--------|---------|-------------|---------|
| `OAI/OpenAPI-Specification` v3.1 | Apache 2.0 | NOTICE retained | ES-S4, ES-S8 |
| `asyncapi/spec` | Apache 2.0 | NOTICE retained | ES-S4 |
| Google AIP-180 (compatibility) | CC BY 4.0 | "© Google LLC, CC BY 4.0" | ES-S8 |
| `json-schema-org/json-schema-spec` | MIT | retained | ES-S4 |

Authority: OpenAPI 3.1 §4.8.5 (Schema Object) defines the shape of a request
header parameter. AIP-180 defines breaking-change taxonomy for ES-S8.

### Family 7 — Testing, RFC, CWE

| Source | License | Attribution | Used in |
|--------|---------|-------------|---------|
| `testcontainers/testcontainers-dotnet` | MIT | retained | ES-S3 |
| `microsoft/testfx` (xUnit / MSTest pattern) | MIT | retained | ES-S3 |
| `microsoft/playwright` documentation | Apache 2.0 | NOTICE retained | ES-S6 (Type A) |
| IETF RFC 6749 — OAuth 2.0 | external_license_checked (PD note) | none required | ES-S5 |
| IETF RFC 7519 — JSON Web Token | external_license_checked (PD note) | none required | ES-S5 |
| IETF RFC 8470 — Using Early Data in HTTP (Idempotency) | external_license_checked (PD note) | none required | ES-S4 |
| IETF RFC 9110 — HTTP Semantics | external_license_checked (PD note) | none required | ES-S4 |
| IETF RFC 8725 — JWT Best Current Practices | external_license_checked (PD note) | none required | ES-S5 |
| MITRE CWE-285 — Improper Authorization | external_license_checked (PD note) | none required | ES-S6 |
| MITRE CWE-287 — Improper Authentication | external_license_checked (PD note) | none required | Stage 9 |

Authority: RFC MUST/SHOULD/MAY language is the ground truth for arbiter pass
or concern verdicts at Node 08. CWE entries supply the natural Type A rejected
content: every CWE has a vulnerable demonstrative example that maps to a
plausible wrong answer.

### Family 8 — NIST Publications

| Source | License | Attribution | Used in |
|--------|---------|-------------|---------|
| NIST SP 800-53 rev. 5 (AC-3 Access Enforcement) | external_license_checked (PD note) | none required | ES-S6 |
| NIST SP 800-53 rev. 5 (AU-2 Audit Events) | external_license_checked (PD note) | none required | ES-S2 |
| NIST SP 800-160 v2 (systems engineering) | external_license_checked (PD note) | none required | ES-S1 |

Authority: NIST AC-3 enforcement language — "the system enforces approved
authorizations" — is a MUST-level requirement that grounds the 6.3 review
verdict in ES-S6.

---

## Excluded sources (deliberately not used)

```
OWASP Testing Guide              — CC BY-SA, share-alike incompatible
Stack Overflow answers           — CC BY-SA
xUnit Test Patterns (book)       — no open license
xiigen-internal session docs     — first-party, not external
Wave 1 corpus                    — not used as chosen content; reserved for
                                   registered-flow comparison branches only
```

ES-S5 uses RFC 7519 + RFC 6749 (under `external_license_checked`) as the
chosen content for the 3.4 comparison branch. Wave 1 corpus is not consumed
in this batch.

---

## Per-scenario source-to-contract map

```
ES-S1  arc42 building-block view + ADR Decision section
       → DomainTranslatorUnit (ARCH formal statement vocabulary)
       → SchemePlannerUnit    (3.1 ARCH branch with depends_on=[])
       → ArbiterUnit          (NIST SP 800-160 boundary integrity check)

ES-S2  Temporal observability docs + Dapr observability
       → DomainTranslatorUnit (3.5 strategy vocabulary: signals, alerts, SLOs)
       → SchemePlannerUnit    (3.5 → 7.3 dependency)
       → ArbiterUnit          (NIST AU-2 audit event coverage)

ES-S3  Testcontainers PostgreSQL module + testfx fixture pattern
       → DomainSplitterUnit   (5.2 packet, prompt_fragment about real container)
       → DomainProcessorUnit  (test scenario derivation)
       → ApplierUnit          (accepted test plan)

ES-S4  RFC 8470 + RFC 9110 + OpenAPI 3.1 header parameter shape
       → MultiSubPhaseRouterUnit (3.3 + 5.2 routing decisions)
       → SchemePlannerUnit    (3.3 contract → 5.2 tests dependency, Type B target)

ES-S5  RFC 7519 JWT + RFC 6749 OAuth (registered comparison for 3.4)
       → MultiSubPhaseRouterUnit (3.4 + 3.3 + 5.2)
       → AdaptiveLogicalOrchestrator (3.4 PASS vs 3.3/5.2 KNOWLEDGE_EDGE)
       → ArbiterUnit          (RFC 7519 §4.1.1 iss claim required)

ES-S6  CWE-285 demonstrative example + NIST SP 800-53 AC-3
       → DomainSplitterUnit   (6.3 review packet)
       → ArbiterUnit          (concern verdict citing AC-3)
       → FormatterUnit        (review finding, not redesign)

ES-S7  Temporal saga compensation + Airflow DAG anti-pattern
       → SchemePlannerUnit    (forward activity → compensation, Type B target)
       → DomainProcessorUnit  (compensation reasoning)

ES-S8  OpenAPI 3.1 + AIP-180 breaking-change taxonomy
       → PromptRoutingInputCompilerUnit (CODE_Q intent verb)
       → MultiSubPhaseRouterUnit (3.3 explanatory routing)
       → FormatterUnit        (explanation, no implementation)
```

---

## License-state field per pair

Every pair in this batch carries the coverage label:

```
source_license_state: external_license_checked
attribution_required: true | false
attribution_text:     <only when CC BY 4.0 is used; otherwise empty>
```

CC BY 4.0 sources (AIP-180 in ES-S8 only) carry the attribution text
`"© Google LLC, CC BY 4.0"`. All other sources are MIT, Apache 2.0, or public
domain and carry `attribution_required: false`.

---

## Last Updated

2026-05-08.
