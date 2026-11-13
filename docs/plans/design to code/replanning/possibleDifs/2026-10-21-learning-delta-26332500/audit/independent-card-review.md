# Независимая проверка ATTN-CAUSAL-SCORES-01

21 октября 2026. Проверяющий: отдельный агент `experiment_audit`, default модель сессии. Область: дизайн нового опыта, native source API, fixed learning budget, постановка before/after и управление исполнением. Host не создан, компиляция и обучение не выполнялись. Проверка не распространяется на все нейронные файлы или остальные новые карточки.

## Что проверено

- В current `CausalSelfAttention.cs:263–291` ordinary QK действительно считает полный square result, затем scale, causal mask `NegativeInfinity`, softmax и `weights @ V`. Cached path отдельный. Следовательно, factor доступен на пути настоящего SFT, а не только внешнего benchmark.
- В `TensorOps.cs:593–704` каждый batch делает transpose B, native SIMD forward, затем тот же полный backward. `ApplyCausalMaskWithPast:1434–1448` пропускает gradient только в разрешённый треугольник. Сохранение прежнего backward и порядка dot accumulation обосновывает ожидаемую эквивалентность на конечных данных; фактическая bitwise parity пока не измерена.
- Typed overload `ValidateFullCheckpointArtifact(Stream,ModelConfig,FullCheckpointLoraTopology,...)` существует и возвращает `OperationResult<bool>`. `GetCheckpointParameterEntries`, `CaptureRandomState`, `RestoreRandomState`, portable AdamW export/import присутствуют в current source; нужные типы входят в назначенные 36 файлов.
- Common 400 SFT — конечная приобретательная стадия. Требуется named TRAIN false→true по полному однотокенному ответу; 0→0 не допускает A/B. 1/2 явно сравнивается с constant-label 1/2 и допускает лишь узкий вывод о данной приобретённой строке и цене обучения; общего закона карточка не обещает.
- TRAIN/EXAM имеют по две метки одного предмета, без EXAM updates и выбора checkpoint по EXAM. Это близкие шаблоны, не доказательство широкого обобщения; ограничение указано.
- 400 common + 4A + 4B = **408 optimizer updates / 408 training Forward / 408 Backward**. Диагностика common4 + arms16 =20, итого **428 Forward**. Каждый shifted TRAIN имеет 21 target, всего **8568 target presentations**. Q имеет `[1,4,21,16]`; для четырёх arm updates dense QK dots7056, triangular3696. Формулы не выданы за измеренный speedup.
- Стоимость четырёх native updates отделена от common/restore/readout/serialization. Все метрики и регрессии сохраняются, mean causal speedup из одного A→B не утверждается. Expected parity охватывает states, logits, RNG и optimizer; новые tolerance и reruns не допускаются.
- Paths, source pin, sole attempt a001, entry command, default model, skills и русский report заданы. CLI честно названа будущей, а не уже рабочей. Same-handle waits и немедленный STOP/NEXT предусмотрены.

## Два замечания, требующие точечного исправления автором

1. **Access modifier нового SIMD helper.** Исходный `SimdOps.MatVecRowUnpublished` — internal (`SimdOps.cs:73`), private — его core (`:99`). Рецепт называл новый `MatVecRowCausalUnpublished` private, хотя он должен вызываться из `TensorOps`. Требуется точная схема `internal static SimdOps.MatVecRowCausalUnpublished(...)` и private core с исходной сигнатурой/порядком вычисления. Буквальный private метод в SimdOps недоступен вызывающему типу.
2. **Полный csproj контракт.** Назначенные native sources используют bare `List`, `CancellationToken`, `Math` и другие framework names. При отключённых родительских props требуется явно включить `ImplicitUsings=enable`; также зафиксировать net9.0, Nullable и порядок SDK imports/Compile 36+Program. Автор не должен находить эти настройки пробами после создания проекта.

Дополнительная ясность управления: после STOP прекратить старую работу; если собственный процесс ещё активен, допускается только необходимое прекращение через уже известный handle/PID, затем короткий фактический статус. Это не разрешение продолжать отчёт, review, hashes или обучение. Иначе буквальное «никаких новых действий» может оставить фоновые updates работать после STOP.

Замечания переданы автору и главному агенту; чужие карточки проверяющий не редактировал. Финальный readback исправлений и окончательный статус добавляются ниже без запуска модели.

## Итоговый readback и статус

`ACCEPTED_WITH_LIMITS_AS_DOCUMENTED_DESIGN`. Автор внёс оба исправления и согласовал карточку с executor prompt и source audit. Повторно прочитаны только затронутые разделы:

- XML в разделе 2 теперь задаёт явные SDK imports, net9.0, ImplicitUsings=enable, Nullable=enable, C#13, пути и Compile36+Program. `ArtifactRoot` относительно назначенного J выходит в правильный корень `experiments-v2/RP01-MICRO/CS01`.
- Раздел 3 и prompt явно задают public TensorOps entry → internal SimdOps unpublished wrapper → private core; прежнее противоречие доступа устранено.
- STOP/NEXT разрешает только необходимое прекращение собственного активного процесса по известному handle/PID и подтверждение, без оформления старой задачи, relaunch или воздействия на чужие процессы.

Это принятие **документированного дизайна** с ограничениями: source-copy host и CLI пока не существуют как проверенная сборка; фактическое приобретение при 400 updates, runtime parity и полезная ресурсная дельта не измерены. При 1/2 возможен лишь узкий вывод о приобретённой строке/cost, а не общий закон знака. Блокирующих противоречий в проверенной области после точечных исправлений не осталось. Новые проверки и архивные прогоны не добавлялись.

## Первичные источники для review

- [CausalSelfAttention ordinary и cached paths](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/Layers/CausalSelfAttention.cs#L263-L318).
- [MatMul forward/backward](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/00.TensorCore/Core/TensorOps.cs#L593-L704), [causal mask backward](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/00.TensorCore/Core/TensorOps.cs#L1434-L1448).
- [Native internal wrapper и private core](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/00.TensorCore/Core/SimdOps.cs#L71-L146).
- [Typed full checkpoint validation](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/LlmModel.cs#L911-L938), [parameter/RNG API](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/LlmModel.cs#L267-L327).
- [Portable AdamW export/import](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/04.TrainingLoop/Optimizers/AdamW.cs#L188-L290), [constant-LR scheduler](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/04.TrainingLoop/Schedulers/CosineWithWarmup.cs#L27-L35).
