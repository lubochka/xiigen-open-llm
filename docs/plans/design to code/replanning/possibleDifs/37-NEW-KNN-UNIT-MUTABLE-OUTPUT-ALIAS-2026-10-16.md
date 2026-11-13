# N37-01 — Новое условное расхождение: обученный KNN Unit не владеет неизменным слепком своего Output

**16.10.2026 · FRESH SOURCE AUDIT · C=SOURCE-CONFIRMED ALIAS PATH · H=PRODUCT QUALITY IMPACT NOT PROVEN · NATIVE_NOT_RUN.** Прочитанная текущая ветка: `lubochka/llm_mvp_core:codex/dpo-implementation`, HEAD во время чтения `793d7b755b23aac90445da3ac6661b1bff7e6672`. Писать материалы разрешено только в `xiigen-planning/llm_plan3/.../replanning/possibleDifs`. `core/docs/plans/replanning-possible-diffs` — READ ONLY.

## 1. На человеческом языке: где может исчезнуть уже выученное

Система обучила KNN-юнит на правильном примере, внесла в индекс вектор и стала отвечать правильно. Но объект **ответа** оставила тем же изменяемым объектом, что был у внешнего поставщика данных. Позже другой код изменил этот объект — или изменил объект, уже полученный из `ExecuteAsync`. При следующем запросе юнит отдаёт новый ответ, хотя он **не проходил новое обучение**, не менял соседей и не принимал обратную связь.

Для `TOutput=string` этот механизм не срабатывает: строка неизменяема. Для `TOutput=Dictionary<string,string>`, `List<T>`, изменяемых DTO, массивов `PredicateIds/DomainTags/EvidenceIds` риск реален. Это касается именно **владения обученным содержимым и идентичностью evidence**, а не уже рассмотренной мутации входного вектора в HNSW, checkpoint cache alias или двойной записи одного урока.

## 2. Точная цепочка текущего C# исходника

| Ступень | Непосредственно прочитанный код | Следствие |
| --- | --- | --- |
| Обучающий тип | [`LogicalKnnTrainingExample<TInput,TOutput>`, L14–21](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L14) хранит `TOutput Output` и три `string[]` | Контракт не требует immutable `TOutput` |
| Вход Train | [`KnnLogicalUnit.Train` L136–154](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136): `exampleMap[trimmedId] = example with { ExampleId = trimmedId };` | `with` создаёт поверхностную копию record; `Output` и массивы остаются ссылочно общими с caller |
| Индекс | [`Vectorize/Index.Add` L151–154](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L151) | Индекс фиксирует вектор из тогдашнего текста/предикатов; он не пересчитывается от будущих мутаций payload |
| Обычный ответ | [`ExecuteAsync` L221–254](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L221): `var bestExample = _examples[best.id]` → `bestExample.Output` и `bestExample.PredicateIds/DomainTags` | Результат возвращает те же изменяемые ссылки. Другой потребитель результата может изменить будущие ответы |
| Альтернативный learned Unit | [`TransformerKnnTrainingExample<TOutput>` L20–26](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L20), [`Train` L~123–145](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L123), [`ExecuteAsync` L181–207](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L181) | Здесь словарь тоже хранит исходный `example`, а исполнение отдаёт `example.Output` без ownership-copy |

**Отрицательная проверка:** в указанном `Train` не видно deep-copy, а в `ExecuteAsync` — snapshot-clone перед отдачей. **Граница факта:** без чтения всех конкретных `TOutput` и downstream callers нельзя говорить, что каждое исполнение мутирует ответ или что это объясняет RP01 free generation 0/3 и 0/4. В `LogicalMetadataSnapshot` существует механизм глубоких снимков **для других метаданных** — он не доказывает, что эти `TOutput` автоматически защищены.

## 3. Соседние ветки и историческая классификация

Сверены blob тела `KnnLogicalUnit.cs`: текущая ветка `2957816a88aa9cdcd3020bb81be56b5dfff165e0`, а `language_translate`, `DesignToCode` и `external_domains` используют один и тот же более ранний blob `72887c5d27f6dffba770df01d4288258efe717cc`. В старом blob на тех же участках сохранились shallow-copy примера и возврат `bestExample.Output`. Аналогичный возврат в transformer-backed Unit также наблюдался в `language_translate`.

**Вердикт о происхождении:** это потенциальный **унаследованный контрактный недостаток**, а не доказанное изменение, внесённое RP01; если исторические положительные опыты использовали только immutable string output, они его не замечали. Отличать «ранее работало» от «перенесли и сломали» обязательно.

