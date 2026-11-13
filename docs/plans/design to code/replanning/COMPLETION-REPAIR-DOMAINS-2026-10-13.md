# Семь заполненных доменных паспортов и способ увидеть обучение

**13.10.2026. Результат проектирования и предметного разбора. Не dataset, не выполненные эксперименты, не независимая Astra экспертиза.**

Этот файл дополняет R08/R12/R13/R21 предметным содержанием. Он не заменяет исходные passports/IDs и не объявляет закрытыми непрослеженные runtime consumers. Все приведённые A/B — **DOCUMENT_EXAMPLE, NOT_APPROVED_FOR_TRAINING**: авторские иллюстрации критериев, а не Chinese-authored DPO. Для реального урока обе стороны создаёт допущенная китайская модель, CODEX/Astra отдельно выбирает урок и оценивает фактическую пару.

## 1. Как понимать паспорта

Паспорт связывает закон, конкретную задачу, смысловое различие, представление, гипотезу kNN, получателя обновления, обычный результат и независимую оценку. Представление — не один универсальный float[]. Raw input, structured features, trainer token IDs, TeachingVector, сохранённый kNN BPE и output projection имеют разные identities и consumers.

Primary planning pins и code traces: [алгоритмический результат](COMPLETION-REPAIR-ALGORITHMS-2026-10-13.md), §2. Direct domain/source observations R08 CP3 сохраняются в `results/R08/FINDINGS.md`. Семь исходных subjects не равны пяти development intents CODE_Q/BUG/PLAN/ARCH/REFACTOR. Builder — отдельная метазадача построения урока для остальных шести.

**Источники предметных правил, проверенные 13.10.2026:**

- C# language reference, раздел Null-conditional operators: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and- . Дата просмотра не фиксирует language version конкретного проекта; она берётся из проекта перед execution.
- Angular official Signals guide, sections lazy/memoized and dynamic dependencies: https://angular.dev/guide/signals . Конкретная установленная Angular version остаётся runtime binding, не угадывается из актуальной страницы.
- PostgreSQL **18**, Table Expressions, §§7.2.1–7.2.2: https://www.postgresql.org/docs/18/queries-table-expressions.html . Диалект PostgreSQL18 фиксируется только для данного примера, не назначается всему SQL-корпусу.
- W3C Media Queries Level 4: https://www.w3.org/TR/mediaqueries-4/ . Числа viewport/breakpoint ниже — условия нашего примера, не требования стандарта.
- W3C ITS 2.0 Localization Quality Issue: https://www.w3.org/TR/its20/#lqissue-typevalues . Используется taxonomy качества перевода; правильный перевод конкретного предложения требует предметной оценки, не выводится из ITS.
- DPO, Rafailov et al., arXiv:2305.18290v3 §4 Eq.7: https://arxiv.org/html/2305.18290v3 ; LoRA, Hu et al., arXiv:2106.09685v2 §4: https://arxiv.org/html/2106.09685v2 . Это научные определения, не доказательство эффективности XIIGEN.

Внешние источники дают семантику. Они не дают автоматического разрешения загрузить их тексты в training corpus. Runtime/teacher/source rights проверяются отдельно действующим admission.

## 2. EN↔RU — отрицание, количество и временное условие

**Задача и закон.** Перевести `Do not send the 10 files before approval.` Сохранить запрет, число, объект и условие разрешения. Точное совпадение формулировки не требуется.

**A, содержательно допустимый пример:** «Не отправляйте эти 10 файлов до получения разрешения». **B, правдоподобный negative:** «Отправьте эти 10 файлов до получения разрешения». Ошибка B — утрата отрицания; стиль и длина не являются основанием предпочтения. «До согласования не отправляйте десять файлов» может быть ещё одним допустимым вариантом; пары из двух таких вариантов не получают forced preference.

**Сохраняемые tokens/features.** Unicode и направление перевода; NEG/SCOPE(send), QUANTITY(10,files), BEFORE(approval), modality; исходные entity IDs. Lowercase/общие stopwords не имеют права уничтожить not/не или 10. Синонимы можно сближать, но не противоположные действия.

**kNN hypothesis.** Eligible pool — разрешённые примеры данного language direction и вида инструкции; признаки отношения negation/object/time имеют больший обучаемый вес при совпадении риска, чем совпадение имён файлов. Сначала fixed pool/encoder/k, меняется только law-aware rerank. Negative memory объясняет потерю scope; warning запускает дополнительную проверку, не вставляет неверный перевод в answer.

