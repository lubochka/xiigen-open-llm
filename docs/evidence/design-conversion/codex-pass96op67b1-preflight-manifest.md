# Codex Pass96OP67B1 Preflight Manifest

claim_scope: Pass96OP67B1 preflight-manifest-only slice for candidate materialization using PATCH_BUNDLE_PLUS_MICRO_SLICES.
evidence_scope: Current Luba Pass96OP67B1 work order, targeted reads of the requested materialization micro-slice rules, full reads of Pass96OP67A inventory and Pass96B-GOV2 method, read-only status/name inventory context, and Get-Item/Get-FileHash metadata for OP67A included existing paths. No temp repo/workspace was created. No candidate materialization was run.
count_claimed: 1 preflight manifest evidence file written; 80 OP67A snapshot candidate include entries classified; 75 included existing paths hashed; 5 tracked artifact deletions recorded; 1 excluded noise path recorded; 0 candidate materialization runs; 0 temp repo/workspace creations; 0 merge-tree runs; 0 product/test/build runs; 0 git write/ref actions.
independent_evidence_count: 1 preflight manifest evidence record. This is not a completed materialization, mergeability proof, review pass, or test/build record.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: yes. The evidence supports only a preflight manifest for the OP67A 80-entry snapshot. Candidate materialization and OP-67 clean mergeability remain not proven.

## Skill Load Status

| file | status | read boundary |
| --- | --- | --- |
| `AGENTS.md` materialization micro-slice rule | found / targeted-read | Read OP-67/temp-proof atomic worker gate and candidate-materialization retry subgate requiring micro-slices by tracked governance/evidence, tracked product/tests, untracked includes, and tracked artifact deletions. |
| `docs/skills/claim-verification-SKILL.md` materialization micro-slice rule | found / targeted-read | Read OP-67 timeout claim gate and candidate-materialization retry claim gate rejecting broad all-path materialization after an incomplete slice. |
| `docs/skills/implementation-integrity-SKILL.md` materialization micro-slice rule | found / targeted-read | Read OP-67 temp-proof integrity gate and candidate-materialization retry integrity gate requiring readback and expected/actual count checks after each class. |
| `docs/evidence/design-conversion/codex-pass96op67a-inventory-only.md` | found / read | Read Pass96OP67A inventory. Bound this manifest to OP67A candidate include total 80 and excluded noise `grep.exe.stackdump`. |
| `docs/evidence/design-conversion/codex-pass96b-gov2-materialization-method-repair.md` | found / read | Read Pass96B-GOV2 method. Applied `PATCH_BUNDLE_PLUS_MICRO_SLICES` preflight manifest guidance and OP67A snapshot/current-drift distinction. |

## Manifest Mode And Count Reconciliation

manifest_mode: OP67A_80_SNAPSHOT.

The included candidate count is intentionally bound to Pass96OP67A's snapshot total: 80. Current worktree drift after OP67A includes later evidence artifacts such as `docs/evidence/design-conversion/codex-pass96op67b-candidate-materialization.md`, `docs/evidence/design-conversion/codex-pass96b-gov2-materialization-method-repair.md`, and this B1 preflight file. Those later files are not silently added to this manifest because the current task says to read Pass96OP67A inventory and classify its include paths. A future worker should switch to `CURRENT_DIRTY_WORKTREE` mode only if Luba explicitly asks for a refreshed current-dirty manifest.

| slice | description | count | candidate include? |
| --- | --- | ---: | --- |
| A | tracked governance/evidence/current docs | 29 | yes |
| B | tracked product/tests | 12 | yes |
| C | untracked includes | 34 | yes |
| D | tracked artifact deletions | 5 | yes, as deletions |
| E | excluded noise: `grep.exe.stackdump` | 1 | no |
| total included | OP67A snapshot candidate include total | 80 | yes |

manifest_hash_coverage: 75/75 included existing paths have byte length and SHA256 recorded below. The 5 tracked deleted paths are recorded as deleted/original path and do not have current file hashes. The excluded noise path is listed separately and is not a candidate include.

