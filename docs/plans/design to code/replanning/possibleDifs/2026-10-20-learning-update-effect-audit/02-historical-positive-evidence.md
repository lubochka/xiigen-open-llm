# 02. Успешные результаты сохраняются вместе с их ограничениями

Числа ниже — из первичных сохранённых отчётов, прочитанных в этом проходе; они не перемерялись. Следует различать приобретение навыка, изменение алгоритма и сохранность уже выученного результата.

## Пять разных линий доказательств

| Опыт | До → после / baseline → enhanced | Что действительно училось или менялось | Обязательные соседние показатели и ограничения |
|---|---|---|---|
| **18 октября: исправление translation ingress** | После одинаковых 8 уроков: **11/24 → 19/24**, новые сочетания **0/8 → 8/8** | В уже существующий Executor стали поступать 16 заданных phrase mappings; оба плеча обучали свежую память | Wrong accepted **1→3**; correct/answered **11/12→19/22**. Потерь прежних правильных ответов нет. Пять edge cases остались 0/5. Это реальный loader→learner→ordinary Flow, без изменения TranslationMachine/Executor algorithm |
| **18 октября: P125 acquisition** | **0/24 → 19/24** после 8 уроков | Память executor-en_ru; нейронных updates0 | Отдельный no-update control остался 0/24. Перестановка двух target-действий дала 13/24. Всего 144 ordinary Flow calls; это не новый наш бюджет. Exact8, near3 и recombination8 нельзя назвать 19 независимыми предметными законами |
| **E25 в language_translate** | Top1 POS-pattern classification **0.3438→0.875**, val32 | Train сохраняет POS-последовательности с pattern labels; Predict: Jaccard POS-биграмм, k=3, голосование | Архивные timings **0.4→15.4 ms**. Данные синтетические,160 примеров. Это классификация структуры, не полный перевод; выигрыш точности существует вместе с большей указанной стоимостью |
| **Поздний E22** | Macro-F1 **0.4709→0.5646**, val90 | kNN над предикатами: router.Train на 360 примерах, Predict routing vector | Архивные timings **0.1→100.5 ms**. Enhanced timer начинается после Train и feature extraction; нельзя назвать 100.5ms временем обучения. Полезный aggregate не скрывает TypeScript/Tests F1=0 |
| **RP01 EN→RU continuation** | Свободные ответы **0/4→0/4**; mean margin **0.0122930527→0.0124030590** | 500 уникальных пар,50 новых optimizer updates;16 LoRA tensors изменились,39 base неизменны; отдельный C50 reference неизменен | Margin delta **+0.0001100063**,11 cases выросли/9 упали, chosen-preferred12/20 в обоих. Full reopen подтверждён отчётом. Это локальное улучшение margin, но улучшения свободного перевода не показано |

Источники: [ingress](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md), [P125](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md), [E25 JSON](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json), [E22 JSON](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json), [RP01 JSON](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json).

**Это не один общий процент качества.** Сравнение 11/24→19/24 оценивает доставку материала при одинаковом обучении; 0/24→19/24 оценивает приобретение памяти. Смешивать их как два результата одной и той же манипуляции нельзя. У E25/E22 другие targets, модели и панели.

## Подтверждённый разрыв E25 → нынешний потребитель

В соседней ветке [PatternClassifier проекта 12](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/PatternClassifier.cs#L29-L86) имеет `_train`, `Train`, `Predict` и расстояние между POS-биграммами. [Driver E25](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs) обучает его до оценки.

Нынешний [PatternClassifier проекта 29](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/29.SemanticCompiler/Translation/PatternClassifier.cs#L20-L64) получает tokens, выводит POS через фиксированные ActionWords/StructureWords и выбирает pattern условиями. В данном классе нет обучения на E25-примерах. [LlmIngredientLogicalUnits](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L213-L242) использует именно этот класс; совпадение pattern добавляет 0.20 к fusion, vector/lexical имеют 0.45/0.35. Это конкретный потребитель, поэтому различие относится к получению результата из памяти. Само число 87.5% на POS не обещает такого же прироста downstream retrieval.

Минимальный перенос должен сохранять совместимые POS-признаки, метки и writer обученной таблицы, затем сравнить один и тот же обычный consumer с двумя способами pattern evidence. Смена токенизатора, состава памяти, k, fusion weights и pattern learner одновременно не локализует эффект. Старые подслучаи уже описаны в [17](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md) и [малых языковых опытах](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/10-additions-language-restore-49.md); это подтверждение прежней гипотезы первичными источниками, не новое открытие.

## Что удалось и не удалось установить про «матрица → kNN»

Прежний [адресный разбор E07/E09](https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) нашёл важные различия: легенда E07 про добавление измерений не совпадает с фактическим ключом N-gram; BPE ordinal и sampled character position — разные координаты. E09 использует AST напрямую. Отрицательный статус E07 поэтому не доказывает бесполезность AST/POS признаков в kNN.

В прочитанном текущем [E09 driver](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L50-L79) baseline оценивается на всей validation, а kNN на `Take(500)`. Сохранённый положительный сигнал заслуживает проверки, но его нельзя сразу назвать чистым paired gain. Нужен один и тот же набор объектов и устойчивое значение target ID между файлами. Эту проверку надо включить в настоящее обучение/потребление StructuralKnn, а не тратить отдельный запуск только на пересчёт чужой таблицы.

Поздний E22-положительный результат действительно существует; прежние FAIL не отменяют его. Но E22 уже использует kNN, а между версиями менялись labels/corpus/baseline. Он **не идентифицирован как точный перенос того самого матричного алгоритма**, о котором сказала Люба. [Сверка истории E22](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md). Конкретная цепочка donor function → move commit → learned state → ordinary consumer остаётся открытым адресным вопросом, а не основанием отвергнуть пользовательскую историю.

## Восстановление выученного результата — полноценная зависимая проверка

[P25](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md) отдельно описывает сохранение/загрузку памяти и воспроизведение 24 outputs. Такой вопрос не требует фиктивного нового обучения: нужен доказанно обученный источник, конкретный loader/state schema, одинаковые запросы и наблюдение доступности выученного результата. В отличие от этого случайное равенство двух пустых объектов ничего не говорит о сохранности навыка.

## Применимость по предметным темам

- **C#**: AST kinds, область видимости, operand order, границы исходных файлов. Неполный префикс для continuation нельзя оценивать как законченный компилируемый файл.
- **Перевод/язык**: порядок и POS, отрицание, вопрос, роли и референция. Признаки синтаксического дерева C# не переносятся сюда по совпадению слова «структура».
- **Маршрутизация задач**: predicate/route labels и реальный выбор следующего learner. Улучшение классификатора без использования его результата не равно улучшению учебной программы.
- **SQL, UI и графовые композиции**: требуется собственный recipient/consumer и предметный output. Они остаются в существующем реестре; фиктивного tiny SQL/Flow host этот пакет не вводит.

Ни рост одной метрики, ни рост затрат не стираются итоговым PASSED/FAILED. Полезный измеренный сдвиг признаётся по своей оси и теме, с указанной панелью и всеми ухудшениями.
