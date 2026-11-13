# Проверка действующих карточек: что уже исправлено и что всё ещё мешает измерить обучение

> **Исторический срез d962.** Перед публикацией поступил core `256f603e70032c11b1f4ffabbdd1fefdab642ea5`: ряд описанных здесь конфликтов уже исправлен документно, опубликован отдельный завершённый N63C-PROBE. Актуальный статус и оставшиеся пробелы — [08-latest-reconciliation.md](08-latest-reconciliation.md). Этот первичный разбор сохранён; его прежние статусы не читать как текущую очередь.

Дата: 21 октября 2026. Источники закреплены на `lubochka/llm_mvp_core@d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19` и `lubochka/xiigen-planning@fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c`. Это чтение и документные предложения. Новое обучение, inference, сборки, тесты, десериализация моделей и изменение защищённой папки не выполнялись.

## Основной вывод

Каталог уже значительно исправлен: отдельная очередь существует, модель исполнителя задана по умолчанию, есть запрет продолжать прошлую задачу после команды перейти дальше, назначены короткие пути и ожидание того же процесса. Повторять эти правила массовым новым шаблоном не требуется. Однако считать все карточки готовыми нельзя. В текущем срезе сохранён уже выполненный и завершившийся ошибкой N63C, а его карточка и машинная очередь продолжают называть его будущим. Кроме того, карточка буквально повторяет причину зафиксированного отказа сохранения RNG и запрещает достаточную подготовку ненулевого учебного состояния.

В каталоге **168 карточек**: 125 справочных семейств P01–P125 и 43 остальных документа. Эти 43 документа адресуют **47 ID**: N62 объединяет четыре ID, N61 — два. Отдельно существуют 13 файлов промптов; другие промпты встроены в карточки. Очередь документирует 9 старых попыток и 38 будущих. Новое первичное свидетельство N63C меняет именно статус попытки: **9 ранее учтённых + 1 подтверждённая сохранённая попытка = 10 issued, 37 ещё не issued в прежнем наборе 47 ID**. Это не число успешных опытов и не число освоенных навыков.

