import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';

import { extractApiErrorMessage } from '../../core/http/api-error';
import { EditUsuarioDialog } from './edit-usuario-dialog/edit-usuario-dialog';
import { StatusUsuario, UpdateUsuarioRequest, UsuarioListItem } from './usuario.models';
import { UsuarioService } from './usuario.service';

@Component({
  selector: 'app-usuarios-page',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatDialogModule
  ],
  templateUrl: './usuarios.page.html',
  styleUrl: './usuarios.page.scss'
})
export class UsuariosPage implements OnInit, AfterViewInit {
  private readonly fb = inject(FormBuilder);
  private readonly usuarioService = inject(UsuarioService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  @ViewChild(MatPaginator) private paginator!: MatPaginator;

  readonly form = this.fb.nonNullable.group({
    login: ['', Validators.required],
    senha: ['', Validators.required]
  });

  readonly displayedColumns = ['login', 'status', 'acoes'];
  readonly dataSource = new MatTableDataSource<UsuarioListItem>([]);
  readonly filtroStatus = signal<StatusUsuario | ''>('');
  readonly submitting = signal(false);

  ngOnInit(): void {
    this.loadUsuarios();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  onFiltroChange(value: StatusUsuario | ''): void {
    this.filtroStatus.set(value);
    this.loadUsuarios();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);

    this.usuarioService.create(this.form.getRawValue()).subscribe({
      next: (created) => {
        this.submitting.set(false);
        this.snackBar.open(`Usuário "${created.login}" cadastrado com sucesso.`, 'Fechar', { duration: 4000 });
        this.form.reset({ login: '', senha: '' });
        this.loadUsuarios();
      },
      error: (err) => {
        this.submitting.set(false);
        this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 });
      }
    });
  }

  editar(usuario: UsuarioListItem): void {
    this.dialog
      .open(EditUsuarioDialog, { data: usuario })
      .afterClosed()
      .subscribe((request: UpdateUsuarioRequest | undefined) => {
        if (!request) {
          return;
        }

        this.usuarioService.update(usuario.id, request).subscribe({
          next: () => {
            this.snackBar.open('Usuário atualizado com sucesso.', 'Fechar', { duration: 4000 });
            this.loadUsuarios();
          },
          error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
        });
      });
  }

  private loadUsuarios(): void {
    const status = this.filtroStatus() || undefined;
    this.usuarioService.list(status).subscribe({
      next: (list) => (this.dataSource.data = list),
      error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
    });
  }
}
