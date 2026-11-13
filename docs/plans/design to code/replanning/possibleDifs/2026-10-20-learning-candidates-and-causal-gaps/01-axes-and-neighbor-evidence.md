# Возможные различия реализации и повторная проверка соседних веток

## Как читать этот реестр

Здесь собраны оси, способные повлиять на обучение или на использование его результата. **Статический факт в коде не равен доказанному влиянию на качество.** `Измерено` означает сохранённый причинно относящийся результат; `source difference` — подтверждённое отличие исходников; `гипотеза` — будущая проверка. Одного названия P-карточки недостаточно, чтобы объявить опыт реализованным.

Исходное RP01-требование включает отдельные уровни Unit, Flow, Orchestrator и DynamicGraph, typed many→many, сохранение relations, реальное изменение состояния и его обычного потребителя. Оно само отличает source/requirement map от доказательства реализации и связывает AST/POS, DPO contrast и собственный Student с разными проверками. Поэтому потеря любой из этих связей существенна, но нельзя приписывать все функции одному LoRA update. [RP01 source/architecture contract][rp01]

## 1. Полная карта направлений этого аудита

| Ось | Возможный пробел и способ не ошибиться в выводе | Что установлено в этом проходе | Покрытие защищённого каталога |
|---|---|---|---|
| Получатель обучения | DPO pair должна менять нужные weights или память, а не только счётчик/отчёт; q/v-LoRA не эквивалентна full-model SFT | Current DPO вызывает backward и optimizer. EMB full-model success не переносится на frozen embedding RP01 | P01, P02, P10, P11; [DPO dossier](dpo-lora.md) |
| Качество chosen/rejected | Реальные запрос и ответы могут быть заменены маркерами из типа дефекта; лёгкая discrimination не обучает содержанию ответа | **Подтверждено source difference** current/donor для двух ProjectWiki pipelines, раздел 2 | P03, P04, P108, P118 |
| Objective и длина ответа | Prompt-inclusive mean, response-only sum, normalization, fractional mask — разные objectives; loss только при одинаковом определении сравним | Exact RP01 использует response masks/sum; fractional legacy derivative уже исправлена и имеет положительный live8 результат | P05, P07, P08, P86, P87; [DPO candidate](positive-dpo-mask-live8-candidate.md) |
| LoRA A/B и численная цепь | Target modules, rank/alpha, initialization, frozen base, optimizer allowlist, cached forward и composition gradients | Нулевой первый A-gradient при fresh B=0 закономерен. Нельзя считать всё отсутствие learning следствием этого. Current path разобран адресно | P01, P06, P09–P12, P105; [source review](dpo-lora.md) |
| Tokenization / materialization | Различия raw→tokens→masks могут уничтожить контраст, response boundary или meaningful tokens; совместный BPE не обязан равняться раздельной конкатенации | Сохранённые neural A/B имеют одинаковый материал. Универсальная эквивалентность исторических tokenizer/models не доказана | P07, P08, P17, P71, P76, P98, P99 |
| Семантические векторы | Сходство bag/count признаков может терять порядок, роли, AST, цифры, операторы и root channel; размерность без schema не гарантия смысла | KNN history сохраняет полезность структурных/POS признаков в конкретных задачах. Не переносить Roslyn geometry в SQL/обычный текст | P16, P26–P28, P51, P61, P103, P113, P122 |
| Учебные метки KNN | Ошибочный teacher способен системно обучать неправильному действию | **N-KU измерен**: DoStatement в label улучшает заданный показатель; есть регрессия и majority effect | N-KU-01; [candidate](candidate-result.md) |
| Число соседей и votes | Частоты классов, ties, невозможные соседи, top-k duplicates и отсутствующий контекст меняют решение | k3 на четырёх N-KU строках даёт постоянное большинство; это ограничивает вывод об обобщении, но не отменяет исправление меток | P20–P23, P30, P35, P47, P60, P64, P77, P90, P100, P107 |
| Последовательное обучение | Replacement Train может очистить полезную память до проверки, что новые строки дают допустимые пары | **Уточнение KH-EXT**: source chain от methods-count gate через Clear до pair-count reader; активный RP01 caller не доказан | P36, P74, P91 частично; точный T1→T2 рецепт отсутствует. [Карточка](method-memory-card.md) |
| Стоимость подготовки урока | Дважды построенный AST перед одним Train увеличивает подготовительную работу | **Новая KH-COST**: два ParseText подтверждены; выигрыша ещё не измеряли | Общие cost-темы не закрывают exact mechanism. [Карточка](knn-cost-card.md) |
| Стоимость LoRA update | Strided gradient computation влияет на реальную цену backward | **Новая GRADB-LOCALITY**: отдельный правый adjoint, прежняя gradA-карточка его исключает | P110 / N62 не заменяют новый exact factor. [Карточка](new-gradb-locality-card.md) |
| DPO→Unit материал | Полноценная пара не гарантирует, что Executor/другой Unit получит нужные subunits, phrase mappings, context и правильные semantic targets | **Измерен ingress loss**: потеря phrase_mappings мешала composition; исправление уже внесено | P52, P58, P88, P92, P106, P118, P125; [перевод](02-translation-and-restoration.md) |
| Unit→Flow / checker | Правильный отдельный Unit, request-only checker или metadata receipt не доказывают исправление обычного Flow; oracle нельзя подставлять вместо Student | P125 использует обычный Flow и learning memory. N-KU использует только native reader, не полный Compile/IDE | P38–P42, P53–P58, P88, P92–P95, P123, P124 |
| Детерминированные замены | Фиксированная эвристика или ручной словарь могут заменить обучаемый компонент; `passed` может оценивать шаблон | E25 donor имеет обучаемый POS KNN, current `29.SemanticCompiler/Translation/PatternClassifier.cs` содержит фиксированные правила. E26 context result не имеет нового Train и исключён из learning-success | P28, P39, P43–P45, P79, P80, P107, P122; [KNN source/history](knn-history.md) |
| Retention / replay | Новый материал может забыть старые навыки; teacher ordering/curriculum может менять само распределение transitions | Есть исторический E43 retention gain; это отдельный learner и панель, не гарантия RP01 q/v-LoRA retention | P15, P24, P48, P59, P67, P116, P119 |
| Restore / identity | Восстановление всех weights недостаточно без tokenizer, unit memory, optimizer/RNG/schema/ordinary loaded identity | P25 сохраняет конкретный learned translation checkpoint. Исторические schema-v1/current-reader несовместимости остаются отдельными задачами | P18, P22, P25, P53, P59, P117; раздел 3 |
| Orchestrator / graph | Fixed branch order и graph closure без learned state не доказывают обучение отношений; async costs могут быть скрыты | В этом проходе нет нового runtime доказательства learned graph. Названия E49/E50 и GATE_PASSED не заменяют writer→state→consumer | P32–P42, P50, P69, P70, P94–P97 |
| Domain coverage и measurement | Synthetic POS, C# methods, перевод EN→RU, ProjectWiki route — разные задачи; числа с разным gold не суммируются | Явно разделены requirement satisfaction, semantic answers, online preference accuracy, train loss, derivative validity и cost | P03, P57, P66, P82, P91, P102, P109, P112 |

