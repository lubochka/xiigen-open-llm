# Добавочные поправки к контрактам карточек — без изменения защищённого дерева

**Статус:** `DOCUMENTATION_PROPOSALS_NOT_APPLIED`, 21 октября 2026. Core `26332500f76ea8345256c73ecfd44c0023b7e883`; planning `14308ff181c5a93d837d92b85b2f35c41b863565`. Пользователь запретил менять `docs/plans/replanning-possible-diffs`; ни один файл этой папки здесь не меняется. Предложения ниже помещаются только в additive пакет `possibleDifs`. Они не являются разрешением на запуск опыта, новый attempt, production transfer или исправление продуктовых тестов.

Проверка index↔registry нашла **0 расхождений в 50 stable ID/card/anchor/next**. Это узкий структурный результат. Сопоставление самих команд и смысла домена оставило конкретные замечания ниже; наличие 50 согласованных маршрутов не означает 50 READY экспериментов. [Покрытие](registry-coverage.md)

## A-EXEC-01. Два разных literal launcher для одного wrapper

**Приоритет:** до исполнения №37/38; это зависимость запуска настоящего обучения, а не отдельный учебный эксперимент. Затронуты `GRAD-ACCUM-COPY-01` и `FROZEN-BROADCAST-MAP-01`.

| Поле/источник | Что написано сейчас |
|---|---|
| [GRAD card:90][agc-command], [FBM card:104][fbm-command] | `pwsh -NoProfile -File '<тот же Run-With-Learning-Signal.ps1>'` |
| [Общий contract27:110–116][contract27] | «Единственные внешние команды исполнения строго»: те же `pwsh` команды |
| [Output contract26:41–42][contract26] | `powershell -NoProfile -ExecutionPolicy Bypass -File <тот же wrapper>` |
| [JSON registry:1290–1292,1319–1321][registry-command] | `entry` и `commands[0]` также используют `powershell -ExecutionPolicy Bypass` |

**Доказано:** авторские команды называют разные executable tokens и разные наборы аргументов. **Не доказано:** что одна из этих команд уже отказала, какая версия установлена на целевой машине, либо что обе эквивалентны для будущего wrapper. Ни одна команда в этом аудите не запускалась. Различие кавычек вокруг пути само по себе конфликтом не считается; здесь различаются executable/arguments.

### Конкретный проект резолюции для автора карточек

Выбранная спецификация предложения — **Windows x64, PowerShell Core 7, `PSEdition=Core`, исполняемый файл `pwsh`**, поскольку именно его требуют две специальные карточки и общий contract27. Это проект согласования, а не утверждение о доступной версии runtime. Точный установленный patch-version записывается будущим единственным prepare owner в `environment.json` до первого обучения; A/B используют один и тот же путь executable и ту же версию. Отсутствие подходящего `pwsh` даёт `PREPARATION_ENVIRONMENT_UNAVAILABLE` до Train; исполнитель не подменяет его `powershell`, не устанавливает runtime и не ищет новый host сам.

Один автор документационного контракта должен согласовать следующие поля одновременно; исполнитель эксперимента их не выбирает:

| ID | Окончательная команда в предлагаемой версии |
|---|---|
| GRAD-ACCUM-COPY-01 | `pwsh -NoProfile -File 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1/host/Run-With-Learning-Signal.ps1'` |
| FROZEN-BROADCAST-MAP-01 | `pwsh -NoProfile -File 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FBM1/host/Run-With-Learning-Signal.ps1'` |

Места для будущего согласования: `future_outputs[id].entry`, `future_outputs[id].commands[0]`, строка соответствующего ID в contract26, platform/runtime field, обе специальные карточки, contract27 и соответствующий executor prompt. H/R, source pin, wrapper filename, owner nonce, 1000 COMMON SFT, qualification, arm budget, source closure и `next_id` остаются прежними. Проверка этой поправки — чтение и сопоставление exact tokens/fields; она **не требует запуска обучения**. Пока защищённое дерево менять нельзя, этот файл служит конкретным кандидатом интеграции, а не скрытой правкой canonical registry.

Обязательный текст для обоих обновляемых prompts: «Модель исполнителя — default текущей сессии без override. Читай уже назначенные skills из карточки и контракт31. Один owner запускает один указанный wrapper. Живую операцию ждать по тому же handle, bounded wait не длиннее 60 секунд; не объявлять never-started по пустому log, не делать relaunch/новый suffix. Прямой STOP/NEXT немедленен: без дописывания старого отчёта, review, hashes, Git или новых model calls. Результаты сохранять. Русский отчёт в существующем R/report.md; полезный выигрыш любой метрики и регрессии рядом».

## A-DOMAIN-01. Домены двух Wiki learner карточек ещё названы по объекту запроса

**Приоритет:** документная точность после блокеров обучения и запуска; новых runs не требует.

[Индекс №29/30][wiki-index] содержит домен `Документация / Wiki`. Но [WIKI-FEATURE-KEYS-01:88–95][wiki-keys] задаёт `CodeTraceTraining / code-trace-builder-unit` и `WikiQuestionAnswer / project-local-wiki-orchestrator`; карточка прямо говорит, что учится выбор route, а downstream CodeTraceBuilder не вызывается. [WIKI-FEATURES-ONCE-01:74–80][wiki-once] задаёт ту же семантику. По [словарю тем][topics] это **оркестрация (D07), выбор маршрутов Wiki**. У WT01/WM01 в текущем индексе такая правка уже сделана.

