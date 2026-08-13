export interface CreateColaboradorRequest {
  nome: string;
  unidadeId: number;
  usuarioId: number;
}

export interface UpdateColaboradorRequest {
  nome?: string;
  unidadeId?: number;
}

export interface UnidadeResumo {
  id: number;
  nome: string;
}

export interface ColaboradorResponse {
  id: number;
  codigo: string;
  nome: string;
  unidade: UnidadeResumo;
  usuarioId: number;
}

export interface ColaboradorListItem {
  id: number;
  codigo: string;
  nome: string;
  unidade: UnidadeResumo;
}
