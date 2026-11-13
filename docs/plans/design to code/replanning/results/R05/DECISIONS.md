# R05 — решения, исправляющий пакет и передача

**CP2, 13.10.2026.** Публикуется внутри исследовательского R05. Изменения core и live slice requirements здесь не выполнены. R05 OPEN с ограничениями FINDINGS §8; конкретный пакет ниже передаётся существующим R12/R13/R18, а не создаёт R26 или новый training pipeline.

## 1. Принятые исследовательские решения

**D1 — сохранить существующую реализацию.** Найденная цепь endpoint/coordinator/server executor/Stable executor/LogicalDpoTrainingUnit/StepVector/AdamW/trial quality/publication является исходной базой. Не писать trainer заново и не превращать отсутствие полного consumer trace в утверждение «обучения в коде нет» (F01,F09).

**D2 — сначала верность материала и цели.** Lossy 480-character Compact и full-sequence mean objective — два связанных участка одного seam: смысл exact approved pair должен доходить до нужного parameter update. Их надо устранить до интерпретации качества следующего урока (F10,F11). Простое увеличение 480 или добавление хеша это не исправление.

**D3 — роли не переоткрываются.** CODEX Astra 6 ultracode (`GPT-6-Astra`/`ultra`) выбирает разрешённый урок вместо человека и отдельно оценивает точные pair/context. Допущенная открытая китайская модель авторствует обе финальные стороны. Student учится; independent Student evaluation остаётся отдельной. Исторические пары не relabel, weak Student response — context, не автоматически rejected. Никаких фиктивных model calls.

**D4 — не терять четыре роли и адрес.** Machine владеет Train; Unit/Flow/Orchestrator/Graph сохраняют исходный смысл. Canon 14-field/semantic OrderedPatternIds сохраняется. Diagnostics — в существующем context/manifest, не пятнадцатое поле пары. Selector recipient устанавливается по реальному owner/session, не `winner.UnitId` по умолчанию. Generic bridge пока не доказан (F05,F15).

**D5 — различать пять доказательств.** Сохранённая пара, качественная пара, допущенный материал, фактическая мутация и полезность на новых задачах — разные утверждения. Training margin-sign accuracy, bytes/hash или изменённый gradient не заменяют последний этап (F02,F06,F13).

**D6 — zero margin не semantic rejection.** При совпадающих distributions margin=0 совместим с полезной парой. Но неправильные masks, одинаковые стороны и неактивная parameter group могут лишить шага полезного сигнала. Actual PE1.2p5 consumer вне прочитанной цепи остаётся неизвестным; неизвестный guard не удаляется под видом исправления runtime (FINDINGS §5.4).

**D7 — соблюдать область кампании.** S04 — ровно свой допущенный один новый урок; S05 — публикация существующего; S06 — уже предусмотренная конечная последовательность разных новых уроков, каждый с собственными baseline/state/evaluation. S06.08B/S06.09C дают learning curves; bulk не означает replay старых пар (F14).

**D8 — не обещать PC-fit из размера LoRA.** Считать base/reference/activations/AdamW/index/RNG/restore/cold-load и remote calls отдельно. Actual per-domain kNN/coordinates и shared/project state решения принадлежат R04/R06/R07; current active-slot multiplier не подменяет их (F03,F04,F12).

## 2. Один future work order R18: exact preference → корректный training objective

**Статус:** DRAFT_WITH_EXPLICIT_RUNTIME_INPUT_GAPS. Кодовый scope и counterexamples определены. Это не authorization на исполнение и не готовый live lesson: фактические lesson/assessment/checkpoint/evaluator/ordinary-entry values перечислены в §5. Разделение A/B — две последовательные части одного deliverable, а не новые проходы/новая очередь. Обе имеют полный prompt и приёмку; общие правила этого раздела передаются каждой буквально.

### Общие контекст, skills, references и ограничения

**Роль исполнителя:** текущий допущенный product executor реализует минимальный seam; не назначает себе роль Chinese author или CODEX judge. Независимый архитектурный reviewer должен читать фактический diff и смысловые результаты; авторская самопроверка не подменяет reviewer. Запрошенную Astra ultra/ultracode конфигурацию и реальный launch различать.

**Task/non-task:** обеспечить соответствие approved pair/context обучающему material и явно выбранному objective. Не менять четырёхуровневую архитектуру, generic Machine API, RAG/kNN family, модели/провайдеры, lesson budgets, authoring rules, authorization/lease issuer, закрытые данные, live campaign или GitHub workflows. Не создавать parallel endpoint/trainer/manifest registry. Не добавлять suite/coverage/Windows gates как доказательство learning.

