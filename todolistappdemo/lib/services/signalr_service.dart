// File: lib/services/signalr_service.dart
import 'package:flutter/material.dart'; // Cần BuildContext
import 'package:provider/provider.dart'; // Cần Provider
import 'package:signalr_netcore/signalr_client.dart';
import '../constants/api_constants.dart'; // Lấy BASE_URL
import '../providers/task_provider.dart'; // Cần TaskProvider
import 'notification_service.dart'; // Cần NotificationService

class SignalRService {
  // Singleton
  static final SignalRService _instance = SignalRService._internal();
  factory SignalRService() {
    return _instance;
  }
  SignalRService._internal();

  HubConnection? _hubConnection;
  BuildContext? _context; // Lưu context để tìm Provider

  // Lấy URL của Hub (bỏ /api)
  String get _hubUrl {
    // BASE_URL là http://10.0.2.2:5123/api
    // Cần đổi thành http://10.0.2.2:5123/notificationHub
    final baseUrl = BASE_URL.replaceAll("/api", "");
    return "$baseUrl/notificationHub";
  }

  // Khởi tạo kết nối
  Future<void> init(String token, BuildContext context) async {
    // Lưu context để sử dụng sau
    _context = context;

    // Chỉ khởi tạo nếu chưa có kết nối
    if (_hubConnection != null && _hubConnection!.state == HubConnectionState.Connected) {
      print("SignalR đã kết nối rồi.");
      return;
    }

    print("Đang khởi tạo SignalR...");

    // Tạo HubConnection
    _hubConnection = HubConnectionBuilder()
        .withUrl(
      _hubUrl,
      options: HttpConnectionOptions(
        // Gửi Token qua query string để Hub [Authorize]
        accessTokenFactory: () async => token,
      ),
    )
        .withAutomaticReconnect() // Tự động kết nối lại
        .build();

    // Lắng nghe các sự kiện từ Hub
    // Tên sự kiện "ReceiveTaskUpdate" phải khớp với Back-end
    _hubConnection!.on("ReceiveTaskUpdate", (arguments) {
      print('SignalR: Nhận được ReceiveTaskUpdate!');
      if (arguments != null && arguments is List && arguments.isNotEmpty) {
        String message = arguments[0] as String;
        // 1. Hiển thị thông báo cục bộ
        NotificationService().scheduleNotification(
          id: UniqueKey().hashCode, // ID ngẫu nhiên cho thông báo
          title: "Cập nhật công việc",
          body: message, // Lấy message từ Back-end
          scheduledDate: DateTime.now().add(Duration(seconds: 1)), // Hiện ngay
        );
      }

      // 2. Tự động tải lại danh sách "Công việc của tôi"
      if (_context != null) {
        Provider.of<TaskProvider>(_context!, listen: false)
            .fetchMyTasks(forceRefresh: true);
      }
    });

    // TODO: Thêm các listener khác (ví dụ: "ReceiveCommentUpdate")

    // Bắt đầu kết nối
    try {
      await _hubConnection!.start();
      print("SignalR Đã kết nối thành công!");
    } catch (e) {
      print("Lỗi khi kết nối SignalR: $e");
    }
  }

  // Dừng kết nối
  Future<void> stop() async {
    if (_hubConnection != null && _hubConnection!.state == HubConnectionState.Connected) {
      await _hubConnection!.stop();
      print("SignalR Đã ngắt kết nối.");
    }
    _hubConnection = null;
    _context = null; // Xóa context
  }
}