# KNN, сохранённая память и соседние ветки: проверенные различия

## Результат этого среза

**Независимо подтверждён конкретный разрыв восстановления: checkpoint с четырьмя направлениями, сохранённый в PrepareToGPL и всё ещё лежащий в текущей ветке, не соответствует шести обязательным направлениям нынешнего TranslationMachine.** Оба формата называются schema 1. Проверка импортёра требует полный набор направлений нового runtime и должна отказать с `EXECUTOR_DIRECTIONS_INCOMPLETE`. В этой сессии выполнено воспроизведение контракта по точным байтам исходников и checkpoint; C# загрузчик не запускался, изменение предметного качества не измерялось.

Одновременно собраны важные ограничения этого вывода. Модуль `12.SemanticCompiler` в двух ветках имеет один Git tree SHA и все 27 одинаковых файлов. Обучаемый `TranslationTrainedUnits.cs` и семантический токенизатор перевода тоже побайтно одинаковы. В уже опубликованных экспериментах текущего контура есть настоящее изменение памяти с выигрышем 0/24 → 19/24, успешное холодное восстановление 19/24 и отдельное исправление доставки phrase mappings с выигрышем 11/24 → 19/24. Их нельзя объявить отсутствующими возможностями или поставить заново в очередь как ещё не реализованные исправления.

### Зафиксированные точки сравнения

| Назначение | Commit |
|---|---|
| Успешная ветка, указанная пользователем | `PrepareToGPL`: `31346d8928be4469b01aaf313780e8f950ff91e2` |
| Проверяемая реализация | `codex/dpo-implementation`: `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6` |
| Соседняя ветка проектирования | `DesignToCode`: `8f20ceb8a9bfbadab2ad309226c47dd76e714548` |
| Соседняя ветка перевода | `language_translate`: `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` |
| Соседняя ветка внешних доменов | `external_domains`: `3036a01acec938f0b83fa4ffbda26202cc35d639` |

Слово «домен» ниже означает предметную область: перевод, программирование. EN→RU и RU→EN — направления внутри перевода; преобразование и объяснение кода — задачи внутри программирования; KNN и LoRA — механизмы. Применены пользовательские skills `trainable-architecture-decision-SKILL.md`, `human-readable-architecture-audit-SKILL.md` и словарь человеческой коммуникации. Отдельно различаются исходник, сохранённый результат чужого запуска, выполненная сейчас диагностика и ещё не измеренная причинная гипотеза.

## 1. Независимо подтверждённый разрыв: расширение runtime с 4 до 6 направлений блокирует старую память

### Что было и что стало

В PrepareToGPL метод `TranslationMachine.CreateRuntime()` создаёт четыре ExecutorUnit: RU_EN, EN_RU, HE_EN, EN_HE. Этот же набор включён в `executorsByDirection`, который экспортируется в checkpoint. [Исходник PrepareToGPL, строки 555–616](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L555-L616)

В текущем коде тот же метод дополнительно создаёт ES_EN и EN_ES; обе единицы входят в адаптеры, trainers и `executorsByDirection`. [Текущий исходник, строки 714–791](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L714-L791)

Экспорт по-прежнему записывает `SchemaVersion: 1`, а импорт по-прежнему принимает только schema 1. Структурная проверка импортёра вычисляет разность **текущих runtime-направлений** и направлений из checkpoint. Непустая разность означает отказ. Проверка выполнена до изменения состояния; это полезная атомарность, которую следует сохранить. [Экспорт и импорт, строки 282–354](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L282-L354), [проверка направлений, строки 407–430](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L407-L430)

