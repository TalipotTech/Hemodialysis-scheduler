export interface User {
  userID: number;
  username: string;
  role: string;
  isActive: boolean;
  createdAt: Date;
  lastLogin?: Date;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: UserInfo;
  expiresIn: number;
}

export interface UserInfo {
  username: string;
  role: string;
  name: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}