Каждая P-ссылка с подслучаями, предметными секциями, stage и открытым execution passport находится в [coverage.json](coverage.json) и [полном аудите](experiment-audit.md). Таблица выше — маршрутизация и итог адресного анализа, **не новый PASS для всех перечисленных 470 сравнений**.

## 2. ProjectWiki: содержание пары потеряно по дороге в DPO

Сопоставлены неизменяемые версии:

- Донор `local_wiki_planning`: `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`.
- Текущий `codex/dpo-implementation`: `9622b575a6aabb79939b12cc9f5d96313f1bf370`.

В донорском `CodeTraceBuilderTrainingPipeline.ToDpoMetricPair` передаются `pair.A`, `pair.Chosen`, `pair.Rejected`. В текущем методе вместо них используются `BuildMetricPrompt(pair.RejectedType)`, `BuildChosenSignal(pair.RejectedType)`, `BuildRejectedSignal(pair.RejectedType)`. Эти builders создают повторяемые строки с `CODE_TRACE_DPO`, `PREFERRED_CODE_TRACE_OUTPUT`, признаками правильности/дефекта. [Донор builder][wiki-old-builder]; [текущий builder][wiki-new-builder]

Аналогично донорский request router включает `Executor prompt: {pair.Prompt}`, настоящий chosen и rejected. Текущий `ToDpoMetricPair` строит обе стороны из `RejectedType`, `CHOSEN_IS_CORRECT`, `FINAL_PASS`/`FINAL_FAIL` и route correction/defect tokens. Исходный запрос и содержание ответов здесь не передаются. [Донор router][wiki-old-router]; [текущий router][wiki-new-router]

