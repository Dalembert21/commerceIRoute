import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificacionServicio } from '../../servicios/notificacion.servicio';

@Component({
  selector: 'app-notificaciones',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notificaciones.component.html',
  styleUrl: './notificaciones.component.css'
})
export class NotificacionesComponent {
  servicioNotificacion = inject(NotificacionServicio);

  cerrar(id: string): void {
    this.servicioNotificacion.cerrar(id);
  }
}
