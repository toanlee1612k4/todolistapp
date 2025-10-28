// File: lib/providers/project_provider.dart
import 'package:flutter/material.dart';
import '../models/project_model.dart';
import '../services/project_service.dart';

// Các trạng thái của việc tải dữ liệu
enum ProjectStatus { Idle, Loading, Success, Error }

class ProjectProvider with ChangeNotifier {
  final ProjectService _projectService = ProjectService();

  List<Project> _projects = [];
  ProjectStatus _status = ProjectStatus.Idle;
  String _errorMessage = '';

  // Getter để UI có thể truy cập
  List<Project> get projects => _projects;
  ProjectStatus get status => _status;
  String get errorMessage => _errorMessage;

  // Hàm để gọi từ UI
  Future<void> fetchProjects() async {
    _status = ProjectStatus.Loading;
    notifyListeners(); // Báo UI "đang tải..."

    try {
      _projects = await _projectService.getProjects();
      _status = ProjectStatus.Success;
    } catch (e) {
      _status = ProjectStatus.Error;
      _errorMessage = e.toString();
    }

    notifyListeners(); // Báo UI "Tải xong!" (kể cả lỗi)
  }

  // ==========================================
  // ===== BẠN HÃY THÊM HÀM NÀY VÀO FILE =====
  // ==========================================
  Future<bool> createProject({
    required String name,
    required String description,
    required DateTime startDate,
    DateTime? endDate,
    int? departmentId,
  }) async {
    try {
      // Gọi service để tạo
      final newProject = await _projectService.createProject(
        name: name,
        description: description,
        startDate: startDate,
        endDate: endDate,
        departmentId: departmentId,
      );

      // Nếu thành công, thêm vào list và báo UI cập nhật
      _projects.insert(0, newProject); // Thêm vào đầu danh sách
      _status = ProjectStatus.Success; // Đảm bảo status là Success
      notifyListeners();
      return true;

    } catch (e) {
      _errorMessage = e.toString();
      _status = ProjectStatus.Error; // Đặt status là Error
      notifyListeners();
      return false;
    }
  }
}