**Состояние и consumer.** Translation domain model/adapter для output или отдельный domain-retrieval policy — два разных experiment recipients. В этой сессии actual translation caller не traced; известный из WorkLeft маршрут `27.TranslationDomain/TranslationDpoTrainEvaluateRunner.cs` и `TranslationDpoTrainerBridge.cs` является source route, не новым runtime доказательством. Учебный runner не заменяет ordinary translator.

**Независимые families и оценка.** Новые тексты про доступ, отгрузку и публикацию; группировка по исходному документу и семантическому шаблону, не по номеру строки. Reverse RU→EN — отдельная страта. Метрики: сохранённые обязательные смысловые единицы, critical negation errors, quantity errors, human/pinned evaluator preference, время/ресурсы. OOD: идиома, отсутствующий referent, смешанный язык; uncertainty и вопрос допустимы. Code switch сам по себе не означает неправильный domain.

## 3. Orchestration — частичный порядок и many→many

**Задача и закон.** Обработать A и B: каждый `send(X)` допустим только после успешного `verify(X)`. A и B независимы; общий бюджет и cancellation заданы входом. Закон здесь — явный task contract, а не статистика наиболее частого порядка.

**A:** verify(A) и verify(B) запускаются независимо; send(A) только после успеха verify(A), send(B) после успеха verify(B). **B:** verify(A)→send(B), verify(B)→send(A). Negative сохраняет красивую форму DAG, но теряет association. Два lawful interleavings — не обязательно разные по качеству: при равной цене это TIE.

**Tokens/features.** Input IDs, port types, prerequisite edges, data-dependency edges, success/cancel/failure states и cost budget; partial order, не только sorted list action names. Completion timestamp не задаёт semantic order. Пустой output после отказа не должен сдвинуть zip-связь соседнего входа.

**kNN hypothesis.** Искать изоморфные typed dependency patterns, не идентичные имена A/B. Domain/risk-compatible pool; hard-invalid plan отбрасывается независимо от similarity. Дополнительные соседи дают разные lawful compositions/evidence; learned selector выбирает при fixed solver outcomes и budget. Нельзя ранжировать с доступом к будущему outcome, которого ordinary policy ещё не знает.

**Состояние и consumer.** `theta_flow` учит associations/corrections; `theta_route` — async composition choice. Это проектируемые state families. R07 уже атрибутированно нашёл L2-authority→L3-flow execution; accepted state→preload ещё не закрыт. Новое прямое чтение `DistributedModelOrchestratorFlow.OrchestrateAsync` показывает routing result, не specialist generation. Следующий owner должен проследить actual selected executor, а не принять endpoint label за output.

**Экзамен.** Новые DAG с 3–5 сущностями, fan-out/fan-in, одним failure, пустой ветвью и разным временем завершения; целые topology families удерживаются вне уроков. Метрики: valid association edges, hard-order violations, lawful completion rate, время до правильного результата, стоимость всех branches, включая отменённые. Опыт Flow фиксирует Units; опыт Orchestrator фиксирует Unit+Flow. Выигрыш быстрого solver не приписывается selector.

## 4. Figma/design→HTML/CSS — структура и несколько viewport

**Задача и закон примера.** Исходное дерево содержит заголовок и четыре карточки; при ширине 360 CSS px — одна колонка без горизонтального overflow, при 1280 CSS px — две равные колонки. Порядок, текст и доступность содержимого сохраняются. Breakpoint=768 CSS px — условие этого задания. Важна совместная пригодность на всех заданных viewport, а не одна похожая картинка.

**A:** responsive layout, удовлетворяющий обоим случаям и исходному дереву. **B:** fixed-width 960 px layout, красивый на desktop, но обрезающий карточки на 360. Альтернативные CSS Grid/Flex решения допустимы, если выполняют тот же контракт. Нельзя предпочесть один framework без требования.

**Tokens/features.** Node IDs/parent-child edges, order, size/units, layout constraints, breakpoint/viewport, text/images, overflow, geometry, accessibility semantics. Отдельные shape и content channels; нельзя нормализовать 360 и 1280 в одинаковые «числовые токены» без значения.

**kNN hypothesis.** Pool ограничен разрешёнными дизайн-источниками и совместимым layout contract; score включает structure/constraints/viewport response, а не только screenshot appearance. Нужны neighbors разных viewport одной design family как связанные observations, не независимые учебные семьи. Negative/warning помечают overflow/hidden-content риски. Новый renderer не предлагается: переиспользовать текущую domain implementation после проверки.