**Прочитанные skills:** K `docs/skills/trainable-architecture-decision-SKILL.md` (learned authority/state), `phase-planning-SKILL.md` (bounded outcome), `plan-review-SKILL.md` (уникальные обязательства), `code-examination-SKILL.md` (producer→consumer), `prompt-patch-authoring-SKILL.md` (конкретный wrong/correct), `human-readable-architecture-audit-SKILL.md` (наблюдаемый эффект), `experiment-design-SKILL.md` (baseline/candidate/metric), `improvement-measurement-SKILL.md` (поведение вместо test counts). Read spans — SK01–SK08. Старые обязательные test-count/Tier3/30–50-pass routines не становятся текущим budget или повторным обучением. Дополнительные skills из действующего role packet читать адресно; не утверждать, что они уже все загружены R05.

**Source/RAG route:** текущий `run-prompts/R05-PARALLEL6.md` → эта пара FINDINGS/DECISIONS и SOURCES → WorkLeft-RAG-00 → WorkLeft-02/INTEGRATION-01/TASK-PROMPTS-TRAINING → текущий S04 RUN-CONTRACT §§2.1–2.3 → plan-07 §§3.1–3.2a → C11/C12/C01. Сначала сравнить actual changed blobs с K; если seam уже исправлен другим коммитом, переиспользовать и оценить его, не дублировать. Не перечитывать весь репозиторий ради двух функций.

**Текущие code owners:**

- `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`: `LogicalDpoTrainingUnit.PrepareExecution`, `BuildBatch`, `Compact`, `ToMask`, actual StepVector caller; C12.
- `11.DPO/DpoTrainer.cs`: StepVector, ComputeLogProbs, BackwardLogProbs и публичный EvaluateAverageNextTokenLogProbability; C01.
- `14.ContinuousLearning/Training/DpoTrainingExecution.cs`: exact raw pair/typed author/lease → prepared execution; C11. Менять только при действительно необходимом переносе уже существующего bound projection.
- `11.DPO/Models/DpoTrainerPairAdmission.cs` и существующий prepared batch contract — исключительно для совместимости exact content/projection, если без этого не передать required masks; новый файл/поле не предполагается существующим. Разрешённость расширения определяет текущий source owner, не R05.
- Existing snapshot/publication owners C09,C10,C16 сохраняются. Их полномасштабный rewrite не часть задачи.

**State/связность:** неизменные admitted raw pair/context/author/evaluator bindings; tokenizer/serializer/objective version; target policy/session identity; accepted parent, full base+adapter checkpoint, trainable group, reference, AdamW step/state, RNG, attempt/applied-pair history. Нельзя утаить изменение shared/project coordinates за тем же размером tensor. Persisted trial и accepted state различаются; failed/unknown effect не получает новый ID ради rerun.

### A. Lossless domain-sensitive preparation

**Полный prompt исполнителю.**

> В существующем `LogicalDpoTrainingUnit` устрани silent whitespace rewriting и per-field 480-character truncation на пути approved Prompt/Chosen/Rejected → tokenizer. Начни с прочитанных C11 TryBuildLogicalRequest → C12 PrepareExecution → BuildBatch: это реальный optimizer path. Сохрани исходные разрешённые bytes и смысл structured/text sides, включая значимые пробелы, literal content, Unicode и OrderedPatternIds. Существующий identity passport оставь source identity, а faithful projection и tokenizer/serialization identity привяжи через текущего владельца prepared execution/metadata; не расширяй 14-field GraphDpoPair молча.
>
> Найди реальные model/tokenizer context limits, а не считай 480 или UTF-8 byte count лимитом модели. До любой мутации проверь целиком обе последовательности с нужными special tokens и response boundaries. При overflow верни существующее допустимое refusal/recovery поведение с причиной: не отрезай хвост, не заменяй пару и не делай новый teacher call. Новый сокращённый материал потребовал бы отдельного допустимого authoring и CODEX exact assessment; это вне этого патча.
>
> Раздели attention/padding semantics и response-loss semantics там, где фактический контракт этого требует. Границы токенов установи по реальному serializer/tokenizer: отдельный Encode(prompt) может не давать prefix length совместно encoded строки. Сохрани conditioning context и полный ответ. Не создавай новый tokenizer и не нормализуй домен без подтверждённого сохранения его смысла.
>
> Верни минимальный diff, source-to-prepared trace и заполненную таблицу смысловых случаев ниже. Никакого обучения/coverage/Windows dispatch этим поручением не добавляй. Часть B получает exact prepared representation и реальные response boundaries, а не догадку о них.

