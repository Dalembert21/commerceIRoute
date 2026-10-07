import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ComercioServicio } from '../../servicios/comercio.servicio';

interface FilaPrevisualizacion {
  fechaProceso: string;
  codigoComercio: string;
  nombreComercial: string;
  numeroDocumento: string;
  tipoDocumento: string;
  estado: string;
}

@Component({
  selector: 'app-cargar-archivo',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cargar-archivo.component.html',
  styleUrl: './cargar-archivo.component.css'
})
export class CargarArchivoComponent {
  private servicioComercio = inject(ComercioServicio);

  archivoSeleccionado: File | null = null;
  filasPrevisualizacion: FilaPrevisualizacion[] = [];
  estaArrastrando = false;
  cargando = false;
  mensajeExito: string | null = null;
  mensajeError: string | null = null;

  alArrastrarSobre(event: DragEvent): void {
    event.preventDefault();
    this.estaArrastrando = true;
  }

  alSalirDeArrastrar(event: DragEvent): void {
    event.preventDefault();
    this.estaArrastrando = false;
  }

  alSoltarArchivo(event: DragEvent): void {
    event.preventDefault();
    this.estaArrastrando = false;
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.procesarArchivo(event.dataTransfer.files[0]);
    }
  }

  alSeleccionarArchivo(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.procesarArchivo(input.files[0]);
    }
  }

  procesarArchivo(archivo: File): void {
    this.mensajeExito = null;
    this.mensajeError = null;

    if (!archivo.name.endsWith('.csv')) {
      this.mensajeError = 'Debe seleccionar un archivo .csv (por ejemplo commerce_07102026.csv).';
      return;
    }

    if (archivo.size === 0) {
      this.mensajeError = 'El archivo seleccionado está completamente vacío.';
      return;
    }

    if (archivo.size > 5 * 1024 * 1024) {
      this.mensajeError = 'El archivo supera el tamaño máximo permitido de 5 MB.';
      return;
    }

    this.archivoSeleccionado = archivo;
    const lector = new FileReader();

    lector.onload = (e) => {
      const texto = e.target?.result as string;
      this.leerLineasCsv(texto);
    };

    lector.onerror = () => {
      this.mensajeError = 'Error al leer el archivo en el navegador.';
    };

    lector.readAsText(archivo, 'utf-8');
  }

  private leerLineasCsv(contenido: string): void {
    const lineas = contenido.split(/\r\n|\n/).filter(l => l.trim() !== '');
    if (lineas.length === 0) return;

    const separador = lineas[0].includes(';') ? ';' : ',';
    const filas: FilaPrevisualizacion[] = [];

    for (let i = 1; i < lineas.length; i++) {
      const partes = lineas[i].split(separador).map(p => p.trim().replace(/^["']|["']$/g, ''));
      filas.push({
        fechaProceso: partes[0] || '',
        codigoComercio: partes[1] || '',
        nombreComercial: partes[2] || '',
        numeroDocumento: partes[3] || '',
        tipoDocumento: partes[4] || '',
        estado: partes[5] || ''
      });
    }

    this.filasPrevisualizacion = filas;
  }

  contieneCaracteresInvalidos(numDoc: string): boolean {
    return !/^\d+$/.test(numDoc);
  }

  enviarAlBackend(): void {
    if (!this.archivoSeleccionado) return;

    this.cargando = true;
    this.mensajeExito = null;
    this.mensajeError = null;

    this.servicioComercio.cargarArchivoCsv(this.archivoSeleccionado).subscribe({
      next: (resp) => {
        this.cargando = false;
        if (resp.exito) {
          this.mensajeExito = resp.mensaje;
        } else {
          this.mensajeError = resp.mensaje;
        }
      },
      error: (err) => {
        this.cargando = false;
        this.mensajeError = err.error?.mensaje || 'Error al conectar con la API.';
      }
    });
  }

  obtenerTamanoFormateado(bytes: number): string {
    return (bytes / 1024).toFixed(1) + ' KB';
  }
}
