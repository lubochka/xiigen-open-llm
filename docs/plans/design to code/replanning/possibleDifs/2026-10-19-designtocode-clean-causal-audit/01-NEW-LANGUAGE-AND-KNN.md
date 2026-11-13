# Три точечных ограничения структурных механизмов DesignToCode-clean

Дата: 19.10.2026. Основной донор — `DesignToCode-clean@e346b88a3cc1fcce48aefa626bfc7b00509eefd6`; проверяемая реализация — `codex/dpo-implementation@358af351e22a92a3f7938d1f9221d5aeb7ddb053`.

**Основной вывод:** полезную обученную память порядка частей речи нужно восстанавливать вместе с проверкой следующего преобразования ответа. Даже правильный результат классификатора не спасает ответ, если последующее правило удаляет участников или перемещает только часть подлежащего. Найдены два таких точных контрпримера для E27 и дополнительный условный пробел меток E13. Эти механизмы присутствуют и в `DesignToCode-clean`, и в нынешней ветке; они не являются доказательством регрессии при переносе Git и не установлены как причина неудачи отдельного нейронного RP01 Student. [Донорский helper E27][clean-e27-helper], [нынешний helper E27][current-e27-helper], [MethodKnn current][method-label].

Здесь выполнены чтение исходников, сверка Git blob SHA и небольшой Python-расчёт по формулам исходников. **Native .NET, новые предметные эксперименты, обучение и оценка качества модели не запускались.** Числа расчёта ниже относятся к специально заданным диагностическим входам. Исторические сохранённые результаты и новые аналитические контрпримеры разделены.

Ролевой контракт дальнейшего обучения: внешняя открытая китайская модель создаёт обе стороны DPO; **CODEX с Astra 6 UltraCode выполняет выбор и оценку качества**. Диагностические примеры этого документа написаны для проверки механизма, не являются teacher-authored DPO и не свидетельствуют о фактическом запуске Astra 6 UltraCode.

## 1. Что именно сначала восстанавливать из успешной ветки

В `DesignToCode-clean` сохранён [E25 результат][e25-result] **34,38% → 87,5%** на 32 проверочных POS-последовательностях из 160 синтетических примеров. [Runner][e25-runner] действительно вызывает `Train(train)` и затем `Predict` на отдельном `val`; [alias][aliases] ведёт в `SemanticCompiler.PatternClassifier` проекта 12. Там обучаемое состояние — список размеченных POS-последовательностей, а прогноз — kNN по множествам направленных POS-биграмм. Это полезное историческое свидетельство обучения памяти структуры, с его собственными данными и метрикой; оно не измеряет автоматическое выделение POS из сырого языка или полный перевод.

Отдельный [E27 результат][e27-result] сообщает **2/7 → 7/7** совпадений порядка. Но [его runner][e27-runner] использует семь заранее заданных случаев с уже известным `SourcePattern` и не вызывает обучение E25. Фактический класс этого runner — `ExperimentsV2.PatternReorderer` из `35.ExperimentsV2/Infrastructure/PatternReorderer.cs`, а не одноимённый класс проекта 12. Оба реализуют изучаемые ниже правила. Значение дополнительного `IsSVO` в receipt равно **4/7**, что уже разбиралось в прежнем исследовании [Core33][prior-e27-core].

Для приоритетного восстановления нужны две короткие раздельные проверки. Сначала — один и тот же E25 learner без памяти и после разрешённых уроков на новых POS-семействах. Затем — одинаковый предсказанный pattern подать в конечный reorder/consumer и проверить сохранение участников и связей. E27 не заменяет измерение обучения и не должен получать заслугу за работу E25. Новые N-K1/N-K2 ниже добавляют конкретные диагностические случаи ко второй проверке; не требуется заново исполнять всю старую матрицу.

Сверка показала одинаковые байты `PatternClassifier`, обоих owners `PatternReorderer`, `MethodKnn`, `StructuralKnn`, `FileActionKnn` и `ContextualDictionary` в clean/current. Простое возвращение этих файлов из донора не меняет формулы. Восстановление полезного поведения дополнительно зависит от фактически загруженных примеров, кодека, настроек и того, кто читает результат. [Сверка файлов](evidence/knn/source-manifest.json).

