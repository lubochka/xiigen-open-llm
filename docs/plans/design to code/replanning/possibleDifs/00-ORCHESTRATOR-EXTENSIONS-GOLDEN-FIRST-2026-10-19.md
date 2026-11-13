# ГЛАВА 0 / P0: вернуть доказуемые способности `orchestrator-extensions` раньше новой оптимизации RP01

**19.10.2026 · исследование первичного кода и сохранённых артефактов · NEW_NATIVE_LEARNING_NOT_RUN · QUALITY_GAIN_NOT_ESTABLISHED**

## 0. Почему именно эта ветка — golden donor

Точный донор: [`llm_mvp_core@b293045bd89b1eb96d3e32fedd341f1a4170adbc`](https://github.com/lubochka/llm_mvp_core/tree/b293045bd89b1eb96d3e32fedd341f1a4170adbc) (ветка `orchestrator-extensions`), 09.07.2026. Текущий проверенный получатель: [`codex/dpo-implementation@44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76`](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76), 19.10.2026. Все упоминания «успеха» ниже относятся к **историческому, ограниченному конкретным доменом протоколу**, не к новой генерации и не к текущему RP01.

В доноре реально сохранён [`models/orchestration/orchestration-v1/runner-report.json`](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/models/orchestration/orchestration-v1/runner-report.json): `learned_flow_grounding_accuracy=1.0`, `unseen_style_accuracy=0.8`, `human_explanation_accuracy=1.0`, `exact_prompt_leakage_rate=0`, экспорт 02.06.2026. Сохранённый [training manifest](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/training-data/orchestration/orchestration-v1/training-manifest.json) говорит о 352 строках: mode1 42 train/20 held-out, mode2 200/40, mode3 30/20. Нельзя суммировать эти выборки, объявлять accuracy процентом на всех 352 строках или переносить этот показатель на C#/RU-EN.

**Удивительная проверенная находка:** этот положительный отчёт (blob `d94756f722d8e2d362437fcd4d02f5395e0d62ea`), его checkpoint (blob `286fbf8082133b8a03a86477cf1aa32544928e48`), manifest (blob `3ca4414098fb5d8e71ce87a74bd61c4a17ef1d92`) и training manifest (blob `5fd9ec6b3add0550d1d1a71030c374679cd60f18`) **идентичны по Git blob SHA в золотой и текущей ветках**. Сам [`OrchestrationPlanMachine`](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L60-L175) в обеих ветках сохраняет режимы trained flow grounding, Mode2 kNN, Mode3 kNN и `TrainOrchestrationBatchAsync` (проверено прямым сопоставлением соответствующих участков). **Следовательно, нельзя объявить первопричиной «удаление этой модели»**; искать нужно, *какой consumer действительно использует её checkpoint*, не подменён ли алгоритм учебным RP01 q/v-LoRA на случайном B0.

## 1. Список экспериментов В НАЧАЛЕ: восстановить реальную функциональность эталона

| Очередь | Эксперимент и домен | Вернуть именно | Короткий исходный контроль / действие |
|---|---|---|---|
| **P0/OE-X01** | **Оркестрация / ProjectWiki** | `orchestration-v1`: 3 разных маршрута, сохранённое обученное состояние | Gold & current **один и тот же checkpoint** → вызвать напрямую `OrchestrationPlanMachine.GeneratePlanAsync` на фиксированных 6+6 новых формулировках; затем выполнить обычный ассистентский запрос и зафиксировать фактического получателя и `route_mode`. Требовать cold reload, не пересоздание дефолтной машины. |
| **P0/OE-X02** | **Обученные Units / orchestration-capability** | способность быть выбранным как **learned Unit**, а не только зарегистрированным retrieval | Различить `isLearned:true` (gold) и `isLearned:false` (current); проверить `ResolveLatest` против `ResolveLatestLearnedEntry` на одном сохранённом Unit, затем действительный consumer и качество. **Не менять флаг вслепую**. |
| **P0/OE-X03** | **Продолжение обучения / минимум два батча** | сохранение старого KNN-корпуса после перезапуска | Один домен, два законных батча, ранний/поздний уникальные примеры; сравнить `live`, `cold` и историческую политику включения строк. Измерять old-hit/old-accuracy, не просто число checkpoint. |
| **P0/OE-X04** | **Запуск обучения / orchestration-capability** | достижимый, санкционированный `TrainAndRegisterAsync` | Проверить обычный `CreateDefault` и явно разрешённый `CreateDefaultWithCapabilityTrainingAuthority`: текущая реализация без authority **явно отказывает**, золотая создавала trainer без такого параметра. Не отключать legal admission или file custody. |
| **P1/OE-X05** | **Cold restore / schema migration** | загрузку проверенного legacy generated capability | Сверить schema и KNN corpus; при несовместимости делать **верифицируемую миграцию**, не создавать фальшивые аттестации; positive и tampered negative. |
| **P1/OE-X06** | **ProjectWiki → C#/SQL/Translation**, только если существует действительный consumer | перенести *полезную функцию* в правильное место | Факторный A/B: только semantic kNN, только DPO/LoRA, оба, ни одного. AST и порядок кода — для C#; scope/NULL — для SQL; отрицание, контекст, направление — для перевода; номера Flow и intent boundary — для оркестрации. |
| **P1/OE-X07** | **Настоящий learner против повторения памяти** | качественную интерпретацию train gain | Тот же checkpoint: exact, changed-style, unseen-family и контрфактический запрос с противоположным законом. Не считать retrieval знакомого примера свободной нейронной генерацией. |

**Исполняемые карточки и критерии отказа:** [52 — короткие микроэксперименты](52-ORCHESTRATOR-EXTENSIONS-CAUSAL-MICRO-EXPERIMENTS-2026-10-19.md). **Исходники, причинные гипотезы и границы доказательства:** [52 — прямые расхождения](52-ORCHESTRATOR-EXTENSIONS-DIRECT-DIFFS-2026-10-19.md). **Сведение с RAG и прежними владельцами:** [52 — source/evidence/review](52-ORCHESTRATOR-EXTENSIONS-EVIDENCE-AND-REVIEW-2026-10-19.md).

## 2. Где уже есть объяснение неуспеха RP01, но НЕ доказана связь с gold

[RP01 diagnostic pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) сообщает: C#500 — 500 пар только 25 параметризованных семейств, 50 шагов, 16 LoRA-тензоров изменились, base не изменился, `free 0/3 → 0/3`; ENRU500 — `free 0/4 → 0/4`; выбранный первый токен остался 388. B0 — небольшая случайно инициализированная модель. Это подтверждает **отсутствие продемонстрированного прироста свободных ответов в данном RP01 опыте**, но не показывает дефект `OrchestrationPlanMachine`. Сравнение нужно делать через одинаковую задачу и её реального потребителя.

В `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` как gold, так и current `TransformerEmbeddingKnnLogicalUnit.ExecuteAsync` возвращает **сохранённый `example.Output`** выбранного кандидата; при наличии модели это retrieval-путь, не свободная генерация параметрической модели. Заявление `DPO + LoRA есть, значит ответ модели учится` некорректно в обе стороны.

## 3. Защита от повторения работ других агентов

Не переоткрывать как «новые» общие уже разобранные темы DPO-контраста, LoRA-gradient, BPE identity, E09/E26/E43, source-only E26 lookup, proxy-overlap, replay и false metrics: см. [индекс](README.md), [ранее опубликованный N51](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md), [RP01 диагностику](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md). **Новые предметные отличия** в этом пакете — конкретный золотой `orchestrator-extensions`, совпадение обученных файлов, изменение `IsLearned`/contract, default admission path и разная политика восстановления KNN-корпуса.

## 4. Условие объявления успеха

`DONOR_REAL_RESULT` ≠ `CURRENT_QUALITY_PASS`; `SOURCE_DIFF` ≠ `ROOT_CAUSE`. Прежде чем писать «качество восстановлено», нужны: **сохранённый общий набор case IDs, исходный checkpoint, точная provenance/tokenizer, собственно cold restore, тот же пользовательский consumer, измеренное качество unseen/scope и один причинно изменённый фактор**. При неоднозначности отмечать `NOT_RUN` или `NOT_PROVEN`; все предложенные здесь новые нативные опыты сейчас **NOT_RUN**. GitHub Windows CI и большие 500-pair прогоны не являются gate для этих малых предметных проверок.

**Scope:** записи только в `xiigen-planning/llm_plan3/.../possibleDifs`; `llm_mvp_core/docs/plans/replanning-possible-diffs/**` — неизменяемый read-only источник.
