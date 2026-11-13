# Pass P06 — Реальность evidence-пакетов (dual-path full-readiness)

Проход P06 из 50. Фокус: папка
`supporting-files/evidence-extension-full-readiness-dual-path/` — это НАСТОЯЩИЕ
runtime-доказательства (traces/screenshots/logs) или копии/синтетика/только-JSON?
Оценка от РЕАЛЬНЫХ артефактов, а не от статус-самоотчёта пакета.

## Что физически лежит в папке

- Всего 84 файла, 9 подпапок. **Скриншотов/видео/изображений в пакете — 0**
  (`find -iname *.png/*.jpg/*.mp4 …` → пусто). Всё pixel-доказательство Path B
  (десктоп) физически НЕ в пакете; оно только по абсолютным ссылкам в реальном
  репо `C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/…`
  (там скриншоты Claude/Cursor/Codex Path B реально есть — проверено `find`).
- Содержимое пакета: JSON/MD статус-борды, матрицы, «intake-downgrade»-джобы фаз
  05–16, три «same-scenario dual-path join»-пакета (Claude/Cursor/Codex), два
  token-savings-набора, один heldout-ablation-набор, phase-16 propagation.

## Ключевые находки по реальности доказательств

### 1. Token-savings = синтетика без отправки, дельта СТРУКТУРНО нулевая
`token-savings-cursor-row12-20260628-153837/measurement.json`:
`before_tokens=6209`, `after_tokens=6209`, `delta_tokens=0`,
`token_reduction_observed=false`, `synthetic_no_send_only` (из follow-up
result.json:11). Core-ответ пришёл из репо-локального no-send пути
`tools/xiigen-extension-core-api.ps1`. Мало того — `baseline-response.json` и
`adaptive-response.json` **байт-в-байт идентичны** (один MD5
`fc5135b6eb699e759a715184fe0cad51`, оба 119855 байт). То есть пара
baseline↔adaptive не может показать дельту токенов ПО ПОСТРОЕНИЮ: сравниваются
два одинаковых синтетических ответа. Это строго конкретнее, чем P01 phase-13
(«не доказала редукцию») — здесь артефакт-пара тождественна.

### 2. Реальный Cursor host-visible harness ПРОВАЛИЛСЯ, логи пустые
`token-host-visible-cursor-row12-sanitized-20260628-160311/result.json`:
`status=BLOCKED_CURSOR_HOST_VISIBLE_HARNESS_FAILED`; `runner_report_created:false`;
6 отсутствующих обязательных trace-ref (`row12-host-visible-runner-report.json`,
`xiigen-cursor-host-runtime-latest.json`, core-trace, host-trace,
entrypoint-response, redacted-trace). Логи-«доказательства»:
`cursor-cmd-stdout.log`=0 байт, `cursor-cmd-stderr.log`=0 байт,
`adaptive/baseline-command-output.txt`=1 байт (пусто),
`…error.txt`=1 байт. Единственная попытка запустить настоящую хост-команду
Cursor не выдала runner-report и пустой stdout — рабочая нагрузка через хост
фактически не отработала.

### 3. «Dual-path join» — бумажный join, не живой одно-сценарный прогон
- Claude `…join-…145508/result.json`: `ui_work_run:false`, `adapter_run:false`,
  `broad_tests_or_builds_run:false`; `proves_only` = «прочитаны два ДРУГИХ
  result.json и пересказаны их статусы». Это связывание уже-запушенных пакетов,
  а не новое исполнение.
- Cursor `join-packet.json`: Path A `ui_used:false`,
  `exact_cursor_adapter_cli_used:false`,
  `route_used="direct XiigenExtensionRequest API/core boundary fallback"` — т.е.
  Path A идёт НЕ через собранное расширение, а прямым вызовом core-контракта
  (тот самый in-process boundary из P03).
- Codex `join-packet.json`: Path A `route_used="direct exported Codex adapter
  function with string host facts"`, `ui_used:false`.
