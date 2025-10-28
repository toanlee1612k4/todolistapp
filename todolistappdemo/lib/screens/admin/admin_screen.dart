// File: lib/screens/admin/admin_screen.dart
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http; // Cần http
import 'package:provider/provider.dart'; // Cần provider
import '../../constants/api_constants.dart';
import '../../providers/auth_provider.dart'; // Cần AuthProvider để lấy token

class AdminScreen extends StatefulWidget {
  @override
  _AdminScreenState createState() => _AdminScreenState();
}

class _AdminScreenState extends State<AdminScreen> {
  String _adminMessage = 'Đang kiểm tra quyền truy cập...';
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    // Gọi hàm kiểm tra quyền ngay khi màn hình mở
    _fetchAdminData();
  }

  // Hàm gọi API admin-only
  Future<void> _fetchAdminData() async {
    // Không cần setState loading ở đây vì build sẽ xử lý
    String? message;
    int? statusCode;

    try {
      // Lấy token từ AuthProvider
      final token = Provider.of<AuthProvider>(context, listen: false).token;
      if (token == null) {
        // Ném lỗi nếu chưa đăng nhập (dù ít khi xảy ra ở màn hình này)
        throw Exception('Chưa đăng nhập. Không thể kiểm tra quyền Admin.');
      }

      // Gọi API GET đến endpoint mới của AdminController
      final response = await http.get(
        Uri.parse('$BASE_URL/admin/admin-only'), // <-- URL ĐÃ SỬA
        headers: {
          'Authorization': 'Bearer $token', // Gửi token để xác thực
        },
      );

      statusCode = response.statusCode; // Lưu lại status code để xử lý

      if (response.statusCode == 200) {
        // Thành công (là Admin)
        final responseData = jsonDecode(response.body);
        message = responseData['message']; // Lấy message chào mừng
      } else if (response.statusCode == 403) {
        // Lỗi 403 Forbidden (Không phải Admin)
        message = 'Lỗi ${response.statusCode}: Bạn không có quyền truy cập khu vực này.';
      } else {
        // Các lỗi khác (401 Unauthorized - token sai/hết hạn, 500 Internal Server Error...)
        // Cố gắng đọc message lỗi từ server nếu có
        try {
          final errorData = jsonDecode(response.body);
          message = 'Lỗi ${response.statusCode}: ${errorData['message'] ?? 'Không thể truy cập dữ liệu Admin.'}';
        } catch(_) {
          // Nếu không đọc được body lỗi, hiển thị status code
          message = 'Lỗi ${response.statusCode}: Không thể truy cập dữ liệu Admin.';
        }
      }

    } catch (e) {
      // Lỗi kết nối mạng hoặc lỗi khác
      message = 'Lỗi kết nối: $e';
    }

    // Cập nhật state chỉ khi widget còn tồn tại
    if(mounted){
      setState(() {
        _adminMessage = message ?? 'Lỗi không xác định.'; // Gán message kết quả
        _isLoading = false; // Tắt trạng thái loading
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Khu vực Admin'),
        elevation: 1,
      ),
      body: Center(
        child: _isLoading
        // Hiển thị vòng tròn loading khi đang kiểm tra
            ? CircularProgressIndicator()
        // Hiển thị message kết quả
            : Padding(
          padding: const EdgeInsets.all(20.0),
          child: Text(
            _adminMessage,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w500,
              // Hiển thị màu đỏ nếu message bắt đầu bằng "Lỗi"
              color: (_adminMessage.startsWith('Lỗi'))
                  ? Colors.red[700] // Màu đỏ cho lỗi
                  : Colors.black87, // Màu đen mặc định
            ),
          ),
        ),
      ),
    );
  }
}