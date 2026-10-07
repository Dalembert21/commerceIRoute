import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AutenticacionServicio } from '../../servicios/autenticacion.servicio';

@Component({
  selector: 'app-barra-navegacion',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './barra-navegacion.component.html',
  styleUrl: './barra-navegacion.component.css'
})
export class BarraNavegacionComponent {
  servicioAuth = inject(AutenticacionServicio);
  private router = inject(Router);

  cerrarSesion(): void {
    this.servicioAuth.cerrarSesion();
    this.router.navigate(['/iniciar-sesion']);
  }
}
