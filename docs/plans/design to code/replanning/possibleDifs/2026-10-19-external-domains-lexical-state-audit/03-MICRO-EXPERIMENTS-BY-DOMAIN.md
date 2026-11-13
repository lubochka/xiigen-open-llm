# Малые опыты по доменам: восстановление external_domains и новые C# разрывы

## Сначала выбрать исторического получателя: G0–G4

Приоритетная [глава 0](00-EXTERNAL-DOMAINS-RESTORATION-FIRST.md) задаёт восстановительный маршрут. Таблица ниже — проекты маленьких опытов; все G-опыты имеют статус `NOT_RUN`. Приведённые объёмы являются предлагаемым пилотом и не означают статистически подтверждённого прироста.

| ID | Предмет и потребитель | Минимальный опыт | Один фактор и независимый результат |
|---|---|---|---|
| G0 | Фактически успешный historical capability | 6–12 новых запросов к одной сохранённой capability в двух pinned checkout; затем reload | Только путь old/current при одинаковых corpus, encoder, options и state; выбранные example IDs, итоговый ответ, cold equality |
| G1 | C# structural KNN | 12 учебных файлов и 12 новых по семейству control-flow; общий SyntaxKind codec и одна validation панель | Только structural signal при общем candidate pool; качество нужного действия/объяснения, next-kind accuracy дополнительно |
| G2a | Порядок слов, E25 memory → переводчик | Сначала внутри проекта 12 восстановить архивный 128 TRAIN/32 validation, затем 12 новых POS; отдельно интеграция в переводчик | `PredictByRules` против trained `Predict` на тех же POS и восьми метках. Helper 29 имеет другой вход и labels; его интеграция требует явного encoder/crosswalk и отдельного опыта |
| G2b | Неоднозначное слово в техническом переводе, E26 | 8 учебных контекстов и 8 новых; два значения одного слова, те же candidates | Только соседний контекст; затем отдельно oracle POS против extracted POS; правильность выбранного значения и предложения |
| G3 | Продолжение выбранного memory Unit либо LoRA | Сохранённые 8 правильных старых случаев, 8 новых уроков, 8 новых TEST | Только replay при одинаковом родителе и общем бюджете; приобретение нового и сохранение старого измеряются раздельно |
| G4-Student | Raw Student C# documentation; отдельный языковой профиль EN→RU | Новый закрытый TEST для B0 и имеющегося обученного стартового checkpoint; одинаковые 2–8 принятых DPO-пар, один update в каждом плече | Только начальное обученное состояние; response logP/margin и независимая правильность свободного ответа; стоимость warm state указать отдельно |
| G4-Units | Переводческая проекция одного DPO | Две принятые исходные пары, шесть materialized Unit records; 6 новых TEST +4 retention | Только одно потребляемое поле target одного Unit. Изменение лишь Executor rejected — ожидаемый no-op контроль; [точные поля и fallback](01-LEARNING-INTERNALS-AND-EXISTING-GAPS.md) |

G1 — испытание C# структуры. SQL нуждается в своём parser/codec; обычный перевод — в языковом контексте и порядке. Языковое направление EN→RU само по себе не означает новый предметный домен: технический перевод и медицинский перевод могут требовать разных ограничений корпуса. Ни E25 labels, ни ActionLabelGenerator, ни marker requiredTerm не являются независимой оценкой качества собственного результата.

Для G3 обучаемая частотная память, KNN examples и AdamW-state проверяются отдельными опытами. E43 подтверждает полезность смешения корпусов у своей bigram-модели, но не устанавливает универсальные 30% replay. Для G4-Student нельзя одновременно сменить базовую модель, LoRA recipient, learning rate и длину маски: такой результат не локализует причину.

## Переход от механической диагностики к качеству

Сохранить до опыта: точный source SHA, checkpoint/optimizer/RNG IDs, tokenizer/feature schema, выбранный public consumer, train/test source families, флаги, k и порядок candidates. При нативном исполнении отдельно записать наблюдаемые значения этих полей: предполагаемая настройка не заменяет фактически использованную.

Затем менять один фактор и оценивать оба плеча на одних новых закрытых примерах. В отчёте должны быть числа новых правильных, потерянных правильных, одинаковых неверных ответов и конкретные исправленные случаи. Положительный результат маленького пилота — локальное причинное свидетельство для этой панели; он не является обещанием общего качества. Улучшение только внутренней метки без улучшения конечного ответа оставляет смысловую гипотезу открытой.

Если для модели не найден совместимый исходный checkpoint или реальный consumer не вызывает нужный компонент, результат обозначается `BLOCKED_MISSING_BASELINE` либо `NOT_APPLICABLE`. Это не неуспех обучения и не основание объединять разные получатели.

## Статус

Python-зеркало E10 и ASCII counter arithmetic исполнены; native .NET probe только подготовлен. Он не компилировался и не запускался: dotnet отсутствует в текущей среде. Учебных/модельных файлов репозитория probe не изменяет.

## Что находится в пакете

