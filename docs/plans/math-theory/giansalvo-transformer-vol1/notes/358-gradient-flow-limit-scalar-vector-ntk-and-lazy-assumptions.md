---
title: "358. Gradient flow и NTK: time scaling, размер kernel и условия замороженного ядра"
source_pdf_pages: [794, 796]
book_pages: [758, 760]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 358. Gradient flow и NTK: time scaling, размер kernel и условия замороженного ядра

## Вопрос

Достаточно ли уменьшить learning rate или увеличить width, чтобы получить описанную в Appendix A линейную динамику?

**В обоих переходах нужны дополнительные условия.** Appendix A даёт краткую памятку; в source сохранён её текст, а здесь указано, как читать его без чрезмерного обобщения. [Gradient flow](../source/0792-0796-appendix-a-mathematical-background.md#equation-a-10), [NTK](../source/0792-0796-appendix-a-mathematical-background.md#equation-a-11).

## Как дискретный шаг приближается к потоку

Gradient descent

$$
\theta_{k+1}=\theta_k-\eta\nabla\mathcal L(\theta_k)
$$

является явным Euler step. Сравнивать надо непрерывное время $t=k\eta$ и интерполяцию траектории при $\eta\to0$, на фиксированном временном интервале. При фиксированном числе шагов $k$ уменьшение $\eta$ обычно просто уменьшает суммарное движение.

Стандартное достаточное условие для контролируемого приближения — local Lipschitz gradient в области, где остаётся траектория, существование решения и ограниченность необходимых величин. Одна дифференцируемость $\mathcal L$, названная в кратком определении, не обеспечивает эти свойства автоматически. ODE limit также не даёт гарантию качества generalisation.

## Scalar NTK и vector output

Формула (A.11) непосредственно относится к scalar $f$. Для vector output $f:\mathcal X\to\mathbb R^q$ Jacobian имеет размер $q\times P$, и полное ядро

$$
\Theta(x,x')=J_\theta f(x)\,J_\theta f(x')^\top
$$

является $q\times q$ matrix-valued object. Frobenius inner product двух Jacobians даёт scalar trace contraction, который вообще не заменяет все cross-output interactions.

Положительная полуопределённость scalar kernel следует из
$\sum_{i,j}c_ic_j\Theta(x_i,x_j)=\|\sum_ic_i\nabla_\theta f(x_i)\|^2\ge0$; это точный вывод из определения, не empirical claim.

## Когда kernel остаётся постоянным

Infinite width, deterministic initial kernel и stationarity вдоль training — разные утверждения. Их совместное выполнение зависит от parameterisation, initialisation, learning-rate scaling, архитектуры и времени наблюдения. Фраза “linear regression in feature space” дополнительно требует соответствующего loss, например square loss; фиксированные features с cross-entropy задают другую оптимизационную задачу.

И finite width не заставляет NTK меняться: для $f(x;\theta)=\theta^\top x$ при конечном числе параметров $\Theta(x,x')=x^\top x'$ точно постоянно. Поэтому source sentence о меняющемся finite-width NTK следует читать как типичное поведение обучаемых nonlinear features.

Библиографические ссылки Appendix A сохранены полностью; указанные работы не выдаются за заново прочитанные или воспроизведённые в этом проходе.

