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
import { ConfirmDialog } from '../../shared/confirm-dialog/confirm-dialog';
import { UnidadeListItem } from '../unidades/unidade.models';
import { UnidadeService } from '../unidades/unidade.service';
import { UsuarioListItem } from '../usuarios/usuario.models';
import { UsuarioService } from '../usuarios/usuario.service';
import { ColaboradorListItem, UpdateColaboradorRequest } from './colaborador.models';
import { ColaboradorService } from './colaborador.service';
import { EditColaboradorDialog } from './edit-colaborador-dialog/edit-colaborador-dialog';

@Component({
  selector: 'app-colaboradores-page',
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
  templateUrl: './colaboradores.page.html',
  styleUrl: './colaboradores.page.scss'
})
export class ColaboradoresPage implements OnInit, AfterViewInit {
  private readonly fb = inject(FormBuilder);
  private readonly colaboradorService = inject(ColaboradorService);
  private readonly unidadeService = inject(UnidadeService);
  private readonly usuarioService = inject(UsuarioService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  @ViewChild(MatPaginator) private paginator!: MatPaginator;

  readonly form = this.fb.nonNullable.group({
    nome: ['', Validators.required],
    unidadeId: [null as number | null, Validators.required],
    usuarioId: [null as number | null, Validators.required]
  });

  readonly displayedColumns = ['codigo', 'nome', 'unidade', 'acoes'];
  readonly dataSource = new MatTableDataSource<ColaboradorListItem>([]);
  readonly unidadesAtivas = signal<UnidadeListItem[]>([]);
  readonly unidadesTodas = signal<UnidadeListItem[]>([]);
  readonly usuariosAtivos = signal<UsuarioListItem[]>([]);
  readonly submitting = signal(false);

  ngOnInit(): void {
    this.loadUnidades();
    this.loadUsuarios();
    this.loadColaboradores();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);

    const raw = this.form.getRawValue();

    this.colaboradorService
      .create({
        nome: raw.nome,
        unidadeId: raw.unidadeId!,
        usuarioId: raw.usuarioId!
      })
      .subscribe({
        next: (created) => {
          this.submitting.set(false);
          this.snackBar.open(`Colaborador "${created.nome}" cadastrado com sucesso.`, 'Fechar', { duration: 4000 });
          this.form.reset({ nome: '', unidadeId: null, usuarioId: null });
          this.loadColaboradores();
        },
        error: (err) => {
          this.submitting.set(false);
          this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 });
          if (err.status === 422) {
            // A unidade selecionada pode ter sido inativada entre o carregamento do
            // formulário e o envio — recarrega a lista para remover a opção inválida.
            this.loadUnidades();
          }
          if (err.status === 409) {
            // O usuário selecionado pode ter sido vinculado a outro colaborador entre
            // o carregamento do formulário e o envio — recarrega a lista para remover a opção inválida.
            this.loadUsuarios();
          }
        }
      });
  }

  editar(colaborador: ColaboradorListItem): void {
    this.dialog
      .open(EditColaboradorDialog, { data: { colaborador, unidades: this.unidadesTodas() } })
      .afterClosed()
      .subscribe((request: UpdateColaboradorRequest | undefined) => {
        if (!request) {
          return;
        }

        this.colaboradorService.update(colaborador.id, request).subscribe({
          next: () => {
            this.snackBar.open('Colaborador atualizado com sucesso.', 'Fechar', { duration: 4000 });
            this.loadColaboradores();
          },
          error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
        });
      });
  }

  remover(colaborador: ColaboradorListItem): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          title: 'Remover colaborador',
          message: `Tem certeza que deseja remover o colaborador "${colaborador.nome}" (${colaborador.codigo})? Esta ação não pode ser desfeita.`,
          confirmLabel: 'Remover'
        }
      })
      .afterClosed()
      .subscribe((confirmado: boolean | undefined) => {
        if (!confirmado) {
          return;
        }

        this.colaboradorService.delete(colaborador.id).subscribe({
          next: () => {
            this.snackBar.open('Colaborador removido com sucesso.', 'Fechar', { duration: 4000 });
            this.loadColaboradores();
          },
          error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
        });
      });
  }

  private loadUnidades(): void {
    this.unidadeService.list().subscribe({
      next: (list) => {
        this.unidadesTodas.set(list);
        this.unidadesAtivas.set(list.filter((u) => u.status === 'Ativo'));
      },
      error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
    });
  }

  private loadUsuarios(): void {
    this.usuarioService.list('Ativo').subscribe({
      next: (list) => this.usuariosAtivos.set(list),
      error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
    });
  }

  private loadColaboradores(): void {
    this.colaboradorService.list().subscribe({
      next: (list) => (this.dataSource.data = list),
      error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
    });
  }
}
