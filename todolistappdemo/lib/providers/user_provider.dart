// File: lib/providers/user_provider.dart
import 'package:flutter/material.dart';
import '../models/user_model.dart';
import '../services/user_service.dart';

class UserProvider with ChangeNotifier {
  final UserService _userService = UserService();

  List<AppUser> _users = [];
  bool _isLoading = false;

  List<AppUser> get users => _users;
  bool get isLoading => _isLoading;

  Future<void> fetchUsers({bool forceRefresh = false}) async {
    // Chỉ tải 1 lần trừ khi bắt buộc
    if (_users.isNotEmpty && !forceRefresh) return;

    _isLoading = true;
    notifyListeners();
    try {
      _users = await _userService.getUsers();
    } catch (e) {
      // Bỏ qua lỗi
    }
    _isLoading = false;
    notifyListeners();
  }
}