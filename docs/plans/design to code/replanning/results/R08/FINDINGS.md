# R08 — CP3: что исправлено и что действительно установлено

**13.10.2026. PROMPT_REPAIR = REPAIRED; RESEARCH_COMPLETENESS = IN_PROGRESS; LEARNING_GAIN = NOT_MEASURED; INDEPENDENT_REVIEW = NOT_PERFORMED.**

Действующий полный [R08 packet](../../run-prompts/R08-PARALLEL9.md) опубликован в `968760498b9a8c4186f5442ee93f084f44412bcd`; [краткий вход R08](../../run-prompts/R08.md) направляет в него. Это исправление поручения R08 и адресный анализ, не выполнение всей программы, не полный domain/code audit и не реальный урок.

Полный прежний [FINDINGS CP2](https://github.com/lubochka/xiigen-planning/blob/7cbf1a744d009af57b8ac47c77442f900a8113ee/llm%20mvp/design%20to%20code%20session/replanning/results/R08/FINDINGS.md) сохранён в Git; его уникальные требования сопоставлены с CP3 в DECISIONS/CHANGE-MAP. Исходные источники, чужие результаты и core не переписывались. Исторические строки SOURCES остаются историческими receipts, а не свежим чтением CP3.

## 1. Вердикт предыдущему R08

CP2 был **полезным, но неполным ремонтом**, а не завершённым исследованием. Он сам оставлял IN_PROGRESS; объявлять это заведомо ложным заявлением DONE было бы неверно. Его сильные решения сохранены: четыре роли, hybrid boundaries, правильные author/assessor роли, independent families, one-pair/no-repeat, full-PC cost, retention и continued feedback.

Подтверждённые проблемы:

| Проблема | Evidence | Исправление |
|---|---|---|
| FINDINGS преимущественно предписывал будущую работу, вместо заполненных предметных результатов | Прежние FINDINGS §§6–13; SOURCES code anchors отмечены TREE_DISCOVERED | Отдельно показаны реальные code observations ниже и model-ready предметные seeds; research не закрыт названием repair |
| DECISIONS CP2 требовал строить passports только после A/B, причём B повторял почти всю domain→DPO→LoRA→consumer архитектуру | Прежние DECISIONS R08-B/C | CP3 A/B начинают по primary; C/D адресно проверяют различающие стыки; полный механизм остаётся у R04–R07 |
| Указанные семь предметов имели общие поля, но не семь конкретных source/rule/contrast/metric наборов в executable packet | Прежний R08-PARALLEL9 A/B | CP3 B1–B7 содержит отдельные примеры, источники семантики, preserved features, kNN hypotheses, ambiguity и metrics |
| Перечень DPO verdict не выражал случай, когда rejected действительно лучше; формулировка «exact projected pair» могла смешать семантику и lossy vector | Прежние DECISIONS R08-D | CP3 D: REJECTED_BETTER отдельно, no silent swap, exact text/context плюс projection binding; pair validity не Student gain |
| Старые skills содержат конфликтующие replay/intent/test-count/escalation правила | experiment-design и improvement-measurement, свежие чтения CP3 | CP3 §2 явным образом сохраняет текущую предметную authority и не запускает эти старые loops |
| First/second domain в прежних prompts выбирались слишком свободно | R08.md / R08-PARALLEL9 против current WorkLeft-02 | Existing CodeDocumentation Student и admitted curriculum сохраняются; остальные предметы не исчезают, но не блокируют первый урок |

**Уточнение промежуточной диагностики:** фактический dispatcher уже ссылался на `run-prompts/R08-PARALLEL9.md`, а `R08.md` уже содержал CP2. Поэтому проблема не в полном отсутствии предыдущей правки стартовых файлов. Проблема — в недостаточной конкретности/согласованности нескольких R08 work orders. Теперь оба входа указывают на один полный CP3 packet; не требуется ещё одна самостоятельная книга инструкций.

## 2. Что найдено непосредственным чтением кода

Core ref: `3c7f5f8c7809936e25579603c96ea6df8598fc40`. Следующие выводы — **статический анализ методов**, не запущенные тесты, не measurement и не доказательство использования каждого метода всеми learner paths.

### F-C01. Raw vector helper может уничтожить предметное различие

`23.SemanticCompilerAdapter/TrainingVectorBuilder.cs` прочитан целиком. `NormalizeRoots` разделяет текст по `[^a-z]+`, удаляет в том числе `before/after`; `Build(string)` формирует root vocabulary из текущего prompt, vocabulary ограничен `Take(32)`.

При прочих одинаковых признаках это даёт конкретные логические контрпримеры для raw helper: `send 10` / `send 20`; `send before verify` / `send after verify`; `да` / `нет`. Смысл числа, порядка или русского ответа различается, а названная нормализация это различие убирает. Per-prompt coordinates также требуют проверки совместимости между разными вызовами.

**Граница:** compiled/supplied-vocabulary overloads и loss-visible token path могут вести себя иначе. Ordinary caller данного CodeDocumentation Student здесь не traced. Нельзя заключить «вся модель не различает русский» или «DPO loss всегда нулевой». Передача R04/R07/R12: установить применимость к конкретному consumer, затем выбирать migration или эксперимент.

### F-C02. Development intent classifier не заменяет предметный mapper

`23.SemanticCompilerAdapter/DomainClassifierUnit.cs` прочитан целиком. Набор CODE_Q/BUG/PLAN/ARCH/REFACTOR/UNKNOWN — development intents. В class используются heuristic confidence adjustments, в частности высокий minimum при успешном trained predicate kNN.

**Следствие для дизайна измерения:** confidence .90/.95 в этом классе не является измеренной точностью 90/95% и не доказывает mapping семи предметов. SQL+BUG должен сохранять независимые subject/intent coordinates. Наличие другого слоя domain mapping не исключено; нужны его actual caller/consumer. Передача R06/R12: mapping отдельно от conditional solver и end-to-end качества.

### F-C03. Inspected kNN unit ещё не доказывает query-adaptive k

`21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` прочитан в пределах строк 1–240: Train/ExecuteAsync и относящиеся поля. `_k` фиксируется у экземпляра; ExecuteAsync вызывает `_index.Search(query,_k)` и использует ближайший hit для output. Это не показанный per-query learned k-policy и не показанная learned arbitration нескольких результатов.

**Граница:** не прочитан весь остаток файла и индекс/внешние consumers. k может влиять на поведение ANN search; нельзя утверждать «изменение k вообще не влияет на результат». kNN memory learning не равно gradient DPO, но и не является «совсем не обучением». Передача R06/R07: fixed/dynamic k при одинаковом legal pool, metric отдельно, cost и meaningful neighbor utility.

## 3. Первичные решения, которые возвращены в критерии

Прямое чтение original/task prompts восстанавливает typed many→many Unit, correction Flow, asynchronous Orchestrator, higher-order Graph; multiple combinations одного input — не одно лучшее совпадение. DTC включает API/модель, tree/HTML/CSS/images, несколько разрешений, оценку и стоимость user tier, БД, обучение и обычное применение результата. `task prompt.txt` требует сохранять модель/DPO и продолжать работу из совместимого состояния, а не начинать заново.

RAG-MASTER active section → WorkLeft-02 current instruction пройден до первичного текста. Это подтверждает существующий CodeDocumentation Student, separate Teacher/Evaluator, actual parameter update и ordinary checkpoint adoption как разные требования. Полный обход остальных RAG branches не заявляется.

WorkLeft-02 редакции 02.10.2026 уточняет S04/S06/S07: noise-aware criterion вместо голого порога 95, отдельные conclusions для механизма/урока/качества/применения; S04 local evidence и S05 remote publication не смешиваются. Историческая цель RESEARCH-01 95%+ к эталону сохраняется как цель программы. `NO_TRAINING_NEEDED`, identical weights и полноценный отрицательный результат процедуры не выдаются за доказанное улучшение.

Current RUN §§2.1–2.2, прочитанные напрямую: CODEX GPT-6-Astra/ultra выбирает разрешённый урок, external admitted Chinese author создаёт обе final sides, CODEX отдельно оценивает exact pair; Student outcome Evaluator — отдельная фиксированная роль. Полный §2.3 в этой сессии не заявляется прочитанным. CP3 требует дочитать применимые state/authority details перед реальным runtime нарядом.

## 4. Семь конкретных предметных examination seeds

Полные task/positive/hard-negative/features/kNN hypothesis/metric/family/edge находятся в CP3 B1–B7. Это **авторские документные примеры**, не реальные пары, не разрешённые dataset artifacts и не результаты запусков.

| Предмет | Различающий пример | Что этим проверяется |
|---|---|---|
| EN↔RU | Сохранить `not`, 10 и before approval; fluent negative теряет отрицание | Meaning/quantity/temporal scope, Unicode, semantic alternatives |
| Orchestration | verify(A)→send(A), verify(B)→send(B), а не send→verify | Partial order, typed many→many associations, asynchronous choice и полная цена |
| DTC | Заданный layout пригоден на 360/1280 CSS px, а не только fixed-width desktop картинка | Hierarchy/geometry/units, multi-viewport fidelity и functional consumer |
| C# | `s?.Length` при null даёт nullable null, не ноль; ?. не подавляет всё подряд | Exact code semantics, typed/control-flow representation, factual explanation |
| Angular | Не прочитанный в текущей computed-ветви signal не отслеживается этой ветвью | Conditional dependencies; invalidation отдельно от eager recomputation |
| SQL | Filter в ON против WHERE после LEFT JOIN на схеме с unmatched customer | NULL/cardinality/position semantics; красивый LEFT JOIN текст недостаточен |
| DPO Builder | Новая содержательная пара по weakness против двух равноценных ответов или leaked reference | Useful preference, source/recipient/admission и дальнейшая Student utility |

Первичные внешние источники семантики указаны в packet и SOURCES: Microsoft C#, Angular Signals, W3C ITS/Media Queries, PostgreSQL 18, DPO/LoRA papers. Они не заменяют реальные user task contracts и разрешения на training data. Полные domain passports с actual runtime consumer и family manifests ещё не готовы.

## 5. Приоритет и готовность первого урока

Первый Student не выбирается заново: current WorkLeft-02 указывает existing CodeDocumentation learner. R08 готовит рецепт для этого learner и сохраняет все остальные предметы в curriculum. Только реальный source/caller trace может установить smallest missing seam; в этом ремонте новый исполняемый trainer entry или готовый command не выдуман.

Проверка улучшения: frozen parent/no-update control → один разрешённый новый урок → independent family panel → accept/restore → reopen ordinary consumer. Следующие bulk состоят из **новых последовательных уроков**, не replay всех pair IDs. Измеряются domain mapping, semantic accuracy, path/association validity, время до полезного урока и принятого улучшения, all-attempt cost, whole-system RAM/VRAM/storage и retention. Полезность hybrid определяется при сопоставимых ресурсах, а не уменьшением одного adapter файла.

LoRA, tokenizer, domain-vector projector, kNN metric и output projection — разные механизмы. Нужна фактическая связь loss/state→ordinary output; наличие папки LoRA или изменение hash этого не доказывает. CP3 D/E задаёт проверку, но такого consumer trace для всех предметов здесь нет.

## 6. Честный остаток

- `09.09.26WorkLeft-RESEARCH-01-REVIEW-20-passes.md`: contents API вернул пустой content при существующем blob `3def99dd83bc6c9728d0c5e97dede47aa3ae6a82`; raw fetch сообщил too large/unsupported. Содержательное чтение и перенос его уникальных review deltas **не выполнены**. R08-A сохраняет адресный остаток; источник не объявлен пустым или ненужным.
- `previous rounds thinking results.md`: свежо прочитано начало запрошенного диапазона 1–120, не весь большой архив. Полная история R08 утверждений не заявляется исследованной.
- Остальные обязательные доменные RAG primary, DTC-specific первоисточник, соответствующие playbook/hybrid sections и actual dataset/permission/family manifests требуют адресного дочитывания. Прежние CP2 receipts не переименованы в свежие CP3.
- Ordinary caller/adoption, actual DPO loss/projector/LoRA recipient и state coherence не прослежены для всех предметов. C0 observations — ограниченный подтверждённый subset.
- Teacher/judge/Student training, bulk outcomes, PC fit, независимый Astra review **не выполнялись**. Отсутствие результата не равно нулевому эффекту или отрицательному вердикту.

Ни один такой остаток не даёт основания повторять всю программу или ждать DONE всех соседей. Каждый направляется точному владельцу по packet A–E; research-completeness остаётся IN_PROGRESS. Плановый ремонт и его GitHub публикация при этом выполнены отдельно от исследования и runtime.
