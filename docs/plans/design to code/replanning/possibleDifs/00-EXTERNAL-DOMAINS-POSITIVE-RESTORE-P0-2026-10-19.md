# ГЛАВА 0 — external_domains: восстановление реально показавших результат механизмов

**Дата:** 2026-10-19. **Источник успешных исторических экспериментов:** `llm_mvp_core@external_domains`, commit [3036a01](https://github.com/lubochka/llm_mvp_core/tree/3036a01acec938f0b83fa4ffbda26202cc35d639). **Реализация для сопоставления:** [codex/dpo-implementation](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation); [RP01](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/). **Разрешённая запись:** только `xiigen-planning/llm_plan3/.../possibleDifs/`. **Запрет:** не изменять `llm_mvp_core/docs/plans/replanning-possible-diffs/**`.

## Критическое уточнение из независимого cross-branch code review

**[N51: подтверждено, что E09/E26/E43, StructuralKnn и HnswIndex в двух ветках кодово идентичны после снятия пяти строк лицензионного заголовка](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md).** Поэтому P0 — **подключить, проверить и перенести полезный механизм к действующему обучаемому получателю**, а не заново писать классы исторических алгоритмов. Качество на реальном consumer ещё не измерено.

**Корректировка статуса E26:** 0.40→1.00 показаны на 15 заранее заданных случаях с `ContextualDictionary.BuildDefault()`, который содержит вручную предзагруженные POS-записи; в E26.Run отсутствует `Train`. Это **положительный опыт контекстного поиска**, **НЕ подтверждённый исторический опыт online-learning**. Прошлая фраза «историческое успешное обучение E26» без этого уточнения была бы завышенной. Новый learned translation store и холодное восстановление требуют отдельного опыта.

## Решение P0: восстановить доказанную способность, не просто повторить старую архитектуру

**Цель:** на том же домене и том же проверяемом конечном вызове получить не худшее качество, чем в независимом замороженном эталоне, сохранив юридический допуск данных, осмысленность результата, восстановление состояния и бюджет ресурсов. Существование исходного алгоритма или сохранённый `passed=true` не являются достаточным доказательством текущего качества.

| Порядок | Домен / конкретная проверка | Исторический первичный результат и доказательство | Что воспроизвести | Важное ограничение |
|---|---|---|---|---|
| **P0-E09** | Языки программирования: C# AST / следующий структурный элемент | [E09 сохранённый JSON](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json): top-1 `0.2413→0.3507`, `k=5`, `trainPairs=4492`, время `14.5→763.5 ms` | Extractor C# AST → вектор синтаксических признаков → KNN → решение конкретного C# consumer; matched-file-heldout, фиксированные `k`, тип метки, latency | Старые baseline и enhanced проверялись на разном количестве элементов; source-disjoint не установлен. Нельзя утверждать причинный +10.94 п.п. |
| **P0-E26** | Перевод: контекстное значение слова, POS/окружение | [E26 сохранённый JSON](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json): disambiguation `0.40→1.00`, 15 случаев, 6 неоднозначных | Контекст до lookup — реально доступен обучаемому переводческому Unit/Flow; заменить контекст на контрольный и проверить изменение выбора | Очень маленький узкий набор, это НЕ доказательство общей точности EN↔RU или переносимости на SQL |
| **P0-E43** | Continual learning / удержание прежнего знания | [E43 сохранённый JSON](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json): old-set `0.3226→0.3978`, `oldPairs=465`, `newPairs=414`, `replay=177` | На **одном** обучаемом получателе сравнить последовательное дообучение с replay и без него; old+new heldout, восстановление после рестарта | Исторический получатель не тождествен нынешнему DPO q/v LoRA Student |
| **P0-SOURCE** | DPO документация: C#, SQL, Angular, MongoDB и Elasticsearch | [Новый первичный аудит 15 JSONL](50-NEW-EXTERNAL-DOMAINS-PAIR-IDENTITY-AND-INVERTED-PREFERENCES-2026-10-19.md): 3 000 пар, **270 тождественных сторон**, **219 кандидатов с обратной шкалой**, только 55 разных `Prompt` | Перед новым neural run проверить, какой корпус реально принят, исключить контрастные ошибки и разнести train/test по исходной задаче/шаблону | Эти исторические `external domains/documentation/learning sets` **не доказаны входом RP01 C#500**; исправление не объясняет RP01 без доказанной цепочки загрузки |
| **P0-CONSUMER** | Все перечисленные темы | [RP01 source/architecture](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md), [разбор learning recipient](00-MASTER-SYNTHESIS-AND-CONFLICTS.md) | Проверить обученное состояние → saved asset → load → ordinary application call → фактический ответ и ссылки на источник | Разделять улучшение от поиска/памяти и изменение нейросетевых весов. Нельзя суммировать метрики разных задач |

### Отдельно о E22 и других «неудачных» опытах

Исторический [E22 в самой external_domains](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/results/E22-DATASET-ROUTER/results/run-2026-05-01-001.json) **неуспешен**: `0.50→0.50`, `passed=false`, `val=4`. В [исследовании позднего E22](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md) документировано позднее `0.4709→0.5646`, **но с иным корпусом и изменённой функцией меток**. Это повод проверить перенесённую в KNN полезную функцию, а не объявить ранний отрицательный результат опровергнутым. Точное перенесение matrix→KNN пока требует `donor commit + старый оператор/признак + новый потребитель + matched causal gain`; см. [исторический разбор](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md).

## Архитектурный контроль переноса

```text
Исторический источник (branch + commit + независимый result)
    → конкретная предметная задача / тип ответа / метрика
    → извлечение признаков (в т.ч. syntax tree или порядок слов)
    → ОБУЧАЕМЫЙ получатель: KNN / словарь / память / LoRA / Unit
    → проверяемое сохранение и восстановление
    → реальный application consumer: обычный запрос, не internal gate
    → blind heldout semantic quality + latency/RAM + failure examples
```

**Синтаксическое дерево:** только в тех задачах, где есть корректный parser и правило синтаксически зависит от AST (C#, в будущем отдельно SQL или TypeScript при собственном extractor). Не активировать признак AST для обычного перевода.

**Порядок слов / фраз:** полезность проверять на contrastive парах перевода с отрицанием, ролью субъекта/объекта, изменённым порядком и тем же набором слов; контекст POS может повышать качество конкретных неоднозначностей. Не применять перестановку без семантического согласования.

**Replay:** проверять как свойство сохранения старого навыка конкретного получателя; нельзя смешивать результат bigram с LoRA, не проверив совместимость optimizer+index+vocabulary.

## Границы доказательств и порядок выполнения

1. **Исторический source+receipt:** E09/E26/E43 действительно есть в `external_domains`. Это **VERIFIED_HISTORICAL_RECEIPT**, не наш новый native learning run.
2. **Новый source audit:** 15 архивных DPO JSONL действительно содержат аномалии; точные выборки и ограничения — [N50](50-NEW-EXTERNAL-DOMAINS-PAIR-IDENTITY-AND-INVERTED-PREFERENCES-2026-10-19.md). Это **SOURCE_MEASURED**, не доказанное ухудшение RP01.
3. **Новый причинный план:** [короткие предметные эксперименты](50-EXTERNAL-DOMAINS-CAUSAL-EXPERIMENTS-2026-10-19.md). **DESIGN_ONLY / NATIVE_NOT_RUN**.
4. **Граница принятия:** допускается только `success` на конечном heldout вызове с frozen prompts, judge rubric, группировкой source-family и прозрачными cost/latency. Исправлять названия показателей вместо алгоритма запрещено.
5. **Не смешивать источники:** внешний synthetic исторический CODE_TO_DOC, RP01 simulation C#500, переводческий перевод, Wiki routing и E09 AST — разные наборы/получатели до появления проверенного lineage. Не обходить ограничения provenance и права на данные.

### Промпт для независимого code+quality review

**Текущая задача:** для каждой P0 способности установить `source path:line → donor revision → recipient state → training call → persisted identity → restored identity → ordinary consumer → heldout semantic outcome`. **Не задача:** запуск полного Windows CI, изменение тестов ради PASS или правка `core/docs/plans/replanning-possible-diffs/**`. **Хороший вывод:** «C# E09 top-1 подтвердился только на собственном наборе; конечная документация ещё не измерена». **Плохой вывод:** «E09 прошёл, значит RP01 восстановлен». Для каждой неизвестной связки писать `NOT_TRACED`, не заполнять догадкой. Навыки: архитектура, планирование, ревью планов и кода, эксперимент, промпт/человеческая коммуникация из [skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills).
