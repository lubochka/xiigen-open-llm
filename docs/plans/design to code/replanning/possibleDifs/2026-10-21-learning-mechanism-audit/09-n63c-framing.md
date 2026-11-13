# N63C-PROBE: учебная строка и контекст измерения различаются

**SOURCE_CONFIRMED / KNOWN_P07_FAMILY / NEW_PROBE_APPLICATION / CAUSAL_EFFECT_NOT_MEASURED.** Это конкретизация известной семьи токенизации и prompt framing, а не ещё одна новая общая гипотеза. Приоритет — исправление измеримости обучения перед новыми затратами. Никакой эксперимент здесь не выполнен и новый execution ID не назначен.

## Подтверждённая цепочка

| Звено на core256 | Конкретное действие |
|---|---|
| [Probe:124–133][acquire] | Acquisition учит `tokenizer.Encode(fixture.Prompt + "\n" + fixture.Chosen)` настоящим `TrainSupervisedNextTokenStep` |
| [Probe:352–363][arm] | В обеих SFT-руках тот же joint Encode с дополнительным LF |
| [Probe:388–399][prepare] | Панель собирается через `LogicalDpoPreparedBatchMaterializer.Prepare` |
| [Materializer:14–20][materializer] | Явно выбирается `DpoExactPairMaterial.Version`, не legacy framing |
| [LogicalDpoTrainingUnit:2378–2380][unit] | Exact ветка передаёт raw Prompt/Chosen/Rejected в CreateBatch |
| [DpoExactPairMaterial:18–22,63–78][exact] | Версия `prompt-response-segments-no-separator-v2`; prompt и response кодируются отдельно и конкатенируются без LF |
| [Probe:434–474][score] | SideScore делает Forward на этих exact IDs и оценивает response-mask targets |

[Фактический fixture][fixture] имеет `Prompt="// file: Abs.cs"`, без завершающего LF. Поэтому обучающая текстовая строка начинается `// file: Abs.cs\nstatic int Abs…`, а decoded exact carrier — `// file: Abs.csstatic int Abs…`. Дополнительно joint BPE и отдельное кодирование сегментов не обязаны иметь одинаковые token boundaries. Это разные условные задачи на одной policy; наличие одного tokenizer не делает контекст одинаковым.

## Что объясняет и чего пока не доказывает это различие

В сохранённом [raw][raw] acquisition loss падает до 0.000028034774, но exact-DPO readout остаётся 22/23, chosen NLL≈1.1099185. Эти числа относятся к разным целевым последовательностям/условиям; их расхождение само по себе не доказывает сломанный backward или невозможность доучить модель.

**Гипотеза:** mismatch контекста, особенно перед первым response token, может мешать выполнить условие полного ответа в этом readout даже при хорошем освоении обучающей строки. В raw нет per-position predictions, поэтому неизвестно, какой из 23 токенов ошибочен. Нельзя утверждать «ошибка точно в первом токене», «виноват только LF» или «выравнивание даст 23/23». Параметры completed raw также расходятся с current source; эта граница отдельно сохранена в 08.

Одинаковый framing mismatch у A/B не стирает уже измеренный B-over-A gain по токенам/NLL/margin. Он ограничивает трактовку абсолютного before и удержания полного ответа. **Результат stale-SFT по своей оси остаётся кандидатом.** Полный ответ, обобщение и причинный вклад framing требуют своего измерения.

## Где исправлять постановку и что должен проверить малый опыт

Для следующей авторской версии семейства P07/N63C следует заранее выбрать одну задачу: либо учить и проверять один и тот же carrier, либо честно назвать измерение переносом между двумя форматами. Нельзя незаметно изменить scorer после обучения, объявить более удобные числа успехом или смешать framing fix со stale-response A/B.

Конкретный будущий контрфактор в экспериментальной копии: A сохраняет текущие joint IDs `Encode(Prompt+LF+Chosen)`, B обучается на exact material `EncodeSegment(Prompt) || EncodeSegment(Chosen)` того же learner/tokenizer. Меняется один заранее названный **контракт обучающего carrier**; это одновременно включает разделитель и границы BPE, поэтому эффект нельзя приписывать только удалению одного символа. Выделение LF и сегментации по отдельности потребовало бы отдельных заранее назначенных опытов, не скрытого третьего плеча.

В обеих руках нужен **настоящий SFT update**, одинаковое подтверждённое исходное состояние и конечный бюджет; измеряются до/после полный ответ, правильные response-токены, per-position NLL/rank и TRAIN/EXAM/CONTROL на обоих фиксированных carriers. Изолированное сравнение token arrays, чтение JSON или повторный scoring без Train не отвечает вопросу о влиянии на обучение. Для восстановления можно использовать только явно проверенный producer/weights/optimizer contract; текущий 600-step raw не превращается в рецепт 60 steps.

**Это авторская поправка к известной семье, не runnable карточка.** Исходный `N63C-PROBE` и его `results`, `results-02`, `results-03` не перезапускаются; новый host/entry и достаточный budget должны быть зафиксированы автором в исправленной карточке после установления exact source/parent. Исполнитель не ищет их сам. Не создаётся новая строка очереди или второй alias P07 ради количества гипотез. [Известное покрытие P07 и прежний незакрытый same-model вопрос][prior] сохранено с атрибуцией.

## Правила следующего поручения

Модель — по умолчанию текущей сессии; отчёт — русский Markdown в назначенном result-root карточки. Skills из `C:/Projects/llm_mvp_core/docs/skills/`: architect-behavior-classifier, phase-planning, plan-review, code-examination, experiment-design, prompt-patch-authoring, human-readable-architecture-audit и human-communication-GLOSSARY, с точными файлами как в 07. Применять текущий protocol30, не старое разрешение расходовать заранее недиагностичный бюджет.

Любое измеренное полезное улучшение сохраняется по своей метрике и домену, рост accuracy не обязателен; ухудшения рядом. Нулевые полные ответы не закрывают цель полного ответа. Production и protected docs здесь не меняются; старые эксперименты/результаты не удаляются. **По NEXT немедленно перейти дальше, без завершения прежних report/hash/review/Git/retry; по STOP немедленно прекратить работу.** Для отдельного документного уточнения NEXT=END. При будущей разрешённой операции ждать тот же session/PID; тишина вывода не означает отказ и не разрешает повтор.

[acquire]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L124-L133
[arm]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L352-L363
[prepare]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L388-L399
[materializer]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/18.CapabilityTraining/LogicalDpoPreparedBatchMaterializer.cs#L14-L20
[unit]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2378-L2380
[exact]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/11.DPO/Models/DpoExactPairMaterial.cs#L18-L78
[score]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L434-L474
[fixture]: https://github.com/lubochka/xiigen-planning/blob/8346dfb697be43041a477886db26c508453fe5ce/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-ingress-and-retention-review/ingress/fixtures/correction-collision.json
[raw]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json
[prior]: ../2026-10-20-learning-evidence-dd150aa/neural/05-NEURAL-DEDUP-AND-COVERAGE.md
