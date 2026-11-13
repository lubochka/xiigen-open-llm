---
title: "Appendix A: Mathematical Background"
source_pdf_pages: [792, 796]
book_pages: [756, 760]
batch: "016"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: false
---

# Appendix A: Mathematical Background

[Library index](../README.md) · [Source policy](../SOURCE.md)

> Editorial note: Full transcription of PDF 792–796, including equations (A.1)–(A.14) and bibliographic notes. The original shorthand statements about skew generators, softmax monotonicity, gradient-flow limits and infinite-width NTK are retained. Their missing qualifications are discussed separately in the reading notes. The original “Chapter ??” is not replaced by a guessed reference.

<a id="pdf-0792"></a>
## PDF 792 · Book page 756

<a id="appendix-a"></a>
## Appendix A — Mathematical Background

This appendix collects, in compact form, the mathematical prerequisites used throughout the volume. Readers comfortable with graduate-level linear algebra and the rudiments of differential geometry may skim or skip it. For those who occasionally need a refresher, each section is self-contained and references the chapters where the corresponding object is used.

The aim is not to substitute for a textbook treatment. The references at the end of the chapter point to the standard sources for each topic.

<a id="section-a-1"></a>
### A.1 Linear algebra

<a id="section-a-1-1"></a>
#### A.1.1 Matrices, transpose, and rank

A matrix $M\in\mathbb R^{m\times n}$ is a rectangular array of real numbers, regarded both as a linear map $\mathbb R^n\to\mathbb R^m$ and as a bilinear form on $\mathbb R^n\times\mathbb R^m$. Its transpose $M^\top\in\mathbb R^{n\times m}$ is defined by $(M^\top)_{ij}=M_{ji}$.

The **rank** of $M$, written $\operatorname{rank}(M)$, is the dimension of its column space (equivalently, of its row space). It coincides with the number of non-zero singular values (Section A.1.4) and satisfies $\operatorname{rank}(M)\le\min(m,n)$.

<a id="section-a-1-2"></a>
#### A.1.2 Trace and Frobenius norm

For a square matrix $M\in\mathbb R^{d\times d}$, the **trace** is $\operatorname{tr}(M)=\sum_iM_{ii}$. The trace is linear, cyclic ($\operatorname{tr}(AB)=\operatorname{tr}(BA)$ whenever both products are defined), and invariant under similarity transforms.

The **Frobenius** (or **Hilbert–Schmidt**) norm on $\mathbb R^{m\times n}$ is

<a id="equation-a-1"></a>
$$
\|M\|_F=\sqrt{\operatorname{tr}(M^\top M)}=\sqrt{\sum_{i,j}M_{ij}^2}.
\tag{A.1}
$$

It is the Euclidean norm associated with the Hilbert–Schmidt inner product $\langle A,B\rangle_F=\operatorname{tr}(A^\top B)$. The Frobenius norm recurs throughout the volume as the canonical way to measure the magnitude of a matrix object, in particular the residual operators $A_{\mathrm{res}}$ and the directional energies $\Gamma$ of Chapter 11.

<a id="pdf-0793"></a>
## PDF 793 · Book page 757

<a id="section-a-1-3"></a>
#### A.1.3 Eigenvalues, eigenvectors, and the spectral theorem

Let $M\in\mathbb R^{d\times d}$. A non-zero vector $v\in\mathbb R^d$ is an **eigenvector** of $M$ with **eigenvalue** $\lambda\in\mathbb C$ if $Mv=\lambda v$. The set of eigenvalues is the **spectrum** $\operatorname{spec}(M)$.

A real symmetric matrix has real eigenvalues and admits an orthonormal basis of eigenvectors: if $M=M^\top$, there exist $\lambda_1\ge\lambda_2\ge\cdots\ge\lambda_d$ and an orthogonal matrix $U$ with

<a id="equation-a-2"></a>
$$
M=U\operatorname{diag}(\lambda_1,\ldots,\lambda_d)U^\top.
\tag{A.2}
$$

This is the spectral theorem. It underlies the analysis of the symmetric sector $M_s$ of the score operator in Chapter 4.

<a id="section-a-1-4"></a>
#### A.1.4 Singular value decomposition

Every matrix $M\in\mathbb R^{m\times n}$ admits a **singular value decomposition** (SVD)

<a id="equation-a-3"></a>
$$
M=U\Sigma V^\top,
\tag{A.3}
$$

where $U\in\mathbb R^{m\times m}$ and $V\in\mathbb R^{n\times n}$ are orthogonal, and $\Sigma\in\mathbb R^{m\times n}$ is diagonal with non-negative entries $\sigma_1\ge\sigma_2\ge\cdots\ge0$, the singular values. The columns of $U$ and $V$ are the left and right singular vectors. The rank of $M$ is the number of non-zero singular values.

The SVD is the workhorse decomposition in deep learning theory and appears in every chapter of Part II.

