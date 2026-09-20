# Mizan Roadmap

This roadmap is a dependency map for engineering work. It is not a promise that every phase or technology listed below will remain unchanged.

## M0 — Product & Domain Foundation

Define:

- Product vision and problem
- User and core jobs
- Initial use cases
- MVP boundary
- Non-goals
- Domain vocabulary
- Financial rules and invariants
- Quality attributes
- Major risks
- Open questions
- Architecture decision points

**Gate outputs:** DG-001 through DG-005, as applicable.

## M1 — Architecture Foundation

Resolve the technical architecture needed to support the approved domain:

- Application boundaries
- Client architecture
- Persistence boundaries
- Backend boundary
- Security model
- Offline strategy
- Synchronization model
- Observability requirements

**Gate outputs:** DG-006 through DG-012.

## M2 — Implementation Foundation

Only after architecture gates are sufficiently resolved:

- Repository/application structure
- CI
- Static analysis and formatting
- Test infrastructure
- Local development environment
- Configuration and secrets strategy
- Minimal vertical slice

## M3 — Financial Core

Implement and verify the authoritative financial behavior:

- Accounts
- Monetary values/currencies according to approved scope
- Income/expense semantics
- Transfers
- Transaction history
- Balance calculation
- Correction/reversal semantics
- Idempotency and atomicity guarantees

## M4 — Mobile MVP

Implement the approved user workflows with the financial core as the source of truth.

Candidate workflows include:

- Account setup
- Recording income
- Recording expense
- Recording transfer
- Viewing balances
- Viewing/searching history
- Correcting records according to approved rules

## M5 — Persistence & Synchronization

Implement the approved local/server persistence and synchronization model.

The exact order and technology depend on DG-006 through DG-008.

## M6 — Security, Reliability & Observability

Harden the system for real use:

- Authentication/authorization as required
- Privacy controls
- Secure storage and transport
- Error handling and recovery
- Telemetry/observability
- Backup/recovery strategy
- Operational diagnostics

## M7 — End-to-End MVP

Verify the complete approved MVP across the supported platforms and operating conditions.

## M8 — Production Readiness

Validate:

- Performance
- Reliability
- Security
- Privacy
- Accessibility
- Data portability
- Migration/recovery
- Monitoring and incident response
- Release/rollback procedures

## M9+ — Evolution

Possible future areas, subject to product decisions and evidence:

- Budgets and goals
- Recurring transactions
- Import/export enhancements
- Bank integrations
- Advanced analytics
- Forecasting
- Intelligent assistance

Future intelligence must not become the source of financial truth.
