# INFRA-WIN11-SAC-01 — policy state observation and next native LoRA check

**14 October 2026.** This is an **evidence record of reported Windows diagnostic results**, not an independently executed Windows query. Original Code Integrity events were captured before the later SAC-state result and remain historical. All current outcomes are limited to the fields below.

```yaml
original_failure: WINDOWS_CODE_INTEGRITY_BLOCK
original_policy: VerifiedAndReputableDesktop
original_policy_guid: 0283ac0f-fff1-49ae-ada1-8a933130cad6
original_dll: LoRA.dll
original_process: testhost.exe
original_dll_authenticode: NotSigned
original_event_ids: [3033, 3077]
original_event_time: NOT_SUPPLIED
earlier_CiTool_access_denied: "0x80070005"  # historical observation
current_Smart_App_Control_value: 0  # user report; 0=Off
current_Smart_App_Control: OFF
current_VerifiedAndReputableDesktop_Is_Currently_Enforced: false
source_of_current_policy_status: USER_REPORTED_CITOOL_OUTPUT
current_Windows_Code_Integrity_block_for_new_attempt: NOT_ESTABLISHED
protected_capture: NOT_VERIFIED
assertions: NOT_VERIFIED
RP01_status: ENVIRONMENT_BLOCKED  # status of old unverified native check, NOT a claim that SAC policy is currently enforced
native_post_off_retest: NOT_RUN
next_action: ONE_TARGETED_LORA_LOAD_RETEST
Student_DPO_update: NOT_PERFORMED_BY_THIS_RECORD
independent_P_C0_T_quality: NOT_MEASURED
```

## Interpretation and precedence

- The original `LoRA.dll` code-integrity block of `testhost.exe` remains valid evidence **for that past attempt**.
- Later SAC `0/Off` and `VerifiedAndReputableDesktop → Is Currently Enforced: false` indicate **that specific policy is not currently enforced**. No new DLL or host block is proved by the earlier event IDs.
- `CiTool 0x80070005` belongs to an earlier attempt to list policies; subsequent reported policy output has greater relevance to current enforcement. There is no reason to re-run `CiTool -r` or edit Windows Security based only on historic records.
- `ENVIRONMENT_BLOCKED` is retained for traceability of the original **unverified LoRA check**; operational next status is `NATIVE_RETEST_PENDING`, not `ACTIVE_SAC_BLOCK`, `ALGORITHM_FAILED` or `TEST_PASSED`.
- Successful native load proves only environment reachability and whatever specific deterministic assertions actually ran, **not** DPO generalization, LoRA efficacy or accepted Student update.

## The only authorized next diagnostic action (one run)

Reuse **the exact previously blocked native LoRA test** using its original test name, runtime, target architecture and real DLL build. First record UTC start, DLL path/SHA256 and test ID; run once; collect exit/stdout/stderr and whether LoRA actually loaded and relevant assertions executed. Read **new** Code Integrity events 3033/3077 **after** that start time with process/DLL and any new policy ID. Do not start a broad `dotnet test`, Windows GitHub Actions, entire package suite or security configuration repair. If the original test identifier is unavailable, return `RETEST_NOT_RUN / ORIGINAL_TEST_NOT_IDENTIFIED` and locate it before execution.

| Exact observation after one retry | Environment verdict | RP01 next move |
| --- | --- | --- |
| LoRA.dll is actually loaded and required assertions executed, without a new matching block | `NATIVE_LOAD_UNBLOCKED` | Continue narrowly to lawful LoRA/DPO checks and independent C# P/C0/T; do not infer gain |
| NEW 3033/3077 for exact DLL and testhost after the attempt | `NEW_CODE_INTEGRITY_BLOCK` | Record actual new policy/signature and investigate only that cause; do not assume SAC reactivated |
| Test fails but there is no new Code Integrity block | `NON_CI_RUNTIME_FAILURE` | Inspect actual .NET architecture/dependency/test cause, not SAC settings |
| Test skipped, wrong binary, unknown case or no genuine native load evidence | `INVALID_EVIDENCE` | Preserve NOT_VERIFIED; identify one correct narrow test |
| No execution performed | `RETEST_NOT_RUN` | Do not claim environment success/failure or LoRA quality |

**Full executor prompt, original/updated provenance, GOOD/BAD/EDGE and output schema:** [INFRA-WIN11-SAC-NATIVE-DLL-BLOCK-2026-10-14.md](INFRA-WIN11-SAC-NATIVE-DLL-BLOCK-2026-10-14.md). **E1 independent tasks remain parallel:** [RP01 V4](RP01-ACTIVE-EXECUTION-V4.md), RP01.00B/02A, and RP02/RP09 source-family exam design.

**Source boundary:** this record was written based on the user's reported numerical result and CiTool line. It did not inspect new host event timestamps, test logs, DLL hash or actual local files.
