---
title: "192. Переход от static rationalisation к INCRT: что задано и что остаётся гипотезой"
source_pdf_pages: [543, 548]
book_pages: [507, 512]
batch: "011"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 192. Переход от static rationalisation к INCRT: что задано и что остаётся гипотезой

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Архитектурное различие

В §14.15 static rationalised block имеет фиксированные head slots, channel allocation, dictionary resolution и FFN capacity. INCRT добавляет изменение этих объектов через **growth, tracking и termination**. «Static» здесь не означает отсутствие обучения весов: различие касается изменения архитектурной ёмкости. [Bridge](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#section-14-15), [Table 14.11](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#table-14-11).

Равенство INCRT = Rationalised Transformer + growth + tracking + termination автор прямо называет концептуальным, не алгебраическим. Оно организует описание, но не является формулой сложности, алгоритмом оптимизации или доказательством сходимости.

## Четыре подготовленных предположения

§14.15.4 явно оставляет недоказанными: устойчивость диагностик на growth window; уменьшение coverage residual при добавлении головы в непокрытую область; сохранение directional tails; прекращение роста после уменьшения residual improvements ниже tolerances. Эти утверждения — предпосылки будущего динамического анализа. [Начало списка](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#section-14-15-4), [продолжение](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#pdf-0545).

Для **фиксированной** меры $\mu$ простая геометрическая часть проверяется: добавление атома даёт

$$
\int d(\sigma,D\cup\{a\})^2\,d\mu
\le\int d(\sigma,D)^2\,d\mu.
$$

Строгое уменьшение требует положительной меры области, где новый атом ближе всех прежних. Однако добавление trainable head не равно простому добавлению атома при неизменных остальных объектах: могут меняться outputs, loss, распределение сигнатур и сами параметры словаря. Геометрическое неравенство не гарантирует улучшения task loss или конечного числа событий. В частности, движущаяся цель может создавать новый residual после уже успешного шага.

## Граница эмпирических свидетельств

§14.15.5 прямо оставляет преимущество при равных параметрах и compute для companion experimental work. Утверждение о task-uniform bound тоже ограничено выбранным классом задач; переход к произвольным классам не объявлен доказанным. Дополнительная проверка самой Proposition 14.17 находится в [181](181-task-class-quantisation-and-covering-number-limits.md).

В описании протокола есть несогласованность. §14.12.5 предлагает небольшую MLM или sequence-classification задачу и сравнение двух блоков. На PDF 545 тот же раздел описан как BERT-base protocol с четырьмя архитектурами и четырьмя geometric KPIs. Первое описание не содержит всех этих конкретизаций. Их следует хранить как разные авторские формулировки планируемой проверки, а не как уже выполненный согласованный эксперимент. [Ранний протокол](../source/0535-0540-ch14-illustrations-implementations-and-boundaries.md#section-14-12-5), [позднее описание](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#section-14-15-5).

## Что начинается в главе 15

Глава 14 заканчивается финальным Takeaway на **PDF 546 / книге 510**. Глава 15 и §15.1 начинаются на **PDF 547 / книге 511**. Введение сохраняет рабочий цикл: train → measure residual demand → expand if needed → stabilise → stop or prune. Автор намерен различать under-coverage, crowding, полезный residual и residual без marginal value. [Начало главы](../source/0547-0550-ch15-dynamic-variables-and-controller-introduction.md#chapter-15), [операционный принцип](../source/0547-0550-ch15-dynamic-variables-and-controller-introduction.md#section-15-2).

Введение утверждает, что последующий controller объяснит termination, а более широкие варианты будут рассмотрены в главе 16. До чтения конкретных условий и доказательств это указание на программу дальнейшего текста, а не уже проверенная гарантия. Для проекта этот участок задаёт интерфейс наблюдений и действий; достаточные условия безопасного изменения архитектуры нужно брать из соответствующих результатов после их обработки.
