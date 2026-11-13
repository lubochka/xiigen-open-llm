# Разрешение конфликтов, порядок работы и недостающее покрытие

**Последняя адресная сверка: core `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`, planning `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`.** N-KU-01 уже зарегистрирован в protected `required-changes.md` внешним коммитом; мы этот реестр не меняли. LF-03 теперь содержит точную fixture и отдельный одношаговый бюджет. В текущем каталоге 135 карточек; исходная полная матрица 130 остаётся привязанной к `9622b575`. Две новые cost-гипотезы повторно проверены на дублирование в поздних документах. См. [финальное дополнение](05-final-publication-delta.md), где отдельно указаны закрытые замечания и оставшиеся ограничения. Новых runtime-измерений и product changes в этой сверке нет.

## 1. Текущая стадия

Пользователь сейчас поручил **документировать результаты и добавить успешный результат кандидатом в исправления**. Поэтому в этом проходе нет новых Train/Predict/generation запусков, production patches или полного CI. Подготовлены документы. Это граница текущей задачи, а не новый общий запрет на обучение.

Действующая политика20.10 уже снимает blanket approvals, повторное личное разрешение каждого SHA, запрет source copies и требование accuracy gain для каждого полезного результата. Она различает `PREPARABLE`, `NEEDS_INPUT`, `OPEN_LEARNING_LINK` и `OUT_OF_LEARNING_SCOPE`; `NOT_RUN` обозначает отсутствие запуска. [Текущая политика][policy]

Не нужно просить пользователя самому выбирать проект, entrypoint, конфликт или место эксперимента. Для новых механизмов эти решения уже вынесены в отдельные самодостаточные карточки. Обычная починка собственного isolated harness до измерения входит в подготовку; она не должна превращаться в исправление всего продукта.

## 2. Приоритеты в соответствии с поручением

| Приоритет | Что сделать | Что уже сделано в этом пакете |
|---|---|---|
| 1. Разблокировать связанное с обучением | Признать native memory Train, preparation→Train, save/load learned state и actual consumer; отменить ложные blanket gates | Области и learning chains обозначены. P125/ingress/P25 и native SFT/DPO успехи сохранены. Отсутствие accuracy gain не отклоняет результат |
| 2. Разрешить конфликты карточек | Указать, какое правило действует и где execution passport ещё незаполнен | Конфликты сведены ниже и в полном аудите. Новые точные карточки не наследуют старый copy ban |
| 3. Включить присланные изменения требований | Изолированные копии; одна гипотеза; любая полезная метрика; все регрессии; user decides product adoption | Правила включены в новые карточки и их executor prompts. N-KU документирован кандидатом; уже внесённые исправления не назначены повторно |
| 4. Остальное | Рассматривать только если показана зависимость от подготовки/обучения/восстановления/его результата | Чистые scorer/parser/graph/format проверки без этой связи не получают новый runtime budget |

Источники нынешнего порядка: прямое поручение пользователя и [политика20.10][policy]. Наличие старого `BLOCKED` или `OUT` у другого frozen варианта не решает автоматически судьбу нового доказанного learning-dependent пути.

## 3. Конфликты закрыты явно

