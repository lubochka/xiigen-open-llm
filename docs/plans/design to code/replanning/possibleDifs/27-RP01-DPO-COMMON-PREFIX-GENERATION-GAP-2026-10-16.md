# RP01 / N27-P — DPO не противопоставляет общий старт 500 ответов; быстрый тест prefix-conditioned decoding

**2026-10-16 · SOURCE-CONFIRMED DATA SHAPE · CAUSAL HYPOTHESIS · DRAFT.** Core источник: [8c7d41611aecba98d3d1bbdbf30d69bd784949ae](https://github.com/lubochka/llm_mvp_core/tree/8c7d41611aecba98d3d1bbdbf30d69bd784949ae). Реальное дообучение, quality A/B и независимый CODEX/Astra-review для этого нового вопроса **не выполнялись**. Документ не меняет код ядра и не переписывает исторические frozen outcomes.

## Что новое по отношению к общему «B0 не умеет генерировать»

**Проверена целиком каждая запись** исходного [C#500 train500.jsonl](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/.kiro/xiigen-slices/work/results/RP01-astra-synthetic-dev-csharp-500-20261015/train500.jsonl); версия корпуса и источник задаются [materialize_dataset.py](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/.kiro/xiigen-slices/work/results/RP01-astra-synthetic-dev-csharp-500-20261015/materialize_dataset.py).

Авторский генератор формирует `chosen = f"It writes {good} because {yes}."`, `rejected = f"It writes {bad} because {no}."`. Все 500 пар **побайтово в UTF-8** имеют одинаковое начало ответов:

- `It writes ` — 500/500 chosen **и** rejected, 10 символов;
- полный общий символьный префикс: ровно **10** символов у **318** пар, **11** у **142**, **12** у **40**;
- средняя длина общего символьного начала **10.444**;
- оба ответа всегда отличаются далее (авторский assert и независимое сравнение); 25 семей — не 500 независимых законов.

**Критическое ограничение:** это **символьный** префикс. Число и идентичность первых BPE-токенов необходимо проверить на *том самом* frozen tokenizer. Нельзя писать «первые N token IDs совпадают», не декодировав IDs отдельно для chosen/rejected.

[Exact material](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/11.DPO/Models/DpoExactPairMaterial.cs#L29-L78) отдельно кодирует prompt/chosen/rejected и делает response-only mask. [DpoTrainer.StepVectorCore](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/11.DPO/DpoTrainer.cs#L866-L922) повышает chosen относительно rejected; [ComputeTargetLogProbs](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/11.DPO/DpoTrainer.cs#L1028-L1067) использует сдвинутые response targets. Для идентичного response-prefix противоположные непосредственные loss-вклады на совпадающие позиции могут взаимно сокращаться. Это **не** означает нулевой общий градиент от префикса: общие параметры, attention и позиции после разветвления могут косвенно влиять на выбранный первый токен.

**Существующее наблюдение RP01:** в [recovery audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) первым токеном при C50/C100 остаётся ID 388, хотя logits изменились. [CodeDocumentationStudentGeneration](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L75) запускает обычный `GenerateDetailed` с неизменным owner prompt; не предоставляет обучающий префикс ответа. Таким образом **нет доказательства**, что DPO научил случайный B0 начать ответ так, как начинаются все chosen примеры.

## Гипотезы, которые нельзя смешивать

| ID | Возможное объяснение | Изолирующая проверка |
| --- | --- | --- |
| **H27-P1, непосредственное начало** | Student научился некоторым условным C# закономерностям только *после* `It writes `, но не умеет сам выдавать этот префикс | При неизменной модели принудительно подать только этот универсальный prefix как **ответные** токены и сравнить свободное продолжение |
| **H27-P2, отсутствие знания** | Даже после принудительного начала продолжение неверно — DPO не научил закону | Forced-prefix continuation на B0 и C50, сравнить по new-source C# задачам |
| **H27-P3, выбор семплера** | Данные не дают достаточного first-token preference; greedy преждевременно фиксирует неверную траекторию | Сохранить next-token distributions/rank gold prefix, отдельный single-variable decode contrast только после P1/P2 |
| **H27-P4, токенизация** | Byte/text prefix равен, но первые segment BPE IDs или framing расходятся, что создаёт иные позиции градиента | Дамп actual tokenizer IDs, response mask, prompt ID span и loss contributions (без внешнего teacher) |

Это **дополнение** к уже описанным cold-start, tokenization и generation gaps, не отдельное доказательство «сломанный LoRA» и не повод менять эти механизмы одновременно.

## FAST-MICRO-PREFIX-01 — одна очень дешёвая качественная интервенция без DPO-тренировки

**ExperimentId:** `RP01-MICRO-PREFIX-01`. **Tier:** exploratory mini (2 saved Student states, всего 3 исторические C# exam prompts + 8–12 новых source-disjoint C# tasks). **Задача:** выяснить, есть ли вообще скрытое conditional knowledge после общего префикса. Не размечать новые gold cases, просмотренные моделью/автором фикстур.

**Baseline A:** обычный `CodeDocumentationStudentGeneration.Generate` на неизменном B0; такой же на сохранённом C50, оба с теми же inference limits и tokenizer.

**Treatment B:** в отдельном disposable shadow-generator append к `tokenizer.Encode(prompt)` лишь `tokenizer.Encode("It writes ")` **как реальные preselected response tokens**, не меняя модель, весов, reference, prompt, температуру и набор тестовых задач. Далее выполнить обычное авторегрессионное continuation (например 64 новых токена). **Никаких правильных чисел, примеров решения или предметных правил не подставлять.** Следить за точным BPE segment boundary и EOS/no-EOS; не подменять эту интервенцию на изменение user prompt.

**Основная метрика:** `domain_law_correct_full_answer / N` по слепой оценке; отдельно частота неправильных чисел, отсутствующих причин, обрывов, abstain; `N` не увеличивать повторением одних и тех же 3 случаев. Secondary: conditional chosen continuation log-odds, first-token rank, tokenizer span identities, stop reason, free-generation token count.

**4 клетки без смены независимых переменных:** `B0 raw`, `B0 forced-prefix`, `C50 raw`, `C50 forced-prefix`. Вычислить `(C50-forced − B0-forced)` и `(C50-raw − B0-raw)`; отдельно эффект самого prefix для обеих моделей. Равный compute на продолжение — считать общую длину response с 10-символьным prefix, а не выдавать B больше токенов.

**Интерпретация:** Если C50 forced правильнее и B0 forced остаётся неправильным, это **узкий сигнал**, что часть изученного поведения доступна условно, а интерфейс начала ответа теряет качество. Если B0 forced≈C50 forced — вероятно, prefix помогает framing, но learning-gain всё ещё не доказан. Если обе forced ≈0, нельзя обвинять только старт: приоритет у [RP01.REC02](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC02-DPO-GRADIENT.md) и [RP01.REC03](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC03-CAPABILITY.md). Если возникают ошибки law при forced, фиксировать ухудшение, а не только readability.

**Порог объявления полезного обучения:** независимая семантическая correctness должна вырасти и не увеличить critical errors на new-source семействе; 8–12 случаев — исключительно пилотный сигнал, не generalization proof. После него *отдельный* one-factor SFT chosen-only warmup vs DPO-only может тестировать формирование способности начинать ответ; не менять pair data, расписание и LoRA одновременно.

## Нормативная граница

- Автор выбранных и отклонённых ответов именно этого исторического C# корпуса — `simulation`/DeterministicGenerator; не external Qwen.
- Новые production DPO стороны должны иметь подлинную, разрешённую provenance от открытой китайской модели; CODEX/Astra выполняет независимый человеческий выбор и оценку, а не заменяет авторство задним числом.
- **Нельзя:** менять original 500, frozen eval/rubric, скрывать нулевой результат, объявлять проверку символьного prefix проверкой BPE IDs, выпускать C50 как production checkpoint после одного shadow probe.
- **Состояние:** собственное текстовое source/proxy измерение завершено; Student shadow infer, .NET native и independent Astra review **NOT_RUN**; артефакт DRAFT.

**Применены:** архитектурное трассирование, code-examination, experiment-design, plan-review и prompt-patch-authorship (один bottleneck за один контроль), human-readable-architecture-audit.