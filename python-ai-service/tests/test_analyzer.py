from a14_ai.analyzer import analyze_frame
from a14_ai.models import FrameFeatures, ProcessFeatures


def test_analyzer_detects_high_cpu_and_disk():
    frame = FrameFeatures(
        cpu_total_percent=92,
        ram_pressure_percent=70,
        disk_queue_depth=3.1,
        disk_latency_ms=24,
    )
    processes = [
        ProcessFeatures(1, "game.exe", 34, 2048, 300, 500),
        ProcessFeatures(2, "updater.exe", 4, 120, 40, 30),
    ]

    result = analyze_frame(frame, processes)

    assert result.health_score < 50
    assert any(b.kind == "cpu" for b in result.bottlenecks)
    assert any(b.kind == "disk" for b in result.bottlenecks)
    assert any(r.id == "safe:background-suppression" for r in result.recommendations)


def test_analyzer_handles_healthy_frame():
    frame = FrameFeatures(
        cpu_total_percent=18,
        ram_pressure_percent=35,
        disk_queue_depth=0.2,
        disk_latency_ms=2.4,
    )
    processes = [ProcessFeatures(1, "idle", 1, 90, 0.1, 0.1)]

    result = analyze_frame(frame, processes)

    assert result.health_score > 75