## Slice A - Tracked Governance/Evidence/Current Docs

| status | label | path | bytes | sha256 |
| --- | --- | --- | ---: | --- |
| existing tracked modified | Governance/skills | `AGENTS.md` | 396308 | dd630980a1804121187ce9e35695e4551c686da3f0adf3358ca3202d3c5718d7 |
| existing tracked modified | Governance/skills | `CLAUDE.md` | 163900 | 20aced15cc990acc7f586ab60a1a512c541e80f8a3df0633888d1529e10a6500 |
| existing tracked modified | Governance/skills | `docs/SESSION-START-PROMPT.md` | 41560 | afea2d5ea9289dc5626901db203bb1c52f9eaf1a7630db0fa95d19692d6f36f5 |
| existing tracked modified | Governance/skills | `docs/skills/agent-output-format-SKILL.md` | 27058 | 22d9e15316d75510ceef97cb07f1b3c57a4b0f04b49f225ad79288a240e8ba42 |
| existing tracked modified | Governance/skills | `docs/skills/architect-behavior-classifier-SKILL.md` | 88492 | 55bdf13c02a06d0129699b00e6e29faa9e5767dee746feae4f81b48ab190959f |
| existing tracked modified | Governance/skills | `docs/skills/audit-protocol-SKILL.md` | 33882 | e89028c8ae0c1ac96ef9e4d65a119605fe778dcaca0a86abb60c5a84191ebf4f |
| existing tracked modified | Governance/skills | `docs/skills/authority-requirement-binding-SKILL.md` | 17703 | 35607efac3f50f3a00b50dc65618a4fcdac29dbd6bc8fbad72ec13072627cba3 |
| existing tracked modified | Governance/skills | `docs/skills/bug-to-tests-SKILL.md` | 7511 | 491c82abf2120763e1f3a82e313ebc267394376bad055e051a5746ec8ca1bd80 |
| existing tracked modified | Governance/skills | `docs/skills/claim-verification-SKILL.md` | 58999 | 89188358822decbb531cee581c586039037fbdde0378c024f3bdf4f206b40347 |
| existing tracked modified | Governance/skills | `docs/skills/codex-planning-quality-GUIDE.md` | 54854 | c030357a2aaffb2c9de053219b96333b801c2aa5d42ddca01a157cc9f41b9d05 |
| existing tracked modified | Governance/skills | `docs/skills/coverage-completeness-gate-SKILL.md` | 8956 | 1fe49ef9dca6595e89dc85866caa74daf12882a6ad47308cf8dde4cc93ef4142 |
| existing tracked modified | Governance/skills | `docs/skills/e2e-test-matrix-SKILL.md` | 10092 | a4c1af58afe0966699b9c7872d05a46a017a6c51d216836f3fee5d18fa86282b |
| existing tracked modified | Governance/skills | `docs/skills/generated-code-review-SKILL.md` | 29308 | 89a5587a76b1c03d531546c2470e7d231830f61b1eb5b0081724fab592ea52d4 |
| existing tracked modified | Governance/skills | `docs/skills/goal-delivery-completeness-SKILL.md` | 36188 | 2012ccff230463c0af2a6ffc3c0da50230052bd4c1d96ac7f0e64dce32972755 |
| existing tracked modified | Governance/skills | `docs/skills/implementation-integrity-SKILL.md` | 38187 | 18c33dd8c4d74770d5daa0a8350bd18fa3f0756f13c05d5e2138e1ab5600dcf1 |
| existing tracked modified | Governance/skills | `docs/skills/no-product-decisions-SKILL.md` | 11041 | 7009f0b516427a5d3b49dc0d71b2dc698cc71f83d00256ff874c8de595293bda |
| existing tracked modified | Governance/skills | `docs/skills/output-readability-gate-SKILL.md` | 39345 | 3f10734fa35c4ad9723ccaef615298f8be0637f022fc369dc99980959a29afc0 |
| existing tracked modified | Governance/skills | `docs/skills/output-skepticism-SKILL.md` | 21108 | afe71fa8a2e944d0b65cf6a34acd5ddec9ee6294f89c086c66769086d45d4c8a |
| existing tracked modified | Governance/skills | `docs/skills/phase-planning-SKILL.md` | 64696 | deb0a3c1277dfc0b3b1222bff9b16b6b8a084a4a0ef592e9967a42a28491e947 |
| existing tracked modified | Governance/skills | `docs/skills/plan-review-SKILL.md` | 211965 | 1b0db5f9cb36991492b385cd29704593773f7931bb986aea4c0ed53de6678714 |
| existing tracked modified | Governance/skills | `docs/skills/planning-session-startup-SKILL.md` | 118131 | 03068ce4da31400af2f628832c3c215987eabae5f0e24063bff2c77ac24fa073 |
| existing tracked modified | Governance/skills | `docs/skills/reconnaissance-gate-SKILL.md` | 27869 | 8aebecadd4f129e3606eb4a178368dd060785b1bf0dc3341549728c854dabf20 |
| existing tracked modified | Governance/skills | `docs/skills/reference-inventory-and-source-map-SKILL.md` | 25682 | ff8388e9c40b3ba18c13e59dc3996b34081507abd7da50d8c5fb18ecb7e3bbb6 |
| existing tracked modified | Governance/skills | `docs/skills/self-verification-SKILL.md` | 6879 | 1a14f71dd7d6d2dc09911451cd479dc0e834afa5fe0f6d54b55bca18224bc3fb |
| existing tracked modified | Governance/skills | `docs/skills/specificity-calibration-SKILL.md` | 18463 | bbcc5ac54a61be4683176be965b44c9227048b043ba699b1d4d6dc7d79a877a5 |
| existing tracked modified | Governance/skills | `docs/skills/test-failure-triage-SKILL.md` | 8136 | 151992d53fe19d0d3988882f4474d2d77cc96137f820b3abc9a15a383b507920 |
| existing tracked modified | Governance/skills | `docs/skills/test-integrity-SKILL.md` | 41213 | fd545554bc746e7851119cc5bd9ec59b122f3194cd577a036220a5c17af1805b |
| existing tracked modified | Governance/skills | `docs/skills/trainable-architecture-decision-SKILL.md` | 41787 | 3a751518e20acbf3d4a044e0f4d63a05ff41b24efde84710addb8367492fc200 |
| existing tracked modified | Governance/skills | `docs/skills/verification-before-completion-SKILL.md` | 33142 | 237b465bafd658a7cb81b685760e6edef4310fc259a9633c19e7cebbe81a19d6 |

