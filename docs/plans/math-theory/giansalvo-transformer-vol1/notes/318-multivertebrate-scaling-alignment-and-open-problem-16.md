---
title: "318. Multi-vertebrate scaling: matched MLM loss, confounders и Open Problem 16"
source_pdf_pages: [740, 741]
book_pages: [704, 705]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 318. Multi-vertebrate scaling: matched MLM loss, confounders и Open Problem 16

## Вопрос

Показывает ли падение V9′, что увеличение pretraining corpus причинно ухудшает downstream transfer?

**Показан отрицательный результат конкретного незавершённого запуска; причинное разложение не установлено.** Сам источник сообщает OOM/disconnect interruptions и прямо предупреждает о нескольких совместимых объяснениях. Эти оговорки должны сохраняться вместе с headline accuracy. [§18.2.4](../source/0739-0742-ch18-corpus-alignment-and-proteome-transfer-limits.md#section-18-2-4), [Figure 18.8](../source/0739-0742-ch18-corpus-alignment-and-proteome-transfer-limits.md#figure-18-8).

## Проверка опубликованных величин

Корпус из восьми видов содержит 283 891 последовательность. По сравнению с приблизительными 20 000 human-only это отношение 14.19455, то есть округлённые “fourteen times” допустимы. V9′ прошёл две эпохи, V9 — десять. Однако число sequence presentations при полном проходе составило бы около $2\cdot283891$ против $10\cdot20000$, то есть отношение **2.84**, а не $1/5$. Epoch count не является числом токенов, gradient steps или FLOPs; длины, batch size и truncation не дают восстановить эти бюджеты из одной таблицы.

Head counts V9′ суммируются в $241+172+117+103+69+50=752$, против 344 у V9; отношение примерно 2.186. Параметры растут $22.7\to49.4$ M, отношение 2.176. Следовательно, одновременно изменились corpus composition, завершённость обучения и сама architecture.

Падение probe $55.94-76.03=-20.09$ pp и fine-tune $63.58-70.78=-7.20$ pp вычислены верно. Внутренний fine-tune minus probe gap V9′ равен $+7.64$ pp. Эти три сравнения имеют разные denominators и вопросы; нельзя называть $7.64$ полной компенсацией $20.09$.

## Что контролирует совпадение MLM loss

$L_{\mathrm{MLM}}=2.67$ на многовидовом корпусе и $2.66$ на human-only — близкие scalar summaries, но ожидания взяты по **разным распределениям**. Их равенство не означает одинакового расстояния до optimum, одинаковой сложности masked tokens или одинаковой полезности features для human Pfam. Для утверждения о matching следует определить общую evaluation distribution и число prediction targets.

Объяснения через truncation, specialisation drift и probe mismatch правдоподобны как гипотезы; источник не разделяет их эффект. В частности, фраза, что capacity на non-human sequences “yields no return”, не является логическим следствием human-only evaluation: transferable features могут помогать человеческим белкам. Отсутствие наблюдаемого выигрыша не измеряет нулевой вклад каждой такой компоненты.

## Статус Open Problem 16

Автор предлагает multi-species probe и увеличенный context window, предполагая, что сужение gap при обоих вмешательствах подтвердит alignment conjecture. Это полезные направления проверки, но не достаточная идентификация единственного механизма. Новый probe меняет test distribution и потенциальную сложность, context window — input information и стоимость обучения. Улучшение совместимо и с другими причинами; отсутствие улучшения не логически вынуждает именно species curriculum или probe-aware growth. [Open Problem 16](../source/0739-0742-ch18-corpus-alignment-and-proteome-transfer-limits.md#open-problem-16).

Чтобы выделить **дополнительную чувствительность INCRT относительно fixed architecture**, нужен сам fixed-architecture comparator при сопоставимых данных, budget и evaluation. Такой comparator в описанном scaling run отсутствует. Для отделения причин также нужны результаты при общей длительности/сходимости и явно согласованном truncation protocol. Это требования к проверке гипотезы, а не предложение запустить новые модели в данном проходе.

Проектный вывод ограничен: architecture search, ориентированный на pretraining distribution, может распределять capacity иначе, чем требуется выбранному probe. Представленный опыт показывает необходимость учитывать alignment, но не устанавливает универсальный вред большего corpus и не доказывает, что dynamic architecture всегда более либо менее чувствительна к mismatch.
