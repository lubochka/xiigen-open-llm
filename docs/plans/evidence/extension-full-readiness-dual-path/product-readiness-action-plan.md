# Product readiness action plan

CURRENT TASK: создать общий план действий, который ведет продукт к реальной готовности через исполнение сценариев и доказательства, но не заявляет готовность сейчас.
active_task_sentence_source: текущая инструкция Luba: "Create a general product-readiness action plan that will guide continued execution to real readiness. This is a planning artifact, not product completion."

NOT THE TASK: реализация продуктового кода, запуск новых продуктовых проверок, изменение scenario board, stage, commit, push, заявление о готовности.

EVIDENCE-ONLY: `product-readiness-scenario-board.md`, `product-readiness-scenario-board.json`, `governance-loophole-repair.md`, `EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md`, `phase-14-user-visible-status-dashboard.md`.

## 1. Смысл плана

Этот файл задает порядок продолжения работы до реальной готовности продукта. Он не завершает продукт и не открывает заявление о готовности. Главная метрика прогресса здесь - движение строк scenario board, а не количество созданных отчетов.

Термины и служебные слова:

- application programming interface / программный интерфейс приложения: автоматизированный путь через ядро, адаптер, контракт запроса и ответа, trace, счетчики и проверки.
- user interface / пользовательский интерфейс: реальная локальная поверхность Claude, Cursor или Codex, где видно поведение продукта, marker, screenshot, video, log или честный blocker.
- Direct Preference Optimization обучающие пары: пары предпочтений для обучения или оценки, которые требуют provenance, split/leakage gate, метрик качества и связи с поведением candidate.
- packet / пакет доказательств: папка или набор файлов одного bounded subagent, где есть отчет, logs/screenshots/traces или честный blocker.
- review / проверка: отдельный приемочный просмотр пакета доказательств; она решает, можно ли обновлять счетчики.
- claim / заявление: утверждение о готовности или улучшении; в этом плане такие заявления остаются закрытыми до accepted evidence.
- host-route / маршрут к локальной программе: способ безопасно дойти до установленного Claude, Cursor или Codex без подмены реального поведения.
- same-marker / тот же маркер: одинаковая контрольная строка или идентификатор, связывающий автоматизированный путь и локальную программу.
- helper-only / только вспомогательный путь: доказательство через helper/script, которое не доказывает реальную локальную пользовательскую поверхность.

## 1.1 Глобальное правило исполнения через bounded subagents

После принятия этого плана любое дальнейшее исполнение сценариев идет только через bounded subagents. Parent/current session не становится исполнителем продукта: она может только создавать bounded work order, ждать результат, review/orchestrate, принимать или отклонять пакет доказательств и формировать следующий bounded work item.

Каждый bounded subagent ограничен 10 минутами. Если ожидаемый шаг больше 10 минут, он должен быть split до запуска. Если subagent timed out или был closed, его файлы не считаются accepted evidence, не двигают счетчики и не могут попасть в stage/commit/push, пока subagent не вернет protocol packet и отдельная проверка не примет его.

## 2. Исходные счетчики

Эти счетчики являются стартовой точкой для следующего исполнения и должны сохраняться или обновляться только после принятого scenario packet / пакета доказательств и review / проверки.

| Счетчик | Текущее значение | Человеческий смысл |
|---|---:|---|
| Всего сценарных строк | 28 | Полная доска сценариев: Claude 9, Cursor 9, Codex 10. |
| Full joined pass | 0 | Нет строки, где автоматизированный путь и локальная реальная программа вместе приняты как готовность. |
| Scoped accepted | 14 | Done: 14/28 scoped evidence accepted. Rows 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 27, and 28 are scoped accepted only. |
| Product-ready full pass | 0 | Product-ready: 0/28. No scenario opens product readiness. |
| Remaining | 14 | Current/remaining: 14 scenarios still open. |
| Readiness claims opened | 0 | Claims / заявления о готовности не открыты. |

