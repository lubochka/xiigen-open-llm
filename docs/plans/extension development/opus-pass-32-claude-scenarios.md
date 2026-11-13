# Pass P32 — Claude scenarios dual-path reality (по-сценарно Path A + Path B vs копии)

Дата: 2026-07-08. Ассессор: Opus 4.8 (REAL-STATE). Фокус: РЕАЛЬНОЕ состояние
каждого Claude-сценария по Path A (автоматический boundary) и Path B (локальный
desktop) — против ПАКЕТНЫХ копий. Оценка ведётся от живых артефактов
`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\`
и host-репо `C:\Projects\claude-xiigen-smart-skills`, а не от copy-status пакета.

Дедуп-граница (что я НЕ повторяю):
- P06 — три join-пакета «бумажные» (`ui_work_run:false/adapter_run:false`),
  package-evidence только JSON, pixel вне пакета, Path B машинная выемка текста
  провалилась (Cursor/Codex).
- P09 — Claude-манифест = только SessionStart-hook, фиктивное имя
  `xiigen.smartSkills.select`, debug metadata-only, wrong output sink,
  test-only локатор, расходящиеся копии.
- P10 — Claude dead-gate semantic_quality, стадия-3 skills = no-op на пустом
  каталоге, 1/3 методов, flat-chain мимо 15.
- P12 — Claude QA = один HOST-08E (node.exe boundary), RUNTIME-02 error-run
  штампован PASS, Path B ограничен фиксированной hook-строкой, нет UX-слоя.
- P31 — канонический инвентарь 28 сценариев, board-счётчики 0/28, Claude 0/9
  blocked, отсутствие A↔B comparison-поля, mojibake рядов 6/11/15.

P32 — впервые ПО-СЦЕНАРНО вскрывает, что реальные Claude Path A / Path B
артефакты представляют собой, и как они (не) соответствуют 9 каноническим
Claude-сценариям.

---

## 1. Что реально есть на диске для Claude dual-path

### 1.1. Девять канонических Claude-сценариев (board rows 1-9) — ВСЕ blocked

`product-readiness-scenario-board.json` (канон в реальном репо и копия в пакете
идентичны), строки 172-306, scenario_id + board_category:

| row | scenario_id | board_category | local_real_program_status |
|---|---|---|---|
| 1 | frdp-p01-claude-product-readiness-product-workflow | blocked_with_next_action | «safe local Claude no-send route is not proven» |
| 2 | frdp-p01-claude-full-extension-entrypoint | blocked_with_next_action | «safe Claude marker route is not proven» |
| 3 | frdp-p01-claude-token-savings-baseline | blocked_with_next_action | «Claude host access to candidate behavior is not proven» |
| 4 | frdp-p01-claude-semantic-quality-heldout | blocked_with_next_action | «Claude host scenario execution is not available» |
| 5 | frdp-p01-claude-learnability-training-continue | blocked_with_next_action | «Claude before/after host path is not proven» |
| 6 | frdp-p01-claude-dpo-quality-provenance | blocked_with_next_action | «... DPO-derived candidate is not host-visible» |
| 7 | frdp-p01-claude-asset-lifecycle-roundtrip | blocked_with_next_action | «Claude restored/rolled-back behavior path is not proven» |
| 8 | frdp-p01-claude-checkpoint-promotion-reload | blocked_with_next_action | «Claude reload from candidate checkpoint is not proven» |
| 9 | frdp-p01-claude-locator-update-reload | blocked_with_next_action | «Claude host reload from locator is not proven» |

Итог: 0/9 Claude-сценариев имеет joined dual-path pass; Path B по всем 9 =
«not proven / not available».

### 1.2. Единственный существующий Claude dual-path JOIN — ОФФ-борд, generic prompt

`scenario-execution/claude-same-scenario-dual-path-join-20260628-145508/`:
- `result.json:3` `status:CLAUDE_SAME_SCENARIO_DUAL_PATH_JOINED`;
- `:5` `same_prompt`= «... Request: create a safe plan for documenting an API
  dashboard.» (generic prompt-to-task проба);
- `:6` `prompt_hash_sha256:305fb4c0...`;
- `:40-41` `ui_work_run:false`, `adapter_run:false`;
- `evidence-map.json:8-9` `prompt_hash.source:"computed_from_same_prompt_text"`.

Этот прогон-«успех» — про generic prompt, а НЕ про один из 9 канонических
scenario_id. На board он не отражён (все 9 blocked). То есть единственный
«CLAUDE_SAME_SCENARIO_DUAL_PATH_JOINED» артефакт и 9 требуемых Claude-сценариев
ДИЗЪЮНКТНЫ: join двигает off-board generic-пробу, 9 типовых capability остаются
blocked.

### 1.3. Path A «PASS» — bespoke node.exe CLI с рукотворным каталогом скиллов

`claude-same-scenario-path-a-20260628-144612/command-log.md:14-23`:
```
node.exe src\claude-entrypoint.js
  -ConcreteTask "...document an API dashboard."
  -Prompt "...document an API dashboard."
  -Method prompt-to-task
  -CoreRoot C:\Projects\llm_mvp_core
  -RequestedSkills xiigen-smart-skills,api-documentation,planning
  -AvailableSkills xiigen-smart-skills,api-documentation,planning,documentation
