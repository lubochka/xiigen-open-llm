# Проверка готовых авторских карточек и промптов

21 октября 2026. Проверяющий: отдельный агент с моделью сессии по умолчанию, без override. `READ_ONLY_RESEARCH`: проверены тексты, source-связи, бюджет и порядок действий; build/test/train/inference не выполнялись. Карточки автора и защищённая папка core не изменялись.

## Объём и вердикт

Полностью прочитаны восемь авторских Markdown: [README](../README.md), [01](../01-learning-paths.md), [05](../05-results-and-candidates.md), [06](../06-novelty.md), две [карточки](../cards/POS-QUERY-SET-ONCE-01.md) и два [промпта](../prompts/POS-QUERY-SET-ONCE-01.md). Разделы04/07 и новые JSON в момент проверки ещё формировались и в этот вердикт не входят. Предыдущий source/novelty review — [independent-review.md](independent-review.md).

**Содержательный дизайн принят с адресными уточнениями lifecycle POS ниже.** Новая причинная польза не приписана статическому чтению. SiLU честно заблокирован без команды исполнения. POS имеет конечный native Train→Predict контраст, однако формулировки о владельце подготовки и arm claim нужно согласовать до выдачи карточки исполнителю. Это review документов, не подтверждение работоспособной сборки либо успешного эксперимента.

## Что согласовано

| Проверка | Результат |
|---|---|
| Учебный механизм POS | Native `GenerateExamples → Train128 → _train → Predict → class`; измерение чтения изменённой обучением памяти. Не подмена private distance benchmark и не заявление об ускорении Train |
| Бюджет POS | A0/B0/A1/B1 по32Predict =128; A/B по1Train128 =2Train и256 предъявлений строк. Neural updates0. Hard subset рассчитывается из сохранённых predictions; полный `E25.Run` прямо запрещён |
| Один фактор POS | Query-set после empty-memory fallback строится один раз; train-set preparation, set-Jaccard, union0, k3, ties, vote и labels неизменны. Singletons/multiset, persistent cache, wiring проекта29 не включены |
| Пути и сборка POS | Заданы короткие PQ01/attempt-0001, две native copies, общий Program, net9.0/BCL, explicit Compile и отключение внешних Directory.Build imports, один внешний Python entry, две build-команды и конечные stage timeouts. Host пока не материализован, что явно указано |
| Измерения POS | До/после каждого плеча; allocations, wall/CPU/GC/peak отдельно для Train и trained readout; serialization за пределами панели. Нет лишнего прогрева/GC.Collect, нет дополнительных parity-классификаций. Fixed-order wall не назван устойчивым speedup |
| SiLU | Реальный update path указан; сохраняется denominator и исходный порядок арифметики. Дополнительный массив и scoring tradeoff от `RequiresGrad≠training:true` открыто описаны. Не создаётся операторный опыт вместо обучения |
| SiLU baseline | H16 partial3/40 и0/2 полных ответов не выдан за готовый skill; исторические1000/200 не превращены в новый бюджет. Нет runtime entry; автор должен сначала задать точный пригодный before и конечный учебный рецепт |
| Условия во всех четырёх card/prompt | Повторены default model без override, русский report, конкретные названия/корень skills, успех по любой полезной измеренной оси с ухудшениями рядом; отдельный немедленный NEXT и STOP; ожидание того же session/PID; запрет retry по отсутствующему выводу |
| Новизна и причинность | Source-specific SiLU/POS вмешательства отличены от SFT-EXP/RMS/N50/P43. N63C filename атрибутирован уточнению пользователя; fresh counters — конкретный код/receipt. Нет заявления о мировой новизне или установленной причине failed RP01 |
| Старые результаты | Полезные resource gains сохранены вместе с regressions и LFS/custody ограничениями;0→0 не объявлен приобретённым ответом. Изменение protected карточек не предлагается как уже совершённое |

Все восемь названных в новых инструкциях skill-файлов существуют в прочитанном core-каталоге. Это проверка доступности ссылок; не заявление о полном новом прочтении всех этих skills проверяющим.

## Замечания автору

### R1. Разделить root preparation claim и arm training claim

**Место:** [POS prompt](../prompts/POS-QUERY-SET-ONCE-01.md), строка7: «child claim до любого classifier call»; [POS card](../cards/POS-QUERY-SET-ONCE-01.md), строки33/51/59: `prepare-fixture` через A вызывает статический `PatternClassifier.GenerateExamples` до `runA`.

Буквальное «любой classifier call» включает генератор. Исполнитель может занять `R/A/training.claim` в prepare, после чего нормальный runA откажется как повтор. Поправка должна прямо назначить: общий `H/execution.claim` создаётся до подготовки; `prepare-fixture` выполняет ровно1GenerateExamples и0Train/0Predict, **не создаёт arm claim**; только режим `run` создаёт собственный arm claim перед constructor/32before/Train/32after. Нельзя ослаблять защиту от повторного run.

### R2. Назначить одного владельца source copies и fixture

**Место:** POS card, строки49 и59–66. Перечень «создать ... Native/PatternClassifier.cs ... fixture.json» допускает их материализацию перед entry; следующая инструкция снова поручает source copy и fixture generation wrapper после claim. При CreateNew это может вызвать предсказуемую коллизию.