Источники: [действующий индекс](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L1-L93), [машинный контракт](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/0ea-execution-contract.json#L80-L101), [сохранённая заявка N63C](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/execution.claim).

## 1. N63C уже израсходовал попытку, но остаётся в будущей очереди

**Сейчас.** Машинный контракт, строка №12, содержит `DESIGNED_NOT_RUN`; `future_outputs` повторяет `future_only_not_executed`. Карточка в первой инструкции говорит, что host не материализован. Между тем в том же Git-срезе есть host, `execution.claim` с временем `2026-10-20T21:30:29.9676906+00:00`, результат A, stack trace и русский отчёт. Плечо A выполнило DPO и SFT; B не запускалось. Новый вызов назначенного entry откажется по существующему `results`/`execution.claim` ещё до повторного обучения.

**Почему это важно для обучения.** Очередь задаёт следующий учебный вызов и его бюджет. Ошибочный статус направляет исполнителя в заведомо занятый путь и провоцирует запрещённый поиск нового каталога либо повтор. Исправление статуса не требует запускать модель. Оно не доказывает качество, но исключает ложное назначение старой попытки.

**Поправка.** Сохранить ID и старые результаты. В additive overlay задать `ATTEMPT_CONSUMED_FAILED`, `learning_objective=OPEN`, `ab_comparison=NOT_ESTABLISHED`, `rerun_old_entry=false`; исключить N63C из списка неиспользованных выходов. Действующий приоритет `PARENT-ADOPTION-01 → N63-INGRESS-LENGTH-LEARNED-01` сохраняется. При достижении исторического N63C выполнить только документную диспозицию; назначенный следующий существующий ID — `WIKI-TAXONOMY-01`. Исправленная постановка требует собственного заранее завершённого авторского паспорта, а не выбора нового host исполнителем.

Доказательства: [карточка, строки 1–7](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L1-L7); [ложный будущий статус, строки 94–100 и 397–414](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/0ea-execution-contract.json#L94-L100); [ранний отказ при повторе, строки 14–29](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/Run-With-Learning-Signal.ps1#L14-L29); [фактический отчёт](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/report.md).

## 2. Сохранение RNG невозможно по указанному в карточке имени

**Сейчас.** Пункт 7 карточки велит вызвать `RepositoryOwnedDpoRandomStateFile.WriteCreateNew` с `after-sft.rng.json`. Точная копия bridge повторяет это в строках 425–427. Сам writer сначала вызывает `ValidateNewPath`, а тот принимает только `BuildFileName(step)`; при step=1 это `dpo_random_state_step_000001.bin`. Файл `after-sft.rng.json` не может пройти этот контракт. Stack trace подтверждает именно `CODE_DOCUMENTATION_DPO_RANDOM_STATE_UNAVAILABLE`.

**Где участвует.** `StepVector → TrainSupervisedNextTokenStep → ScoreAll(S2) → SaveCheckpoint → ExportPortableState → RNG writer`. Отказ происходит после обучения и оценок A, но до успешного завершения A и старта B. Следовательно, это часть сохранения результата обучения и завершения настоящего A/B, а не посторонняя проверка файла.

**Как исправлять.** Writer менять не нужно. В одинаковой экспериментальной обвязке обоих плеч создать отдельный owner-only каталог `R/<arm>/sft-state/`, сохранить туда snapshot с `RepositoryOwnedDpoRandomStateFile.BuildFileName(snapshot.RandomStateStep)`. В receipt явно оставить `stage=SFT` и отдельный SFT optimizer step; DPO checkpoint в `R/<arm>/checkpoints/` не трогать. Новый каталог разделяет стадии; каноническое имя сохраняет настоящий контракт. В будущем restore читать те же weights, tokenizer, LoRA topology, optimizer m/v/step и фактический RNG snapshot; простое наличие файлов или совпадение первого loss не доказывает полное продолжение состояния. Эта техническая поправка должна быть одинаковой в A и B и не смешиваться с исследуемой заменой stale SFT.Response.

Статус — **подтверждённый отказ постановки**, не измеренный выигрыш обучения и не доказательство поломки продуктового writer. Никакой повтор сейчас не назначен. [Карточка:128](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L128), [bridge:415–427](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L415-L427), [writer:70–75](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs#L70-L75), [writer:417–428](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs#L417-L428).

## 3. Поздняя ошибка стирает счёт выполненных учебных обновлений

**Сейчас.** `ArmOutcome` присваивает обоим счётчикам `error is null ? 1 : 0`. Поэтому поздняя ошибка записи RNG даёт `dpoUpdates=0` и `sftUpdates=0`, хотя raw содержит `optimizerStep=1`, completed applyOptimizer=1, SFT loss, стоимость SFT и панель S2. Отчёт уже объясняет это, но машинные поля продолжают сообщать другое.

**Почему это важно.** Неуспех всего плеча не отменяет совершённые изменения состояния и расход бюджета. Автоматический читатель, который смотрит только верхние поля JSON, сочтёт попытку необучавшейся и может назначить лишние updates. Учитывать частичный расход особенно важно после timeout/задержки вывода.

**Точная поправка в будущей экспериментальной обвязке.** `dpoUpdatesCompleted` увеличивать после успешного возврата `StepVector`; `sftUpdatesCompleted` — после возврата `TrainSupervisedNextTokenStep`. Хранить отдельно `updatesAttempted`, `updatesCompleted`, `armCompleted`, `resultPersistenceCompleted` и `errorStage`. Последующие ошибки не обнуляют предыдущие этапы. Если ошибка произошла внутри update и исход не установлен, писать `UNKNOWN`, сверять сохранённую step receipt того же запуска, не выдавать 0 и не повторять. Исторический JSON не переписывать: рядом с ним достаточно корректирующей записи, ссылающейся на первичные поля и код.

Это новая находка учёта выполненного обучения относительно 883 проверенных planning Markdown: точные строки `N63CollisionBridge`, `after-sft.rng.json`, `FAILED_PRESERVE_EVIDENCE`, `N63C_ARM_FAILED` в них не найдены. Поиск имён не доказывает отсутствие всякой общей идеи об учёте ошибок; новизна ограничена этим конкретным source/receipt противоречием. Новое обучение для подтверждения не требовалось. [bridge:336–338](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L336-L338), [bridge:395](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L395), [bridge:486–512](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L486-L512), [raw A](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json).

## 4. Запрет 0→0 всё ещё применяется после расхода, а не при проектировании

Контракт 29, строки 11–13, сознательно оставляет запуск `DESIGNED_UNQUALIFIED_LEARNING_SIGNAL` без квалифицированного начала. Четыре H512 карточки прямо сохраняют холодный B0 и конечные 2–3 updates; N63C сохраняет один DPO и один SFT на плечо при LR=1e-5 и запрещает warmup. Это не забытый warning: [журнал 21 октября](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-21-study-log.md) прямо отвергает поступивший запрос подготовить диагностичный before для H512.

Из всех карточек **16** имеют буквальный статус `DESIGNED_UNQUALIFIED_LEARNING_SIGNAL`; перечисление сохранено в `queue-inventory.json`. Одна из них — уже выполненная N63C. Разница с числом 15 в контракте — дополнительный симптом несогласованных статусов, не самостоятельная причина плохого обучения.

Нужно различать два требования пользователя. Любое измеренное улучшение времени, памяти, NLL либо другого полезного показателя сохраняет успех по этой оси; рост accuracy для него не обязателен. Но это не разрешает проектировать пустой опыт, где задача не усваивается и все сравниваемые ответы остаются нулевыми. Для retention/restore исходное умение должно существовать до вмешательства. Для cost-сравнения требуется реальная сопоставимая учебная работа и пригодная панель, а не только запуск функции с изменившимися весами.

**Авторская поправка:** для каждого из этих рецептов до назначения определить конкретную предметную панель и воспроизводимый ненулевой baseline либо фиксированную оплачиваемую подготовку до первой диагностичной способности. Заранее задать модель, corpus, checkpoint, criterion, конечный потолок и статус при его недостижении. Не поручать исполнителю поиск другого checkpoint, бесконечное обучение до PASS или самостоятельную смену seed. Если такой паспорт ещё не существует, статус `NEEDS_DIAGNOSTIC_RECIPE`, а не «готовый маленький эксперимент». Существующие независимые Unit/kNN/BPE опыты не блокируются отсутствием нейронного H512 donor.

**Доступный warm-start не следует выдумывать.** H16 `RESTORE-MINIMAL-SET/results/trained-weights.bin` после 1000 updates даёт TRAIN 3/40 response-токенов, целых ответов 0/2. H32 `N63R-PROBE/results/wa-trained.bin` даёт 45/92 токенов, целых ответов 0/2. Это частичные состояния; H16, H32 и H512 различаются формой, tokenizer/context/LoRA. Ни один из них не доказан как совместимый ненулевой полный H512 before для N63C. PA01 пока только будущий accepted-parent reader. Локальный N63C-PROBE из сообщения пользователя не найден среди каталогов remote RP01-MICRO на выбранном SHA; это не утверждение об отсутствии локального файла у пользователя. Нельзя выдавать его запуск или результат за прочитанный Git-артефакт.

Источник границ: [контракт 29:7–23 и 38–48](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/29-learned-start-and-parent-continuation.md#L7-L23), [N63C start/budget:73–79, 154–167](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L73-L79).

## Что доказывает сохранённая N63C-попытка

| Показатель | A | B | Допустимый вывод |
|---|---:|---:|---|
| Завершившиеся DPO updates | 1 | 0 | Учебная операция была, поздний отказ её не отменяет |
| Завершившиеся SFT updates | 1 | 0 | Совпавший stale SFT действительно предъявлен A |
| Side-score forwards | 18 | 0 | Половина плановой панели 36 |
| T1 correct response tokens S0/S1/S2 | 0/23 → 0/23 → 0/23 | Не измерено | Приобретение/стирание навыка не показано |
| T1 teacher-forced response exact | false → false → false | Не измерено | Целый ответ не усвоен |
| DPO margin S0→S1 | 0.2279687 → 0.23142481 | Не измерено | Наблюдение preference, не выученный ответ |
| Stale SFT margin S1→S2 | 0.23142481 → 0.23094606 | Не измерено | Не доказательство сравнительного вреда A против B |
| Сохранение SFT weights/optimizer | Файлы существуют | Нет | RNG отсутствует; полного bundle нет |
| Итог | FAILED_PRESERVE_EVIDENCE | NOT_RUN | A/B не установлен, полезный causal learning gain не установлен |

Сохранены `A/arm-result.json`, DPO checkpoint и его optimizer/RNG receipts, `A/after-sft.bin`, `A/after-sft.optimizer.bin`, `runner-failure.txt`, `execution.claim`, журналы и отчёт. `.bin` в Git могут быть LFS pointers; эта проверка не десериализовала модель и не утверждает локальную доступность payload. Полный расход inferred из подтверждённых вызовов: 2 training updates; 18 side-score forwards; внутренние DPO forward/backward отражены в raw timing. Не складывать overlapping времена. Поля верхнего уровня updates=0 ошибочны, исторически сохраняются с erratum.

## Проверка правил пользователя без повторного шаблона

| Требование | Состояние и граница |
|---|---|
| Небольшой опыт с доменом, данными и реальным learner | 125 P-карточек честно справочные; 43 документа имеют отдельные малые рецепты/исторические попытки. Это не 168 готовых runnable опытов. |
| Любое полезное улучшение — успех своей оси, ухудшения рядом | Общая политика и карточки содержат это правило; сохранять ресурсные gains, не требовать обязательного accuracy gain. |
| Функция должна участвовать в обучении/загрузке/чтении результата | N63C связан с настоящими updates и сохранением состояния. Не назначать отдельный «тест отчёта» как обучение. |
| Сначала разблокировать обучение, затем конфликты/обновления | Срочный PA01→corrected08 уже впереди. Не возвращать устаревший NEXT09 из входящего пакета. |
| Экспериментальная копия; перенос в продукт решает пользователь | Эта граница задана; изменения writer/production здесь не требуются. |
| Ясные host, модель, entry и конфликты | Основной пробел N63C — конкретно неисполняемый writer path и занятый output, а не отсутствие общего правила. |
| Старые эксперименты только сохранять | Предлагаемый overlay не меняет старые карточки/raw, не освобождает claims и не делает retry. |
| Не продолжать прошлую задачу при NEXT; STOP останавливает всё | Прямой запрет присутствует в общих блоках и compact новых карточках/промптах; не переписывать сотни карточек повторно. |
| Реестр задаёт порядок | Есть 47-ID очередь, но N63C status/consumption требует исправления. |
| Ненулевой минимальный учебный сет, достаточный бюджет | Не выполнено для N63C; 16 unqualified recipes требуют предметной квалификации. Частичный токен не выдавать за полный ответ. |
| Исполнитель умеет ждать и находит заданные tools | Существующий execution-policy запрещает повтор на timeout и требует наблюдать тот же process/session. Отсутствующий лог не доказательство остановки. |
| Русский отчёт и известные skills | Формат уже задан; избыточный повторный обход навыков/хэшей ради оформления запрещён. |

## Глубина и ограничения этой проверки

Полностью материализованы и проиндексированы все 168 карточек, 13 отдельных промптов, управляющие документы и root/probe-источники — 335 защищённых текстовых файлов. Материализация сверена по Git blob; это проверка полноты текста, не 335 смысловых ревью. Проверены поля/статусы всей очереди, наличие конкретных правил во всех карточках и промптах, затем вручную разобраны apparent marker misses. Глубокий source→raw→card разбор выполнен для N63C и для диагностичности/совместимости объявленных neural starts. Все 883 planning Markdown использованы для дедупликационного поиска конкретной находки, а не объявлены заново полностью изученными.

`skill_load_status`: найдены и целево прочитаны применимые части `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`; AGENTS/CLAUDE доступны, применены phase/scope/OP-65. Полный повторный load всех огромных файлов не заявляется. Нужные для этой проверки инструменты найдены; отсутствующих обязательных файлов среди использованных нет. Субагентов этот исследователь не запускал. Число независимых runtime проверок — 0; число новых training runs — 0.
