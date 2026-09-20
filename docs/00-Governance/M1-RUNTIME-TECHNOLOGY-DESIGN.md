# M1 — Runtime & Technology Foundation Decision Gate

## Status
**State:** Accepted
**Phase:** M1 — Thin Technical Foundation
**Decision owner:** Khaled

## Purpose
Select only the runtime technologies required to implement the approved thin vertical slice. This gate deliberately avoids selecting future mobile, AI, integrations, or distributed infrastructure.

## Decision 1 — Backend runtime

### A — ASP.NET Core / .NET — recommended
Strong fit for explicit domain/application boundaries, relational transactions, mature testing, dependency injection, and the Product Owner's existing C#/.NET capability.

### B — Python / FastAPI
Excellent for AI/data-heavy evolution and rapid APIs, but introduces a second primary ecosystem if the broader product later uses a different frontend/mobile stack.

### C — Node.js / TypeScript
Strong full-stack type sharing and ecosystem, but does not provide a material advantage over A for the financial core.

**Recommendation:** A.

## Decision 2 — Database

### A — PostgreSQL — recommended
Relational integrity, transactional guarantees, constraints, indexing, JSON support where useful, portability, and strong fit for Operation/Effect persistence.

### B — SQL Server
Excellent relational capability and familiar to the Product Owner, but PostgreSQL gives the product a broader cloud/platform portability posture.

### C — SQLite for MVP
Useful for local/offline scenarios but not sufficient as the primary shared backend persistence for the product's thin server slice.

**Recommendation:** A.

## Decision 3 — API contract

### A — REST/HTTP JSON — recommended
Simple, explicit, easy to test, portable across web/mobile/future clients.

### B — GraphQL
Useful for complex client-driven reads, but unnecessary for the first vertical slice.

### C — gRPC
Strong internal service communication, but adds complexity without a current distributed-service requirement.

**Recommendation:** A.

## Decision 4 — Frontend in this slice

### A — No production UI yet; API-first vertical slice — recommended
Proves financial correctness before committing to UI architecture. A minimal API test client is sufficient.

### B — React web client now
Provides visible end-to-end UX but adds UI complexity before the core financial path is verified.

### C — Mobile-first
Premature because mobile technology is explicitly deferred to a later architecture gate.

**Recommendation:** A.

## Decision 5 — Test framework

For .NET: **xUnit + FluentAssertions** is recommended for domain/application tests, with ASP.NET integration testing for the API path.

The test stack must support:
- deterministic invariant tests;
- database integration tests;
- API end-to-end verification;
- failure/rollback tests;
- idempotency tests;
- rebuild verification.

## Decision 6 — Repository solution structure

Recommended:
- `src/` — production projects
- `tests/` — test projects
- `docs/` — governance/domain/product
- one solution file at repository root
- explicit dependency direction: `API → Application → Domain`
- `Infrastructure → Application/Domain`
- Domain depends on no infrastructure.

No generic shared/common project until a real cross-boundary need exists.

## Decision 7 — ORM / database access

### A — EF Core — recommended
Strong transactional support, migrations, testing ecosystem, and productive relational mapping.

### B — Dapper
More SQL control and less abstraction, but more persistence code and mapping responsibility.

### C — Raw ADO.NET
Maximum control, maximum implementation burden.

**Recommendation:** A, while keeping persistence behind application-facing abstractions so the domain does not depend on EF Core.

## Decision 8 — Migration strategy

### A — Versioned EF Core migrations committed to Git — recommended
Database schema becomes reproducible and reviewable.

### B — Manual SQL only
Maximum explicit SQL control but weaker developer workflow and repeatability.

### C — Generate schema dynamically
Not appropriate for production financial persistence.

**Recommendation:** A.

## Recommended thin-slice stack

**Backend:** ASP.NET Core / .NET  
**Database:** PostgreSQL  
**API:** REST/HTTP JSON  
**Client:** no production UI yet  
**Tests:** xUnit + FluentAssertions + ASP.NET integration testing  
**ORM:** EF Core  
**Migrations:** versioned EF Core migrations  
**Architecture:** modular monolith with Domain/Application/Infrastructure/API boundaries

## Definition of Done
- Backend runtime selected.
- Database selected.
- API style selected.
- Client sequencing selected.
- Test stack selected.
- Solution/project dependency direction selected.
- Persistence technology selected.
- Migration strategy selected.
- Decision recorded and downstream RED tests use the selected stack.

## Decision record
**Decision:** Accepted  
**Approval:** Khaled — 2026-09-20