# Row1 S1 same-scenario contract

claim_scope: planning-only Row1 S1 same-scenario contract for `frdp-p01-claude-product-readiness-product-workflow`.
evidence_scope: this artifact defines the future scenario contract only; it does not run product, runtime, UI, tests, eval, heldout, training, or Claude host actions.
count_claimed: total scenarios 28; scoped accepted 14/28; product-ready full pass 0/28; remaining 14.
independent_evidence_count: 2 split target files read plus 1 Row1 evidence folder listing in this current work.
subagent_id_or_NONE: McClintock the 2nd / 019f0627-e4ff-7a51-ade4-102f09e5e6f3
downgrade_needed: no. The current sub-agent id is now recorded; this packet is planning-complete and does not claim product readiness.

## Человеческий статус

Готово в scope: 14/28. Product-ready full pass: 0/28. Осталось: 14. Row1 не является product-ready из этого пакета.

Этот файл является только плановым контрактом S1. Он не доказывает локальную готовность Claude, не доказывает extension/core readiness, не доказывает экономию токенов, качество ответа, обучение, checkpoint promotion, locator update или полный product workflow.

## Назначение сценария

S1 задает один и тот же безопасный пользовательский запрос, который будущие исполнители должны использовать в двух путях:

- Automatic path: проверка до host UI на границе extension/core, если существует отдельно разрешенный non-mutating entrypoint.
- Local UI/agent path: проверка того же запроса через локальный Claude app/window/session маршрут, где это возможно и только после отдельной execution-prep авторизации.

Смысл S1: от пользовательского запроса до видимого ответа в Claude workflow, но в этом пакете фиксируется только контракт. Отправки запроса, запуска UI и runtime здесь нет.

## Same-scenario prompt

Marker/correlation id: `ROW1-S1-CLAUDE-WORKFLOW-SAFE-20260627`

Точный текст запроса:

```text
[ROW1-S1-CLAUDE-WORKFLOW-SAFE-20260627]
Без внешних данных, секретов, персональных данных и файловой системы.
Ответь на русском языке ровно тремя строками:
1. marker: ROW1-S1-CLAUDE-WORKFLOW-SAFE-20260627
2. summary: один короткий ответ, что запрос принят и обработан безопасно
3. boundary: это демонстрационный ответ для проверки маршрута, не продуктовая готовность
```

Запрос безопасен и редактирован: он не содержит секретов, учетных данных, пользовательских данных, ссылок на приватные файлы, команд на запуск продукта или инструкций на изменение состояния.

## Automatic path definition

Automatic path может проверять только то, что возможно до host UI: прохождение того же prompt package до extension/core boundary через отдельно разрешенный non-mutating entrypoint.

Ожидаемые request fields:

- `scenario_id`: `frdp-p01-claude-product-readiness-product-workflow`
- `scenario_step`: `Row1-S1`
- `marker`: `ROW1-S1-CLAUDE-WORKFLOW-SAFE-20260627`
- `prompt_text`: точный текст из раздела Same-scenario prompt
- `mode`: `planning_contract_or_boundary_probe`
- `mutating_actions_allowed`: `false`

Ожидаемые response fields:

- `accepted`: boolean только для boundary acceptance, не для product readiness
- `marker_echo`: должен совпадать с marker
- `normalized_visible_answer_contract`: три ожидаемые строки, если route поддерживает ответный contract
- `claim_scope`: `boundary_only`
- `product_readiness_claim`: `false`
- `claude_host_readiness_claim`: `false`
- `errors_or_blockers`: список, если route отсутствует или entrypoint не разрешен

Ожидаемые future evidence files:

- `row1-s1-automatic-request.json`
- `row1-s1-automatic-response.json`
- `row1-s1-automatic-boundary-log.txt`
- `row1-s1-automatic-claim-locks.json`

Automatic path PASS в будущем не равен локальному Claude host PASS. Boundary/API evidence != host readiness.

## Local UI/agent path definition

Перед любым локальным UI/agent действием нужен preflight:

- Назвать точное приложение или окно Claude, которое будет целью.
- Подтвердить session state без захвата секретов: signed-in, logged-out или unknown.
- Подтвердить, что экран, окно и input surface видимы и не перекрыты.
- Подтвердить, что разрешены только действия текущего work order.
- Подтвердить `no secrets captured`.

