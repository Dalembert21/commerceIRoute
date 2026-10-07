import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ComercioServicio } from '../../servicios/comercio.servicio';
import { ResultadoProcesoFecha } from '../../modelos/comercio.modelo';

@Component({
  selector: 'app-procesar-fecha',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './procesar-fecha.component.html',
  styleUrl: './procesar-fecha.component.css'
})
export class ProcesarFechaComponent implements OnInit {
  private servicioComercio = inject(ComercioServicio);

  fechaProceso = '07/10/2026';
  fechasDetectadas: string[] = [];
  procesando = false;
  resultado: ResultadoProcesoFecha | null = null;
  mensajeError: string | null = null;

  ngOnInit(): void {
    this.cargarFechasExistentes();
  }

  // Trae las fechas existentes en la tabla commerce
  cargarFechasExistentes(): void {
    this.servicioComercio.obtenerComercios().subscribe({
      next: (resp) => {
        if (resp.exito && resp.datos) {
          const fechas = Array.from(new Set(resp.datos.map(c => c.fechaProceso).filter(f => !!f)));
          this.fechasDetectadas = fechas;
          if (fechas.length > 0 && !this.fechaProceso) {
            this.fechaProceso = fechas[0];
          }
        }
      },
      error: () => {}
    });
  }

  // Llama al endpoint POST /api/comercios/procesar-fecha
  procesarPorFecha(): void {
    if (!this.fechaProceso.trim()) {
      this.mensajeError = 'Debe indicar la fecha a procesar.';
      return;
    }

    this.procesando = true;
    this.resultado = null;
    this.mensajeError = null;

    this.servicioComercio.procesarPorFecha(this.fechaProceso.trim()).subscribe({
      next: (resp) => {
        this.procesando = false;
        if (resp.exito) {
          this.resultado = resp.datos;
          this.cargarFechasExistentes();
        } else {
          this.mensajeError = resp.mensaje;
        }
      },
      error: (err) => {
        this.procesando = false;
        this.mensajeError = err.error?.mensaje || 'Error al conectar con la API.';
      }
    });
  }
}
