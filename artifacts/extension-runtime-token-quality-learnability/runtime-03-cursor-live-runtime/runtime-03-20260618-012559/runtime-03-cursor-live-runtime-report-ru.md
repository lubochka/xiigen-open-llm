# RUNTIME-03 Cursor live runtime QA

Статус: PASS для узкого Cursor Extension Host command/runtime path, parent review pending. Это не full Cursor chat UI proof.

## Что доказано
- Запущен фактический Cursor executable: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`.
- Extension Host dev-load path: `--extensionDevelopmentPath C:/Projects/cursor-xiigen-smart-skills`, test path `C:/Projects/cursor-xiigen-smart-skills/reports/RUNTIME-03-cursor/runtime03-extensionHostQA.js`.
- Extension `xiigen.cursor-xiigen-smart-skills` загружен и активирован из `C:\Projects\cursor-xiigen-smart-skills`.
- Command path `xiigen.smartSkills.select` вызван внутри Cursor Extension Host и дошел до `llm_mvp_core/tools/xiigen-extension-core-api.ps1`.
- В positive request `HostType=Cursor`, `Method=prompt-to-task`, `HostFacts.external_ai=false`, `HostFacts.host_training=false`.
- TargetLocator присутствовал в request, `LocatorMode=target`; core response вернул `target_locator_bound=true` и `target_locator_mode=target`.
- Debug display shape был видимым в command output: `xiigen prompt-to-task :`, `input:`, `output:`.
- Controlled negative path прошел через missing core API script и вернул `CORE_ENDPOINT_MISSING` с raw/redacted request/response/trace artifacts.

## Что НЕ доказано
- Full Cursor chat UI / user-visible chat participant invocation не доказан: safe noninteractive chat invocation API не найден, chat participant не вызывался.
- Panel path в финальном RUNTIME-03 run не вызывался; panel evidence остается только из принятого HOST-IMPL-CURSOR-01.
- Token savings, semantic quality, quality improvement, learnability, DPO, training, checkpoint mutation, production locator promotion и final readiness не заявлены.
- Cursor stderr содержит фоновые unauthenticated aiserver/model metadata ошибки Cursor runtime. XIIGen task prompt не отправлялся в Cursor chat/model; для task solving зафиксировано `external_ai_task_solving=false`.

## Основные refs
- Evidence summary: `C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/runtime-03-cursor-live-runtime/runtime-03-20260618-012559/runtime-03-cursor-live-runtime-evidence.json`
- Validation: `C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/runtime-03-cursor-live-runtime/runtime-03-20260618-012559/runtime-03-cursor-live-runtime-validation.json`
- Cursor extension host report copy: `C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/runtime-03-cursor-live-runtime/runtime-03-20260618-012559/cursor-extension-host-report.json`
- Cursor launch summary copy: `C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/runtime-03-cursor-live-runtime/runtime-03-20260618-012559/cursor-launch-summary.json`
- Cursor repo mirror: `C:/Projects/cursor-xiigen-smart-skills/reports/RUNTIME-03-cursor-live-runtime/runtime-03-20260618-012559/runtime-03-cursor-live-runtime-mirror-ru.md`