| Конфликт | Решение для этого дополнения |
|---|---|
| Старый `81-validation-program` запрещает копирование исходника, новые headers требуют A/B source copies | Применять current isolated-copy contract. Продукт до измеренного результата не патчить |
| «Не запускалось» читают как запрет готовить | `NOT_RUN` — только состояние evidence. В текущем поручении исполняется документация; при поручении на запуск подготовка разрешена |
| Успех признают только при accuracy gain или ≥5% | Любое измеренное улучшение полезной метрики — успех этой метрики и домена; остальные метрики рядом. Нет нового порога или обязательного CI/significance gate |
| Только neural updates считаются обучением | KNN/Executor memory Train — обучение. Назвать recipient, state mutation и reader; счётчик вызовов сам по себе недостаточен |
| Restore без нового Train запрещают | Допустим один ранее доказанно обученный checkpoint и same queries/state. Формальное переобучение не требуется |
| Чистый компонентный finite difference выдают за full training gain | Для EMB/DPO показана связь с настоящими updates. Linear bias full-run с bias disabled не подтверждает эффект этого исправления |
| Каждый cost experiment раздувают до полного product pipeline | Измерять только достигнутую учебную работу. Generation для cost сама по себе не обязательна. В GRADB выбран существующий live8 protocol со своей фиксированной панелью, не весь pipeline |
| Историческое `passed=false` отменяет численный выигрыш | Переклассифицировать результат по метрике; например E18 `.825→.85` остаётся локальным улучшением |
| `GATE_PASSED` в одном поле означает публикацию/приобретение навыка | Проверять actual training_status, heldout definition, receipt и consumer. ProjectWiki router092 имеет `trained_not_published` |
| `N62 context` автоматически доказывает потерю POS продуктом | Без original writer→state→consumer это feature-utility hypothesis. Не выдумывать product bug и original fix address |
| Нет файла `84-check-source-coverage.md` | Зафиксировать навигационный остаток. Использовать существующие source registry/claim ledger/findings map, не запускать новый learning experiment ради ссылки |

Подробные pinned anchors каждого конфликта: [аудит, разделы2–4](experiment-audit.md). Старые защищённые документы сохранены; этот файл задаёт однозначное чтение для нового дополнения.

## 4. Проверка «все детали есть в экспериментах»

| Finding | Есть ли точное описание в protected на core pin | Что добавлено здесь | Что остаётся открытым |
|---|---|---|---|
| N-KU-01 DoStatement label | Да: карточка, source copies, raw arms/gold/report | Подтверждённый positive candidate, корректная метрика, текущий product target, k3/regression ограничения | Решение пользователя о production adoption; исходный product не менялся |
| EMB-SFT8 и DPO-MASK8 | Да: настоящие live results, validations, receipts | Положительная переклассификация с точными loss denominators и full cost tables | Нет новой runtime задачи; current fixes уже есть |
| P125 / translation ingress / P25 | Да: reports, native evidence, независимые reviews | Общая learning chain и различение приобретения, доставки урока и сохранения результата | Нельзя автоматически распространить на neural RP01 или старые schemas |
| KH-COST-01 shared AST preparation | **Нет exact двух-ParseText→same Train recipe**; general cost темы не заменяют этот механизм | [Полная карточка](knn-cost-card.md), exact source copies/host/fixtures/commands/metrics | В protected не добавлено, потому что менять его запрещено; execution ещё не было |
| GRADB-LOCALITY-ENRU-01 | **Нет exact right-adjoint factor**; prior N-ADJOINT явно исключает gradB | [Полная карточка](new-gradb-locality-card.md), native host, scalar delta, build order, scorer/compare contract | Наличие архивных C50 и runtime binaries ещё надо проверить в окружении будущего запуска; в protected не добавлено |
| KH-EXT-01 T1→sparseT2 | Общий methods/pairs gate уже известен LT-N02; **конкретный sequential retention recipe не найден** | [Точная карточка](method-memory-card.md): T1 Train и T2 admission (в B T2 отклонён), queries до/после, reject-before-Clear | Active repeated RP01 caller не доказан; в protected не добавлено |
| ProjectWiki marker transformation | Да, families P04/P108/P118 и ранее собранная история | Перепроверено по donor/current исходникам, ограничены claims старых успехов | Выбранный execution passport ещё не заполнен; этот source review не притворяется runnable experiment |
| Старые translation/OR checkpoints | Да, restore/migration families и прошлые findings | Current pinned readers/assets сверены; P25 отделён от старой схемы | Конкретный новый migration run не выбран и не запущен; admission не ослабляется |

**Таким образом, полное наличие новых деталей внутри защищённого каталога не подтверждено.** Утверждать обратное было бы недостоверно. Отсутствующее покрытие обозначено и оформлено как additive документы в разрешённом `possibleDifs`; это не скрытая правка protected и не удаление старых экспериментов.

