from __future__ import annotations

from dataclasses import dataclass
from typing import Iterable

from .models import FrameFeatures, ProcessFeatures


@dataclass(slots=True)
class BottleneckSignal:
    kind: str
    target: str
    severity: int
    explanation: str


@dataclass(slots=True)
class RecommendationSignal:
    id: str
    action: str
    estimated_impact: int
    risk_score: int
    requires_confirmation: bool


@dataclass(slots=True)
class AnalysisSignal:
    health_score: int
    bottlenecks: list[BottleneckSignal]
    recommendations: list[RecommendationSignal]
    model_version: str


MODEL_VERSION = "a14-hybrid-rules-v1"


def _clamp(value: int, low: int, high: int) -> int:
    return max(low, min(high, value))


def analyze_frame(frame: FrameFeatures, processes: Iterable[ProcessFeatures]) -> AnalysisSignal:
    bottlenecks: list[BottleneckSignal] = []

    cpu_penalty = frame.cpu_total_percent * 0.45
    ram_penalty = frame.ram_pressure_percent * 0.20
    io_penalty = frame.disk_latency_ms * 1.10 + frame.disk_queue_depth * 3.0
    health_score = _clamp(int(100 - cpu_penalty - ram_penalty - io_penalty), 0, 100)

    if frame.cpu_total_percent >= 85:
        bottlenecks.append(BottleneckSignal("cpu", "system", 82, "Sustained system CPU saturation"))

    if frame.ram_pressure_percent >= 85:
        bottlenecks.append(BottleneckSignal("memory", "system", 76, "High memory pressure detected"))

    if frame.disk_latency_ms >= 20 or frame.disk_queue_depth >= 2.5:
        bottlenecks.append(BottleneckSignal("disk", "system", 74, "Disk latency/queue depth indicates I/O contention"))

    noisy = sorted(processes, key=lambda p: (p.cpu_percent + (p.disk_write_kbps / 1024.0)), reverse=True)[:3]
    for proc in noisy:
        if proc.cpu_percent >= 20:
            bottlenecks.append(BottleneckSignal("process", proc.name, 65, f"High CPU by process {proc.name}"))

    recommendations = [
        RecommendationSignal(
            id="safe:background-suppression",
            action="Suppress non-essential background processes",
            estimated_impact=35,
            risk_score=20,
            requires_confirmation=True,
        ),
        RecommendationSignal(
            id="safe:power-plan-balanced",
            action="Switch to balanced power policy under sustained thermal load",
            estimated_impact=18,
            risk_score=10,
            requires_confirmation=False,
        ),
    ]

    return AnalysisSignal(
        health_score=health_score,
        bottlenecks=bottlenecks,
        recommendations=recommendations,
        model_version=MODEL_VERSION,
    )
