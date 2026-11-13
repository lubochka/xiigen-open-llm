# N33: градиенты Embedding/bias и первый supervised-шаг

16 октября 2026. `READ_ONLY_RESEARCH`. Primary C# прочитан на core `019cb5a43e2fb817d4353c4218f6d5a90342ea0c`. Разница этого SHA с предыдущим `32ac6e741c` затронула только исследовательские Markdown. Ни код, ни runtime не менялись этим исследованием; ни одного нового запуска не было.

## Что подтверждается и что необходимо сузить

**Embedding действительно копирует значения без пути обратного градиента к собственной таблице.** Аналогично работает разворачивание bias обычного Linear. Это реальные ограничения общего обучения этих параметров. Однако в RP01 база заморожена, а основные Linear используют `useBias:false`: эти два дефекта не объясняют автоматически неудачный q/v LoRA-опыт.

**Общий supervised API с положительным warmup не проходит первый шаг из-за нулевого LR. Но найденный production caller уже обходит это условие, задавая `WarmupSteps=0`.** Называть весь incremental SFT заблокированным было бы неверно. Один предложенный помощниками pretraining-пример дополнительно имеет недопустимую конфигурацию и не достигает проверяемого условия.

## 1. Где теряется производная

[Embedding.cs:39](C:/Projects/llm_mvp_core/00.TensorCore/Layers/Embedding.cs:39) копирует строки `Weight.Value.Data` в новый массив и возвращает обычный `Tensor`. [Tensor.cs:39](C:/Projects/llm_mvp_core/00.TensorCore/Core/Tensor.cs:39) по умолчанию создаёт его с `RequiresGrad=false`; нет `Children` и `BackwardFunc`, ведущих к Weight. Обход [Backward:64](C:/Projects/llm_mvp_core/00.TensorCore/Core/Tensor.cs:64) не может обнаружить отсутствующую связь.

Регистрация параметра в [Module.cs:18](C:/Projects/llm_mvp_core/00.TensorCore/Layers/Module.cs:18) только сохраняет его в словаре. Она не добавляет производную. Поэтому комментарий в [Linear.ExpandBias:84](C:/Projects/llm_mvp_core/00.TensorCore/Layers/Linear.cs:84), что bias-gradient якобы накапливается через `RegisterParameter`, не соответствует механизму регистрации. Копирование bias также возвращает несвязанный Tensor.

[AdamW.cs:79](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:79) пропускает параметры с `Grad == null`. Для свежего параметра, получающего сигнал только через эти Forward, путь к градиенту отсутствует. Если кто-то отдельно вручную записал Grad, поведение optimizer будет иным; общий вывод «параметр невозможно изменить вообще» здесь не делается.

Предсказание будущей проверки: сумма output для IDs `[3,3,1]` должна давать каждой координате строки 3 производную 2, строке 1 — 1. Для Linear с двумя нулевыми входными строками сумма output должна давать bias-производную 2. Это аналитические ожидания, не полученные здесь native результаты. Проверять только `output.Grad` недостаточно: нужен именно градиент исходного параметра.

## 2. Достижимость в текущей модели и истории

[LlmModel.Forward:225](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:225) реально вызывает этот Embedding. Для предполагаемого обучения всей базы с нуля его отключённая производная существенна: остальные матрицы могут обучаться, а таблица останется прежней. Это не утверждение, что обучение с фиксированными embeddings совсем невозможно.

Адресный поиск `new Linear(` в C# вне Tests нашёл восемь созданий в обычной Transformer-цепочке; все явно `useBias:false`. Здесь не найден live biased consumer, поэтому bias остаётся условным дефектом API. Поиск точных вызовов `TensorOps.Slice/Concat/Unsqueeze/Gather` вне Tests не дал совпадений; это узкая текстовая проверка, не доказательство отсутствия любых непрямых или внешних вызовов. Их generic backward нельзя объявлять причиной текущего RP01 без вызывающей цепочки. Дифференцируемые attention/DPO-операции рассмотрены отдельно в [41](41-current-dpo-gradient-chain-source-review.md) и [47](47-current-attention-backprop-and-position-contract.md).

Прочитанные версии Embedding и Linear из локальных refs `DesignToCode` и `language_translate` содержат те же копии без backward. В их Trainer нет `TrainSupervisedNextTokenStep`. Поэтому embedding/bias — унаследованные ограничения, а не доказанная новая регрессия при переносе в RP01. Это согласуется с тем, что старые успехи могли обучать другие матрицы, отдельную ProbeNeuralNetwork или дискретную память. Исторические успехи не отменяются.

