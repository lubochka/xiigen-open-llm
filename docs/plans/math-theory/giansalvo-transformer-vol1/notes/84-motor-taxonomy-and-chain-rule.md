---
title: "84. Motor formalism: сигнал, оператор и правило цепочки"
source_pdf_pages: [312, 314]
book_pages: [276, 278]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 84. Motor formalism: сигнал, оператор и правило цепочки

[Индекс библиотеки](../README.md) · [Глава 9](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#chapter-9) · [Предыдущая карточка](83-signature-crowding-and-chapter-eight-boundaries.md) · [Следующая карточка](85-motor-literature-map-and-mlm-entry.md)

## Зачем автор вводит термин motor

Начало главы 9 переводит внимание с распределения сигнатур на локальные объекты computational graph. Автор рассматривает, как task signal проходит через MLP, нормировку, attention, Q/K/V routes и residual paths. Motor определяется широко: сигнал или оператор, управляющий вариацией выбранной переменной. Это новая организация стандартных differential formulas, а не отдельно доказанный оптимизатор. [PDF 312–313 и Definition 9.1](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#definition-9-1).

Definition 9.2 разделяет три роли:

| Класс | Типичный объект | Вопрос, на который отвечает |
|---|---|---|
| Signal motor | Incoming gradient или forward perturbation | Какая вариация поступает на локальный участок графа? |
| Structural motor | Weight, Jacobian, gate, mask, projection | Как участок преобразует эту вариацию? |
| Effective motor | Композиция сигнала и соответствующих операторов | Какая вариация или pullback фактически получается на выбранной переменной? |

[Definition 9.2 и Figure 9.1](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#definition-9-2).

Классификация зависит от выбранной границы блока: effective motor одного участка может стать входящим signal motor следующего. Широкое определение само не задаёт уникального разложения, отдельной единицы измерения или canonical motor norm. Для воспроизводимости нужно назвать конкретную переменную, loss/perturbation и набор путей графа. Это редакторское следствие включённости столь разных объектов в одно определение.

## Пример gated MLP

Автор пишет

$$
\frac{\partial L}{\partial y_m}=g_mW_2^\top D_mW_1^\top.
$$

Здесь $g_m$ — signal, а три следующих фактора — structural motors. Их полный product является effective motor. [Формула на PDF 313](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#display-9-gated-motor-example).

Редакторская проверка размеров для обычного двухслойного MLP: если $W_1\in\mathbb R^{d\times f}$, $W_2\in\mathbb R^{f\times d}$, $D_m\in\mathbb R^{f\times f}$ и $g_m\in\mathbb R^{1\times d}$, то

$$
(1\times d)(d\times f)(f\times f)(f\times d)=1\times d.
$$

Для ReLU $D_m$ обычно является локальной diagonal derivative mask. Для другой activation нужна её фактическая производная; нельзя автоматически переносить binary ReLU mask на гладкую функцию. В точках смены ReLU режима классическая производная требует оговорки. Эта строка описывает путь через MLP, а не автоматически весь residual block: отдельные пути должны быть добавлены согласно графу.

## Композиция — обычный pullback

Proposition 9.4 рассматривает $x\mapsto h(x)\mapsto y(h)$ и даёт

$$
M_x=M_yJ_{y\leftarrow h}J_{h\leftarrow x}.
$$

Это корректная chain rule для incoming row covector при convention $J_{y\leftarrow h}\in\mathbb R^{d_y\times d_h}$ и $J_{h\leftarrow x}\in\mathbb R^{d_h\times d_x}$. [Proposition 9.4 и proof](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#proposition-9-4).

Важно различать backward covector и forward tangent. Если forward perturbations хранятся строками, то редакторская запись при той же convention имеет вид

$$
dy=dx\,J_{h\leftarrow x}^\top J_{y\leftarrow h}^\top.
$$

Таким образом, одна широкая taxonomy допускает оба вида анализа, но порядок и transpose зависят от направления передачи. Нельзя подставить произвольный forward signal в backward formula без проверки типов.

Для графа с ветвлениями chain rule также суммирует вклад всех путей. Например, для $y=x+F(x)$:

$$
M_x=M_y+M_yJ_F=M_y(I+J_F).
$$

Это редакторский пример, поясняющий фразу источника о распространении Proposition 9.4 на finite computational graph. Наличие identity term не гарантирует lower bound нормы результата: другой term может его частично или полностью сократить.

## Что это добавляет к geography

Локальный differential object объясняет чувствительность выбранного участка при фиксированной точке вычисления. Он не реконструирует все глобальные сигнатуры и не доказывает causal importance при большом удалении головы или блока. Источник сам ограничивает начало главы одним блоком, masked position или слоем и считает глобальную geography внешним контекстом. [PDF 313](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#pdf-0313).

При переносе в проект полезно хранить по пути signal shape, structural factors, effective expression и точку вычисления. Это проверяемая форма документации backpropagation; эффективность обучения Xiigen из одного переименования chain rule не следует.
