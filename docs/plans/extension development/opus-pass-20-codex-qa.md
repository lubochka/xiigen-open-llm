# Pass P20 — Codex: QA + dual-path (реальное состояние)

Дата: 2026-07-08. Ассессор: Opus 4.8. Слайс: РЕАЛЬНАЯ QA-инфраструктура и dual-path
(Path A автоматический boundary + Path B локальный desktop Codex) именно для Codex-расширения,
по живым артефактам `C:\Projects\codex-xiigen-smart-skills`. Оценка ведётся от кода/артефактов,
а не от copy-status пакета. Различаю COPIED_DOC_ONLY / BUILT_NOT_PROVEN / PROVEN.

## Что реально проверено (bounded reads)

1. `test/host-qa/HOST-08F-codexComponentQA.js` (241 стр, прочитан целиком) — единственный
   исполняемый Codex QA-раннер + второй файл `test/host-qa/HOST-REPAIR-CODEX-AVAILABLE-SKILLS-PARSER.js`.
2. QA-отчёты: `reports/HOST-08F-codex-plugin-app-qa.json`, `reports/HOST-IMPL-CODEX-01/`
   (summary.json, validation.json, component-qa/20260617-235258/**), `reports/CX-04R-host-runtime-path-report.json`.
3. Dual-path/host-runtime: `reports/RUNTIME-04-codex-live-runtime/.../runtime-04-codex-live-runtime-mirror.json`,
   вся серия `RUNTIME-04{,B,C,D,E,F,G}-codex/**` (app-server/CLI-пробы).
4. `RUNTIME-04G-codex/20260616-235353/runtime04g-app-server-adapter-core-boundary-summary.json`
   (сильнейшее «host runtime» доказательство Codex) + его send-message stdout/stderr.
5. `package.json` (scripts/deps), поиск `.github`/тест-раннера, поиск скриншотов/видео по всему репо,
   поиск любых `*dual*/*same-scenario*/*path-b*/*join*` артефактов.
6. QA visible-answer.txt по кейсам; negative-missing-target-locator case-summary.json.

Дедуп: НЕ повторяю P17 (entrypoint/marketplace-registration/runCli-dead-answer/manifest-triplication/
settings/cache-copy/access-denied/inferAssetRoot), P18 (валидация метода/само-ссылочный каталог/1-3
метода/defaultPrompt), P19 (lifecycle/dead-locator/rollback), P12 (Claude QA), P16 (Cursor QA),
P06 (пакетный dual-path join — там КОПИЯ, здесь настоящий репо).

## Ключевые факты слайса

- **QA есть, но это handler/component-level, само-декларирует не-host-runtime.** `HOST-08F` вызывает
  `invokeXiigenCore()` напрямую из `src/codex-entrypoint.js` (import, не через Codex-хост);
  `passed:true`, но `codex_plugin_runtime_qa:false`, `full_plugin_ui_evidence:false`,
  `codex_exec_model_backed_qa:false` (HOST-08F report :7-10, :182-185).
- **Оракул QA — только плумбинг/redaction.** `passed`-логика (`HOST-08F-codexComponentQA.js:141-143`):
  `exitCode`, `responseSuccess`, `exactDebugShape` (форма redacted-строки), `!sentinelLeaked`;
  для error-кейса — строковый матч `controlled_error_state`. Ноль ассертов о КОРРЕКТНОСТИ
  task/plan/skill. Видимый ответ ОДИНАКОВ для всех методов: `visible-answer.txt` =
  «Core task+plan+skills ready. Semantic quality is not claimed.» и для `prompt-to-task`, и для
  `skill-selection`.
- **QA гоняет мёртвые функции.** Тест импортирует и напрямую зовёт `createVisibleAnswerDisplay`
  (`:121-127`) и `createDebugTraceDisplay` (`:114-118`), которые поставляемый `runCli`
  (`src/codex-entrypoint.js:528-539`, установлено P17 gap-2) НИКОГДА не вызывает. То есть QA
  зелёный на функциях, которых нет на поставляемом runtime-пути.
- **`HOST-IMPL-CODEX-01` verdict:PASS = JSON-parse-гейт.** `validation.json:4` verdict PASS выведен из
  `json_parse` 39 файлов (`:6` target_count 39, failure_count 0) + `required_evidence` булевы
  (`:208-227`: external_ai_false, training_started_false, debug_redaction_passed, target_locator_bound,
  git diff --check pass). Ни одной проверки корректности/семантики runtime. «PASS» = «39 файлов
  парсятся + ожидаемые флаги присутствуют».
- **Path B (видимый desktop Codex) отсутствует конструктивно.** 0 скриншотов/видео во всём репо
  (`find -iname *.png/*.jpg/*.mp4` → пусто). 0 артефактов `*dual*/*same-scenario*/*path-b*/*join*`.
  Вся «runtime» — CLI/app-server-пробы или node из installed-cache. RUNTIME-04 mirror `proof_scope`:
  «narrow installed-cache/plugin-entrypoint runtime evidence only; full Codex chat UI unproven».
- **RUNTIME-04G — сильнейший «PROVEN», но самопровозглашённый.** `verdict:
  PASS_APP_SERVER_CHAT_INSTALLED_SKILL_ADAPTER_CORE_BOUNDARY_PROVEN_WITH_NEGATIVE_PATH` (:6), но
  positive-turn `positive_turn_wrapper_timed_out_after_positive_artifacts:true` (:15); «финальный
  ответ» — это МОДЕЛЬ Codex, нарративно подтверждающая вердикт («Verdict: Yes, within the evidence
  scope, Codex app-server/chat send-message-v2 executed the xiigen-smart-skills adapter/core
  boundary…»), т.е. модель сама себе судья; boundary всё тот же
  `core_boundary_partial_no_semantic_quality_claim`, `semantic_quality_claimed:false`.
  send-message-v2 attempt2/attempt3 при этом упали `codex.exe : error: unexpected argument
  'redacted' found`.
- **Утечка секрета в закоммиченном evidence.** RUNTIME-04G positive: `raw_private_prompt_persisted:true`
  (:42), `contains_secrets:true` (:43) — сырой приватный промпт с секретами персистится в дерево
  доказательств, при том что HOST-08F гордо ассертит redaction.
- **Разнобой Codex-бинарей в наборе доказательств.** По reports встречаются ПЯТЬ версий:
  `codex-cli 0.130.0-alpha.5` (RUNTIME-04G), `0.136.0-alpha.2` (CX-04R), `0.140.0-alpha.2`,
  `OpenAI.Codex_26.601.2237.0`, `OpenAI.Codex_26.609.4994.0` (RUNTIME-04). Сборный dual-path-«proof»
  смешивает разные host-бинари.
- **Единственный «негатив» тестирует поставляемый ДЕФОЛТ.** `negative-missing-target-locator`:
  `exit_code:0`, `include_target_locator:false` — но по P19 `TargetLocator` на поставляемом пути и так
  всегда null, т.е. «негатив» проверяет штатное состояние (успех, не-AI), а не деградацию на плохом
  входе. Единственный настоящий error-негатив — транспортный (missing core-api script).
- **QA невоспроизводима как гейт.** `package.json` scripts = только `install-smoke`/`source-wiring-check`;
  нет `test`-скрипта/раннера, `devDependencies:{}`, нет `.github` CI; 2 тест-файла запускаются только
  ad-hoc `node test/host-qa/...`.

## НОВЫЕ дыры (P20)

| # | Component | Real-status | Evidence file:line | Gap | Closure |
|---|---|---|---|---|---|
| 1 | HOST-IMPL-CODEX-01 verdict PASS | JSON-PARSE-GATE / not runtime verdict | `validation.json:4` PASS, `:6` json_parse 39/0, `:208-227` required_evidence = presence/negation флаги | Топ-вердикт Codex host-impl — well-formedness 39 JSON + наличие ожидаемых флагов (external_ai_false, debug_redaction_passed, target_locator_bound, git diff --check), не корректность/семантика runtime | Ввести runtime-correctness-ассерты (известный вход → ожидаемый task/plan/skills) как условие verdict; JSON-parse оставить preflight-проверкой, не вердиктом |
| 2 | HOST-08F QA оракул + мёртвые функции | PLUMBING-ONLY / tests-off-shipped-path | `HOST-08F-codexComponentQA.js:114-127` (зовёт `createDebugTraceDisplay`/`createVisibleAnswerDisplay`), `:141-143` passed=exit+redaction-shape; `src/codex-entrypoint.js:528-539` runCli их не зовёт; `visible-answer.txt` идентичен для prompt-to-task и skill-selection | QA зелёный на функциях видимого ответа/debug, которых нет на поставляемом runCli-пути; оракул проверяет только exit/redaction-форму, ни одного ассерта корректности task/plan/skill; вход синтетический sentinel; deps пусты, нет `npm test` | Тестировать ИМЕННО поставляемый runCli-вывод; ввести golden вход→ожидаемый структурный ответ; добавить тест-раннер+`npm test`+CI |
| 3 | RUNTIME-04G «PROVEN» app-server вердикт | OVERCLAIMED-VERDICT / self-narrated + timeout | `runtime04g-...-summary.json:6` verdict PROVEN, `:15` positive_turn timed_out, `:17` final_answer=модель нарратит вердикт, positive boundary `core_boundary_partial_no_semantic_quality_claim`/`semantic_quality_claimed:false`; attempt2/3 stderr `unexpected argument 'redacted'` | Сильнейшее Codex host-runtime доказательство помечено PROVEN, но positive-turn истёк по таймауту, «финальный ответ» — сама модель Codex, подтверждающая вердикт (self-judge, не независимый оракул), boundary плумбинг; send-message-попытки падали | Переклассифицировать в honest-narrow (app-server turn started + adapter boundary executed only); независимый оракул вместо модельного само-вердикта; перегнать без таймаута и командных ошибок |
| 4 | RUNTIME-04G persisted secrets | EVIDENCE-HYGIENE DEFECT / secret leak | `runtime04g-...-summary.json:42` raw_private_prompt_persisted:true, `:43` contains_secrets:true; при этом HOST-08F `redaction:true` | Закоммиченный positive-артефакт содержит сырой приватный промпт с секретами в дереве доказательств — противоречит redaction-гейту и privacy-требованиям dual-path evidence | Удалить/санитизировать закоммиченные секреты; ввести pre-commit секрет-скан для evidence; гарантировать redaction на ВСЕХ персистируемых артефактах, не только в HOST-08F |
| 5 | Path B видимый desktop Codex + same-scenario | MISSING PATH B / dual-path constructively absent | `find *.png/*.mp4`→0 по репо; `find *dual*/*same-scenario*/*path-b*/*join*`→0; RUNTIME-04 mirror proof_scope «full Codex chat UI unproven»; вся runtime = CLI/app-server/node-cache | У Codex нет ни одного видимого desktop-прогона (0 pixel) и ни одного same-scenario dual-path join в самом репо; Path A (boundary через invokeXiigenCore/app-server) и Path B (видимый Codex-чат) не сходятся на одном сценарии/ответе | Прогнать реальный видимый Codex-чат: промпт→ответ на экране+маркер+скрин/видео, привязать к prompt-hash; собрать same-scenario join поверх свежих Path A+B |
| 6 | Единственный «негатив» = поставляемый дефолт | WEAK-NEGATIVE / default-as-negative | `negative-missing-target-locator/case-summary.json` exit_code:0, include_target_locator:false; по P19 TargetLocator и так null на shipped-пути; настоящий error-негатив только транспортный (missing core-api script, HOST-08F `:98-108`) | «Негативный» кейс проверяет штатное состояние (locator отсутствует → всё равно exit 0 success), а не деградацию/ошибку на плохом входе; семантических/деградационных негативов нет | Добавить настоящие негативы: bad method (unknown), пустой/битый каталог скиллов, checkpoint-mismatch, повреждённый ввод — с ассертом честного degraded-ответа |
| 7 | Разнобой Codex-бинарей в evidence-наборе | UNPINNED HOST PROVENANCE | reports: `codex-cli 0.130.0-alpha.5` (RUNTIME-04G:10), `0.136.0-alpha.2` (CX-04R), `0.140.0-alpha.2`, `OpenAI.Codex_26.601.2237.0`, `26.609.4994.0` (RUNTIME-04 mirror) | Сборный Codex QA/dual-path «proof» смешивает ≥5 разных host-бинарей/версий; ни один прогон не закреплён на одной версии Codex — воспроизводимость и валидность вердиктов подорваны | Закрепить одну поддерживаемую версию Codex; перегнать QA/dual-path на ней; зафиксировать `codex_cli_version` как обязательное поле-гейт во всех evidence |

## Вывод P20

Codex QA существует как код (2 тест-файла + серия RUNTIME-04*), что богаче Claude (P12, 1 отчёт) и
сопоставимо с Cursor (P16), НО: (1) единственный исполняемый QA (HOST-08F) — component-level плумбинг
без оракула корректности, гоняющий функции вне поставляемого runCli-пути; (2) верхний verdict PASS
(HOST-IMPL-CODEX-01) — JSON-parse+флаги, не runtime; (3) сильнейший «app-server PROVEN» (RUNTIME-04G) —
таймаут + само-вердикт модели + плумбинг-boundary + утёкшие секреты; (4) Path B видимого desktop нет
вообще (0 pixel, 0 same-scenario join); (5) негативы вырождены (дефолт-как-негатив + транспорт);
(6) набор доказательств смешивает ≥5 версий Codex-бинаря; (7) QA невоспроизводима (нет `npm test`/CI).
Реальный статус Codex QA+dual-path: BUILT_NOT_PROVEN с несколькими overclaim/evidence-hygiene дефектами;
PROVEN дуал-путь для Codex = 0.