**Вход:** unchanged pair/context + tokenizer/model limits из настоящего существующего owner; K source trace и действующий контракт P08. **Выход:** source-faithful preparation или явный bounded refusal до мутации, а также actual response boundary contract для B.

**Хороший пример:** две авторские стороны различаются только содержательным заключением после длинного общего prefix; обе целиком доходят до optimizer либо обе отклонены по реальному limit без update. Значимые пробелы внутри code/string literals сохранены.

**Плохой пример:** увеличить 480 до 4096; хешировать raw pair, но обучать сокращённую; сортировать semantic path; «чинить» rejected вручную под Chinese provenance; считать all-nonpadding mask автоматически response mask.

**Крайние случаи и ожидаемое поведение:** общий prefix >480; whitespace-sensitive code/string literal; RTL/Unicode/surrogate boundary; empty completion; content identical после actual tokenization; unequal lengths; tokenizer boundary merge; missing EOS/BOS policy; несовместимые context/version; overflow одной стороны; stale assessment; conflicting project target. Не выдумывать новый retry. Для неоднозначных/одинаковых сторон — текущий semantic uncertainty/admission маршрут, не произвольный positive label.

**Приёмка A:** reviewer по raw text, serializer, token span и контексту объясняет, где находится каждое смысловое различие и почему оно сохранено. Для overflow видно отсутствие новой mutation, а не падение после optimizer. Проверка grounded content/ports не является claim о learning gain; числовые learning результаты остаются NOT_MEASURED. Отдельная маленькая документная/инструментальная проверка представления допустима только в фактическом authorized scope, не превращается в новую product test suite.

### B. Response-conditioned objective в том же trainer

**Полный prompt исполнителю.**

> Используй результат A и действующий plan-07 §3.2a. Зафиксируй x, response/action serialization y, policy/reference identity и objective version. Для standard DPO вычисляй сумму conditional log-probabilities полного ответа при том же context, с правильно сдвинутой response mask; padding и prompt targets не входят в response score. Forward и backward должны использовать один и тот же mask/reduction. Не подставляй CODEX score, kNN distance или единый invented reward вместо policy/reference likelihood.
>
> В `DpoTrainer` отдели DPO response scorer от публичного `EvaluateAverageNextTokenLogProbability`, чей average контракт может иметь других consumers. Не меняй его скрытно. Не вводи length-normalized alternative под прежним названием: такую альтернативу можно лишь явно предложить и отдельно сравнить после решения R12. Сохрани существующие trainer/AdamW/lease/publication/reference/one-pair boundaries и current hyperparameters до обоснованного отдельного решения.
>
> Выведи исходную и требуемую формулы и покажи, почему unequal response lengths сейчас оставляют общий prompt term. Проверь смысловой walkthrough pi=pi_ref: нулевой margin не semantic rejection, а derivative по margin = -1/2; реальные параметрические градиенты всё ещё зависят от masks, различимости и группы параметров. При dropout различай same weights и same realized probabilities. Не удаляй непроверенный PE1.2p5 guard — локализуй actual consumer отдельно.
>
> Явно опиши текущий TeachingVector active-slot multiplier: он не output token и не learned kNN. Не расширяй scope до переобучения embeddings. Сохрани его как отмеченную heuristic variant либо вынеси proposed ablation в R13; не прячь изменение его semantics внутри mask fix. Reported loss и applied gradient weighting должны быть документно согласованы.
>
> Верни bounded diff, математическую проверку response boundaries/reduction/sign, source-to-parameter trace и learning recipe §3. Не утверждай улучшение без ordinary consumer и независимых observations. Подготовка эксперимента не разрешает новый update.

**Вход:** A prepared sequences/boundaries; exact policy/reference compatible checkpoint и current C01; P04 conditional objective. **Выход:** named response-conditioned objective и согласованный gradient path без side-effect на public average scorer; explicit residual vector weighting variant.

**Хороший пример:** общий prompt длинный, полезный ответ короче плохого; preference objective учитывает полный ответ, но не награждает произвольную prompt-length нормализацию. Numeric/state deltas и held-out semantic outcome выводятся раздельно.

