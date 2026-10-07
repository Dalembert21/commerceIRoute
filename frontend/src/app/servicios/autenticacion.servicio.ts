import { Injectable, signal } from '@angular/core';

export interface UsuarioSesion {
  nombreUsuario: string;
  nombreCompleto: string;
  rol: string;
}

@Injectable({
  providedIn: 'root'
})
export class AutenticacionServicio {
  private claveAlmacenamiento = 'sesion_usuario_iroute';
  usuarioActual = signal<UsuarioSesion | null>(this.obtenerSesionGuardada());

  iniciarSesion(nombreUsuario: string, contrasena: string): boolean {
    if ((nombreUsuario === 'admin' || nombreUsuario === 'evaluador') && contrasena === '123456') {
      const usuario: UsuarioSesion = {
        nombreUsuario,
        nombreCompleto: nombreUsuario === 'admin' ? 'Administrador IRoute' : 'Evaluador Técnico',
        rol: 'Supervisor'
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
