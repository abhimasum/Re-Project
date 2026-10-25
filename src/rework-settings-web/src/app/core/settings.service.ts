import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './config';
import { AppSetting, CreateSettingRequest } from './settings.models';

/** Talks to the backend's /api/settings endpoints. Every call returns an Observable; components subscribe. */
@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${API_BASE_URL}/api/settings`;

  list(category?: string): Observable<AppSetting[]> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    return this.http.get<AppSetting[]>(this.base, { params });
  }

  create(request: CreateSettingRequest): Observable<AppSetting> {
    return this.http.post<AppSetting>(this.base, request);
  }

  update(key: string, value: string): Observable<AppSetting> {
    return this.http.put<AppSetting>(`${this.base}/${encodeURIComponent(key)}`, { value });
  }
}