**Состояние и ordinary consumer.** Design conversion model/adapter создаёт HTML/CSS; evidence/routing policy — отдельный получатель. Прямо прочитанный `task prompt.md` требует полную цепь client export→platform API→model API→multi-resolution renders→quality/user-tier budget→DB→lawful pairs→training→publication→ordinary conversion. R08/WorkLeft указывают `44.DesignConversionDomain`; конкретный accepted checkpoint adoption всех API этой сессией не доказан.

**Экзамен.** Скрыть целые layout/component families, не варианты одного макета. Сравнивать structure preservation, overflow/missing content, task behavior и blinded visual judgment при одинаковых viewport/fonts/assets/render policy. Время включает рендеры и неудачные улучшения; cost cap user tier сохраняется. Нет полного браузерного e2e gate перед unrelated CodeDocumentation lesson; для DTC эти рендеры — предметное наблюдение, не процент coverage.

## 5. C# documentation — null не равен нулю

**Задача:** объяснить `int? Size(string? s) => s?.Length;`.

**A:** при null возвращается null; при непустой/пустой строке — её Length, в том числе 0 для пустой. **B:** при null возвращается 0. Ошибка B — подмена nullable результата default value. Доменное основание — Microsoft null-conditional semantics; `?.` не подавляет любую ошибку у ненулевого объекта.

**Tokens/features.** `?.`, `??`, nullable types, literal 0/null, return type, expression tree, branch/scope и exact code spans. `s?.Length` и `s?.Length ?? 0` — контрфактические исходники разных задач, не две стороны одной DPO-пары. В паре обе стороны объясняют один frozen source.

**kNN hypothesis.** Concrete candidate полностью заполнен в алгоритмическом результате §7: eligibility, seven features, query-dependent score, fixed-k first comparison, bounded next k-policy, typed evidence aggregator и exact/ANN separation. Нормализация операторов в общие слова создаёт потерю определяющего различия.

**Состояние и consumer.** Для первого E1 — existing CodeDocumentation LlmModel/adapter по current WorkLeft-02. `LogicalDpoTrainingUnit.BuildBatch` и `DpoTrainer.StepVector` непосредственно прочитаны; ordinary generation entry и actual checkpoint ещё не связаны. Для E2 — отдельный learned reranker; его gain нельзя назвать выполненным нейронным S04 уроком.

**Families.** Учебная слабость — nullable propagation в одном разрешённом источнике. Development families — иные методы/вызовы. Confirmation — другие provenance families: цепочки доступа, nullable value returns, coalescing; переименования одного snippet остаются одной family. Границы: getter throw, parenthesized chain, side effects; неверное обобщение «?. ловит исключения» должно быть обнаружено. Метрики: доля подтверждённых semantic claims, critical false claims, полнота null/non-null cases, source binding, ordinary answer quality/time.

## 6. Angular documentation — реально прочитанная зависимость

**Задача:** `enabled=false`, `n=1`, `v=computed(()=>enabled()?n():0)`. После первого чтения v изменить только n, не меняя enabled. Объяснить зависимость и момент вычисления.

**A:** эта ветвь не читала n; n не стал её tracked dependency. При enabled=true и следующем чтении derivation читает n, после чего изменение n инвалидирует кеш. **B:** любые изменения любого объявленного signal немедленно пересчитывают все computed. Negative путает lexical occurrence, tracking, invalidation и lazy recomputation.

**Tokens/features.** Reads/writes, branch guard, active dependency graph, signal identity, computed/effect distinction, first/subsequent read и framework version. Нельзя превратить branch-specific dependency graph в множество всех имён файла.

**kNN hypothesis.** Domain=Angular, intent=CODE_Q; framework-compatible examples с тем же dynamic-dependency motif выше lexical matches про другой reactive mechanism. Дополнительные neighbors дают counterfactual branch conditions, а не большинство голосов по common word signal. State — documentation adapter или relation-aware reranker, отдельно по эксперименту.

**Ordinary consumer.** Existing CodeDocumentation Angular track упомянут в MECH/R08; фактический current generation caller этой сессией не traced. Нельзя считать вызов учебного evaluator обычной IDE-документацией.

**Экзамен.** Незнакомые component/function families, добавление и удаление dependency, nested computed, изменение только inactive branch signal. Версия runtime фиксируется до задания; не переносить документацию новой версии на старую без сверки. Метрики: правильность dependency set и explanations, различение invalidation/recompute, correctness на обычном source question, latency/retention. Код не исполнялся.

## 7. SQL documentation — WHERE после LEFT JOIN меняет результат

