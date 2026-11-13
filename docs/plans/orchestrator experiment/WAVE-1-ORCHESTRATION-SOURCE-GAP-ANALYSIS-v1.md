# Wave 1 Orchestration Source Gap Analysis
# Version: 1.0 - 2026-05-05

---

## Source Reviewed

Archive:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1.zip
```

Manifest inside archive:

```text
wave-1/WAVE-1-STAGE-1-ROUND-MANIFEST-2026-05-05.md
```

Observed archive shape:

```text
143 markdown pair files under wave-1/CB4-stage-3-pairs
approximately 3.46 MB of markdown pair material in that folder
manifest records 2,436 pair instances
manifest records 1,218 base pairs
sub-phase focus: 3.4 security and authorization model
```

---

## What Wave 1 Teaches Well

Wave 1 is deep and valuable, but it is concentrated.

It teaches:

```text
security and authorization design;
OAuth and OIDC normative handling;
JWT validation and trust boundaries;
mTLS and DPoP token binding;
PAR and PKCE enforcement;
token storage and logging safety;
scope enforcement;
issuer and audience validation;
refresh token rotation and revocation behavior;
MUST versus SHOULD severity calibration;
PASS, REPROCESS, and CONCERN verdict distinctions;
source-authority preservation across OWASP, NIST, CWE, and IETF material.
```

This is excellent material for:

```text
3.4 security and authorization model;
6.3 security and compliance review;
5.2 integration tests for identity and tenant isolation;
7.3 CI/CD gates for security release blocking;
security-related transition examples.
```

---

## What Wave 1 Does Not Teach

Wave 1 does not teach the whole development area.

It does not provide strong coverage for:

```text
1.1 design system and tokens
1.2 screen and layout structure
1.3 interaction model and state transitions
1.4 accessibility and compliance
1.5 motion and animation
2.1 user stories and acceptance criteria
2.2 business rules outside security rules
2.4 data model boundaries outside identity/security data
3.1 architecture topology outside security architecture
3.2 schema and migration design
3.5 infrastructure and deployment topology outside TLS/security concerns
4.2 frontend rendering
4.4 data migration execution
4.5 documentation
5.3 end-to-end scenario testing outside security
5.4 performance and load testing
6.1 functional review
6.2 regression coverage verification
6.4 accessibility audit
7.1 test scenario automation
7.2 regression suite maintenance
7.4 coverage and reporting outside security evidence
```

These are not failures of Wave 1. They are source boundaries.

---

## Orchestration-Relevant Additions From Wave 1

Wave 1 adds task types that should be promoted into orchestration learning.

| Task type | Lifecycle placement |
|---|---|
| normative severity calibration | 3.4, 6.3, 7.3 |
| source authority distinction | 2.2, 3.4, 6.3 |
| trust boundary detection | 3.1, 3.4, 4.1, 5.2 |
| token lifecycle evidence | 3.4, 4.1, 5.2, 6.3 |
| proof-of-possession validation | 3.4, 4.3, 5.2, 6.3 |
| revocation and rotation verification | 3.4, 4.1, 5.2, 6.3, 7.3 |
| conformance claim rejection | 6.3, 7.3 |
| concern versus reprocess verdict | 6.3, 7.4 |

These task types must appear in prompt-level teaching rows, not only in
node-08 arbiter rows.

---

## Transition Examples Wave 1 Can Support

Wave 1 can support these transitions immediately:

### 3.4 -> 4.1

Security model to backend service implementation.

Example carry-forward:

```text
tenant isolation must be enforced at every resource-server authorization point;
token validation is not enough without scope and tenant boundary validation.
```

### 4.1 -> 5.2

Backend implementation to integration tests.

Example carry-forward:

```text
integration tests must simulate cross-tenant access attempts, expired tokens,
wrong audience, wrong issuer, and revoked refresh-token families.
```

### 5.2 -> 6.3

Integration tests to security review.

Example carry-forward:

```text
security review must not accept passing happy-path tests as evidence that
tenant isolation and token-binding failure paths are covered.
```

### 6.3 -> 7.3

Security review to CI/CD gate.

Example carry-forward:

```text
release gate must block when a MUST-level security requirement is failed or
when a required security evidence artifact is absent.
```

---

## Missing Orchestration Sources

The next source search should target:

```text
UI/UX design process sources for Phase 1;
requirements engineering sources for Phase 2;
architecture decision and system design sources for 3.1, 3.2, 3.5;
implementation planning sources for Phase 4;
test strategy sources for Phase 5;
QA review and accessibility audit sources for Phase 6;
QA automation and CI/CD governance sources for Phase 7;
transition-specific sources showing how decisions become constraints in later phases.
```

Until those sources exist, their rows remain `source_gap` or local XIIGen
source-backed rows.

---

## Immediate Use In O1

Wave 1 is used directly in O1.

It supplies:

```text
security design constraints for 3.4;
backend implementation constraints for 4.1;
integration test obligations for 5.2;
security review verdict standards for 6.3;
release blocking rules for 7.3.
```

It does not supply the full O1 architecture. XIIGen internal documents supply
the architecture and topology parts.

---

## Last Updated

2026-05-05.
