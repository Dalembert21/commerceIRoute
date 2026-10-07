import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AutenticacionServicio } from '../../servicios/autenticacion.servicio';

@Component({
  selector: 'app-inicio-sesion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inicio-sesion.component.html',
  styleUrl: './inicio-sesion.component.css'
})
export class InicioSesionComponent {
  private auth = inject(AutenticacionServicio);
  private router = inject(Router);

  usuario = '';
  clave = '';
  mostrarClave = false;
  errorMensaje: string | null = null;

  alternarMostrarClave(): void {
    this.mostrarClave = !this.mostrarClave;
  }

  rellenarCredenciales(usr: string, pass: string): void {
    this.usuario = usr;
    this.clave = pass;
  }

  iniciarSesion(): void {
    this.errorMensaje = null;
    const ok = this.auth.iniciarSesion(this.usuario, this.clave);
    if (ok) {
      this.router.navigate(['/cargar-archivo']);
    } else {
      this.errorMensaje = 'Usuario o clave incorrecta (use admin / 123456).';
    }
  }
}