**Задача:** customers содержит id=1,2; orders содержит единственную строку `(customer_id=1,status='paid')`. Сравнить запрос A с `LEFT JOIN orders o ON o.customer_id=c.id AND o.status='paid'` и запрос B с `LEFT JOIN orders o ON o.customer_id=c.id WHERE o.status='paid'`.

**Правильное объяснение:** A сохраняет customer2 с NULL right columns; B удаляет его фильтром. **Negative:** оба запроса всегда сохраняют всех customers, потому что написано LEFT JOIN. Это конкретное следствие заданных данных и PostgreSQL18 join/WHERE semantics, не performance guess.

**Tokens/features.** Join type, preserved side, predicate location, NULL/three-valued logic, aliases, cardinality, keys, actual SQL dialect/version. Лексические слова почти совпадают, но расположение predicate определяет результат. AST/relational graph сохраняет ON/WHERE.

**kNN hypothesis.** Eligible same-dialect relation-pattern examples; learned score учитывает null-rejecting predicate, optional side и preserved rows. Neighbors про INNER JOIN не становятся положительными из-за одинаковых слов. Warning memory указывает риск потери unmatched rows. State — SQL documentation adapter или отдельный retrieval policy.

**Ordinary consumer.** Existing C#/Angular/SQL CodeDocumentation curriculum по MECH/R08; concrete SQL caller/checkpoint требует runtime binding. Одна generic routing confidence не доказывает SQL понимание.

**Экзамен.** Независимые схемы с unmatched rows, many matches, nullable status, predicate `IS NULL`, дополнительные filters. Dialect-specific differences фиксируются. Оценка по semantic result/cardinality/source explanation и latency; query text не обязан совпадать. Как собственное аналитическое следствие здесь ожидаются 2 rows у A и 1 у B; SQL engine не запускался, это не runtime record.

## 8. Builder — пара должна быть пригодной и полезной, а не просто контрастной

**Задача.** По разрешённой слабости «путает ON/WHERE при LEFT JOIN» подготовить новый урок на другом разрешённом исходнике; не использовать закрытые exam answers. Выбрать recipient SQL documentation, а не graph selector, если учится объяснение SQL.

**Хороший результат Builder.** Один frozen prompt/source, две правдоподобные candidate explanations; B имеет одну конкретную ошибку относительно source; provenance соответствует фактическому автору; judge независимо определяет winner и pair_usable. **Плохой результат:** две равноценные правильные формулировки с насильно заданным chosen, бессмысленный rejected, неверный recipient или копия reference без права использования.

**Tokens/features.** Domain+intent+task/source identity, candidate semantic claims, contrast type, permissible context, recipient family, reason/evidence; separate author label и judge verdict. Служебное имя chosen/rejected не должно быть подсказкой student или pair judge. Lossy truncation после judgment аннулирует доказательство пригодности.

**kNN hypothesis.** Memory полезных педагогических ошибок, curriculum gaps и rejected lesson reasons, отфильтрованная по source rights/domain/recipient. Similarity со старым уроком не делает повтор новым независимым семейством. Candidate attempts учитываются полностью.

**Состояние и consumer.** Собственный Builder ILearningDynamicGraph — целевая способность исходной задачи. Первый этап использует admitted Chinese author; вывод этого документа не обучает Builder. Builder recipient и downstream Student recipient различаются. Замерить отдельно качество созданных пар и эффект этих пар на Student.

**Экзамен.** Новые слабости в разных subjects; meaningful chosen, author-inverted candidates, TIE, BOTH_BAD, UNJUDGEABLE, identical-after-projection, conflicting feedback и forbidden source. Метрики: доля usable/admitted среди всех attempts, типы ошибок, разнообразие независимых families, cost/time-to-usable-pair, затем downstream gain на других задачах. Рост pass rate judge без Student utility не доказывает улучшение Builder.

## 9. Дополнительные направления и область всей программы

HE↔EN сохранено как discovered additional direction из R01 AD01/первичной карты core; обязательность конкретного текущего curriculum и ordinary adapter не доказана одним locator. R02/R21 сохраняют direction+source+owner, R04/R08 — Unicode/scope/semantic representation и evaluation. Другие найденные language/code/stored-procedure/IDE task families остаются в исходных U12/AD02 и full-delivery DAG; семь паспортов не объявляются всем scope.

При неизвестном/смешанном domain полезны multi-label assignment и отказ от ложной уверенности. Сравнение oracle-domain vs predicted-domain диагностирует mapper; oracle hints не подаются actual production arm. Hard-law filters дополнительно оцениваются на false exclusions законных кандидатов, иначе «повышение accuracy» может быть только удалением сложных задач.