- [regex_proxy.py](evidence/regex_proxy.py) и [результат](evidence/regex_proxy_result.json): точное для четырёх простых ASCII fixtures зеркалирование порядка regex.
- [e02_identity_proxy.py](evidence/e02_identity_proxy.py) и [результат](evidence/e02_identity_proxy_result.json): отдельная арифметическая проверка case-insensitive счётчика.
- [NativeProbe.csproj](native/NativeProbe.csproj) и [Program.cs](native/Program.cs): вызов настоящих публичных classes из 12.SemanticCompiler через ProjectReference. Код реализации не копируется и не подменяется mock-классами.
- [source_parity.json](evidence/source_parity.json): две pinned ветки, blob SHA, равенство тел шести source files.

## Нативный запуск

Использовать уже имеющиеся dependency cache и .NET 9 SDK. CoreRepo — абсолютный путь к отдельному checkout нужного SHA; probe располагается вне него. Сначала одна ветка, затем другая, в отдельных выходных каталогах.

Checkout должен быть одноразовым: обычный ProjectReference build может создавать bin/obj в ссылочных проектах. Неизменяемыми остаются tracked source и model/training artifacts; физическая read-only файловая система для сборки не предполагается.

~~~text
dotnet build native/NativeProbe.csproj -p:CoreRepo=ABSOLUTE_CORE_CHECKOUT -o TEMP_OUTPUT_DIRECTORY
dotnet TEMP_OUTPUT_DIRECTORY/NativeProbe.dll
~~~

Не интерпретировать отсутствие/ошибку build как опровержение гипотезы. Сохранить фактический SHA checkout рядом с JSON. Переменная KNN_AUDIT_SOURCE_REF необязательна: она лишь подписывает результат; значение UNSPECIFIED запрещает branch-parity claim. Проверить CSharpQueryDetected, QueryStack, непустое QueryAction и QueryExpectationMatched/ControlExpectationMatched: отсутствующий route или выключенный predictor не является успешным восстановлением. Эти поля диагностические; они не заменяют semantic quality.

## Последовательность и ожидаемые различия

Обозначения в пакете: **C1 = KNN-RX1**, **C2 = KNN-RX2**, **C3 = KNN-OBS1**. R0 нативного проекта — узкий C# вариант общего G0; он не восстанавливает автоматически все состояния исторической capability.

| Опыт | Один изменяемый фактор | Что печатает runner | Предсказание по исходнику |
| --- | --- | --- | --- |
| R0 | Только checkout external/current при одинаковых inputs и options | Та же панель diagnostics и решений | Из-за равенства тел результаты этой панели должны совпасть; несовпадение требует проверки зависимостей/окружения |
| RX1 | Preprocess off/on для URL-строки | Normalized text, syntax errors, AST member/call counts | URL // удалит code tail; native errors ожидаются, ещё не измерены |
| RX2 | Только E10 on/off у свежих facade с теми же raw TRAIN | Raw/normalized AST; feature cosine; action на interpolation и constant control | Исчезнет member access; tiny query ожидаемо поменяет nearest-class, constant control — нет |
| OBS1 | Обычный . против условного ?. в наблюдаемом вызове | Extracted calls, completion ranks | Conditional calls отсутствуют в памяти; этот результат ещё не выполнен в .NET |
| Supplemental ID | Только sensitive/insensitive spelling при одном receiver | Ранги для [read,Read] и [Read,read] | При merged key побеждает первый candidate; это арифметически воспроизведено отдельно |

## Проверка влияния на полезный результат

После L0 остановиться, если actual caller нужного домена не вызывает исследованный компонент. Если вызывает, провести один маленький C# pilot:

1. Зафиксировать 12 TRAIN и 12 новых исходников TEST, раздельных по семейству кода. Слои: HTTP-строки, interpolated logging, nullable collection access. Добавить четыре обычных неповреждаемых C# случая.
2. Для RX менять только источник AST (raw против существующего E10); для OBS менять только обработку MemberBinding. Не менять одновременно tokenizer, k, labels и learning budget.
3. Freeze independent semantic gold: какие реальные обращения/вызовы присутствуют и какой ответ/действие требуется при данном контексте. Label generator не служит gold-authority. Финальные DPO sides — внешняя открытая китайская модель; выбор и quality review — CODEX с Astra 6 UltraCode.
4. Считать правильность конечного ответа ordinary consumer и сохранность старых правильных ответов, отдельно механические counts. Обучаемый counter/KNN state и DPO/LoRA weights — разные получатели; не объединять их score.
5. Восстановить обученную память в новом instance из того же raw corpus, повторить ту же TEST-панель, добавить один source из другой семьи и проверить сохранность старых результатов. При смене feature schema требуется полный согласованный rebuild, а не query-only patch.

Для EN↔RU, Hebrew, SQL и произвольного текста эти C# эксперименты не являются универсальным лекарством. Если domain routing отдаёт их C# parser, это отдельная гипотеза маршрутизации; её здесь не исправляют.

## Что опровергнет причинную гипотезу

Совпадающие native normalized features после intervention; неиспользование этих функций реальным consumer; одинаковое semantic качество обеих ветвей при содержательном независимом TEST; либо улучшение только deterministic surrogate label, без улучшения конечного ответа. Любой из этих исходов должен понизить или закрыть соответствующую гипотезу, а не запускать более крупное обучение автоматически.
