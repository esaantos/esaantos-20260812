export type StatusUnidade = 'Ativo' | 'Inativo';

export interface CreateUnidadeRequest {
  nome: string;
}

export interface UnidadeResponse {
  id: number;
  codigoUnidade: string;
  nome: string;
  status: StatusUnidade;
}

export interface ColaboradorResumo {
  codigo: string;
  nome: string;
}

export interface UnidadeListItem {
  id: number;
  codigoUnidade: string;
  nome: string;
  status: StatusUnidade;
  colaboradores: ColaboradorResumo[];
}
