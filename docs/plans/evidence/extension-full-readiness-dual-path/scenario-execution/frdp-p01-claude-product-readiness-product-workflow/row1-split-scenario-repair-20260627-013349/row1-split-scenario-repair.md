# Row1: ремонт split-сценария только для читаемости

claim_scope: только плановый ремонт читаемости Row1 split-артефакта `frdp-p01-claude-product-readiness-product-workflow`.
evidence_scope: этот пакет фиксирует разбиение Row1 на малые будущие сценарии и не является выполнением продукта, UI, runtime, тестов, eval, heldout или training.
count_claimed: всего сценариев 28; принято в scope 14/28; product-ready 0/28; осталось 14; Row1 разбит на 7 будущих sub-сценариев/гейтов длительностью <=10 минут каждый.
independent_evidence_count: 2 целевых файла прочитаны в текущем repair-заказе перед переписыванием.
subagent_id_or_NONE: Dalton the 2nd / 019f0610-7555-7ff3-8f59-3d40babb17ad
downgrade_needed: да для любых утверждений о product readiness, runtime, UI, token savings, semantic quality, training, checkpoint promotion или locator update; этот пакет доказывает только плановое разбиение.

## Человеческий статус

Готово в scope: 14/28. Product-ready: 0/28. Осталось: 14. Row1 остается открытым. Этот артефакт является только split-only planning repair: он не выполняет сценарий, не отправляет prompt в Claude и не доказывает готовность продукта.

## Что Row1 должен проверить в целом

Row1 должен в будущем проверить Claude extension product workflow: от пользовательского запроса до видимого ответа в Claude, с одним и тем же сценарием через extension/core boundary, token/accounting evidence, semantic-quality evidence, lifecycle evidence, checkpoint/locator evidence и Claude host-visible behavior.

Текущие доказательства Row1 не доказывают полный product workflow. Поэтому этот пакет только разбивает Row1 на маленькие, читаемые, не-мутирующие sub-сценарии/гейты. Он не запускает продукт, runtime, UI, тесты, eval, heldout или training.

## Общие правила Row1

- Dual-path same-scenario rule: automatic path и локальный Claude UI/agent path должны проверять один и тот же текст сценария, marker/correlation id, expected response contract и поля сравнения доказательств, где это возможно.
- Product-ready lock: нельзя утверждать product readiness до прохождения всех sub-сценариев Row1, независимой проверки доказательств и финального Row1 review.
- Unknown Claude route: если точный маршрут Claude неизвестен, это discovery scenario, а не общий stop/blocker для всего плана.
- Нельзя изобретать Claude host capability. Если программа, окно, input route или session state не названы и не проверены, результат отдельного sub-сценария должен быть `NEEDS_FIX` или `BLOCKED`.
- Этот split repair не разрешает product/runtime/UI/tests/eval/heldout/training, mutation, send action, checkpoint promotion, locator update, code edit, board edit, commit или push.

## Сценарий S1 - Row1 same-scenario contract и prompt package

Timebox: <=10 минут.

Purpose: определить точный единый сценарий, который позже должны использовать automatic path и локальный Claude path.

Что проверяет: существует стабильный Row1 scenario contract: текст пользовательского запроса, marker/correlation id, expected response shape, имена будущих evidence-файлов, non-claim locks и поля сравнения.

Что не доказывает: extension/core behavior, Claude host behavior, token savings, semantic quality, lifecycle readiness, checkpoint promotion, locator update, visible answer или product readiness.

Inputs: текущая Row1 строка board, счетчики product-readiness action plan, счетчики status dashboard и этот split packet.

Разрешено: создать planning-only contract packet в будущей Row1 evidence subfolder; назвать один и тот же prompt/marker; перечислить ожидаемые будущие evidence-файлы.

Запрещено: запускать product/runtime/UI/tests/eval/heldout/training; отправлять prompt в Claude; менять checkpoint/locator/model/product code; редактировать board/action plan/dashboard; stage/commit/push.

Критерии PASS: contract packet содержит точный prompt, marker, expected answer contract, evidence filenames, forbidden actions и claim locks; JSON parses; reviewer понимает будущие действия без догадок.

Критерии NEEDS_FIX: prompt, marker, expected response, evidence filenames или claim locks расплывчаты.

Критерии BLOCKED: из текущих planning artifacts нельзя определить приемлемый non-mutating scenario text.

Next review requirement: independent planning review contract packet до любого automatic или Claude-route execution work order.

## Сценарий S2 - Automatic path до extension/core boundary

Timebox: <=10 минут.

Purpose: подготовить или, только при отдельной execution authority, позже выполнить automatic path до extension/core boundary для того же Row1 scenario, если доступен approved non-mutating entrypoint.

Что проверяет: максимум то, что тот же Row1 request может дойти до approved adapter/core boundary и создать structured boundary packet.

Что не доказывает: Claude local host readiness, visible Claude answer, full extension readiness, token savings, semantic improvement, lifecycle readiness, product readiness или formal joined pass.

