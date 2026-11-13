# Round Manifest - Phase 97 - CB4 ext25 Pipeline Chain
# Date: 2026-05-05

---

## Round Purpose

Register `node-pipeline-chain-set-ext25.md` as executable scenarios and verify
that each accepted full-pipeline trace keeps its source anchor through all nine
logical stations.

This round adds:

```text
S17 -> CB4-3.4-811 -> REPROCESS -> AS accepts public clients without PKCE
S18 -> CB4-3.4-812 -> PASS      -> AS rejects non-PKCE public clients
S19 -> CB4-3.4-813 -> REPROCESS -> HS256 multi-party JWT signing
S20 -> CB4-3.4-814 -> PASS      -> RS256 multi-party JWT signing
S21 -> CB4-3.4-815 -> CONCERN   -> IP-only token endpoint throttling
S22 -> CB4-3.4-816 -> PASS      -> per-client-id token endpoint throttling
S23 -> CB4-3.4-817 -> CONCERN   -> OIDC logout without id_token_hint
S24 -> CB4-3.4-818 -> PASS      -> OIDC logout with id_token_hint validation
```

---

## Files Changed

### Code

```text
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecuritySplitterUnit.cs
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityArbiterSelectorUnit.cs
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityTranslatorUnit.cs
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityPlannerUnit.cs
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityArbiterUnit.cs
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityFormatterUnit.cs
C:\Projects\llm_mvp_core\Tests\LogicalOrchestratorAssistantTests.cs
```

### Documentation And Evidence

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CB4-STAGE-5M-EXT25-PIPELINE-CHAIN-EVIDENCE-2026-05-05.json
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-COMPLETION-LEDGER-v17.md
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ROUND-MANIFEST-2026-05-05-PHASE-97-CB4-EXT25-PIPELINE-CHAIN.md
```

---

## Implementation Summary

The security flow now has direct executable scenarios for every base pair in
`node-pipeline-chain-set-ext25.md`.

The new test does not only check final verdicts. For each of the eight traces it
asserts that the same source pair id is preserved by:

```text
node_01_source_pair_id
node_02_source_pair_id
node_03_source_pair_id
node_04_source_pair_id
node_05_source_pair_id
node_06_source_pair_id
node_07_source_pair_id
node_08_source_pair_id
node_09_source_pair_id
```

This is the first CB4 security test that verifies full nine-node trace
continuity for an external-source pipeline-chain file.

---

## Verification

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LogicalOrchestratorAssistant_RunSecurityVerification34"
Result: 5/5 passing

dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LogicalOrchestratorAssistant" --no-build
Result: 19/19 passing

dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA" --no-build
Result: 7/7 passing

dotnet test Tests\LlmFromScratch.Tests.csproj --no-build
Result: 1176/1176 passing

dotnet build LlmFromScratch.sln
Result: Build succeeded, 0 warnings, 0 errors
```

Compliance scan on touched files found no `Console.WriteLine`, direct `throw`,
`.Result`, `.Wait()`, or `GetAwaiter` usage.

---

## Gate Status

```text
All ext25 base pairs represented: PASS
Every ext25 scenario preserves all nine source anchors: PASS
Verdicts match accepted pipeline traces: PASS
Human result keeps trace-specific signal: PASS
External AI calls remain zero: PASS
```

---

## Open Work

This closes ext25 at executable-flow level.

The next external-source target should be selected from the manifest according
to which capability we want to stress next:

```text
arbiter-only breadth -> node-08-arbiter-set-ext files
multi-territory breadth -> additional node-cross-source-set-ext files
development lifecycle corpus -> first quota batch across selected sub-phases
```
