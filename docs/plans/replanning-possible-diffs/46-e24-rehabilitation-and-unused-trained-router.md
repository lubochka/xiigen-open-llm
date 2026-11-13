# E24: почему старый отрицательный результат нельзя переносить на новый маршрут

16 октября2026. READ_ONLY_RESEARCH. Прочитаны исторические документы, девять сохранённых результатов и адресные source/history. Опыты, сборки, тесты и обучение не запускались. Это продолжение поиска случая, который сначала сочли неудачным, а затем применили иначе. **E24 действительно был пересмотрен, но он пока не установлен как тот самый успешный перенос механизма из матрицы в kNN, о котором говорит Люба.**

## Найденное различие

В библиотеке есть прямое решение A50: прежний запрет A46 снимается, потому что вместо одновременного распределения фрагментов по наборам данных выбран один домен на сегмент. Это содержательная смена архитектурного контракта. Она не означает, что прежний отрицательный опыт стал положительным задним числом.

Дополнительно найдено расхождение нынешнего измерительного кода: E24 обучает `PredicateDatasetRouter`, но затем оценивает **другой** `AttributedSegmentRouter`, которому обученный объект не передаётся. Поэтому текущий E24 нельзя использовать как измерение пользы этих обучающих примеров. Это касается конкретного E24 runner, а не всей системы и не доказанной причины неудачи RP01.

## 1. Что действительно было измерено раньше

Прочитаны все9 JSON в [каталоге E24](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E24-ATTRIBUTED-ROUTER/results). Первый — [30 апреля001](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E24-ATTRIBUTED-ROUTER/results/run-2026-04-30-001.json), последний — [1 мая001](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E24-ATTRIBUTED-ROUTER/results/run-2026-05-01-001.json).

| Поле | Во всех9 сохранённых записях |
|---|---:|
| Основная метрика | avg_noise_ratio; меньше лучше |
| Baseline | 0.4121 |
| Enhanced | 0.5626 |
| Изменение | −36.51% |
| Дополнительный показатель | 11/16 правильно атрибутированных пар,68.75% |
| Число фиксированных случаев | 5 |
| routerExamples в notes | 17 |
| Passed / ShouldScaleUp | false / false |

Это9 сохранённых повторов, а не9 независимых семейств или9 современных проверок. Enhanced time меняется примерно3.1–6.2мс; основная оценка одинакова. Здесь не измеряются свободный ответ Student, LoRA или перенос навыка. Источник бинарника и полная среда каждого архивного запуска не восстановлены.

## 2. Что именно пересмотрели в A50

Первичные документные свидетельства:

- [SESSION-STATE v16, A46/A49/A50,275–308](<C:/Users/lubao/Documents/xiigen/llm mvp/session-state-extracted/SESSION-STATE-2026-05-06-v16.md:275>): прежний запрет заменён per-segment-per-domain; предыдущий контекст должен обогащать существующий classifier, без второго самостоятельного классификатора.
- [PROJECT-HISTORY v7,231–240](<C:/Users/lubao/Documents/xiigen/llm mvp/session-state-extracted/PROJECT-HISTORY-v7.md:231>): описана та же реабилитация.
- [WHAT-TO-DO v13,354–357](<C:/Users/lubao/Documents/xiigen/llm mvp/WHAT-TO-DO-WHAT-NOT-TO-DO-v13.md:354>) запрещает перенос прежнего решения «unsafe» на новую конструкцию. В текущем исследовании это историческое решение, не инструкция запуска.

Старый/current compatibility [AttributedRouter,37–59](C:/Projects/llm_mvp_core/35.ExperimentsV2/Compatibility/AttributedRouter.cs:37) использует bool-vector от обученного dataset router: один сегмент может добавляться в несколько dataset lists, затем тексты объединяются по dataset. Нынешний [AttributedSegmentRouter,82–100](C:/Projects/llm_mvp_core/23.SemanticCompilerAdapter/AttributedSegmentRouter.cs:82) возвращает один Domain на каждый сегмент и сохраняет его текст отдельно. Это действительно разные операции.

Но фразу исторического документа «wrong slice structurally impossible» нельзя считать универсальным доказательством: один домен на сегмент исключает конкретное многоканальное смешение, **не исключая ошибочную границу сегмента, неверный домен, потерю связи между сегментами или пропущенный смысл**. Положительного matched quality receipt именно A50 в прочитанных девяти E24 результатах нет; все они предшествуют документному пересмотру. Это открытый поиск, не утверждение отсутствия результата во всех архивах.

## 3. Как изменился сам E24 runner

В read-only history найден commit `c8dde8d038ef403dba076adbe7ede0004325956b` от7 мая: `Implement compiled prompt KNN integration and SIMD matmul`. Его diff E24 заменяет `new AttributedRouter(router)` на `DevelopmentPromptCompiler` и `new SemanticCompilerAdapter.AttributedSegmentRouter()`. Вызов `router.Train(...)` остаётся.

Текущий [E24AttributedRouter.cs,28–48](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E24AttributedRouter.cs:28):

1. Из C# файлов извлекаются предикаты и AutoLabel dataset-target.
2. На80% examples обучается `PredicateDatasetRouter(k:3)`.
3. Отдельно создаётся новый segment router без этого объекта.
4. [76–80](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E24AttributedRouter.cs:76) оценивает compiled prompt через новый router.

Его default constructor создаёт новый `DomainClassifierUnit`: [34–37](C:/Projects/llm_mvp_core/23.SemanticCompilerAdapter/AttributedSegmentRouter.cs:34). У того `_trained` включается только через [Train,64–68](C:/Projects/llm_mvp_core/23.SemanticCompilerAdapter/DomainClassifierUnit.cs:64); обученный kNN используется при `_trained && Predicates.Length>0`: [158–181](C:/Projects/llm_mvp_core/23.SemanticCompilerAdapter/DomainClassifierUnit.cs:158). В этом E24 вызова такого Train нет. Остальные keyword/predicate-affinity пути существуют и способны давать полезные решения, но они не используют обученный в строке45 dataset router.

