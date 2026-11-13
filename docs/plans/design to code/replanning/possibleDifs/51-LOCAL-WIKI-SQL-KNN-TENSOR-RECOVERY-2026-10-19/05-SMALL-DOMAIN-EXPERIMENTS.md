# Малые предметные опыты: сначала восстановление local_wiki

**19 октября 2026. PLAN / NOT RUN.** Это постановка будущих проверок. Здесь не вызывались внешние модели, native trainer или генератор ответов. Donor W — `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`; current C — `4945b8e85c17b5f150eb5b840d16bbdc4b506242`. Сначала проверяется конкретное утраченное поведение W, затем независимые улучшения. Общая карта — [восстановление Wiki](00-LOCAL-WIKI-RECOVERY-FIRST.md).

| Очередь | Один вопрос | Получатель и граница |
|---|---|---|
| 1 | **AC-SQL01:** помогает ли вернуть условное SQL-представление? | Wiki metric learner; влияние на обычный ответ требует собственного consumer сохранённой policy |
| 2 | **AC-EV02:** видит ли AnswerComposer разрешённое evidence? | Вход neural policy; отдельный опыт, без смены SQL curriculum |
| 3 | NK-01/02/03: сохраняется ли полезный retrieval у настоящего потребителя? | AST-индекс или отбор skills; это изменение поиска, обычно 0 neural updates |
| 4 | Известный контраст `lm_head` и q/v-LoRA | RP01 neural Student; сначала C# **или** EN→RU, отдельный совместимый baseline |
| Условно | `Mean`/`MatMul` на проблемных формах | P2, только после подтверждения вызывающего кода; оба свойства унаследованы от W |

История SQL 110–112 с preference 47/50 обосновывает первую очередь, но не приписывает успех одной строке кода: менялось состояние, а 111/112 повторяли один корпус. Это не 47 правильных свободных ответов. История, точные условия `signal=true`/`stored_procedure_answer` и границы новизны сохранены в [SQL-досье](02-NEW-SQL-UNIT-GAPS.md). В [прежнем дополнении 49][prior49] уже есть планы «два урока»: здесь они не объявлены новыми и не подменяют контракт настоящего Wiki runner.

<a id="roles"></a>
## Роли: автор пары, выбор и экзамен разделены

**CODEX с запрошенной конфигурацией Astra 6 ultracode выбирает задачу, допустимые источники, предметную истину и материал. Внешняя открытая китайская модель пишет ОБА окончательных текста пары.** Отдельный CODEX judge рассматривает exact prompt/evidence и exact candidate bytes, проверяет предложенное предпочтение и принимает пару либо возвращает `tie`, `both_bad`, `needs_info`. Эти три исхода не становятся учебной парой. Если предпочтение автора не подтверждено, пара возвращается на переработку до фиксации; окончательные стороны после приёмки не переименовываются.

CODEX может принять или отклонить пару либо заказать повторную генерацию. Он не дописывает маркер и не исправляет «одно слово» в принятом candidate, сохраняя чужое авторство. Для SQL A/B внешний автор возвращает `chosenA`, `chosenB` и общий `rejected`; judge принимает обе полные пары и проверяет, что различие положительных сторон ограничено разрешённым представлением одних фактов. Не получилось — regenerate. Для curriculum выбираются уже принятые пары.

После обучения другой сеанс judge оценивает **собственные ответы исследуемого consumer вслепую**, по заранее установленной истине. Ему не показывают плечо, checkpoint, training label, teacher answer или предпочтение предыдущего judge. Независимость означает раздельные задания и доступ к данным, а не обязательного второго провайдера. Фактические provider/model/version/settings и hashes сохраняются. Если запрошенная конфигурация недоступна, пишется точное ограничение; другая модель не называется Astra 6 ultracode. У этого документа нет выполненных вызовов такой конфигурации.

## Общий причинный протокол