## 2. N-K1: после правильной классификации императива исчезают объект и получатель

### Что происходит

У E27 есть операция удаления явно названного подлежащего из императива. Но условие удаляет **каждое** слово с тегом `PRON`, без различия между подлежащим, объектом и получателем:

```csharp
words.Where(w => w.PosTag != 3).ToList()
```

Источник: [фактический experimental helper, строки 51–53][helper-pronoun] и [project-12 counterpart, строки 50–52][project-pronoun]. Сейчас никакое число правильно выученных `IMPERATIVE_VS` не защищает объект: потеря происходит после решения классификатора. В [контракте TaggedWord][tagged-word] есть только текст и POS, без синтаксической роли.

Ниже дан вход именно уровня E27: слова уже приведены к английской лексике и размечены, как в семи исходных случаях донора. Это не утверждение, что нынешний сырой tokenizer выдаст такие же теги.

| Заданный вход и контракт | Ожидаемое преобразование | Расчёт нынешнего правила |
|---|---|---|
| `Add you check to method`; `you` — явное подлежащее | `Add check to method` | Такой же правильный результат; исходный положительный контроль донора |
| `Send you it to her`; `you` — подлежащее, `it` — объект, `her` — получатель; pattern **правильный IMPERATIVE_VS** | `Send it to her` | `Send to`: исчезают объект и получатель |
| `Tell you him about it`; `you` — подлежащее | `Tell him about it` | `Tell about`: исчезают обе объектные связи |
| `Send it to her`; pattern **IMPERATIVE_V** | Вход остаётся целым | Вход остаётся целым; отрицательный контроль другого правильного pattern |

Таким образом, ошибка не требует неправильного выбора E25. У самого E25 также нет общего решения этой роли через один только POS: подлежащее и объект оба кодируются как 3. Но добавление роли в расстояние классификатора — отдельное вмешательство; оно не заменяет исправление удаления после правильного pattern.

### Как проверить и какое изменение конкретно сравнивать

**Тема — перевод; первый профиль — преобразование в английский порядок.** Для русского и иврита нужны собственные источники ролей; для EN→RU это правило «превратить в английский императив» не переносится автоматически. Для C#, SQL и оркестрации этот конкретный POS-фильтр неприменим.

Для первого native шага достаточно настоящего `ExperimentsV2.PatternReorderer.Reorder` на четырёх входах таблицы, без тренировки модели. A — нынешнее удаление всех PRON. B — диагностический `ReorderWithRoles(words, pattern, subjectSpan)`, который удаляет только диапазон явно указанного подлежащего. `subjectSpan=(start,endExclusive)` поступает в обоих плечах как одинаковая проверенная входная аннотация; A её игнорирует, B использует. Остальные слова, POS, pattern и порядок неизменны. Не выделять подлежащее правилом «первое местоимение»: оно повторит ту же ошибку на другом порядке.

Предлагаемый носитель для проверки — `ReorderRoleAnnotation(SubjectStart, SubjectEndExclusive, TokenRoles[])`; функция остаётся внутренним преобразователем, не новой обученной единицей. Вход с неизвестной ролью сохраняется с явным `roles_unknown`, без объявления перевода правильным. Новые checkpoints этому чистому helper не нужны. При будущем включении в существующий `ExecutorUnit` состояние и provenance остаются у этой Unit; source-only аннотация должна приходить от её обычного parser/получателя, а не из выбранного ответа или закрытой проверки. Пара `Send you it to her → Send it to her` годится как диагностический контракт, но перед DPO обе стороны генерирует внешний teacher, CODEX выбирает и оценивает.

Основные наблюдения: какие token indices удалены, сохранены ли объект и получатель, точный конечный текст и отказ при неизвестной роли. **Фальсификатор:** настоящий helper не удаляет указанные объектные PRON либо actual serving path вообще не вызывает эту трансформацию. Во втором случае находка остаётся ограничением предлагаемого восстановления E27, а не объяснением действующего RP01.

