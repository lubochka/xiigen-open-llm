# Claude Path B route/entrypoint visibility — 2026-06-28 14:15:58

## Что проверялось

Проверялся только локальный маршрут/entrypoint XIIGen/Smart Skills внутри реального Claude Desktop на этом компьютере. Prompt не отправлялся. Готовность Claude extension, token savings, качество, learnability, training, checkpoint mutation и promotion не проверялись и не заявляются.

## Целевое окно

- Приложение: Claude Desktop
- Window title: `Claude`
- Process: `claude`, PID `15384`
- Path: `C:\Program Files\WindowsApps\Claude_1.15962.0.0_x64__pzs8sxrjxfjjc\app\Claude.exe`
- Target HWND: `0x103B8`
- Bounds: `Left=689 Top=160 Right=1905 Bottom=837 Width=1216 Height=677`

## Что было видно и сделано

1. Был найден реальный Claude Desktop window candidate.
2. Первый screen capture показал перекрытие Cursor/Claude, поэтому отдельно зафиксирован `PrintWindow` target capture.
3. В Claude был открыт modal `Claude is still working`. Я нажал только `Cancel`, чтобы закрыть modal и не прерывать текущую задачу.
4. В sidebar Claude Desktop в разделе `Recents` был виден route-кандидат `XIIGen Smart Skills UI proof run`.
5. Я кликнул этот recent route. Открылся target view с содержимым XIIGen/Smart Skills UI proof run и пустым input `Type / for commands`.

## Узкий результат

Status: `CLAUDE_ROUTE_RECOVERED`.

Доказано только узкое: локальный Claude Desktop содержит видимый route/entrypoint `Recents > XIIGen Smart Skills UI proof run`, и этот route открывается в Claude Code view.

## Что это не доказывает

- Не доказывает full Claude extension readiness.
- Не доказывает registered packaged XIIGen Claude extension host entrypoint/panel/command.
- Не доказывает token savings.
- Не доказывает semantic quality improvement.
- Не доказывает learnability, training, DPO, checkpoint mutation или target promotion.
- Не доказывает, что prompt execution разрешен или был выполнен.

## Evidence screenshots

- `screenshot-001-claude-foreground.png`: первая попытка screen capture target bounds; виден overlap.
- `screenshot-003-claude-printwindow.png`: точный target HWND capture с modal `Claude is still working`.
- `screenshot-004-claude-after-minimize-cursor.png`: Claude visible после handling overlap.
- `screenshot-005-claude-after-cancel-modal.png`: modal закрыт, в `Recents` виден `XIIGen Smart Skills UI proof run`.
- `screenshot-006-claude-xiigen-route-opened.png`: route открыт; target view отображает XIIGen/Smart Skills UI proof run.

## Следующее bounded действие

Если parent reviewer принимает этот packet, следующий bounded action: отдельный Claude Path B same-scenario executor work order, где prompt submission явно разрешен, открывает `Claude Desktop > Code > Recents > XIIGen Smart Skills UI proof run`, выполняет только утвержденный same-scenario prompt, сохраняет screenshots/log/result, и не заявляет readiness шире полученных evidence.