**Нулевой этап — 0 updates.** Назвать learner, изменяемое состояние и обычный reader. Зафиксировать код, tokenizer, policy/reference, trainable tensors, LR/DPO beta, optimizer/RNG, decoder и фактические входы. Проверить, что изменение достигает нужного получателя. Наличие данных в JSON не доказывает наличия соответствующих token IDs. Для восстановления открыть сохранённый кандидат отдельным экземпляром, а не повторно использовать живой объект.

Во всех сравниваемых плечах одинаковы исходные policy/reference bytes, доступный optimizer state, порядок уроков, seeds, число предъявлений и decoder. Выбирается совместимый общий старт; обязательного исторического parent нет. Если старый формат сохраняет только веса, оба плеча получают явно одинаковые свежие Adam moments/step: это не полное возобновление исторического обучения. Отсутствующий binary или reader отмечается `invalid/nonreached`, а не заменяется receipt либо готовым ответом.

**TRAIN, preference HELDOUT и ответный экзамен разделяются по независимым source families.** Переименование одного метода или копия одной процедуры не создают новую семью. SQL-семья включает процедуру, её версии, схему и .NET callers; все связанные версии остаются внутри одного split. Истина извлекается из замороженных source/evidence до ответов модели. EXAM не используется для выбора пар, коэффициентов, порядка или числа эпох.

Минимальная ответная панель — **8 случаев**: четыре новых предметных контраста, включая отрицательный; два случая неизвестного/неполного evidence; два ранее правильно решавшихся случая для retention. Если прежняя правильность не подтверждена baseline, последние два называются контрольными, а retention — неизмеренным. На каждом плече сохраняются ответы до обучения, после него и после cold load: **до 24 ответов на плечо**. Для общего идентичного baseline возможен повторный use сохранённого результата с явным учётом этого сокращения.

Сравнивать изменение относительно собственного baseline, затем разность изменений плеч. Preference, correctness ответа, критические ошибки, abstention и retention записывать раздельно. Обычный структурный Wiki composer может дать правильный ответ без чтения metric policy; такой результат получает `NOT_APPLICABLE_TO_THIS_RECIPIENT` для заявления о DPO. Reader/candidate hash и реально переданный контекст должны позволять это различить. [Граница pipeline][composer-pipeline], [обычный composer][ordinary-composer].

## Готовая постановка опыта: AC-SQL01, сначала A00 против A10

**Вопрос:** улучшает ли адресное представление SQL table effects усвоение при неизменных уроках? Исторический `BuildSignalChosen` добавлял условные признаки, current сохраняет count-признаки, но удаляет этот блок. Проверяется именно `signal=true` и `stored_procedure_answer`, а не все SQL/.NET ответы. [W код][sql-w], [C код][sql-c].