После положительного native сигнала достаточно одного маленького опыта обучения на выбранном ordinary path: 2 разрешённых урока, 4 новых задания, одинаковые исходные состояния A/B, проверка до/после. Это 16 ответов, а не обязательный новый большой корпус. Если структура выбрана правильно, но неверный постпроцессинг стирает приобретённое умение, разница проявится в final answer при одинаковом classifier output. Если результат меняется только на ручной аннотации, статус — «механизм подтверждён; сырой языковой путь ещё не проверен».

## 3. N-K2: VSO перемещает существительное, разрывая именную группу

### Что происходит

В [ветке VSO experimental helper][helper-vso] `FindIndex` выбирает первое noun/pronoun после глагола, затем `RemoveAt`/`Insert` переносят ровно **одно** слово. У [project-12 counterpart][project-vso] то же правило. Оно правильно работает на донорском `add I check`, где подлежащее занимает одно слово. У многословного подлежащего определение остаётся рядом с объектом.

| Правильно размеченный VSO-вход | Смысл аннотации | Ожидаемый порядок | Расчёт нынешнего правила |
|---|---|---|---|
| `add I check` | Подлежащее `I` | `I add check` | Совпадает с донорским положительным контролем |
| `reads old server file` | Подлежащее `old server`; объект `file` | `old server reads file` | `server reads old file` |
| `reads the old server file` | Подлежащее `the old server` | `the old server reads file` | `server reads the old file` |
| `reads server file` | Однословное подлежащее `server` | `server reads file` | Правильный отрицательный контроль длины группы |

Это не потеря общего порядка bag-of-words: последовательность существует и преобразователь её меняет. **Точная потеря — граница зависимых слов внутри выбранного подлежащего.** В двух ошибочных случаях [IsSVO][helper-svo] всё равно возвращает true: существительное уже стоит перед глаголом. Прежние работы знали, что `IsSVO` — ограниченная метрика; новый контрпример показывает конкретное изменение связи определения при правильном pattern и формально успешной проверке.

### Как проверить и какое изменение конкретно сравнивать

Native preflight повторяет четыре строки таблицы на текущем helper. A переносит одно слово. B переносит только заранее заданный цельный диапазон `subjectSpan`; token order внутри диапазона сохраняется. Например, для второй строки диапазон `[1,3)` даёт `old server`; для третьей `[1,4)` — `the old server`. Новый overload и `ReorderRoleAnnotation` те же, что выше, но intervention отдельный: N-K1 меняет правило удаления, N-K2 — единицу перемещения. Проверять их в разных A/B, чтобы не смешивать причины.

Метрики — сохранение связи `old→server`, сохранение объекта, конечный порядок и ложные успехи IsSVO. Ошибку нельзя «исправить» лишь заменой gold на нынешний текст. Чтобы перейти от ручных spans к обычному переводу, реальный источник структуры должен сохранить диапазон подлежащего, включая определения и составные имена. Если доступен только POS-bigram class, данных для этой операции недостаточно: роль классификатора — выбрать общий тип, не угадать все границы по одному номеру класса.

**Фальсификаторы и границы:** если parser обычного пути не передаёт многословное подлежащее или E27 не достигается, downstream benefit не установлен. Если предметная задача действительно переносит отдельный head-token, а определения намеренно обрабатывает другой подтверждённый stage, полный trace этого stage может опровергнуть предполагаемую потерю. Подставлять нейронную генерацию, другой POS extractor или новый k одновременно с заменой единицы перемещения нельзя.

Профиль относится к переводу/грамматической перестановке при цели SVO. Для языков с другой допустимой позицией определения, согласованием и свободным порядком требуется собственный контракт. Не вводить глобальную обязанность «всегда перемещать все слова до первого глагола».

## 4. N-K3: условный пробел E13 — While/Do сохранены в признаках, но отсутствуют в метке Loop

У [MethodExtractor][method-extract] `WhileStatement` преобразуется в `While`, а `DoStatement` по fallback `EndsWith("Statement")` — в `Do`. Но [CanonicalType][method-label] распознаёт Loop только по `ForEach` или `For`:

