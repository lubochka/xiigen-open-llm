# Round Manifest - Phase 95 - CB4 Cross-Source Broadening
# Date: 2026-05-05

---

## Round Purpose

Broaden the registered `security-verification-3.4` flow from single-rule
Action 3 probes into cross-source combinations from `node-cross-source-set-ext24.md`.

The round adds three scenarios:

```text
S7 -> CB4-3.4-819 -> REPROCESS -> no PKCE enforcement plus HS256 multi-party
S8 -> CB4-3.4-820 -> PASS      -> PKCE S256 enforcement plus RS256/JWKS
S9 -> CB4-3.4-821 -> CONCERN   -> IP-only throttling plus End Session without id_token_hint
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
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CB4-STAGE-5K-CROSS-SOURCE-EVIDENCE-2026-05-05.json
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-COMPLETION-LEDGER-v15.md
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ROUND-MANIFEST-2026-05-05-PHASE-95-CB4-CROSS-SOURCE.md
```

---

## Implementation Summary

Cross-source detectors now run before single-rule detectors in
`SecuritySplitterUnit` and `SecurityArbiterUnit`.

This is the key behavioral guard. Without it:

- CB4-3.4-819 could collapse into the CB4-3.4-803 PKCE-only case.
- CB4-3.4-821 could collapse into the CB4-3.4-807 throttling-only case.

The new test verifies that the combined source pair survives through node-01 and
node-08 and that the final human result names the combined rule signal.

---

## Verification

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LogicalOrchestratorAssistant_RunSecurityVerification34"
Result: 3/3 passing

dotnet test Tests\LlmFromScratch.Tests.csproj --filter "LogicalOrchestratorAssistant" --no-build
Result: 17/17 passing

dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA" --no-build
Result: 7/7 passing

dotnet test Tests\LlmFromScratch.Tests.csproj --no-build
Result: 1174/1174 passing

dotnet build LlmFromScratch.sln
Result: Build succeeded, 0 warnings, 0 errors
```

Compliance scan on touched files found no `Console.WriteLine`, direct `throw`,
`.Result`, `.Wait()`, or `GetAwaiter` usage.

---

## Gate Status

```text
S7 cross-source verdict preserved: PASS
S8 cross-source pass preserved: PASS
S9 concern-graduation rule preserved: PASS
Node-01 source pair anchors preserved: PASS
Node-08 source pair anchors preserved: PASS
External AI calls remain zero: PASS
```

---

## Open Work

The remaining ext24 cross-source pairs are not yet represented in the executable
flow:

```text
CB4-3.4-822 through CB4-3.4-828
```

The next round should either add the remaining seven ext24 combinations or split
them into a smaller batch if one-pair-per-risk-family isolation is preferred.