**Точная добавочная правка:** для обоих ID назначить `learning_topic=orchestration`, `domain_profile=D07`, `task_scope=Wiki request routing`; в пользовательской колонке написать «Оркестрация (D07), маршруты Wiki». Эти поля должны одинаково отображаться в строках индекса, карточках, fixture metadata, prompts и будущем report header. CodeTraceTraining/WikiQuestionAnswer остаются классами одного маршрутизатора. TRAIN/EXAM, gold, labels, source, budget и порядок не меняются. Не переименовывать эти две метки в две обучаемые темы и не добавлять обучение нижележащих Units.

## A-STATUS-01. Сохранённые успехи не терять, незакрытые цели не закрывать

Это **не новая находка**: актуальный core в основном уже содержит правильные ограничения. Здесь даны точные значения для additive registry, чтобы старый snapshot или краткая сводка не восстановили ложный статус.

| Stable ID | execution_status | learning/hypothesis status, который требуется сохранить |
|---|---|---|
| N63-INGRESS-LENGTH | `ATTEMPT_CONSUMED` | Исходное 0→0 невалидно для учебного вывода; отдельный №11 designed, ещё не запущен |
| N63-AR-CAP | Исходная попытка сохранена; отдельная probe завершена | Original invalid 0→0; probe — успех частичного retention A07/EO02 с регрессиями и неравной ценой; full response OPEN |
| N63-CORRECTION-COLLISION | `PARTIAL_A`; фактически 1 DPO+1 SFT, B unmeasured | Original invalid 0/23; отдельная завершённая probe 22/23→A20/B21, partial success, full response OPEN; current source≠historic recipe |
| WIKI-MARGIN-01 | A0/B1 model update, сохранённые raw | Trigger сработал, B2/4=constant-class 2/4; discrimination OPEN, OOD errors рядом |
| N60-E1 | A/B `COMPLETED`, поздний summary failure отдельно | Valid named Unit comparison; false-reject success 1→0, false-accept regression 0→1, total accuracy 3/4→3/4 |

Нельзя задним числом менять raw counters, переписывать старый опыт под новую квалификацию или объявлять историческое `UNKNOWN` итогом после появления полного JSON. Необходимо хранить независимые `execution_status`, `learning_validity`, `metric_results`, `hypothesis_scope`, `production_eligibility`. Для неизвестной операции count=null; отсутствующее плечо не равно нулю. Детальные ссылки и значения — в [saved-results-audit.md](saved-results-audit.md).

## A-REPAIR-01. Требования к следующему ремонту открытого N63C

**Не новая назначенная карточка и не разрешение повторить probe.** Сначала PA01 и исправленный ingress в текущем порядке; открытая N63C цель требует авторского полного рецепта до исполнения. 600 acquiring updates уже дали 22/23, поэтому простое повторение этих чисел не гарантирует полный ответ; 4 updates старой карточки тем более не являются достаточным learned-before. [Текущая карточка N63C][nc-card]

Автор будущей отдельной постановки должен зафиксировать заранее: один предмет C#; точный source/binary и learner; literal TRAIN/EXAM/negative controls и семейства; невыбранный по EXAM snapshot или конечный acquiring budget; predicate полного T1 до вмешательства, если проверяется именно утрата целого ответа; какие частичные метрики разрешено признать успехом при неполном навыке; одинаковые before-state/optimizer/RNG/teacher material; единственный factor stale-vs-corrected SFT; все реальные acquiring/arm/readout costs; exact H/R/entry/default model/skills/русский report; обработку existing claim и late-save failure. Если предикат не достигнут, записать выполненные updates и оставить цель OPEN, не увеличивать бюджет до PASS и не запускать arms, требующие отсутствующего before.

Любое измеренное полезное улучшение по валидной именованной метрике остаётся успехом этой метрики/домена; accuracy↑ не обязательна. Это не разрешает выдавать 0→0, отсутствующее обучение, constant-class collapse или частичные tokens за целый навык. Условия успеха и область вывода должны стоять отдельно.

## Контроль пользовательских правил в этом дополнении

| Правила пользователя | Как соблюдены / что проверено |
|---|---|
| 1,3,4,6,11 | Реальные learner→state→consumer и отдельные saved outcomes; не создан внешний «эксперимент» по форматированию; production не меняется |
| 2,15,18 | Метрики и регрессии вместе; 0→0 не зачтён; partial/full разделены; один предмет, TRAIN classes и OOD роли названы; constant baseline учтён |
| 5,9,14,17 | Текущие 50 stable ID, 37 future/13 saved; parent→corrected ingress первыми; маршруты и точные anchors сверены, source labels не маршрут |
| 7,10,12 | Конкретные conflict resolutions; одинаковая default модель, навыки и русский отчёт; нет переноса продуктового CI/всех запретов в маленькую экспериментальную копию |
| 8 | Старые карточки, raw, failed attempts и кандидаты не удалены |
| 13,16 | STOP/NEXT обнаружен во всех 188 MD; same-handle bounded wait и запрет relaunch включены в конкретные обновляемые prompts |

Механическая проверка подтверждает наличие правил во всех карточках, но не доказывает идеальное исполнение будущим агентом. Полный семантический аудит всех исторических 171 карточки не заявляется. Наличие несовпадения shell или домена не называется причиной уже измеренного ухудшения обучения без runtime-доказательства.

[agc-command]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/GRAD-ACCUM-COPY-01.md#L87-L91
[fbm-command]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/FROZEN-BROADCAST-MAP-01.md#L101-L105
[contract27]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/27-tensor-learning-execution-contract.md#L108-L119
[contract26]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/26-output-and-entry-contract.md#L41-L42
[registry-command]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/0ea-execution-contract.json#L1290-L1321
[wiki-index]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L82-L83
[wiki-keys]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURE-KEYS-01.md#L88-L95
[wiki-once]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURES-ONCE-01.md#L74-L80
[topics]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/05-topics-and-profiles.md
[nc-card]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md