## Slice B - Tracked Product/Tests

| status | label | path | bytes | sha256 |
| --- | --- | --- | ---: | --- |
| existing tracked modified | Product | `44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs` | 32869 | 467da330c8aeb0a63b1f342f0aa174c70eb75fc0c20f9280ce892cf16b0f0a02 |
| existing tracked modified | Product | `44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs` | 44971 | a951ac924af49c60f3d9305e69131238261b2fb5f93ea22194cd06ffeb19bf3b |
| existing tracked modified | Product | `44.DesignConversionDomain/DesignConversionDpoDisplay.cs` | 16914 | 0f343b74f2eeaf8312fbabdfccf81df1f152f3de9713bf7f925d43e91aebca6f |
| existing tracked modified | Product | `44.DesignConversionDomain/DesignConversionDpoStorage.cs` | 14650 | 018c7454ffe19f029aa70e472b8c99dcaefa49b4f7ecac577f0ec0dc061565a3 |
| existing tracked modified | Product | `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs` | 38107 | 81e776a585bee965824da169d02e2d1169200afb8d61018b2a6b7753e1a57aed |
| existing tracked modified | Product | `44.DesignConversionDomain/DesignConversionVisualJudge.cs` | 25927 | 3cd84abdd8f63cea073e4d9d4ffc2dbb3da08247f0ed5a07482c2094ba866324 |
| existing tracked modified | Product | `44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs` | 11848 | eb3c9a5ecf496bcbee60867714adabddf2dfa18ecb6fe03a10f52d3ab39d03af |
| existing tracked modified | Tests | `Tests/DesignConversionBrowserQaProviderTests.cs` | 16955 | 33ad31bcc8c3492cfa20f65698d5023b1bd7baff14e7ab490336881700d09a00 |
| existing tracked modified | Tests | `Tests/DesignConversionDpoStorageTests.cs` | 21176 | 8c968adb7ec8b9e8c06c9a2c11a657def94d01ca2c30d27d9b4d56fb005293f1 |
| existing tracked modified | Tests | `Tests/DesignConversionInternalServiceTests.cs` | 32227 | 35e1cc5f327f34a4d17de4f3368261e0690009e9b011dd0eaa84f1b33d2c0211 |
| existing tracked modified | Tests | `Tests/DesignConversionVisualJudgeTests.cs` | 18489 | 64d3529e2a89a18f09a3e2c6ff277c988582b136ef5849880883784f86602962 |
| existing tracked modified | Tests | `Tests/DesignConversionVisualQaBoundaryPolicyTests.cs` | 12514 | 361afa1ce8a1a5b6e531c4cc6b2244534d2a036e79d6bd967082530ae18e3bed |

