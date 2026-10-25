// Mirrors the API contract in Rework.WebApi/Controllers/SettingsController.cs.

export interface AppSetting {
  key: string;
  value: string;
  category: string;
  updatedAt: string;
}

export interface CreateSettingRequest {
  key: string;
  value: string;
  category: string;
}
