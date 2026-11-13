# Codex real UI scenario prep

claim_scope: подготовлен только сценарий проверки живого Codex UI; запуск Codex UI не выполнялся
evidence_scope: README.md и scenario.json в этой папке
count_claimed: 1 bounded scenario-prep slice
independent_evidence_count: 0 до выполнения будущего runtime-сценария
subagent_id_or_NONE: bounded Codex scenario-prep sub-agent
downgrade_needed: да; product readiness не заявляется

## Человеческий статус

Фаза 4 из 10: подготовить живое runtime-доказательство в реальных host UI.

Текущий шаг: Codex real UI scenario prep.

Что решает этот шаг: будущий агент получает точный сценарий, как проверить Codex app/plugin/chat UI тем же смысловым запросом, который проверяется через boundary path. Это закрывает слепое пятно "агент сам догадается, куда нажимать" и запрещает подменять живой UI proof JSON-логами, marker-only screenshots или core trace.

Что этот шаг не решает: он не запускает Codex UI, не отправляет prompt, не проверяет ответ в живом чате, не доказывает token savings, semantic quality, learnability, DPO, promotion, locator update, Claude readiness, Cursor readiness или full product readiness.

Счёт по этому prep-slice:
- Всего действий: 4.
- Сделано: 4, когда README.md, scenario.json, JSON parse и sentinel/format checks пройдены.
- В процессе: 0 после успешной верификации пакета.
- Осталось: 0 внутри scenario-prep slice; дальше нужен отдельный runtime slice.

Product readiness count remains 0.

## Preconditions перед реальным запуском

1. Codex app должен быть видимым и разблокированным на локальном компьютере.
2. Нельзя вводить, записывать, скриншотить или логировать пароль, PIN, токены, cookie, recovery codes или другие secrets.
3. Должна быть найдена правильная поверхность: нужный Codex thread/project/plugin route для XIIGen Smart Skills.
4. Если появляется login/session prompt, агент останавливается до ввода credentials; пользователь завершает login локально.
5. Если виден model/reasoning tier и он не соответствует ожидаемому, агент записывает `MODEL_TIER_WARNING`; агент не скрывает и не исправляет это молча.
6. Если окно/поверхность не найдены, результат сценария `TARGET_WINDOW_NOT_FOUND`, а не попытка доказать readiness через другой путь.

## Dual-path same-scenario proof

### Path A: non-UI boundary path

Path A доказывает только adapter/core boundary. Он должен использовать тот же exact prompt, что и Path B, и собрать:
- `XiigenExtensionRequest`;
- `RequestedSkills`;
- `AvailableSkills`;
- core response;
- adapter/plugin trace;
- проверку, что host не добавил свой selector/scorer и не подменил решение core.

Path A может ссылаться на уже существующее Codex non-UI installed plugin entrypoint proof или быть запущен заново с тем же prompt. Но Path A не является Codex chat UI proof.

### Path B: real Codex UI path

Path B должен проверить реальный видимый путь: пользовательский prompt в Codex UI -> XIIGen extension/plugin route -> `llm_mvp_core` -> видимый ответ в Codex UI.

Точные шаги:
1. Открыть уже разблокированное окно Codex app.
2. Найти нужный thread/project/plugin surface для XIIGen Smart Skills.
3. Сделать screenshot до ввода prompt: окно Codex, выбранный thread/project/plugin route, без secrets.
4. Ввести exact prompt из scenario.json.
5. Сделать screenshot перед отправкой prompt, чтобы было видно введённый текст и правильную поверхность.
6. Отправить prompt.
7. Дождаться ответа или явной ошибки.
8. Сделать screenshot после ответа.
9. Собрать correlation evidence, если доступно: request id, trace id, plugin log, adapter trace, core request/response trace.
10. Записать human summary: что увидел пользователь, какой ответ появился, совпадает ли он с ожидаемым contract.

## Exact prompt

```text
Через XIIGen Smart Skills разберись с задачей: пользователь хочет улучшить документацию продукта так, чтобы она была понятна человеку. Переведи запрос в универсальную задачу, найди универсальный план оркестрации и выбери подходящие скилы. Используй только универсальную часть без привязки к конкретному языку программирования, фреймворку или стеку.
```

Этот prompt специально проверяет три вещи: prompt -> task, task -> orchestration plan, task/plan -> skills. Он не должен заставлять систему выбирать стековые правила.

## Anti-substitution rules

- JSON/log/core trace alone is not Codex UI proof.
- marker-only screenshot is not full UI proof.
- Codex adapter proof is not Codex chat UI readiness.
- Path A без Path B не доказывает живой Codex UI.
- Screenshot только с открытым окном, но без введённого prompt и видимого ответа, не доказывает runtime readiness.
- Core success без видимого ответа в Codex UI остаётся boundary evidence, не product readiness.

## Acceptance criteria

- `PASS`: Path A и Path B выполнены с тем же exact prompt; видимый Codex UI показывает ответ; core trace найден; redaction log чистый; нет host-side selector/scorer substitution.
- `PARTIAL`: один из путей выполнен, но полного dual-path proof нет.
- `BLOCKED_LOGIN_REQUIRED`: Codex просит login/session/credentials; агент остановился до credentials.
- `TARGET_WINDOW_NOT_FOUND`: нужное окно или поверхность Codex не найдены.
- `RESPONSE_NOT_VISIBLE`: core/plugin trace есть, но ответ не виден в Codex UI.
- `CORE_TRACE_MISSING`: UI ответ есть, но нет core/correlation trace.
- `WRONG_THREAD_OR_SURFACE`: действия выполнены не в нужном thread/project/plugin route.
- `MODEL_TIER_WARNING`: модель или reasoning tier видимы и отличаются от ожидаемого; это warning, не readiness proof.

## Evidence required after execution

Будущий runtime slice должен сохранить:
- window inventory;
- screenshot до ввода prompt;
- screenshot перед отправкой prompt;
- screenshot после ответа или ошибки;
- core request/response/trace;
- adapter/plugin trace;
- redaction log;
- human summary;
- verdict по acceptance criteria.

## Что этот сценарий не докажет сам по себе

Даже выполненный Path B не доказывает всё сразу. Отдельными фазами должны доказываться:
- token savings;
- semantic quality improvement;
- learnability/training;
- DPO quality;
- checkpoint mutation/promotion;
- locator update;
- Claude readiness;
- Cursor readiness;
- full product readiness.