Inputs: S1 contract, ссылки на существующие boundary evidence, approved non-mutating command/callable entrypoint при отдельной авторизации.

Разрешено: в этом planning split только перечислить required evidence fields; в будущем отдельно авторизованном execution packet - запустить только точный approved non-mutating boundary command, если он существует, и сохранить request/response/log/status artifacts.

Запрещено: изобретать entrypoint; запускать broad tests; вызывать mutating training/lifecycle/checkpoint/locator operations; использовать helper-only proof как Claude host proof; stage/commit/push.

Критерии PASS: будущий packet называет точный approved command/entrypoint, записывает тот же marker, structured request/response, exit status если запуск был разрешен, и boundary-only claim locks.

Критерии NEEDS_FIX: command/entrypoint отсутствует, неоднозначен, мутирующий или не связан с S1 same scenario.

Критерии BLOCKED: approved non-mutating automatic route недоступен; отсутствие route фиксируется как boundary blocker, не как product failure.

Next review requirement: independent review должен проверить, что route approved, non-mutating, same-scenario и boundary-only.

## Сценарий S3 - Claude auth/session preflight

Timebox: <=10 минут.

Purpose: установить, можно ли безопасно проверить реальную Claude host session до любого input или send action.

Что проверяет: максимум то, что named Claude program/window/session target доступен, signed in или явно not signed in, и безопасен для будущей no-send работы.

Что не доказывает: input route, message send, visible answer, extension behavior, product readiness или Claude readiness.

Inputs: S1 contract и текущая формулировка host-route blocker.

Разрешено: в будущем отдельно авторизованном host-preflight packet открыть или сфокусировать точное named Claude app/window; сделать redacted session-state evidence; записать program/window title, account/session state и no-secrets confirmation.

Запрещено: захватывать secrets; вводить credentials; отправлять prompts; выводить login из stale screenshots; использовать browser/private pages без явной authority; менять product/runtime state; stage/commit/push.

Критерии PASS: exact program/window target назван; signed-in или logged-out state видим и redacted; no secrets captured; имена evidence-файлов записаны.

Критерии NEEDS_FIX: window target, account state или redaction instructions неполные.

Критерии BLOCKED: Claude opens anonymously/logged out, требуются credentials, или allowed host window не найден. Следующий шаг - login recovery или human-assisted session step с no secrets captured.

Next review requirement: independent review screenshots/logs на redaction, exact window identity и no-send compliance.

## Сценарий S4 - Claude input route discovery без send

Timebox: <=10 минут.

Purpose: обнаружить, существует ли безопасный input route в реальном Claude host без отправки сообщения.

Что проверяет: максимум то, что input surface существует и может быть идентифицирован, либо что точный input route неизвестен.

Что не доказывает: message delivery, answer generation, extension behavior, product readiness или local host pass.

Inputs: S1 contract, S3 session packet, exact host/window target если S3 passed.

Разрешено: в будущем отдельно авторизованном discovery packet inspect visible UI controls; click/focus только при authorization; capture screenshots of the route; записать control names/coordinates только как route evidence.

Запрещено: send или submit text; вставлять полный Row1 prompt; использовать unsupported automation, меняющую host state; вызывать product/runtime/UI execution beyond discovery; stage/commit/push.

Критерии PASS: exact input route documented with visible control evidence и no send action occurred.

Критерии NEEDS_FIX: route evidence ambiguous, screenshot не показывает target, или action log недостаточно детален.

Критерии BLOCKED: input control не найден или его нельзя безопасно сфокусировать. Unknown route остается discovery blocker и не останавливает весь план.

Next review requirement: independent route review должен подтвердить no-send behavior и exact route evidence.

## Сценарий S5 - Claude same-scenario no-send dry run

Timebox: <=10 минут.

Purpose: доказать только то, что S1 same-scenario prompt можно подготовить для Claude local path без submit.

Что проверяет: максимум local no-send preparation того же scenario marker и prompt в approved route.

Что не доказывает: что Claude получил prompt, сгенерировал answer, использовал extension, улучшил quality, сэкономил tokens или достиг product readiness.

Inputs: S1 contract, S3 preflight PASS, S4 route PASS.

Разрешено: в будущем отдельно авторизованном dry-run packet поместить только safe marker или shortened no-send draft, если это разрешено route; capture screenshot before any send; remove/clear draft если authorized и safe.

Запрещено: pressing Enter/Send; full prompt submission; hidden background send; training/eval/heldout/checkpoint/locator actions; product readiness claim; stage/commit/push.

Критерии PASS: evidence показывает same marker/no-send prompt preparation, no send action, clear before/after status и cleanup или explicit non-cleanup note.

Критерии NEEDS_FIX: no-send boundary unclear или prompt/marker не связан с S1.

Критерии BLOCKED: route не позволяет safe no-send draft preparation.