**Поздний известный LF-03:** на `ab583032` существующая fixture и отдельный бюджет 1 update/arm, 2 всего уже внесены в N62 внешним коммитом. Старое замечание о нехватке данных/бюджета закрыто. В [late-core-update.md](late-core-update.md) остаётся полезным точное размещение будущих outputs вне protected; [финальная сверка](05-final-publication-delta.md) отделяет это ограничение от уже исправленного. Это уточнение существующей карточки, не четвёртая новая гипотеза.

## 5. Точные площадки трёх новых документов

| Карточка | Учебный получатель и предмет | Назначенное место будущего host | Материал / бюджет / результаты |
|---|---|---|---|
| KH-COST-01 | `FileActionKnnPredictor.Train`; программирование на C# | `experiments-v2/RP01-MICRO/KH-COST-01/KhCost01Runner.csproj`; copies под `35.ExperimentsV2/Experiments/Rp01Micro/KhCost01` | 4 TRAIN +4 EXAM на arm; итого2 native Train,8 TRAIN presentations,16 logical queries (8 empty-memory guards +8 native Predict). Полные Program/csproj/prepare/run в карточке; сегодня NOT_RUN |
| KH-EXT-01 | MethodKnn memory; последовательное сохранение next-method способности в C# | `experiments-v2/RP01-MICRO/KH-EXT-01/KhExt01Runner.csproj`; copies под `35.ExperimentsV2/Experiments/Rp01Micro/KhExt01` | Q0→T1→Q1→T2→Q2;4 entry attempts,10 source presentations,3 native Train,24 queries (12 empty-memory guards +12 native Predict); полный script/fixtures в карточке. Сегодня NOT_RUN |
| GRADB-LOCALITY-ENRU-01 | Native DpoTrainer/LoRA updates; EN→RU neural profile | `experiments-v2/RP01-MICRO/GRADB-LOCALITY-ENRU-01/` — точные project/runtime/host paths внутри карточки | Same8 pairs, C50, archived runtime;16 native updates/48 заданных ответов A/B; frozen scorer. Сегодня NOT_RUN / ARCHIVED_INPUTS_TO_VERIFY |

Автору будущего запуска не нужно придумывать конфликт: если workspace уже содержит эти пути, применяется описанный в соответствующей карточке owner/readback contract; неизвестный существующий результат не перезаписывается. Наличие пути в этом Markdown не означает, что проект уже создан или скомпилирован.

Для GRADB архивные binaries и C50 не находятся в доступном GitHub source. Их недоступность — **конкретный технический input gap**, не новый персональный approval gate. Нельзя молча заменить другой базой, иным tokenizer или фиктивным native Step. Если exact input отсутствует, документировать его отсутствие и завершить независящую подготовку; не выдавать новый baseline за старый.

## 6. Общий промпт для следующего документационного прохода

> Продолжай документировать learning candidates по этому пакету. Приоритет: связь с обучением и снятие ложных блокировок, затем конфликты карточек, затем пользовательские изменения, затем остальные связанные проверки. Защищённую docs/plans/replanning-possible-diffs не меняй; старые эксперименты не удаляй. Любое измеренное улучшение полезного показателя — успех этой метрики и предметного домена, рост accuracy не требуется; все ухудшения и неизмеренные поля покажи рядом. Не запускай новые эксперименты и не внедряй product patches в документационном поручении. Не придумывай результат. Для неподтверждённой новой гипотезы заранее задай original owner, isolated copy, project, entrypoint, данные, состояние, команды, budget и конфликт. Если цепь не затрагивает подготовку/обучение/восстановление/потребление learned result, она вне этой очереди. P-ID и README-ссылка не считаются готовым execution passport. Кандидатное изменение продукта остаётся отдельным решением пользователя.

Три экспериментальные карточки содержат собственные, более конкретные prompts. Этот общий текст не заменяет их exact paths и не поручает искать новые площадки.

[policy]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md#L3-L40