## 4. Какие домены уязвимы

| Домен / получатель | Условие применимости | Потенциальный ущерб | Не распространять автоматически |
| --- | --- | --- | --- |
| C# / Design-to-Code typed `ILearningUnit` | Unit сохраняет `TOutput` как mutable DTO с AST, diagnostics, patches | уже обученный вывод меняется без обучения; повторное исправление ухудшает качество | Neural Student `TextGenerator` без этого Unit |
| SQL / schema/query routing | сохранённая рекомендация или шаблон — mutable словарь/список | «память» теряет ограничения NULL/JOIN или типы | immutable query string |
| Планирование/граф/многие-ко-многим | `TOutput` список связанных результатов или mutable `DomainTags` | связь «пример → доказательство» перестаёт соответствовать фиксированному вектору | структуры, защищённые глубоким неизменяемым типом |
| EN→RU translation | **только если** конкретный flow использует этот generic Unit с mutable payload | возможно побочное изменение метаданных | обычный `ExecutorUnit` возвращает `TranslationCandidate`/string и должен исследоваться отдельно |

## 5. Дешёвый причинный опыт N37-ALIAS-01

**ExperimentId:** `N37-ALIAS-01`, **Tier:** 1, один узкий native .NET harness, 4 обучающих примера и 8 source-family-disjoint вопросов в C# и SQL; **не запускать Windows CI, не делать DPO500**. Детальное исполнение: [N37 micro protocol](37-MICRO-TYPED-UNIT-AND-SAMPLER-CAUSAL-PROTOCOL-2026-10-16.md).

- **Baseline A (действительный код):** Train с `TOutput=Dictionary<string,string>` и массивами evidence. Снять хэш выхода при Execute. После этого изменить исходный `Output["law"]` и `PredicateIds[0]` извне; повторить Execute с тем же входом без обучения. Снять `chosenId, outputHash, predicateIds, index candidate score`.
- **Enhanced B (test-only intervention):** тот же corpus, query и индекс, но объект payload глубоким снимком отделён в момент принятия урока **и** в момент передачи наружу. Никаких изменений retrieval scores или learner.
- **Negative controls:** `TOutput=string`; один и тот же immutable DTO; отсутствие мутаций; отдельный запрос с действительно новым уроком (ожидаемое изменение возможно). После получения результата A изменить `result.Output` — ещё раз запросить тот же Unit; B не должен протекать.
- **Primary metric:** `untrained_answer_change_count`; ожидается A>0 при подтверждении ссылочного пути и B=0. **Secondary:** сохранение provenance/index consistency, lawful answer rate на тех же 8 закрытых заданиях, speed/cost; признавать улучшение качества только если blind domain judge подтверждает, что изменённые ответы были ошибочны.
- **Фальсификаторы:** исполнитель не допускает mutable `TOutput`; реальный consumer создаёт defensive clone до любых мутаций; на native B/A выходы идентичны; проблема не встречается при actual RP01 consumer trace.
- **Return evidence:** source SHA, тестовый diff/команда/результат, before/after deep hashes, example IDs и output IDs, trace производственного caller, отдельный verdict C/NATIVE/H. Если generic typed Unit вовсе не вызывается на изучаемом domain, отметить `NOT_APPLICABLE_TO_RP01`.

## 6. Независимая проверка и приоритет

**P1 для typed Units с mutable result; P3/неприменимо для RP01 pure neural free answers, пока не найден call path.** Review должен спросить: «было ли новое обучение или лишь изменён объект?», «не выдаём ли safety isolation за semantic learning?», «сохранилась ли метрика на закрытом исходном домене?». Рекомендуемое исправление **только после falsifier**: контракт immutable/admission snapshot на уровне типа `TOutput` плюс defensive outgoing snapshot; не применять произвольную JSON-clone ко всем типам без схемы сериализации, не менять tokenizer, LoRA, DPO и KNN одновременно.

**Dedup:** соседний [N34 one lesson double insert](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md) — про идентификатор и повторную вставку в индекс, а не изменение `Output` после принятия. [N27 translation checkpoint cache](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) — другой recipient. [Core/51 vector-index partial state](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/51-vector-index-admission-and-partial-state.md) — vector admission, а не ownership обученной метки. Оставлено как **кандидат дополнительного различия с перекрёстными ссылками**, не доказанная новая первопричина.
