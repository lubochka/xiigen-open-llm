# Phase 10 Cursor full extension live proof

Фаза 10/14: Cursor full extension live proof.
Что решает фаза: доказывает, что Cursor extension имеет живой callable command/handler path, который тонко передает запрос в llm_mvp_core и возвращает видимый ответ или ошибку.

Хостовый запуск: Cursor Extension Host command automation.
Путь extension: c:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills.
Evidence leaf: C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-10-cursor-full-extension-live-proof\run-20260623T1416172131380Z\cursor.

Покрытие подшагов:
- 10.02: package/entrypoint wiring проверен через package.json, dist/extension.js и команду xiigenSmartSkills.runtimeProof.invoke.
- 10.03: live Extension Host command path выполнен через vscode.commands.executeCommand внутри Cursor/VS Code Extension Host; полный ручной UI screenshot не заявлен.
- 10.04: видимый success/error path проверен: success возвращает CursorDisplayText, controlled error начинается с XIIGen Cursor error.
- 10.05: debug off не показывает xiigen-блок; debug on показывает точную форму xiigen <method> / input / output с redacted payload.
- 10.06: same-run correlation id совпал между Cursor request и core trace для success cases.

Что доказано:
- Cursor extension entrypoint callable.
- Host layer остается thin adapter: semantic ranking/training/model mutation в extension не выполнялись.
- Запрос дошел до llm_mvp_core через tools/xiigen-extension-core-api.ps1 в этом же прогоне.
- Raw prompt sentinel присутствует только в request payload по контракту и отсутствует в response/trace/display artifacts.

Что НЕ заявлено:
- не заявлен full manual Cursor UI screenshot;
- не заявлены all-host readiness, Codex/Claude readiness, token savings, semantic quality, local model readiness или final project completion;
- external AI/network не использовались.

Итог runner: PASS_PENDING_REVIEW.