Кроме того, [BuildSegmentInputs,192–202](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E24AttributedRouter.cs:192) передаёт Domain, уже выбранный компилятором. При известном ExistingDomain новый router [89–98](C:/Projects/llm_mvp_core/23.SemanticCompilerAdapter/AttributedSegmentRouter.cs:89) оставляет именно его. Поэтому даже отдельное обучение injected classifier не обязательно изменит итог этой ветки. Такое предпочтение existing domain прямо предусмотрено [исходником теста,230–240](C:/Projects/llm_mvp_core/Tests/PromptRoutingInputCompilerUnitTests.cs:230); тест здесь только прочитан.

**Вывод:** наличие17 примеров в notes, вызова Train и слова KNN в commit message не доказывает участие обученного состояния в измеренном ответе. Исторический FAIL и нынешний код должны храниться с раздельной lineage. Нельзя объявлять, что повторный современный E24 воспроизводит старую обученную систему без проверки этой связи.

## 4. Ограничения метрики и ещё одна граница identity

Baseline [57–66](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E24AttributedRouter.cs:57) — вычисление отношения длин ожидаемого релевантного текста к полному prompt, без вызова обученного router. Enhanced [82–101](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E24AttributedRouter.cs:82) использует `max(0,1−expectedLength/actualLength)` и проверку присутствия первого слова ожидаемого текста.

Это не coverage смысла: очень короткий ошибочный фрагмент может получить noise0, а присутствие первого слова не доказывает правильность всего сегмента. [ForDataset,205–227](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E24AttributedRouter.cs:205) выбирает только первый подходящий segment; Generic всегда получает первый. Поэтому потерянный второй релевантный сегмент не становится автоматически видимым. Показатель полезен лишь с собственной узкой интерпретацией, а не как accuracy обучения ответа.

У `AttributedSegmentDocument` есть ещё условное расхождение: если ExistingDomain заменил classified.Label, поля Confidence/Evidence всё равно взяты от отвергнутой классификации. Это следует из [89–98](C:/Projects/llm_mvp_core/23.SemanticCompilerAdapter/AttributedSegmentRouter.cs:89). Найденный caller [PromptRoutingInputCompilerUnit,145–150](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs:145) печатает итоговый Domain рядом с этими confidence/evidence. Возможна запись «доменA, обоснованиеB». Вред downstream решению не измерен; намеренное право ExistingDomain сохранено, проблема здесь в согласованности происхождения полей.

## 5. Реальный consumer и контрдоказательство «всё отключено»

`PromptRoutingInputCompilerUnit` действительно [вызывает segment router,121–155](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs:121) при нескольких compiled segments и переносит результат в SubPhaseRouterSourceDocument. [ResolveAttributedSegmentRouter,160–169](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs:160) умеет брать learned Unit из registry, если flags разрешают и подходящая запись есть. При отсутствии этих условий берётся default wrapper.

Это положительная существующая возможность learned replacement, не доказательство, что конкретный сохранённый RP01 run её использовал. Default wrapper, registry learned Unit и E24 experiment runner — три разных consumer configurations. Локальный/remote snapshot каждого прочитанного файла не переносится на старые бинарники. Standalone neural RP01 examiner не установлен как caller этого пути.

## Будущая короткая проверка — сейчас не запускать

Для дешёвого исполнителя после прямого разрешения:

1. На одном заранее сохранённом наборе сегментов сравнить именно потребление trained state: прежний dataset router; нынешний default segment router; тот же segment router с обученным DomainClassifierUnit. Отдельно удерживать ExistingDomain=UNKNOWN и известный ExistingDomain. Не менять одновременно segmentation.
2. Сохранять ожидаемые текстовые spans и Domain, полный returned text, ID вызванного Unit, источник final domain/confidence. Считать полноту нужных spans и чужой текст раздельно. Старый noise оставить дополнительным показателем, не изменять исторические receipts.
3. Положительный контроль: конфликт keyword и заранее усвоенного predicate mapping при UNKNOWN должен отличать trained/untrained consumer, если этот mapping реально используется. Контроль известного ExistingDomain должен показывать намеренный приоритет компилятора, а не считаться провалом обучения.
4. Только затем отдельный обычный orchestrator запрос проверяет, меняется ли предметное решение от правильного routing. Успех routing не приравнивать к росту свободной C# генерации Student.

Этот дизайн различает неработающий learned consumer и ограниченную метрику. Он не обещает вернуть прежний успех и не требует нового большого DPO цикла.

## Открытые первичные следы

Нужен архивный quality-run нового per-domain пути после A50 с привязкой к коду и тем же предметным oracle. История класса показывает введение6 мая (`1917945c…`/соседняя lineage`1779e883…`), перенос контрактов7 мая (`eed8672…`/`a8553cb…`), затем header-only изменение. Их названия с количеством passing tests не считаются доказательством качественного обучения. Числа27 positive из SESSION-STATE также не приняты как27 прочитанных независимых экспериментов.

Предел чтения: адресные разделы перечисленных документов, весь небольшой segment router, начало/классификация DomainClassifierUnit, caller и source-test фрагменты; не весь compiler/orchestrator. Девять JSON прочитаны; часть широких поисковых выводов была усечена и не засчитана полным чтением. Изменён только этот Markdown. Прямой matrix→kNN donor продолжает разыскиваться, а E24 сохраняется как отдельный пример пересмотра отрицательного вывода при изменении контракта.