## 10. Первый документно заполненный E1 recipe — не готовый runtime launch

**Объект:** current CodeDocumentation Student, C# explanation task; конкретный материал из §5 — пример рубрики, не pair для обучения. **Гипотеза:** после одного нового допустимого урока с сохранённым contrast и response-only objective Student точнее объясняет nullable behavior на других source families, а не только узнаёт текст урока.

**Предпосылки:** заполнены actual parent/checkpoint/tokenizer/config, trainable key set, approved author/judge/evaluator profiles и ordinary generation entry. Эти значения не наблюдались; статус `RUNTIME_BINDING_OPEN`. Ниже заполнен метод, а не выдуманные IDs/команда.

**Порядок.** Сначала исходный ordinary answer и fixed operational panel на parent. Проверка допустимости нового отдельного урока; one-pair snapshot; один update; isolated challenger evaluation той же панелью; retain/promote/rollback; затем cold ordinary consumer выбранного состояния. Нельзя сначала объявить child accepted, а потом искать подтверждающий эффект. Next bulk запускает новые уроки, не повторяет старые.

**Контроль.** Parent/no-update и challenger получают одинаковые разрешённые evidence, candidate pool и prompt policy. Учебный target не добавляется в память только challenger. Требуемый stable response-objective/material contract фиксируется в обоих arms; сравнение «исправленная objective против старой» — отдельное исследование, не смешивается с основным before/after одного урока. При one-pair/no-repeat не дублировать обучение той же pair в разных branches без отдельного разрешения.

**Оперативные family strata:** nullable propagation; short-circuit scope; coalescing distinction; вызовы со side effect. Порядок/имена и paraphrases одной provenance family не считаются независимыми. Confirmation families отделены от operational families до authoring; показываемые здесь examples нельзя потом объявить секретным final exam. Замороженный operational panel пригоден для решения promote, но после многократного использования перестаёт быть независимым подтверждением общего качества.

**Предметная рубрика.** Каждому ответу присваиваются: правильный return/nullable semantics; branch/side-effect explanation; подтверждение source; material false claim; достаточность ответа. Вес/шкала и practically-useful threshold принимаются до challenger на pilot calibration и действующем noise-aware policy; этот документ не заменяет их придуманными 95% или фиксированным количеством заданий. Если calibration и cost budget отсутствуют, final launch binding не завершён.

**Данные измерения.** `bulk_id, family_id, task_id, domain/intent/direction, arm, parent/challenger, representation/index, trained_keys, ordinary_version, quality, critical_errors, elapsed_time, total_attempt_cost, peak_RAM, peak_VRAM, disk_growth, retention, verdict_reason`. Записывать время baseline/evaluation/authoring/judging/update/adoption раздельно. В denominator входят неудачные попытки.

**Статистическая интерпретация.** Сравнивать paired deltas по одинаковым задачам внутри bulk; неопределённость считать по независимым families, не по числу tokens. Не смешивать более лёгкий новый bulk с ростом навыка; для межпартийной кривой нужен стабильный anchor и domain-stratified reporting. Размер панели/число lessons выбирать по точности, бюджету и stop policy до результата; не бесконечный optional stopping. Multiple exploratory contrasts не объявлять одной подтверждающей проверкой.

**Исходы:** IMPROVE — практически значимая польза при допустимых costs/retention; REGRESS — сохранить parent; INCONCLUSIVE — сохранить parent и зафиксировать недостаточную точность/эффект; INVALID_EVIDENCE — исправить нарушенный эксперимент. Успешный механический update, valid negative experiment и доказанное обучение — разные статусы. Astra opinion исследует смысл, но не заменяет эти наблюдения.

## 11. Приёмка этого документа и следующий адресный остаток

Заполнены семь предметных цепочек и один E1 protocol. Sources подкрепляют semantic rules; выбранные features/kNN/projection и family designs остаются гипотезами. Не закрыты реальные material manifests, все ordinary callers, принятые weights, trainable key sets и значения budget/noise thresholds. Это исполнимые инженерные/материальные bindings для R05/R13/R18, не повод снова составлять пустую форму семи паспортов.

R08 принимает §§2–9 и дочитывает только необходимые consumer seams/уникальные research-review deltas. R12 принимает mechanistic connections из алгоритмического файла. R13 оформляет exact binding и preregistered criteria для выбранного реального lesson. R21 расширяет curriculum; R22 проверяет взаимодействия; R23 отвечает за полный PC/feedback cost. Никакие workers statuses этим документом не переписаны.