```text
WORK ORDER SQL-REPRESENTATION-01 — будущий изолированный опыт.

1. Раздели два вида материала.
   R: immutable source/архивные signal-строки — только реконструкция
   различий и zero-update preflight. Это не новые принятые DPO-пары.
   Q: новый quality pilot — окончательные candidate bytes пишет внешний
   автор, затем отдельный CODEX judge принимает каждую exact пару.
   Исторический deterministic BuildSignalChosen не автор Q.

2. Заморозь source panel и split manifest. Подготовь 50 TRAIN и 50 HELDOUT
   разных строк; не размножай два урока до 50. TRAIN: по 10 случаев
   table effects, result shape, parameters, callers, procedure-name-only.
   HELDOUT одинаков для обоих плеч, из других source families.
   У каждого gold-факта есть source location и граница полноты evidence.
   Пустой SqlTablesWritten не равен доказанному read-only без проверки SQL.

3. Внешний автор пишет для каждого TRAIN chosenA/chosenB/common rejected.
   A00: нынешнее представление с общими и числовыми признаками.
   A10: те же байты вне выделенных полей, плюс условные признаки
   соответствующего аспекта, согласованные с donor 111/112.
   Факты, числа, имена, правильность и общий rejected одинаковы.
   Не добавляй глобальные persistence-маркеры ко всем типам вопросов.
   Judge принимает обе пары и разрешённый diff; при иной правке regenerate.
   HELDOUT получает одну общую внешне написанную factual пару на строку,
   без названия плеча. Заморозь exact prompt/chosen/rejected, actual IDs,
   masks и scorer для обоих плеч. Нельзя переключить BuildSignalChosen
   на всём pipeline вместе с экзаменом: representation меняет только TRAIN.

4. До обучения сохрани actual model-visible prompt/chosen/rejected IDs,
   masks, counts и hashes после штатной подготовки обоих плеч.
   Prompt projection, tokenizer, prefix layout и response budget одинаковы.
   Подтверди видимость нужных признаков и отсутствие потери смыслового
   хвоста. Если новый знак отсутствует либо изменилась скрытая truncation,
   zero-update этап объясняет ограничение; matched quality A/B не готов.
   Не добавляй evidence в prompt одновременно с representation.

5. Сделай независимые копии общего policy/reference/state. Запиши actual
   путь ProjectWikiAnswerComposerTrainingPipeline → Wiki metric runner →
   native DpoTrainer → candidate → reader. Для нового Q используй
   подтверждённый author-preserving ingress, описанный после задания;
   default fixed-generator pipeline не принимает его автоматически. Проверь consumer
   до обучения. Если он не читает candidate, отмечай это отдельно;
   bounded preference pilot допустим, но не доказывает улучшения ответа.

6. Первый run: epochCount=1, штатные 50 TRAIN/50 HELDOUT, batch 5,
   10 optimizer updates на плечо; итого 20 для A00/A10. Не подключай
   одновременно другую LoRA, loss, tokenizer, KNN или evidence projection.
   Верхний предел всей проверки — 5 эпох/50 updates на плечо.
   Продление сохраняет trainer state; новый AdamW не считается
   продолжением. При отсутствии такого seam остановись после 10 updates.

7. Проведи общий 50-row HELDOUT score до/после/cold: до 150 pair evaluations
   на плечо, плюс отдельно учти встроенные оценки самого runner.
   Выполни 8 ответных случаев до/после/cold. Новые контрасты:
   read-only report; invoice+audit transaction; Get-именованная процедура,
   которая пишет; изменённый result contract с .NET mapping.
   Unknown: недоступный dynamic SQL; неполная версия caller contract.
   Два retention-случая не входят в TRAIN. Negative Get-case запрещает
   угадывать эффект по названию. Оценивай только доступные consumer факты.

8. Отдай experiment.json, split-and-sources.jsonl, accepted-pairs.jsonl,
   model-visible-material.jsonl, scores.jsonl, answers-blinded.jsonl,
   state-readback.json и короткий result.md. Это артефакты исследования,
   не новый продуктовый API. Укажи реальные updates, calls, tokens,
   время по стадиям, peak RAM/VRAM и bytes состояния; неизвестное — null.
```

У настоящего current runner действительно есть `epochCount`, guard **50 TRAIN/50 HELDOUT**, `MiniBatchSize=5` и вызов `StepMany`. Поэтому 10 updates — выполнимый первый бюджет по его структуре, а не обещанные «две строки через штатный runner». Реальный доступ к admission/model route проверяется preflight без подмены его условий. [Constructor][runner-config], [batch size][runner-batch], [guard][runner-guard], [loop][runner-loop].

