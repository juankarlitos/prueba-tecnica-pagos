import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Acepta enteros o decimales con hasta 2 decimales. Permite punto o coma como separador. */
const AMOUNT_PATTERN = /^-?\d+([.,]\d{1,2})?$/;

/** Convierte el texto ingresado (ej: "1000,50") a número. */
export function parseAmount(value: string): number {
  return Number(value.trim().replace(',', '.'));
}

/** La referencia no puede contener solo espacios. */
export function notBlank(): ValidatorFn {
  return (control: AbstractControl<string>): ValidationErrors | null => {
    const value = control.value ?? '';
    return value.length > 0 && value.trim().length === 0 ? { blank: true } : null;
  };
}

/** Largo máximo considerando la referencia sin espacios al inicio ni al final. */
export function trimmedMaxLength(max: number): ValidatorFn {
  return (control: AbstractControl<string>): ValidationErrors | null => {
    const length = (control.value ?? '').trim().length;
    return length > max ? { trimmedMaxLength: { max, actual: length } } : null;
  };
}

/** El monto debe ser numérico y tener como máximo dos decimales. */
export function amountFormat(): ValidatorFn {
  return (control: AbstractControl<string>): ValidationErrors | null => {
    const value = (control.value ?? '').trim();
    if (value.length === 0) {
      return null; // Lo informa Validators.required.
    }
    return AMOUNT_PATTERN.test(value) ? null : { amountFormat: true };
  };
}

/** El monto debe ser mayor que cero. */
export function positiveAmount(): ValidatorFn {
  return (control: AbstractControl<string>): ValidationErrors | null => {
    const value = (control.value ?? '').trim();
    if (value.length === 0 || !AMOUNT_PATTERN.test(value)) {
      return null; // Lo informan required y amountFormat.
    }
    return parseAmount(value) > 0 ? null : { positive: true };
  };
}