**Возможное следствие:** модель может научиться отличать метку дефекта или служебный носитель, не научившись выбирать правильное содержательное решение для исходного запроса. Это обоснованная гипотеза по source transformation, не измеренное в этом проходе падение качества. Также нельзя утверждать, что любой current DPO во всех доменах использует этот путь: finding ограничен названными pipelines.

**Адрес кандидата в исправление после отдельного опыта:** два указанных `ToDpoMetricPair`; сохранить реальные prompt/chosen/rejected в experimental B при неизменных train/heldout families, model, objective и budget. Это известное направление P04/P108/P118, не новая гипотеза этого пакета. Полного выбранного паспорта для такого будущего запуска этот раздел не выдаёт: он не должен становиться поручением «придумай host и запусти».

### Успехи соседней ветки действительно сохранены

| Сохранённый report | Что в нём есть | Что из него не следует |
|---|---|---|
| CodeTrace builder, batch064 | 50 train /50 heldout; DPO heldout accuracy **0.92**, training_status `published` | Это не доказательство 92% end-to-end ответов RP01 и не matched A/B нынешних маркеров |
| Query planning, batch134 | 50/50; DPO heldout accuracy **1.0**, training_status `published` | Не универсальное качество генерации документации или всех Units |
| Request router, batch092 | DPO heldout accuracy **0.66**, training_status `trained_not_published`, summary.passed=false | Поле overallVerdict содержит GATE_PASSED, но публикация именно не состоялась; одно слово PASS читать нельзя |

Первичные JSON: [builder064][wiki-result-builder], [query134][wiki-result-query], [router092][wiki-result-router]. Это прочитанные сохранённые результаты донорской ветки; не повторное исполнение и не аттестация бинарника нынешнего SHA. `baselinePrimary:0` в summary не трактуется как измеренный C0 neural baseline без отдельного receipt. Поля `primary_accuracy` и `dpo_heldout_accuracy` обозначают разные проверки.

## 3. Восстановление: успешный current checkpoint не закрывает старый donor

### Перевод: четыре направления против шести

В tracked `models/translation/translation-v1/translation-v1-checkpoint.json` содержится schema1 и четыре executors: RU_EN, EN_RU, HE_EN, EN_HE. Current `TranslationMachine` создаёт ещё ES_EN и EN_ES, а preflight требует наличие каждой runtime direction. Поэтому старый четырёхнаправленный файл и новый шестинаправленный checkpoint нельзя считать взаимозаменяемыми на основании одинакового номера schema. [Старый tracked checkpoint][translation-old]; [preflight][translation-preflight]; [runtime directions][translation-runtime]

Это известная migration/compatibility задача. **Успешный P25 — другое доказательство:** он загрузил конкретное новое обученное состояние и сохранил его ответы. Отсутствие дополнительных испанских executors в старом файле нельзя «чинить» ложным заявлением, что они уже обучены. Будущий эксперимент должен явно сравнивать выбранную миграцию старого состояния с сохранением действительно обученных направлений; в текущем проходе он не запущен и product не изменён.