<a id="section-a-1-5"></a>
#### A.1.5 Schatten $p$-norms

The **Schatten $p$-norm** of $M\in\mathbb R^{m\times n}$ is

<a id="equation-a-4"></a>
$$
\|M\|_{S_p}=\left(\sum_i\sigma_i(M)^p\right)^{1/p},
\qquad1\le p<\infty,
\tag{A.4}
$$

and $\|M\|_{S_\infty}=\sigma_1(M)$ (the **operator** or **spectral norm**). The special cases:

- $p=1$: nuclear (trace) norm, $\|M\|_{S_1}=\sum_i\sigma_i$.

- $p=2$: Frobenius norm, $\|M\|_{S_2}=\|M\|_F$.

- $p=\infty$: spectral norm.

Appendix D develops the operator-theoretic machinery of greedy Schatten deflation that underlies the residual energy analysis of the main text.

<a id="section-a-1-6"></a>
#### A.1.6 Pseudoinverse and projection operators

The **Moore–Penrose pseudoinverse** $M^\dagger\in\mathbb R^{n\times m}$ of $M$ is the unique matrix satisfying the four Penrose identities; it generalises the inverse to non-square or rank-deficient matrices. $MM^\dagger$ is the orthogonal projector onto the column space of $M$, and $M^\dagger M$ is the projector onto the row space.

Orthogonal projectors $\Pi$ in $\mathbb R^d$ satisfy $\Pi^2=\Pi$ and $\Pi=\Pi^\top$. Their trace equals the dimension of the subspace onto which they project.

<a id="pdf-0794"></a>
## PDF 794 · Book page 758

<a id="section-a-2"></a>
### A.2 The Lie algebra $\mathfrak{so}(d)$

The space

<a id="equation-a-5"></a>
$$
\mathfrak{so}(d)=\{A\in\mathbb R^{d\times d}:A^\top=-A\}
\tag{A.5}
$$

of $d\times d$ skew-symmetric matrices is the **Lie algebra** of the special orthogonal group $\mathrm{SO}(d)$ of rotations of $\mathbb R^d$. It is a real vector space of dimension $d(d-1)/2$, equipped with the **Lie bracket** (commutator)

<a id="equation-a-6"></a>
$$
[A,B]=AB-BA,
\tag{A.6}
$$

which is bilinear, antisymmetric ($[A,B]=-[B,A]$), and satisfies the **Jacobi identity**

<a id="equation-a-7"></a>
$$
[A,[B,C]]+[B,[C,A]]+[C,[A,B]]=0.
\tag{A.7}
$$

For $A\in\mathfrak{so}(d)$, the matrix exponential $\exp(A)=\sum_{k=0}^{\infty}A^k/k!$ lies in $\mathrm{SO}(d)$; the exponential map $\exp:\mathfrak{so}(d)\to\mathrm{SO}(d)$ is surjective for $d\ge2$ but not in general injective.

The eigenvalues of $A\in\mathfrak{so}(d)$ are purely imaginary and occur in conjugate pairs $\pm i\theta_k$, $k=1,\ldots,\lfloor d/2\rfloor$ (plus a zero eigenvalue if $d$ is odd). The associated invariant 2-planes are the **eigenplanes**, on which $A$ acts as a 2D rotation by $\theta_k$. The eigenplane structure underlies the antisymmetric motor analysis of Chapter **??**.

<a id="section-a-3"></a>
### A.3 The softmax operator

The **softmax** maps a vector $z\in\mathbb R^n$ to the open probability simplex $\Delta^{n-1}=\{p\in\mathbb R^n:p_i>0,\sum_ip_i=1\}$:

<a id="equation-a-8"></a>
$$
\operatorname{softmax}(z)_i=\frac{e^{z_i}}{\sum_je^{z_j}},
\qquad i=1,\ldots,n.
\tag{A.8}
$$

It is smooth, invariant under shifts $z\mapsto z+c\mathbf1$, and strictly increasing in each $z_i$. Its Jacobian at $z$ is

<a id="equation-a-9"></a>
$$
\partial_j\operatorname{softmax}(z)_i=p_i\delta_{ij}-p_ip_j,
\qquad p=\operatorname{softmax}(z),
\tag{A.9}
$$

that is, $J(z)=\operatorname{diag}(p)-pp^\top$, a symmetric positive-semidefinite matrix of rank $n-1$ whose null space is $\operatorname{span}(\mathbf1)$.

When applied row-wise to a score matrix $S\in\mathbb R^{n\times n}$, the softmax produces a row-stochastic attention matrix $A\in[0,1]^{n\times n}$ with $A\mathbf1=\mathbf1$. The row-wise nature of the softmax is the source of every asymmetry property of attention discussed in Part II.

<a id="section-a-4"></a>
### A.4 Gradient flow and the NTK

<a id="section-a-4-1"></a>
#### A.4.1 Gradient flow

