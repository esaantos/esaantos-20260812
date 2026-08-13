import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { UnidadeListItem } from '../../unidades/unidade.models';
import { ColaboradorListItem, UpdateColaboradorRequest } from '../colaborador.models';

export interface EditColaboradorDialogData {
  colaborador: ColaboradorListItem;
  unidades: UnidadeListItem[];
}

@Component({
  selector: 'app-edit-colaborador-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './edit-colaborador-dialog.html'
})
export class EditColaboradorDialog {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<EditColaboradorDialog>);
  readonly data = inject<EditColaboradorDialogData>(MAT_DIALOG_DATA);

  readonly form = this.fb.nonNullable.group({
    nome: [this.data.colaborador.nome, Validators.required],
    unidadeId: [this.data.colaborador.unidade.id, Validators.required]
  });

  isUnidadeSelecionavel(unidade: UnidadeListItem): boolean {
    return unidade.status === 'Ativo' || unidade.id === this.data.colaborador.unidade.id;
  }

  cancel(): void {
    this.dialogRef.close();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: UpdateColaboradorRequest = this.form.getRawValue();
    this.dialogRef.close(request);
  }
}
