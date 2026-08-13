import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatListModule } from '@angular/material/list';

import { ColaboradorResumo } from '../unidade.models';

export interface ColaboradoresDialogData {
  unidadeNome: string;
  colaboradores: ColaboradorResumo[];
}

@Component({
  selector: 'app-colaboradores-dialog',
  imports: [MatDialogModule, MatButtonModule, MatListModule],
  templateUrl: './colaboradores-dialog.html'
})
export class ColaboradoresDialog {
  readonly data = inject<ColaboradoresDialogData>(MAT_DIALOG_DATA);
}
