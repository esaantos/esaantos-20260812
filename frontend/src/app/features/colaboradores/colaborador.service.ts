import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ColaboradorListItem,
  ColaboradorResponse,
  CreateColaboradorRequest,
  UpdateColaboradorRequest
} from './colaborador.models';

@Injectable({ providedIn: 'root' })
export class ColaboradorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/colaboradores`;

  create(request: CreateColaboradorRequest): Observable<ColaboradorResponse> {
    return this.http.post<ColaboradorResponse>(this.baseUrl, request);
  }

  update(id: number, request: UpdateColaboradorRequest): Observable<ColaboradorResponse> {
    return this.http.put<ColaboradorResponse>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  list(): Observable<ColaboradorListItem[]> {
    return this.http.get<ColaboradorListItem[]>(this.baseUrl);
  }
}
