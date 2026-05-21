from concurrent import futures
import grpc

import telemetry_pb2
import telemetry_pb2_grpc


class TelemetryAnalysisService(telemetry_pb2_grpc.TelemetryAnalysisServicer):
    def StreamTelemetry(self, request_iterator, context):
        for frame in request_iterator:
            score = max(0, min(100, int(100 - frame.cpu_total_percent * 0.5 - frame.disk_latency_ms * 1.2)))
            bottlenecks = []
            if frame.cpu_total_percent > 85:
                bottlenecks.append(telemetry_pb2.Bottleneck(type="cpu", target="system", severity=80, explanation="High sustained CPU utilization"))
            if frame.disk_latency_ms > 20:
                bottlenecks.append(telemetry_pb2.Bottleneck(type="disk", target="system", severity=70, explanation="Elevated disk latency"))

            recommendations = [
                telemetry_pb2.Recommendation(
                    id="safe:background-suppression",
                    action="Suppress non-essential background processes",
                    estimated_impact=35,
                    risk_score=20,
                    requires_confirmation=True,
                )
            ]

            yield telemetry_pb2.AnalysisResponse(
                health_score=score,
                bottlenecks=bottlenecks,
                recommendations=recommendations,
                model_version="mvp-rule-v0",
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
