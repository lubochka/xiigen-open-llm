---
title: "245. Random growth: expectation, high probability и различие способов дефляции"
source_pdf_pages: [629, 630]
book_pages: [593, 594]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 245. Random growth: expectation, high probability и различие способов дефляции

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Верное среднее и неверное направление вывода

Для независимого uniform unit vector $v\in S^{d-1}$ верно $\mathbb E[vv^\top]=I/d$, поэтому $\mathbb E[v^\top Av]=\operatorname{tr}(A)/d$. Для PSD $A$ и конкретного последовательного обновления $A'=P_vAP_v$, $P_v=I-vv^\top$, trace identity даёт условное **равенство** $\mathbb E[T'\mid A]=(1-1/d)T$. [Theorem 16.38 и proof sketch](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#theorem-16-38), [Напечатанное неравенство](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#display-0629-random-trace-recurrence).

В источнике из верхней оценки следующего expected residual выводится lower scaling числа событий. Такая верхняя оценка сама позволяет и гораздо более быстрое уменьшение: например, нулевой остаток уже после одного шага тоже удовлетворяет ей. Даже точное равенство для ожидания не даёт high-probability lower bound на stopping time без дополнительного контроля распределения.

Markov inequality для неотрицательного residual из **upper expectation** даёт оценку вероятности ещё не достигнуть порога, то есть достаточное время достижения с высокой вероятностью. Для противоположной границы нужен иной аргумент. В формулировке (16.59) присутствует confidence $1-\delta$, но в proof не выведено событие с такой вероятностью и не показана зависимость констант от $\delta$.

## Что именно считается добавлением направления

Если на каждом шаге остаток определяется как compression на дополнение к **полному накопленному span**, то $d$ независимых uniform directions почти наверное порождают всё $\mathbb R^d$. После $d$ шагов residual равен нулю при любом положительном пороге. Поэтому универсальная lower bound порядка $d\log(\Gamma_0/\theta_w)$ для сколь угодно малого $\theta_w$ не может относиться к этой процедуре без дополнительного ограничения режима параметров.

Если же применять свежий rank-one projector последовательно, не ортогонализуя все направления, результат иной: ранее удалённые направления могут снова участвовать в представлении остатка. Это уже не точная дефляция по накопленному span. Источник должен выбрать одну процедуру и связать количество её шагов с количеством реально новых heads. Упоминание Neumann inverse Gram в предыдущем разделе не решает эту неоднозначность. [244](244-nonorthogonal-deflation-rip-and-alignment.md).

## Trace и Frobenius norm

Для PSD residual ранга $r$:

$$
\|A\|_F\le\operatorname{tr}(A)\le\sqrt r\,\|A\|_F.
$$

Переход между двумя stopping criteria меняет пороги и константы; нужно контролировать effective rank вдоль процесса. Для signed operator trace может быть нулевым при ненулевой Frobenius norm, и этот перенос вообще не работает. Автор сам ограничивает proof sketch «trace-based obstruction» и mentions bounded-effective-rank assumptions; заголовок high-probability theorem шире фактически приведённого аргумента.

## Таблица трёх режимов и сравнение сложности

Из $\kappa_A\le d$ не следует $\kappa_A^2\le d$, нужное для заявления «random всегда хуже». Например, $\kappa_A=20,d=100$ удовлетворяют первому неравенству, но $d/\kappa_A^2=1/4$. Следует указать точное определение complexity index и отдельный диапазон, в котором сравнение верно. При напечатанных SST-2 значениях $d=768,\kappa_A\approx13$ отношение равно примерно 4.54; заявленные 4.4 могут отражать иное неокруглённое $\kappa_A$, поэтому это ограниченная точность калибровки, не самостоятельное опровержение. [Interpretation](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#pdf-0630), [Таблица режимов](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#table-16-6).

Adversarial пример $v_k\perp\operatorname{range}(\mathcal B)$ действительно не захватывает энергию, если update сохраняет этот смысл и ненулевое ортогональное дополнение существует. Для full-rank оператора ненулевых unit vectors в таком дополнении нет. Он демонстрирует необходимость условий alignment, но не доказывает предложенный phase transition или исчерпывающую классификацию всех growth algorithms. В этом пакете проверены формулы и логика bounds; random-growth experiment не запускался.
