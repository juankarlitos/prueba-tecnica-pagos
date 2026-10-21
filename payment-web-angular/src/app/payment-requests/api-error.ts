import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from './payment-request.model';

export interface UserError {
  message: string;
  details: string[];
}

const UNEXPECTED_ERROR = 'Ocurrió un error inesperado. Intente nuevamente.';
const CONNECTION_ERROR = 'No fue posible conectar con el servidor. Verifique su conexión e intente nuevamente.';
const SERVICE_UNAVAILABLE = 'El servicio no está disponible en este momento. Intente nuevamente.';

function isApiError(value: unknown): value is ApiError {
  return (
    typeof value === 'object' &&
    value !== null &&
    typeof (value as ApiError).code === 'string' &&
    typeof (value as ApiError).message === 'string'
  );
}

/** Convierte cualquier error HTTP en un mensaje comprensible para el usuario. */
export function toUserError(error: unknown): UserError {
  if (error instanceof HttpErrorResponse) {
    if (isApiError(error.error)) {
      return { message: error.error.message, details: error.error.details ?? [] };
    }
    if (error.status === 0) {
      return { message: CONNECTION_ERROR, details: [] };
    }
    if (error.status >= 500) {
      return { message: SERVICE_UNAVAILABLE, details: [] };
    }
  }
  return { message: UNEXPECTED_ERROR, details: [] };
}