# A14 Performance Engineering Suite

A14 is a Windows performance control platform architecture split into:
- **C#/.NET control plane** (only layer allowed to modify system state)
- **Python AI diagnostics plane** (analysis-only)
- **gRPC streaming contract** between both planes

## What is implemented now

### C# control layer scaffold
- Multi-project .NET 8 solution under `src/`.
- Shared contracts for telemetry, risk, optimization requests/results, snapshot references, and AI responses.
- `TelemetryCollector` MVP for frame + process samples.
- `OptimizationEngine` MVP with risk scoring, confirmation checks, block policy, and pre-change snapshot creation.
- `SnapshotManager` with snapshot history and rollback lookup foundation.
- `AiDiagnosticsClient` gRPC streaming client bridge from C# to Python.
- End-to-end orchestration demo in `A14.Suite.App`.

### Python AI diagnostics scaffold
- gRPC service implementation for `TelemetryAnalysis.StreamTelemetry`.
- Analyzer module with reusable model objects + analysis engine (`a14-hybrid-rules-v1`).
- Health scoring, bottleneck detection, and ranked recommendation signals.
- Unit tests for analyzer behavior.

### Shared protocol
- `proto/telemetry.proto` with bidirectional telemetry-analysis stream.

## Repository layout
- `src/A14.Core` - shared C# contracts.
- `src/A14.Telemetry` - telemetry sampler.
- `src/A14.Snapshot` - snapshot history + rollback foundation.
- `src/A14.Optimization` - risk and execution gate.
- `src/A14.Grpc` - gRPC client bindings + analysis bridge.
- `src/A14.Suite.App` - demo orchestration flow.
- `python-ai-service/a14_ai` - Python service and analysis engine.
- `python-ai-service/tests` - analyzer tests.
- `proto` - shared protobuf contract.

## MVP execution flow
1. C# collects telemetry.
2. C# streams telemetry to Python over gRPC.
3. Python analyzes and returns health/bottlenecks/recommendations.
4. C# evaluates action risk.
5. C# creates pre-change snapshot.
6. C# executes or dry-runs action with rollback reference.

## Remaining to reach full production completion
- WinUI 3 app with full design-tokenized interface.
- Real Windows collectors (ETW/PDH/WMI, DPC/ISR latency, GPU/network per-process).
- Persistent snapshot diff/timeline and one-click restore execution engine.
- Game performance orchestrator and profile management.
- Network latency engine with jitter/loss diagnostics.
- Windows control matrix graphing and dependency-aware management.
- Plugin sandbox/signing/version policies.
- ML-based anomaly detection, clustering, and forecasting pipelines.
