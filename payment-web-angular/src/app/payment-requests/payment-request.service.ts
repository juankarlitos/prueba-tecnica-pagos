import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreatePaymentRequest, PaymentChannel, PaymentRequest } from './payment-request.model';

/** Acceso HTTP a la API .NET. La URL es relativa y el proxy de desarrollo la redirige a .NET. */
@Injectable({ providedIn: 'root' })
export class PaymentRequestService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/payment-requests';

  getAll(channel: PaymentChannel | null = null): Observable<PaymentRequest[]> {
    const params = channel ? new HttpParams().set('channel', channel) : undefined;
    return this.http.get<PaymentRequest[]>(this.baseUrl, { params });
  }

  create(request: CreatePaymentRequest): Observable<PaymentRequest> {
    return this.http.post<PaymentRequest>(this.baseUrl, request);
  }
}