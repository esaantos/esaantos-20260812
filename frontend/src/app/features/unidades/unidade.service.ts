import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CreateUnidadeRequest, UnidadeListItem, UnidadeResponse } from './unidade.models';

@Injectable({ providedIn: 'root' })
export class UnidadeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/unidades`;

  create(request: CreateUnidadeRequest): Observable<UnidadeResponse> {
    return this.http.post<UnidadeResponse>(this.baseUrl, request);
  }

  inativar(id: number): Observable<UnidadeResponse> {
    return this.http.put<UnidadeResponse>(`${this.baseUrl}/${id}`, { status: 'Inativo' });
  }

  ativar(id: number): Observable<UnidadeResponse> {
    return this.http.put<UnidadeResponse>(`${this.baseUrl}/${id}`, { status: 'Ativo' });
  }

  list(): Observable<UnidadeListItem[]> {
    return this.http.get<UnidadeListItem[]>(this.baseUrl);
  }
}
