package com.prueba.feeservice.service;

import com.prueba.feeservice.domain.Channel;
import com.prueba.feeservice.domain.FeeCalculation;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.Objects;

@Service
public class FeeCalculatorService {

    private static final int MONEY_SCALE = 2;
    private static final RoundingMode ROUNDING = RoundingMode.HALF_UP;

    public FeeCalculation calculate(BigDecimal amount, Channel channel) {
        Objects.requireNonNull(amount, "amount no puede ser null");
        Objects.requireNonNull(channel, "channel no puede ser null");

        BigDecimal fee = amount.multiply(channel.rate()).setScale(MONEY_SCALE, ROUNDING);
        BigDecimal total = amount.add(fee).setScale(MONEY_SCALE, ROUNDING);

        return new FeeCalculation(fee, total);
    }
}