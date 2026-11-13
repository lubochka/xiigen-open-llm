# N28 — новое независимое расхождение: показатель качества переводов может принять смысловую ошибку

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED + PYTHON_LOGIC_PROXY; NATIVE_DOTNET_NOT_RUN; ACTUAL_LEARNING_GAIN_NOT_MEASURED; INDEPENDENT_CODEX_ASTRA_REVIEW_NOT_RUN.

**Зафиксированная база:** `llm_mvp_core@d5c2409b24292d361593b8262b0c54997b15f539` (ветка `codex/dpo-implementation`); историческая ветка `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`. Новые сведения публикуются **только** в `xiigen-planning/llm_plan3/.../replanning/possibleDifs`. Каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` — READ ONLY.

## 1. Вывод понятным языком

Система отдельно считает, чему научился `TranslationMachine`, и насколько хорош перевод на отложенных заданиях. Однако в одном конкретном `TranslationProductHeldOutRunner` поле `ProductMatch` **не сравнивает смысл `ActualTranslation` с `ExpectedTranslation`**. Оно проверяет заданные строковые маркеры, письменность, копирование входа, bootstrap и необязательные emoji/entity-маркеры. Если маркеров нет либо они не выражают отрицание/отношения ролей, бессмысленный или противоположный ответ может стать «правильным» для `RuntimeProductAccuracy`.

Это **не доказательство**, что Student RP01 обучался на ошибочных метках: RP01 C#500 и ENRU500 используют отдельный `DpoTrainer/CodeDocumentation...` путь и сохранили free-answer 0/3 и 0/4. Это **самостоятельный дефект измерения и возможного отбора** обученной памяти Unit/Flow и полезности исторических механизмов. Нельзя заключать «потеря обучения» или «исправлено обучение» по `PassesGate` этого runner без CODEX-экзамена смысла.

## 2. Первичные строки и новые N28 различия

### N28-A / HIGH — формальная ProductMatch вместо законного смыслового ответа

- Код: [TranslationProductHeldOutRunner.cs#L141-L198](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L141-L198), особенно L159–179. `ExactMatch` рассчитывается отдельно по `ExpectedTranslation`, но **не является условием** `ProductMatch`. `BLEU1` и `ChrF1` тоже не являются условиями `ProductMatch`.
- В `RunAllDirectionsAsync` повторён тот же механизм: [L324–380](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L324-L380).
- Фиксированный контрпример для RU→EN: вход «Не удаляй файл», ожидаемо «Do not delete the file», реально «Delete the file», `required_markers=["delete","file"]`, `forbidden_markers=[]`. Проверки маркеров проходят, направление EN верное, полного копирования RU нет, `ProductMatch=true`, хотя смысл и полярность нарушены. Другой случай: «Собака кусает человека» → ожидалось «dog bites man», реально «man bites dog», маркеры `dog,man` присутствуют; обе метрики ниже тоже дают 1.0, роли обратные.
- При `required_markers=[]` любая латинская несходная строка вроде `banana` может пройти RU→EN gate: source «Привет мир» / expected «Hello world». Наличие полезного ответа **не проверялось**.
- **Влияние на качество:** возможный ложноположительный `RuntimeProductAccuracy`, если такой показатель используется для решения о готовности или сравнении вариантов. Проверить действительных потребителей `PassesGate` и конкретный training/evaluation corpus. В самом runner `PassesPromotionGate=false` — **автоматическая promotion этой функцией НЕ доказана** ([L249–267](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L249-L267)).
- **Домены:** отрицание, действие/объект, порядок ролей, юридические запреты, команды и безопасность — HIGH; простой словарь без синтаксиса — меньший риск; C# / HTML / AST — только если этот runner когда-либо вызывается для этих задач, **CALLER_NOT_SHOWN**.

### N28-B / HIGH conditional — якобы сохранённый emoji/сущности при отсутствии спецификации

- Код [L168–169](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L168-L169): `(testCase.EmojiMarkers ?? []).All(...)` и `(testCase.NamedEntities ?? []).All(...)`. Пустые множества возвращают `true`, даже если исходное сообщение содержит emoji/имя, а результат их потерял.
- Код [L211–218](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L211-L218) и [L394–404](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L394-L404): в первом runner emoji-denominator определяется **наличием emoji в source**, во втором — **заданными EmojiMarkers**; если denominator пуст, accuracy принудительно 1.0. Это два разных набора испытаний при одном названии метрики.
- Контрпример: вход «Привет 🙂», expected «Hello 🙂», actual «Hello», `EmojiMarkers=null` → `EmojiPreserved=true`; в RU_EN emoji-case учитывается, но ложно считается сохранённым, а в AllDirections может вообще не учитываться. Аналогичный риск для незадекларированных имён, кодовых идентификаторов и чисел; последние требуют отдельного semantic-law marker.
- **Домены:** пользовательские сообщения, карточки товаров, FAQ, именованные сущности, код/параметры API; при gold fixtures с полным списком маркеров риск ниже. Фактическая полнота fixtures **НЕ ПРОВЕРЕНА**.

### N28-C / MEDIUM — числа качества с неправильной семантикой BLEU/ChrF

- Код [L594–646](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L594-L646). `ComputeBleu1` здесь — unigram precision **без brevity penalty**; `ComputeChrF1` — F1 на **неупорядоченном мультимножестве символов**, а не стандартный chrF с n-граммами.
- Поэтому `dog bites man` и `man bites dog` имеют обе метрики **1.0** при противоположных ролях; `Do not delete the file` против `Delete the file` даёт unigram precision **1.0** после удаления `not`; «Hello 🙂» против «Hello» также может дать обе **1.0**, поскольку пунктуация/символы отсеиваются.
- В коде это **диагностические** метрики, не текущие условия `meetsAccuracyTargets`. Опасность — их неверная интерпретация людьми/последующими отчетами; не утверждать, что отсутствие brevity-penalty само по себе изменило train gradient.
- **Домены:** word order, negation, role-reversal, morphology, punctuation/code semantics — HIGH, word-bag тематическая близость — MEDIUM.

### N28-D / MEDIUM — ложный отказ на потолке качества

- В `RunRuEnAsync` [L222–230](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L222-L230) и `RunAllDirectionsAsync` [L408–416](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L408-L416) требуют `AccuracyAfter >= AccuracyBefore + 0.02` одновременно с absolute product thresholds. При `0.99→1.00` обучаемый Unit уже достиг потолка, но условие ложное; при `1.00→1.00` тоже ложное, даже если удержание идеально.
- Нужно отдельно оценивать `retention`, прежние ошибки и новые задачи; прогресс и абсолютную готовность не объединять в одно обязательное условие. Результат «gate false» не является доказательством деградации learned Unit.

### N28-E / P2 conditional — соседняя граница DPO-документа: Shuffle отбрасывает admission

- [DpoDataset.cs#L41–75](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/11.DPO/DpoDataset.cs#L41-L75): `GetRow` возвращает `DpoDatasetRow` с `Admission`, а `Shuffle` возвращает только `IEnumerable<DpoPair>`, выбрасывая маршрутную метаинформацию admission из обёртки. [DpoTrainer.cs#L1163–1206](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/11.DPO/DpoTrainer.cs#L1163-L1206) требует admission на момент мутации.
- Это **контрактная потенциальная ошибка передачи provenance, не установленная причина ухудшения**. Получатель `Shuffle`, который заново правильно прикрепляет admission, может быть безопасен; если consumer не пользуется `Shuffle`, влияния нет. Короткий preflight: поиск прямых вызовов `DpoDataset.Shuffle` в active RP01, доказать либо `UNREACHABLE`, либо trace до mutation (тогда отдельно чинить). Не повышать до дефекта обучения без caller-evidence.

## 3. Что не надо повторно «открывать»

Существующие зарегистрированные [DIF-14.01 и DIF-14.04](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) уже доказали duplicate `ExampleId` / RNG HNSW; они **не новые находки N28**. [N23 Tatoeba ProductMatch](23-INDEPENDENT-TATOEBA-QUALITY-SCORER-LEXICON-CACHE.md) описывает **другой runner**, где целевой semantic score тоже может ложно подтвердить урок. Здесь найден отдельный `TranslationProductHeldOutRunner` с другим контрактом (`RequiredMarkers/ForbiddenMarkers/EmojiMarkers`); общей причины по caller пока нет. [N27 Unit metric bias](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md) касается self-labeled Tokenizer и negative-only Verifier, а не `ProductMatch` на открытых переводах.

Сравнимые с реальными историческими успехами механизмы различаются: [E25, 2026-05-01, 001](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json) сообщает `top1_accuracy 0.3438→0.875` (val 32); [E26, 003](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json) — `disambiguation_accuracy 0.40→1.00` на 15 случаях; [E40V2, 002](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-002.json) — `trust_accuracy 0.6667→1.00` (21 val). Это сохранённые сведения о **разных задачах и метриках**, не доказательство, что новая neural Student умеет переводить или что `ProductMatch` валидирует именно эти результаты.

## 4. Проверяемая причинная цепочка

`known data → chosen/rejected (external open Chinese model) → independent pair review CODEX/Astra 6 UltraCode → trainable recipient → actual state delta → cold checkpoint → real ordinary consumer → blind semantic judge CODEX/Astra 6 UltraCode`.

- N28-A/B/C — прежде всего **observer/gate**; их нельзя без caller trace называть причиной RP01 0/3 или 0/4. См. [RP01 code recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).
- N28-D — **false-negative gate** при корректном удержании; проверять отдельно от полезности выученного результата.
- N28-E — **потенциальная потеря provenance на Shuffle**; сперва выяснить достижимость.
- Обязательно проводить blind semantic-law checks с раздельными denominators по направлениям RU→EN/EN→RU/HE, отрицанию, роли, цитированию, emoji и кодовым токенам.

**Роли не подменять:** человек-выборщик в конвейере — **CODEX с моделью Astra 6 UltraCode**; разрешённая внешняя открытая китайская модель генерирует **обе стороны `chosen` и `rejected`**; независимую оценку качества готовых неизменённых DPO-пар **тоже выполняет CODEX с Astra 6 UltraCode**. Автор этого source-аудита не исполнял ни внешнюю DPO-генерацию, ни CODEX/Astra judge.

## 5. Следующий короткий опыт и честные ограничения

См. [N28 MICRO one-factor](28-MICRO-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-16.md) и [сырую proxy-квитанцию](28-PRODUCT-HELDOUT-SCORER-PROXY-RESULT.json). Proxy реализует конкретные видимые предикаты, но **не компилирует C# и не доказывает пользовательский learning gain**. В native проверке сначала воспроизвести только различие `ProductMatch` и blind смысловой вердикт на 4–8 фиксированных cases, затем провести тот же holdout с реальным `TranslationMachine`, если вызывающая цепочка подтверждена; не расширять тесты и не запускать 500 DPO. Full 175-doc cross-reference, полный поиск caller/branch lineage, независимый CODEX-review: **NOT_DONE**. Описаны новые кандидаты относительно сверенных README, N23/N27 и DIF-14; отсутствие их во всех старых файлах не доказано.
