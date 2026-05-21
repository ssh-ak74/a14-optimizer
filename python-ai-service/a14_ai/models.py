from dataclasses import dataclass


@dataclass(slots=True)
class ProcessFeatures:
    pid: int
    name: str
    cpu_percent: float
    ram_mb: float
    disk_read_kbps: float
    disk_write_kbps: float


@dataclass(slots=True)
class FrameFeatures:
    cpu_total_percent: float
    ram_pressure_percent: float
    disk_queue_depth: float
    disk_latency_ms: float