### OrchestrationReasoning: сохранённые активы и current admission

Текущий tracked manifest остаётся schema1 и указывает `orchestration-reasoning-v1-checkpoint.json`. В named checkpoint **15 learned patterns, 11 acceptedRefIds**. Соседний generic `checkpoint.json` содержит **4 patterns, 4 refs**. Имена файлов поэтому не обозначают одно и то же обученное состояние. [Manifest][or-manifest]; [named checkpoint][or-named]; [generic checkpoint][or-generic]

Current `OrchestrationReasoningMachine` требует schema2 и **явно отклоняет common schema1 с требованием provenance rebuild**. Current API пытается загрузить manifest target, а при неуспехе сохраняет изначально созданную машину, пишет `orchestration_reasoning_asset_loaded=false`, load_error и warning. Следовательно, здесь нельзя делать два противоположных неверных вывода: «старые навыки обязательно загружены» или «ошибка полностью молча скрыта». [Schema/read validation][or-validate]; [API consumer][or-api]

Это конкретный restore/admission разрыв использования старого результата обучения. Он **не разрешает ослаблять проверку происхождения**, менять manifests или выдавать новый пустой instance за восстановленный. В будущем нужно подготовить совместимый asset с сохранением проверяемого learned state/provenance и проверить обычного consumer. В текущем проходе фиксируются адреса и статус; новый restore run не назначен.

## 4. Положительный replay в соседней ветке

В `orchestrator-extensions` (`b293045bd89b1eb96d3e32fedd341f1a4170adbc`) сохранён E43 tier2: retention_accuracy **0.3226→0.3978**, oldPairs465, newPairs414, replay177. Это улучшение **7.52 процентного пункта**; baseline_time0.1ms и enhanced_time5.8ms показаны рядом и не означают ускорения. [Первичный receipt][replay]

Число retention_rate1.1562 в note — отношение к выбранному прежнему baseline, не 115.62% accuracy. Этот результат поддерживает полезность replay на той задаче. Он не доказывает, что replay встроен в нынешний RP01 и не переносит автоматически качество на нейронные adapters. Pair splitting и границы файлов отдельно покрывает P116.

## 5. Почему «матрица не помогла» не закрывает KNN

История E07/E09 показывает разные места использования структурной информации. Отрицательный результат одной matrix/representation реализации не доказывает бесполезность AST для learned neighbor selection. При этом их разные targets/splits/representations не позволяют выдать две архивные цифры за чистый matched перенос одной функции из матрицы в KNN. Сохранённые положительные E09, E13, E14, E18, E22, E25 и их ограничения разобраны в [KNN history](knn-history.md).

Для C# важны Roslyn AST, method boundaries и statement families. Для языка важны порядок, POS, роли, морфология, отрицание и direction. При сравнении нужно менять один фактор внутри одного предметного профиля. Кодовые имена KNN/LoRA/DPO/Unit сами по себе доменами не являются.

[rp01]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md
[wiki-old-builder]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L119-L120
[wiki-new-builder]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L124-L206
[wiki-old-router]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L110-L116
[wiki-new-router]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L131-L167
[wiki-result-builder]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/project-wiki-training-loop-result.json
[wiki-result-query]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/project-wiki-training-loop-result.json
[wiki-result-router]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-request-router-post-registry-selector-promotion-confirmation-092/project-wiki-training-loop-result.json
[translation-old]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/models/translation/translation-v1/translation-v1-checkpoint.json
[translation-preflight]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/27.TranslationDomain/TranslationMachine.cs#L390-L431
[translation-runtime]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/27.TranslationDomain/TranslationMachine.cs#L718-L780
[or-manifest]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json
[or-named]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json
[or-generic]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/models/orchestration-reasoning/orchestration-reasoning-v1/checkpoint.json
[or-validate]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L971-L1079
[or-api]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1403-L1445
[replay]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json
