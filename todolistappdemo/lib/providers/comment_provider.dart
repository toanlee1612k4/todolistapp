// File: lib/providers/comment_provider.dart
import 'package:flutter/material.dart';
import 'package:signalr_netcore/http_connection_options.dart';
import 'package:signalr_netcore/hub_connection.dart';
import 'package:signalr_netcore/hub_connection_builder.dart';
import 'package:signalr_netcore/itransport.dart';
 // <-- THÊM GÓI signalr_core
import '../models/comment_model.dart';
import '../services/comment_service.dart';
import '../constants/api_constants.dart'; // <-- Để lấy BASE_URL

enum CommentStatus { Idle, Loading, Success, Error }

class CommentProvider with ChangeNotifier {
  final CommentService _commentService = CommentService();

  Map<int, List<Comment>> _commentsByTask = {};
  CommentStatus _status = CommentStatus.Idle;
  String _errorMessage = '';

  // ===== SignalR Hub =====
  HubConnection? _hubConnection;

  // ===== Getters =====
  List<Comment> commentsForTask(int taskId) => _commentsByTask[taskId] ?? [];
  CommentStatus get status => _status;
  String get errorMessage => _errorMessage;

  // ===== Khởi tạo & Kết nối Hub =====
  Future<void> initSignalR() async {
    if (_hubConnection != null &&
        _hubConnection!.state == HubConnectionState.Connected) return;

    _hubConnection = HubConnectionBuilder()
        .withUrl(
      "$BASE_URL/notificationHub".replaceFirst("/api", ""),
      options: HttpConnectionOptions(
        transport: HttpTransportType.WebSockets,
      ),
    )
        .build();

    // Lắng nghe sự kiện từ server
    _hubConnection!.on("ReceiveCommentNotification", (args) {
      if (args == null || args.isEmpty) return;

      final data = args[0] as Map<String, dynamic>;
      _handleCommentRealtime(data);
    });

    await _hubConnection!.start();
    debugPrint("✅ Connected to SignalR Hub");
  }

  // ===== Ngắt kết nối Hub =====
  Future<void> disposeSignalR() async {
    if (_hubConnection != null) {
      await _hubConnection!.stop();
      _hubConnection = null;
      debugPrint("❌ Disconnected from SignalR Hub");
    }
  }

  // ===== Xử lý dữ liệu realtime =====
  void _handleCommentRealtime(Map<String, dynamic> data) {
    final String type = data['Type'];
    final int taskId = data['TaskId'];

    if (type == 'Created') {
      final newCmt = Comment.fromJson(data['Comment']);
      if (_commentsByTask.containsKey(taskId)) {
        _commentsByTask[taskId]!.insert(0, newCmt);
      } else {
        _commentsByTask[taskId] = [newCmt];
      }
    } else if (type == 'Updated') {
      final updatedCmt = Comment.fromJson(data['Comment']);
      if (_commentsByTask.containsKey(taskId)) {
        final index = _commentsByTask[taskId]!
            .indexWhere((c) => c.commentId == updatedCmt.commentId);
        if (index != -1) {
          _commentsByTask[taskId]![index] = updatedCmt;
        }
      }
    } else if (type == 'Deleted') {
      final int deletedId = data['CommentId'];
      if (_commentsByTask.containsKey(taskId)) {
        _commentsByTask[taskId]!.removeWhere((c) => c.commentId == deletedId);
      }
    }

    notifyListeners();
  }

  // ===== CRUD =====
  Future<void> fetchComments(int taskId) async {
    if (_status == CommentStatus.Loading) return;
    if (_commentsByTask.containsKey(taskId)) return;

    _status = CommentStatus.Loading;
    notifyListeners();
    try {
      final comments = await _commentService.getComments(taskId);
      _commentsByTask[taskId] = comments;
      _status = CommentStatus.Success;
      _errorMessage = '';
    } catch (e) {
      _status = CommentStatus.Error;
      _errorMessage = e.toString().replaceFirst('Exception: ', '');
    }
    notifyListeners();
  }

  Future<bool> createComment(int taskId, String content) async {
    _errorMessage = '';
    try {
      final newComment = await _commentService.createComment(taskId, content);
      if (_commentsByTask.containsKey(taskId)) {
        _commentsByTask[taskId]!.insert(0, newComment);
      } else {
        _commentsByTask[taskId] = [newComment];
      }
      _status = CommentStatus.Success;
      notifyListeners();
      return true;
    } catch (e) {
      _errorMessage = e.toString().replaceFirst('Exception: ', '');
      notifyListeners();
      return false;
    }
  }

  Future<bool> updateComment(int taskId, int commentId, String content) async {
    _errorMessage = '';
    try {
      final updatedComment =
      await _commentService.updateComment(taskId, commentId, content);
      if (_commentsByTask.containsKey(taskId)) {
        final index = _commentsByTask[taskId]!
            .indexWhere((c) => c.commentId == commentId);
        if (index != -1) {
          _commentsByTask[taskId]![index] = updatedComment;
          notifyListeners();
        }
      }
      return true;
    } catch (e) {
      _errorMessage = e.toString().replaceFirst('Exception: ', '');
      notifyListeners();
      return false;
    }
  }

  Future<bool> deleteComment(int taskId, int commentId) async {
    _errorMessage = '';
    try {
      bool success = await _commentService.deleteComment(taskId, commentId);
      if (success && _commentsByTask.containsKey(taskId)) {
        _commentsByTask[taskId]!.removeWhere((c) => c.commentId == commentId);
        notifyListeners();
      }
      return success;
    } catch (e) {
      _errorMessage = e.toString().replaceFirst('Exception: ', '');
      notifyListeners();
      return false;
    }
  }

  void clearComments(int taskId) {
    _commentsByTask.remove(taskId);
    _status = CommentStatus.Idle;
    _errorMessage = '';
  }
}
