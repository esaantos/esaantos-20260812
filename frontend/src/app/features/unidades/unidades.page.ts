import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';

import { extractApiErrorMessage } from '../../core/http/api-error';
import { ConfirmDialog } from '../../shared/confirm-dialog/confirm-dialog';
import { ColaboradoresDialog } from './colaboradores-dialog/colaboradores-dialog';
import { UnidadeListItem } from './unidade.models';
import { UnidadeService } from './unidade.service';

@Component({
  selector: 'app-unidades-page',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatDialogModule
  ],
  templateUrl: './unidades.page.html',
  styleUrl: './unidades.page.scss'
})
export class UnidadesPage implements OnInit, AfterViewInit {
  private readonly fb = inject(FormBuilder);
  private readonly unidadeService = inject(UnidadeService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  @ViewChild(MatPaginator) private paginator!: MatPaginator;

  readonly form = this.fb.nonNullable.group({
    nome: ['', Validators.required]
  });

  readonly displayedColumns = ['codigoUnidade', 'nome', 'status', 'colaboradores', 'acoes'];
  readonly dataSource = new MatTableDataSource<UnidadeListItem>([]);
  readonly submitting = signal(false);

  ngOnInit(): void {
    this.loadUnidades();
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

    this.unidadeService.create(this.form.getRawValue()).subscribe({
      next: (created) => {
        this.submitting.set(false);
        this.snackBar.open(`Unidade "${created.nome}" cadastrada com sucesso (status Ativo).`, 'Fechar', {
          duration: 4000
        });
        this.form.reset({ nome: '' });
        this.loadUnidades();
      },
      error: (err) => {
        this.submitting.set(false);
        this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 });
      }
    });
  }

  verColaboradores(unidade: UnidadeListItem): void {
    this.dialog.open(ColaboradoresDialog, {
      data: { unidadeNome: unidade.nome, colaboradores: unidade.colaboradores }
    });
  }

  inativar(unidade: UnidadeListItem): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          title: 'Inativar unidade',
          message: `Tem certeza que deseja inativar a unidade "${unidade.nome}" (${unidade.codigoUnidade})? Unidades inativas não podem receber novos colaboradores. Esta transição não pode ser revertida por aqui.`,
          confirmLabel: 'Inativar'
        }
      })
      .afterClosed()
      .subscribe((confirmado: boolean | undefined) => {
        if (!confirmado) {
          return;
        }

        this.unidadeService.inativar(unidade.id).subscribe({
          next: () => {
            this.snackBar.open('Unidade inativada com sucesso.', 'Fechar', { duration: 4000 });
            this.loadUnidades();
          },
          error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
        });
      });
  }

  ativar(unidade: UnidadeListItem): void {
    this.dialog
      .open(ConfirmDialog, {
        data: {
          title: 'Ativar unidade',
          message: `Tem certeza que deseja ativar a unidade "${unidade.nome}" (${unidade.codigoUnidade})? Ela voltará a poder receber novos colaboradores.`,
          confirmLabel: 'Ativar'
        }
      })
      .afterClosed()
      .subscribe((confirmado: boolean | undefined) => {
        if (!confirmado) {
          return;
        }

        this.unidadeService.ativar(unidade.id).subscribe({
          next: () => {
            this.snackBar.open('Unidade ativada com sucesso.', 'Fechar', { duration: 4000 });
            this.loadUnidades();
          },
          error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
        });
      });
  }

  private loadUnidades(): void {
    this.unidadeService.list().subscribe({
      next: (list) => (this.dataSource.data = list),
      error: (err) => this.snackBar.open(extractApiErrorMessage(err), 'Fechar', { duration: 6000 })
    });
  }
}
