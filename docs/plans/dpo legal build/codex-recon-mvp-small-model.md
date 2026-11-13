# codex-recon-mvp-small-model

Корпус: `mvp` · slug: `mvp-small-model` · relevance: **high**
Вопрос: рациональ/замысел малой обучающейся модели и её архитектуры.

---

## Что это и где (точные ключевые пути)

Область: `C:/Users/lubao/Documents/xiigen/llm mvp/Small model racional`. Пять артефактов; смысловое ядро — два .md + один .txt (zip'ы это bulk-evidence, не вскрывались):

- `Small model racional/TRANSLATION-SMALL-MODEL-RATIONALE-v1.md` — главный документ: *почему малой модели достаточно* для curriculum перевода (RU/HE <-> EN). v1.0, 2026-05-11, 204 строки.
- `Small model racional/TRANSLATION-MINDSET-FAQ-v1.md` — поведенческий FAQ/якорь, встраивается в skills агентов; формализует «переломный момент» и анти-паттерны (scaffolding-as-result, подмена корпуса). v1.1, 2026-05-29.
- `Small model racional/Переломный момент мышления.txt` — первоисточник (транскрипт, 43 стр.): момент, когда модель честно отделила scaffolding (`[RU_EN:BOOTSTRAP] <текст>`) от реального обученного перевода. Это origin для FAQ.
- `Small model racional/35.ExperimentsV2.zip` (~4.5 МБ) — собранный harness экспериментов (bin/Debug+Release net9.0: TensorCore, DPO, LoRA, TrainingLoop, TransformerModel, DynamicDomains, Embeddings, VectorSearch...) + результаты E01–E50/progress.json. **Evidence-only, не извлекал.**
- `Small model racional/test hypothesys.zip` (~0.7 МБ) — research/experiment-планы: `RESEARCH-04-UNIFIED-PREDICTION-FRAMEWORK-v1.md`, `PROGRESSIVE-TRAINING-CURRICULUM-PLAN-X04`, E01–E12 структурные/KNN/regex/PMI планы, micro-experiment framework. **Evidence-only, не извлекал.**

Связанный код (упомянут в FAQ, в самой области отсутствует): `27.TranslationDomain/` (`TranslationMachine.cs`, `TranslationTrainedUnits.cs`, адаптеры); `00.Core` (TensorCore).

---

## Как устроено / работает (кратко)

**Тезис рациона:** малой модели достаточно, потому что задачу НЕ заставляют учить «весь перевод» в одном недифференцированном пространстве. Задача расщеплена на 6 слоёв:
1. обучаемый routing flow; 2. direction-специфичные адаптеры; 3. topic-специфичные потоки; 4. level-difficulty gates; 5. малые измеряемые батчи по 50 пар; 6. held-out оценка после каждого батча.

**Архитектурные опоры (из rationale §«Why This Works»):**
- *Routing сужает поиск*: `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>` сначала выбирает direction / topic / register / risk / level / adapter id; адаптер-переводчик решает уже узкую задачу (напр. `RU_EN x general x Level 1`).
- *Адаптеры делают обучение обратимым*: 4 направления (RU_EN, EN_RU, HE_EN, EN_HE) — отдельные апдейты; регресс одного потока чинится без отката всего.
- *Батчи 50 пар = измеримые инкременты*: после каждого батча раннер меряет DPO reward margin, DPO accuracy, BLEU, chrF, target-script accuracy, copied-source rate, wrong-direction rate, named-entity preservation, completeness, CPU/RAM/disk/время.
- *DPO учит границы предпочтений, не язык с нуля*: «prefer лучший перевод над правдоподобно-неверным»; «плохая» сторона кодирует wrong-direction / drop clause / смена register / порча entity / неверный adapter.
- *Порядок целей*: сначала надёжный КОНТРОЛЬ (направление, adapter, target-only вывод, без копирования источника, сохранение смысла/entities), и лишь потом морфология/идиомы/стиль.

**Чего рациональ НЕ утверждает:** что малой модели хватит на всё — legal/medical/financial production, литературный перенос стиля, неограниченный разговорный перевод, broad web-corpora. Это требует поздних уровней / ревью / больших адаптеров / большей модели.

**Дисциплина (из FAQ):** первое действие сессии — один живой перевод; если вывод `[DIRECTION:BOOTSTRAP] ...`, это честный baseline, дальше — обучение, а не отчёт. Зафиксированы анти-паттерны: показ scaffolding как перевода; принятие accuracy >=0.95 без проверки контаминации; подмена указанного источника корпуса (Tatoeba/UN) на «уже имеющиеся» старые батчи (это та же контаминация, что TR-HO-17..32, где runtime_exact_accuracy=1.000 = меморизация KNN, а не обобщение).

---

## Релевантность к ILearningDynamicGraph

Высокая — это прямой прецедент для авто-DPO по уровням ILearning*:

- **Уровни/иерархия.** Рациональ уже мыслит уровнями: routing как `ILogicalFlow`, переводчики как нижестоящие обучаемые юниты. Это шаблон отображения «capability -> разрешённый уровень», который нужен ILearningDynamicGraph для узлов/рёбер.
- **DPO-построение.** Чёткая схема pair-building: chosen = лучший перевод, rejected = конкретные классы ошибок (направление, пропуск, register, entity, mis-route). Готовый словарь negative-категорий для авто-генерации DPO-троек на рёбрах графа.
- **Измерение/гейты.** Полный набор per-batch метрик + «repair, not faith» — это контракт измеримости каждого инкремента, переносимый на per-edge/per-node измерение в графе.
- **Teacher / provenance.** Жёсткое предупреждение в FAQ §4 о data leakage и Rule 10 («всё с нуля», внешний LLM = противоречие). Прямо пересекается с Legal Teacher/Provenance Gate скила: закрытые проприетарные teacher-выходы запрещены как distillation/DPO-таргеты; источник корпуса — часть задания и не подменяется.
- **Оркестрация.** Routing-перед-решением = тот же паттерн, что оркестрация выбора узлов/потоков в ILearningDynamicGraph; рациональ явно переиспользует «orchestration discipline».
- **Anti-memorization.** Кейс TR-HO-17..32 (held-out == train, accuracy 1.000) — готовый ablation/leakage-урок для гейта обобщения в графе.

---

## Классификация компонентов (по trainable-architecture-decision-SKILL.md)

Это документы-рациональ, не код; классифицирую упомянутые механизмы:

- `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>` — заявлен как **ILogicalFlow** (разрешённый уровень). Корректно как уровень-оркестрации маршрута ДО перевода; финальная authority по переводу должна быть у обучаемого юнита/машины, не у детерминированного селектора внутри flow. Lifecycle-evidence (checkpoint/export/import/locator/fresh-load) в самих документах НЕ показан — это рациональ, не реализация.
- Direction-специфичные адаптеры (RU_EN/EN_RU/HE_EN/EN_HE) — кандидаты в **ILogicalUnit** (или общий+пользовательский leg как `ILearningMachinePair`, если есть user-адаптивная нога). В документах уровень явно не закреплён — пробел для плана.
- KNN, упомянутый в FAQ §4 — **внутренний детерминистический хелпер/компонент памяти**, НЕ самостоятельная архитектура. Скил: KNN audit-triggered, допустим как компонент внутри разрешённого уровня, но как финальная authority на unseen он дал accuracy 1.000 = меморизация -> для перевода это `FORBIDDEN_RUNTIME_SHORTCUT` / `DATASET_REBUILD_REQUIRED` (контаминация). Это пример «нельзя принимать KNN/retrieval как всю обучаемую архитектуру».
- Bootstrap-обёртка `TranslateAsync -> [RU_EN:BOOTSTRAP] <текст>` и `TrainBatchAsync` (символическая запись, веса не обновляются) — **не обучаемая модель**, честный untrained scaffold. По правилам: это `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` максимум, и его нельзя репортить как trained feature.
- Метрики (BLEU/chrF/reward margin/script accuracy и т.д.) — eval-сигналы, не архитектура.

Вердикт уровня: документы описывают разрешённую раскладку (Flow + Units/Pair), но без lifecycle-доказательств; для ILearningDynamicGraph их брать как **паттерн-прецедент**, а реальную обучаемость доказывать отдельно (checkpoint, fresh-load, continue-training, ablation, no-leakage).

---

## Пробелы / что доисследовать

- Документы рациона НЕ содержат сигнатур классов/записей, контрактов Input/Output юнитов-адаптеров и типизированного trained-state — только функциональный замысел. Уровень адаптеров (ILogicalUnit vs ILearningMachinePair) не закреплён.
- Реальный код `27.TranslationDomain/` и `00.Core` (TensorCore) в области отсутствует — нужно читать в репозитории `llm_mvp_core`, чтобы классифицировать фактические механизмы и проверить lifecycle.
- `35.ExperimentsV2.zip` (E01–E50, progress.json) и `test hypothesys.zip` (RESEARCH-04 unified prediction framework, progressive curriculum X04, E08–E12 structural/KNN планы) не извлекались — там, вероятно, численные доказательства «малой модели достаточно» и конкретные алгоритмы предсказания/графовые гипотезы; релевантно для DPO/измерения, но это bulk-evidence (требует отдельного бюджета на распаковку).
- Нет явного отображения рациона перевода на ILearningDynamicGraph-узлы/рёбра (fan-in/fan-out, multi-output edge contracts, async-исполнение) — это должен сделать сам план.
- Provenance/teacher политика описана только текстом (Rule 10, leakage-кейс); для авто-DPO в графе нужен формальный provenance-набор полей на каждое сгенерированное ребро.