Given a differentiable loss $\mathcal L:\mathbb R^P\to\mathbb R$ over parameters $\theta\in\mathbb R^P$, the **gradient flow** is the continuous-time dynamical system

<a id="equation-a-10"></a>
$$
\dot\theta(t)=-\nabla_\theta\mathcal L\bigl(\theta(t)\bigr).
\tag{A.10}
$$

Gradient descent with learning rate $\eta$ is the explicit Euler discretisation of this ODE: $\theta_{t+1}=\theta_t-\eta\nabla_\theta\mathcal L(\theta_t)$. In the limit $\eta\to0$, gradient descent converges to gradient flow.

<a id="pdf-0795"></a>
## PDF 795 · Book page 759

<a id="section-a-4-2"></a>
#### A.4.2 Neural Tangent Kernel

The **Neural Tangent Kernel** (NTK) of a parameterised function $f(x;\theta)$ is the symmetric positive-semidefinite kernel

<a id="equation-a-11"></a>
$$
\Theta(x,x')=\langle\nabla_\theta f(x;\theta),\nabla_\theta f(x';\theta)\rangle.
\tag{A.11}
$$

At initialisation and in the infinite-width limit, $\Theta$ becomes deterministic and stationary along the gradient flow, reducing the training dynamics to a linear regression in feature space. At finite width, the NTK evolves in time; its time derivative is the differential NTK (dNTK), which features in Chapter 11.

<a id="section-a-5"></a>
### A.5 Some convex geometry

A subset $C\subseteq\mathbb R^n$ is **convex** if for every $x,y\in C$ and $\lambda\in[0,1]$ the point $\lambda x+(1-\lambda)y$ also lies in $C$. The **convex hull** $\operatorname{conv}(S)$ of a set $S$ is the smallest convex set containing $S$.

The **relative interior** $\operatorname{relint}(C)$ of a convex set $C$ is the interior of $C$ within its affine hull. For a non-empty convex set, the relative interior is non-empty.

The standard $(n-1)$-simplex is

<a id="equation-a-12"></a>
$$
\Delta^{n-1}=\left\{p\in\mathbb R_{\ge0}^n:\sum_ip_i=1\right\},
\tag{A.12}
$$

the codomain of the softmax (its relative interior is the image of softmax). The attention matrices of Chapter 6 live on the row-wise product of simplices $(\Delta^{n-1})^n\subset\mathbb R^{n\times n}$.

<a id="section-a-6"></a>
### A.6 Spectral norms, perturbation, and Weyl

**Weyl's inequality** states that for symmetric $A,B\in\mathbb R^{d\times d}$ with eigenvalues $\lambda_1(\cdot)\ge\cdots\ge\lambda_d(\cdot)$,

<a id="equation-a-13"></a>
$$
\bigl|\lambda_k(A+B)-\lambda_k(A)\bigr|\le\|B\|_2
\qquad\forall k=1,\ldots,d.
\tag{A.13}
$$

**Wedin's theorem** provides the analogous perturbation bound for singular value decompositions and for the angle between invariant subspaces. These bounds underlie the robustness analysis of the spectral gate in Chapter 17.

<a id="section-a-7"></a>
### A.7 Compressed sensing in one paragraph

Compressed sensing concerns the recovery of an $s$-sparse signal $x_0\in\mathbb R^n$ from $m\ll n$ linear measurements $y=\Phi x_0\in\mathbb R^m$. When the sensing matrix $\Phi$ satisfies the **restricted isometry property** (RIP) of order $2s$ with constant $\delta_{2s}<\sqrt2-1$, the convex program

<a id="equation-a-14"></a>
$$
\min_x\|x\|_1\quad\text{subject to }\Phi x=y
\tag{A.14}
$$

recovers $x_0$ exactly. The Compressed-Sensing Theorem for INCRT (Chapter 15) lifts this paradigm to the architectural setting: the converged head count $K^\star$ plays the role of sparsity, and the inputs play the role of measurements.

<a id="pdf-0796"></a>
## PDF 796 · Book page 760

<a id="section-a-8"></a>
### A.8 Bibliographic notes

The linear-algebra material is standard and can be found in any graduate text; Horn and Johnson (2013) is the canonical reference for matrix analysis. The Lie-algebraic material follows Hall (2015) for the elementary theory and Warner (1983) for the differential-geometric viewpoint. The Schatten-norm machinery is covered in Simon (2005).

The Neural Tangent Kernel was introduced by Jacot et al. (2018) and extended to finite-width corrections by Roberts et al. (2022), whose Chapter 11 of *The Principles of Deep Learning Theory* is the primary reference for the dNTK formalism invoked in Chapter 11.

The compressed-sensing background follows Candès et al. (2006) and the textbook treatment of Foucart and Rauhut (2013).



> Editorial navigation: [Previous source](0788-0791-ch19-residual-questions-spectral-design-and-conclusion.md#pdf-0791) · [Next source](0797-0800-appendix-b-notation-summary.md#pdf-0797)