Вывод: «same-scenario dual-path coverage» — документальный join поверх ранее
снятых артефактов; ни один ЖИВОЙ прогон не гоняет host→core→host для одного и
того же промпта.

### 4. Path B «visible response» = скриншот + человеческое утверждение; машинная
   выемка текста ПРОВАЛИЛАСЬ
- Cursor `cursor-path-b-final-response-debug-…/result.json`:
  `uia_capture_error="Argument types do not match"` (UIA-текст НЕ извлечён),
  `visible_final_response_seen:true` опирается на 1 скриншот + globalStorage-JSON.
- Codex `codex-path-b-response-capture-…/result.json`:
  `new_prompt_submitted:false`, `thinking_visible:false`,
  `exact_prompt_previously_submitted:true` — это ПЕРЕ-фотография уже отправленного
  ранее промпта, не свежий submit→answer round-trip.
Значит Path B не подтверждает машинно, что видимый текст — это ответ модели именно
на промпт данного сценария.

### 5. phase-16-propagation-evidence — governance-литералы, не продукт, но лежит
   в папке readiness-evidence
`phase-16-propagation-evidence.json`: проверяет, что 10 anti-overclaim строк
(«trace != savings», «marker-only != full local desktop pass», …) присутствуют
как подстроки в AGENTS.md/CLAUDE.md/skills — 88 term-file пар, `PASS 88/88`,
`tests_builds_probes_training_evals_run:0`. Помечено «Фаза 17 из 17 complete
pending final review». Это COPIED_DOC/governance-работа: доказывает лишь наличие
литералов в доках, ноль продуктовой/runtime-готовности, но лежит внутри
«evidence-extension-full-readiness-dual-path» и читается как завершение фазы.

### 6. КОРРЕКЦИЯ P03/P04: три ЦЕЛЕВЫХ host-репозитория РАСШИРЕНИЙ реально
   существуют — их пропустили из-за неверных имён
`C:\Projects\{claude,cursor,codex}-xiigen-smart-skills` **существуют**:
- `cursor-xiigen-smart-skills`: настоящий VS Code/Cursor extension —
  `package.json` (`publisher:xiigen`, `engines`, `activationEvents`,
  `contributes`, `main:./dist/extension.js`), `src/extension.ts` **1345 строк**,
  собранный `dist/extension.js` **48148 байт**; команда
  `xiigenSmartSkills.runtimeProof.invoke` (`src/extension.ts:116/:148/:598`).
- `codex-xiigen-smart-skills`: `package.json` `main:"src/codex-entrypoint.js"`
  (Codex CLI-стиль entrypoint).
- `claude-xiigen-smart-skills`: `package.json` есть, но `dist/extension.js` и
  `src/extension.ts` не найдены — наименее зрелый.
P03 искал только внутри `llm_mvp_core`; P04 искал `xiigen_{claude,cursor,codex}`
(НЕВЕРНЫЕ имена) и объявил их отсутствующими. Реальные имена —
`{claude,cursor,codex}-xiigen-smart-skills`. Значит «Host entrypoints MISSING»
(P01) как абсолют — неверно: host-код ЕСТЬ (Cursor — собран), но принятое
evidence в пакете доказывает его лишь синтетикой Path A + заблокированным/
скриншот-Path B; сквозного машинно-проверенного prompt→answer через собранную
команду в принятых артефактах НЕТ.

## Итог P06
Пакет `evidence-…-dual-path` — это в основном JSON-статус + документальные join'ы
поверх внешних артефактов. Настоящие runtime-следы существуют, но: (а) token —
синтетика с нулевой/тождественной парой; (б) реальный host-harness заблокирован,
логи пусты; (в) join'ы не исполняют, а ссылаются; (г) Path B — скриншот без
машинной выемки; (д) governance-пропагейшн выдаётся за readiness-evidence; (е)
целевые host-репо существуют (важная коррекция), но не доказаны end-to-end.
Ни одно из этих доказательств не поднимает статус выше BUILT_NOT_PROVEN, а token —
STUB EVIDENCE.