**Плохой пример:** использовать тот же all-token mask; поменять forward на sum и оставить backward `/cnt`; принять update по margin>0 на самой учебной паре; менять beta до красивого результата; назвать shape-compatible vector semantically aligned.

**Крайние случаи:** нулевая response-mask; next-token boundary первого response token; padding; EOS consistency; разная длина; identical tokenized sides; same weights с различными dropout realizations; reference storage shared; frozen parameter group; B=0 LoRA initialization; interrupted step; уже applied pair. Правильное поведение определяется соответствующим existing content/state contract, а не новым blanket success.

**Приёмка B:** независимый reviewer способен восстановить conditioning, score, gradient sign и reduction из current code и одного walkthrough; no learned-margin admission подмены. Frozen reference численно независим, нужная target group названа. Состояние не публикуется как useful только по loss/parameter delta. Реальная эффективность определяется §3 после разрешения §5.

## 3. Future examination R13 — первая применимая схема

**Вопрос:** даёт ли одна смыслово сохранённая Chinese-authored/CODEX-approved preference полезное изменение ordinary CodeDocumentation ответа сверх эффекта той же памяти/контекста и в действительном PC budget?

**Гипотеза, не обещание:** исправление faithful pair/objective делает возможным релевантный preference signal; оно само не гарантирует улучшение от одного шага. **Falsifier:** контраст после preparation исчез; ordinary consumer использует старую версию; улучшение целиком объясняется memory-only; independent quality не меняется/ухудшается; retention/resources выходят за заранее принятые task bounds. Последние случаи не требуют повторять тот же урок до успеха.

**Входной сценарий:** исходный WorkLeft пример CodeDocumentation — объяснение cancellation/exception поведения отдельного разрешённого метода. Actual method/source family пока не выбран этим R05. CODEX выбирает из имеющегося разрешённого curriculum; Chinese author пишет обе стороны; CODEX отдельно оценивает exact pair. Закрытый экзамен и платные/ограниченные чужие ответы не становятся targets.

**Сопоставление:** accepted baseline + no-update control при той же permitted memory/context и сравнимом compute против candidate после одной новой допущенной пары. Existing baseline сначала сохраняется/перечитывается. Не обучать control ради симметрии; не повторять уже used pair. Все версии отвечают на одну заранее фиксированную operational panel независимых method/source families; не выбирать лучшую пару/порог по закрытой panel. Технический lossless/mask walkthrough не является дополнительным real lesson.

**Rubric первого домена:** фактическое описание cancellation/exception/nullability/control-flow; отсутствие опасных ложных обещаний; полнота для запроса; реальная полезность. Нормы берутся из исходного code/contract соответствующей задачи, не из предпочтённого стиля модели. Pair judge оценивает contrast, independent Student evaluator — новые ответы. Оценщик получает одинаковые разрешённые сведения, labels/version скрываются там, где существующий protocol это допускает.

**Метрики:** per-family domain score/lawful completion и uncertainty; harmful assertion rate; domain confusion/abstention; latency distribution с count/precision; total authoring+assessment+train+eval+failed branches+cold-load/reindex time; RSS/VRAM/storage и retention на старых families. Никакой придуманной конфигурации PC, sample size или нового 95%. Numeric meaningful-effect/precision и evaluator/noise settings берутся из task/campaign до наблюдения candidate. Точки learning curve — последовательные новые уроки в существующей S06 кампании.

**Решение:** IMPROVE только с evidence сверх принятой uncertainty и без нарушения обязательных constraints; REGRESS — сохранить negative finding и existing full-state rejection/restore; INCONCLUSIVE — сохранить принятый parent и предел знания; INVALID_EVIDENCE — missing/wrong pair/context/consumer/state/evaluator и текущий recovery, не learning success. Неизвестное наличие update не разрешает второй update.

**Rollback/продолжение:** distinction между in-process parameter/AdamW/RNG snapshot C16 и durable attempt C09–C11 сохраняется. Accepted parent/pair history и trial state различаются. Версия tokenizer/LoRA/shared/project/index должна совпадать с восстанавливаемым owner; здесь не доказано, что существующий snapshot охватывает все generic graph/index objects. Ввод изменения такой state family требует явного owner/restore, не одного file hash.

**До фактического запуска все measurements NOT_MEASURED.** Отдельная Astra ultra экспертиза должна быть реально вызвана в допустимом host и идентифицирована; этот авторский анализ её не заменяет.

## 4. Return schema каждой части и всего work order

Каждый реальный исполнитель возвращает один существующий work record, а не новую систему отчётности:

```text
part: A | B | examination
status: IMPLEMENTED | EQUIVALENT_EXISTING | PARTIAL | BLOCKED_EXACT_INPUT
source_heads_and_changed_blobs:
actual_files_and_symbols:
input_pair_context_tokenizer_target_bindings:
current_to_target_behavior:
lossless_projection_and_response_boundary:
objective_reduction_and_gradient:
mutable_state_owner_and_restore:
good_bad_edge_case_observations:
ordinary_consumer_and_selected_version:
measurements: NOT_MEASURED | actual observation references
uncertainty_and_falsifier:
remaining_exact_gap_and_owner:
reviewer_identity_and_actual_scope: NOT_RUN | actual evidence
publication_commit_and_remote_readback:
next_existing_task:
```

Не заполнять IMPLEMENTED, reviewer PASS или реальные метрики только на основании этого документа. A передаёт exact representation в B; B — packet в существующий authorized learning lifecycle, не directly dispatch training. При разбиении дочернему исполнителю передать общую шапку + ровно свою часть + source subset + свой return/приёмку; return-only права не становятся write/training правами. Новые отдельные агенты в R05 не запускались.

## 5. Точные runtime input gaps, запрещающие назвать опыт executable

| Неизвестное значение | Где разрешить, без новой инвентаризации |
|---|---|
| Конкретный разрешённый lesson/source family и права, selected context и saved CODEX selection | Current held S04 Session/SourceSelection по RUN-CONTRACT §2.3; owner R08/R18 |
| Реальный Chinese author profile/response + exact accepted pair/context assessment | `CodeDocumentationRepositoryOwnedDpoDatasetAuthoring` и текущие dataset/assessment owners; не labels из примера |
| Принятый model checkpoint, tokenizer/LoRA/target group и ordinary inference entrypoint/version | Current checkpoint loader + actual CodeDocumentation production composition/caller; R05 continuation/R18/R09 |
| Независимый evaluator/config/panel/noise policy, precision, machine budget | Current one-lesson evaluation/grant owners, R13; никакой выдуманной sample size или внешнего сервиса |
| PE1.2p5 actual margin consumer и generic selector recipient bridge | R12/R07 с primary rule и producer/consumer; не требуется писать новый trainer |

Названные gaps не отменяют найденных F10/F11 и не препятствуют их адресному планированию. Они препятствуют утверждению «всё готово, начинаем оплаченный урок».

## 6. Адресные изменения будущего плана, а не новые раунды

**R16/R17:** применить текущую Chinese-both-sides policy к конфликтующему plan-07 teacher-vs-teacher запрету; не relabel историю. Исправить `NodeOrEdgeId=winner.UnitId` только после связывания recipient с действительным learner owner. Сохранить 14-field row и ordered semantic payload. Убрать перенос S04 total limit на S06 и повторную базовую реализацию уже имеющегося trainer/coordinator. Конфликтующие skill templates «новые tests/count в каждом STOP» и обязательный Tier3 заменить для learning work на наблюдаемый результат/uncertainty/ограниченный budget, не вырезая действующие data/state safeguards. Здесь эти live файлы не изменялись.

**R04/R06:** получить F03/F10/F12, current representation/projection consequences и shared/project/index state. Выбор доменных tokens/dynamic kNN — содержательная работа этих существующих проходов, не hard-coded R05 рецепт. **R07:** actual port/recipient/credit bridge с четырьмя ролями. **R12:** response objective/length handling и unresolved margin consumers; не повторное утверждение авторства. **R13:** §3, source-family units и полная стоимость. **R18:** §2 A→B в existing pipeline. **R22–R25:** learning curves/PC/online feedback/ordinary consumer по широте доменов; не все как prerequisites первой пары. **R10:** CP2 evidence и честный OPEN статус, не fabricated DONE.

## 7. Самопроверка этого документа

Source links и claims согласованы с прочитанными spans; CP1 IDs сохранены. Два дефекта имеют фактический caller, а не только название функции. Learning formula отделена от current mean/weighting variant; dropout/zero-gradient caveats отражены. Нет утверждения об отсутствии всех margin gates, freeze всей base, работе normal consumer или measured improvement. Две части одного work order содержат task/non-task, inputs/outputs, skills/RAG/references, конкретные действия, good/bad/edge, приёмку и return. Это авторский document/code walkthrough. Независимый reviewer: **NOT_RUN**. Build/tests/coverage/Windows/Actions/training: **NOT_RUN**.
