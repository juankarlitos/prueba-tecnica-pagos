package com.prueba.feeservice.error;

import java.util.List;

/**
 * Estructura única de error: { code, message, details }.
 */
public record ErrorResponse(String code, String message, List<String> details) {

    public static ErrorResponse of(String code, String message) {
        return new ErrorResponse(code, message, List.of());
    }
}