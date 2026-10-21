import { Component } from '@angular/core';
import { PaymentRequestsPage } from './payment-requests/payment-requests-page';

@Component({
  selector: 'app-root',
  imports: [PaymentRequestsPage],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}