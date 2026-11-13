---
title: "284. Дефект ортогональности, ошибка дефляции и недоказанное ускорение dwell-time"
source_pdf_pages: [688, 690]
book_pages: [652, 654]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 284. Дефект ортогональности, ошибка дефляции и недоказанное ускорение dwell-time

## Вопрос

Как ошибка базиса влияет на residual и обосновывают ли (17.9–17.11) ускорение в $10^{10}$ раз?

**Потеря ортогональности действительно нарушает формулу проектора, но авторская оценка выигранного времени не следует из данного анализа.** Для прикладного использования полезнее контролировать измеримый дефект $Q^\top Q-I$ и его вклад в residual, чем принимать объявленный множитель ускорения. [§17.2.4](../source/0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#section-17-2-4), [§17.2.5](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#section-17-2-5).

## Явная связь с ошибкой оператора

Пусть $Q$ имеет полный столбцовый rank и $\delta=\|Q^\top Q-I\|_2<1$. Обозначим через $U$ ортонормированный polar factor с тем же column space. Тогда

$$
\|QQ^\top-UU^\top\|_2=\delta.
$$

Для истинного проектора $P=I-UU^\top$ и используемой матрицы $\widetilde P=I-QQ^\top$ получаем

$$
\|\widetilde P A\widetilde P-PAP\|_2
\le(2\delta+\delta^2)\|A\|_2.
$$

Это следует из раскрытия $\widetilde P=P+D$, $\|D\|_2=\delta$, и $\|P\|_2\le1$. Отдельно

$$
\widetilde P^2-\widetilde P
=Q(Q^\top Q-I)Q^\top,\qquad
\|\widetilde P^2-\widetilde P\|_2\le(1+\delta)\delta.
$$

Таким образом, defect basis-to-residual имеет конкретную проверяемую цену. Сюда ещё надо добавить roundoff самих matrix products; даже точно ортонормированный вход не делает все последующие floating-point операции точными.

Источник пишет накопление порядка $K^2\varepsilon_{\mathrm{mach}}$ и после reorthogonalisation — $K\varepsilon_{\mathrm{mach}}$. Без модели присоединяемых векторов, их обусловленности, размерности и выбранного алгоритма это эвристические масштабы. Почти линейная зависимость кандидата от старого span важнее одного только числа $K$. Figure 17.3 — геометрическая схема drift, а не количественное доказательство универсальной оценки. [Figure 17.3 и формулировки drift](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#figure-17-3).

## Проверка множителя ускорения

С обозначением $L=\log(n/(\rho_{\max}^{2}L_{\mathrm{depth}}))$ источник сопоставляет нижние требования

$$
T_{\mathrm{exact}}\ge c\Delta^{-2}L,\quad
T_{\mathrm{INCRT}}\ge c\Delta^{-2}K^2\varepsilon_{\mathrm{mach}}^{-1/2}L,\quad
T_{\mathrm{unified}}\ge c\Delta^{-2}KL.
$$

Это (17.9–17.11); различение $L$-логарифма и depth здесь редакторское. При $K=100$, $\varepsilon_{\mathrm{mach}}=10^{-16}$ добавочный множитель второй формулы действительно равен $10^{12}$, а его отношение к $K$ равно $10^{10}$. Арифметика верна. [Формулы (17.9–17.10)](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#equation-17-9), [(17.11)](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#equation-17-11).

Но в тексте нет recurrence или perturbation-to-time theorem, выводящей фактор $\varepsilon_{\mathrm{mach}}^{-1/2}$ из ошибок предыдущих страниц. При фиксированной целевой точности он **растёт** при улучшении machine precision, хотя заявленная причина overhead — roundoff. Требовалось бы объяснить, меняется ли одновременно stopping tolerance или иной параметр задачи. Кроме того, сравнение двух нижних ограничений не даёт отношения фактических runtime и не гарантирует достаточность меньшего времени.

## Чего не устанавливает замена eigensolver

RQI использует Rayleigh quotient как shift, поэтому чувствительность этого shift не исчезает от замены градиентного шага на inverse solve. Reorthogonalisation уменьшает одну ошибку, но не устраняет target drift, sampling noise и стоимость внутренних solves. Даже доказанное сокращение числа gate-итераций надо переводить в число matvec, solve accuracy и время при одинаковом критерии остановки. Из рассматриваемых формул нельзя экспортировать обещание $10^{10}$ экономии обучения. Можно экспортировать необходимость стабильного basis maintenance и измерения $\delta$, residual error и solver cost.