Правило учета: новый packet / пакет доказательств может менять счетчики только после review / проверки. Если пакет не принят, счетчики остаются прежними, а результат становится следующим конкретным work item.

## 3. Нельзя завершать работу статусом "не доказано"

Работа не может считаться завершенной, пока все продуктовые требования не имеют accepted evidence или Luba явно не остановит работу. Downgrade, blocker, not proven, missing evidence или status-only artifact не являются completion. Каждый такой результат обязан создать следующий конкретный work item:

- какая строка scenario board остается открытой;
- какой путь не доказан: программный интерфейс приложения, пользовательский интерфейс или joined evidence;
- какой минимальный следующий пакет доказательств нужен;
- какие заявления остаются закрытыми;
- какие счетчики должны остаться без повышения.

## 4. Общий порядок движения к готовности

Фазы ниже двигают продуктовую готовность через сценарные строки. Документация и статусные отчеты разрешены только как сопровождение accepted evidence, а не как самостоятельный прогресс readiness.

| Фаза | Что доказывает | Строки сценариев | Какие доказательства должны появиться | Ожидаемое изменение счетчиков | Что нельзя заявлять |
|---:|---|---|---|---|---|
| 1 | Довести ближайшую partial строку до joined pass или точного blocker. | 11 | Cursor same-marker / тот же маркер local route packet / пакет локального маршрута, command/log/screenshot или exact blocker; join report с ссылкой на существующий core packet / пакет ядра. | Если принят pass: full joined pass 0 -> 1, partial 1 -> 0. Если blocker: partial остается 1 или переводится в blocked only после review / проверки; readiness claims 0. | Нельзя заявлять полную готовность Cursor extension или продукта. |
| 2 | Сдвинуть оставшиеся Cursor guard-only строки с наибольшей проверяемостью. | 12, 13 | Token before/after packet / пакет до-после по токенам и semantic held-out packet / пакет скрытой семантической оценки, каждый с workload/hash/rubric/log и Cursor-visible evidence или exact blocker. | Not moved 8 -> 6 при двух accepted packets / принятых пакетах; partial может вырасти до 3 или часть строк может стать full joined pass. | Нельзя заявлять token savings или semantic quality improvement без accepted numeric eval и host-visible evidence. |
| 3 | Разбить тяжелые Cursor строки на test-only non-mutating packets / тестовые немутирующие пакеты. | 14, 15, 16, 17, 18 | Read-only learnability eval design, Direct Preference Optimization обучающие пары provenance/eval packet, lifecycle dry-run packet, checkpoint candidate load/rollback packet, locator dry-run diff packet. | Not moved уменьшается только по accepted scenario packets / принятым сценарным пакетам; claims / заявления остаются закрытыми. | Нельзя заявлять training success, lifecycle readiness, checkpoint promotion или locator update. |
| 4 | Снять host-route / blocker маршрута к локальной программе для Claude entrypoint без product mutation. | 1, 2, затем 3-9 по очереди | Safe no-send Claude route packet / безопасный пакет маршрута без отправки или exact blocker; same-marker / тот же маркер local evidence; join report с существующим boundary evidence. | Blocked with next action 19 может уменьшаться по одной строке; full joined pass растет только после accepted join. | Нельзя заявлять Claude ready, если route доказан только helper / вспомогательным путем или marker-only. |
| 5 | Снять host-route / blocker маршрута к локальной программе для Codex entrypoint и real chat surface отдельно от активного разговора. | 19, 20, 21, затем 22-28 по очереди | Clean Codex local route packet / чистый пакет локального маршрута, real Codex app/chat marker proof или exact blocker; helper-only / только вспомогательное evidence остается downgraded. | Blocked with next action уменьшается только после accepted proof; row 21 не может стать pass без real Codex chat surface. | Нельзя заявлять Codex chat readiness по helper / вспомогательному пути, file association или active conversation evidence. |
| 6 | Собрать product workflow rows после накопления sub-evidence. | 1, 10, 19 | Product workflow boundary harness или split sub-scenarios; join report связывает adapter/core, token, quality, lifecycle, checkpoint, locator и host behavior. | Full joined pass может расти только по host-specific workflow rows. | Нельзя заявлять product readiness, если хотя бы один обязательный workflow component отсутствует. |
| 7 | Закрыть cross-host quality, token, learnability и lifecycle claims / заявления после host route evidence. | 3-9, 12-18, 22-28 | Reproducible before/after token metrics, held-out semantic eval, provenance/leakage checks, non-mutating lifecycle/checkpoint/locator proofs, host-visible behavior where claimed. | Partial/full counts обновляются строками; readiness claims opened остается 0 до final readiness review. | Нельзя заявлять savings, quality improvement или learnability from schema, trace or marker-only evidence. |
| 8 | Final readiness review / финальная проверка готовности только после accepted packets / принятых пакетов по всем требованиям. | 1-28 | Review ledger / журнал проверок over all accepted packets, exact counts, non-claim audit, hidden shortcut audit, final joined report. | Только здесь можно рассматривать readiness claims opened 0 -> N, если evidence действительно accepted. | Нельзя открывать final claim / финальное заявление при любом unresolved blocker, stale folder или unreviewed packet. |

