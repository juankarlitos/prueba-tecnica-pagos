package com.prueba.feeservice.api.dto;

import java.math.BigDecimal;

public record FeeCalculationResponse(BigDecimal fee, BigDecimal total) {
}