## 3. Нулевой LR: класс и его production caller расходятся

[TrainingConfig.cs:17](C:/Projects/llm_mvp_core/04.TrainingLoop/TrainingConfig.cs:17) задаёт `WarmupSteps=100`. [CosineWithWarmup.GetLr:28](C:/Projects/llm_mvp_core/04.TrainingLoop/Schedulers/CosineWithWarmup.cs:28) на step 0 возвращает 0. [Trainer.TrainSupervisedNextTokenStep:125](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:125) сначала делает backward/clip, затем отвергает LR<=0, очищает градиенты и выбрасывает исключение. До optimizer и увеличения `_supervisedStep` управление не доходит. Это свойство прямого вызова с defaults и пригодными входами.

Но найденная цепочка [ProductionSupervisedTrainingStepExecutor:27](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs:27) получает Trainer из [IncrementalTrainingSession.Build:403](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainingSession.cs:403). Там явно написано:

`var perCallConfig = trainingConfig with { WarmupSteps = 0 };`

Далее создаётся Trainer и новый AdamW. [IncrementalTrainer:505](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:505) применяет LoRA, а [658](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:658) вызывает supervised executor на учебных примерах. Значит, прямой API-дефект не равен блокировке этого production-маршрута. Этот путь адаптерного обучения также нельзя без изменения контракта считать полным предварительным обучением базы.

У нового Trainer `_supervisedStep=0`, и публичного восстановления этого счётчика в прочитанном классе нет. Но Build одновременно создаёт **новый optimizer**, а не просто новый scheduler при сохранённых моментах. Поэтому предложенный helper-контраст «новый Trainer, прежний optimizer» проверяет изолированное свойство класса. Чтобы объяснить production continuation, нужно установить всю последовательность восстановления и намеренную границу сессий. Сейчас это условная гипотеза, не найденный инцидент сброса обучения.

## 4. Pretraining и неверная конфигурация будущего probe

[Trainer:238](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:238) в pretraining разрешает LR=0, вызывает optimizer, увеличивает CompletedSteps. При конечных значениях обновляются счётчик/moments, но множители LR дают нулевое перемещение весов. Это может быть обычной первой точкой warmup и не означает, что все последующие шаги бесполезны. Step-count и число ненулевых изменений весов — разные показатели.

В `33-MICRO-AUTOGRAD-WARMUP-AND-LEARNING-IMPACT-2026-10-16.md` предложено `TotalSteps=1, WarmupSteps=100`. [ValidatePretrainingConfiguration:302](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:302) отвергает WarmupSteps>TotalSteps. Такой запуск остановится до наблюдения нулевого optimizer-шага; его нельзя засчитать как подтверждение N33-D.

Для будущего минимального механического примера допустима согласованная пара `TotalSteps=1, WarmupSteps=1` с корректными batch/seq/context, либо наблюдение первого шага более длинной допустимой конфигурации. Это предложение по постановке; ничего не запускалось и настройки не менялись. Нулевой LR не гарантирует сохранность весов при NaN-градиенте: арифметика `0×NaN` отдельно требует finite-границы, что не измерялось этим чтением.

## Будущая проверка без смешения факторов

1. Отдельно проверить производную Embedding и bias по аналитическому ожиданию и finite difference. Не выдавать появление градиента за улучшение языка.
2. Отдельно сопоставить прямой SFT API с defaults и реально сконфигурированный incremental caller. Зафиксировать **эффективный** config, а не исходный объект до `with`.
3. Для эксперимента обучения базы заранее разрешить и перечислить её trainable-параметры, зафиксировать положительный LR в обоих плечах. Менять только связь Embedding; измерять train/heldout NLL, реальные свободные ответы, ошибки и полную стоимость.
4. Для restoration-гипотезы прежде найти actual session lifecycle и сохранённые model/optimizer/scheduler артефакты. Не создавать искусственный сброс ради доказательства, что он происходит у пользователя.

Три входа N33 рассмотрены: `33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS`, `33-NOVEL-AUTOGRAD-REACHABILITY-EMBEDDING-AND-BIAS`, `33-MICRO-AUTOGRAD-WARMUP-AND-LEARNING-IMPACT` от 2026-10-16. Ссылка второго файла на отдельный `REACHABILITY-AND-SFT-EXPERIMENT` появилась в следующем поступлении и требует собственного чтения. Старые инструкции helper о ролях и запрете записи core-отчётов не переопределяют нынешнее пользовательское исследовательское поручение.