Это не только возможный пользовательский snapshot. В **обеих** ветках отслеживается один и тот же файл `models/translation/translation-v1/translation-v1-checkpoint.json`, Git blob `a4cf054ba3f8a6ee6aa9818edc6c5705e3456c25`, 2550 байт. В нём schema 1 и ровно четыре ExecutorUnit. [Старый checkpoint, строки 1–66](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/models/translation/translation-v1/translation-v1-checkpoint.json#L1-L66), [тот же checkpoint в текущей ветке, строки 1–66](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/models/translation/translation-v1/translation-v1-checkpoint.json#L1-L66)

| Свойство | PrepareToGPL | Текущий runtime / отслеживаемый asset |
|---|---|---|
| Обязательные ExecutorUnit | 4 | Runtime требует 6 |
| Направления в отслеживаемом checkpoint | 4 | Тот же checkpoint содержит 4 |
| Версия схемы | 1 | 1 |
| Отсутствующие направления относительно своего runtime | Нет | EN_ES, ES_EN |
| Предсказанный результат проверки набора | Проходит именно проверку полноты | Отказ `EXECUTOR_DIRECTIONS_INCOMPLETE` |
| Новое обучение, необходимое для проверки | 0 | 0 |

Файл manifest перевода также одинаков: blob `9c79956c7c8bddbbc7afba37112c94f11648797b`. Совпадение объектов и перечни различий сохранены в [PREPARETOGPL-BLOB-IDENTITY-2026-10-18.json](PREPARETOGPL-BLOB-IDENTITY-2026-10-18.json).

### Почему это может выглядеть как ухудшение обучения

Успешное обучение изменяет конкретную память конкретных Unit. Если более новый runtime не принимает прежний носитель этой памяти, новый процесс не получает прежние learned records. Проверять коэффициент LoRA или расстояние KNN до проверки восстановления такого состояния преждевременно.

Цепочка здесь короткая и наблюдаемая:

1. `LoadCheckpointAsync` читает JSON.
2. Создаётся новый обычный `TranslationMachine` с шестью направлениями.
3. `ImportCheckpoint` проверяет полный набор направлений.
4. При отсутствии двух испанских ExecutorUnit возвращается ошибка; перенос KNN и maps не начинается.
5. Только при успешном импорте возвращается машина, готовая отвечать без replay.

Это следует из [единственного заявленного post-restart load path, строки 608–650](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L608-L650). В данном фрагменте нет доказанного автоматического fallback к пустой машине: утверждать, что приложение молча продолжает с пустым состоянием, нельзя без проверки конкретного caller.

### Какая память должна сохраниться

| Владелец | Передаваемое состояние | Почему имеет значение |
|---|---|---|
| Общий `TranslationKnnStore` | signature/source/target entries | Соседи, извлечённые из прежних уроков |
| `SemanticTokenizerUnit` | pattern/signature corrections | Представление последующих запросов |
| Старые четыре `ExecutorUnit` | exact-source, phrase, surface-word, word maps | Прямое использование и композиция выученных переводов |
| `QualityVerifierUnit` | выученные правила и required terms | Решение о пригодности ответа |
| `TranslationMachine` | history, identities, version/readiness | Продолжение обучения и принадлежность состояния |

Состав экспорта виден в [TranslationMachine, строки 282–300](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L282-L300), четыре maps Executor — в [TranslationTrainedUnits, строки 486–535](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationTrainedUnits.cs#L486-L535). Это обученное состояние памяти; нейронные слои, LoRA rank и optimizer здесь не задействованы.

**Предел доказательства:** отслеживаемый маленький checkpoint — безопасный fixture с тремя KNN triples и одной exact-source записью. Поле `accuracy_after_last_batch=0.3333` является метаданными и не переоценивалось. Более того, запись под RU_EN содержит английский source. Этот asset доказывает несовместимость контракта, но не представляет автоматически именно то богатое обученное состояние, на котором пользователь наблюдал высокое качество. Для восстановления прежней способности нужен отдельно идентифицированный успешный PrepareToGPL snapshot и его исходные наблюдения. [Содержимое fixture, строки 6–43 и 72–94](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/models/translation/translation-v1/translation-v1-checkpoint.json#L6-L94)

### Что выполнено сейчас

Создан и запущен read-only [PREPARETOGPL-CHECKPOINT-PREFLIGHT-2026-10-18.py](PREPARETOGPL-CHECKPOINT-PREFLIGHT-2026-10-18.py). Он получает два исходника и checkpoint, читает JSON как UTF-8 с допустимым BOM, извлекает направления строго из тела `CreateRuntime()`, сверяет созданные ExecutorUnit с `executorsByDirection`, фиксирует schema literals и проверку полноты. В нём нет запуска C#, обучения или изменения исходников.

[Сохранённый JSON результата](PREPARETOGPL-CHECKPOINT-PREFLIGHT-RESULT-2026-10-18.json):

- `classification = SOURCE_CONTRACT_REPRODUCTION_ONLY`;
- `native_csharp_load_executed = false`, `quality_measured = false`, `training_updates = 0`;
- `old_missing_directions = []`;
- `new_missing_directions = ["EN_ES", "ES_EN"]`;
- schema одинакова;
- для обоих исходников и checkpoint `api_blob_match_exact = true`.

Точные blob SHA проверенных байтов: старый исходник `4b94eb7967410ca2cd2d41ec3cd6a5f4dd21ead7`, новый `fd0e736de3739236b4dde2d6b9d000f5ebae8416`, checkpoint `a4cf054ba3f8a6ee6aa9818edc6c5705e3456c25`.

### Быстрый причинный эксперимент для главы восстановления PrepareToGPL

**Вопрос:** блокируется ли перенос старой памяти именно из-за добавления направлений, и возвращает ли совместимое представление тот же learned state без уроков?

**Предметная задача:** перевод. RU/EN/HE — старые направления; ES — контроль новых пустых единиц. Другие домены не получают эту гипотезу автоматически: сначала требуется такой же факт расширения topology при неизменном контракте.

**A:** текущий native `LoadCheckpointAsync` на точной неизменённой копии старого четырёхнаправленного checkpoint.

**B:** тот же native loader, тот же checkpoint в отдельной диагностической копии; единственное вмешательство — добавить две пустые записи ExecutorUnit для ES_EN и EN_ES. Все поля старых четырёх ExecutorUnit, KNN, tokenizer, verifier, history, identities и metadata оставить неизменными. Это диагностическая имитация additive migration, не готовое исправление продукта.

**Контроль:** уже имеющийся шестинаправленный P125 checkpoint и отчёт P25 показывают, что обычный cold load в текущем семействе кода работает. Его не следует повторно объявлять непроверенной возможностью. Для локальной проверки harness он может быть положительным контролем.

**Отрицательные контроли:** из отдельной копии убрать реально старое обязательное направление RU_EN либо продублировать направление. Эти случаи должны продолжить давать отказ. Эксперимент не должен просто отключить проверку completeness/duplicates.

**Наблюдения до качества:** load status/error code; число и canonical hash старых четырёх maps и остальных learned sections; количество lessons/updates строго 0. Не использовать source-history/metadata как замену фактическим записям. Для B допускается ожидаемое появление только двух пустых новых ExecutorUnit.

**Поведение:** после успешного B обычный `GetOrchestratorAsync().RunAsync()` на одной фиксированной панели и сравнение с наблюдениями старого процесса. Сначала exact recall прежних learned facts, затем новые source families и композиции. Отдельно correctness/все, coverage, meaning errors. Совпадение двух неверных ответов подтверждает retention поведения, но не качество.

**Порог причинного подтверждения:** A даёт отказ по направлениям, B проходит, старое learned state сохраняется без переобучения и обычный consumer видит прежние данные. На fixture достаточно подтвердить контракт; claim «восстановлено высокое качество PrepareToGPL» требует качественного старого snapshot и предметной панели.

**Минимальная будущая реализация:** явная миграция известной legacy-four-direction schema1 topology в six-direction schema1 либо отдельная версия topology в контракте/manifest. Миграция добавляет только новые пустые ExecutorUnit и сохраняет все старые bytes/значения learned sections в canonical representation. Не ослаблять атомарный импорт. Предлагаемое место ответственности — отдельный migrator перед структурной проверкой; существующие Unit обучаться заново не должны. Это предложение, не уже реализованный класс.

**Ресурсы:** 0 neural instances/updates, trainable layers N/A; измерить только время native load, RAM процесса, размер checkpoint и обычного inference. Никакой DPO генерации для первого шага не требуется. Если расширять предметную панель, выбор и оценка gold принадлежат CODEX с Astra 6 UltraCode согласно пользовательскому процессу; внешний китайский генератор может готовить стороны новых учебных примеров, но не выполнять оценку вместо CODEX.

## 2. Контрдоказательство: ключевой KNN и исполнитель не были заменены

Git tree `12.SemanticCompiler` **одинаков** в PrepareToGPL и текущем commit: `cae2d9355e803e5b46ec52b7d4fe0f00b524af0f`, 27 blob объектов. В том числе `StructuralKnn`, `MethodKnn`, `FileActionKnn`, фасад и contextual dictionary имеют одинаковые bytes. [Старый StructuralKnn](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/12.SemanticCompiler/Prediction/StructuralKnn.cs#L21-L35), [текущий StructuralKnn](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/StructuralKnn.cs#L21-L35).

В `27.TranslationDomain` из 31 старого blob сохранены 21 без изменения, изменены 10, добавлены 15, удалённых нет. `TranslationTrainedUnits.cs` имеет один blob `1742abfc9fb617dfc66e4d618e0c169da01a6ccc`, `TranslationSemanticTokenizer.cs` — `ee6d81c7933fa33a9c57a2111990019ec55e5dab`. Полная сверка — в [PREPARETOGPL-BLOB-IDENTITY-2026-10-18.json](PREPARETOGPL-BLOB-IDENTITY-2026-10-18.json).

В одинаковом Executor обучение десериализует chosen, добавляет exact-source mapping, три представления пары в KNN и переданные word/phrase mappings. При ответе идут exact source → phrase composition → word mapping → условный KNN. KNN смотрит cascade по full/prefix/structure signature, берёт k=5 и выбирает совместимого соседа. [Обучение, строки 403–455](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455), [обычный ответ, строки 264–294](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L294), [поиск соседа, строки 333–363](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L363).

Из этого нельзя вывести равенство всего приложения: caller, доступность уроков, сохранённая память, проверка источника и фактически выбранный Unit могли измениться. Но предложение «вернуть прежнюю реализацию KNN/Executor» не имеет причинного смысла для этих побайтно одинаковых файлов. Нужно искать изменившийся вход или соединение с потребителем.

## 3. Подтверждённые положительные результаты: сохранить и использовать как контроль

Ниже — **результаты, уже опубликованные другими исполнителями**, а не новые запуски этой сессии.

| Проверка | Измеренный исход в сохранённом отчёте | Правильный статус в общем списке |
|---|---|---|
| P125, настоящий memory update и обычный Flow | До 0/24; после 8 уроков 19/24 правильных, 22/24 принятых; swapped-target контроль 13/24 | Положительное доказательство обучения памяти; не нейронный DPO и не доказательство связи с RP01 |
| P25, новый OS process и cold load P125 snapshot | Все 24 outputs/routes/confidence/verdict совпали; 19/24 правильных сохранились; 0 уроков | Успешное восстановление текущего шестинаправленного snapshot |
| Исправление translation-memory-ingress | Одинаковые 8 уроков: 11/24 → 19/24; новые сочетания 0/8 → 8/8 | Уже исправленная доставка phrase mappings; использовать как donor, не как новый pending defect |

Источники: [P125 report, строки 3–29](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md#L3-L29), [P25 report, строки 3–35](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md#L3-L35), [memory ingress report, строки 3–11 и 57–79](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md#L3-L79).

P125 изменил сразу exact/phrase/shared-KNN память и сопутствующий state. Поэтому 19/24 нельзя приписать исключительно дистанции KNN или исключительно phrase mapping. Зато отдельно измеренный ingress repair при неизменной Domain DLL связывает выигрыш 11/24 → 19/24 с доставкой уже заданных phrase mappings в существующий Executor. [P125 состав вмешательства, строки 54–56](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md#L54-L56), [ingress границы сравнения, строки 57–79](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md#L57-L79).

В P25 новый loader восстановил другой, современный checkpoint: это не тест старого tracked четырёхнаправленного asset. Поэтому успешный P25 **не противоречит** новой гипотезе legacy topology. P25 также не доказывает корректность всех прежних ответов: отрицание и вопрос преобразовывались в утвердительную команду и после загрузки сохранились. Третий спорный край `проверь файл кэш` отмечен как неверный по строгому scorer/неоднозначный; его нельзя уверенно называть ещё одной доказанной инверсией смысла. [P25 результат и пределы, строки 25–41](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md#L25-L41), [независимая проверка P25](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/independent-review/review.md#L1-L44).

Нулевая внутренняя `accuracyAfterLastBatch` P125/P25 не опровергает внешние 19/24: отчёт связывает эту metadata-метрику с покрытием фиксированного списка units, включающего RU_EN, а эксперимент обучал EN_RU. Это отдельный известный дефект интерпретации метрики; предметная оценка получена из фактических outputs. [P25 report, строка 35](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md#L35).

## 4. Соседние ветки: прочитаны конкретные функции, а не только названия

### language_translate: контекстный выбор перевода

Прочитан `12.SemanticCompiler/Translation/ContextualDictionary.cs` на commit `f6032aff...`, blob `de49df00edc75c6d992fa17bac87bcd3c31e550e`. Между ним и PrepareToGPL добавлена только лицензия из пяти строк; функциональный код одинаков.

`TranslateWithContext` отбирает записи по stem-prefix, ранжирует по расстоянию соседних POS-контекстов, при равенстве по Priority и выбирает первую. `ContextDistance` отдельно рассматривает левый и правый контекст, но внутри каждой стороны использует Any-overlap, а не точный порядок. Это предметно релевантно неоднозначным словам перевода. Комментарий «+150%» сам по себе не является независимым повторным измерением. [Функция и расстояние соседней ветки, строки 62–107](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/ContextualDictionary.cs#L62-L107)

В общем каталоге уже есть E26 и уменьшение контекстных возможностей. Это не новая гипотеза данной сессии. Для нового причинного теста следует фиксировать словарь, source и gold; менять лишь left/right/order-aware расстояние и проверять перевод неоднозначных слов. AST-роли C# сюда напрямую не переносятся.

### external_domains: структурный KNN

Прочитан `12.SemanticCompiler/Prediction/StructuralKnn.cs` на commit `3036a01...`, blob `f5132ce4800082797ca7d0197373cbdbd82fdcae`. Отличие от PrepareToGPL — только пять строк лицензии.

Дистанция сохраняет веса синтаксической роли и родителей AST; extractor сохраняет position/depth/scope и строится на Roslyn. Это действующий источник идеи для задач C#, где роль левого/правого операнда, target присваивания, аргумент и возврат различают смысл. [Дистанция и extractor, строки 21–97](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Prediction/StructuralKnn.cs#L21-L97), [роли, строки 178–203](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Prediction/StructuralKnn.cs#L178-L203)

Указанные признаки не обязаны помогать обычному предложению, переводу или SQL без собственного parser/feature contract. Контроль для переноса в задачу кода: одна общая панель и vocabulary labels, одинаковые train/query, вмешательство только в набор признаков расстояния; отдельные одинаковые слова с переставленными ролями. Это уточнение уже известных E09-проверок, не новый обнаруженный дефект.

### DesignToCode: та же старая topology

Прочитан `27.TranslationDomain/TranslationMachine.cs` на commit `8f20ceb...`, blob `c9e78ab7f4fcb7e23bd88f9a2c872a17a651ccbf`. Отличие от PrepareToGPL — лицензия, алгоритм тот же. `CreateRuntime()` ещё четырёхнаправленный. Это дополнительная первичная lineage-проверка происхождения старого checkpoint-контракта. [Соседний runtime, строки 550–611](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationMachine.cs#L550-L611)

Локальные копии этих трёх файлов находятся в `audit/knn_src/language_translate`, `audit/knn_src/external_domains` и `audit/knn_src/DesignToCode`. Это выборочные первичные проверки; полный аудит всех файлов всех соседних веток не заявляется.

## 5. E07 → E09: положительную историю сохранить, причинность уточнить

Найдены и прочитаны оба первичных JSON результата на PrepareToGPL:

| Архивный запуск | Baseline | Enhanced | Статус в JSON |
|---|---:|---:|---|
| E07-STRUCTURAL-CONTEXT-TOKEN, tier 1, 2026-05-01 | 0.1531 | 0.1462 | passed=false, should_scale_up=true |
| E09-STRUCTURAL-KNN, tier 2, 2026-05-01 | 0.2413 | 0.3507 | passed=true, +45.33% относительно baseline |

Источники: [E07 JSON, строки 1–18](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-001.json#L1-L18), [E09 JSON, строки 1–18](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json#L1-L18).

В коде E07 Enhanced — `MultidimensionalNGram`, словарь переходов с ключом `(bpe, ast_type, depth)`, а не градиентное обучение матрицы transformer. Он использует BPE512; E09 строит векторы AST-узлов и локально нумерует SyntaxKind каждого файла. В E09 baseline оценивается на всей val, enhanced ограничен первыми 500 элементами. Эти уже известные различия не позволяют назвать пару JSON строгим парным доказательством переноса одной функции «из матрицы в KNN». Но они также не позволяют игнорировать положительный зарегистрированный E09. [E07 конкретный алгоритм, строки 20–43 и 87–100](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L20-L100), [E07 train/predict, строки 283–322](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L283-L322), [E09 панели, строки 26–77](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L26-L77), [E09 labels, строки 107–120](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L107-L120).

Быстрое уточнение истории: сделать общий heldout C# panel и стабильный словарь target SyntaxKind; брать одни и те же query/next-target пары в обоих плечах. Сначала воспроизвести неизменённый KNN как позитивный donor, затем парно отключать role/parent/order-компоненты его расстояния. Механизм нумерации labels, набор query, корпус и признаки вне выбранной оси должны быть зафиксированы. Только такой тест отделит полезность структуры внутри KNN от другого определения задачи. Нейронный DPO/LoRA потребует отдельного плана и не является частью этого дешёвого диагностического сравнения.

Локальный readback: `audit/knn_src/base/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs`, `E09StructuralKnn.cs` и оба JSON под `audit/knn_src/base/35.ExperimentsV2/results/`.

## 6. Новизна, ограничения и порядок включения в общий пакет

По обоим скачанным каталогам выполнен поиск совпадений; весь корпус содержит 627 файлов, включая вложенный `core/experimets` с карточками и реальными отчётами. Полностью прочитаны релевантные P25/P117 cards, P125/P25 reports и independent reviews, ingress repair report. Это поиск новизны плюс чтение относящихся документов, а не заявление о полном построчном прочтении всех 627 файлов.

- **Независимое подтверждение существующей гипотезы:** legacy-four-direction schema1 checkpoint против six-direction runtime. В исходном корпусе на81a0f456 этот точный разрыв не был найден, однако при обновлении planning доa4a3e709 он уже описан коллегой в [N41-T1](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md). Поэтому новизна не заявляется. Добавлены проверенные bytes tracked asset, одинаковый blob в двух ветках и source-contract preflight; P25/P117 остаются соседними темами.
- **Известное, теперь проверено первичным кодом:** структурные KNN, E07/E09 методологические различия, E26 контекст, chosen-only map learner, направление/фильтр KNN.
- **Уже положительно измерено другими:** native learned memory P125, cold restore P25, исправленный phrase-mapping ingress.
- **Контрдоказательство необоснованной регрессии:** одинаковые 27 файлов semantic compiler, одинаковые trained units/tokenizer перевода.
- **Не доказано:** что именно legacy checkpoint refusal вызвал наблюдённый провал RP01; что маленький shipped fixture является хорошей пользовательской моделью; что восстановление четырёхнаправленного богатого snapshot уже выполнено; что исторический KNN выигрыш автоматически переносится на другие домены.

Рекомендуемый порядок в главе восстановления: (1) новая короткая проверка legacy checkpoint topology и связь с фактически использованным snapshot; (2) уже успешные memory/ingress/cold-load результаты как сохранённые controls и donor path; (3) проверка подключения прежних KNN/units и доставки learned state, с учётом того, что их исходник не изменился; (4) новые доменные исследования структуры/порядка, отделённые от восстановления уже работавшей способности.

Исследователь этого среза работал только с локальными копиями; публикацию согласованного пакета в xiigen-planning выполняет основной исполнитель. Core и его `docs/plans/replanning-possible-diffs` этим аудитом не изменяются.

