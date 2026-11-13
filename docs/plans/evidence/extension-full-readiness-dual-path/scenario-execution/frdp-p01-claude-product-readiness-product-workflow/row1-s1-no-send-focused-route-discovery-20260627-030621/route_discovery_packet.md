Человеческий смысл: это был безопасный no-send discovery, чтобы понять, можно ли сейчас увидеть и сфокусировать реальное окно Claude и найти путь к input surface. Путь не подтвержден, потому что интерактивный foreground оказался Windows Lock Screen, а сохраненные crops не показали UI Claude.

# Row1 S1 Claude no-send focused route discovery packet

## Итог

Вердикт: `BLOCKED_ROUTE_UNKNOWN`.

Route к input surface / extension entrypoint не подтвержден. Claude window существует как процесс и видимое окно, но foreground/focus не был подтвержден: после focus-only titlebar/chrome click foreground стал `LockApp` / `Windows Default Lock Screen`, а не Claude. Crops через `CopyFromScreen` и `PrintWindow` не показали usable Claude UI.

Это не product execution и не prompt send.

## Что было сделано

- Найдено одно видимое окно Claude: title `Claude`, process `claude`, pid `15384`, handle `0x103B8`.
- Попытка `SetForegroundWindow` по handle `0x103B8`: foreground не подтвердился.
- Попытка `WScript.Shell.AppActivate(15384)`: вернула `true`, но `GetForegroundWindow` все равно не подтвердил Claude.
- UI Automation увидел root window `Claude`, enabled/on-screen, но не раскрыл input/compose controls.
- Сохранены только narrow crops, без full-window screenshot artifact.
- Выполнен один focus-only click по chrome/titlebar coordinate `730,178`; после него foreground определился как `LockApp` / `Windows Default Lock Screen`.
- Prompt text typed: `0`.
- Prompts sent: `0`.
- Product readiness claims: `0`.

## Window / process / monitor facts

- Claude visible window:
  - handle: `0x103B8`
  - pid: `15384`
  - process: `claude`
  - title: `Claude`
  - bounds after final enumeration: `left=689`, `top=160`, `right=1905`, `bottom=837`, `width=1216`, `height=677`
- Claude executable path observed for processes: `C:\Program Files\WindowsApps\Claude_1.15962.0.0_x64__pzs8sxrjxfjjc\app\Claude.exe`
- Monitors:
  - `\\.\DISPLAY1`: non-primary, bounds `x=-1920 y=0 w=1280 h=720`
  - `\\.\DISPLAY2`: primary, bounds `x=0 y=0 w=1920 h=1080`
- Active foreground after focus-only click:
  - process: `LockApp`
  - title: `Windows Default Lock Screen`
  - Claude focus matched: `false`

## Auth/session observation

Session authentication state for Claude is unknown.

Evidence supports only that a Claude app process/window exists. I did not see a Claude account indicator, login screen, conversation UI, input placeholder, or send button. I did not capture or expose account data, tokens, secrets, or private chat contents.

## Screenshot evidence

Stored screenshots are privacy-safe but not route-proving:

- `claude_top_chrome_crop.png`: `CopyFromScreen` top crop; visually black/blank.
- `claude_bottom_input_crop.png`: `CopyFromScreen` bottom/input-area crop; visually black/blank.
- `claude_printwindow_top_crop.png`: `PrintWindow` top crop; visible window chrome only, no Claude content route.
- `claude_printwindow_bottom_crop.png`: `PrintWindow` bottom crop; blank white content, no input route.

No full-window screenshot was saved.

## Exact future UI scenario if route remains blocked

Route is not known enough for a future same-scenario send test. The next safe discovery step is:

1. Human or authorized desktop recovery unlocks the Windows session. No secrets are captured in artifacts.
2. Re-run no-send discovery against the same Claude app/window or a newly enumerated Claude main window.
3. Bring Claude to foreground and confirm foreground process/title is Claude, not LockApp.
4. Capture only cropped evidence:
   - top/account/status region, redacted if it exposes account/private data;
   - bottom compose/input region before any typing;
   - optional crop of extension entrypoint if visible.
5. Read only visible route/auth labels needed to determine:
   - whether Claude appears authenticated;
   - whether an input/compose surface exists;
   - whether any extension entrypoint is visible.
6. Stop before typing. Actual send remains forbidden until a later explicitly authorized product scenario.

## Missing information

- Whether Claude session is authenticated.
- Whether prompt input surface is visible and clickable.
- Whether extension entrypoint exists in the current Claude UI.
- Exact click target for a future no-send preflight screenshot.

## Evidence records

- Win32 enumeration found one visible Claude window.
- Win32 focus attempt did not confirm foreground Claude.
- AppActivate returned true but foreground remained unconfirmed.
- UI Automation found root Claude window only; no input controls.
- `CopyFromScreen` narrow crops were black/blank.
- `PrintWindow` narrow crops were blank/chrome only.
- Focus-only titlebar/chrome click resulted in foreground `LockApp`, not Claude.

## Safety counters

- Prompts typed: `0`.
- Prompts sent: `0`.
- Model-backed responses invoked: `0`.
- Product code edits: `0`.
- Governance/plan/board edits outside this packet: `0`.
- Product readiness claims: `0`.
