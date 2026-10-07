import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ComercioServicio } from '../../servicios/comercio.servicio';
import { Comercio } from '../../modelos/comercio.modelo';

@Component({
  selector: 'app-comercios-activos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comercios-activos.component.html',
  styleUrl: './comercios-activos.component.css'
})
export class ComerciosActivosComponent implements OnInit {
  private servicioComercio = inject(ComercioServicio);

  listaComercios: Comercio[] = [];
  terminoBusqueda = '';
  cargando = false;

  ngOnInit(): void {
    this.cargarComercios();
  }

  // Consulta la tabla commerce llamando a GET /api/comercios
  cargarComercios(): void {
    this.cargando = true;
    this.servicioComercio.obtenerComercios().subscribe({
      next: (resp) => {
        this.cargando = false;
        if (resp.exito) {
          this.listaComercios = resp.datos || [];
        }
      },
      error: () => {
        this.cargando = false;
      }
    });
  }

  get comerciosFiltrados(): Comercio[] {
    if (!this.terminoBusqueda.trim()) return this.listaComercios;
    const q = this.terminoBusqueda.toLowerCase().trim();
    return this.listaComercios.filter(c =>
      (c.pcNomcomred && c.pcNomcomred.toLowerCase().includes(q)) ||
      (c.pcNumdoc && c.pcNumdoc.toLowerCase().includes(q)) ||
      (c.pcCodcom && c.pcCodcom.toLowerCase().includes(q)) ||
      (c.pcProcessdate && c.pcProcessdate.toLowerCase().includes(q))
    );
  }
}