Если route известен, будущий executor-prep должен указать точные клики/typing/expected visible state до отправки. Если route неизвестен, это discovery path, а не product execution:

- Разрешено: найти или сфокусировать указанное Claude window; осмотреть видимые controls; сделать redacted screenshot; записать window title, route candidate, coordinates/control name; остановиться до send.
- Запрещено: вводить credentials; захватывать секреты; нажимать Send/Enter; отправлять полный prompt; запускать product/runtime/tests/eval/heldout/training; менять code, board, checkpoint, locator; commit/push.

BLOCKED criteria для local route:

- Claude app/window не найден.
- Claude opens anonymously или logged out и требует credentials.
- Input route не найден или не может быть безопасно сфокусирован.
- Любое продолжение потребует secret capture, credential entry или submit/send.

Если route остается unknown, следующий work order должен быть S1 review или execution-prep/discovery-only. Он не должен быть product execution.

## Ожидаемый видимый результат

Человек должен увидеть ответ на русском языке ровно в трех строках:

```text
marker: ROW1-S1-CLAUDE-WORKFLOW-SAFE-20260627
summary: запрос принят и обработан безопасно
boundary: это демонстрационный ответ для проверки маршрута, не продуктовая готовность
```

Допустимы мелкие различия в пунктуации только если marker полностью совпадает и смысл `not product readiness` сохранен. Если marker отсутствует, ответ не принимается.

## Что это доказывает и не доказывает

Что доказывает этот planning contract:

- Есть один конкретный same-scenario prompt для future automatic path и local UI/agent path.
- Определены ожидаемые поля request/response.
- Определены future evidence filenames.
- Определены claim locks и границы доказательства.

Что не доказывает:

- Product-ready full pass.
- Claude host readiness.
- Extension/core runtime behavior.
- Token savings.
- Semantic quality improvement.
- Training, learnability, eval или heldout result.
- Checkpoint promotion или locator update.
- Что запрос был реально отправлен или что ответ был виден в Claude.

## Разрешенные и запрещенные действия

Разрешено в рамках этого S1 contract packet:

- Читать только разрешенные planning/evidence файлы.
- Создать новую папку `row1-s1-contract-20260627-015856/`.
- Создать `row1-s1-contract.md` и `row1-s1-contract.json`.
- Проверить JSON parsing, `git diff --check` по новой папке и отсутствие forbidden mojibake marker pattern в новых target files.

Запрещено:

- Product/runtime/UI execution.
- Tests, eval, heldout, training.
- Prompt submit/send в Claude.
- Code/tools/AGENTS/CLAUDE/skills/board edits.
- Checkpoint promotion, locator update, model mutation.
- Commit/push.
- Product-ready claim.

## Criteria

PASS criteria:

- Контракт содержит точный safe prompt, marker, expected visible result, automatic path fields, local UI/agent preflight, proof limits, allowed/forbidden actions и next work order.
- JSON parses.
- `git diff --check -- <new folder>` passes.
- В новых target files нет forbidden mojibake marker pattern.
- `subagent_id_or_NONE` содержит настоящий текущий sub-agent id из tool result, не generic и не NONE.

NEEDS_FIX criteria:

- Настоящий текущий sub-agent id недоступен или не внесен.
- Prompt, marker, evidence files или route criteria расплывчаты.
- Файл позволяет product-ready claim из planning-only evidence.
- JSON не parse-ится или есть mojibake markers.

BLOCKED criteria:

- Текущие planning artifacts не позволяют безопасно задать один same-scenario prompt.
- Дальнейшая подготовка требует product/runtime/UI execution или secret capture.

Current result: PASS_CONTRACT. The prior placeholder/id blocker is removed. This is a planning-only contract pass and not a product-ready pass.

## Exact next work order

Role: bounded Row1 S1 contract review sub-agent.

Max active time: 10 minutes.

Task: review only `row1-s1-contract.md` and `row1-s1-contract.json` in `row1-s1-contract-20260627-015856/`. Do not run product/runtime/UI/tests/eval/heldout/training. Verify Russian readability, same-scenario prompt exactness, automatic path fields, local UI/agent discovery criteria, proof limits, claim locks, JSON parsing, `git diff --check -- <new folder>`, and absence of forbidden mojibake marker pattern.

The real current sub-agent id is recorded. Review may evaluate the contract without the prior placeholder/id blocker.