## Slice C - Untracked Includes

| status | label | path | bytes | sha256 |
| --- | --- | --- | ---: | --- |
| existing untracked include | Product | `44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs` | 9762 | 6a6103b1c43cee5ceb756575661e89723e891d6f4255725605dcd5e20307101b |
| existing untracked include | Product | `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs` | 44137 | 2b148a2a5c72320e0212186320b94fd9c79a5c966d3f33d00335d3d139ef5b4b |
| existing untracked include | Product | `44.DesignConversionDomain/DesignConversionTrainingMeasurementGate.cs` | 23221 | 700efe8ae2d463f6cea61a37adfcf7f1db3aa63199f4df7339cfec93de28cda3 |
| existing untracked include | Tests | `Tests/DesignConversionDpoUsefulnessApiTests.cs` | 11620 | 20601807a2c431f71ca987bd1ef9649b249757a5b81789460f6b2b9eb567d94d |
| existing untracked include | Tests | `Tests/DesignConversionLocalFullLoopE2eTests.cs` | 20075 | ff2365be7719d99f5864f17ee84084cd83be506f59c5f69404b0e31fdf746fa0 |
| existing untracked include | Tests | `Tests/DesignConversionTrainingMeasurementGateTests.cs` | 13856 | 260551c976b33aef315da02a697a057e8d46ab1c2a125f0e67b697fbcfcdb3e2 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass74-live-host-probe.md` | 19672 | b2805dd6269e03dce50359d8491e0a6c0fedf51fc144e0d2a7c8ce5a9a249ea3 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass75-platform-gateway-proxy.md` | 13539 | 31e46d66576db97dbdcec64f0bb52ace9b4ba228643f47fa2fa96aa953ba3e36 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass76-platform-flow-two-process-probe.md` | 25069 | 67b3f10aed1b0357a1dd9c19df8997b1771c8940403588c75e1627051d9ca919 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass77-caller-client-to-platform-flow.md` | 24038 | 347835efebb3cd22cf0e6d7d58c3d4b37a12caf24066ea2dd9733f12a9a9effb |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass78-browser-render-qa.md` | 5144 | 1d9ba699949e13aab8a1f0fdbdf29c543fcb1856f445544cb4bd4a6e25724467 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass79-visual-judge-provider-boundary.md` | 7898 | 112a212d1dd09b444d703ec80e7143a1e37ff87f6590ff5b81bbc9508807598e |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass80-all-dirty-mergeability.md` | 10062 | 56a8ca4bc9f6c37c3ab5bbece08311d86959a6a023c2fa3e47704079d5686b63 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass81-storage-training-artifact-db.md` | 11561 | 426291844805805b07909d2a2b9693c0a3460bc259de6a56659842f41bf0632c |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass82-training-measurement-gate.md` | 12080 | ed0e7330c2b6f847848481c6e70104fac990c1dc50cc5c536e2ec73bce20f5e6 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass83-all-dirty-mergeability.md` | 11272 | a5ecfea2bed4c7d1b1bf75769e0899823c81e414b91c5601f4a3f752dc8e6879 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass84-holistic-readiness-simulation.md` | 27172 | cf05c3dd9e4124b5f09b7658940a746f500534c507fab33ff43b5758e13706ab |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass85-post-pass84-mergeability.md` | 13914 | 7e4bd0a2329f1d8ce519dbd735195ff5a269de3ef3564c80246530cc8c770908 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass86r-dpo-usefulness-api-verification.md` | 8110 | c36ae86bdcf041cccc7cfaf5a76907778e7777a72c7a8c86f321f30df635df66 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass87-local-full-loop-e2e.md` | 7149 | 2e8fb00f0decd3d00a231b568d84462bca44ff3c7c39b8c7167fe7ca850346cd |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass87gov1-subagent-microcheck-loop-repair.md` | 10947 | 5617fae0f9dc2596bbeec2da46dd875b2f3208c280052e675d05cbbe35feabfe |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass87gov2-exact-literal-repair.md` | 4603 | 22b989b1967f0281b5c7090d5257797ea62b4ab13c3d0e9609408a089a2e3af9 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass87r2-mergeability-risk-report.md` | 16257 | 7a20b7b4f4e66b83961334e0f4c8e54f9aca7dba0c981379ca62247a9b9f596a |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass87s-main-merge-conflict-diagnosis.md` | 13643 | ed24a617bbf6f16c2193dc4e5bf3aa86fa20160f81a399abede1f278c935b794 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass87t-governance-merge-repair.md` | 14121 | 75669c567ab6b8cb262256bcf96b13114da72ad136c8822b3e06eb1c71147d96 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass88-remaining-queue-status-board.md` | 14575 | 6bfd7b496dea9d66270380ad12cbd8fc9fd4d050b161d4c9e440f47e4ef75e6d |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass89-op67-closure-plan.md` | 16895 | 5595c0af7ce05d0fa91ca4c0100fecaa0d2ff77c0e70c0654869d35f3f0ffe5a |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass90-governance-overlap-repair.md` | 9770 | a77807ee240b35bb8f314f568a7e558f8708f800043adbe7baa06e0f0548d351 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass91b-candidate-tree-blocker-and-cleanup.md` | 9674 | 591559c5c19d3dab5b80f388def2915987fa0e4b14430a35d5647382e0c9b79f |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass93-full-access-no-approval-cleanup-governance-repair.md` | 3716 | df1834ae71e997d65c8168e917a155e72c12690c61327089e02efd886669d3a4 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass95-skill-stack-mojibake-repair.md` | 8462 | 363c23ae7af4b4cd3b73f317f4179054ffc6e3de24c5fc648ebd88a5a02f52bb |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass95r-skill-stack-mojibake-identity-recovery.md` | 9761 | 7d0c986f3455dbc8dd63c6aa62f440d23cb5a794a7a4d253708e73a2d07c5064 |
| existing untracked include | Evidence generated before this slice and intentionally included | `docs/evidence/design-conversion/codex-pass96gov-op67-timeout-workorder-repair.md` | 8326 | a86122a6728e0df513192220ccabb1f8e28d110868384f1b025e3f911f1d4c08 |
| existing untracked include | Evidence generated by OP67A after its pre-write inventory commands, included for a post-Pass96OP67A candidate | `docs/evidence/design-conversion/codex-pass96op67a-inventory-only.md` | 20359 | e36ea534b8a5ce28bd30b15855b0f8cbbf9b3d85b331db075fa8900c556ba1f4 |

