---
title: "240. Adam preconditioner: EMA, ожидание корня и зависимость от координат"
source_pdf_pages: [621, 623]
book_pages: [585, 587]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 240. Adam preconditioner: EMA, ожидание корня и зависимость от координат

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Исходное обновление и границы

В (16.40)–(16.42) записаны первые и вторые моменты Adam, bias correction и деление на $\sqrt{\widehat v_t}+\varepsilon_{\rm Adam}$. При нулевой инициализации моментов и $\|g_t\|_\infty\le G$ bias-corrected second moment каждой координаты лежит в $[0,G^2]$. Поэтому корректная верхняя граница диагонали $P_t$ равна **$G+\varepsilon_{\rm Adam}$**, а не $G$; грубая граница condition number — $(G+\varepsilon_{\rm Adam})/\varepsilon_{\rm Adam}$. [Определение preconditioner и свойства (a), (b)](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#section-16-8-1).

Положительная ограниченная диагональ задаёт диапазон шагов. Сама по себе она не доказывает сходимость сложной momentum-динамики: нужны условия на objective, шум, schedule и способ взаимодействия с gate. Ссылки `[citation]` в исходнике не раскрыты; здесь не подставляется внешняя теорема вместо недостающих условий.

## EMA не становится константой при стационарном шуме

Lemma 16.30 утверждает точные пределы $P_\infty^+=\Gamma_0/\sqrt d$ и $P_\infty^-=\delta_0/\sqrt d$. Её доказательство заменяет EMA случайных квадратов их стационарным средним. [Лемма и proof](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#lemma-16-30).

Для одной координаты положим $Y_t=g_t^2$, где $Y_t$ независимы и одинаково распределены, $\operatorname{Var}(Y_t)=s^2>0$, и $v_t=\beta v_{t-1}+(1-\beta)Y_t$. Тогда

$$
\mathbb E v_t\to\mathbb E Y_t,\qquad
\operatorname{Var}(v_t)\to\frac{1-\beta}{1+\beta}\,s^2>0.
$$

Bias correction стремится к единице и не удаляет эту остаточную дисперсию. Следовательно, даже в простом стационарном i.i.d. случае $v_t$ не сходится в среднем квадрате к постоянному второму моменту. Среднее приближается с множителем $\beta^t$; $1-\beta$ задаёт вес нового наблюдения, а не буквально ту же временную скорость.

Кроме того, $\mathbb E\sqrt{v_t}\ne\sqrt{\mathbb E v_t}$ в общем случае. Даже если second moments стали известны без ошибки, сохранённый в алгоритме $\varepsilon_{\rm Adam}>0$ меняет отношение масштабов на

$$
\frac{\Gamma_0/\sqrt d+\varepsilon_{\rm Adam}}
{\delta_0/\sqrt d+\varepsilon_{\rm Adam}},
$$

которое обычно не равно $\kappa_A=\Gamma_0/\delta_0$.

## Координатная диагональ не равна собственному базису

Adam использует квадраты **параметрических координат**. Условия на $\mathbb E\|g_t^+\|^2$ и $\mathbb E\|g_t^-\|^2$ не определяют все эти координаты и не доказывают, что $P_t$ диагонален в базисе $u^+,u^-$. [Формула 16.43](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#equation-16-43), [Описание двух gate flows](../source/0622-0624-ch16-adam-preconditioning-and-architecture-invariance.md#section-16-8-2).

Редакторская иллюстрация: повернём ковариацию $\operatorname{diag}(a^2,b^2)$ на $45^\circ$. В исходных координатах оба диагональных элемента полученной ковариации равны $(a^2+b^2)/2$, хотя собственные масштабы различаются. Идеализированный coordinate-wise Adam получает скалярный preconditioner и не устраняет этот спектральный перекос. Это не эксперимент с оптимизатором, а алгебраический пример недостаточности предпосылок.

Чтобы выводить gate-specific rate, нужны выбранный базис, распределение шума, контроль изменения eigenvectors, действие preconditioner на касательное пространство и оценка погрешности замены $P_t$ постоянным оператором. Средние нормы двух ветвей и stationary-input формулировка этого не заменяют.
