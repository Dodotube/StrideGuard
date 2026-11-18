# 🛡️ StrideGuard — Threat Modeling Platform (STRIDE)

Plataforma web corporativa voltada à prática de **Security by Design** e **Shift-Left Security**, permitindo que arquitetos e desenvolvedores modelem sistemas, identifiquem ameaças automaticamente com base na metodologia **STRIDE**, avaliem riscos de forma quantitativa e gerenciem contramedidas técnicas com rastreabilidade completa.

> **Projeto Final — Engenharia de Software / Desenvolvimento de Sistemas**  
> **Instituição:** Centro Universitário de Brasília (UniCEUB)  
> **Autor:** Paulo Israel de Aguiar  
> **Repositório:** [github.com/Dodotube/StrideGuard](https://github.com/Dodotube/StrideGuard)

---

## 🎯 Funcionalidades Principais

* **Autenticação & Controle de Acesso (JWT + BCrypt):** Registro e login de usuários com emissão de tokens JWT e isolamento total de portfólio por proprietário.
* **Gestão de Projetos e Componentes Arquiteturais:** Cadastro de sistemas e mapeamento de ativos (*API REST, Banco de Dados, Autenticação/IdP, Frontend SPA, Mensageria/Fila*) classificados por **Fronteira de Confiança** (*Internet Pública, DMZ, Rede Interna*).
* **Motor Automatizado STRIDE:** Análise inteligente da arquitetura cadastrada para identificação preliminar de vetores de ataque pertinentes a cada tipo de componente e zona de exposição.
* **Radar das 6 Dimensões STRIDE & Semáforo GO/NO-GO:** Painel interativo com contagem e filtragem por categoria (*Spoofing, Tampering, Repudiation, Information Disclosure, Denial of Service, Elevation of Privilege*) e indicador executivo de liberação conforme riscos Altos/Críticos abertos.
* **Mapa Visual de Fronteiras de Confiança (*Trust Boundaries*):** Visualização gráfica dos componentes distribuídos entre *Internet Pública*, *DMZ* e *Rede Interna*, sinalizando nós **Blindados** ou com ameaças pendentes.
* **Ameaças Manuais com Prévia de Risco:** Registro customizado de ameaças com cálculo dinâmico em tempo real da matriz `Probabilidade × Impacto`.
* **Gestão de Contramedidas & Trava de Validação:** Fluxo modal para vínculo de controles técnicos aprovados, impedindo o encerramento prematuro de ameaças sem mitigação comprovada.
* **Relatório Executivo:** Modo de exportação/impressão formatado com métricas de cobertura e histórico de mitigações.
* **Documentação Interativa (Swagger / OpenAPI 3.0):** Endpoints RESTful documentados e testáveis em `/swagger`.

---

## 🏗️ Arquitetura e Stack Tecnológica

O projeto foi estruturado seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, garantindo desacoplamento entre regras de negócio, persistência e apresentação:

```text
StrideGuard/
├── src/
│   ├── StrideGuard.Domain/          # Entidades, Enums STRIDE, Exceções e Invariantes de Negócio
│   ├── StrideGuard.Application/     # DTOs, Interfaces de Repositório, AuthService e MotorGeradorStrideService
│   ├── StrideGuard.Infrastructure/  # AppDbContext (EF Core), DbSeeder, Repositórios e Segurança (JWT/BCrypt)
│   ├── StrideGuard.API/             # Controllers REST, Middleware GlobalExceptionHandler, Swagger e Frontend (wwwroot)
│   └── StrideGuard.Tests/           # Testes Unitários automatizados com xUnit e Moq
├── Dockerfile
├── docker-compose.yml
└── StrideGuard.slnx
```

* **Backend:** C# / ASP.NET Core Web API
* **Persistência:** Entity Framework Core + SQL Server 2022
* **Segurança:** Autenticação JWT Bearer + Hash de senhas com BCrypt
* **Frontend:** HTML5, JavaScript (Fetch API) e Tailwind CSS (servido estaticamente em `wwwroot/index.html`)
* **Qualidade & Testes:** xUnit + Moq
* **Infraestrutura:** Docker & Docker Compose

---

## 📏 Regras de Negócio Implementadas

| Código | Regra de Negócio | Comportamento no Sistema |
| :--- | :--- | :--- |
| **RN01** | **Taxonomia STRIDE Obrigatória** | Toda ameaça (automática ou manual) pertence estritamente a uma das 6 categorias válidas do modelo STRIDE. |
| **RN02** | **Matriz Determinística de Risco** | O nível de criticidade é calculado automaticamente via `Score = Probabilidade × Impacto` (1–5): **Baixo** (1–6), **Médio** (7–14), **Alto** (15–19) e **Crítico** (20–25). |
| **RN03** | **Trava de Mitigação Obrigatória** | O domínio bloqueia a alteração de status para **`Mitigada`** caso não exista ao menos uma contramedida vinculada e formalmente **aprovada**. |
| **RN04** | **Isolamento de Propriedade** | Apenas o usuário criador do projeto pode visualizar, editar ou excluir sua arquitetura e ameaças. |
| **RN05** | **Pré-condição de Análise** | O motor automatizado STRIDE exige ao menos um componente arquitetural cadastrado no projeto antes da execução. |

---

## 🚀 Como Executar o Projeto

### Opção 1: Via Docker Compose (Recomendado)
Sobe automaticamente o container do **SQL Server 2022** e a aplicação **StrideGuard API + Frontend**:

```bash
docker-compose up -d --build
```

* **Interface Web:** `http://localhost:5000`
* **Swagger UI (API):** `http://localhost:5000/swagger`

### Opção 2: Execução Local via .NET CLI
Certifique-se de ter uma instância do SQL Server configurada em `src/StrideGuard.API/appsettings.json` e execute:

```bash
dotnet restore
dotnet run --project src/StrideGuard.API
```

### Credenciais Padrão (Seed Automático)
Ao iniciar pela primeira vez, o `DbSeeder` cria automaticamente um usuário arquiteto e um projeto modelo (*API Gateway de Pagamentos PIX*):
* **E-mail:** `arquiteto@strideguard.com`
* **Senha:** `Stride@123`

---

## 🧪 Testes Automatizados

Para executar a suíte de testes unitários que valida o motor STRIDE, o cálculo da matriz de risco (RN02) e as travas de domínio (RN03):

```bash
dotnet test
```
