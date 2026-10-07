import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ComercioServicio } from '../../servicios/comercio.servicio';
import { ComercioCuarentena } from '../../modelos/comercio.modelo';

@Component({
  selector: 'app-comercios-cuarentena',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comercios-cuarentena.component.html',
  styleUrl: './comercios-cuarentena.component.css'
})
export class ComerciosCuarentenaComponent implements OnInit {
  private servicioComercio = inject(ComercioServicio);

  listaCuarentena: ComercioCuarentena[] = [];
  terminoBusqueda = '';
  cargando = false;
  mensajeError: string | null = null;

  ngOnInit(): void {
    this.cargarCuarentena();
  }

  // Consulta la lista llamando a GET /api/comercios/cuarentena
  cargarCuarentena(): void {
    this.cargando = true;
    this.mensajeError = null;

    this.servicioComercio.obtenerComerciosCuarentena().subscribe({
      next: (resp) => {
        this.cargando = false;
        if (resp.exito) {
          this.listaCuarentena = resp.datos || [];
        } else {
          this.mensajeError = resp.mensaje;
        }
      },
      error: (err) => {
        this.cargando = false;
        this.mensajeError = err.error?.mensaje || 'Error al consultar registros de cuarentena.';
      }
    });
  }

  // Filtro en memoria
  get registrosFiltrados(): ComercioCuarentena[] {
    if (!this.terminoBusqueda.trim()) return this.listaCuarentena;
    const q = this.terminoBusqueda.toLowerCase().trim();
    return this.listaCuarentena.filter(item =>
      (item.motivo && item.motivo.toLowerCase().includes(q)) ||
      (item.pcNomcomred && item.pcNomcomred.toLowerCase().includes(q)) ||
      (item.pcNumdoc && item.pcNumdoc.toLowerCase().includes(q)) ||
      (item.pcCodcom && item.pcCodcom.toLowerCase().includes(q)) ||
      (item.pcProcessdate && item.pcProcessdate.toLowerCase().includes(q))
    );
  }

  // Métricas de conteo por tipo de observación
  contarPorMotivo(tipo: 'nombre' | 'documento'): number {
    return this.listaCuarentena.filter(item => {
      const m = item.motivo?.toLowerCase() || '';
      return tipo === 'nombre' 
        ? (m.includes('nombre') || m.includes('nomcomred')) 
        : (m.includes('número') || m.includes('numdoc') || m.includes('letras') || m.includes('caracteres'));
    }).length;
  }
}
