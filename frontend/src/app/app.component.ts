import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BarraNavegacionComponent } from './componentes/barra-navegacion/barra-navegacion.component';
import { NotificacionesComponent } from './componentes/notificaciones/notificaciones.component';
import { AutenticacionServicio } from './servicios/autenticacion.servicio';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, BarraNavegacionComponent, NotificacionesComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  servicioAuth = inject(AutenticacionServicio);
  titulo = 'iRoute Comercio';
}
