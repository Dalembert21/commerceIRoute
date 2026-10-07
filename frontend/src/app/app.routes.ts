import { Routes } from '@angular/router';
import { CargarArchivoComponent } from './componentes/cargar-archivo/cargar-archivo.component';
import { ProcesarFechaComponent } from './componentes/procesar-fecha/procesar-fecha.component';
import { ComerciosCuarentenaComponent } from './componentes/comercios-cuarentena/comercios-cuarentena.component';
import { ComerciosActivosComponent } from './componentes/comercios-activos/comercios-activos.component';
import { InicioSesionComponent } from './componentes/inicio-sesion/inicio-sesion.component';

export const routes: Routes = [
  { path: '', redirectTo: 'cargar-archivo', pathMatch: 'full' },
  { path: 'cargar-archivo', component: CargarArchivoComponent, title: 'IRoute - Cargar Archivo CSV' },
  { path: 'procesar-fecha', component: ProcesarFechaComponent, title: 'IRoute - Procesar por Fecha' },
  { path: 'cuarentena', component: ComerciosCuarentenaComponent, title: 'IRoute - Registros en Cuarentena' },
  { path: 'comercios-activos', component: ComerciosActivosComponent, title: 'IRoute - Comercios Válidos' },
  { path: 'iniciar-sesion', component: InicioSesionComponent, title: 'IRoute - Inicio de Sesión' },
  { path: '**', redirectTo: 'cargar-archivo' }
];