Next review requirement: independent review должен проверить same-scenario marker и no-send boundary до любого будущего send-authorized work.

## Сценарий S6 - Row1 component-evidence dependency ledger

Timebox: <=10 минут.

Purpose: сопоставить компоненты Row1 workflow с отдельными accepted evidence requirements до любого joined claim.

Что проверяет: максимум то, что существует dependency map для adapter/core, token/accounting, semantic quality, lifecycle, checkpoint, locator и Claude host behavior.

Что не доказывает: любую metric, host answer, token savings, semantic improvement, lifecycle readiness, checkpoint promotion, locator update или product readiness.

Inputs: S1 contract, S2 boundary plan/status, S3-S5 Claude route statuses, текущие board/action plan/status dashboard counters.

Разрешено: создать planning ledger, где для каждого component перечислены required accepted evidence packet, current status и claim lock.

Запрещено: запускать metrics/eval/training/tests; менять model/checkpoint/locator; объединять stale helper-only artifacts в host proof; увеличивать counters; редактировать board/action plan/dashboard; commit/push.

Критерии PASS: каждый Row1 component имеет required evidence, status, non-claim lock и named next packet.

Критерии NEEDS_FIX: component missing, mixed with another claim или не имеет evidence filename requirements.

Критерии BLOCKED: current planning artifacts не идентифицируют достаточно component categories; missing component source фиксируется как blocker для ledger completion.

Next review requirement: independent review должен подтвердить, что ledger не превращает guard/schema/trace/marker evidence в readiness.

## Сценарий S7 - Evidence packaging и final Row1 review gate

Timebox: <=10 минут для packaging prep; final review только после существования всех sub-scenario packets.

Purpose: определить, как Row1 evidence будет упаковано и проверено, когда все prior sub-scenarios завершены.

Что проверяет: существуют packaging/review rules, предотвращающие overclaim.

Что не доказывает: product readiness, joined pass, Claude readiness или любой missing component.

Inputs: reviewed packets from S1-S6, каждый с PASS или accepted blocker status.

Разрешено: подготовить final Row1 review checklist и JSON schema/fields для будущего review.

Запрещено: mark product-ready early; hide blockers; treat marker-only as local pass; treat boundary/API as host readiness; treat trace as savings; treat schema as training; treat checkpoint candidate as promotion; commit/push.

Критерии PASS: final review gate требует, чтобы все sub-scenarios passed или имели accepted blockers, все claim locks preserved, а counters не менялись до отдельного accepted final review.

Критерии NEEDS_FIX: final review checklist позволяет readiness claim из partial, stale, helper-only, marker-only или unreviewed evidence.

Критерии BLOCKED: любой prerequisite packet missing или unreviewed.

Next review requirement: independent final Row1 review после появления всех required packets; до этого `product_readiness_claim=false` и `formal_joined_pass=false`.

## Product-ready lock

Row1 нельзя называть product-ready из этого packet. Product-readiness claim locked до тех пор, пока:

1. S1-S7 завершены с accepted evidence.
2. Automatic path и local Claude path используют один и тот же scenario, где это возможно.
3. Unknown Claude route questions resolved через discovery или accepted blockers.
4. Component evidence для boundary, token, semantic quality, lifecycle, checkpoint, locator и host-visible behavior accepted и reviewed.
5. Final Row1 review explicitly passes и counters обновлены отдельным authorized board/status action.

До этого сохраняются lock-поля:

- `product_readiness_claim=false`
- `formal_joined_pass=false`
- `claude_host_readiness_claim=false`
- `token_savings_claim=false`
- `semantic_quality_claim=false`
- `training_or_learnability_claim=false`
- `checkpoint_promotion_claim=false`
- `locator_update_claim=false`

## Точный следующий sub-agent work order

Role: bounded Row1 S1 same-scenario contract planning sub-agent.

Max active time: 10 минут.

Current authority quote: "Ты bounded Row1 split-only scenario repair sub-agent в C:\Projects\llm_mvp_core. Max 10 minutes. Planning-only; no product/runtime/UI execution."

Task: создать один planning-only S1 contract packet для Row1 `frdp-p01-claude-product-readiness-product-workflow` в новой Row1 evidence subfolder. Определить exact same-scenario prompt text, marker/correlation id, expected response contract, future evidence filenames, claim locks и PASS/NEEDS_FIX/BLOCKED criteria. Не запускать product/runtime/UI/tests/eval/heldout/training. Не редактировать board/action plan/dashboard/code/AGENTS/CLAUDE/skills. No commit/push.

Allowed reads: этот `row1-split-scenario-repair.md/json`, product-readiness scenario board/action plan/status dashboard, next-scenario selection after Row28 files, plan file only if needed for scenario names.

Allowed writes: одна новая S1 planning evidence folder под `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-claude-product-readiness-product-workflow/` с одним Markdown packet и одним JSON packet only.

Return: PASS/NEEDS_FIX, files changed, commands run, exact next work order. No commit/push.
