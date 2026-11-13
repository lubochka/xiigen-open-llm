# NEW-26 — E30: один документный вектор для разных учебных переходов; прежний FAIL нельзя переносить на другой KNN

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Актуализация доказательств 01.11.2026 — E30/P47

3c13e6df01d9 добавил current action в44-dimensional native experimental address; next/gold в добавленный блок не входит, saved9/9. Старое SOURCE-only описание больше не означает отсутствие этого ремонта. Приёмка теперь требует per-file transitions, current/next separation и actual caller при product claim; helper/reflection не доказывает ProductHost gain.

Основание: [2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md), закреплённый core `790228c256168a336b01abae4387e28391048c0c`. Старый текст и его ограничения относятся к указанной там дате; новый блок уточняет текущий статус, не переисполняет исторический опыт и не меняет научную очередь.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**16.10.2026 · independent targeted source audit · SOURCE_CONFIRMED для конкретных операций; quality impact HYPOTHESIS; native E30 run NOT_RUN; CODEX/Astra independent review NOT_RUN.**  
**Source pin:** [llm_mvp_core@8c7d41611aec](https://github.com/lubochka/llm_mvp_core/tree/8c7d41611aecba98d3d1bbdbf30d69bd784949ae), ветка codex/dpo-implementation. **Target:** только документация в xiigen-planning/llm_plan3/possibleDifs; core/docs/plans/replanning-possible-diffs не изменять.

## Главный вывод для человека

Старый опыт E30 проверял обучение предсказанию **следующего логического предиката в исходном C# файле**. Его обучаемая память получала вектор признаков **всего файла**, но правильный ответ различался для **каждого перехода внутри него**. Поэтому по одному и тому же адресу памяти записывались потенциально противоположные метки. При поиске кэш даже не знал текущий предикат. Он мог извлечь первый сохранённый ответ вместо правильного продолжения. Это не ошибка матричного умножения, а потеря информации при превращении файла в обучающий признак.

Такой механизм даёт **новую конкретную гипотезу** о прежних неудачах KNN: возможно, отказали не сами nearest-neighbour алгоритмы, а неверно определённый учебный объект и адрес его запоминания. Но нет доказательств, что E30 был перенесён в текущий RP01, что исправленный E30 когда-либо был успешным, либо что это вызвало сегодняшние C#0/3 или EN→RU0/4.

## 1. Первичные доказательства: одна точная цепочка от данных до решения

| Шаг | Source evidence | Что непосредственно следует |
| --- | --- | --- |
| Формирование пар | [E30PredicateCache.cs:21–29, 91–104](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E30PredicateCache.cs#L21-L29) | Для каждого файла извлекается последовательность предикатов. Метод вычисляет **один** BuildVector(text), затем использует его для **всех** пар (Current, Next) в файле |
| Состав признака | [PredicateCache.cs:27–43](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Infrastructure/PredicateCache.cs#L27-L43) | BuildVector строит 22D частотный вектор из целого text; не принимает индекс перехода, Current, позицию и порядок соседей |
| Обучение | [E30PredicateCache.cs:43–46](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E30PredicateCache.cs#L43-L46) | На обучающих переходах вызывается cache.Add(p.Vec, p.Next); поле p.Current НЕ участвует |
| Выбор ответа | [PredicateCache.cs:15–26](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Infrastructure/PredicateCache.cs#L15-L26) | Lookup(queryVec) выбирает максимальный cosine. При равном cosine условие строго sim > bestSim оставляет **первую** метку, даже если последующий урок противоречит ей |
| Fallback | [E30PredicateCache.cs:47–64](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E30PredicateCache.cs#L47-L64) | Только при cache MISS учитывается p.Current через bigram. При cache HIT именно этот полезный контекст пропускается |
| Архивные числа | [E30 run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/results/E30-PREDICATE-CACHE/results/run-2026-05-01-001.json) | tier2: baseline 0.3225, enhanced 0.1667, ухудшение −48.31%; 128/276 cache hits, **cacheAcc=2.3% при hitRate=46.4%**. Реальный сохранённый запуск был отрицательным. Этот run не изолирует наш механизм как единственную причину |

**Контрпример, следующий из исходников.** В файле предикаты идут A → B → C. Для переходов (A,B) и (B,C) записан **тот же** вектор V всего файла. KNN store получает (V,B) и (V,C). Любой запрос V делает одинаковое ранжирование и выбирает первый B; для Current=B ожидаемый C никогда не может быть различён по V. Порог cosine тут бессилен: оба cosine равны 1.0. Этот контрпример зависит от наличия в файле разных Next; наличие таких конфликтов в каждом реальном документе **не подсчитано**.

## 2. Новые проверяемые гипотезы (не равны подтверждённой деградации продукта)

**NEW26.01 — условная информация потеряна до nearest-neighbour.** Формальная проверка: I(Next; Current | FileVector) > 0 на последовательностях, содержащих более одного типа перехода. При одном FileVector и без Current любая детерминированная функция F(FileVector) выдаёт одно значение для всех переходов. Нулевая H0: текущий предикат не добавляет прогностической информации в реальном valid-set либо fallback всегда корректно спасает выбор. Опровержение H0: native matched heldout показывает прирост правильного следующего предиката при единственном изменении адреса памяти на (FileVector, Current).

**NEW26.02 — first-wins при корректирующем уроке.** Даже если позже добавлен новый label под тем же идентичным vector, Lookup оставляет первый при cosine ties; порядок train rows становится скрытой детерминистической policy. Это локально доказанная операция, **не** доказательство появления таких коррекций в настоящем ходе непрерывного обучения. Фальсификатор: два одинаковых вектора с противоречащими labels, переставить insertion order; если выбранный label меняется вместе с порядком, нет арбитража по актуальности/праву/quality. Не сливать этот вопрос с другим случаем хронологии в TranslationKnnStore из [16-KNN-CORRECTION](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md).

**NEW26.03 — E30V2 не является эквивалентным повтором E30.** В [E30PredicateCacheV2.cs:9–18, 19–82](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E30PredicateCacheV2.cs#L9-L82) задача меняется на выбор из восьми NL категорий по word-Jaccard и статической таблице из 16 примеров; исходная задача следующего C# predicate, original train/test files и условные переходы не сохранены. Результат [E30V2 run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/results/E30V2-PREDICATE-CACHE/results/run-2026-05-01-001.json): baseline 0.1875, enhanced 0.125, −33.33%, FAILED. Это отдельная отрицательная NL-панель, **не причинный A/B proof** исправления E30.

**NEW26.04 — bag-of-predicates теряет структурный порядок и контекст управления.** BuildVector агрегирует частоты предикатов по всему файлу. Два разных порядка с одинаковыми частотами неразличимы, даже без проблемы Current. Это E30-конкретный случай уже существующего родительского [DIF-13 о bag/order](12-DIF-01-44-CHEAP-CAUSAL-PROBES.md); **не считать его ещё одним полностью независимым discovery**. Для C# стоит сравнить локальное окно предикатов, роль AST, scope и полярность, а не автоматически заменять KNN глубокой сетью.

## 3. Проверка соседних веток вместо предположения по названию

Полные содержательные тела трёх исходных файлов E30PredicateCache.cs, E30PredicateCacheV2.cs, PredicateCache.cs непосредственно сопоставлены через GitHub connector в ветках **codex/dpo-implementation, language_translate, DesignToCode, external_domains, orchestrator-extensions, main**. Текущие blob SHA отличаются от старых из-за добавления пяти строк лицензии; после удаления только этого header тело всех трёх файлов совпадает. Это подтверждает сохранение старой реализации **в прочитанных ветках**. Нельзя из этого делать вывод, что в других файлах/экспериментах веток не было успешной функциональной замены. Поиск actual production caller этого cache вне E30 остаётся OPEN.

## 4. Соответствие более ранней программе и предотвращение дублирования

Ранее существующие [44 DIF](12-DIF-01-44-CHEAP-CAUSAL-PROBES.md), [KNN-identity/predicate metadata](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [E25/E26 и production rules](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md), [E40V2 learned KNN vs fixed rule](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md) показывают отдельные области. В доступных каталогах и тематических уже прочитанных файлах E30 condition-erasure не найдено, но **поиск по всем 122 документам каталога не выполнялся**, поэтому абсолютную уникальность утверждать нельзя. Подлинный указанный владельцем случай «матрица FAILED → функция KNN успешна» остаётся не локализован до конкретного migration commit; E30 **не объявляется тем самым случаем**.

**RP01 boundary:** нынешний C#500/ENRU500 изменял q/v LoRA Student; код E30 не является доказанным частью его output path. Историческая ошибка E30 объясняет возможную потерю полезной KNN-функции **при архитектурном переносе**, но не является доказанной первопричиной RP01 Student free-output 0/3,0/4. Не объединять DPO margin с retrieval accuracy.

## 5. Исполнимый короткий опыт и договор завершения

Эксперимент: [MICRO-26 E30 paired native work order](26-MICRO-E30-CONDITIONED-KNN-CAUSAL-EXPERIMENT-2026-10-16.md). Вспомогательный [Python proxy](26-MICRO-E30-CONTEXT-IDENTITY-PROXY.py) и [результат](26-MICRO-E30-CONTEXT-IDENTITY-PROXY-RESULT.json): на **синтетических одинаковых** 22D векторах, 12 train transitions и 12 heldout transitions, первая метка кэша дала 6/12, а включение Current в lookup дало 12/12; испорченный label контроль 6/12. Это **только точная иллюстрация неоднозначности и зависимости от обучающих labels**, не родной C# запуск, не неизвестные смысловые семьи, не демонстрация product gain.

**Следующая проверка для CODEX:** одно строгое изменение feature key, одинаковые raw files, train/test identities, seed/split, legal source oracle и budget; сравнить base и conditioned **на незнакомых source families** и затем actual consumer if present. Невоспроизводимый/отсутствующий caller = TRACE_ONLY, не переходить к внедрению. Ни 500 DPO, ни Windows CI для этого вопроса не нужны.

## 6. Непереопределяемый протокол ролей

- **CODEX / GPT-6 Astra 6 UltraCode** делает человеческий выбор учебной задачи, закона, новых train/heldout семейств, принимаемой гипотезы.
- Если опыт расширяется до **DPO**: разрешённая внешняя открытая **китайская** модель генерирует **обе** стороны chosen и rejected с неизменными model/license/provider/actual-actor receipts; независимый **CODEX / GPT-6 Astra 6 UltraCode** проверяет качество этих конкретных пар и принимает/reject их *до* обучения.
- **CODEX / GPT-6 Astra 6 UltraCode** отдельно и вслепую оценивает семантическую корректность learned Unit / Student по source-disjoint задачам. Авторская/встроенная эвристика не заменяет этот judge. В этом исследовании новых DPO и таких CODEX-судейств **не было**.

**Skills применены как метод проверки:** [architecture](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md), [phase planning](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/phase-planning-SKILL.md), [plan review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [code examination](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/code-examination-SKILL.md), [experiment design](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md), [prompt patching](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md), [human communication](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/conversation-intent-and-self-reflection-SKILL.md). Это авторский разбор, а не независимая модельная приёмка.
