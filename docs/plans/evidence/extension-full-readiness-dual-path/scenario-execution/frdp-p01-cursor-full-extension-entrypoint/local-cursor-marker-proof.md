# Local Cursor marker proof - no-send route

CURRENT TASK: obtain the closest honest local Cursor proof for `frdp-p01-cursor-full-extension-entrypoint`.
NOT THE TASK: send a prompt, use external AI or web, claim full Cursor readiness, claim product readiness, stage, commit, or push.
EVIDENCE-ONLY: this packet proves only a local Cursor program marker-file route for the same scenario marker.

## What was tested

The worker created [local-cursor-marker.txt](local-cursor-marker.txt) with the marker `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint`, then invoked the installed local Cursor program against that file. This is a no-send local marker-file route: no prompt was sent, no extension command was invoked, and no external AI was used by this worker.

This improves the local side from older marker-only screenshot evidence to a fresh local Cursor program launch tied to the same marker file. It is still not enough for a full joined pass because the missing proof is a real Cursor extension command or host route reaching the same application programming interface / программный интерфейс приложения handler path.

## Local command evidence

| Check | Result |
|---|---|
| Marker file | `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/local-cursor-marker.txt` |
| Cursor command | `C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd` |
| Cursor version output | `3.8.23`; commit `7cf19b7482706625cdb70db3211b7dd035b7aa30`; architecture `x64` |
| Cursor processes before launch | `0` |
| Launch command | `cursor.cmd --reuse-window C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint\local-cursor-marker.txt` |
| Launch error | `null` |
| Cursor processes after launch | `5` |
| Main process command line contained marker file | `true` |
| Main window title observed | `Cursor` |
| Prompt or message sent by worker | `false` |
| External AI intentionally used by worker | `false` |
| Web/browser intentionally used by worker | `false` |
| Network isolation proven | `false` |

Main process command line observed:

```text
C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe --reuse-window C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint\local-cursor-marker.txt
```

## Join decision

Accepted as a narrow local program marker-file proof for the same scenario marker.

Not accepted as a full local user interface / пользовательский интерфейс proof, because it does not prove prompt intake, extension command activation, host-visible answer display, or that the Cursor extension command reached `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.

Joined status remains partial. Counts stay unchanged:

- full joined pass: `0 -> 0`
- partial: `1 -> 1`
- not moved: `8 -> 8`
- blocked with next action: `19 -> 19`
- readiness claims opened: `0 -> 0`

## Next smallest subtask

Find or add a safe no-send Cursor extension-host log path for the same marker, then rerun only row 11 and prove that the extension command reaches the same application programming interface / программный интерфейс приложения request path already captured in [response.json](response.json) and [trace.json](trace.json).
