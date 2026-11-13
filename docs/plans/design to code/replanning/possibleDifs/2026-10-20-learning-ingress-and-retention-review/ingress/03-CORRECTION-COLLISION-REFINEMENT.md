# CORRECTION-COLLISION-01 — исправленный feedback и повторное SFT на том же rejected source

**REFINEMENT / PREPARABLE / NOT_RUN.** Generic DPO→SFT — уже ND-DPO-SFT/P87. Здесь добавлен source-linked случай точного совпадения conditional target и заранее выбранный фактор для малого опыта. Полного native DPO admission host для этого случая в данной публикации нет; карточка не выдаётся READY. Первым выполняется [P0 реального SFT ingress](02-INGRESS-LENGTH-EXPERIMENT.md).

## Доказательство и границы

`FeedbackRouter.RouteEdited` сохраняет chosen=correction и rejected=modelResponse, а затем помещает correction в положительный cache. [FeedbackRouter,130–173][feedback] `ContinuousLearningPipeline` использует для SFT только `indexResult.Value.Where(domain)`; cache-положительный target в этот список не включён. [Pipeline,63–88,175–180][pipeline] `IncrementalTrainer` кодирует `Prompt+LF+Response`, сначала делает DPO и затем SFT на тех же adapters. [Trainer,402–418,581–672][incremental]

Если одновременно совпадают domain, prompt и rejected=response индексированного файла, SFT обучает именно тот conditional ответ, вероятность которого только что пытался снизить DPO. Следствие не универсально: это не утверждение, что любой source-SFT вреден, что DPO всегда побеждает SFT или что именно это случилось в сохранённом RP01 run. Частоту столкновений в реальном материале ещё надо зафиксировать.

Доказанный потребитель — combined incremental path. Это не direct RP01 simulation; не переносить вывод на EN→RU/SQL/Angular без собственного combined caller. Происхождение current material legacy и exact-v2 research нельзя смешивать: защищённое описание ND-DPO-SFT уже предупреждает об этом. [Protected N60 ORDER,175–185][order]

## Минимальный фиксированный A/B

Вход: [correction-collision.json](fixtures/correction-collision.json). Это **дизайн-фикстура**, не законно допущенная production DPO-пара. Для неё не выдуманы teacher passport, governance decision или transaction lease. Ручное написание синтетических материалов для эксперимента допустимо; существующую production проверку подлинности они не проходят автоматически.

| Часть | A | B |
|---|---|---|
| B0, frozen R0, tokenizer, LoRA, stage optimizers | Одинаковые сохранённые bytes и пустые моменты до начала | Те же bytes и состояния |
| DPO | Один и тот же prompt/chosen/rejected, один update | Идентичный update |
| SFT | Prompt=`// file: Abs.cs`; response точно равен rejected source | Тот же prompt; response точно равен исправленному chosen source |
| Порядок | DPO→SFT | DPO→SFT |
| Число шагов | Один DPO + один SFT | Один DPO + один SFT |
| Прочие факторы | Фиксированы; dropout0; profile/material version явны | Не менять |

Фикстура chosen/rejected имеет одинаковую ASCII длину, но равенство фактических token counts проверяется native tokenizer, а не выводится из длины строки. Для первого опыта выбрать одинаковый byte256 profile context128, который вмещает обе стороны полностью. Это **исследовательский профиль**, а не незаметная смена tokenizer рабочего checkpoint. Число target tokens в обоих SFT должно совпасть; otherwise карточка остаётся `FIXTURE_MISMATCH`, BPE/length effect в этот опыт не добавляется.

В обеих руках сохраняются S0, S_DPO, S_SFT. Измеряются обычным scorer на одинаковых prompt/response:

- chosen/rejected response logP **sum и mean**, разница chosen−rejected и число target tokens;
- full-sequence/response NLL; entropy только если scorer определяет одинаковый denominator;
- собственный response/teacher-forced token accuracy на TRAIN и HELDOUT отдельно; semantic validity коротких C# ответов — дополнительный показатель, не условие признания иных успехов;
- adapter/base parameter hashes, optimizer steps, update norm, достигнутые trainable targets;
- wall time DPO и SFT отдельно, изменения на несвязанном контрольном source; прежнее знание на warmed B0 при наличии такого B0, без выдумывания retention у random base;
- количество cached corrections, точных indexed collisions, active chosen/stale targets, rejected/ deferred rows. Это охват, не доказательство лучшего learning качества.

