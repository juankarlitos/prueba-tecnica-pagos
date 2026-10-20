package com.prueba.feeservice.domain;

import java.math.BigDecimal;

/**
 * Canales de origen y su tasa de comisión.
 * Para cambiar una tasa o agregar un canal, se modifica solo este enum.
 */
public enum Channel {

    WEB(new BigDecimal("0.010")),
    MOBILE(new BigDecimal("0.005")),
    BRANCH(new BigDecimal("0.015"));

    private final BigDecimal rate;

    Channel(BigDecimal rate) {
        this.rate = rate;
    }

    public BigDecimal rate() {
        return rate;
    }
}