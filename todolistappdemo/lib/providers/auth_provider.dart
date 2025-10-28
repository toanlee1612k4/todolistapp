// File: lib/providers/auth_provider.dart
import 'package:flutter/material.dart'; // <-- THÊM IMPORT NÀY (cho BuildContext)
import 'package:shared_preferences/shared_preferences.dart';
import '../services/auth_service.dart';
import 'package:jwt_decoder/jwt_decoder.dart';
import '../services/signalr_service.dart'; // <-- Đã import

class AuthProvider with ChangeNotifier {
  final AuthService _authService = AuthService();

  String? _token;
  bool _isAuthenticated = false;
  String _authMessage = '';

  // Biến lưu thông tin user
  String? _username;
  String? _email;
  String? _fullName;

  // --- Getters ---
  bool get isAuthenticated => _isAuthenticated;
  String? get token => _token;
  String get authMessage => _authMessage;

  // Getters cho thông tin user
  String? get username => _username;
  String? get email => _email;
  String? get fullName => _fullName;


  AuthProvider() {
    _tryAutoLogin();
  }

  // Tự động đăng nhập
  Future<void> _tryAutoLogin() async {
    final prefs = await SharedPreferences.getInstance();
    final storedToken = prefs.getString('authToken');
    if (storedToken != null) {
      _token = storedToken;
      _username = prefs.getString('username');
      _email = prefs.getString('email');
      _fullName = prefs.getString('fullName');
      _isAuthenticated = true;
    }
  }

  // ===== SỬA HÀM LOGIN ĐỂ NHẬN CONTEXT VÀ GỌI SIGNALR =====
  Future<bool> login(String username, String password, BuildContext context) async { // Thêm BuildContext context
    // Gọi hàm loginAndGetData từ AuthService
    final Map<String, dynamic>? loginData = await _authService.loginAndGetData(username, password);

    if (loginData != null && loginData['success'] == true) {
      final prefs = await SharedPreferences.getInstance();
      _token = await _authService.getToken();

      // Lưu thông tin user từ response
      _email = loginData['email'];
      _fullName = loginData['fullName'];
      _username = loginData['username'];

      await prefs.setString('email', _email ?? '');
      await prefs.setString('fullName', _fullName ?? '');
      await prefs.setString('username', _username ?? '');

      _isAuthenticated = true;
      _authMessage = loginData['message'] ?? 'Đăng nhập thành công!';

      // ===== KHỞI ĐỘNG SIGNALR SAU KHI LOGIN THÀNH CÔNG =====
      if (_token != null) {
        // Khởi động SignalR
        // Truyền context để SignalR có thể tìm Provider
        await SignalRService().init(_token!, context);
      }
      // ====================================================

      notifyListeners();
      return true;
    } else {
      _authMessage = loginData?['message'] ?? 'Tên tài khoản hoặc mật khẩu không đúng.';
      notifyListeners();
      return false;
    }
  }
  // =========================================================

  // Đăng ký (Không đổi)
  Future<bool> register({
    required String fullName, required String email, required String phoneNumber,
    required String username, required String password, required String confirmPassword,
  }) async {
    final result = await _authService.register(
      fullName: fullName, email: email, phoneNumber: phoneNumber,
      username: username, password: password, confirmPassword: confirmPassword,
    );
    _authMessage = result['message'];
    notifyListeners();
    return result['success'];
  }

  // ===== SỬA HÀM LOGOUT ĐỂ DỪNG SIGNALR =====
  Future<void> logout() async {
    await _authService.logout();
    _token = null;
    _isAuthenticated = false;
    _username = null;
    _email = null;
    _fullName = null;

    // ===== DỪNG KẾT NỐI SIGNALR =====
    await SignalRService().stop();
    // ================================

    // Xóa khỏi SharedPreferences (đã được gọi trong authService.logout())
    notifyListeners();
  }
  // ======================================

  // Hàm đổi mật khẩu (Đã có)
  Future<bool> changePassword({
    required String currentPassword,
    required String newPassword,
    required String confirmNewPassword,
  }) async {
    _authMessage = '';
    final result = await _authService.changePassword(
        currentPassword: currentPassword,
        newPassword: newPassword,
        confirmNewPassword: confirmNewPassword
    );
    _authMessage = result['message'];
    return result['success'];
  }

  // Hàm quên mật khẩu (Đã có)
  Future<bool> forgotPassword(String email) async {
    _authMessage = '';
    notifyListeners();
    final result = await _authService.forgotPassword(email);
    _authMessage = result['message'];
    notifyListeners();
    return result['success'];
  }

  // Hàm đặt lại mật khẩu (Đã có)
  Future<bool> resetPassword({
    required String email,
    required String token,
    required String newPassword,
    required String confirmNewPassword,
  }) async {
    _authMessage = '';
    notifyListeners();
    final result = await _authService.resetPassword(
        email: email,
        token: token,
        newPassword: newPassword,
        confirmNewPassword: confirmNewPassword
    );
    _authMessage = result['message'];
    notifyListeners();
    return result['success'];
  }

  // Hàm getUserId (Đã có và đúng)
  int? getUserId() {
    if (_token != null && _isAuthenticated) {
      try {
        Map<String, dynamic> decodedToken = JwtDecoder.decode(_token!);
        final userIdClaim = decodedToken['nameid'];
        if (userIdClaim is String) {
          return int.tryParse(userIdClaim);
        } else if (userIdClaim is int) {
          return userIdClaim;
        }
        print("Không tìm thấy claim 'nameid' trong token.");
        return null;
      } catch (e) {
        print("Lỗi giải mã token trong getUserId: $e");
        return null;
      }
    }
    return null;
  }
}