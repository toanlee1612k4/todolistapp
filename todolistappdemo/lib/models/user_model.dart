// File: lib/models/user_model.dart
class AppUser { // Đặt tên AppUser để tránh trùng với class User của Flutter
  final int id;
  final String fullName;
  final String? userName;
  final String? email;

  AppUser({
    required this.id,
    required this.fullName,
    this.userName,
    this.email,
  });

  factory AppUser.fromJson(Map<String, dynamic> json) {
    return AppUser(
      id: json['userId'] ?? json['id'], // API Members trả về userId
      fullName: json['fullName'] ?? 'N/A', // Đảm bảo không null
      userName: json['userName'],
      email: json['email'],
    );
  }
}