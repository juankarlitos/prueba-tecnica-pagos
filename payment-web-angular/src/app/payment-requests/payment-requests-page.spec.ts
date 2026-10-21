import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaymentRequest } from './payment-request.model';
import { PaymentRequestsPage } from './payment-requests-page';

const API_URL = '/api/v1/payment-requests';

const CREATED_REQUEST: PaymentRequest = {
  id: '8b80dd63-c588-4a79-9a0b-e9c8f126e171',
  externalReference: 'PAY-2026-0001',
  amount: 1000.5,
  channel: 'WEB',
  fee: 10.01,
  total: 1010.51,
  status: 'PENDING',
};

describe('PaymentRequestsPage', () => {
  let fixture: ComponentFixture<PaymentRequestsPage>;
  let component: PaymentRequestsPage;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaymentRequestsPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentRequestsPage);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);

    // Al entrar a la pantalla se consulta la lista.
    fixture.detectChanges();
    httpMock.expectOne(API_URL).flush([]);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function renderedText(): string {
    fixture.detectChanges();
    return (fixture.nativeElement as HTMLElement).textContent ?? '';
  }

  function fillValidForm(): void {
    component.form.setValue({ externalReference: ' PAY-2026-0001 ', amount: '1000,50', channel: 'WEB' });
  }

  // Prueba obligatoria: referencia vacía.
  it('detecta una referencia vacía o con solo espacios', () => {
    const reference = component.form.controls.externalReference;

    reference.setValue('');
    expect(reference.hasError('required')).toBe(true);

    reference.setValue('   ');
    expect(reference.hasError('blank')).toBe(true);

    expect(component.form.invalid).toBe(true);
  });

  // Prueba obligatoria: monto menor o igual que cero.
  it('detecta un monto menor o igual que cero', () => {
    const amount = component.form.controls.amount;

    amount.setValue('0');
    expect(amount.hasError('positive')).toBe(true);

    amount.setValue('-10');
    expect(amount.hasError('positive')).toBe(true);

    amount.setValue('10.50');
    expect(amount.valid).toBe(true);
  });

  it('no envía la solicitud si el formulario es inválido y muestra las validaciones', () => {
    component.submit();

    httpMock.expectNone({ method: 'POST', url: API_URL });
    const text = renderedText();
    expect(text).toContain('La referencia es obligatoria.');
    expect(text).toContain('El monto es obligatorio.');
    expect(text).toContain('Seleccione un canal.');
  });

  it('registra la solicitud, limpia el formulario y recarga la lista', () => {
    fillValidForm();
    component.submit();

    const post = httpMock.expectOne({ method: 'POST', url: API_URL });
    expect(post.request.body).toEqual({ externalReference: 'PAY-2026-0001', amount: 1000.5, channel: 'WEB' });
    post.flush(CREATED_REQUEST, { status: 201, statusText: 'Created' });

    httpMock.expectOne(API_URL).flush([CREATED_REQUEST]);

    expect(component.requests()).toEqual([CREATED_REQUEST]);
    expect(component.form.controls.externalReference.value).toBe('');
    expect(renderedText()).toContain('Solicitud PAY-2026-0001 registrada correctamente.');
  });

  // Prueba extra: manejo de errores HTTP.
  it('muestra el error de la API y conserva los datos del formulario cuando el cálculo no está disponible', () => {
    fillValidForm();
    component.submit();
    expect(component.submitting()).toBe(true);

    httpMock.expectOne({ method: 'POST', url: API_URL }).flush(
      {
        code: 'CALCULATION_SERVICE_UNAVAILABLE',
        message: 'No fue posible calcular la comisión. Intente nuevamente.',
        details: [],
      },
      { status: 503, statusText: 'Service Unavailable' },
    );

    expect(component.submitting()).toBe(false);
    expect(renderedText()).toContain('No fue posible calcular la comisión. Intente nuevamente.');
    expect(component.form.getRawValue()).toEqual({
      externalReference: ' PAY-2026-0001 ',
      amount: '1000,50',
      channel: 'WEB',
    });
  });

  it('muestra un mensaje cuando no hay solicitudes registradas', () => {
    expect(renderedText()).toContain('No hay solicitudes registradas.');
  });

  it('consulta la API con el canal seleccionado en el filtro', () => {
    component.channelFilter.setValue('MOBILE');

    httpMock
      .expectOne((request) => request.url === API_URL && request.params.get('channel') === 'MOBILE')
      .flush([]);

    expect(renderedText()).toContain('No hay solicitudes para el canal seleccionado.');
  });
});