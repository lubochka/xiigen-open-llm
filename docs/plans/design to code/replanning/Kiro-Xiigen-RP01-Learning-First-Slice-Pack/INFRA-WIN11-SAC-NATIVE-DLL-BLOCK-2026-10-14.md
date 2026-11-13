# INFRA-WIN11-SAC-01 — unblock local native LoRA load without distorting RP01 learning evidence

> **Latest observed-status record:** [INFRA-WIN11-SAC-01 results — Off/not enforced; one native LoRA retest NOT_RUN](INFRA-WIN11-SAC-01-RESULT-2026-10-14.md). The old requirement to switch Smart App Control is historical; don't refresh policies or run Windows CI.

<!-- INFRA-SAC-LIVE-RECHECK-2026-10-14 -->
## CURRENT EVIDENCE UPDATE — policy OFF, native retest pending (2026-10-14)

**Evidence provenance:** the device user reports a Windows Smart App Control numeric result `0` (**Off**) and later `CiTool` policy output `VerifiedAndReputableDesktop → Is Currently Enforced: false`. This GitHub author did NOT run Windows commands, inspect raw logs, change the policy, sign binaries or execute LoRA locally. Historical `0x80070005` was from an **earlier** CiTool attempt; it is not the result of the later successful policy-state observation.

```yaml
original_failure: WINDOWS_CODE_INTEGRITY_BLOCK
original_process: testhost.exe
original_dll: LoRA.dll
original_signature: NotSigned
original_event_ids: [3033, 3077]  # historical events, timestamps must be retained
original_policy_name: VerifiedAndReputableDesktop
original_policy_id: 0283ac0f-fff1-49ae-ada1-8a933130cad6
earlier_CiTool_query: 0x80070005 # historical, not current enforcement proof
current_SAC_numeric: 0 # USER_REPORTED
current_SAC_state: OFF # USER_REPORTED
current_policy_enforcement: false # USER_REPORTED exact "Is Currently Enforced: false"
current_SAC_block_of_new_LoRA_attempt: NOT_DEMONSTRATED
protected_capture: NOT_VERIFIED
previous_native_assertions: NOT_VERIFIED
one_targeted_native_retest: NOT_RUN
RP01_native_check_status: ENVIRONMENT_BLOCKED # historical check still unverified; NOT active-policy proof
next_step: RECHECK_ONE_PREVIOUSLY_BLOCKED_LORA_LOAD
```

**Decision NOW:** STOP security-setting work. **DO NOT** turn SAC off again, touch Defender/SmartScreen/UAC, run `CiTool.exe -r`, remove policies, change DLL signing or start VM migration based only on old events. There is no current policy-enforcement problem to fix in those reports. An actual post-OFF native load is still unverified: `ENVIRONMENT_BLOCKED` applies only to the **uncompleted earlier native proof**, while the specific SAC policy status is `OFF / NOT_ENFORCED`. This distinction prevents both false "still blocked" and false "LoRA tests pass".

### NEXT executable model-ready task — ONE native test, no Windows CI

**Role / skills:** independent local Windows infrastructure executor; read the **current** RP01 selected `LoRA.dll` call/test, original failed command/test name, this status, and actual `K/docs/skills/infrastructure-discovery-SKILL.md`, `test-failure-triage-SKILL.md`, `code-examination-SKILL.md` as applicable. Record the exact read sections, current core HEAD, selected test method, DLL path and SHA256. Existing E1 algorithm/test scope stays untouched.

**Copyable prompt:**

> CURRENT TASK: confirm SAC remains OFF and this policy is not enforced using the already recorded diagnostic evidence, **without changing settings or running CiTool refresh**. From the original `testhost.exe` failure locate the **specific narrow LoRA load test** (command, configuration, test name, executable/assembly architecture). First record exact DLL SHA256 and `Get-AuthenticodeSignature` as context, and the wall-clock start time. Run exactly that previously failing test **ONCE**, recording command, process ID if available, UTC start/end, stdout/stderr, exit code, whether the test actually attempted/achieved native `LoRA.dll` load and its exact SHA. Inspect only Code Integrity 3033/3077 **newer than start time** matching DLL + process + policy, not historical events. Do not rerun all tests, Windows CI or modify assertions. GOOD: actual load verified, test relevant assertions truly executed and PASS, with no new matching block → `NATIVE_LOAD_UNBLOCKED` and RP01.01B/03A native checking may continue. BAD: old 3077 called current block, or test exits zero after being skipped and counts as LoRA PASS. EDGE: new matching 3077 despite `Is Currently Enforced:false` → record event's actual *new* policy identity, signer and time, investigate without assuming SAC is cause; missing original test command → `RETEST_NOT_RUN`, reconstruct only the exact test; failure without 3077 → diagnose ordinary load/architecture/dependency/test assertion failure separately; no silent bypass or repeated retries.

