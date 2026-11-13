# Сначала восстановить подтверждённое поведение DesignToCode-clean

## Главный вывод

**В DesignToCode-clean сохранилось конкретное успешное обучение CodeTrace019 и положительные результаты E25, E26, E09 и E43.** Их нужно использовать как исходные точки восстановления с собственными данными, моделью, метрикой и ограничениями. Нынешний RP01 Student не получает обещание «49/50» от чужого получателя обучения.

Проверены фиксированные версии:

- успешная ветка `DesignToCode-clean`: `e346b88a3cc1fcce48aefa626bfc7b00509eefd6`;
- текущая `codex/dpo-implementation`: `358af351e22a92a3f7938d1f9221d5aeb7ddb053`.

Три архивных текста CodeTrace019 и шесть выбранных исходников E25/E26/E09/E43/E07 **уже побайтно одинаковы в обеих версиях**. Поэтому первый вопрос — какое состояние и какой алгоритм обычный запрос фактически использует. По этим файлам не подтверждается версия, что успешный экспериментальный код исчез из current.

## 1. Первые короткие проверки

| Порядок | Проверка | Ожидаемый наблюдаемый результат | Когда нельзя объявлять восстановление |
|---|---|---|---|
| **R0-SNAPSHOT — состояние → обычный ответ** | Получить настоящий payload donor checkpoint, сверить идентификатор, открыть без обучения и отправить одинаковые запросы; затем cold restart | Тот же выбранный получатель и его состояние до/после restart; воспроизводимая старая метрика и отдельно качество обычного ответа | Есть только LFS pointer, manifest или статус trained; запрос обслуживает другой получатель; подтверждена только запись JSON |
| **R0-INPUT — вход и выбор получателя** | Зафиксировать выбранную модель/Unit, точный вход, токены и learned state; исправлять только подтверждённую потерю данных или выбора | Исправленный сигнал достигает выбранного обучаемого механизма и меняет его ответ в ожидаемых случаях | Исправление существует лишь в helper, а consumer получает прежний текст/состояние; результат создаёт другой fallback |
| **R0-LEARN — материал после проверки представления** | Один frozen набор Chinese-generated DPO, один recipient, один фактор в токенизации или разложении по Unit; 12–20 задач обучения и сопоставимый независимый контроль | Предметное улучшение на новых задачах, сохранённое после restart; закономерные отрицательные контроли | Рост только training score; repeated-prompt lookup; одновременно изменены tokenizer, objective, checkpoint и получатель |

Внутри причинного опыта обозначения едины: **P** — исходный parent; **C0** — тот же parent после того же load/eval цикла без update; **T** — одно проверяемое вмешательство. Исправление входа не называется C0. Это верх списка опытов. Архивное восстановление запускается прежде широкой настройки параметров. Подтверждённые нынешние исправления нейронного обучения — graph/backward, точный материал DPO, прикрепление LoRA и сохранение состояния — сохраняются; старое число не является основанием откатить их. Совместимость старого получателя и проверка нынешнего Student оформляются разными измерениями.

**Роли во всех опытах неизменны:** внешняя открытая китайская модель создаёт DPO-кандидаты. **CODEX с моделью Astra 6 ultracode выполняет роль человека при выборе и отдельно оценивает качество DPO.** Создание этих документов и пересчёт старых JSON не заявлены ни внешней генерацией DPO, ни выполненным CODEX/Astra review.

Перед новым training для DesignConversion также учесть уже опубликованный [direct projection/caller crosswalk54](../54-DESIGNTOCODE-CLEAN-SOURCE-PARITY-AND-NEW-GAPS-2026-10-19.md): потерю visual_score/category signal, новый admitted entry и границы source/vector представления. В [нашей общей карте](03-MECHANISM-COVERAGE-AND-DPO-LORA.md) эти наблюдения признаны чужими KNOWN_LATE и связаны с DC-01; изменившийся отчётный margin не объявлен множителем learning update.

## Новый узкий restore contrast — DC-01b-R