## Slice D - Tracked Artifact Deletions

| status | original path |
| --- | --- |
| tracked deleted | `artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass-empty.sh` |
| tracked deleted | `artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass-main.js` |
| tracked deleted | `artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass.sh` |
| tracked deleted | `artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/ssh-askpass-empty.sh` |
| tracked deleted | `artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/ssh-askpass.sh` |

## Slice E - Excluded Noise

| status | path | bytes | candidate include? | reason |
| --- | --- | ---: | --- | --- |
| excluded noise | `grep.exe.stackdump` | 1013 | no | OP67A classified this as stackdump/noise, not product, test, governance, skill, or codex evidence. |

## Commands Used

Allowed commands used for this manifest:

```text
rg -n "materialization|micro-slice|PATCH_BUNDLE_PLUS_MICRO_SLICES|Pass96|OP67" AGENTS.md docs/skills/claim-verification-SKILL.md docs/skills/implementation-integrity-SKILL.md docs/evidence/design-conversion/codex-pass96op67a-inventory-only.md docs/evidence/design-conversion/codex-pass96b-gov2-materialization-method-repair.md
git -c safe.directory=C:/Projects/llm_mvp_core status --short
Get-Content -LiteralPath AGENTS.md
Get-Content -LiteralPath docs/skills/claim-verification-SKILL.md
Get-Content -LiteralPath docs/skills/implementation-integrity-SKILL.md
Get-Content -LiteralPath docs/evidence/design-conversion/codex-pass96op67a-inventory-only.md
Get-Content -LiteralPath docs/evidence/design-conversion/codex-pass96b-gov2-materialization-method-repair.md
Get-Item -LiteralPath <manifest path>
Get-FileHash -Algorithm SHA256 -LiteralPath <manifest path>
Get-Content -LiteralPath docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md
Select-String count checks for slice rows and SHA256 rows
rg -n "^(<<<<<<<|=======|>>>>>>>)" docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md
rg -n "\x{00D0}|\x{00D1}|\x{00C2}|\x{00E2}|\x{FFFD}" docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md
git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md
rg -n "[ \t]+$" docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md
```

