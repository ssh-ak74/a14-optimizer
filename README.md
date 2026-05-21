# A14 Performance Engineering Suite

Professional-grade Windows Performance Control Layer + AI Diagnostic Engine + System Orchestration Platform.

## Implemented in this scaffold

### C# (.NET 8) control layer
- Modular solution with core contracts, telemetry collector, snapshot manager, optimization engine, and app orchestration shell.
- C# is the only path for optimization execution.
- Safety envelope for every action:
  - risk score
  - dependency impact text
  - rollback reference
  - dry-run support
  - snapshot creation before execution

### Python (3.11+) analysis layer
- gRPC AI diagnostic service (`StreamTelemetry`) consumes telemetry stream and emits:
  - health score
  - bottlenecks
  - recommendations with risk and confirmation hints
- Python does not execute system changes.

### gRPC communication
- Shared contract in `proto/telemetry.proto`.
- C# client (`AiDiagnosticsClient`) streams telemetry frames and reads AI responses in real time.

## Current project structure
- `src/A14.Suite.App` - orchestration demo entry point.
- `src/A14.Core` - shared domain contracts.
- `src/A14.Telemetry` - telemetry sampling provider.
- `src/A14.Grpc` - generated client stubs + AI diagnostics client.
- `src/A14.Optimization` - optimization safety and execution pipeline.
- `src/A14.Snapshot` - snapshot history and rollback foundation.
- `python-ai-service` - Python gRPC diagnostic service.
- `proto` - gRPC service definitions.

## MVP execution flow
1. C# collects telemetry.
2. C# streams telemetry to Python over gRPC.
3. Python returns health score, bottlenecks, recommendations.
4. C# evaluates risk and confirmation requirement.
5. C# creates pre-change snapshot.
6. C# executes action (or dry-run), logging rollback reference.

## What is still pending for “whole project complete”
This repo is now a solid foundation, but full production completion still requires:
- WinUI 3 desktop shell and full design-token implementation.
- Real Windows collectors (ETW/PDH/WMI, DPC/interrupt latency, per-process GPU/network, process tree).
- Full rollback diff timeline persisted to storage.
- Control Matrix graph UI and dependency traversal.
- Game orchestrator automation and per-game profiles.
- Network lab deep diagnostics and jitter/packet-loss estimations.
- Plugin security/versioning and sandbox boundaries.
- AI model pipeline integration (IsolationForest, clustering, forecasting).