**Конкретная граница подачи нового Q.** Обычный AnswerComposerTrainingPipeline сам вызывает фиксированный generator для обоих splits. В metric runner обычный ToTrainingBatch присваивает DeterministicGenerator admission; этот путь нельзя использовать для нового внешне написанного Q, переименовав происхождение. В коде есть отдельный TrainAndEvaluateRepositoryOwnedNegativeDpoAsync с dataset address/manifest/checkpoint context и ToRepositoryOwnedTrainingBatch, сохраняющий исходную repository pair. Preflight должен подтвердить именно этот или уже существующий эквивалентный путь с корректным авторством, exact pair correspondence и отдельным frozen EXAM. Если связь из конкретного SQL опыта с ним ещё не оформлена, это явно ограниченный integration seam следующей реализации; сегодня готовой one-command CLI для Q не заявляется. Пустая authority, manual capability и подмена TeacherSource не используются как обход. [Публичные repository-owned входы][runner-repository-entry], [два способа admission][runner-admission].

**Критерий остановки подготовки TRAIN:** нет разрешённого однокомпонентного diff, сигнал не дошёл до токенов, unknown ошибочно размечен read-only или source families пересекаются. Отсутствующий reader останавливает проверку пользы для ответа; отдельный preference pilot сохраняет ограниченный статус. **Польза не показана**, если A10 не исправляет ни одного нового предметного случая относительно A00. Новая критическая ложь о записи/отсутствии записи либо потеря baseline-correct retention означает regression. На восьми случаях это screening, не статистическое доказательство общего улучшения. Рост только preference называется эффектом score.

**Единственный четырёхплечевой 2×2** выполняется позже, если нужен ответ о curriculum: A00/A10 используют general 10/50 table-effects; A01/A11 — focus 35/50, по 5 result/parameters/callers. Representation — первый фактор, распределение — второй. Внутри каждой пары меняется только один фактор; pool уже принята, HELDOUT общий. Это отдельная постановка с тем же пределом 10 начальных/50 максимальных updates на плечо. Прежний failed 106 не доказывает, что focus полезен. Не добавлять к этому 2×2 смену reference, evidence или LoRA.

## AC-EV02: evidence и prefix отдельно

Сначала 0 updates: одна пара версий SQL/.NET символа имеет одинаковый вопрос, но разные доказанные writes/returns/callers. A — current question-only projection; B — вопрос с минимальным разрешённым evidence/version. Оба candidate-текста по-прежнему пишет внешний автор. Не включать `Expected`, выбранную сторону или error-type gold в model input. [Текущая проекция][composer-pipeline].

Default context 192 оставляет prompt budget 48. Это **условный риск**: appended evidence может не войти в actual prefix; здесь не заявляется выполненный контрпример его потери. Сначала вывести реальные IDs/masks, проверить, что flipped факт меняет вход, а незначимая перестановка полей после canonical serialization — нет. [Tokenizer budget][runner-tokens].

Для проверки содержимого выбрать общий context, вмещающий обе версии во всех плечах; изменение установки от default указать явно. B меняет только содержимое evidence. С одним fixed checkpoint измерить A_before/B_before до уроков, затем A_after/B_after и cold readback. Выигрыш B_before от доставки фактов — эффект входа при 0 updates; вклад уроков проверяется через ΔA=A_after−A_before и ΔB=B_after−B_before, затем ΔB−ΔA. Это два варианта в двух состояниях, не четыре независимые интервенции. **Отдельный** следующий опыт layout сохраняет полное содержание и default budget, меняя лишь размещение полей. Не смешивать две проверки. При достижимом learner — тот же бюджет 50+50, 1 эпоха/10 updates; сначала SQL, затем самостоятельная .NET panel. Gold: обе версии отвечены верно, нет stale facts, missing evidence вызывает abstention. Новые project/version IDs с независимо изменёнными фактами проверяют shortcut по имени версии. Улучшение structural composer без изменения neural consumer не засчитывается как neural learning.

## Другие домены: короткие независимые профили

