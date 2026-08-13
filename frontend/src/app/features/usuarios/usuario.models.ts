export type StatusUsuario = 'Ativo' | 'Inativo';

export interface CreateUsuarioRequest {
  login: string;
  senha: string;
}

export interface UpdateUsuarioRequest {
  senha?: string;
  status?: StatusUsuario;
}

export interface UsuarioResponse {
  id: number;
  codigo: string;
  login: string;
  status: string;
}

export interface UsuarioListItem {
  id: number;
  login: string;
  status: StatusUsuario;
}
