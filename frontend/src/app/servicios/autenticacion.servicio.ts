import { Injectable, signal } from '@angular/core';

export interface UsuarioSesion {
  nombreUsuario: string;
}

@Injectable({
  providedIn: 'root'
})
export class AutenticacionServicio {
  private claveAlmacenamiento = 'sesion_usuario_iroute';
  usuarioActual = signal<UsuarioSesion | null>(this.obtenerSesionGuardada());

  iniciarSesion(nombreUsuario: string, contrasena: string): boolean {
    const usuarioLimpio = nombreUsuario?.trim() ?? '';
    const claveLimpia = contrasena?.trim() ?? '';

    if (usuarioLimpio.length > 0 && claveLimpia.length > 0) {
      const usuario: UsuarioSesion = {
        nombreUsuario: usuarioLimpio
      };
      localStorage.setItem(this.claveAlmacenamiento, JSON.stringify(usuario));
      this.usuarioActual.set(usuario);
      return true;
    }
    return false;
  }

  cerrarSesion(): void {
    localStorage.removeItem(this.claveAlmacenamiento);
    this.usuarioActual.set(null);
  }

  estaAutenticado(): boolean {
    return this.usuarioActual() !== null;
  }

  private obtenerSesionGuardada(): UsuarioSesion | null {
    try {
      const datos = localStorage.getItem(this.claveAlmacenamiento);
      return datos ? JSON.parse(datos) : null;
    } catch {
      return null;
    }
  }
}
