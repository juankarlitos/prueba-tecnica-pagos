package com.prueba.feeservice.domain;

import java.math.BigDecimal;

public record FeeCalculation(BigDecimal fee, BigDecimal total) {
}