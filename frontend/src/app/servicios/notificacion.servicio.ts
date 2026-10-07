import { Injectable, signal } from '@angular/core';

export type TipoNotificacion = 'exito' | 'error' | 'advertencia' | 'info';

export interface NotificacionItem {
  id: string;
  tipo: TipoNotificacion;
  mensaje: string;
  duracionMs: number;
}

/**
 * Servicio Angular para la gestión de notificaciones tipo Toast / Snackbar
 * Siguiendo el estándar de Angular y Material Design para retroalimentación de usuario.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificacionServicio {
  readonly notificaciones = signal<NotificacionItem[]>([]);

  mostrar(mensaje: string | null | undefined, tipo: TipoNotificacion = 'info', duracionMs = 5000): void {
    if (!mensaje || !mensaje.trim()) return;
    const id = Math.random().toString(36).substring(2, 9);
    const item: NotificacionItem = { id, tipo, mensaje, duracionMs };

    this.notificaciones.update(lista => [...lista, item]);

    if (duracionMs > 0) {
      setTimeout(() => {
        this.cerrar(id);
      }, duracionMs);
    }
  }

  exito(mensaje: string | null | undefined, duracionMs = 4500): void {
    this.mostrar(mensaje, 'exito', duracionMs);
  }

  error(mensaje: string | null | undefined, duracionMs = 6000): void {
    this.mostrar(mensaje, 'error', duracionMs);
  }

  advertencia(mensaje: string | null | undefined, duracionMs = 5000): void {
    this.mostrar(mensaje, 'advertencia', duracionMs);
  }

  info(mensaje: string | null | undefined, duracionMs = 4500): void {
    this.mostrar(mensaje, 'info', duracionMs);
  }

  cerrar(id: string): void {
    this.notificaciones.update(lista => lista.filter(n => n.id !== id));
  }
}
