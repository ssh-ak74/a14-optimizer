# A14 Performance Engineering Suite

## Implemented capabilities

### 1) Real data collection layer (foundation added)
- Collector abstraction with ETW/PDH/WMI fallback classes in C#.
- Preferred capture pipeline that attempts ETW -> PDH -> WMI fallback.
- Extended telemetry frame includes DPC latency and interrupt rate fields.

### 2) Persistent snapshot/state system
- Disk-backed snapshot storage with atomic write pattern (`.tmp` then move).
- Versioned snapshot state, content hash, and diff support between snapshots.
- Snapshot manager now persists snapshots and can compute diffs.

### 3) Execution hardening layer
- Command sandbox allowlist.
- Execution planner stage.
- Policy engine stage (allow/confirm/deny) with kill-switch support.
- Optimizer flow: AI suggestion -> planner -> policy -> execution.

### 4) Game mode foundation
- Optimization mode includes `Gaming`.
- Stress scenario runner provides simulator-driven game-like CPU spike telemetry.

### 5) Observability time-series backbone
- JSONL telemetry time-series store.
- Replay API for last N minutes (e.g., replay last 10 minutes).

### 6) Plugin security model foundation
- Plugin manifest with permissions, CPU/RAM limits, and signature field.
- Security validator enforcing permission allowlist and resource caps.

### 7) AI evolution path (current state)
- Current: modular rules/hybrid scoring analyzer in Python.
- Contract and architecture support future ML model insertion without changing transport.

### 8) Testing/simulation layer
- Python analyzer tests.
- C# stress scenario runner for CPU spike simulation telemetry.

### 9) Decision engine separation
- AI plane (Python): diagnostics only.
- Policy engine (C#): allow/confirm/deny decisions.
- Execution engine (C# optimizer): controlled execution with snapshot.

### 10) User explanation layer
- Human-readable summary builder in app shell (`why lagging` style output).

## Current gaps for full production
- Actual Windows ETW/PDH/WMI bindings are still scaffold-level wrappers, not full low-level integration.
- WinUI 3 interface and full modules (network lab, control matrix, plugin isolation runtime) remain to be implemented.
