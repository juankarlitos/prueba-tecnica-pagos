package com.prueba.feeservice.api.dto;

import jakarta.validation.constraints.DecimalMin;
import jakarta.validation.constraints.Digits;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Pattern;

import java.math.BigDecimal;

public record FeeCalculationRequest(

        @NotNull(message = "amount es obligatorio")
        @DecimalMin(value = "0.00", inclusive = false, message = "amount debe ser mayor que cero")
        @Digits(integer = 15, fraction = 2, message = "amount admite como máximo dos decimales y 15 dígitos enteros")
        BigDecimal amount,

        @NotNull(message = "channel es obligatorio")
        @Pattern(regexp = "WEB|MOBILE|BRANCH", message = "channel debe ser WEB, MOBILE o BRANCH")
        String channel
) {
}