Первичный причинный контраст: `margin(S_SFT) − margin(S_DPO)` между A/B; дополнительный итог `margin(S_SFT) − margin(S0)`. Так отдельно видно, что получилось после DPO и что добавил SFT. Одного конечного score недостаточно. Увеличение chosen−rejected margin может сопровождаться ростом общей NLL или ухудшением другой конструкции; всё показывается рядом.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост accuracy не требуется.** Изменение только счетчика cache или успешная сериализация, без реального update и измерения его результата, не является этим экспериментом. Нельзя объявить успех одной метрики доказательством общего улучшения продукта.

## Исполнение: сначала закрыть конкретный недостающий dependency

В защищённой библиотеке есть ORDER, но нет готового portable public host для этого точного случая. Исторический `dpo-mask-live8-01/harness` использует другие material/profile и environment assumptions; его нельзя назвать готовым этой карточке. N61 reference card сама отмечает необходимость source/dependency/capability closure. [N61,66–69][n61]

Принятое расположение будущего host — `<core-parent>/xiigen-correction-collision-2026-10-20-attempt01/host/CorrectionCollisionProbe.csproj`; source — sibling `source` на `ce890a...`; inputs — `inputs/correction-collision.json`; outputs — `results/run01`. Core остаётся неизменным. Native stage executor должен быть подключён только после явного выбора research legal-authority entry без impersonation friend assembly и без выключения policy в original code. **Это точный незакрытый dependency карточки**, не предложение исполнителю наугад искать иной runner и не блокировка P0 public SFT опыта.

Перед сменой статуса на READY в новую addendum-карточку должны войти: полный host source, csproj, точный допущенный construction DPO request, команды build/run, identity frozen R0 до обеих стадий и факт компиляции. Пока этих файлов нет, команды запуска намеренно не выдуманы. Исполнитель завершает эту подготовку как отдельный source-only результат; токены на фиктивное «измерение» не тратятся. Запрещённую папку не редактировать. Старые эксперименты сохраняются.

## Возможное исправление оригинального пути — только после результатов

Место ремонта — выбор active supervised material перед admission/candidate binding в Pipeline, не формула DPO и не глобальная перестановка этапов. Для exact collision связать исправление с canonical source ID старой и новой версии, выбрать corrected target, сохранить stale version как superseded/deferred. Обновить source manifest и candidate content hash **до** mutation. FeedbackRouter создаёт cache-example без SourceId; простое чтение всего cache натолкнётся на provenance validation или создаст неверное происхождение. [Feedback,171–173][feedback]; [Source manifest,71–135][provenance]

Отрицательные контроли заранее заданы: индексированный исходник уже chosen; unrelated SFT response; prompt не совпадает. Их результаты не смешивать с точными collisions. Синтаксические правила C# нельзя объявлять общим механизмом всех доменов.

## Промпт исполнителю подготовки

> Подготовь только указанный isolated native host для CORRECTION-COLLISION-01, закрыв явно названный DPO authority/source dependency; до этого статус PREPARABLE. Используй fixture и два плеча с неизменным DPO→SFT порядком; synthetic design fixture не выдавай за admitted material. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост accuracy не требуется; все прочие изменения и ухудшения публикуются рядом. Измеряй S0, сразу после DPO и после SFT на реальном состоянии learner. Не подменяй update scorer-only опытом, не исправляй production и не запускай full suite. Сначала выполни уже полностью специфицированный public SFT unblocker; эту карточку не использовать для задержки доступного обучения. После результатов решение о полном внедрении принимает пользователь.

[feedback]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Feedback/FeedbackRouter.cs#L130-L173
[pipeline]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L63-L180
[incremental]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L402-L672
[order]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L175-L185
[n61]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L66-L69
[provenance]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Core/TrainingProvenance.cs#L71-L135
