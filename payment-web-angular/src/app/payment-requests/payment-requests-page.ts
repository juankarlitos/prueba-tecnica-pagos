import { DecimalPipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, catchError, finalize, of, switchMap, tap } from 'rxjs';
import { UserError, toUserError } from './api-error';
import { CreatePaymentRequest, PAYMENT_CHANNELS, PaymentChannel, PaymentRequest } from './payment-request.model';
import { PaymentRequestService } from './payment-request.service';
import { amountFormat, notBlank, parseAmount, positiveAmount, trimmedMaxLength } from './payment-request.validators';

type FormField = 'externalReference' | 'amount' | 'channel';

@Component({
  selector: 'app-payment-requests-page',
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './payment-requests-page.html',
  styleUrl: './payment-requests-page.scss',
})
export class PaymentRequestsPage implements OnInit {
  private readonly paymentRequestService = inject(PaymentRequestService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly reload$ = new Subject<void>();

  readonly referenceMaxLength = 50;
  readonly channels = PAYMENT_CHANNELS;

  readonly form = new FormGroup({
    externalReference: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, notBlank(), trimmedMaxLength(this.referenceMaxLength)],
    }),
    amount: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, amountFormat(), positiveAmount()],
    }),
    channel: new FormControl<PaymentChannel | ''>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  readonly channelFilter = new FormControl<PaymentChannel | ''>('', { nonNullable: true });

  readonly requests = signal<PaymentRequest[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly submitError = signal<UserError | null>(null);
  readonly successMessage = signal<string | null>(null);

  constructor() {
    // Cada recarga cancela la anterior (switchMap) para evitar resultados desordenados.
    this.reload$
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.loadError.set(null);
        }),
        switchMap(() =>
          this.paymentRequestService.getAll(this.channelFilter.value || null).pipe(
            catchError((error: unknown) => {
              this.loadError.set(toUserError(error).message);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((requests) => {
        if (requests) {
          this.requests.set(requests);
        }
        this.loading.set(false);
      });

    this.channelFilter.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.loadRequests());
  }

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.reload$.next();
  }

  submit(): void {
    if (this.submitting()) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { externalReference, amount, channel } = this.form.getRawValue();
    const request: CreatePaymentRequest = {
      externalReference: externalReference.trim(),
      amount: parseAmount(amount),
      channel: channel as PaymentChannel,
    };

    this.submitting.set(true);
    this.submitError.set(null);
    this.successMessage.set(null);

    this.paymentRequestService
      .create(request)
      .pipe(
        finalize(() => this.submitting.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (created) => {
          this.successMessage.set(`Solicitud ${created.externalReference} registrada correctamente.`);
          this.form.reset();
          this.loadRequests();
        },
        // Si falla, el formulario conserva los datos ingresados.
        error: (error: unknown) => this.submitError.set(toUserError(error)),
      });
  }

  isInvalid(field: FormField): boolean {
    const control = this.form.controls[field];
    return control.invalid && (control.touched || control.dirty);
  }

  hasError(field: FormField, errorCode: string): boolean {
    const control = this.form.controls[field];
    return control.hasError(errorCode) && (control.touched || control.dirty);
  }

  channelLabel(channel: PaymentChannel): string {
    return this.channels.find((option) => option.value === channel)?.label ?? channel;
  }

  statusLabel(status: string): string {
    return status === 'PENDING' ? 'Pendiente' : status;
  }
}