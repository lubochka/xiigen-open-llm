# Corpus Completion Ledger v15
# Date: 2026-05-05

---

## Current State

The orchestrator machinery is functional for the registered proof paths, but the
full corpus is not complete.

Current verified code state:

```text
Full suite: 1174/1174 passing
WDA lifecycle checks: 7/7 passing
LogicalOrchestratorAssistant checks: 17/17 passing
Security verification flow: node-01 through node-09 all represented as logical units
```

---

## Newly Closed In Phase 95

The `security-verification-3.4` flow now supports the first three ext24
cross-source combinations:

```text
CB4-3.4-819 -> S7 -> REPROCESS -> no PKCE enforcement plus HS256 multi-party
CB4-3.4-820 -> S8 -> PASS      -> PKCE S256 enforcement plus RS256/JWKS
CB4-3.4-821 -> S9 -> CONCERN   -> IP-only throttling plus End Session without id_token_hint
```

The important learning from this round is detector order. Combined-rule cases
must be recognized before single-rule cases, otherwise the flow can preserve the
right verdict class while losing one of the source obligations.

---

## What Is Proven

The registered CB4 security flow has executable logical-unit coverage for:

```text
S1 -> CB4-3.4-001 -> original clean ASVS PASS
S2 -> CB4-3.4-005 -> original PKCE plain REPROCESS
S3 -> CB4-3.4-009 -> original ASVS/internal-standard CONCERN
S4 -> CB4-3.4-803 -> Action 3 RFC 9700 PKCE enforcement REPROCESS
S5 -> CB4-3.4-807 -> Action 3 IP-only throttling CONCERN
S6 -> CB4-3.4-808 -> Action 3 per-client-id throttling PASS
S7 -> CB4-3.4-819 -> cross-source PKCE plus HS256 REPROCESS
S8 -> CB4-3.4-820 -> cross-source PKCE plus RS256 PASS
S9 -> CB4-3.4-821 -> cross-source throttling plus logout CONCERN
```

---

## What Is Not Complete

The corpus expansion target is still not complete.

The external Wave 1 source folder currently provides 828 base pairs / 1656
instances according to the latest manifest. The executable flow only registers a
small proof slice from those sources.

The broader v4 corpus target still requires:

```text
Real training rows across all 31 development sub-phases
Held-out rows for conformance verification
Transition-evaluation rows for phase and sub-phase carry-forward
Executable registration of remaining security-flow combinations
Completion manifests and round zips for each quota batch
```

---

## Next Gate

Next round:

```text
Register CB4-3.4-822 through CB4-3.4-828 from node-cross-source-set-ext24.md.
```

This will close the ext24 cross-source file at the executable-flow level. After
that, the next useful gate is Stage 5 registration for the broader ext31 Action
3 families, followed by quota-batch production for the development sub-phase
corpus.
