import { inject } from '@angular/core';
import { Routes, Router, CanActivateFn } from '@angular/router';
import { CargarArchivoComponent } from './componentes/cargar-archivo/cargar-archivo.component';
import { ProcesarFechaComponent } from './componentes/procesar-fecha/procesar-fecha.component';
import { ComerciosCuarentenaComponent } from './componentes/comercios-cuarentena/comercios-cuarentena.component';
import { ComerciosActivosComponent } from './componentes/comercios-activos/comercios-activos.component';
import { InicioSesionComponent } from './componentes/inicio-sesion/inicio-sesion.component';
import { AutenticacionServicio } from './servicios/autenticacion.servicio';

const authGuard: CanActivateFn = () => {
  const auth = inject(AutenticacionServicio);
  const router = inject(Router);
  if (auth.estaAutenticado()) {
    return true;
  }
  return router.createUrlTree(['/iniciar-sesion']);
};

export const routes: Routes = [
  { path: '', redirectTo: 'iniciar-sesion', pathMatch: 'full' },
  { path: 'iniciar-sesion', component: InicioSesionComponent, title: 'iRoute - Inicio de Sesión' },
  { path: 'cargar-archivo', component: CargarArchivoComponent, canActivate: [authGuard], title: 'iRoute - Cargar Archivo CSV' },
  { path: 'procesar-fecha', component: ProcesarFechaComponent, canActivate: [authGuard], title: 'iRoute - Procesar por Fecha' },
  { path: 'cuarentena', component: ComerciosCuarentenaComponent, canActivate: [authGuard], title: 'iRoute - Registros en Cuarentena' },
  { path: 'comercios-activos', component: ComerciosActivosComponent, canActivate: [authGuard], title: 'iRoute - Comercios Válidos' },
  { path: '**', redirectTo: 'iniciar-sesion' }
];