```csharp
if (kinds.Contains("ForEach") || kinds.Contains("For"))
    return "Loop";
```

При одинаковом дополнительном `Call`: `{While,Call}` и `{Do,Call}` становятся `Mixed`, а `{For,Call}` и `{ForEach,Call}` — `Loop`. Здесь feature не стёрт, expression body не нужен, нескольких обязанностей нет. **Условный риск:** если получатель понимает Loop как «метод с циклом», обучение воспроизводит слишком узкую онтологию; при нынешнем правиле голоса соседей, чьи следующие методы содержат только while/do и Call, останутся Mixed. Другие соседи всё ещё могут предсказать Loop: это прогноз следующего метода, а не классификация самого query.

Выход доходит до наблюдаемого контракта: [Predict][method-predict] группирует соседей по `CanonicalType(Next.KindBag)`; [facade][facade-predict] вызывает его, если включён `UseMethodKnn`, вход C# и накоплено хотя бы 4 пары; [Compile][facade-result] возвращает строку в `CompilationResult.PredictedNextMethodType`. Это concrete consumer граница. Дальнейший генератор текста, который использует данное поле, в этой подзадаче не установлен; поэтому ущерб свободному ответу условный.

**Возможное контрдоказательство:** прежняя задача могла намеренно определить Loop только как семейство for/foreach. Тогда исходный label корректен, а предлагаемое расширение — новый контракт, не найденный баг. Для frozen legacy E13 metric нельзя менять определения и объявлять получившийся прирост восстановлением старой точности. Также не включать сюда прежний composite-label вопрос Guard+Async.

Минимальная проверка: 5 прямых `CanonicalType` входов из [расчёта](evidence/knn/knn_structural_proxy_results.json), затем одинаковая frozen память пар и k. A использует прежнюю метку, B добавляет `While` и `Do` в **тот же** Loop branch, сохраняя приоритеты Try/Guard/Async и все признаки/расстояния. Независимый gold сначала утверждает, что в этой задаче Loop действительно означает все четыре цикла. При исследовании facade нужны ≥4 сохранённых пар, иначе отсутствие ответа объяснит уже известный порог методов/пар. Считать label coverage и `PredictedNextMethodType`, а конечную документацию — только при найденном reader этого поля. Все старые данные и результаты остаются под исходной версией label schema; версия B получает своё имя.

Для предметной фазы N-K2 достаточно двух уроков с однословным и составным подлежащим и четырёх новых входов при том же classifier output; N-K3 — двух новых разрешённых переходов и четырёх новых запросов поверх одной и той же исходной памяти, уже удовлетворяющей порогу ≥4 пары. Новый урок и штатный label должны дойти до обычного получателя. Механический вызов helper и конечный пользовательский ответ сохраняются раздельно. Полный маленький план приведён в [главе05](05-SMALL-EXPERIMENTS-AND-PROMPTS.md).

## 5. Новизна и предел доказательств

Поиск выполнен по **689 Markdown** двух реестров на pinned refs: planning `b0308fbaa783758104987e6544d057062ccf6fc3`, core `358af351e22a92a3f7938d1f9221d5aeb7ddb053`; байты корпуса проверены родительской задачей. Это адресный текстовый поиск с чтением близких работ, не заявление о построчном review всех документов. [Регулярные выражения и совпадения](evidence/knn/novelty-search.json).

| Новая конкретизация | Уже известная ближайшая работа | Почему это отдельная точечная проверка |
|---|---|---|
| N-K1: правильный IMPERATIVE_VS удаляет объектные PRON после классификации | [N26 E27 coverage loss][prior-e27], [Core33][prior-e27-core] | Они различают whole reorder и local null-check swap; не выделяют потерю объекта при правильном donor pattern |
| N-K2: VSO разрывает `old server`, при этом IsSVO=true | Те же E27 материалы; общая P16 потеря ролей/порядка | Изменяется единица движения: один head-token против цельного подлежащего. Ни новая метрика, ни смена k не устраняет эту ошибку |
| N-K3: While/Do исключены из Loop | [N38 expression-body][prior-expression], [N38 composite labels][prior-composite] | Признак существует; одиночная требуемая категория не покрывает конкретные циклы. Это условный контрактный вопрос, не новая независимая версия прежнего multi-label дефекта |