Финальный source review нашёл конкретную несовместимость **в DesignConversion state**: clean сохраняет нормализованный `correction_category:generic_visual_defect`, current whitelist его запрещает, а importer отвергает весь otherwise-valid checkpoint с таким весом. Это отдельный потребитель, не CodeTrace scorer. Поставить его проверку без обучения рядом с R0-SNAPSHOT: known / underscore / hyphen / оба alias / forbidden key, затем обычный load и сохранность остальных weights. Простое переименование alias может создать duplicate keys; whitelist ослаблять не требуется. [Точная цепочка, source refs и малый опыт](01-NEW-DESIGN-CORRECTION-SEMANTICS.md#dc-01b-r-неправильное-имя-может-блокировать-весь-старый-checkpoint). Реальный архивный отказ и его влияние на качество пока не измерены.

Общий downstream-механизм отказа всего checkpoint уже независимо описан в [CLEAN-CP01](../54-DESIGNTOCODE-CLEAN-RECOVERY-2026-10-19/02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md) для старого provenance key. Здесь восстановительный опыт расширяет его конкретным semantic generic alias и коллизией двух имён; новизна всего import-механизма не заявляется.

## 2. R0 — CodeTrace019: доказанное архивное обучение и точный предел 49/50

### Что действительно находится на clean

Серия `2026-05-21-code-trace-builder-promotion-confirmation-batch-019` содержит summary, все 100 DPO-строк и все 100 per-pair diagnostics. Для неё пересчитано:

| Поле | Значение |
|---|---|
| Обучающие/отложенные пары | 50 / 50 |
| Heldout pair accuracy | **49/50** |
| `missing_callee` | 10/10 |
| `missing_value_flow` | 10/10 |
| `wrong_caller` | 10/10 |
| `invented_callee` | **9/10** |
| `incomplete_trace` | 10/10 |
| Разнообразие всех 100 строк | 100 уникальных prompt, 100 chosen, 27 rejected |
| Архивный режим | 5 epochs, learning rate `0.0005000000237487257` |
| Архивная длительность | `163074.8545 ms` |
| Начало обучения | policy и reference из предыдущего **batch018** |
| Различие policy/reference до run019 | maximum absolute delta `0.07606835663318634` |

[Summary: конфигурация, продолжение и heldout, строки 8–26](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/project-wiki-dpo-metric-summary.json#L8-L26); [50 heldout diagnostics, строки 51–100](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/dpo-pair-diagnostics.jsonl#L51-L100); [DPO-материал, строки 1–100](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/dpo-pairs.jsonl#L1-L100). Единственный отрицательный margin среди 50 heldout находится в [строке 94 diagnostics](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/dpo-pair-diagnostics.jsonl#L94): `invented_callee`, `-0.002895355224609375`.

Время и параметры — сохранённые исторические сведения. В данном исследовании заново выполнялся пересчёт архива; нового training run не было.

### Что именно значит эта accuracy

В clean [wiki runner вызывает `EvaluateMany`, 357–360](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L357-L360), а [per-pair diagnostics, 430–449](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L430-L449) используют тот же evaluator. [Его формула, 249–274](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/11.DPO/DpoTrainer.cs#L249-L274):

`margin = Beta × ((policyChosen − referenceChosen) − (policyRejected − referenceRejected))`

`Accuracy = доля пар, где margin > 0`.

Во всех 100 сохранённых diagnostics значение Accuracy действительно совпадает со знаком сохранённого RewardMargin. **49/50 означает относительное preference-ranking заданных ответов к reference.** Это не 50 заданий со свободной генерацией и не независимый экзамен обычного wiki-ответа. Даже абсолютное предпочтение policyChosen над policyRejected этой метрикой не гарантируется.

В сохранённой паре присутствуют структурированные `PASS_CODE_TRACE` / `FAIL_CODE_TRACE` ответы. Для новой оценки нужны также варианты без подобных подсказок качества, новые исходные файлы и контроль лишних придуманных символов. Разнообразие 100 строк не заменяет разделение по семействам исходного кода. Код clean подтверждает семантику metric; точная бинарная сборка 21 мая этим чтением не аттестована.

### Почему checkpoint пока нельзя считать восстановленным

По путям [policy_final.bin](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/checkpoints/policy_final.bin#L1-L3) и [reference_base.bin](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/checkpoints/reference_base.bin#L1-L3) получены **132-byte Git LFS pointers**:

| Объект | Payload SHA-256 из LFS pointer | Размер payload |
|---|---|---|
| policy_final | `9a971504c9025d669a2f814fb5a6781bd08b527c8a575333c4280522dc72c726` | 1,188,744 bytes |
| reference_base | `7c3bfdacb17667843c7b9bc65feb380489106059a3e5eb1d7a89fe4b55b5d77c` | 1,188,744 bytes |

Полезные бинарные данные в этом исследовании не получены, не десериализованы и не проверены после перезапуска. Текущий статус — **roundtrip_NOT_PROVEN**. Первая работа R0 — получить настоящие payload и установить их совместимость с model config/tokenizer, затем прочитать обученное состояние обычным путём.

### Короткий восстановительный опыт R0

1. **Проверка без обучения.** Открыть final019 с его input pipeline и frozen reference, пересчитать те же 50 heldout, сохранить per-pair logits/margins, absolute policy ordering и выбранный recipient. Перед cold restart и после него сравнить идентификаторы состояния и ответы.
2. **Проверка продолжения.** Для текущего малого опыта использовать восстановленный совместимый parent и новые разрешённые пары с одним применением каждой; сравнение с новым началом — отдельное плечо при тех же новых парах и бюджете. Буквальное повторение batch018→019 с пятью эпохами относится к отдельной реконструкции исторического режима: её нельзя выдавать за текущий admitted pipeline, разрешать ей повтор старых pair IDs в production или считать обязательной первой работой. Если payload018 и его input contract недоступны, точное воспроизведение019 остаётся NOT_PROVEN.
3. **Проверка смысла.** Добавить 20 независимых C# заданий, по 4 на каждый тип ошибки. Оценивать полноту вызовов, argument handoff, terminal return path и invented symbols. Сохранить обычные ответы, не только DPO score.
4. **Проверка consumer.** Доказать, какой downstream механизм использует обновлённое состояние. Если улучшился лишь pair scorer, это полезный, но ограниченный результат для scorer; он не доказывает улучшение свободного Student.

При выборе входного представления нужна аккуратность: [прочитанный clean wiki runner, 616–626](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L616-L626) использует hash слов в ID и ограничение prompt, а не обратимый BPE. Его [default config, 45–57](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L45-L57) — отдельный model setup. Подача этих весов другому tokenizer/Student без преобразования не является восстановлением того же опыта.

## 3. R1–R4 — положительные механизмы, которые нужно проверить в правильном домене

### R1. E25: обучаемый порядок POS

На clean [raw E25](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json#L6-L18) сообщает `.3438→.875` при 32 validation cases; округлённые доли соответствуют 11/32 и 28/32. [Исполняемый код,22–47](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L22-L47) действительно обучает KNN classifier k=3 на синтетических POS sequences, seed=42, split80/20.

Это пригодный донор для памяти порядка в переводе. Малый опыт: 16 обучающих примеров и 16 новых предложений с перестановками ролей; одинаковые слова, одна разница в представлении порядка. Сначала проверить pattern classification, затем точность «кто сделал что кому» в обычном переводе. Результат на POS templates не переносится автоматически в C#, SQL или произвольный язык.

### R2. E26: полезный двусторонний контекст, но не доказанное обучение

[Raw E26](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json#L6-L18) показывает 40%→100% на 15 заданных случаях. Однако [experiment22–38](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L22-L38) сразу вызывает `BuildDefault`; [словарь 29–63](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L29-L63) заранее содержит нужные значения. [Выбор 66–84](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L66-L84) — ближайший контекст с priority tie-break. Поле k само по себе не доказывает голосование k соседей.

Для нового обучаемого опыта: пустая память, 12 Chinese-generated/CODEX-selected context lessons и 12 heldout форм двусмысленных слов. Менять только доступность правого контекста. Старые 15 случаев оставить для совместимости, а эффект обучения проверять отдельно от заранее заданного словаря.

### R3. E09: структурный KNN для C# при сопоставимой оценке

[Raw E09](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json#L6-L18) сохраняет 24.13%→35.07%, k5, 5617 узлов и 4492 train pairs. Но [код 50–77](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L50-L77) оценивает baseline на всей validation части, а KNN только на первых 500 узлах. Из зафиксированного count и split получаются разные знаменатели:1123 перехода против 499. [Target code107–120](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L107-L120) вдобавок использует ordinal SyntaxKind отдельно в каждом файле.

Поэтому восстановительный опыт использует один codec, общий train/heldout по файлам и одинаковые eval transitions. На 8–12 C# файлах сравнить один KNN с/без AST признака; сохранить paired predictions, затем проверить downstream source-grounded ответ. Positive raw receipt — основание исследовать структурную функцию, но не точная оценка улучшения общего продукта на 45.33%.

### R4. E43: удержание старого знания

[Raw E43](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json#L6-L18) сообщает 32.26%→39.78%,oldPairs465,newPairs414,replay177. [Код 23–55](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L23-L55) отдельно строит bigram counts для new-only и mixed70/30. Это положительное свидетельство для смешивания памяти. Оно не выполняет resume neural optimizer и не доказывает холодное восстановление LoRA.

Малый опыт для памяти: 12 old lessons и 12 new, сопоставимые 12+12 heldout; сравнить сохранение старых записей с их заменой при одинаковом новом материале и фиксированном бюджете. Архивное mixed70/30 можно отдельно воспроизвести как bigram-механизм. Для нейронного получателя повтор старых DPO не разрешается самим фактом существования E43: нужен допустимый самостоятельный режим и state policy/reference/optimizer; исходный E43 не заменяет эту спецификацию.

## 4. Matrix → KNN: сохранить идею, уточнить сравнение

[Отрицательный E07 raw](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-001.json#L6-L18) —15.31%→14.62%, `SCALE_UP_DATA_TOO_SMALL`. [Код E07,39–100](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L39-L100) использует BPE512 и многомерные n-gram counts; E09 использует структурные соседи и иные targets. Следовательно, неудача E07 не запрещает применение структуры внутри KNN, но две архивные цифры не являются controlled A/B того же перенесённого признака.

Быстрый приоритет — один KNN с/без структуры на общих данных. Если нужно проверить именно причину matrix-неудачи, добавить отдельную вторую пару table/n-gram с/без того же признака, сохранив общий target, split и eval denominator. Вывод об AST относится к программированию; вывод о порядке предложений — к соответствующим языковым задачам.

## 5. Какие сведения относятся к соседним веткам

| Архив | Что известно из предыдущих исследований | Статус на проверенном clean |
|---|---|---|
| CodeTrace019 |49/50, продолжение 018| **Первичные файлы найдены и пересчитаны здесь** |
| CodeTrace064 |46/50 в соседнем wiki snapshot| Три точных файла 404; соответствующего каталога нет в 389-entry parent listing |
| QueryPlanning134 |50/50;16 разных prompt,один chosen,один rejected в предыдущем пересчёте| Три точных файла 404 наclean |
| ContextRouting075 |50/50;10 разных prompt,два chosen,один rejected в предыдущем пересчёте| Три точных файла 404 наclean |

[Предыдущий отчёт P50 и его границы, строки108–115](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/01-NEW-CURRICULUM-FEEDBACK-GAP.md#L108-L115). Соседние результаты сохранены в качестве направлений поиска. Их цифры здесь повторно не пересчитывались и успехом clean не объявляются. Конкретные пути и граница проверки записаны в evidence; [родительский каталог clean](https://github.com/lubochka/llm_mvp_core/tree/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/43.LearningLocalWikiData/artifacts/runner-metrics) открыт полностью. Отсутствие трёх точных каталогов не означает отсутствия любых иных успешных запусков этих доменов в clean.

## 6. Почему новый пакет не требует переписывать уже сохранённые эксперименты

Побайтно одинаковы clean/current шесть исходников: E25PatternClassifier, E26ContextualDictionary, ContextualDictionary, E43ReplayBuffer, E09StructuralKnn, E07StructuralContextToken. Также одинаковы CodeTrace019 summary,diagnostics,pairs. Из этого следуют два ограничения:

1. Для перечисленных файлов не доказана регрессия «старый успешный код удалён». Нужна проверка фактического состояния и consumer.
2. Общие для clean/current дефекты токенизации, разложения урока, correction selection или fallback могут быть полезными новыми гипотезами. Их наличие само по себе не объясняет разницу качества двух веток.

Первые критерии успеха восстановления: настоящий checkpoint payload, воспроизводимый правильный получатель, сохранённое качество после cold restart, различающееся поведение на новых предметных задачах. Число успешных инженерных проверок и статус trained этих критериев не заменяют.

**Статус главы:** первичные source/archive подтверждены; historical numbers пересчитаны; новые training/restore/ordinary-consumer опыты **NOT_RUN**.

## Артефакты этого пересчёта

[Архивная квитанция](evidence/clean-recovery-receipt.json) · [Равенство шести source files](evidence/clean-source-parity.json) · [Точная граница отсутствующих трёх архивов](evidence/clean-absence-scope.json) · [Общий протокол малых опытов](04-SMALL-DOMAIN-EXPERIMENTS.md). Исторические данные пересчитаны; native learning, реальный LFS payload load и cold restore здесь не запускались.