```
`result.json:23-28` `response_status:core_boundary_partial_no_semantic_quality_claim`,
`selected_skills:[xiigen-smart-skills, api-documentation, planning]`,
`semantic_quality_claimed:false`.

Ключ: непустой `selected_skills` получен ТОЛЬКО потому, что драйвер вручную
подал `-AvailableSkills xiigen-smart-skills,api-documentation,planning,documentation`.
Поставляемая Claude-поверхность — SessionStart-hook (P09) — `-AvailableSkills`
НЕ передаёт (P10 gap-2: пустой каталог → стадия skills no-op
`NO_AVAILABLE_SKILLS_PROVIDED`). Значит «Claude same-scenario Path A» не
воспроизводим через реальный Claude-хост: это прямой node-CLI вызов
core-boundary с сфабрикованным каталогом, маскирующим shipped-no-op.
(Отличается от P12 gap-1 — там HOST-08E QA-отчёт; здесь именно scenario-execution
Path A + fabricated catalogue.)

### 1.4. Path B — re-photograph уже открытого окна, БЕЗ промпта и хеша

`claude-path-b-prompt-submission-20260628-142748/`:
- `result.json:2` `status:CLAUDE_PROMPT_SUBMITTED_FINAL_VISIBLE_RESPONSE`,
  `:32` `no_new_prompt_submitted:true`;
- в пакете НЕТ ни поля prompt, ни prompt_hash;
- `README.md:14` статус-основа = «Уже отправленный промпт ... дошёл до финального
  видимого ответа. Новый снимок current-claude-window показывает видимый ответ»;
- `action-log.md:14-21` — «packetization worker»: «Queried local desktop
  processes and found a visible claude window. Captured current-claude-window...
  Determined the allowed narrow status». Ни OCR, ни выемки текста, ни submit.

То есть Path B «финальный видимый ответ» объявлен отдельным упаковщиком по
ПЕРЕ-фотографии уже открытого окна, без отправки промпта данного сценария и без
машинной привязки видимого ответа к промпту.

### 1.5. Path B ведёт НАТИВНЫЙ Claude Desktop композер, не XIIGen-расширение

`claude-path-b-composer-input-proof-20260628-120014/result.json`:
- `:2` `BLOCKED_UIA_HELPER_ARGUMENT_TYPES`, `:3` scenario_id
  `frdp-p01-claude-full-extension-entrypoint` (типовой row 2);
- `:17-27` `route_selected:false, composer_found:false, prompt_typed_or_pasted:false,
  prompt_sent:false, enter_sent:false, response_or_status_seen:false`;
- `:36-41` blocker: UIA-хелпер `Get-RedactedUiaSummary` упал
  «Argument types do not match» / «System.String does not contain ToArray» в
  `run/claude-composer-input-proof.ps1:220/:377`;
- `:10` `prompt_sha256:52846bd2...` (ДРУГОЙ промпт, чем join 305fb4c0), 460 симв.

Механизм Path B — найти НАТИВНЫЙ композер Claude Desktop, кликнуть, ввести
промпт, нажать Enter (`run/claude-composer-input-proof.ps1`). Это драйв
собственного чат-поля Claude Desktop, а НЕ вызов какой-либо XIIGen-поверхности.
XIIGen-«расширение» Claude — SessionStart-hook (P09) без пользовательской
команды — в петле Path B НЕ участвует. Значит видимый ответ на скриншотах — это
собственная модель Claude, и НЕТ доказательства, что XIIGen произвёл/изменил
этот ответ. Dual-path join связывает XIIGen-core Path A с не-XIIGen Path B — обе
ноги не разделяют расширение. (P12 gap-4 говорил «Path B = фиксированная
hook-строка»; здесь реальный Path B вообще не использует hook — это сырой
Claude-app, аргументированно ещё слабее.)

### 1.6. Path B по факту — цепочка ремонта UIA-маршрута, не исполненный сценарий

11 `claude-path-b-*` папок (по .png):
- composer-input-proof ×3 (+ retry, + after-helper) — 0 png, все BLOCKED_UIA;
- composer-locator-map, route-locator-repair, no-secret-execution,
  route-preflight ×2 — 0 png;
- route-navigation-recovery — 6 png; prompt-submission — 8 png
  (`no_new_prompt_submitted:true`).

Единственные «успешные» скриншоты — re-photograph (prompt-submission) и
navigation-recovery; чистого typed+sent прогона в свежем запуске нет. Path B —
это трейл route-discovery/repair из-за багованного UIA-хелпера, а не executed
scenario. (P06 gap-5 — Cursor/Codex machine-text; P16 — Cursor headless. Здесь
Claude-специфично: сам композер-драйвер сломан, ни один typed-промпт не
отправлен в свежем прогоне.)

### 1.7. Прогон-«успех» и типовые сценарии — про РАЗНЫЕ промпты

- Join / Path A «PASS»: prompt_hash `305fb4c0` («document an API dashboard»).
- Типовой entrypoint Path B (composer-input-proof): prompt_sha256 `52846bd2`,
  460 симв. (из `scenario-prep/claude-full-extension-runtime-.../scenario.json`
  `exact_prompt`).

Ни один единый scenario-промпт не имеет одновременно валидного Path A и реального
Path B. «Same-scenario» нарушено даже между тем, что «прошло» (generic), и тем,
что пытались (типовой entrypoint).

## 2. Что скопировано в пакет

`supporting-files/evidence-extension-full-readiness-dual-path/`:
- скопирована ТОЛЬКО join-папка
  `scenario-execution__claude-same-scenario-dual-path-join-20260628-145508`
  (2.4КБ result.json + evidence-map);
- НЕ скопированы ни Path A `response.json` (36КБ), ни 8 Path B PNG.

То есть единственный Claude dual-path артефакт в пакете — бумажный
status-matcher (`reference_status_verification`: читает 2 result.json, сверяет
`observed==expected`), ссылающийся на абсолютные пути реального репо, которых в
пакете нет.

## 3. Новые дыры этого прохода (5)

См. таблицу в мастер-реестре `opus-PROJECT-CLOSURE-PLAN.md`, раздел
«Pass P32». Коротко:

1. Единственный Claude dual-path JOIN — off-board generic prompt-to-task проба,
   не соответствует ни одному из 9 канонических Claude scenario_id; 9 типовых
   сценариев = 0 joined pass.
2. Claude same-scenario Path A «PASS» достигнут bespoke node.exe CLI с
   рукотворным `-AvailableSkills`; невоспроизводим через shipped SessionStart-hook.
3. A↔B same-prompt identity: join-хеш вычислен из Path A; Path B без промпта/хеша
   (`no_new_prompt_submitted`); привязка B к сценарию недоказуема.
4. Claude Path B ведёт нативный Claude Desktop композер, не XIIGen-поверхность;
   видимый ответ — модель Claude, XIIGen в петле Path B отсутствует.
5. Claude Path B = сломанная UIA-цепочка ремонта маршрута; typed+sent прогон не
   исполнен; «успех» = re-photograph уже открытого окна.

## 4. Честная граница

Всё выше — про EVIDENCE-артефакты Claude dual-path. Реальный код Claude-хоста
(P09-P12) и ядра (P10) оценивался в предыдущих проходах. P32 не заявляет
готовности/незавершённости кода — только: по-сценарно ни один из 9 канонических
Claude-сценариев не имеет настоящего Path A(через host)+Path B(видимый тот же
промпт) joined pass; единственный «joined» артефакт — off-board generic проба с
не-XIIGen Path B.
