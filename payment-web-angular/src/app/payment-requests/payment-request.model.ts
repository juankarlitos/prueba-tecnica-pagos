export type PaymentChannel = 'WEB' | 'MOBILE' | 'BRANCH';

export const PAYMENT_CHANNELS: readonly { value: PaymentChannel; label: string }[] = [
  { value: 'WEB', label: 'Web' },
  { value: 'MOBILE', label: 'Móvil' },
  { value: 'BRANCH', label: 'Sucursal' },
];

/** Cuerpo de la solicitud de creación enviada a la API .NET. */
export interface CreatePaymentRequest {
  externalReference: string;
  amount: number;
  channel: PaymentChannel;
}

/** Solicitud de pago devuelta por la API .NET. */
export interface PaymentRequest {
  id: string;
  externalReference: string;
  amount: number;
  channel: PaymentChannel;
  fee: number;
  total: number;
  status: string;
}

/** Estructura de error común de las APIs: { code, message, details }. */
export interface ApiError {
  code: string;
  message: string;
  details?: string[];
}