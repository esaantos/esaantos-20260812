import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CreateUsuarioRequest,
  StatusUsuario,
  UpdateUsuarioRequest,
  UsuarioListItem,
  UsuarioResponse
} from './usuario.models';

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/usuarios`;

  create(request: CreateUsuarioRequest): Observable<UsuarioResponse> {
    return this.http.post<UsuarioResponse>(this.baseUrl, request);
  }

  update(id: number, request: UpdateUsuarioRequest): Observable<UsuarioResponse> {
    return this.http.put<UsuarioResponse>(`${this.baseUrl}/${id}`, request);
  }

  list(status?: StatusUsuario): Observable<UsuarioListItem[]> {
    const params = status ? new HttpParams().set('status', status) : undefined;
    return this.http.get<UsuarioListItem[]>(this.baseUrl, { params });
  }
}