## 5. Следующая исполнимая фаза

Следующий полезный шаг: Фаза 1A, строка 11 Cursor full extension entrypoint.

Почему именно она: это единственная partial строка. У нее уже есть accepted boundary и fresh core harness packet / свежий пакет запуска ядра. Самый короткий путь к реальному прогрессу - проверить, может ли локальный Cursor command/no-send route доказать same-marker / тот же маркер и связаться с тем же core request path.

Ограничение размера: один bounded subagent до 10 минут.

Если за 10 минут route / маршрут не найден, bounded subagent не расширяет scope. Он возвращает protocol packet / протокольный пакет:

- что пробовал;
- какие файлы создал;
- какой exact blocker найден;
- какие artifacts доказывают blocker;
- какие счетчики не меняются;
- какой следующий smaller work item нужен.

Фаза 1A не должна менять продуктовый код и не должна менять scenario board. Она может создать только scenario-execution packet / сценарный пакет под row 11 или blocker packet / пакет blocker для row 11.

## 6. Шаблон work order для Фазы 1A

Тип артефакта: scenario execution packet / пакет исполнения сценария.

Разрешенная область записи:

- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/`
- optional review note / необязательная заметка проверки в той же папке.

Запрещено:

- правки продуктового кода;
- изменение счетчиков scenario board;
- stage, commit, push;
- обучение модели, checkpoint mutation, locator update;
- широкий cleanup stale folders.

Обязательные доказательства:

- exact marker used;
- command or no-send local route attempt;
- log/screenshot/video or exact blocker;
- relation to existing `response.json`, `trace.json`, `command-result.json`;
- join decision: pass, partial, blocker, or not accepted;
- explicit non-claims.

Ожидаемый эффект на счетчики:

- accepted joined pass: full joined pass 0 -> 1; partial 1 -> 0;
- accepted blocker only: full joined pass 0; partial remains 1 unless review / проверка moves row 11 to blocker; readiness claims opened remains 0;
- timed out or closed without protocol packet / протокольного пакета: no count change.

## 7. Правило scope для bounded subagents

Любой bounded subagent, от которого ожидается работа больше 10 минут, должен быть split до запуска. Split должен оставлять один scenario row, один host, один evidence path и один review gate.

Файлы timed-out или closed bounded subagent остаются unaccepted, пока он не вернет protocol packet / протокольный пакет и review / проверка не примет его. Такие файлы нельзя stage/commit, использовать как proof или использовать для обновления счетчиков, пока accepted review не скажет, что это valid evidence.

Если packet / пакет доказательств становится больше одного сценария или одной route problem / проблемы маршрута, он должен остановиться на первой честной границе и создать следующий smaller work item.

## 8. Проверочные gates

At least one review / минимум одна проверка required after every scenario packet / после каждого сценарного пакета.

Review / проверка must check:

- the scenario row number and host match the work order;
- evidence paths exist and are inside allowed scope;
- claims / заявления are downgraded to the narrowest supported wording;
- no readiness claim / заявление о готовности is opened by a partial packet;
- current counters are preserved or updated exactly;
- stale/unreviewed folders are not counted;
- any blocker creates a next concrete work item.

The review packet / пакет проверки must say one of:

- accepted and count-changing;
- accepted and no count change;
- rejected with fix required;
- rejected as out of scope;
- unaccepted because timed out/closed/no protocol packet / протокольного пакета.

## 9. Правило фиксации в git

Только accepted reviewed packets / принятые и проверенные пакеты можно stage или commit. Stale, unreviewed, timed-out или foreign scenario folders остаются вне фиксации.

В текущей задаче полномочия на git commit явно запрещены. Этот план можно проверять, но нельзя stage, commit или push в этом turn.

Перед любым будущим git commit:

- перечислить exact accepted packets / точные принятые пакеты;
- перечислить exact reviewed files / точные проверенные файлы;
- показать изменения счетчиков;
- исключить stale/unreviewed scenario folders;
- не включать product readiness claim / заявление о готовности продукта, если final readiness review не разрешила его.

## 10. Шаблон фазового отчета

Использовать этот человекочитаемый шаблон после каждой фазы и каждого scenario packet / сценарного пакета.

```text
Фаза <номер>: <название>.
Шаг <номер>: <название шага>.
Что эта фаза решает: <человеческое объяснение, какое product-readiness gap она двигает>.

