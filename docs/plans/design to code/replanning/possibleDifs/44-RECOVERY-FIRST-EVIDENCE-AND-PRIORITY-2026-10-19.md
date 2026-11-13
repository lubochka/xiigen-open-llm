# N44 — В первую очередь вернуть измеренное обучение: эталоны, новые различия и отрицательные контроли

**Дата:** 2026-10-19. **Режим:** независимый GitHub SOURCE/RECEIPT AUDIT + DESIGN_ONLY для новых опытов. **Срезы:** `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, `PrepareToGPL@31346d8928be4469b01aaf313780e8f950ff91e2`, `codex/dpo-implementation@a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`. **Целевая ветка документации:** `xiigen-planning@llm_plan3`. **Запрещено изменять:** `llm_mvp_core/docs/plans/replanning-possible-diffs/**`.

## 0. Решение по приоритету

**Не заменять прежнюю работавшую обучаемую память исправлениями DPO-градиента, если обычный потребитель ответа остаётся другим.** Сначала восстановить и померить конкретные положительные recipients и их обычные вызовы, после — исследовать новый neural Student. Результат «weights changed», «DPO PASS», «structural_accuracy=1» и исторический успех kNN имеют разные знаменатели и не складываются в одну оценку качества.

### Приоритет P0: доказанные исторические способности — только в своих доменах

| Место | Домен / обученный получатель | Первичный сохранённый результат | Что вернуть и как фальсифицировать | Статус новизны |
|---|---|---|---|---|
| **P0-01** | Перевод, `TranslationMachine` / `ExecutorUnit` / сохранённая kNN-память | В [существующем пакете N43](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/README.md) уже подтверждены 11/24→19/24 при доставке частей урока, отдельный cold-restore 24/24 на специфических примерах; старый checkpoint с 4 направлениями не проходит современный шестинаправленный loader без явной миграции | 4/6-направленная совместимость, идентичность урока, точный ordinary flow после нового процесса; не возвращать недопущенные данные | **УЖЕ ИЗВЕСТНО, повышено в очередь** |
| **P0-02** | C# AST / E09 structural kNN | [Исходный JSON E09](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json): top-1 **0.2413→0.3507**, 4 492 train pairs, k=5, время 14.5→763.5 ms | Ровно этот обученный структурный поиск подключить к C# consumer и сравнить на независимых семействах исходников. Старый top-1 ≠ правильность итоговой документации | **УЖЕ ИЗВЕСТНО** |
| **P0-03** | Перевод с контекстом / E26 contextual dictionary | [Исходный JSON E26](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json): disambiguation **0.40→1.00** (15, из них 6 hard) | Действительно передать контекст соседних POS/ролей в **learned** слово/phrase consumer; текущее E26 правило не равнозначно исторической реализации. Новый held-out набор, word-sense и грамматика отдельно | **УЖЕ ИЗВЕСТНО** |
| **P0-04** | Continual learning / E43 replay | [Исходный JSON E43](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json): old-set **0.3226→0.3978**, oldPairs 465, newPairs 414, replay 177 | Same recipient, known old/new families, with/without bounded replay; исключить phantom bigrams, пересечение corpus/file boundaries, несопоставимый scoring | **УЖЕ ИЗВЕСТНО** |
| **P0-05** | `language_translate`: R1 выбор направления + R2 выбор домена *перед обучаемым переводчиком* | Прямой diff old/current: old принимает сырую строку и optional hints; current требует SourceLanguage и принимает готовый TopicDomain ([N44-02](44-NEW-LANGUAGE-ROUTING-CONTRACT-DIFF-2026-10-19.md)) | Отделить качество *выбора маршрута* от качества *перевода*. На том же consumer проверить автоопределение языка/темы, когда их реально не предоставляет caller | **НОВОЕ source-level различие данного аудита; causal gain не доказан** |

**Неинтерпретируемые без проверки:** разнообразие домейнов/экспериментов в старой ветке не означает, что *один и тот же* q/v-LoRA Student раньше свободно переводил или писал C# документацию. Коды E09, E26, E43 — разные статистические вопросы и owners, а не общая точка качества RP01. Сравнивать только в одном домене, с одной метрикой, на непересекающихся независимых тестовых семействах.

## 1. Новая по времени запись: после улучшения градиента качество всё ещё не показано

В защищённом core-каталоге (прочитан **только на чтение**) уже опубликованы [индекс native опытов 18 октября](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/README.md), [первичный отчёт](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md) и [независимый review](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/independent-neural-eight-lesson-review/review.md).

* Native эксперимент с legacy fractional response mask, **8 накопительных DPO updates** по 8 выбранным EN→RU simulation парам: **0/24→0/24** независимых правильных ответов, **24/24 сгенерированных текста совпали**. Все 16 LoRA тензоров получили обновления; finite-difference проверка **126/162→162/162**. Исправленная математика **сама по себе не вернула перевод**. Это экспериментальный host, не ordinary production CLI и не доказательство, что любой DPO обречён.
* Отдельное исправление Embedding backward позволило менять **38 400 координат** в supervised full-model SFT и изменило **21/24** текста, но correctness опять **0/24→0/24**. Это другой обучаемый recipient (55 trainable tensors, не frozen-base q/v-LoRA); эффект обновления параметров не равен semantic gain.
* Linear bias backward стал локально правильнее, но в проверяемом C50 `useBias:false`, следовательно, это **не доказанное объяснение RP01**.

**Решение:** пометить legacy-mask-only и «починить autograd ⇒ будет лучше» как **недостаточные для P0 восстановления**, пока не показан хотя бы один новый корректный ordinary ответ при неизменном независимом судье. Не переписывать прежние 0/24, не менять rubrics после просмотра результатов, не выдавать proxy/finite difference за native quality.

## 2. Разделение нового и известного (чтобы не плодить дубли)

| Семейство | Где уже исследовано | Дельта N44 |
|---|---|---|
| 4→6 checkpoint, schema/loader, legality, feed-back ordinary translation | [N43](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/README.md), [N41](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md) | Только приоритет, не новая находка |
| Морфологические признаки EN/RU/HE, family-size и homonyms | [N13](13-LANGUAGE-TRANSLATE-BRANCH-MORPHOLOGY-GAPS.md), core protected `09-domain-functions-and-sibling-branches.md` | Учитывать доменную условность; не называть новую |
| DPO, LoRA, BPE, gradients, structural kNN, replay, hidden rules, learning Units | [00 master](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [N42](42-NEW-CAUSAL-HYPOTHESES-AND-DOMAIN-MICROEXPERIMENTS-2026-10-18.md) | Новый **сквозной historical R1/R2 contract comparison**, учёт 18.10 native negative-result, проверочный протокол consumer parity |
| RAG/отсылки в родительские директории | [WorkLeft RAG master](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), [RP01 sources](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md) | Переносить только первичные evidence pointers, не отменять gates по DPO авторству и legal admission |

## 3. Единый экзамен успешного переноса

Требовать: `source SHA → provenance/PairId → train-admission → trained state delta → checkpoint identity → cold restore → **real normal consumer** → raw answer → independent domain metric`. Отдельно отчёт по ошибкам, времени/памяти и отказам. Любая пропущенная стрелка помечается `NOT_YET_PROVEN`, а не `PASS`. Искомое восстановление — не повышение количества тестов или изменение интерфейса без результата, а причинная зависимость обычного ответа от нужного выученного урока.

**Далее:** [сравнение новых швов language_translate](44-NEW-LANGUAGE-ROUTING-CONTRACT-DIFF-2026-10-19.md) → [малые эксперименты с фальсификаторами](44-SMALL-CAUSAL-EXPERIMENTS-BY-DOMAIN-2026-10-19.md) → [synthetic fixture](44-ROUTING-CAUSAL-FIXTURE-2026-10-19.json).
