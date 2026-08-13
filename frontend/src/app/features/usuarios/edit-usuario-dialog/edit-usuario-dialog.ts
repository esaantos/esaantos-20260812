import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { StatusUsuario, UpdateUsuarioRequest, UsuarioListItem } from '../usuario.models';

@Component({
  selector: 'app-edit-usuario-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './edit-usuario-dialog.html'
})
export class EditUsuarioDialog {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<EditUsuarioDialog>);
  readonly usuario = inject<UsuarioListItem>(MAT_DIALOG_DATA);

  readonly form = this.fb.nonNullable.group({
    senha: [''],
    status: [this.usuario.status as StatusUsuario, Validators.required]
  });

  cancel(): void {
    this.dialogRef.close();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const request: UpdateUsuarioRequest = { status: raw.status };
    if (raw.senha) {
      request.senha = raw.senha;
    }

    this.dialogRef.close(request);
  }
}