**Stop/return contract:**
```yaml
task: INFRA-WIN11-SAC-01-ONE-POST-OFF-RETEST
policy_state: OFF_USER_REPORTED
policy_enforced: FALSE_USER_REPORTED
dll_sha256_and_exact_path: ACTUAL | NOT_OBSERVED
replayed_original_test_identifier_and_command: ACTUAL | NOT_FOUND
execution_utc_start_end_exit: ACTUAL | NOT_RUN
did_load_LoRA_dll: YES | NO | NOT_VERIFIED
assertions_really_executed: YES | NO | NOT_VERIFIED
NEW_matching_code_integrity_events_after_start: [actual records] | NONE_OBSERVED | NOT_CHECKED
verdict: NATIVE_LOAD_UNBLOCKED | NEW_CODE_INTEGRITY_BLOCK | NON_CI_RUNTIME_FAILURE | RETEST_NOT_RUN | INVALID_EVIDENCE
RP01_algorithm_proof: NOT_MEASURED # unless separately executed with actual receipts
P_C0_T_quality_gain: NOT_MEASURED
next_owner: RP01.01B/RP01.03A only after actual native runtime load is proven
```

**Current outcome remains `RETEST_NOT_RUN`.** The immediately following material is the *original* troubleshooting plan from before the policy was confirmed OFF; any requirement to switch SAC, troubleshoot CiTool, sign binaries or set up a VM is **historical/fallback only and does not run now**. Only a **new** failure can justify a new bounded diagnosis.
<!-- END INFRA-SAC-LIVE-RECHECK-2026-10-14 -->

**Date:** 2026-10-14. **Owner:** independent Windows developer-environment/infrastructure task, **NOT** an RP01 algorithm or machine-learning test-coverage obligation. **Current state after later user diagnostics:** `SAC_OFF / VERIFIED_AND_REPUTABLE_DESKTOP_NOT_ENFORCED / NATIVE_RETEST_PENDING`; original native attempt remains `ENVIRONMENT_BLOCKED` as **historical** evidence only; diagnosis below is **USER-REPORTED evidence**, not raw event logs independently inspected by this planner. No system security setting changed, no PC command executed and no LoRA test run by publication of this document.

## Reported evidence and scoped inference

| Observed by user | Sound conclusion | NOT proved |
| --- | --- | --- |
| `Get-AuthenticodeSignature LoRA.dll` → `NotSigned` | the built file has no Authenticode digital signature | that signing is the only policy decision factor or every XIIGEN DLL is blocked |
| Code Integrity events **3033 / 3077**, `testhost.exe` attempting to load `LoRA.dll` | actual Windows code-integrity block of **that binary/attempt** under recorded policy | that DPO/LoRA implementation is algorithmically incorrect |
| Policy GUID `0283ac0f-fff1-49ae-ada1-8a933130cad6` | matches Microsoft `VerifiedAndReputableDesktop`, the built-in policy used by **Smart App Control On** | independent fresh `CiTool -lp` proof of entire current policy inventory |
| Earlier `CiTool.exe -lp` → `0x80070005`; later user report `Is Currently Enforced: false` | Earlier query was denied; later policy output reports enforcement now false | that the new native LoRA load has already succeeded |

