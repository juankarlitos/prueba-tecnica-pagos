package com.prueba.feeservice.api;

import com.prueba.feeservice.api.dto.FeeCalculationRequest;
import com.prueba.feeservice.api.dto.FeeCalculationResponse;
import com.prueba.feeservice.domain.Channel;
import com.prueba.feeservice.domain.FeeCalculation;
import com.prueba.feeservice.service.FeeCalculatorService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/v1/fees")
@Tag(name = "Fees", description = "Cálculo de comisiones por canal")
public class FeeController {

    private final FeeCalculatorService feeCalculatorService;

    public FeeController(FeeCalculatorService feeCalculatorService) {
        this.feeCalculatorService = feeCalculatorService;
    }

    @PostMapping("/calculate")
    @Operation(summary = "Calcula la comisión y el total para un monto y canal")
    public FeeCalculationResponse calculate(@Valid @RequestBody FeeCalculationRequest request) {
        // El canal ya fue validado por @Pattern, por lo que valueOf es seguro.
        Channel channel = Channel.valueOf(request.channel());
        FeeCalculation result = feeCalculatorService.calculate(request.amount(), channel);
        return new FeeCalculationResponse(result.fee(), result.total());
    }
}