No temp repo/workspace, copy/materialization, patch apply, git add, write-tree, merge-tree, tests/builds, commit/stage/push/switch/checkout/merge/rebase/reset/stash/clean, destructive command, or write outside this evidence file was performed.

## Files Changed

files_changed:

- `docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md`

## Verification

verification:

- evidence_file_readback: PASS. `Get-Content -LiteralPath docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md` read back this file after creation.
- count_totals_match_pass96op67a: PASS. Row count check returned A=29, B=12, C=34, D=5, E=1, included_total=80, hash_rows=75.
- conflict_marker_scan: PASS. `rg -n "^(<<<<<<<|=======|>>>>>>>)" docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md` returned no matches.
- configured_mojibake_marker_scan: PASS. `rg -n "\x{00D0}|\x{00D1}|\x{00C2}|\x{00E2}|\x{FFFD}" docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md` returned no matches.
- git_diff_check_on_evidence_file: PASS. `git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md` exited 0 with no output.
- extra_trailing_whitespace_scan: PASS. `rg -n "[ \t]+$" docs/evidence/design-conversion/codex-pass96op67b1-preflight-manifest.md` returned no matches.

## Next Recommended Worker

next_recommended_worker: Pass96OP67C candidate-materialization micro-slice worker.

The next worker should materialize only if explicitly authorized. It should use this OP67A_80_SNAPSHOT manifest and proceed in order: A tracked governance/evidence/current docs, B tracked product/tests, C untracked includes, D tracked artifact deletions. It must not run merge-tree proof, tests, builds, product implementation, staging, commits, branch switches, checkout/merge/rebase/reset/stash/clean in the original repo, or cleanup outside any declared future temp path.

## Final Verdict

final_verdict: PASS96OP67B1_PREFLIGHT_MANIFEST_WRITTEN_AND_VERIFIED_MATERIALIZATION_NOT_RUN.

I did not create a temp repo/workspace. I did not materialize a candidate. I did not apply a patch, run git add, write tree, or merge-tree. I did not implement product features, run tests/builds, stage/commit/push/switch/checkout/merge/rebase/reset/stash/clean, write outside this evidence file, write `C:\Users\lubao\Documents\xiigen`, or write `claude-*` files.