Готовое уточнение: до внешнего entry материализуются только `run.py`, общий `Program.cs` и два `.csproj`. После exclusive root claim wrapper читает pinned source, проверяет blob, создаёт A-copy, B-copy с ровно назначенным patch и builds; общий prepare единожды создаёт fixture. Иной однозначный вариант ownership допустим, но два конкурирующих создателя этих файлов — нет. Runtime source copy не должна случайно затереть B patch исходником A.

### R3. Убрать двусмысленное «0Generate»

**Место:** POS card, строка41. Native `GenerateExamples` уже один раз разрешён строкой33. В суммарном бюджете написать отдельно: **1GenerateExamples в common prepare;2Train;128Predict;0нейронного Generate**. Это уточнение существующего бюджета, не добавление вызова.

### R4. Две редакционные границы

В [01](../01-learning-paths.md), строка26, формулировка «нельзя считать Train несуществовавшим» двусмысленна рядом с E26 default dictionary без acquisition. Точнее: «нельзя объявлять предзаполненный словарь результатом отсутствовавшего Train».

В README/реестре16 `DESIGNED_UNQUALIFIED_LEARNING_SIGNAL` следует оставлять **инвентарём явных заголовков**, не исчерпывающим списком рисков и не утверждением, что все16 — холодные starts. У corrected08 назначен partial H16before; авторская отметка `PARTIAL_LEARNING_RECIPE / FULL_ANSWER_OBJECTIVE_OPEN` верно сохраняет границу. PA01 — технический выбор/доставка parent, его готовность не равна учебному успеху.

## Условие закрытия этого review

После текстового согласования R1–R3 достаточно одного адресного readback изменённых card/prompt: заново обходить все источники, запускать эксперимент или чинить тесты ради review не требуется. R4 устраняется документно. Неизменённый статус новых факторов — NOT_RUN; SiLU остаётся blocked до явно заданного подходящего before, POS остаётся будущей реализацией, пока host не материализован отдельным поручением.

## Адресное закрытие замечаний и сверка реестра

21 октября 2026, после авторских поправок. Выполнен один адресный readback POS card/prompt, исправленных мест README/01 и новых [04-registry.md](../04-registry.md)/[registry-47-overlay.json](../data/registry-47-overlay.json). Повторного аудита корпуса или runtime не было. Первоначальные замечания выше сохранены как история review; их текущий статус указан ниже.

| Замечание | Статус | Прочитанное исправление |
|---|---|---|
| R1: prepare мог занять arm claim | **CLOSED** | POS card:51/59 и prompt:7 прямо отделяют общий root claim от arm claim. Prepare выполняет1GenerateExamples/0Train/0Predict, arm claim не создаёт. Только run создаёт claim перед constructor и всеми before/Train/after вызовами |
| R2: два возможных создателя native copies/fixture | **CLOSED** | Card:49/59 и prompt:7 разрешают до entry только script/Program/2csproj. Wrapper после root claim один раз создаёт A-copy и B-copy с patch, затем fixture; запрет копировать A поверх B после patch указан явно |
| R3:0Generate против1GenerateExamples | **CLOSED** | Card:41 и prompt:11 согласованно перечисляют1GenerateExamples,2Train,256 предъявлений/128 общих строк,128Predict,0нейронного Generate. Hard-subset читается из уже сохранённых ответов, дополнительных вызовов не добавлено |
| R4: E26 и границы16headers/partial08 | **CLOSED** | В01:26 предзаполненный словарь не выдаётся за результат отсутствовавшего Train. README:12 и04 явно ограничивают16headers инвентарём. Corrected08 — PARTIAL_LEARNING_RECIPE/FULL_ANSWER_OBJECTIVE_OPEN; PA01 — технический выбор parent, не учебный успех |

JSON разобран как данные. В `rows` ровно47 уникальных ID и47 уникальных исходных позиций: consumed10, unissued37; explicit unqualified16. В `new_ids` отдельно2 гипотезы; они не входят в10/37. N63C отмечен consumed_failed с A:DPO1+SFT1/18scores, B:not-run; повтор исходного entry запрещён. Исходный `source_next` corrected08 сохранён исторически, а `author_overlay_next_existing_id` задаёт WIKI-TAXONOMY-01, пропуская consumed N63C. Это различение источника и предложения корректно, защищённый registry не назван уже изменённым.

Приоритеты04 согласованы: PA01 → квалификация corrected08/конфликтов → прежние13–45 в исходном относительном порядке → условная SiLU после пригодного before → прежние46–47 → POS последним. Колонка «Следующий исходный ID» намеренно относится к прежним47; её END не выдаётся за конец расширенного runtime-маршрута. Заключительный абзац04 явно разделяет отдельное документное SiLU→POS→END и будущую общую runtime-программу, для которой нужен один заранее согласованный NEXT-route после закрытия SiLU-предпосылки. Поэтому прежняя формулировка NEXT в SiLU card/prompt не создаёт действующего разрешения перескочить46–47.

**Итоговый вердикт:** R1–R4 закрыты. В проверенных изменениях не осталось противоречий ownership/claim/budget/count/приоритета. Пакет принят как документация и авторский overlay; runnable-готовность host не аттестована. SiLU сохраняет DESIGN_BLOCKED_LEARNED_BASELINE без entry, POS — DESIGNED_NOT_RUN/HOST_NOT_MATERIALIZED. Дальнейшие runtime-действия ради закрытия этого review не нужны.