**C# structural retrieval.** NK-01: 6–8 настоящих файлов, 4 фиксированных запроса; fresh index, два повторных обновления тех же ID, cold rebuild. A/B меняет только вычитание старого document-frequency вклада. Для трёх состояний и двух плеч — 24 retrieval вызова, 0 neural updates; проверить точную частоту по уникальным ID, её неизменность при одинаковых bytes, `df≤n`, top1/recall@3 и потом контекст обычного документационного consumer. NK-02 отдельно: 4 запроса к 8 методам с `==null`, `!=null`, `is null`, `return null`; восемь A/B retrieval за фазу. Сначала согласованность relation features при заданном query mode, затем отдельный natural-query gate опыт. Случайно непустой topK не означает правильной структуры. AST-правила C# не переносятся на SQL или перевод без другого parser. [Полные механизмы](03-NEW-KNN-CONSUMER-GAPS.md).

**Skills/orchestration.** NK-03: 8 задач, 12 eligible skills и 12 вне области, fixed scores/index. Сравнить `global topK → intersection` с `scope → topK`: 16 отборов, 0 обучения. Включить unknown, одинаковый content разных ScopeId и retention обязательных навыков. Измерить переданные ordinary consumer тексты, затем его выполнение задачи при одинаковом context budget. Default PassThrough не является failing KnnSkillFilter. Отдельный lexical Router можно проверить по прежнему профилю O: 2 различных урока, 4 новых запроса, native `Train`/`RouteForEvaluation`, затем ordinary `Route`/Flow и cold snapshot. Это PRIOR, другое состояние и другая метрика; DPO score его не заменяет. [KNN-досье](03-NEW-KNN-CONSUMER-GAPS.md), [прежний профиль 49][prior49].

**Neural C# или EN→RU.** Известный recipient contrast сравнивает q/v-LoRA с явно обучаемой выходной проекцией на одном совместимом B0/tokenizer/reference. Фиксировать число параметров либо считать его частью вмешательства. C# проверяет порядок аргументов/вычитания, EN→RU — кто действует на кого; отрицательные пары переставляют роли, unknown не угадывается. Сохранить нынешний корректный DPO знак и mask. Прежний 2-lesson/4-exam план условен до bounded caller: общий бюджет 4 native updates для двух плеч, общая 8-case панель включает четыре новых EXAM, unknown и retention, с отдельным cold readback. Не размножать две пары до 50 и не загружать hashed Wiki head в BPE Student. [Recovery](00-LOCAL-WIKI-RECOVERY-FIRST.md), [предыдущие планы][prior49].

**Inherited P2.** Для Mean/MatMul сначала показать actual caller, shape и достижение плохой ветви. Тогда native scalar/gradient сравнение — 0 обучающих updates. Если RP01 продолжает использовать `keepDims=true`, одинаковые leading axes или 2D правую матрицу, предметный quality run не назначается. Эти факты не превращаются в регрессию переноса. [Условное tensor-приложение](04-TENSOR-CONDITIONAL-GAPS.md).

## Как закончить опыт

Финал показывает конкретные ответы, state и стоимость, затем один вывод: **observable answer gain**, технический эффект без пользы ответа, no effect, regression или invalid/nonreached. Положительный screening требует хотя бы одного нового исправленного случая, отсутствия новых критических ошибок и сохранения подтверждённой retention после cold load. Независимая новая source family нужна для дальнейшего обобщения. Менять hashes полезно для атрибуции; сами по себе они не означают, что чему-либо научился обычный ответчик.

[prior49]: https://github.com/lubochka/llm_mvp_core/blob/649b9343cfda5b81cc54396a3840ad5db5e0b179/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/10-additions-language-restore-49.md
[sql-w]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs#L175-L212
[sql-c]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs#L177-L199
[composer-pipeline]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingPipeline.cs#L106-L111
[ordinary-composer]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectLocalWikiModelUnit.cs#L40-L81
[runner-config]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L90-L110
[runner-batch]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L33
[runner-guard]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L273-L289
[runner-loop]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L572-L604
[runner-tokens]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L1796-L1817

[runner-repository-entry]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L193-L242
[runner-admission]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L1523-L1565