**Что реально дал новый расчёт:** 2 из 4 специально заданных pronoun cases расходятся с аннотацией при legacy-фильтре; 2 из 4 constituent cases расходятся при single-token move и обе ошибочные строки проходят IsSVO; 2 из 5 KindBag меняют метку при объявленном расширении Loop. Положительные donor/control cases сохранены. Это арифметика/операции списка по исходному коду, не оценка распределения пользовательских запросов и не измеренное повышение качества обучения. [Скрипт](evidence/knn/knn_structural_proxy.py), [результат](evidence/knn/knn_structural_proxy_results.json).

Запуск расчёта с полными локальными checkout:

```bash
python evidence/knn/knn_structural_proxy.py --repo /path/to/llm_mvp_core
```

Перед публикацией дополнительно проверены 31 новый/изменённый Markdown на planning `64ade14ed5dfb799c5e3edc7ac44acdc957aabcd`, а root прочитал ещё два навигационных README. Точных повторов N-K1/N-K2/N-K3 не найдено; [решение отдельного reviewer](evidence/review/review-publication-delta.md). Это уточняет область поиска, но не доказывает глобальную уникальность во всех ветках. Обновлённый скрипт сверяет ожидаемые commit/blob/SHA256/размер до расчёта; [проверка исправления и отрицательный контроль](evidence/knn/proxy-harness-verification.json).

Большая доля KNN-проблем уже описана прежними агентами: топология HNSW, aliased vectors, подсказки по id, maxLexical, ограничение 64 токенов, кратность POS-биграмм, expression-body и confidence. Они не посчитаны новыми. В соседней зоне проверены реальные owners/aliases и текущий facade output; отсутствие полного ordinary raw-language trace, parser-generated role spans и измеренного обучения явно остаётся открытым. Такой trace может сузить или опровергнуть перенос N-K1/N-K2 на действующий продукт.

Применены проектные навыки архитектурного решения, человеческого архитектурного объяснения и словарь, проверки generated code и дизайна эксперимента. Разделение темы/профиля/получателя соблюдено; deterministic E27 назван внутренним преобразователем, E25 — обучаемой памятью. Ни один source/corpus файл protected `docs/plans/replanning-possible-diffs` не изменён. Этот пакет — результат ограниченного исследования для родительской проверки и интеграции, без remote mutations и без утверждения полной достаточности аудита.

[clean-e27-helper]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L8-L88
[current-e27-helper]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L8-L88
[e25-result]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json#L2-L18
[e25-runner]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L20-L48
[aliases]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/GlobalUsings.cs#L6-L11
[e27-result]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E27-PATTERN-REORDERER/results/run-2026-05-01-001.json#L2-L18
[e27-runner]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E27PatternReorderer.cs#L20-L45
[helper-pronoun]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L51-L53
[project-pronoun]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/Translation/PatternReorderer.cs#L50-L52
[tagged-word]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L88-L89
[helper-vso]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L33-L45
[project-vso]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/Translation/PatternReorderer.cs#L33-L44
[helper-svo]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L78-L84
[method-extract]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/Prediction/MethodKnn.cs#L61-L104
[method-label]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/Prediction/MethodKnn.cs#L171-L184
[method-predict]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/Prediction/MethodKnn.cs#L119-L146
[facade-predict]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/SemanticCompilerFacade.cs#L313-L323
[facade-result]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/SemanticCompilerFacade.cs#L155-L175
[prior-e27]: https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/26-NEW-E27-STRUCTURAL-REORDER-COVERAGE-LOSS-2026-10-16.md
[prior-e27-core]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/33-e27-structure-transfer-and-wordmap-counterexample.md
[prior-expression]: https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/38-NEW-E13-EXPRESSION-BODY-COLLAPSE-2026-10-16.md
[prior-composite]: https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md
