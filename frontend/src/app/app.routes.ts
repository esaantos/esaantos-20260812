import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';
import { Shell } from './layout/shell/shell';
import { LoginPage } from './features/login/login.page';
import { UsuariosPage } from './features/usuarios/usuarios.page';
import { UnidadesPage } from './features/unidades/unidades.page';
import { ColaboradoresPage } from './features/colaboradores/colaboradores.page';

export const routes: Routes = [
  { path: 'login', component: LoginPage },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      { path: 'usuarios', component: UsuariosPage },
      { path: 'unidades', component: UnidadesPage },
      { path: 'colaboradores', component: ColaboradoresPage },
      { path: '', redirectTo: 'usuarios', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: '' }
];
