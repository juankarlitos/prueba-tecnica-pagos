package com.prueba.feeservice.service;

import com.prueba.feeservice.domain.Channel;
import com.prueba.feeservice.domain.FeeCalculation;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

import java.math.BigDecimal;

import static org.junit.jupiter.api.Assertions.assertEquals;

class FeeCalculatorServiceTest {

    private final FeeCalculatorService service = new FeeCalculatorService();

    @ParameterizedTest(name = "{1} con monto {0} -> fee {2}, total {3}")
    @CsvSource({
            "1000.00, WEB,    10.00, 1010.00",
            "1000.00, MOBILE,  5.00, 1005.00",
            "1000.00, BRANCH, 15.00, 1015.00",
            // Caso del enunciado: 0.005 se redondea HALF_UP a 0.01 (HALF_EVEN daría 0.00)
            "1.00,    MOBILE,  0.01,    1.01",
            "123.45,  WEB,     1.23,  124.68",
            "0.33,    BRANCH,  0.00,    0.33"
    })
    void calculatesFeeAndTotalWithTwoDecimals(BigDecimal amount, Channel channel,
                                              BigDecimal expectedFee, BigDecimal expectedTotal) {
        FeeCalculation result = service.calculate(amount, channel);

        // assertEquals con BigDecimal compara valor y escala: también valida los 2 decimales
        assertEquals(expectedFee, result.fee());
        assertEquals(expectedTotal, result.total());
    }
}