**Primary Microsoft references:**
- [Inbox App Control policies — VerifiedAndReputableDesktop and GUID](https://learn.microsoft.com/en-us/windows/security/application-security/application-control/app-control-for-business/operations/inbox-appcontrol-policies)
- [CiTool `--list-policies` (`-lp`) documentation](https://learn.microsoft.com/en-us/windows/security/application-security/application-control/app-control-for-business/operations/citool-commands)
- [Smart App Control FAQ, exceptions and re-enabling](https://support.microsoft.com/en-us/windows/security/threat-malware-protection/smart-app-control-frequently-asked-questions)

## Goal and NON-goals

**Current goal:** without changing Windows security again, **re-execute once** the narrow previously blocked `LoRA.dll` loading test under the now-reported OFF/not-enforced policy and capture new evidence. Only investigate alternate VM/signing/security configurations if that new exact test still fails. This proves native functionality, **not** DPO learning gain.

**NOT:** change DPO objective, loosen masks/optimizer assertions, rewrite LoRA implementation, call a Windows 3077 block a model regression, claim quality-gain, disable all Windows security automatically, remove `.cip` files or policies, command `CiTool -rp`, or rerun all Windows Actions until something becomes green.

## Copyable, narrow, novice-friendly executor instruction

**HISTORICAL PRE-OFF TASK (superseded by CURRENT EVIDENCE UPDATE above):** **ROLE:** Windows infrastructure diagnostic operator (not Trainer/Teacher/Astra/Student). **Read actually:** `K/docs/skills/infrastructure-discovery-SKILL.md`, `code-examination-SKILL.md`, `systematic-debugging-SKILL.md`, `test-failure-triage-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md`; cite opened sections. Context: this ticket, original RP01.01B/03A selected native LoRA call, current `testhost.exe` attempt and user's event evidence. Pin `P_HEAD`, `K_HEAD`, DLL hash/path/architecture/config/time, and exact native caller.

> **TASK:** (1) Without changing security, confirm `LoRA.dll` path/SHA and `Get-AuthenticodeSignature` status, event ID 3077/3033 PolicyId, file and `testhost.exe` time; never infer other DLL statuses. (2) In `Windows Security → App & browser control → Smart App Control settings`, read On/Off/Evaluation, record result, do not change it without explicit informed device-owner choice. (3) From an **elevated PowerShell**, execute **diagnostic-only** `CiTool.exe -lp`; if Access Denied persists, record independent `CITOOL_ACCESS_DENIED` and use GUI/event evidence rather than changing system policy. (4) Recommend **one** owner-selected solution: A. isolate development in a VM where the native assembly can run while host protection remains enabled; B. appropriately signed/trusted binary and verify SAC actually permits it (self-signature alone is not assurance of trust/reputation); C. on a personal device, user **voluntarily** turns Smart App Control Off, accepting reduced protection and that ability to turn it back On depends on Windows version/configuration. DO NOT disable Defender/SmartScreen/UAC, tamper with policy files or silently flip system state. (5) After the chosen change, restart affected test host where necessary and run **one narrow previously blocked native-load smoke** (not full `dotnet test` or CI), capturing native DLL full path/hash, process, result, exit code and new 3033/3077 timestamps. (6) If load now succeeds, mark `NATIVE_LOAD_UNBLOCKED` and hand off RP01's *still unexecuted* mathematical/LoRA correctness verification and later independent P/C0/T. If still blocked, retain `ENVIRONMENT_BLOCKED`; identify *new specific* event/other policy rather than asserting algorithm failure. No Student update/quality result may be inferred.

**GOOD:** SAC On and matching 3077 for the exact unsigned LoRA; isolated VM permits same authorized native loading; only then run targeted local sign/LoRA state guard. **Why:** isolates environmental cause without masking learner quality.

**BAD:** make `LoRA.dll` signature appear green by editing tests, deleting Windows CI, disabling all protections, or marking training `IMPROVE` after one DLL loads. **Why:** changes the wrong layer and contaminates scientific evidence.

**EDGE/RECOVERY:**
- `CiTool -lp` as admin still Access Denied → preserve error, GUI status and exact Code Integrity event; optional IT/security owner escalation; no direct policy deletion.
- Smart App Control Off but same 3077 → do NOT force original hypothesis; investigate current event GUID, other application-control policy, trust and process architecture.
- DLL signed but still rejected → valid signature != trusted/reputable, review signature chain/reputation/policy rather than sign repeatedly.
- No local rights to toggle or managed endpoint → choose approved VM/environment path or request device admin decision.
- Code review succeeds but native load fails → continue READ_ONLY `codex review`, 00B source tracing, 02A frozen scientific design and RP02/RP09 independent non-native examinations. Only native affected steps remain `ENVIRONMENT_BLOCKED`.
- Same native test changes source/config during retry → comparison INVALID; pin exact DLL SHA/caller and preserve old event evidence.

## Output contract and minimal focused verification

```yaml
task: INFRA-WIN11-SAC-01
P_HEAD: exact
K_HEAD: exact
permissions_and_user_choice: read_only | VM | sign_trusted | SAC_Off_consented | managed_block
smart_app_control_GUI: On | Off | Evaluation | UNKNOWN
LoRA_file_SHA_path: actual or NOT_OBSERVED
Authenticode: NotSigned | Valid | Other | NOT_RUN
code_integrity_events: [actual Event ID, time, policy GUID, process, DLL]
CiTool_lp_elevated: exact output | 0x80070005 | NOT_RUN
before_native_load: ENVIRONMENT_BLOCKED
after_single_native_load: NATIVE_LOAD_UNBLOCKED | ENVIRONMENT_BLOCKED | NOT_RUN
changed_security: explicit device-owner choice only, or NONE
focused_probe_command_and_exit: actual | NOT_RUN
DPO_material_optimizer_correctness: NOT_EXAMINED_UNLESS_SEPARATELY_RUN
Student_optimizer_update: NOT_PERFORMED
heldout_P_C0_T_gain: NOT_MEASURED
next_owner: RP01.01B/03A only when selected native runtime actually unblocked
```

No new *coverage quota*. A blocked native check is **NOT_RUN / ENVIRONMENT_BLOCKED**, not a passing test; after unblocking, a changed deterministic LoRA permission/sign/journal/mask contract still requires its minimal scoped runnable proof. Full RP01 learning utility still requires an independent frozen lawful domain exam and actual ordinary consumer.

## Reporting / scheduling

Separate infrastructure status from the RP01 learner scoreboard: `SAC_OFF_POLICY_NOT_ENFORCED` (user-reported), `NATIVE_RETEST_PENDING` (not executed), historical `WINDOWS_CODE_INTEGRITY_BLOCK`, `TRAINER_STATE_NOT_OBSERVED`, and `STUDENT_GAIN_NOT_MEASURED`. Never interpret the old `NATIVE_ENVIRONMENT_BLOCKED` as an active enforced SAC policy without a new event. Does NOT block static Codex review, source mapping `RP01.00B`, frozen panel `RP01.02A`, or unrelated RP02/RP09 domain work; it **does** block claiming actual local LoRA runtime execution until a genuine native load succeeds.
