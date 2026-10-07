import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ComercioServicio } from '../../servicios/comercio.servicio';

interface FilaPrevisualizacion {
  pcProcessdate: string;
  pcCodcom: string;
  pcNomcomred: string;
  pcNumdoc: string;
  pcTipdoc: string;
  pcEstado: string;
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

  private readonly patronNombreArchivo = /^commerce_\d{8}\.csv$/i;

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
      this.mensajeError = 'Debe seleccionar un archivo con extensión .csv.';
      return;
    }

    if (!this.patronNombreArchivo.test(archivo.name)) {
      this.mensajeError = `El nombre del archivo debe cumplir con el formato requerido: commerce_DDMMYYYY.csv (ejemplo: commerce_07102026.csv). Archivo actual: "${archivo.name}".`;
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
    const cabeceras = lineas[0].split(separador).map(c => c.trim().toLowerCase().replace(/^["']|["']$/g, ''));

    let idxFecha = cabeceras.findIndex(c => c.includes('processdate') || c.includes('fecha'));
    let idxCod = cabeceras.findIndex(c => c.includes('codcom') || c.includes('codigo'));
    let idxNom = cabeceras.findIndex(c => c.includes('nomcomred') || c.includes('nombre'));
    let idxDoc = cabeceras.findIndex(c => c.includes('numdoc') || c.includes('documento'));
    let idxTip = cabeceras.findIndex(c => c.includes('tipdoc') || c.includes('tipo'));
    let idxEst = cabeceras.findIndex(c => c.includes('estado'));

    if (idxFecha === -1) idxFecha = 0;
    if (idxCod === -1 && cabeceras.length > 1) idxCod = 1;
    if (idxNom === -1 && cabeceras.length > 2) idxNom = 2;
    if (idxDoc === -1 && cabeceras.length > 3) idxDoc = 3;
    if (idxTip === -1 && cabeceras.length > 4) idxTip = 4;
    if (idxEst === -1 && cabeceras.length > 5) idxEst = 5;

    const filas: FilaPrevisualizacion[] = [];

    for (let i = 1; i < lineas.length; i++) {
      const partes = lineas[i].split(separador).map(p => p.trim().replace(/^["']|["']$/g, ''));
      filas.push({
        pcProcessdate: partes[idxFecha] || '',
        pcCodcom: partes[idxCod] || '',
        pcNomcomred: partes[idxNom] || '',
        pcNumdoc: partes[idxDoc] || '',
        pcTipdoc: partes[idxTip] || '',
        pcEstado: partes[idxEst] || ''
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
