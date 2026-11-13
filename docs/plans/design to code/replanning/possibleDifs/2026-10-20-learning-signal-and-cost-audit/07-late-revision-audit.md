# Проверка позднего обновления core: bf9b543e → f5dd5320

## Что действительно изменилось

Во время работы ветка core продвинулась на три коммита: `bf9b543e149ef197da85a767821d98c9b67916d2` → `f5dd532045716495a2c7668573250e5d7cc46810`. Сопоставление GitHub показывает **152 изменённых Markdown исключительно внутри docs/plans/replanning-possible-diffs/experimets/**, включая новый policy-файл. Файлы программного кода в этом диапазоне не менялись. Это внешнее обновление; наш аудит не записывал ничего в core. Нельзя утверждать, что защищённое дерево вообще не менялось в течение сессии. [Полное сравнение](https://github.com/lubochka/llm_mvp_core/compare/bf9b543e149ef197da85a767821d98c9b67916d2...f5dd532045716495a2c7668573250e5d7cc46810).

Все 152 поздних файла скачаны отдельно от исходного снимка. По ним выполнены адресный diff, повторная структурная проверка карточек и поиск новых allocation-механизмов. Подробно перечитаны новая политика, README/методика, нейронный раздел, изменённая карта покрытия и расширенная карточка 23. Старый подробный crosswalk сохранён с явным указанием ревизии; он не выдан за 179 заново прочитанных поздних recipes. [Реестр изменённых файлов](inventory/late-delta.json).

## Исправления, которые уже есть и не должны снова попадать в backlog как нерешённые

| Область | Состояние f5dd | Следствие для этого аудита |
|---|---|---|
| Общая блокировка карточек и повторное личное подтверждение точного SHA | Отменены новой policy | Доступные поля и micro caller — самостоятельная подготовка, а не обязательный вопрос пользователю |
| Полезный результат без роста accuracy | Явно признан успехом по метрике/домену | Показывать одновременно потери других осей; не восстанавливать старые Tier/accuracy gates |
| Восстановление и использование trained state с 0 новыми updates | Разрешены при доказанной зависимости и точном code-to-fix | Старые blanket OUT по serving/cold/query пересматриваются адресно |
| Resource-опыт настоящего update без Generate | Разрешён | Качество NOT_MEASURED, затраты самого обучения измеряются |
| N-KU-01: четыре TRAIN/k3 | Постоянное большинство и потеря recall уже прямо указаны | LF-01 перестала быть новой находкой относительно f5dd; шестистрочный вариант полезен для другого, структурного вопроса |
| N60-E1 и N60-E2 | Добавлены маленькие доменные рецепты и source crosswalk | Нужно проверять именно их достигнутые writers/readers, а не объявлять отсутствие всей темы |

Источник правил — [00-execution-policy-2026-10-20.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md); источник N-KU/E1/E2 — [23-additions-n60-small-learning.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md).

## N60-E1: карточка честно оставляет reachability открытой; теперь она проверена по исходникам

Текущая E1 уже предупреждает: если source marker пуст, её пример не проверяет заявленную связь, и нельзя одновременно чинить N57. Мы установили эту конкретную недостижимость для `быстро`/`тихо` и исключили альтернативную concept/signature запись. Это устраняет техническую неопределённость, а не создаёт новый общий запрет. [LF-04](01-new-findings-and-small-experiments.md) задаёт симметричный материал с `нежно`/`горестно`, существующий ADV путь, точный original admission patch, 2 batch / 11 Unit calls / 16 Verify и полный prompt.

Исходная панель имеет реальное обучение соседних Units, но не intended Verifier rule write. Поэтому нулевое различие её Verify нельзя интерпретировать как опровержение полезности admission-ремонта. Нулевой эффект **достигнутого** нужного writer остаётся полноценным результатом; две ситуации различаются.

## N60-E2: это проект представления, а не готовый опыт

E2 предлагает два урока `delete the file`→`удали файл`, `save the cache`→`сохрани кэш` и новую запись source-support при Executor.TrainOnExample с соответствующим чтением обычным переводчиком. В самой карточке явно написано `OPEN_STATE_AND_READER`: точные schema, extractor и reader метода не установлены. Поэтому E2 присутствует в покрытии **как подготовительный проект**, но ещё не как исполнимый A/B. [E2, строки 220–230](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L220-L230).

Следующее конкретное действие: определить одну версию source-support record, её actual writer в [Executor.TrainOnExample](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455), источник признаков только из TRAIN и обычный reader, который использует эту запись. Бюджет следует из выбранного пути; готовая команда и число native calls сейчас не выдумываются. Дописывание gold отрицания/вопроса в TRAIN или общий serving guard поменяли бы вопрос. Два утвердительных урока не обучают полному смыслу отрицания; допустимый ограниченный вывод — область поддержки и отказ от неподтверждённой композиции. Safe refusal и правильный перевод считать отдельно.

## Что ещё не покрыто защищёнными карточками

| Наш результат | Проверка покрытия f5dd | Действие |
|---|---|---|
| LF-02: expVals allocation внутри каждого target backward | Точный механизм не найден ни в исходных 156, ни в 152 поздних документах; generic PAD/numerical темы не тот же фактор | Карточка и prompt подготовлены в 01; protected не менялся |
| LF-03: двойное копирование initialGradient через TensorStorage | Точный механизм не найден; generic autograd reachability не закрывает copy cost | Отдельный actual-update A/B в 01, без смешения с LF-02 |
| LF-04: E1 fixture не достигает marker writer | Сама E1 есть и предупреждает о риске; конкретная source-цепочка и исправленные слова отсутствовали | Исправление постановки в 01, не новый повтор E1 |
| LF-01: константа 4/k3 | Уже явно описана | Сохранить KNOWN_IN_LATE_REVISION; optional структурная постановка отдельно |

Следовательно, фраза «все новые детали уже корректно описаны в защищённых экспериментах» пока неверна. Нарушать запрет пользователя на редактирование этого каталога ради формальной полноты нельзя. Этот пакет содержит адресную карту недостающих дополнений и готовые новые карточки в разрешённом planning-пути.

## Текущие статусы без нового разрешительного процесса

READY, PREPARABLE, NEEDS_INPUT, OPEN_LEARNING_LINK и OUT_OF_LEARNING_SCOPE имеют смысл из policy. NOT_RUN описывает отсутствие запуска. PREPARABLE_EXPERIMENT_HOST в LF-02/03 означает конкретную инженерную задачу подготовки точной копии/host; отсутствие нового личного approval не blocker. Источник проблемы и измеряемое изменение должны быть реальными, но неоформленные доступные поля заполняет исполнитель. Этот аудит завершён документацией и публикацией в requested possibleDifs; продуктовый код и результаты обучения не менялись.

## Последний readback: core62bcc и новые planning-пакеты

После основного late review появился core `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`: ещё два коммита, 145 изменённых Markdown внутри experimets, без изменения runtime source. [Сравнение](https://github.com/lubochka/llm_mvp_core/compare/f5dd532045716495a2c7668573250e5d7cc46810...62bcc8f2368840e22d2ea98ef16d9bd526ad9a39). Полный список путей сохранён в inventory/final-core-delta.json; содержательно перечитаны новая N-KU карточка и изменённые участки 23. Это targeted delta review, не заявление о повторном чтении всех 145 тел.

**N-KU-01 уже имеет подготовленный проект runner.** Новая [cards/N-KU-01-do-loop-label.md](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md) сохраняет 4 TRAIN/k3/30features, 2 Train/8 source presentations/16 запросов изолированного адаптера (Facade.Compile=0). Она включает source-copy preparation, provenance, build и два fresh процесса как будущий рецепт; NOT_RUN означает, что сборка/запуск не выполнены. Константа большинства и пропуск KQ4 признаны прямо. Поэтому «без наших шести строк запуск недопустим» было бы ошибкой: исходный опыт пригоден для узкого label→memory→vote эффекта. Наш шестистрочный вариант нужен только при отдельной цели структурного различения.

В карточке 23 материал E1 с быстро/тихо и открытый E2 не изменились. LF-04 по-прежнему относится к действующим точным E1 строкам; аналитическая проверка повторно подтверждена вторым source review. Новые общие инструкции подготовки не являются новым approval gate. Они задают точную experimental copy и отдельный host **вместо обязательного изменения production dispatch**. Это внесено в 03 и каждый новый LF-prompt. Сама публикация этого документного аудита в requested planning-путь не является переносом experimental кода в продукт. Указание внешней policy писать experimental results под protected не применяется к этому поручению: прямой запрет пользователя менять этот каталог сохраняется. [Полный финальный delta review](reviews/final-core-delta-review.md).

Planning одновременно продвинулся до `fb9aadf69e0669d36b820d61f76fe3ecc8697a7d`. Появившийся каталог 61 уже принадлежит другому аудиту, поэтому этот пакет опубликован под самостоятельным именем `2026-10-20-learning-signal-and-cost-audit`. Все внешние материалы сохранены. В 24 изменённых Markdown выполнены повторный novelty search и адресное чтение конкретных дополнений; никакие их результаты не присвоены нашему проходу.

| Параллельное дополнение | Что оно уже подготовило | Как учитывается здесь |
|---|---|---|
| [Пакет 61, Mode1](https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-FIRST-AUDIT-2026-10-20/02-HISTORY-AND-KNN-HYPOTHESES.md) | Overwrite per-Flow features последующим уроком; глобальный поиск learned pair может дать текст другого выбранного Flow | Чужие новые гипотезы, не LF-02/LF-03. Получатели PromptFlowGroundingModel/Composer; domain и бюджет отдельные |
| [LC-REF-CACHE](https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/dpo/01-reference-score-reuse.md) | Reuse frozen reference scores при повторных actual updates на тех же парах | Другой DPO cost-фактор. Наши LF-02/LF-03 не кэшируют reference и используют один update/arm |
| [KN-ROUTER](https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/routing/KN-ROUTER-LEARNED-KEY-CANONICALIZATION-2026-10-20.md) | Несогласованная нормализация C#/.NET между writer/readers learned keyword profiles | Другой learned-output фактор; не объявляется нашим открытием |
| [N60 concrete overlays](https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/coverage/02-N60-CONCRETE-OVERLAYS.md) | E2 SourceSpanFrames writer/state/reader и конкретные 2TRAIN/4EXAM; отдельный более широкий E1 ordinary-retention вариант | Снять утверждение, что конкретного E2 проекта вообще нигде нет. Protected E2 всё ещё открыт, но доступный planning overlay уже является PREPARABLE проектом; не создавать ещё один. Малый RU→EN Verify LF-04 не заменяется большим warmup/retention опытом |

E2 overlay задаёт source-derived span lengths/gaps и читает их в TryComposeFromPhraseMappings, с явным checkpoint extension. Его автор сам ограничивает вывод: на выбранных четырёх входах эффект может совпасть с известным full-coverage guard P54; уникальное преимущество representation не доказано. Мы прочитали проект как актуальное дополнение покрытия, **не выполнили независимую runtime-валидацию его полной новой реализации**. Ссылка на него — достаточный следующий шаг вместо дублирования нового документа.

На planning fb9aadf6 точный E1 быстро/тихо путь, expVals и двойная initialGradient copy в этих двух пакетах не обнаружены. Этот результат поиска относится только к указанному срезу; следующая дельта ниже отменяет отдельную новизну LF-02.

## Дополнение перед записью: planning64aa3c75 и совпадение LF-02

Ветка продвинулась ещё на два commits: [fb9aadf6→64aa3c75](https://github.com/lubochka/xiigen-planning/compare/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d...64aa3c75e378ddf32af517ab139f17dea1f0ed15). Проверены metadata61 изменённого пути, получены все26 Markdown и повторён адресный поиск; точные рецепты прочитаны содержательно. [Receipt и поисковые совпадения](inventory/parallel-planning-delta.json) дополняют предыдущий срез, не подменяют его. Чужие файлы и навигация сохранены.

| Новая параллельная публикация | Сопоставление с нашим пакетом |
|---|---|
| [OCT20-R2: local expVals](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-update-effect-audit/06-new-R2-softmax-buffer.md) | Точное совпадение LF-02: тот же allocation move после cnt<=0, тот же full overwrite и порядок арифметики. Однопарный LF-02 — вариант этой семьи, не отдельное открытие и не обязательный повтор R2 |
| [OCT20-R1: frozen-base comparison](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-update-effect-audit/05-new-R1-frozen-base-comparison.md) | Другой source-confirmed cost-фактор, полное сравнение frozen base; не наш LF-03 |
| [Пакет62: optimizer step read](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/62-LEARNING-METRICS-AND-REFERENCE-COST-AUDIT-2026-10-20/OPTIMIZER-STEP-READ-COST-2026-10-20.md) | Export portable AdamW m/v только ради Step — отдельная гипотеза другого автора. Tensor initialGradient copy ею не покрыта |
| [Пакет62: positive-feedback verifier candidate](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/62-LEARNING-METRICS-AND-REFERENCE-COST-AUDIT-2026-10-20/CANDIDATE-NOT-PROMOTED-VERIFIER-POSITIVE-FEEDBACK.md) | chosen.Pass=true не пишет состояние, действительный positive producer не установлен. У LF-04 builder установлен, chosen.Pass=false, а конкретный marker не извлекается. Общий тезис «Train count не доказывает state write» не объявляется новым |

Самостоятельная новизна **LF-02 снята**. LF-03 и точный LF-04 join остаются дополнительными к проверенному корпусу; это ограниченный вывод по закреплённым refs, не гарантия приоритета относительно любой неопубликованной работы. Независимый reviewer подтвердил точное совпадение LF-02 и различие двух verifier-сценариев. Все улучшения новых карточек остаются NOT_RUN/NOT_MEASURED.

## Последняя ограниченная сверка core7c5718

От62bcc до7c57183670ccaf554a47c0a4f44cabadde8c303d появились5 commits и148 изменённых Markdown:145 modified и3 added, все внутри protected experimets. Runtime source не изменялся. Полный compare, шесть проверенных source snapshots и точные ограничения находятся в [receipt](inventory/final-core-delta.json) и [независимом review](reviews/final-core-delta-review.md). Это адресная проверка дельты; повторный полный аудит125 карточек не заявляется.

- [P01:3–25](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md#L3-L25) разрешает простой host с throw/try-catch/void/DTO, без обязательного продуктового OperationResult. Существующие эксперименты, результаты и bin/obj сохраняются; новая версия Markdown не назначает перенос/повтор начатого опыта.
- [N-KU:73](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L71-L73) сохраняет прежние VariantA/VariantB/runner для уже начатого паспорта. Новая карточка относится к ещё не начатому варианту. [Preregistration](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/preregistration.md) не является результатом обучения; её роли и дополнительные операции не переопределяют наши LF-бюджеты.
- Gold первых3 задач новой N-KU теперь NO_EXTRA_LOOP с непустым actual!=ADD_LOOP, четвёртой ADD_LOOP. На фиксированных двух TRAIN-классах аналитика4/k3 не меняется; это не замер и не прогноз optional6-row. Series23 ссылается на dedicated card вместо дублирования паспорта.
- [N61 reference reuse](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md) и [N61 Mode1 retention/identity](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md) теперь явно присутствуют в protected. Эти находки других авторов больше нельзя описывать как существующие только в planning. Они не закрывают expVals-loop allocation или Tensor initialGradient copy.
- [E1/E2](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L187-L229) побайтно сохранили проверенные тела62bcc. Материал быстро/тихо и открытый E2 writer/read контракт этой дельтой не исправлены; LF-04 остаётся адресным дополнением.

Этот pin завершает bounded core join. Будущие документные commits не назначают бесконечное перечитывание, остановку или повтор зафиксированного опыта; реальное изменение используемых source/input разбирается отдельно. Запрет пользователя менять защищённую папку нами соблюдён.