Счетчики:
- всего сценарных строк: <число>;
- full joined pass: <число>;
- partial: <число>;
- not moved: <число>;
- blocked with next action: <число>;
- readiness claims opened: <число>.

Сделано: <счетчик + конкретные артефакты>.
В процессе: <что сейчас исполняется или проверяется>.
Осталось: <оставшийся счетчик и строки сценариев>.

Сдвинутый сценарий: <номер строки, host, scenario id или NONE>.
Созданные доказательства: <пути к артефактам принятого пакета или exact blocker>.
Результат проверки: <accepted/rejected/unaccepted и почему>.
Что изменилось в счетчиках: <старое -> новое или no count change>.
Что нельзя заявлять: <closed claims / закрытые заявления и downgraded wording>.
Следующее действие: <один конкретный bounded work item>.
```

Отчет недействителен, если он дает только пути к файлам, машинные labels статуса или checklist rows без человеческого смысла, счетчиков и следующего действия.

## 11. Очередь категорий сценариев

После Фазы 1A использовать эту очередь, если Luba не изменит приоритет:

1. Cursor row 11: попытка joined evidence, потому что это единственная partial строка.
2. Cursor rows 12 и 13: token и semantic guard evidence уже существуют и могут быть превращены в concrete proof малыми packets / пакетами доказательств.
3. Cursor rows 17 и 18: checkpoint/locator можно начать как non-mutating load/dry-run proof до promotion.
4. Cursor rows 14, 15 и 16: split на read-only learnability, Direct Preference Optimization обучающие пары и lifecycle dry-run packets / dry-run пакеты lifecycle.
5. Claude row 2: repair route / ремонт маршрута, затем Claude rows 3-9.
6. Codex rows 20 и 21: repair route / ремонт маршрута, затем Codex rows 22-28.
7. Product workflow rows 1, 10 и 19: выполнять после того, как их sub-evidence станет достаточно сильным.
8. Финальная проверка готовности по всем accepted packets / принятым пакетам.

Эту очередь можно изменить только текущей инструкцией Luba или accepted review / принятой проверкой, которая показывает, что у другой строки есть более короткий proof path.

## 12. Оставшиеся пробелы после этого плана

- Готовность продукта (product readiness) не проверена.
- Полная готовность расширения (full extension readiness) не проверена для Claude, Cursor или Codex.
- Реальная Codex chat/user surface / пользовательская поверхность Codex не проверена.
- Экономия токенов (token savings) не доказана.
- Улучшение semantic quality / семантического качества не доказано.
- Learnability/training и качество Direct Preference Optimization обучающих пар не доказаны.
- Lifecycle backup/export/import/share/continue-training/rollback не выполнен как принятое продуктовое доказательство.
- Checkpoint promotion и locator update не выполнены как принятое продуктовое доказательство.
- Риск hidden shortcut / скрытого shortcut не закрыт.
- Все будущие изменения счетчиков по-прежнему требуют accepted scenario packet / принятого сценарного пакета plus review / плюс проверку.

Текущий корректный статус: план существует; scenario execution должен продолжаться, пока accepted evidence не покроет все требования продукта или Luba явно не остановит работу.

## 13. Row20 status update - Codex entrypoint

This addendum records the accepted Row20 status update without changing product-readiness counters.

Row20 `frdp-p01-codex-full-extension-entrypoint` now has accepted Path A boundary metadata evidence only: `PATH_A_BOUNDARY_METADATA_PRESERVED`, `path_a_acceptance_gate_passed=true`, `host_ui_proven=false`, `formal_joined_pass=false`, and `product_readiness_claim=false`.

Row20 Path B remains blocked with `CODEX_ROUTE_BLOCKED_CLEAN_SURFACE_NOT_PROVEN`. The blocker preserves `private_content_exposure_risk=true`, `host_ui_proven=false`, `formal_joined_pass=false`, and `product_readiness_claim=false`.

Counter rule after this update: product-ready extension count remains `0`; product readiness full pass remains `0`; full joined pass remains `0`; scoped Row20 evidence does not increment product readiness.

Exact next bounded action: continue clean Codex marker-only UI route/control-path recovery or exact blocker continuation. If the board advances to Row21 `frdp-p01-codex-codex-chat-ui-marker`, Row21 remains blocked/conditional until a clean host route is proven. Do not claim Codex extension product-ready from Row20 Path A boundary metadata or Row20 Path B blocker evidence.

## 14. Row21 status update - Codex chat UI marker

This addendum records the accepted Row21 blocker/status update without changing product-readiness counters.

Row21 `frdp-p01-codex-codex-chat-ui-marker` is accepted as blocker/status only: `ROW21_BLOCKED_UI_INPUT_CONTROL_NOT_FOUND`. The accepted evidence says `row21_route_attempted=true`, `row21_route_safely_available=false`, `row21_marker_evidence_captured=false`, `private_content_exposure_risk=true`, `host_ui_proven=false`, `formal_joined_pass=false`, and `product_readiness_claim=false`.

No prompt was typed, pasted, or sent through Codex UI. No visible Codex UI response was proven. No confirmed Codex composer/input control or safe target rectangle was available.

Counter rule after this update: product-ready extension count remains `0`; product readiness full pass remains `0`; full joined pass remains `0`; Row21 blocker/status does not increment product readiness.

Exact next bounded action: continue clean Codex marker-only input-surface route recovery or exact blocker continuation. If the board sequence advances to Row22, Row22 remains conditional on Codex host route recovery. Do not claim Codex extension product-ready from Row21 supporting boundary readback or Row21 blocker evidence.

## 15. Row22 status update - Codex token savings baseline

This addendum records the accepted-with-downgrades Row22 status update without changing product-readiness counters.

Row22 `frdp-p01-codex-token-savings-baseline` is accepted only as narrow deterministic local no-send token measurement evidence on the scripted workload/tokenizer. The accepted counts are `before_tokens=157`, `after_tokens=93`, `token_delta=-64`, and `token_reduction_observed=true`. The quality guard is accepted as `15/15` passing assertions on the same deterministic workload/tokenizer.

Reviewer record: Descartes / `019f0513-9d11-7cd1-be55-544990bba642`, decision `ACCEPTED_WITH_DOWNGRADES`.

Downgrade recorded: metadata supports `run_attempts=1` and `row22_measurement_command_run=true`; metadata lacks recorded `exit_code`, so recorded exit code `0` is not independently proven.

Boundary locks remain: `boundary/API evidence != host readiness`; `marker-only != full local desktop pass`; `trace != savings`; `schema != training`; `checkpoint candidate != promotion`.

Counter rule after this update: product-ready extension count remains `0`; product readiness full pass remains `0`; full joined pass remains `0`; formal joined pass remains `false`; Row22 accepted-with-downgrades evidence does not increment product readiness.

Not proven after Row22: host-visible candidate behavior is `not_proven`; product readiness is `not_proven`; full Codex UI readiness is `not_proven`; real Codex runtime token savings are `not_proven`; semantic quality improvement is `not_proven`.

Exact next bounded action: continue to the next Codex scenario/work item, Row23 `frdp-p01-codex-semantic-quality-heldout`, because stopping is allowed only when 3 extensions are product ready. Do not claim Codex extension product-ready from Row22 deterministic local no-send measurement evidence.

## 16. Row23 status update - Codex semantic quality heldout

This addendum records the reviewed Row23 blocker/status update and reviewed revised design acceptance without changing product-readiness counters.

Row23 `frdp-p01-codex-semantic-quality-heldout` is accepted only as reviewed blocker/status: `blocked_not_proven`. The accepted scope is Row23 blocker/status packet only. It does not prove semantic quality improvement, heldout pass, host UI readiness, product readiness, or formal joined pass.

Reviewer record: Darwin / `019f0523-d5ab-7a91-a4a6-6ddef4b91238`, decision `ACCEPTED`.

Follow-up reviewer record: Confucius / `019f0530-2ce8-7920-8d3a-f62901e73067`, decision `ACCEPTED` for Row23 follow-up command-discovery blocker only. Accepted scope supports only `command_found_status=not_found` and `semantic_quality_status=blocked_not_proven`; it does not prove semantic quality improvement, heldout pass, host UI readiness, product readiness, or formal joined pass.

Revised design reviewer record: Arendt / `019f0540-a8ec-7293-856c-468ff7a41f69`, decision `ACCEPTED` for proposed future Row23 evidence path only. The accepted design allows deterministic fixture construction only as runner-contract/readback smoke evidence, not eval generation, not a heldout/eval dataset, and not support for semantic-quality claims. Real quality claims require pre-existing or separately approved heldout input plus independently reviewed same-scenario Row23 Codex before/after and host-visible evidence. `design_status=proposed`; `fixed_finding_status=fixed`; `product_readiness_claim=false`; `formal_joined_pass=false`.

Downgrade recorded: Row23 has 0 semantic-quality runs. The reviewer did not verify directory-wide file counts, so this action plan does not claim an exact directory file count. The revised design is accepted only as proposed future implementation guidance, not as a heldout pass or semantic-quality proof.

Boundary locks remain: `boundary/API evidence != host readiness`; `marker-only != full local desktop pass`; `trace != savings`; `schema != training`; `checkpoint candidate != promotion`.

Counter rule after this update: product-ready extension count remains `0`; product readiness full pass remains `0`; full joined pass remains `0`; formal joined pass remains `false`; Row23 reviewed blocker/status evidence does not increment product readiness.

Not proven after Row23: semantic quality improvement is `not_proven`; heldout pass is `not_proven`; host UI readiness is `not_proven`; product readiness is `not_proven`; full Codex readiness is `not_proven`.

Runner-contract/readback smoke reviewer record: Newton / `019f054d-7e4c-70e3-9212-6044495d315d`, decision `ACCEPTED_WITH_FINDINGS`. Accepted implementation scope is local deterministic no-send runner-contract/readback smoke artifacts only. `runner_contract_status=passed`; `semantic_quality_status=not_proven`; `product_readiness_claim=false`; `formal_joined_pass=false`.

Runner-contract finding preserved: worker-reported smoke execution was not independently rerun by reviewer; reviewer verified saved result and script source only.

Heldout/same-scenario blocker reviewer record: Curie / `019f055e-8349-7391-a65e-a2e8c3f4e4ee`, decision `ACCEPTED`. Accepted scope is Row23 heldout/same-scenario discovery blocker only. It records `preexisting_or_separately_approved_heldout_input_status=not_found`, `same_scenario_before_after_status=not_found`, `heldout_command_status=not_found`, `semantic_quality_status=blocked_not_proven`, `product_readiness_claim=false`, and `formal_joined_pass=false`.

The runner-contract/readback smoke remains accepted-with-findings only as runner-contract evidence. It does not become heldout evidence, semantic-quality proof, host-visible proof, product readiness, or formal joined pass.

No supported claim after the runner-contract review: semantic-quality evaluation, heldout pass, product readiness, host readiness, eval generation, training, DPO, checkpoint or locator mutation, host UI automation, hidden/external AI, adapter/core invocation, git action, token savings, learnability, checkpoint promotion, and locator update remain not proven/not performed.

Exact next bounded action: Row23 authority/input request or plan-repair path. Provide pre-existing or separately approved Row23 Codex heldout input plus accepted same-scenario before/after and host-visible evidence, or authorize a different exact safe evidence path. Do not advance to Row24 as if Row23 semantic quality passed.

## 17. Row23 authority/blocker repair accepted status update

This addendum records the reviewed Row23 authority/blocker repair packet without changing product-readiness counters.

Row23 `frdp-p01-codex-semantic-quality-heldout` remains accepted only as blocker/status: `blocked_not_proven`. The newly accepted scope is authority-material absence repair only. It does not prove semantic quality improvement, heldout pass, host UI readiness, product readiness, or formal joined pass.

Authority/blocker reviewer record: Godel / `019f056c-afbd-7511-a406-5ac42c04ca2d`, decision `ACCEPTED`. Accepted scope is Row23 blocker/authority-material absence repair packet only.

Accepted authority/blocker fields: `authority_material_status=not_found`; `preexisting_or_separately_approved_heldout_input_status=not_found`; `same_scenario_before_after_status=not_found`; `host_visible_evidence_path_status=not_found`; `alternative_safe_evidence_path_status=not_found`; `semantic_quality_status=blocked_not_proven`; `product_readiness_claim=false`; `formal_joined_pass=false`.

Runner-contract/readback smoke remains accepted-with-findings only as runner-contract evidence. Preserve the narrow finding: the smoke execution was not independently rerun by reviewer Newton; Newton verified saved result and script source only. `runner_contract_status=passed` remains runner-contract level only.

Counter rule after this update: product-ready extension count remains `0`; product readiness full pass remains `0`; full joined pass remains `0`; formal joined pass remains `false`; Row23 remains open/blocked.

Boundary locks remain: `boundary/API evidence != host readiness`; `marker-only != full local desktop pass`; `trace != savings`; `schema != training`; `checkpoint candidate != promotion`.

Exact next bounded action: Row23 authority/input request or plan-repair path before any heldout/same-scenario execution. Do not advance to Row24 as if Row23 semantic quality passed.

## 18. Row23 authority/input request reviewed accepted status update

This addendum records the reviewed Row23 authority/input request packet without changing product-readiness counters.

Row23 `frdp-p01-codex-semantic-quality-heldout` remains accepted only as blocker/status: `blocked_not_proven`. The newly accepted scope is authority/input request preparation only. It does not prove semantic quality improvement, heldout pass, host UI readiness, product readiness, or formal joined pass.

Authority/input request reviewer record: Wegener / `019f057c-2f1a-7530-9698-55ac4a6ddbe4`, decision `ACCEPTED`; `revision_status=accepted`; `fixed_finding_status=fixed_verified`; `request_status=prepared`.

Accepted authority/input fields: `authority_material_status=not_found`; `preexisting_or_separately_approved_heldout_input_status=not_found`; `same_scenario_before_after_status=not_found`; `host_visible_evidence_path_status=not_found`; `alternative_safe_evidence_path_status=not_found`; `semantic_quality_status=blocked_not_proven`; `product_readiness_claim=false`; `formal_joined_pass=false`.

The request is prepared-not-proven: it asks for one pre-existing or separately approved Row23 Codex heldout input/path, accepted same-scenario before/after criteria, accepted host-visible evidence path, or a different exact safe evidence path before any heldout/same-scenario execution.

Runner-contract/readback smoke remains accepted-with-findings only as runner-contract evidence. Preserve the narrow finding: the smoke execution was not independently rerun by reviewer Newton; Newton verified saved result and script source only. `runner_contract_status=passed` remains runner-contract level only.

Counter rule after this update: product-ready extension count remains `0`; product readiness full pass remains `0`; full joined pass remains `0`; formal joined pass remains `false`; Row23 remains open/blocked.

Boundary locks remain: `boundary/API evidence != host readiness`; `marker-only != full local desktop pass`; `trace != savings`; `schema != training`; `checkpoint candidate != promotion`.

Exact next bounded action: wait for, supply, or approve exactly one safe Row23 input/path before any heldout/same-scenario execution. Do not advance to Row24 as if Row23 semantic quality passed.

## 19. Row27 status update - Codex checkpoint promotion reload

This addendum records the reviewed Row27 status update without changing product-readiness counters.

Row27 `frdp-p01-codex-checkpoint-promotion-reload` is accepted only as scoped temp/staging checkpoint reload and rollback evidence after execution, fix, review, and metadata repair. Row27 meaning: temp/staging checkpoint reload and rollback path checked without production mutation.

Evidence folder: `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/row27-execution-20260627-001332/`.

Counter rule after the Row27-only update is superseded by the Row28 status update below. Product readiness full pass remains `0`; full joined pass remains `0`; formal joined pass remains `false`; Row27 does not increment product readiness.

Non-claims after Row27: Row27 cannot be used to claim product readiness, real promotion, locator update, semantic quality, training, token savings, or host/UI readiness. It also does not prove production locator update, production checkpoint mutation, full extension readiness, or product-ready Codex behavior.

Exact next bounded action: continue remaining open scenarios with the same no-overclaim gates. Do not mark Codex extension product-ready from Row27 temp/staging reload and rollback evidence.

## 20. Row28 status update - Codex locator update reload

This addendum records the reviewed Row28 status update without changing product-readiness counters.

Row28 `frdp-p01-codex-locator-update-reload` is accepted only as scoped dry-run/staging locator update/reload and rollback evidence after prep, review, dry-run execution, review, and metadata repair. Row28 meaning: dry-run/staging locator update/reload and rollback path checked; production locator hash unchanged; UI path remained blocker.

Evidence folder: `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-locator-update-reload/row28-execution-20260627-010525/`.

Counter rule after this update: Done: 14/28 scoped evidence accepted. Product-ready: 0/28. Current/remaining: 14 scenarios still open. Product readiness full pass remains `0`; full joined pass remains `0`; formal joined pass remains `false`; Row28 does not increment product readiness.

Non-claims after Row28: Row28 cannot be used to claim product readiness, real locator update, checkpoint promotion, semantic quality, training, token savings, or host/UI readiness. It also does not prove production locator update, production checkpoint mutation, full extension readiness, or product-ready Codex behavior.

Exact next bounded action: continue remaining open scenarios with the same no-overclaim gates. Do not mark Codex extension product-ready from Row28 dry-run/staging locator update/reload and rollback evidence.
