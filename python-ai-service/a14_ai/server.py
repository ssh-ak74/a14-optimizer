from concurrent import futures

import grpc

import telemetry_pb2
import telemetry_pb2_grpc
from a14_ai.analyzer import analyze_frame
from a14_ai.models import FrameFeatures, ProcessFeatures


class TelemetryAnalysisService(telemetry_pb2_grpc.TelemetryAnalysisServicer):
    def StreamTelemetry(self, request_iterator, context):
        for frame in request_iterator:
            ram_pressure = 0.0 if frame.ram_total_gb <= 0 else (frame.ram_used_gb / frame.ram_total_gb) * 100.0
            features = FrameFeatures(
                cpu_total_percent=frame.cpu_total_percent,
                ram_pressure_percent=ram_pressure,
                disk_queue_depth=frame.disk_queue_depth,
                disk_latency_ms=frame.disk_latency_ms,
            )

            process_features = [
                ProcessFeatures(
                    pid=p.pid,
                    name=p.name,
                    cpu_percent=p.cpu_percent,
                    ram_mb=p.ram_mb,
                    disk_read_kbps=p.disk_read_kbps,
                    disk_write_kbps=p.disk_write_kbps,
                )
                for p in frame.processes
            ]

            signal = analyze_frame(features, process_features)
            yield telemetry_pb2.AnalysisResponse(
                health_score=signal.health_score,
                bottlenecks=[
                    telemetry_pb2.Bottleneck(
                        type=b.kind,
                        target=b.target,
                        severity=b.severity,
                        explanation=b.explanation,
                    )
                    for b in signal.bottlenecks
                ],
                recommendations=[
                    telemetry_pb2.Recommendation(
                        id=r.id,
                        action=r.action,
                        estimated_impact=r.estimated_impact,
                        risk_score=r.risk_score,
                        requires_confirmation=r.requires_confirmation,
                    )
                    for r in signal.recommendations
                ],
                model_version=signal.model_version,
            )


def serve() -> None:
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    telemetry_pb2_grpc.add_TelemetryAnalysisServicer_to_server(TelemetryAnalysisService(), server)
    server.add_insecure_port("[::]:50051")
    server.start()
    print("A14 Python AI service running on :50051")
    server.wait_for_termination()


if __name__ == "__main__":
    serve()
